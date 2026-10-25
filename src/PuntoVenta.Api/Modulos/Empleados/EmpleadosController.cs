using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Shared.Comun;
using PuntoVenta.Shared.Empleados;

namespace PuntoVenta.Api.Modulos.Empleados;

/// <summary>
/// CRUD de empleados.
///   GET /api/empleados?buscar=texto&amp;idPuesto=2   GET /api/empleados/opciones   GET /api/empleados/5
///   POST /api/empleados   PUT /api/empleados/5   DELETE /api/empleados/5
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpleadosController(IEmpleadoService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EmpleadoDto>>> Listar([FromQuery] string? buscar, [FromQuery] short? idPuesto) =>
        await servicio.ListarAsync(buscar, idPuesto);

    [HttpGet("opciones")]
    public async Task<ActionResult<List<OpcionDto>>> Opciones() => await servicio.OpcionesAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmpleadoDto>> Obtener(int id) => await servicio.ObtenerAsync(id);

    [HttpPost]
    public async Task<ActionResult<EmpleadoDto>> Crear(EmpleadoDto datos)
    {
        var creado = await servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdEmpleado }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EmpleadoDto>> Actualizar(int id, EmpleadoDto datos) =>
        await servicio.ActualizarAsync(id, datos);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }
}
