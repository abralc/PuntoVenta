using System.ComponentModel.DataAnnotations;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Shared.Empleados;

public class EmpleadoDto
{
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [MaxLength(60, ErrorMessage = "Los nombres no pueden tener más de 60 caracteres.")]
    [RegularExpression(Patrones.Nombre, ErrorMessage = "Los nombres solo pueden contener letras.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [MaxLength(60, ErrorMessage = "Los apellidos no pueden tener más de 60 caracteres.")]
    [RegularExpression(Patrones.Nombre, ErrorMessage = "Los apellidos solo pueden contener letras.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [MaxLength(80, ErrorMessage = "La dirección no puede tener más de 80 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [MaxLength(25, ErrorMessage = "El teléfono no puede tener más de 25 caracteres.")]
    [RegularExpression(Patrones.Telefono, ErrorMessage = "Teléfono inválido. Ej. 5512-3456")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DPI es obligatorio.")]
    [RegularExpression(Patrones.Dpi, ErrorMessage = "El DPI debe tener exactamente 13 dígitos.")]
    public string Dpi { get; set; } = string.Empty;

    /// <summary>true = Masculino, false = Femenino</summary>
    public bool Genero { get; set; } = true;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [EdadMinima(18, ErrorMessage = "El empleado debe ser mayor de edad (18 años).")]
    public DateOnly? FechaNacimiento { get; set; }

    [Range(1, short.MaxValue, ErrorMessage = "Seleccione un puesto.")]
    public short IdPuesto { get; set; }

    [Required(ErrorMessage = "La fecha de inicio de labores es obligatoria.")]
    [FechaNoFutura(ErrorMessage = "La fecha de inicio de labores no puede ser posterior a hoy.")]
    public DateOnly? FechaInicioLabores { get; set; }

    // ---- Solo lectura (los llena la API) ----
    public string? Puesto { get; set; }
    public DateTime? FechaIngreso { get; set; }

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}
