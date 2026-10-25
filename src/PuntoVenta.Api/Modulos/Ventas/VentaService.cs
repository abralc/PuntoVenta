using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Configuracion;
using PuntoVenta.Api.Modulos.Productos.Entidades;
using PuntoVenta.Api.Modulos.Ventas.Entidades;
using PuntoVenta.Shared.Ventas;

namespace PuntoVenta.Api.Modulos.Ventas;

public interface IVentaService
{
    Task<List<VentaDto>> ListarAsync(DateOnly? desde, DateOnly? hasta, int? idCliente, int? idEmpleado, string? buscar);
    Task<VentaDto> ObtenerAsync(int id);
    Task<DisponibilidadDto> DisponibilidadAsync(int idProducto, string? sesionPos = null);
    /// <param name="sesionPos">Si la venta viene del POS: sus propias reservas no le restan disponibilidad y se liberan al cobrar.</param>
    Task<VentaDto> CrearAsync(VentaGuardarDto datos, string? sesionPos = null);
    Task<VentaDto> ActualizarAsync(int id, VentaGuardarDto datos);
    Task EliminarAsync(int id);
}

/// <summary>
/// Ventas (maestro/detalle). Reglas de negocio:
///   1. Antes de vender se verifica la existencia: no se puede vender más de lo disponible
///      (disponible = existencia − unidades reservadas en otras cajas POS).
///   2. Al confirmar, la existencia disminuye automáticamente (30 − 5 = 25).
///   3. El precio es el precio_venta de la tabla productos.
///   4. Subtotal = cantidad × precio; total = suma de subtotales.
/// Todo en UNA transacción con las filas de producto bloqueadas (FOR UPDATE).
/// </summary>
public class VentaService(AppDbContext db, IConfiguracionService configuracion) : IVentaService
{
    public async Task<List<VentaDto>> ListarAsync(DateOnly? desde, DateOnly? hasta, int? idCliente, int? idEmpleado, string? buscar)
    {
        var q = db.Ventas.AsNoTracking();
        if (desde is not null) q = q.Where(v => v.FechaFactura >= desde);
        if (hasta is not null) q = q.Where(v => v.FechaFactura <= hasta);
        if (idCliente is > 0) q = q.Where(v => v.IdCliente == idCliente);
        if (idEmpleado is > 0) q = q.Where(v => v.IdEmpleado == idEmpleado);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = int.TryParse(t, out var numero)
                ? q.Where(v => v.NoFactura == numero || v.Cliente!.Nit.Contains(t))
                : q.Where(v => v.Cliente!.Nombres.Contains(t) || v.Cliente.Apellidos.Contains(t) || v.Cliente.Nit.Contains(t));
        }

        return await q
            .OrderByDescending(v => v.FechaFactura).ThenByDescending(v => v.IdVenta)
            .Select(v => new VentaDto
            {
                IdVenta = v.IdVenta,
                NoFactura = v.NoFactura,
                Serie = v.Serie,
                FechaFactura = v.FechaFactura,
                IdCliente = v.IdCliente,
                Cliente = v.Cliente!.Nombres + " " + v.Cliente.Apellidos,
                NitCliente = v.Cliente.Nit,
                IdEmpleado = v.IdEmpleado,
                Empleado = v.Empleado!.Nombres + " " + v.Empleado.Apellidos,
                FechaIngreso = v.FechaIngreso,
                Productos = v.Detalles.Count,
                Unidades = v.Detalles.Sum(d => d.Cantidad),
                Total = v.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
            })
            .ToListAsync();
    }

    public async Task<VentaDto> ObtenerAsync(int id)
    {
        var venta = await db.Ventas.AsNoTracking()
            .Where(v => v.IdVenta == id)
            .Select(v => new VentaDto
            {
                IdVenta = v.IdVenta,
                NoFactura = v.NoFactura,
                Serie = v.Serie,
                FechaFactura = v.FechaFactura,
                IdCliente = v.IdCliente,
                Cliente = v.Cliente!.Nombres + " " + v.Cliente.Apellidos,
                NitCliente = v.Cliente.Nit,
                IdEmpleado = v.IdEmpleado,
                Empleado = v.Empleado!.Nombres + " " + v.Empleado.Apellidos,
                FechaIngreso = v.FechaIngreso,
                Detalles = v.Detalles.OrderBy(d => d.IdVentaDetalle).Select(d => new VentaDetalleDto
                {
                    IdVentaDetalle = d.IdVentaDetalle,
                    IdProducto = d.IdProducto,
                    Producto = d.Producto!.Nombre,
                    Marca = d.Producto.Marca!.Nombre,
                    Imagen = d.Producto.Imagen,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList()
            })
            .FirstOrDefaultAsync() ?? throw new NoEncontradoException("La venta no existe.");

        venta.Productos = venta.Detalles.Count;
        venta.Unidades = venta.Detalles.Sum(d => d.Cantidad);
        venta.Total = venta.Detalles.Sum(d => d.Subtotal);
        return venta;
    }

    public async Task<DisponibilidadDto> DisponibilidadAsync(int idProducto, string? sesionPos = null)
    {
        var p = await db.Productos.AsNoTracking().FirstOrDefaultAsync(x => x.IdProducto == idProducto)
                ?? throw new NoEncontradoException("El producto no existe.");
        return new DisponibilidadDto
        {
            IdProducto = p.IdProducto,
            Producto = p.Nombre,
            PrecioVenta = p.PrecioVenta,
            Existencia = p.Existencia,
            Reservado = await ReservadoPorOtrosAsync(idProducto, sesionPos)
        };
    }

    public async Task<VentaDto> CrearAsync(VentaGuardarDto datos, string? sesionPos = null)
    {
        var lineas = AgruparDetalle(datos);
        await ValidarMaestro(datos);
        var serie = (await configuracion.ObtenerAsync()).SerieFactura;

        await using var tx = await db.Database.BeginTransactionAsync();

        var productos = await BloquearProductosAsync(lineas.Keys);

        // Regla 1: verificar existencia ANTES de vender
        foreach (var (idProducto, cantidad) in lineas)
            await VerificarExistenciaAsync(productos[idProducto], cantidad, sesionPos);

        var venta = new Venta
        {
            Serie = serie,
            NoFactura = await SiguienteFacturaAsync(serie),
            FechaFactura = datos.FechaFactura!.Value,
            IdCliente = datos.IdCliente,
            IdEmpleado = datos.IdEmpleado,
            FechaIngreso = DateTime.Now
        };

        foreach (var (idProducto, cantidad) in lineas)
        {
            var producto = productos[idProducto];
            venta.Detalles.Add(new VentaDetalle
            {
                IdProducto = idProducto,
                Cantidad = cantidad,
                PrecioUnitario = producto.PrecioVenta   // Regla 3
            });
            producto.Existencia -= cantidad;            // Regla 2
        }

        db.Ventas.Add(venta);
        await db.SaveChangesAsync();

        // Venta desde el POS: liberar las reservas de esa caja
        if (!string.IsNullOrWhiteSpace(sesionPos))
            await db.Database.ExecuteSqlAsync($"DELETE FROM reservas_pos WHERE sesion_pos = {sesionPos}");

        await tx.CommitAsync();
        return await ObtenerAsync(venta.IdVenta);
    }

    public async Task<VentaDto> ActualizarAsync(int id, VentaGuardarDto datos)
    {
        var lineas = AgruparDetalle(datos);
        await ValidarMaestro(datos);

        await using var tx = await db.Database.BeginTransactionAsync();

        var venta = await db.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.IdVenta == id)
                    ?? throw new NoEncontradoException("La venta no existe.");

        var productos = await BloquearProductosAsync(venta.Detalles.Select(d => d.IdProducto).Union(lineas.Keys));

        // Devolver al inventario lo de la venta anterior, conservando el precio con que se vendió
        var preciosAnteriores = venta.Detalles.ToDictionary(d => d.IdProducto, d => d.PrecioUnitario);
        foreach (var anterior in venta.Detalles)
            productos[anterior.IdProducto].Existencia += anterior.Cantidad;

        foreach (var (idProducto, cantidad) in lineas)
            await VerificarExistenciaAsync(productos[idProducto], cantidad, null);

        db.VentasDetalle.RemoveRange(venta.Detalles);
        venta.Detalles = lineas.Select(l => new VentaDetalle
        {
            IdVenta = id,
            IdProducto = l.Key,
            Cantidad = l.Value,
            // Productos que ya estaban conservan su precio; los nuevos usan el precio_venta actual
            PrecioUnitario = preciosAnteriores.TryGetValue(l.Key, out var precio) ? precio : productos[l.Key].PrecioVenta
        }).ToList();

        foreach (var (idProducto, cantidad) in lineas)
            productos[idProducto].Existencia -= cantidad;

        venta.IdCliente = datos.IdCliente;
        venta.IdEmpleado = datos.IdEmpleado;
        venta.FechaFactura = datos.FechaFactura!.Value;

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var venta = await db.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.IdVenta == id)
                    ?? throw new NoEncontradoException("La venta no existe.");

        // Anular la venta: las unidades regresan al inventario
        var productos = await BloquearProductosAsync(venta.Detalles.Select(d => d.IdProducto));
        foreach (var d in venta.Detalles)
            productos[d.IdProducto].Existencia += d.Cantidad;

        db.Ventas.Remove(venta); // el detalle se elimina en cascada
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    // ------------------------------------------------------------------

    /// <summary>Une las líneas repetidas del mismo producto sumando cantidades.</summary>
    private static Dictionary<int, int> AgruparDetalle(VentaGuardarDto datos)
    {
        if (datos.Detalles.Count == 0)
            throw new ReglaNegocioException("Agregue al menos un producto a la venta.");
        if (datos.Detalles.Any(d => d.Cantidad <= 0))
            throw new ReglaNegocioException("Las cantidades deben ser mayores a cero.");

        return datos.Detalles
            .GroupBy(d => d.IdProducto)
            .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));
    }

    private async Task ValidarMaestro(VentaGuardarDto datos)
    {
        if (!await db.Clientes.AnyAsync(c => c.IdCliente == datos.IdCliente))
            throw new ReglaNegocioException("El cliente seleccionado no existe.");
        if (!await db.Empleados.AnyAsync(e => e.IdEmpleado == datos.IdEmpleado))
            throw new ReglaNegocioException("El empleado seleccionado no existe.");
    }

    private async Task VerificarExistenciaAsync(Producto producto, int cantidad, string? sesionPos)
    {
        var reservado = await ReservadoPorOtrosAsync(producto.IdProducto, sesionPos);
        var disponible = producto.Existencia - reservado;
        if (cantidad > disponible)
        {
            var detalle = reservado > 0 ? $" ({reservado} reservadas en otra caja)" : "";
            throw new ReglaNegocioException(
                $"No se puede realizar la venta. Existencia insuficiente de '{producto.Nombre}': " +
                $"disponible {Math.Max(0, disponible)}{detalle}, solicitado {cantidad}.", StatusCodes.Status409Conflict);
        }
    }

    /// <summary>Unidades apartadas por las cajas POS (reservas vigentes), sin contar la sesión indicada.</summary>
    private async Task<int> ReservadoPorOtrosAsync(int idProducto, string? sesionPos)
    {
        var sesion = sesionPos ?? string.Empty;
        var filas = await db.Database.SqlQuery<long>(
                $"SELECT CAST(COALESCE(SUM(cantidad), 0) AS SIGNED) AS Value FROM reservas_pos WHERE id_producto = {idProducto} AND expira > NOW() AND sesion_pos <> {sesion}")
            .ToListAsync();
        return (int)filas.FirstOrDefault();
    }

    /// <summary>Siguiente número de factura de la serie (la fila se bloquea para evitar números repetidos).</summary>
    private async Task<int> SiguienteFacturaAsync(string serie)
    {
        var ultima = await db.Ventas
            .FromSql($"SELECT * FROM ventas WHERE serie = {serie} ORDER BY no_factura DESC LIMIT 1 FOR UPDATE")
            .ToListAsync();
        return (ultima.FirstOrDefault()?.NoFactura ?? 0) + 1;
    }

    private async Task<Dictionary<int, Producto>> BloquearProductosAsync(IEnumerable<int> ids)
    {
        var resultado = new Dictionary<int, Producto>();
        foreach (var id in ids.Distinct().OrderBy(x => x))
        {
            var filas = await db.Productos
                .FromSql($"SELECT * FROM productos WHERE id_producto = {id} FOR UPDATE")
                .ToListAsync();
            resultado[id] = filas.FirstOrDefault()
                            ?? throw new ReglaNegocioException($"El producto con código {id} no existe.");
        }
        return resultado;
    }
}
