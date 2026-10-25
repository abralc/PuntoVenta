using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Shared.Seguridad;

public class LoginDto
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [MaxLength(30, ErrorMessage = "El usuario no puede tener más de 30 caracteres.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MaxLength(100, ErrorMessage = "La contraseña no puede tener más de 100 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRespuestaDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int? IdEmpleado { get; set; }
}
