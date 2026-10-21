using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Shared.Productos;

namespace PuntoVenta.Api.Modulos.Productos;

/// <summary>
/// CRUD de productos + imagen.
///   GET    /api/productos?buscar=texto&amp;idMarca=1   GET /api/productos/opciones   GET /api/productos/5
///   POST   /api/productos   PUT /api/productos/5   DELETE /api/productos/5
///   POST   /api/productos/5/imagen  (multipart/form-data, campo "archivo")
///   DELETE /api/productos/5/imagen
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController(IProductoService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProductoDto>>> Listar([FromQuery] string? buscar, [FromQuery] short? idMarca) =>
        await servicio.ListarAsync(buscar, idMarca);

    [HttpGet("opciones")]
    public async Task<ActionResult<List<ProductoOpcionDto>>> Opciones() => await servicio.OpcionesAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> Obtener(int id) => await servicio.ObtenerAsync(id);

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Crear(ProductoDto datos)
    {
        var creado = await servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdProducto }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductoDto>> Actualizar(int id, ProductoDto datos) =>
        await servicio.ActualizarAsync(id, datos);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/imagen")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ImagenProductoService.TamanoMaximo + 64 * 1024)]
    public async Task<ActionResult<ImagenProductoDto>> SubirImagen(int id, IFormFile archivo) =>
        await servicio.SubirImagenAsync(id, archivo);

    [HttpDelete("{id:int}/imagen")]
    public async Task<IActionResult> EliminarImagen(int id)
    {
        await servicio.EliminarImagenAsync(id);
        return NoContent();
    }
}
