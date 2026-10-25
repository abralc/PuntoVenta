using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Shared.Seguridad;

public class UsuarioDto
{
    private string? _password;

    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]{3,30}$",
        ErrorMessage = "El usuario debe tener de 3 a 30 caracteres: letras, números, punto, guion o guion bajo (sin espacios).")]
    public string Usuario { get; set; } = string.Empty;

    /// <summary>Obligatoria al crear. Al editar: vacía = no cambiar.</summary>
    [MaxLength(100, ErrorMessage = "La contraseña no puede tener más de 100 caracteres.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres, con letras y números.")]
    public string? Password
    {
        get => _password;
        set => _password = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public int? IdEmpleado { get; set; }

    [Range(1, short.MaxValue, ErrorMessage = "Seleccione un rol.")]
    public short IdRol { get; set; }

    public bool Activo { get; set; } = true;

    // ---- Solo lectura ----
    public string? Rol { get; set; }
    public DateTime? FechaIngreso { get; set; }
}
