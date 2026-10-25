using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Shared.Comun;
using PuntoVenta.Shared.Marcas;

namespace PuntoVenta.Api.Modulos.Marcas;

/// <summary>
/// CRUD de marcas.
///   GET /api/marcas?buscar=texto   GET /api/marcas/opciones   GET /api/marcas/5
///   POST /api/marcas   PUT /api/marcas/5   DELETE /api/marcas/5
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarcasController(IMarcaService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MarcaDto>>> Listar([FromQuery] string? buscar) => await servicio.ListarAsync(buscar);

    [HttpGet("opciones")]
    public async Task<ActionResult<List<OpcionDto>>> Opciones() => await servicio.OpcionesAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MarcaDto>> Obtener(short id) => await servicio.ObtenerAsync(id);

    [HttpPost]
    public async Task<ActionResult<MarcaDto>> Crear(MarcaDto datos)
    {
        var creada = await servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(Obtener), new { id = creada.IdMarca }, creada);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MarcaDto>> Actualizar(short id, MarcaDto datos) => await servicio.ActualizarAsync(id, datos);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(short id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }
}
