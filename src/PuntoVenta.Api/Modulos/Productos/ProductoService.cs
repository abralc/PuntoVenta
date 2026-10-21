using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Configuracion;
using PuntoVenta.Api.Modulos.Productos.Entidades;
using PuntoVenta.Shared.Productos;

namespace PuntoVenta.Api.Modulos.Productos;

public interface IProductoService
{
    Task<List<ProductoDto>> ListarAsync(string? buscar, short? idMarca);
    Task<ProductoDto> ObtenerAsync(int id);
    Task<List<ProductoOpcionDto>> OpcionesAsync();
    Task<ProductoDto> CrearAsync(ProductoDto datos);
    Task<ProductoDto> ActualizarAsync(int id, ProductoDto datos);
    Task EliminarAsync(int id);
    Task<ImagenProductoDto> SubirImagenAsync(int id, IFormFile archivo);
    Task EliminarImagenAsync(int id);
}

public class ProductoService(AppDbContext db, IConfiguracionService configuracion, IImagenProductoService imagenes) : IProductoService
{
    public async Task<List<ProductoDto>> ListarAsync(string? buscar, short? idMarca)
    {
        var consulta = db.Productos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            consulta = consulta.Where(p => p.Nombre.Contains(t) || p.Descripcion.Contains(t) || p.Marca!.Nombre.Contains(t));
        }
        if (idMarca is > 0)
            consulta = consulta.Where(p => p.IdMarca == idMarca);

        var lista = await consulta
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoDto
            {
                IdProducto = p.IdProducto,
                Producto = p.Nombre,
                IdMarca = p.IdMarca,
                Marca = p.Marca!.Nombre,
                Descripcion = p.Descripcion,
                Imagen = p.Imagen,
                PrecioCosto = p.PrecioCosto,
                PrecioVenta = p.PrecioVenta,
                Existencia = p.Existencia,
                FechaIngreso = p.FechaIngreso
            })
            .ToListAsync();

        var reservas = await ReservasVigentesAsync();
        foreach (var p in lista) p.Reservado = reservas.GetValueOrDefault(p.IdProducto);
        return lista;
    }

    public async Task<ProductoDto> ObtenerAsync(int id)
    {
        var p = await db.Productos.AsNoTracking()
            .Where(x => x.IdProducto == id)
            .Select(x => new ProductoDto
            {
                IdProducto = x.IdProducto,
                Producto = x.Nombre,
                IdMarca = x.IdMarca,
                Marca = x.Marca!.Nombre,
                Descripcion = x.Descripcion,
                Imagen = x.Imagen,
                PrecioCosto = x.PrecioCosto,
                PrecioVenta = x.PrecioVenta,
                Existencia = x.Existencia,
                FechaIngreso = x.FechaIngreso
            })
            .FirstOrDefaultAsync() ?? throw new NoEncontradoException("El producto no existe.");

        p.Reservado = (await ReservasVigentesAsync()).GetValueOrDefault(id);
        return p;
    }

    public async Task<List<ProductoOpcionDto>> OpcionesAsync()
    {
        var lista = await db.Productos.AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoOpcionDto
            {
                IdProducto = p.IdProducto,
                Producto = p.Nombre,
                IdMarca = p.IdMarca,
                Marca = p.Marca!.Nombre,
                PrecioCosto = p.PrecioCosto,
                PrecioVenta = p.PrecioVenta,
                Existencia = p.Existencia,
                Imagen = p.Imagen
            })
            .ToListAsync();

        var reservas = await ReservasVigentesAsync();
        foreach (var p in lista) p.Reservado = reservas.GetValueOrDefault(p.IdProducto);
        return lista;
    }

    public async Task<ProductoDto> CrearAsync(ProductoDto datos)
    {
        var producto = new Producto { FechaIngreso = DateTime.Now, Existencia = datos.Existencia };
        await Copiar(datos, producto, null);
        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        return await ObtenerAsync(producto.IdProducto);
    }

    public async Task<ProductoDto> ActualizarAsync(int id, ProductoDto datos)
    {
        var producto = await db.Productos.FindAsync(id) ?? throw new NoEncontradoException("El producto no existe.");
        // La existencia NO se edita aquí: la modifican las compras (+) y las ventas (-)
        await Copiar(datos, producto, id);
        await db.SaveChangesAsync();
        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(int id)
    {
        var producto = await db.Productos.FindAsync(id) ?? throw new NoEncontradoException("El producto no existe.");
        var imagen = producto.Imagen;
        db.Productos.Remove(producto);
        await db.SaveChangesAsync(); // si tiene compras o ventas, la FK lo impide → 409
        imagenes.Eliminar(imagen);
    }

    public async Task<ImagenProductoDto> SubirImagenAsync(int id, IFormFile archivo)
    {
        var producto = await db.Productos.FindAsync(id) ?? throw new NoEncontradoException("El producto no existe.");
        var anterior = producto.Imagen;

        producto.Imagen = await imagenes.GuardarAsync(id, archivo);
        await db.SaveChangesAsync();

        imagenes.Eliminar(anterior);
        return new ImagenProductoDto { Imagen = producto.Imagen };
    }

    public async Task EliminarImagenAsync(int id)
    {
        var producto = await db.Productos.FindAsync(id) ?? throw new NoEncontradoException("El producto no existe.");
        var anterior = producto.Imagen;
        producto.Imagen = null;
        await db.SaveChangesAsync();
        imagenes.Eliminar(anterior);
    }

    private async Task Copiar(ProductoDto d, Producto p, int? idActual)
    {
        var nombre = d.Producto.Trim();
        if (!await db.Marcas.AnyAsync(m => m.IdMarca == d.IdMarca))
            throw new ReglaNegocioException("La marca seleccionada no existe.");
        if (await db.Productos.AnyAsync(x => x.Nombre == nombre && x.IdMarca == d.IdMarca && x.IdProducto != idActual))
            throw new ReglaNegocioException($"Ya existe el producto '{nombre}' para esa marca.", StatusCodes.Status409Conflict);

        p.Nombre = nombre;
        p.IdMarca = d.IdMarca;
        p.Descripcion = d.Descripcion.Trim();
        p.PrecioCosto = d.PrecioCosto;
        // Regla de negocio: precio de venta = precio de costo + 25 % (porcentaje configurable)
        p.PrecioVenta = ConfiguracionService.CalcularPrecioVenta(d.PrecioCosto, await configuracion.PorcentajeGananciaAsync());
    }

    /// <summary>Unidades reservadas en el POS por producto (reservas que no han vencido).</summary>
    private async Task<Dictionary<int, int>> ReservasVigentesAsync()
    {
        var filas = await db.Database.SqlQuery<ReservaPorProducto>(
            $"SELECT id_producto AS IdProducto, CAST(SUM(cantidad) AS SIGNED) AS Cantidad FROM reservas_pos WHERE expira > NOW() GROUP BY id_producto")
            .ToListAsync();
        return filas.ToDictionary(f => f.IdProducto, f => (int)f.Cantidad);
    }
}

/// <summary>Fila del total reservado por producto (resultado de SQL directo).</summary>
public class ReservaPorProducto
{
    public int IdProducto { get; set; }
    public long Cantidad { get; set; }
}
