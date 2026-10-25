using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Shared.Ventas;

namespace PuntoVenta.Api.Modulos.Ventas;

/// <summary>
/// Ventas maestro/detalle.
///   GET    /api/ventas?desde=&amp;hasta=&amp;idCliente=&amp;idEmpleado=&amp;buscar=
///   GET    /api/ventas/5        GET /api/ventas/disponibilidad/3   (verifica existencia antes de agregar)
///   POST   /api/ventas          PUT /api/ventas/5      DELETE /api/ventas/5 (anula y devuelve existencia)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentasController(IVentaService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<VentaDto>>> Listar([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] int? idCliente, [FromQuery] int? idEmpleado, [FromQuery] string? buscar) =>
        await servicio.ListarAsync(desde, hasta, idCliente, idEmpleado, buscar);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> Obtener(int id) => await servicio.ObtenerAsync(id);

    [HttpGet("disponibilidad/{idProducto:int}")]
    public async Task<ActionResult<DisponibilidadDto>> Disponibilidad(int idProducto) =>
        await servicio.DisponibilidadAsync(idProducto);

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Crear(VentaGuardarDto datos)
    {
        var creada = await servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(Obtener), new { id = creada.IdVenta }, creada);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VentaDto>> Actualizar(int id, VentaGuardarDto datos) =>
        await servicio.ActualizarAsync(id, datos);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }
}
