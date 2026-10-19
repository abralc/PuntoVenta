namespace PuntoVenta.Shared.Comun;

/// <summary>Respuesta de GET /api/estado (verifica API y conexión a MySQL).</summary>
public class EstadoServidorDto
{
    public bool ApiActiva { get; set; }
    public bool BaseDatosConectada { get; set; }
    public string Servidor { get; set; } = string.Empty;
    public DateTime FechaServidor { get; set; }
    public string Version { get; set; } = string.Empty;
}
