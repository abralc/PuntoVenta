using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Shared.Seguridad;

namespace PuntoVenta.Api.Modulos.Seguridad;

/// <summary>
/// Inicio de sesión.
///   POST /api/auth/login   → público, devuelve el JWT
///   GET  /api/auth/perfil  → protegido, devuelve los datos del token
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginRespuestaDto>> Login(LoginDto datos) =>
        await auth.IniciarSesionAsync(datos);

    [HttpGet("perfil")]
    [Authorize]
    public IActionResult Perfil() => Ok(new
    {
        idUsuario = User.IdUsuario(),
        usuario = User.FindFirst(ClaimsPv.Usuario)?.Value,
        nombre = User.Identity?.Name,
        rol = User.Rol(),
        idEmpleado = User.IdEmpleado()
    });
}
