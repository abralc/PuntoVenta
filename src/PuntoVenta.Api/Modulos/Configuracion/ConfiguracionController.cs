using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Shared.Configuracion;

namespace PuntoVenta.Api.Modulos.Configuracion;

/// <summary>
///   GET  /api/configuracion                     → cualquier usuario con sesión
///   PUT  /api/configuracion                     → solo Administrador
///   POST /api/configuracion/recalcular-precios  → solo Administrador
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracionController(IConfiguracionService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ConfiguracionDto>> Get() => await servicio.ObtenerAsync();

    [HttpPut]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ConfiguracionDto>> Put(ConfiguracionDto datos) => await servicio.GuardarAsync(datos);

    [HttpPost("recalcular-precios")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<RecalculoPreciosDto>> RecalcularPrecios() => await servicio.RecalcularPreciosAsync();
}
