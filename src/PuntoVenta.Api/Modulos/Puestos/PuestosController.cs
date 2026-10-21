using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Shared.Comun;
using PuntoVenta.Shared.Puestos;

namespace PuntoVenta.Api.Modulos.Puestos;

/// <summary>
/// CRUD de puestos.
///   GET    /api/puestos?buscar=texto   GET /api/puestos/opciones   GET /api/puestos/5
///   POST   /api/puestos                PUT /api/puestos/5          DELETE /api/puestos/5
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PuestosController(IPuestoService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PuestoDto>>> Listar([FromQuery] string? buscar) =>
        await servicio.ListarAsync(buscar);

    [HttpGet("opciones")]
    public async Task<ActionResult<List<OpcionDto>>> Opciones() => await servicio.OpcionesAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PuestoDto>> Obtener(short id) => await servicio.ObtenerAsync(id);

    [HttpPost]
    public async Task<ActionResult<PuestoDto>> Crear(PuestoDto datos)
    {
        var creado = await servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdPuesto }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PuestoDto>> Actualizar(short id, PuestoDto datos) =>
        await servicio.ActualizarAsync(id, datos);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(short id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }
}
