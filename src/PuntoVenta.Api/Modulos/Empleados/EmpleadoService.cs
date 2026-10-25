using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Empleados.Entidades;
using PuntoVenta.Shared.Comun;
using PuntoVenta.Shared.Empleados;

namespace PuntoVenta.Api.Modulos.Empleados;

public interface IEmpleadoService
{
    Task<List<EmpleadoDto>> ListarAsync(string? buscar, short? idPuesto);
    Task<EmpleadoDto> ObtenerAsync(int id);
    Task<List<OpcionDto>> OpcionesAsync();
    Task<EmpleadoDto> CrearAsync(EmpleadoDto datos);
    Task<EmpleadoDto> ActualizarAsync(int id, EmpleadoDto datos);
    Task EliminarAsync(int id);
}

public class EmpleadoService(AppDbContext db) : IEmpleadoService
{
    public async Task<List<EmpleadoDto>> ListarAsync(string? buscar, short? idPuesto)
    {
        var consulta = db.Empleados.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            consulta = consulta.Where(e => e.Nombres.Contains(t) || e.Apellidos.Contains(t)
                                           || e.Dpi.Contains(t) || e.Telefono.Contains(t)
                                           || e.Puesto!.Nombre.Contains(t));
        }
        if (idPuesto is > 0)
            consulta = consulta.Where(e => e.IdPuesto == idPuesto);

        return await consulta
            .OrderBy(e => e.Nombres).ThenBy(e => e.Apellidos)
            .Select(e => new EmpleadoDto
            {
                IdEmpleado = e.IdEmpleado,
                Nombres = e.Nombres,
                Apellidos = e.Apellidos,
                Direccion = e.Direccion,
                Telefono = e.Telefono,
                Dpi = e.Dpi,
                Genero = e.Genero,
                FechaNacimiento = e.FechaNacimiento,
                IdPuesto = e.IdPuesto,
                Puesto = e.Puesto!.Nombre,
                FechaInicioLabores = e.FechaInicioLabores,
                FechaIngreso = e.FechaIngreso
            })
            .ToListAsync();
    }

    public async Task<EmpleadoDto> ObtenerAsync(int id) =>
        (await db.Empleados.AsNoTracking()
            .Where(e => e.IdEmpleado == id)
            .Select(e => new EmpleadoDto
            {
                IdEmpleado = e.IdEmpleado,
                Nombres = e.Nombres,
                Apellidos = e.Apellidos,
                Direccion = e.Direccion,
                Telefono = e.Telefono,
                Dpi = e.Dpi,
                Genero = e.Genero,
                FechaNacimiento = e.FechaNacimiento,
                IdPuesto = e.IdPuesto,
                Puesto = e.Puesto!.Nombre,
                FechaInicioLabores = e.FechaInicioLabores,
                FechaIngreso = e.FechaIngreso
            })
            .FirstOrDefaultAsync())
        ?? throw new NoEncontradoException("El empleado no existe.");

    public async Task<List<OpcionDto>> OpcionesAsync() =>
        await db.Empleados.AsNoTracking()
            .OrderBy(e => e.Nombres).ThenBy(e => e.Apellidos)
            .Select(e => new OpcionDto { Id = e.IdEmpleado, Texto = e.Nombres + " " + e.Apellidos })
            .ToListAsync();

    public async Task<EmpleadoDto> CrearAsync(EmpleadoDto datos)
    {
        await Validar(datos, null);
        var empleado = new Empleado { FechaIngreso = DateTime.Now };
        Copiar(datos, empleado);
        db.Empleados.Add(empleado);
        await db.SaveChangesAsync();
        return await ObtenerAsync(empleado.IdEmpleado);
    }

    public async Task<EmpleadoDto> ActualizarAsync(int id, EmpleadoDto datos)
    {
        var empleado = await db.Empleados.FindAsync(id) ?? throw new NoEncontradoException("El empleado no existe.");
        await Validar(datos, id);
        Copiar(datos, empleado);
        await db.SaveChangesAsync();
        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(int id)
    {
        var empleado = await db.Empleados.FindAsync(id) ?? throw new NoEncontradoException("El empleado no existe.");
        db.Empleados.Remove(empleado);
        await db.SaveChangesAsync(); // si tiene ventas, la FK lo impide → 409
    }

    private async Task Validar(EmpleadoDto d, int? idActual)
    {
        if (!await db.Puestos.AnyAsync(p => p.IdPuesto == d.IdPuesto))
            throw new ReglaNegocioException("El puesto seleccionado no existe.");

        if (d.FechaNacimiento is not null && d.FechaInicioLabores is not null
            && d.FechaInicioLabores <= d.FechaNacimiento.Value.AddYears(14))
            throw new ReglaNegocioException("La fecha de inicio de labores no es coherente con la fecha de nacimiento.");

        var dpi = d.Dpi.Trim();
        if (await db.Empleados.AnyAsync(e => e.Dpi == dpi && e.IdEmpleado != idActual))
            throw new ReglaNegocioException($"Ya existe un empleado con el DPI {dpi}.", StatusCodes.Status409Conflict);
    }

    private static void Copiar(EmpleadoDto d, Empleado e)
    {
        e.Nombres = d.Nombres.Trim();
        e.Apellidos = d.Apellidos.Trim();
        e.Direccion = d.Direccion.Trim();
        e.Telefono = d.Telefono.Trim();
        e.Dpi = d.Dpi.Trim();
        e.Genero = d.Genero;
        e.FechaNacimiento = d.FechaNacimiento!.Value;
        e.IdPuesto = d.IdPuesto;
        e.FechaInicioLabores = d.FechaInicioLabores!.Value;
    }
}
