using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Shared.Reportes;

namespace PuntoVenta.Api.Modulos.Reportes;

/// <summary>
///   GET /api/reportes/productos?idMarca=1
///   GET /api/reportes/existencias?soloBajas=true
///   GET /api/reportes/compras?desde=2026-10-01&amp;hasta=2026-10-31&amp;idProveedor=1
///   GET /api/reportes/ventas?desde=&amp;hasta=&amp;idCliente=&amp;idEmpleado=
///   GET /api/reportes/resumen   (indicadores de la pantalla de inicio)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportesController(IReporteService servicio) : ControllerBase
{
    [HttpGet("productos")]
    public async Task<ActionResult<List<ReporteProductoDto>>> Productos([FromQuery] short? idMarca) =>
        await servicio.ProductosAsync(idMarca);

    [HttpGet("existencias")]
    public async Task<ActionResult<List<ReporteExistenciaDto>>> Existencias([FromQuery] bool soloBajas = false) =>
        await servicio.ExistenciasAsync(soloBajas);

    [HttpGet("compras")]
    public async Task<ActionResult<List<ReporteCompraDto>>> Compras([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] int? idProveedor) =>
        await servicio.ComprasAsync(desde, hasta, idProveedor);

    [HttpGet("ventas")]
    public async Task<ActionResult<List<ReporteVentaDto>>> Ventas([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] int? idCliente, [FromQuery] int? idEmpleado) =>
        await servicio.VentasAsync(desde, hasta, idCliente, idEmpleado);

    [HttpGet("resumen")]
    public async Task<ActionResult<ResumenDto>> Resumen() => await servicio.ResumenAsync();
}
