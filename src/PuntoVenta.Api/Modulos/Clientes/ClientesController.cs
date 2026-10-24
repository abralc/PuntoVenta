using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Shared.Clientes;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Api.Modulos.Clientes;

/// <summary>
/// CRUD de clientes.
///   GET /api/clientes?buscar=texto   GET /api/clientes/opciones   GET /api/clientes/5
///   POST /api/clientes   PUT /api/clientes/5   DELETE /api/clientes/5
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController(IClienteService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ClienteDto>>> Listar([FromQuery] string? buscar) => await servicio.ListarAsync(buscar);

    [HttpGet("opciones")]
    public async Task<ActionResult<List<OpcionDto>>> Opciones() => await servicio.OpcionesAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Obtener(int id) => await servicio.ObtenerAsync(id);

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Crear(ClienteDto datos)
    {
        var creado = await servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdCliente }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Actualizar(int id, ClienteDto datos) => await servicio.ActualizarAsync(id, datos);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }
}
