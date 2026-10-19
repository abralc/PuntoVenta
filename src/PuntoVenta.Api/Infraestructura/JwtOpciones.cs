namespace PuntoVenta.Api.Infraestructura;

/// <summary>Sección "Jwt" de appsettings.json.</summary>
public class JwtOpciones
{
    /// <summary>Clave secreta HS256. Mínimo 32 caracteres.</summary>
    public string Clave { get; set; } = string.Empty;
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int MinutosExpiracion { get; set; } = 480;
}

/// <summary>Nombres de los claims que viajan dentro del token.</summary>
public static class ClaimsPv
{
    public const string IdUsuario = "sub";
    public const string Usuario = "usuario";
    public const string Nombre = "name";
    public const string Rol = "role";
    public const string IdEmpleado = "id_empleado";
}
