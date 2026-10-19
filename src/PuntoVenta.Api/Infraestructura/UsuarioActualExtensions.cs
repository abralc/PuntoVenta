using System.Security.Claims;

namespace PuntoVenta.Api.Infraestructura;

/// <summary>
/// Atajos para leer el usuario que inició sesión desde el token, en cualquier
/// controlador: User.IdUsuario(), User.IdEmpleado(), User.Rol().
/// </summary>
public static class UsuarioActualExtensions
{
    public static int IdUsuario(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimsPv.IdUsuario), out var id)
            ? id
            : throw new ReglaNegocioException("Sesión inválida.", StatusCodes.Status401Unauthorized);

    public static int? IdEmpleado(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimsPv.IdEmpleado), out var id) ? id : null;

    public static string Rol(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimsPv.Rol) ?? string.Empty;

    public static bool EsAdministrador(this ClaimsPrincipal user) =>
        user.Rol() == "Administrador";
}
