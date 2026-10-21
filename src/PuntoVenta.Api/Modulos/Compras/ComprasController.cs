using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Shared.Compras;

namespace PuntoVenta.Api.Modulos.Compras;

/// <summary>
/// Compras maestro/detalle.
///   GET    /api/compras?desde=2026-10-01&amp;hasta=2026-10-31&amp;idProveedor=1&amp;buscar=1005
///   GET    /api/compras/5                 GET /api/compras/siguiente-orden
///   POST   /api/compras                   (maestro + detalle en un solo JSON)
///   PUT    /api/compras/5                 DELETE /api/compras/5
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComprasController(ICompraService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CompraDto>>> Listar([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] int? idProveedor, [FromQuery] string? buscar) =>
        await servicio.ListarAsync(desde, hasta, idProveedor, buscar);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompraDto>> Obtener(int id) => await servicio.ObtenerAsync(id);

    [HttpGet("siguiente-orden")]
    public async Task<ActionResult<int>> SiguienteOrden() => await servicio.SiguienteOrdenAsync();

    [HttpPost]
    public async Task<ActionResult<CompraDto>> Crear(CompraGuardarDto datos)
    {
        var creada = await servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(Obtener), new { id = creada.IdCompra }, creada);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CompraDto>> Actualizar(int id, CompraGuardarDto datos) =>
        await servicio.ActualizarAsync(id, datos);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }
}
