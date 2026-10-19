namespace PuntoVenta.Shared.Comun;

/// <summary>
/// Formato único de error que devuelve la API. Blazor lo lee para mostrar
/// el mensaje en un toast y los errores por campo en el formulario.
/// </summary>
public class ErrorApiDto
{
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Errores por campo (nombre de la propiedad → mensajes). Puede ser null.</summary>
    public Dictionary<string, string[]>? Errores { get; set; }
}
