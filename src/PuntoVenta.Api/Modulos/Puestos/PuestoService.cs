using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Puestos.Entidades;
using PuntoVenta.Shared.Comun;
using PuntoVenta.Shared.Puestos;

namespace PuntoVenta.Api.Modulos.Puestos;

public interface IPuestoService
{
    Task<List<PuestoDto>> ListarAsync(string? buscar);
    Task<PuestoDto> ObtenerAsync(short id);
    Task<List<OpcionDto>> OpcionesAsync();
    Task<PuestoDto> CrearAsync(PuestoDto datos);
    Task<PuestoDto> ActualizarAsync(short id, PuestoDto datos);
    Task EliminarAsync(short id);
}

public class PuestoService(AppDbContext db) : IPuestoService
{
    public async Task<List<PuestoDto>> ListarAsync(string? buscar)
    {
        var consulta = db.Puestos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
            consulta = consulta.Where(p => p.Nombre.Contains(buscar.Trim()));

        // Cantidad de empleados por puesto (SQL directo para no depender del módulo Empleados)
        var conteos = await db.Database
            .SqlQuery<ConteoPuesto>($"SELECT id_puesto AS IdPuesto, COUNT(*) AS Cantidad FROM empleados GROUP BY id_puesto")
            .ToListAsync();
        var porPuesto = conteos.ToDictionary(c => c.IdPuesto, c => (int)c.Cantidad);

        var puestos = await consulta.OrderBy(p => p.Nombre).ToListAsync();
        return puestos.Select(p => new PuestoDto
        {
            IdPuesto = p.IdPuesto,
            Puesto = p.Nombre,
            Empleados = porPuesto.GetValueOrDefault(p.IdPuesto)
        }).ToList();
    }

    public async Task<PuestoDto> ObtenerAsync(short id)
    {
        var p = await db.Puestos.AsNoTracking().FirstOrDefaultAsync(x => x.IdPuesto == id)
                ?? throw new NoEncontradoException("El puesto no existe.");
        return new PuestoDto { IdPuesto = p.IdPuesto, Puesto = p.Nombre };
    }

    public async Task<List<OpcionDto>> OpcionesAsync() =>
        await db.Puestos.AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new OpcionDto { Id = p.IdPuesto, Texto = p.Nombre })
            .ToListAsync();

    public async Task<PuestoDto> CrearAsync(PuestoDto datos)
    {
        var nombre = datos.Puesto.Trim();
        await ValidarDuplicado(nombre, null);

        var puesto = new Puesto { Nombre = nombre };
        db.Puestos.Add(puesto);
        await db.SaveChangesAsync();
        return await ObtenerAsync(puesto.IdPuesto);
    }

    public async Task<PuestoDto> ActualizarAsync(short id, PuestoDto datos)
    {
        var puesto = await db.Puestos.FindAsync(id) ?? throw new NoEncontradoException("El puesto no existe.");
        var nombre = datos.Puesto.Trim();
        await ValidarDuplicado(nombre, id);

        puesto.Nombre = nombre;
        await db.SaveChangesAsync();
        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(short id)
    {
        var puesto = await db.Puestos.FindAsync(id) ?? throw new NoEncontradoException("El puesto no existe.");
        db.Puestos.Remove(puesto);
        // Si tiene empleados, MySQL rechaza el borrado (FK) y el middleware responde 409 con un mensaje claro
        await db.SaveChangesAsync();
    }

    private async Task ValidarDuplicado(string nombre, short? idActual)
    {
        if (await db.Puestos.AnyAsync(p => p.Nombre == nombre && p.IdPuesto != idActual))
            throw new ReglaNegocioException($"Ya existe el puesto '{nombre}'.", StatusCodes.Status409Conflict);
    }
}

/// <summary>Fila del conteo de empleados por puesto (resultado de SQL directo).</summary>
public class ConteoPuesto
{
    public short IdPuesto { get; set; }
    public long Cantidad { get; set; }
}
