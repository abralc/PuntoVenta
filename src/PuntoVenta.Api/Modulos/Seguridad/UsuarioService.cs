using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Seguridad.Entidades;
using PuntoVenta.Shared.Comun;
using PuntoVenta.Shared.Seguridad;

namespace PuntoVenta.Api.Modulos.Seguridad;

public interface IUsuarioService
{
    Task<List<UsuarioDto>> ListarAsync(string? buscar);
    Task<UsuarioDto> ObtenerAsync(int id);
    Task<List<OpcionDto>> RolesAsync();
    Task<UsuarioDto> CrearAsync(UsuarioDto datos);
    Task<UsuarioDto> ActualizarAsync(int id, UsuarioDto datos, int idUsuarioActual);
    Task EliminarAsync(int id, int idUsuarioActual);
}

/// <summary>Administración de usuarios del sistema. Las contraseñas se guardan con hash BCrypt.</summary>
public class UsuarioService(AppDbContext db) : IUsuarioService
{
    public async Task<List<UsuarioDto>> ListarAsync(string? buscar)
    {
        var q = db.Usuarios.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
            q = q.Where(u => u.NombreUsuario.Contains(buscar.Trim()) || u.Rol!.Nombre.Contains(buscar.Trim()));

        return await q.OrderBy(u => u.NombreUsuario).Select(u => new UsuarioDto
        {
            IdUsuario = u.IdUsuario,
            Usuario = u.NombreUsuario,
            IdEmpleado = u.IdEmpleado,
            IdRol = u.IdRol,
            Rol = u.Rol!.Nombre,
            Activo = u.Activo,
            FechaIngreso = u.FechaIngreso
        }).ToListAsync();
    }

    public async Task<UsuarioDto> ObtenerAsync(int id) =>
        (await db.Usuarios.AsNoTracking().Where(u => u.IdUsuario == id).Select(u => new UsuarioDto
        {
            IdUsuario = u.IdUsuario,
            Usuario = u.NombreUsuario,
            IdEmpleado = u.IdEmpleado,
            IdRol = u.IdRol,
            Rol = u.Rol!.Nombre,
            Activo = u.Activo,
            FechaIngreso = u.FechaIngreso
        }).FirstOrDefaultAsync()) ?? throw new NoEncontradoException("El usuario no existe.");

    public async Task<List<OpcionDto>> RolesAsync() =>
        await db.Roles.AsNoTracking().OrderBy(r => r.IdRol)
            .Select(r => new OpcionDto { Id = r.IdRol, Texto = r.Nombre })
            .ToListAsync();

    public async Task<UsuarioDto> CrearAsync(UsuarioDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Password))
            throw new ReglaNegocioException("La contraseña es obligatoria para un usuario nuevo.");

        await Validar(datos, null);
        var usuario = new Usuario
        {
            NombreUsuario = datos.Usuario.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(datos.Password),
            IdEmpleado = datos.IdEmpleado is > 0 ? datos.IdEmpleado : null,
            IdRol = datos.IdRol,
            Activo = datos.Activo,
            FechaIngreso = DateTime.Now
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return await ObtenerAsync(usuario.IdUsuario);
    }

    public async Task<UsuarioDto> ActualizarAsync(int id, UsuarioDto datos, int idUsuarioActual)
    {
        var usuario = await db.Usuarios.FindAsync(id) ?? throw new NoEncontradoException("El usuario no existe.");
        await Validar(datos, id);

        if (id == idUsuarioActual && (!datos.Activo || datos.IdRol != usuario.IdRol))
            throw new ReglaNegocioException("No puede desactivar ni cambiar el rol de su propio usuario.");

        usuario.NombreUsuario = datos.Usuario.Trim().ToLowerInvariant();
        usuario.IdEmpleado = datos.IdEmpleado is > 0 ? datos.IdEmpleado : null;
        usuario.IdRol = datos.IdRol;
        usuario.Activo = datos.Activo;
        if (!string.IsNullOrWhiteSpace(datos.Password))
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(datos.Password);

        await db.SaveChangesAsync();
        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(int id, int idUsuarioActual)
    {
        if (id == idUsuarioActual)
            throw new ReglaNegocioException("No puede eliminar su propio usuario.");
        var usuario = await db.Usuarios.FindAsync(id) ?? throw new NoEncontradoException("El usuario no existe.");
        db.Usuarios.Remove(usuario);
        await db.SaveChangesAsync();
    }

    private async Task Validar(UsuarioDto d, int? idActual)
    {
        var nombre = d.Usuario.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == nombre && u.IdUsuario != idActual))
            throw new ReglaNegocioException($"El usuario '{nombre}' ya existe.", StatusCodes.Status409Conflict);
        if (!await db.Roles.AnyAsync(r => r.IdRol == d.IdRol))
            throw new ReglaNegocioException("El rol seleccionado no existe.");

        if (d.IdEmpleado is int idEmpleado and > 0)
        {
            // SQL directo para no depender del módulo Empleados
            var existe = await db.Database
                .SqlQuery<long>($"SELECT COUNT(*) AS Value FROM empleados WHERE id_empleado = {idEmpleado}")
                .ToListAsync();
            if (existe.FirstOrDefault() == 0)
                throw new ReglaNegocioException("El empleado seleccionado no existe.");
            if (await db.Usuarios.AnyAsync(u => u.IdEmpleado == idEmpleado && u.IdUsuario != idActual))
                throw new ReglaNegocioException("Ese empleado ya tiene un usuario asignado.", StatusCodes.Status409Conflict);
        }
    }
}
