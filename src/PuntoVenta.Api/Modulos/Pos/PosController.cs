using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Shared.Pos;
using PuntoVenta.Shared.Ventas;

namespace PuntoVenta.Api.Modulos.Pos;

/// <summary>
/// Punto de venta (POS).
///   GET    /api/pos/productos?sesion=GUID&amp;buscar=&amp;idMarca=   productos con disponibilidad
///   GET    /api/pos/carrito/{sesion}                          ticket de la caja
///   PUT    /api/pos/reservas                                  agrega / cambia / quita (cantidad 0)
///   DELETE /api/pos/carrito/{sesion}                          vaciar ticket (libera reservas)
///   POST   /api/pos/cobrar                                    registra la venta
/// </summary>
[ApiController]
[Route("api/pos")]
[Authorize]
public class PosController(IPosService servicio) : ControllerBase
{
    [HttpGet("productos")]
    public async Task<ActionResult<List<PosProductoDto>>> Productos([FromQuery] string sesion, [FromQuery] string? buscar,
        [FromQuery] short? idMarca) =>
        await servicio.ProductosAsync(sesion, buscar, idMarca);

    [HttpGet("carrito/{sesion}")]
    public async Task<ActionResult<PosCarritoDto>> Carrito(string sesion) => await servicio.CarritoAsync(sesion);

    [HttpPut("reservas")]
    public async Task<ActionResult<PosCarritoDto>> Reservar(PosReservaDto datos) =>
        await servicio.ReservarAsync(datos, User.IdUsuario());

    [HttpDelete("carrito/{sesion}")]
    public async Task<IActionResult> Vaciar(string sesion)
    {
        await servicio.VaciarAsync(sesion);
        return NoContent();
    }

    [HttpPost("cobrar")]
    public async Task<ActionResult<VentaDto>> Cobrar(PosCobrarDto datos) => await servicio.CobrarAsync(datos);
}
