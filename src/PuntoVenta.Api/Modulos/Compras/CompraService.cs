using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Compras.Entidades;
using PuntoVenta.Api.Modulos.Configuracion;
using PuntoVenta.Api.Modulos.Productos.Entidades;
using PuntoVenta.Shared.Compras;

namespace PuntoVenta.Api.Modulos.Compras;

public interface ICompraService
{
    Task<List<CompraDto>> ListarAsync(DateOnly? desde, DateOnly? hasta, int? idProveedor, string? buscar);
    Task<CompraDto> ObtenerAsync(int id);
    Task<int> SiguienteOrdenAsync();
    Task<CompraDto> CrearAsync(CompraGuardarDto datos);
    Task<CompraDto> ActualizarAsync(int id, CompraGuardarDto datos);
    Task EliminarAsync(int id);
}

/// <summary>
/// Compras (maestro/detalle). Reglas de negocio al registrar una compra:
///   1. existencia del producto += cantidad comprada            (20 + 10 = 30)
///   2. precio_costo del producto  = precio de la compra
///   3. precio_venta del producto  = precio_costo + 25 %         (100.00 → 125.00)
/// Todo se ejecuta en UNA transacción: si algo falla, no se guarda nada.
/// </summary>
public class CompraService(AppDbContext db, IConfiguracionService configuracion) : ICompraService
{
    public async Task<List<CompraDto>> ListarAsync(DateOnly? desde, DateOnly? hasta, int? idProveedor, string? buscar)
    {
        var q = db.Compras.AsNoTracking();
        if (desde is not null) q = q.Where(c => c.FechaOrden >= desde);
        if (hasta is not null) q = q.Where(c => c.FechaOrden <= hasta);
        if (idProveedor is > 0) q = q.Where(c => c.IdProveedor == idProveedor);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = int.TryParse(t, out var numero)
                ? q.Where(c => c.NoOrdenCompra == numero || c.Proveedor!.Nombre.Contains(t))
                : q.Where(c => c.Proveedor!.Nombre.Contains(t));
        }

        return await q
            .OrderByDescending(c => c.FechaOrden).ThenByDescending(c => c.IdCompra)
            .Select(c => new CompraDto
            {
                IdCompra = c.IdCompra,
                NoOrdenCompra = c.NoOrdenCompra,
                IdProveedor = c.IdProveedor,
                Proveedor = c.Proveedor!.Nombre,
                FechaOrden = c.FechaOrden,
                FechaIngreso = c.FechaIngreso,
                Productos = c.Detalles.Count,
                Unidades = c.Detalles.Sum(d => d.Cantidad),
                Total = c.Detalles.Sum(d => d.Cantidad * d.PrecioCostoUnitario)
            })
            .ToListAsync();
    }

    public async Task<CompraDto> ObtenerAsync(int id)
    {
        var compra = await db.Compras.AsNoTracking()
            .Where(c => c.IdCompra == id)
            .Select(c => new CompraDto
            {
                IdCompra = c.IdCompra,
                NoOrdenCompra = c.NoOrdenCompra,
                IdProveedor = c.IdProveedor,
                Proveedor = c.Proveedor!.Nombre,
                FechaOrden = c.FechaOrden,
                FechaIngreso = c.FechaIngreso,
                Detalles = c.Detalles.OrderBy(d => d.IdCompraDetalle).Select(d => new CompraDetalleDto
                {
                    IdCompraDetalle = d.IdCompraDetalle,
                    IdProducto = d.IdProducto,
                    Producto = d.Producto!.Nombre,
                    Marca = d.Producto.Marca!.Nombre,
                    Imagen = d.Producto.Imagen,
                    Cantidad = d.Cantidad,
                    PrecioCostoUnitario = d.PrecioCostoUnitario
                }).ToList()
            })
            .FirstOrDefaultAsync() ?? throw new NoEncontradoException("La compra no existe.");

        compra.Productos = compra.Detalles.Count;
        compra.Unidades = compra.Detalles.Sum(d => d.Cantidad);
        compra.Total = compra.Detalles.Sum(d => d.Subtotal);
        return compra;
    }

    public async Task<int> SiguienteOrdenAsync() =>
        (await db.Compras.MaxAsync(c => (int?)c.NoOrdenCompra) ?? 1000) + 1;

    public async Task<CompraDto> CrearAsync(CompraGuardarDto datos)
    {
        ValidarDetalle(datos);
        await ValidarMaestro(datos, null);

        await using var tx = await db.Database.BeginTransactionAsync();

        var productos = await BloquearProductosAsync(datos.Detalles.Select(d => d.IdProducto));

        var compra = new Compra
        {
            NoOrdenCompra = datos.NoOrdenCompra,
            IdProveedor = datos.IdProveedor,
            FechaOrden = datos.FechaOrden!.Value,
            FechaIngreso = DateTime.Now
        };

        foreach (var linea in datos.Detalles)
        {
            compra.Detalles.Add(new CompraDetalle
            {
                IdProducto = linea.IdProducto,
                Cantidad = linea.Cantidad,
                PrecioCostoUnitario = linea.PrecioCostoUnitario
            });
            // Regla 1: la cantidad comprada aumenta la existencia
            productos[linea.IdProducto].Existencia += linea.Cantidad;
        }

        db.Compras.Add(compra);
        await db.SaveChangesAsync();

        // Reglas 2 y 3: precio de costo y precio de venta (+25 %)
        await ActualizarPreciosAsync(productos.Values);
        await db.SaveChangesAsync();

        await tx.CommitAsync();
        return await ObtenerAsync(compra.IdCompra);
    }

    public async Task<CompraDto> ActualizarAsync(int id, CompraGuardarDto datos)
    {
        ValidarDetalle(datos);
        await ValidarMaestro(datos, id);

        await using var tx = await db.Database.BeginTransactionAsync();

        var compra = await db.Compras.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.IdCompra == id)
                     ?? throw new NoEncontradoException("La compra no existe.");

        var ids = compra.Detalles.Select(d => d.IdProducto).Union(datos.Detalles.Select(d => d.IdProducto));
        var productos = await BloquearProductosAsync(ids);

        // Revertir el efecto del detalle anterior y aplicar el nuevo
        foreach (var anterior in compra.Detalles)
            productos[anterior.IdProducto].Existencia -= anterior.Cantidad;
        foreach (var linea in datos.Detalles)
            productos[linea.IdProducto].Existencia += linea.Cantidad;

        ValidarExistenciasNoNegativas(productos.Values, "modificar");

        db.ComprasDetalle.RemoveRange(compra.Detalles);
        compra.Detalles = datos.Detalles.Select(l => new CompraDetalle
        {
            IdCompra = id,
            IdProducto = l.IdProducto,
            Cantidad = l.Cantidad,
            PrecioCostoUnitario = l.PrecioCostoUnitario
        }).ToList();

        compra.NoOrdenCompra = datos.NoOrdenCompra;
        compra.IdProveedor = datos.IdProveedor;
        compra.FechaOrden = datos.FechaOrden!.Value;

        await db.SaveChangesAsync();
        await ActualizarPreciosAsync(productos.Values);
        await db.SaveChangesAsync();

        await tx.CommitAsync();
        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var compra = await db.Compras.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.IdCompra == id)
                     ?? throw new NoEncontradoException("La compra no existe.");

        var productos = await BloquearProductosAsync(compra.Detalles.Select(d => d.IdProducto));
        foreach (var d in compra.Detalles)
            productos[d.IdProducto].Existencia -= d.Cantidad;

        ValidarExistenciasNoNegativas(productos.Values, "eliminar");

        db.Compras.Remove(compra); // el detalle se elimina en cascada
        await db.SaveChangesAsync();
        await ActualizarPreciosAsync(productos.Values);
        await db.SaveChangesAsync();

        await tx.CommitAsync();
    }

    // ------------------------------------------------------------------

    private static void ValidarDetalle(CompraGuardarDto datos)
    {
        if (datos.Detalles.Count == 0)
            throw new ReglaNegocioException("Agregue al menos un producto a la compra.");

        var repetido = datos.Detalles.GroupBy(d => d.IdProducto).FirstOrDefault(g => g.Count() > 1);
        if (repetido is not null)
            throw new ReglaNegocioException("Hay un producto repetido en el detalle. Combine las cantidades en una sola línea.");
    }

    private async Task ValidarMaestro(CompraGuardarDto datos, int? idActual)
    {
        if (!await db.Proveedores.AnyAsync(p => p.IdProveedor == datos.IdProveedor))
            throw new ReglaNegocioException("El proveedor seleccionado no existe.");

        if (await db.Compras.AnyAsync(c => c.IdProveedor == datos.IdProveedor
                                          && c.NoOrdenCompra == datos.NoOrdenCompra
                                          && c.IdCompra != idActual))
            throw new ReglaNegocioException($"Ya existe la orden de compra No. {datos.NoOrdenCompra} para este proveedor.",
                StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// Bloquea las filas de los productos (SELECT ... FOR UPDATE) hasta el final de la transacción,
    /// para que dos compras/ventas simultáneas no modifiquen la misma existencia a la vez.
    /// </summary>
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

    private static void ValidarExistenciasNoNegativas(IEnumerable<Producto> productos, string accion)
    {
        var negativo = productos.FirstOrDefault(p => p.Existencia < 0);
        if (negativo is not null)
            throw new ReglaNegocioException(
                $"No se puede {accion} la compra: ya se vendieron unidades de '{negativo.Nombre}' " +
                $"y la existencia quedaría en {negativo.Existencia}.", StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// precio_costo = costo de la compra más reciente del producto;
    /// precio_venta = precio_costo + porcentaje de ganancia (25 % por defecto).
    /// </summary>
    private async Task ActualizarPreciosAsync(IEnumerable<Producto> productos)
    {
        var porcentaje = await configuracion.PorcentajeGananciaAsync();
        foreach (var producto in productos)
        {
            var ultimoCosto = await db.ComprasDetalle
                .Where(d => d.IdProducto == producto.IdProducto)
                .OrderByDescending(d => d.IdCompra).ThenByDescending(d => d.IdCompraDetalle)
                .Select(d => (decimal?)d.PrecioCostoUnitario)
                .FirstOrDefaultAsync();

            if (ultimoCosto is decimal costo)
            {
                producto.PrecioCosto = costo;
                producto.PrecioVenta = ConfiguracionService.CalcularPrecioVenta(costo, porcentaje);
            }
        }
    }
}
