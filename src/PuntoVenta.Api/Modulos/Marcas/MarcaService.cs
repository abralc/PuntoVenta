using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Marcas.Entidades;
using PuntoVenta.Shared.Comun;
using PuntoVenta.Shared.Marcas;

namespace PuntoVenta.Api.Modulos.Marcas;

public interface IMarcaService
{
    Task<List<MarcaDto>> ListarAsync(string? buscar);
    Task<MarcaDto> ObtenerAsync(short id);
    Task<List<OpcionDto>> OpcionesAsync();
    Task<MarcaDto> CrearAsync(MarcaDto datos);
    Task<MarcaDto> ActualizarAsync(short id, MarcaDto datos);
    Task EliminarAsync(short id);
}

public class MarcaService(AppDbContext db) : IMarcaService
{
    public async Task<List<MarcaDto>> ListarAsync(string? buscar)
    {
        var consulta = db.Marcas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
            consulta = consulta.Where(m => m.Nombre.Contains(buscar.Trim()));

        var conteos = await db.Productos.AsNoTracking()
            .GroupBy(p => p.IdMarca)
            .Select(g => new { IdMarca = g.Key, Cantidad = g.Count() })
            .ToListAsync();
        var porMarca = conteos.ToDictionary(c => c.IdMarca, c => c.Cantidad);

        var marcas = await consulta.OrderBy(m => m.Nombre).ToListAsync();
        return marcas.Select(m => new MarcaDto
        {
            IdMarca = m.IdMarca,
            Marca = m.Nombre,
            Productos = porMarca.GetValueOrDefault(m.IdMarca)
        }).ToList();
    }

    public async Task<MarcaDto> ObtenerAsync(short id)
    {
        var m = await db.Marcas.AsNoTracking().FirstOrDefaultAsync(x => x.IdMarca == id)
                ?? throw new NoEncontradoException("La marca no existe.");
        return new MarcaDto { IdMarca = m.IdMarca, Marca = m.Nombre };
    }

    public async Task<List<OpcionDto>> OpcionesAsync() =>
        await db.Marcas.AsNoTracking()
            .OrderBy(m => m.Nombre)
            .Select(m => new OpcionDto { Id = m.IdMarca, Texto = m.Nombre })
            .ToListAsync();

    public async Task<MarcaDto> CrearAsync(MarcaDto datos)
    {
        var nombre = datos.Marca.Trim();
        await ValidarDuplicado(nombre, null);
        var marca = new Marca { Nombre = nombre };
        db.Marcas.Add(marca);
        await db.SaveChangesAsync();
        return await ObtenerAsync(marca.IdMarca);
    }

    public async Task<MarcaDto> ActualizarAsync(short id, MarcaDto datos)
    {
        var marca = await db.Marcas.FindAsync(id) ?? throw new NoEncontradoException("La marca no existe.");
        var nombre = datos.Marca.Trim();
        await ValidarDuplicado(nombre, id);
        marca.Nombre = nombre;
        await db.SaveChangesAsync();
        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(short id)
    {
        var marca = await db.Marcas.FindAsync(id) ?? throw new NoEncontradoException("La marca no existe.");
        db.Marcas.Remove(marca);
        await db.SaveChangesAsync(); // si tiene productos, la FK lo impide → 409
    }

    private async Task ValidarDuplicado(string nombre, short? idActual)
    {
        if (await db.Marcas.AnyAsync(m => m.Nombre == nombre && m.IdMarca != idActual))
            throw new ReglaNegocioException($"Ya existe la marca '{nombre}'.", StatusCodes.Status409Conflict);
    }
}
