using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Api.Data;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Api.Modulos.Estado;

/// <summary>
/// GET /api/estado — público. Sirve para comprobar desde otra máquina de la red
/// que la API responde y que tiene conexión con MySQL.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class EstadoController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<EstadoServidorDto>> Get()
    {
        bool conectada;
        try { conectada = await db.Database.CanConnectAsync(); }
        catch { conectada = false; }

        return new EstadoServidorDto
        {
            ApiActiva = true,
            BaseDatosConectada = conectada,
            Servidor = Environment.MachineName,
            FechaServidor = DateTime.Now,
            Version = typeof(EstadoController).Assembly.GetName().Version?.ToString() ?? "1.0"
        };
    }
}
