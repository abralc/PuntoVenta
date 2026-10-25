using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Shared.Seguridad;

namespace PuntoVenta.Api.Modulos.Seguridad;

public interface IAuthService
{
    Task<LoginRespuestaDto> IniciarSesionAsync(LoginDto datos);
}

/// <summary>Valida usuario y contraseña contra la tabla usuarios y genera el JWT.</summary>
public class AuthService(AppDbContext db, IJwtService jwt) : IAuthService
{
    private const string MensajeCredenciales = "Usuario o contraseña incorrectos.";

    public async Task<LoginRespuestaDto> IniciarSesionAsync(LoginDto datos)
    {
        var nombreUsuario = datos.Usuario.Trim();

        var usuario = await db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

        // Mismo mensaje si el usuario no existe o la contraseña es incorrecta (no dar pistas)
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(datos.Password, usuario.PasswordHash))
            throw new ReglaNegocioException(MensajeCredenciales, StatusCodes.Status401Unauthorized);

        if (!usuario.Activo)
            throw new ReglaNegocioException("El usuario está inactivo. Contacte al administrador.", StatusCodes.Status403Forbidden);

        // Nombre del empleado asociado (consulta directa para no depender del módulo Empleados)
        var nombre = usuario.NombreUsuario;
        if (usuario.IdEmpleado is int idEmpleado)
        {
            var nombreEmpleado = await db.Database
                .SqlQuery<string>($"SELECT CONCAT(nombres, ' ', apellidos) AS Value FROM empleados WHERE id_empleado = {idEmpleado}")
                .FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(nombreEmpleado)) nombre = nombreEmpleado;
        }

        var rol = usuario.Rol?.Nombre ?? string.Empty;
        var (token, expira) = jwt.GenerarToken(usuario, rol, nombre);

        return new LoginRespuestaDto
        {
            Token = token,
            Expira = expira,
            Usuario = usuario.NombreUsuario,
            Nombre = nombre,
            Rol = rol,
            IdEmpleado = usuario.IdEmpleado
        };
    }
}
