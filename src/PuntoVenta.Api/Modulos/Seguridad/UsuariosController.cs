using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Shared.Comun;
using PuntoVenta.Shared.Seguridad;

namespace PuntoVenta.Api.Modulos.Seguridad;

/// <summary>
/// Administración de usuarios (solo rol Administrador).
///   GET /api/usuarios?buscar=   GET /api/usuarios/roles   GET /api/usuarios/5
///   POST /api/usuarios   PUT /api/usuarios/5   DELETE /api/usuarios/5
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class UsuariosController(IUsuarioService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> Listar([FromQuery] string? buscar) => await servicio.ListarAsync(buscar);

    [HttpGet("roles")]
    public async Task<ActionResult<List<OpcionDto>>> Roles() => await servicio.RolesAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Obtener(int id) => await servicio.ObtenerAsync(id);

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear(UsuarioDto datos)
    {
        var creado = await servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdUsuario }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Actualizar(int id, UsuarioDto datos) =>
        await servicio.ActualizarAsync(id, datos, User.IdUsuario());

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await servicio.EliminarAsync(id, User.IdUsuario());
        return NoContent();
    }
}
