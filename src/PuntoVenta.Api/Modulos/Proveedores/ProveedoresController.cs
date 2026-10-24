using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Shared.Comun;
using PuntoVenta.Shared.Proveedores;

namespace PuntoVenta.Api.Modulos.Proveedores;

/// <summary>
/// CRUD de proveedores.
///   GET /api/proveedores?buscar=texto   GET /api/proveedores/opciones   GET /api/proveedores/5
///   POST /api/proveedores   PUT /api/proveedores/5   DELETE /api/proveedores/5
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProveedoresController(IProveedorService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProveedorDto>>> Listar([FromQuery] string? buscar) => await servicio.ListarAsync(buscar);

    [HttpGet("opciones")]
    public async Task<ActionResult<List<OpcionDto>>> Opciones() => await servicio.OpcionesAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProveedorDto>> Obtener(int id) => await servicio.ObtenerAsync(id);

    [HttpPost]
    public async Task<ActionResult<ProveedorDto>> Crear(ProveedorDto datos)
    {
        var creado = await servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdProveedor }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProveedorDto>> Actualizar(int id, ProveedorDto datos) => await servicio.ActualizarAsync(id, datos);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }
}
