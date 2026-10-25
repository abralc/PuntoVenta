using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Modulos.Configuracion;
using PuntoVenta.Api.Modulos.Productos;
using PuntoVenta.Shared.Reportes;

namespace PuntoVenta.Api.Modulos.Reportes;

public interface IReporteService
{
    Task<List<ReporteProductoDto>> ProductosAsync(short? idMarca);
    Task<List<ReporteExistenciaDto>> ExistenciasAsync(bool soloBajas);
    Task<List<ReporteCompraDto>> ComprasAsync(DateOnly? desde, DateOnly? hasta, int? idProveedor);
    Task<List<ReporteVentaDto>> VentasAsync(DateOnly? desde, DateOnly? hasta, int? idCliente, int? idEmpleado);
    Task<ResumenDto> ResumenAsync();
}

/// <summary>Reportes que combinan información de varias tablas (JOIN mediante navegaciones de EF Core).</summary>
public class ReporteService(AppDbContext db, IConfiguracionService configuracion) : IReporteService
{
    public async Task<List<ReporteProductoDto>> ProductosAsync(short? idMarca)
    {
        var q = db.Productos.AsNoTracking();
        if (idMarca is > 0) q = q.Where(p => p.IdMarca == idMarca);
        return await q
            .OrderBy(p => p.Marca!.Nombre).ThenBy(p => p.Nombre)
            .Select(p => new ReporteProductoDto
            {
                IdProducto = p.IdProducto,
                Producto = p.Nombre,
                Marca = p.Marca!.Nombre,
                Descripcion = p.Descripcion,
                Existencia = p.Existencia,
                PrecioCosto = p.PrecioCosto,
                PrecioVenta = p.PrecioVenta
            })
            .ToListAsync();
    }

    public async Task<List<ReporteExistenciaDto>> ExistenciasAsync(bool soloBajas)
    {
        var minima = (await configuracion.ObtenerAsync()).ExistenciaMinima;

        var lista = await db.Productos.AsNoTracking()
            .OrderBy(p => p.Existencia).ThenBy(p => p.Nombre)
            .Select(p => new ReporteExistenciaDto
            {
                IdProducto = p.IdProducto,
                Producto = p.Nombre,
                Marca = p.Marca!.Nombre,
                Existencia = p.Existencia,
                UnidadesCompradas = db.ComprasDetalle.Where(d => d.IdProducto == p.IdProducto).Sum(d => d.Cantidad),
                UnidadesVendidas = db.VentasDetalle.Where(d => d.IdProducto == p.IdProducto).Sum(d => d.Cantidad)
            })
            .ToListAsync();

        var reservas = await db.Database.SqlQuery<ReservaPorProducto>(
                $"SELECT id_producto AS IdProducto, CAST(SUM(cantidad) AS SIGNED) AS Cantidad FROM reservas_pos WHERE expira > NOW() GROUP BY id_producto")
            .ToListAsync();
        var porProducto = reservas.ToDictionary(r => r.IdProducto, r => (int)r.Cantidad);

        foreach (var p in lista)
        {
            p.Reservado = porProducto.GetValueOrDefault(p.IdProducto);
            p.Estado = p.Existencia <= 0 ? "Agotado" : p.Existencia <= minima ? "Por agotarse" : "Disponible";
        }

        return soloBajas ? lista.Where(p => p.Estado != "Disponible").ToList() : lista;
    }

    public async Task<List<ReporteCompraDto>> ComprasAsync(DateOnly? desde, DateOnly? hasta, int? idProveedor)
    {
        var q = db.Compras.AsNoTracking();
        if (desde is not null) q = q.Where(c => c.FechaOrden >= desde);
        if (hasta is not null) q = q.Where(c => c.FechaOrden <= hasta);
        if (idProveedor is > 0) q = q.Where(c => c.IdProveedor == idProveedor);

        return await q
            .OrderByDescending(c => c.FechaOrden).ThenByDescending(c => c.IdCompra)
            .Select(c => new ReporteCompraDto
            {
                IdCompra = c.IdCompra,
                FechaOrden = c.FechaOrden,
                NoOrdenCompra = c.NoOrdenCompra,
                Proveedor = c.Proveedor!.Nombre,
                Productos = c.Detalles.Count,
                Unidades = c.Detalles.Sum(d => d.Cantidad),
                Total = c.Detalles.Sum(d => d.Cantidad * d.PrecioCostoUnitario)
            })
            .ToListAsync();
    }

    public async Task<List<ReporteVentaDto>> VentasAsync(DateOnly? desde, DateOnly? hasta, int? idCliente, int? idEmpleado)
    {
        var q = db.Ventas.AsNoTracking();
        if (desde is not null) q = q.Where(v => v.FechaFactura >= desde);
        if (hasta is not null) q = q.Where(v => v.FechaFactura <= hasta);
        if (idCliente is > 0) q = q.Where(v => v.IdCliente == idCliente);
        if (idEmpleado is > 0) q = q.Where(v => v.IdEmpleado == idEmpleado);

        var lista = await q
            .OrderByDescending(v => v.FechaFactura).ThenByDescending(v => v.IdVenta)
            .Select(v => new ReporteVentaDto
            {
                IdVenta = v.IdVenta,
                FechaFactura = v.FechaFactura,
                Serie = v.Serie,
                NoFactura = v.NoFactura,
                Cliente = v.Cliente!.Nombres + " " + v.Cliente.Apellidos,
                Nit = v.Cliente.Nit,
                Empleado = v.Empleado!.Nombres + " " + v.Empleado.Apellidos,
                Unidades = v.Detalles.Sum(d => d.Cantidad),
                Total = v.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
            })
            .ToListAsync();
        return lista;
    }

    public async Task<ResumenDto> ResumenAsync()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
        var hace7 = hoy.AddDays(-6);
        var minima = (await configuracion.ObtenerAsync()).ExistenciaMinima;

        var ventasMes = await VentasAsync(inicioMes, hoy, null, null);
        var comprasMes = await ComprasAsync(inicioMes, hoy, null);
        var ultimos = await VentasAsync(hace7, hoy, null, null);

        // Agrupar por id (traducción segura a SQL) y luego buscar los nombres
        var top = await db.VentasDetalle.AsNoTracking()
            .GroupBy(d => d.IdProducto)
            .Select(g => new
            {
                IdProducto = g.Key,
                Unidades = g.Sum(d => d.Cantidad),
                Total = g.Sum(d => d.Cantidad * d.PrecioUnitario)
            })
            .OrderByDescending(t => t.Unidades)
            .Take(5)
            .ToListAsync();
        var idsTop = top.Select(t => t.IdProducto).ToList();
        var nombres = await db.Productos.AsNoTracking()
            .Where(p => idsTop.Contains(p.IdProducto))
            .ToDictionaryAsync(p => p.IdProducto, p => p.Nombre);
        var masVendidos = top.Select(t => new TopProductoDto
        {
            Producto = nombres.GetValueOrDefault(t.IdProducto, $"#{t.IdProducto}"),
            Unidades = t.Unidades,
            Total = t.Total
        }).ToList();

        return new ResumenDto
        {
            VentasHoy = ventasMes.Count(v => v.FechaFactura == hoy),
            TotalHoy = ventasMes.Where(v => v.FechaFactura == hoy).Sum(v => v.Total),
            VentasMes = ventasMes.Count,
            TotalMes = ventasMes.Sum(v => v.Total),
            ComprasMes = comprasMes.Count,
            TotalComprasMes = comprasMes.Sum(c => c.Total),
            ProductosAgotados = await db.Productos.CountAsync(p => p.Existencia <= 0),
            ProductosPorAgotarse = await db.Productos.CountAsync(p => p.Existencia > 0 && p.Existencia <= minima),
            ValorInventario = await db.Productos.SumAsync(p => p.Existencia * p.PrecioCosto),
            MasVendidos = masVendidos,
            UltimosDias = Enumerable.Range(0, 7).Select(i => hace7.AddDays(i)).Select(f => new VentaDiaDto
            {
                Fecha = f,
                Ventas = ultimos.Count(v => v.FechaFactura == f),
                Total = ultimos.Where(v => v.FechaFactura == f).Sum(v => v.Total)
            }).ToList()
        };
    }
}
