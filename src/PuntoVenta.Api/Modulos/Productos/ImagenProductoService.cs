using PuntoVenta.Api.Infraestructura;

namespace PuntoVenta.Api.Modulos.Productos;

public interface IImagenProductoService
{
    Task<string> GuardarAsync(int idProducto, IFormFile archivo);
    void Eliminar(string? rutaRelativa);
}

/// <summary>
/// Guarda las imágenes de productos en el disco del servidor:
///   {API}/wwwroot/images/productos/{id}-{aleatorio}.jpg
/// En la base de datos solo se guarda la ruta relativa: /images/productos/5-a1b2c3d4.jpg
/// </summary>
public class ImagenProductoService(IWebHostEnvironment entorno, ILogger<ImagenProductoService> logger) : IImagenProductoService
{
    public const long TamanoMaximo = 2 * 1024 * 1024; // 2 MB
    private const string CarpetaRelativa = "images/productos";

    private static readonly Dictionary<string, byte[][]> Firmas = new()
    {
        [".jpg"] = [[0xFF, 0xD8, 0xFF]],
        [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
        [".png"] = [[0x89, 0x50, 0x4E, 0x47]],
        [".webp"] = [[0x52, 0x49, 0x46, 0x46]] // "RIFF"
    };

    private string CarpetaFisica =>
        Path.Combine(entorno.WebRootPath ?? Path.Combine(entorno.ContentRootPath, "wwwroot"), "images", "productos");

    public async Task<string> GuardarAsync(int idProducto, IFormFile archivo)
    {
        if (archivo.Length == 0)
            throw new ReglaNegocioException("El archivo de imagen está vacío.");
        if (archivo.Length > TamanoMaximo)
            throw new ReglaNegocioException("La imagen no puede pesar más de 2 MB.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!Firmas.TryGetValue(extension, out var firmasValidas))
            throw new ReglaNegocioException("Formato no permitido. Use JPG, PNG o WEBP.");

        // Verificar que el contenido realmente sea una imagen (no solo la extensión)
        var cabecera = new byte[8];
        await using (var lectura = archivo.OpenReadStream())
        {
            var leidos = await lectura.ReadAsync(cabecera);
            if (leidos < 4 || !firmasValidas.Any(f => cabecera.Take(f.Length).SequenceEqual(f)))
                throw new ReglaNegocioException("El archivo no es una imagen válida.");
        }

        Directory.CreateDirectory(CarpetaFisica);
        // Nombre único: si se reemplaza la imagen, el navegador no muestra la anterior en caché
        var nombre = $"{idProducto}-{Guid.NewGuid().ToString("N")[..8]}{(extension == ".jpeg" ? ".jpg" : extension)}";
        var rutaFisica = Path.Combine(CarpetaFisica, nombre);

        await using (var destino = File.Create(rutaFisica))
            await archivo.CopyToAsync(destino);

        return $"/{CarpetaRelativa}/{nombre}";
    }

    public void Eliminar(string? rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa)) return;
        var nombre = Path.GetFileName(rutaRelativa); // evita rutas como ../../
        var rutaFisica = Path.Combine(CarpetaFisica, nombre);
        try
        {
            if (File.Exists(rutaFisica)) File.Delete(rutaFisica);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo eliminar la imagen {Ruta}", rutaFisica);
        }
    }
}
