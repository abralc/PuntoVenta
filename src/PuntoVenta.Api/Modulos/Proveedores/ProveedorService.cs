using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Proveedores.Entidades;
using PuntoVenta.Shared.Comun;
using PuntoVenta.Shared.Proveedores;

namespace PuntoVenta.Api.Modulos.Proveedores;

public interface IProveedorService
{
    Task<List<ProveedorDto>> ListarAsync(string? buscar);
    Task<ProveedorDto> ObtenerAsync(int id);
    Task<List<OpcionDto>> OpcionesAsync();
    Task<ProveedorDto> CrearAsync(ProveedorDto datos);
    Task<ProveedorDto> ActualizarAsync(int id, ProveedorDto datos);
    Task EliminarAsync(int id);
}

public class ProveedorService(AppDbContext db) : IProveedorService
{
    public async Task<List<ProveedorDto>> ListarAsync(string? buscar)
    {
        var consulta = db.Proveedores.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            consulta = consulta.Where(p => p.Nombre.Contains(t) || p.Nit.Contains(t)
                                           || p.Direccion.Contains(t) || p.Telefono.Contains(t));
        }

        // Compras por proveedor (SQL directo para no depender del módulo Compras)
        var conteos = await db.Database
            .SqlQuery<ConteoProveedor>($"SELECT id_proveedor AS IdProveedor, COUNT(*) AS Cantidad FROM compras GROUP BY id_proveedor")
            .ToListAsync();
        var porProveedor = conteos.ToDictionary(c => c.IdProveedor, c => (int)c.Cantidad);

        var lista = await consulta.OrderBy(p => p.Nombre).ToListAsync();
        return lista.Select(p => ADto(p, porProveedor.GetValueOrDefault(p.IdProveedor))).ToList();
    }

    public async Task<ProveedorDto> ObtenerAsync(int id)
    {
        var p = await db.Proveedores.AsNoTracking().FirstOrDefaultAsync(x => x.IdProveedor == id)
                ?? throw new NoEncontradoException("El proveedor no existe.");
        return ADto(p, 0);
    }

    public async Task<List<OpcionDto>> OpcionesAsync() =>
        await db.Proveedores.AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new OpcionDto { Id = p.IdProveedor, Texto = p.Nombre })
            .ToListAsync();

    public async Task<ProveedorDto> CrearAsync(ProveedorDto datos)
    {
        var proveedor = new Proveedor();
        await Copiar(datos, proveedor, null);
        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync();
        return await ObtenerAsync(proveedor.IdProveedor);
    }

    public async Task<ProveedorDto> ActualizarAsync(int id, ProveedorDto datos)
    {
        var proveedor = await db.Proveedores.FindAsync(id) ?? throw new NoEncontradoException("El proveedor no existe.");
        await Copiar(datos, proveedor, id);
        await db.SaveChangesAsync();
        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(int id)
    {
        var proveedor = await db.Proveedores.FindAsync(id) ?? throw new NoEncontradoException("El proveedor no existe.");
        db.Proveedores.Remove(proveedor);
        await db.SaveChangesAsync(); // si tiene compras, la FK lo impide → 409
    }

    private async Task Copiar(ProveedorDto d, Proveedor p, int? idActual)
    {
        var nit = NitAttribute.Normalizar(d.Nit);
        var nombre = d.Proveedor.Trim();

        if (await db.Proveedores.AnyAsync(x => x.Nit == nit && x.IdProveedor != idActual))
            throw new ReglaNegocioException($"Ya existe un proveedor con el NIT {nit}.", StatusCodes.Status409Conflict);
        if (await db.Proveedores.AnyAsync(x => x.Nombre == nombre && x.IdProveedor != idActual))
            throw new ReglaNegocioException($"Ya existe el proveedor '{nombre}'.", StatusCodes.Status409Conflict);

        p.Nombre = nombre;
        p.Nit = nit;
        p.Direccion = d.Direccion.Trim();
        p.Telefono = d.Telefono.Trim();
    }

    private static ProveedorDto ADto(Proveedor p, int compras) => new()
    {
        IdProveedor = p.IdProveedor,
        Proveedor = p.Nombre,
        Nit = p.Nit,
        Direccion = p.Direccion,
        Telefono = p.Telefono,
        Compras = compras
    };
}

/// <summary>Fila del conteo de compras por proveedor (resultado de SQL directo).</summary>
public class ConteoProveedor
{
    public int IdProveedor { get; set; }
    public long Cantidad { get; set; }
}
