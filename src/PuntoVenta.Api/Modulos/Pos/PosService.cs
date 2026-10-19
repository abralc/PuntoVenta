using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Configuracion;
using PuntoVenta.Api.Modulos.Ventas;
using PuntoVenta.Shared.Pos;
using PuntoVenta.Shared.Ventas;

namespace PuntoVenta.Api.Modulos.Pos;

public interface IPosService
{
    Task<List<PosProductoDto>> ProductosAsync(string sesion, string? buscar, short? idMarca);
    Task<PosCarritoDto> CarritoAsync(string sesion);
    Task<PosCarritoDto> ReservarAsync(PosReservaDto datos, int idUsuario);
    Task VaciarAsync(string sesion);
    Task<VentaDto> CobrarAsync(PosCobrarDto datos);
}

/// <summary>
/// Punto de venta con RESERVA TEMPORAL entre cajas.
/// Cada pantalla POS abierta es una "sesión" (GUID). Al agregar un producto al ticket se
/// guarda una reserva en reservas_pos que vence en N minutos (configuración, 15 por defecto).
///   disponible para una caja = existencia − reservas vigentes de las OTRAS cajas
/// Así dos cajas no pueden vender la misma última unidad. Al cobrar, la venta se registra con
/// VentaService (mismas reglas que el módulo de Ventas) y las reservas se liberan.
/// </summary>
public class PosService(AppDbContext db, IConfiguracionService configuracion, IVentaService ventas) : IPosService
{
    public async Task<List<PosProductoDto>> ProductosAsync(string sesion, string? buscar, short? idMarca)
    {
        ValidarSesion(sesion);

        var q = db.Productos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = int.TryParse(t.TrimStart('P', 'p'), out var codigo)
                ? q.Where(p => p.IdProducto == codigo || p.Nombre.Contains(t))
                : q.Where(p => p.Nombre.Contains(t) || p.Marca!.Nombre.Contains(t) || p.Descripcion.Contains(t));
        }
        if (idMarca is > 0) q = q.Where(p => p.IdMarca == idMarca);

        var productos = await q.OrderBy(p => p.Nombre).Select(p => new PosProductoDto
        {
            IdProducto = p.IdProducto,
            Producto = p.Nombre,
            IdMarca = p.IdMarca,
            Marca = p.Marca!.Nombre,
            Imagen = p.Imagen,
            PrecioVenta = p.PrecioVenta,
            Existencia = p.Existencia
        }).ToListAsync();

        var reservas = await db.Database.SqlQuery<ReservaPosFila>($"""
            SELECT id_producto AS IdProducto,
                   CAST(SUM(CASE WHEN sesion_pos =  {sesion} THEN cantidad ELSE 0 END) AS SIGNED) AS Mio,
                   CAST(SUM(CASE WHEN sesion_pos <> {sesion} THEN cantidad ELSE 0 END) AS SIGNED) AS Otros
            FROM reservas_pos
            WHERE expira > NOW()
            GROUP BY id_producto
            """).ToListAsync();
        var porProducto = reservas.ToDictionary(r => r.IdProducto);

        foreach (var p in productos)
        {
            if (!porProducto.TryGetValue(p.IdProducto, out var r)) continue;
            p.EnMiTicket = (int)r.Mio;
            p.ReservadoOtros = (int)r.Otros;
        }
        return productos;
    }

    public async Task<PosCarritoDto> CarritoAsync(string sesion)
    {
        ValidarSesion(sesion);

        var filas = await db.Database.SqlQuery<LineaReservaFila>($"""
            SELECT id_producto AS IdProducto, cantidad AS Cantidad,
                   TIMESTAMPDIFF(SECOND, NOW(), expira) AS Segundos
            FROM reservas_pos
            WHERE sesion_pos = {sesion} AND expira > NOW()
            """).ToListAsync();

        var carrito = new PosCarritoDto { Sesion = sesion };
        if (filas.Count == 0) return carrito;

        var ids = filas.Select(f => f.IdProducto).ToList();
        var productos = await db.Productos.AsNoTracking()
            .Where(p => ids.Contains(p.IdProducto))
            .Select(p => new { p.IdProducto, p.Nombre, Marca = p.Marca!.Nombre, p.Imagen, p.PrecioVenta, p.Existencia })
            .ToListAsync();

        var otras = await ReservasDeOtrasCajasAsync(sesion);

        carrito.Lineas = filas
            .Join(productos, f => f.IdProducto, p => p.IdProducto, (f, p) => new PosLineaDto
            {
                IdProducto = p.IdProducto,
                Producto = p.Nombre,
                Marca = p.Marca,
                Imagen = p.Imagen,
                PrecioVenta = p.PrecioVenta,
                Cantidad = f.Cantidad,
                Maximo = Math.Max(0, p.Existencia - otras.GetValueOrDefault(p.IdProducto))
            })
            .OrderBy(l => l.Producto)
            .ToList();
        carrito.SegundosRestantes = (int)Math.Max(0, filas.Min(f => f.Segundos));
        return carrito;
    }

    public async Task<PosCarritoDto> ReservarAsync(PosReservaDto datos, int idUsuario)
    {
        ValidarSesion(datos.Sesion);
        var minutos = (await configuracion.ObtenerAsync()).MinutosReservaPos;

        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            // Bloquear la fila del producto: dos cajas no pueden reservar la misma unidad a la vez
            var producto = (await db.Productos
                    .FromSql($"SELECT * FROM productos WHERE id_producto = {datos.IdProducto} FOR UPDATE")
                    .ToListAsync())
                .FirstOrDefault() ?? throw new NoEncontradoException("El producto no existe.");

            await db.Database.ExecuteSqlAsync($"DELETE FROM reservas_pos WHERE expira <= NOW()");

            if (datos.Cantidad == 0)
            {
                await db.Database.ExecuteSqlAsync(
                    $"DELETE FROM reservas_pos WHERE sesion_pos = {datos.Sesion} AND id_producto = {datos.IdProducto}");
            }
            else
            {
                var otras = (await ReservasDeOtrasCajasAsync(datos.Sesion)).GetValueOrDefault(datos.IdProducto);
                var disponible = producto.Existencia - otras;
                if (datos.Cantidad > disponible)
                {
                    var motivo = otras > 0 ? $" ({otras} reservadas en otra caja)" : "";
                    throw new ReglaNegocioException(
                        $"Existencia insuficiente de '{producto.Nombre}': disponible {Math.Max(0, disponible)}{motivo}.",
                        StatusCodes.Status409Conflict);
                }

                await db.Database.ExecuteSqlAsync($"""
                    INSERT INTO reservas_pos (id_producto, cantidad, sesion_pos, id_usuario, expira)
                    VALUES ({datos.IdProducto}, {datos.Cantidad}, {datos.Sesion}, {idUsuario}, DATE_ADD(NOW(), INTERVAL {minutos} MINUTE))
                    ON DUPLICATE KEY UPDATE cantidad = VALUES(cantidad), id_usuario = VALUES(id_usuario), expira = VALUES(expira)
                    """);
            }

            // Cualquier movimiento renueva la reserva de todo el ticket
            await db.Database.ExecuteSqlAsync(
                $"UPDATE reservas_pos SET expira = DATE_ADD(NOW(), INTERVAL {minutos} MINUTE) WHERE sesion_pos = {datos.Sesion}");

            await tx.CommitAsync();
        }

        return await CarritoAsync(datos.Sesion);
    }

    public async Task VaciarAsync(string sesion)
    {
        ValidarSesion(sesion);
        await db.Database.ExecuteSqlAsync($"DELETE FROM reservas_pos WHERE sesion_pos = {sesion}");
    }

    public async Task<VentaDto> CobrarAsync(PosCobrarDto datos)
    {
        var carrito = await CarritoAsync(datos.Sesion);
        if (carrito.Lineas.Count == 0)
            throw new ReglaNegocioException("El ticket está vacío o la reserva venció. Agregue los productos de nuevo.");

        var venta = new VentaGuardarDto
        {
            IdCliente = datos.IdCliente,
            IdEmpleado = datos.IdEmpleado,
            FechaFactura = DateOnly.FromDateTime(DateTime.Today),
            Detalles = carrito.Lineas.Select(l => new VentaDetalleGuardarDto { IdProducto = l.IdProducto, Cantidad = l.Cantidad }).ToList()
        };

        // Mismas reglas del módulo de Ventas (verifica existencia, descuenta y usa precio_venta).
        // Se pasa la sesión para que sus propias reservas no le resten y se liberen al final.
        return await ventas.CrearAsync(venta, datos.Sesion);
    }

    // ------------------------------------------------------------------

    private static void ValidarSesion(string sesion)
    {
        if (!Guid.TryParse(sesion, out _))
            throw new ReglaNegocioException("Sesión de POS inválida. Recargue la página.");
    }

    private async Task<Dictionary<int, int>> ReservasDeOtrasCajasAsync(string sesion)
    {
        var filas = await db.Database.SqlQuery<ReservaPosFila>($"""
            SELECT id_producto AS IdProducto, CAST(0 AS SIGNED) AS Mio, CAST(SUM(cantidad) AS SIGNED) AS Otros
            FROM reservas_pos
            WHERE expira > NOW() AND sesion_pos <> {sesion}
            GROUP BY id_producto
            """).ToListAsync();
        return filas.ToDictionary(f => f.IdProducto, f => (int)f.Otros);
    }
}

/// <summary>Reservas por producto separadas en "mi caja" y "otras cajas" (resultado de SQL directo).</summary>
public class ReservaPosFila
{
    public int IdProducto { get; set; }
    public long Mio { get; set; }
    public long Otros { get; set; }
}

/// <summary>Línea reservada por una sesión (resultado de SQL directo).</summary>
public class LineaReservaFila
{
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public long Segundos { get; set; }
}
