using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Api.Modulos.Seguridad;

/// <summary>GET /api/menu — menú dinámico del usuario que inició sesión (según su rol).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController(IMenuService menu) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MenuItemDto>>> Get() =>
        await menu.ObtenerMenuPorRolAsync(User.Rol());
}
