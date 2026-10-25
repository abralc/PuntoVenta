using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Shared.Puestos;

public class PuestoDto
{
    public short IdPuesto { get; set; }

    [Required(ErrorMessage = "El nombre del puesto es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El puesto no puede tener más de 50 caracteres.")]
    [MinLength(3, ErrorMessage = "El puesto debe tener al menos 3 caracteres.")]
    public string Puesto { get; set; } = string.Empty;

    /// <summary>Solo lectura: cuántos empleados tienen este puesto.</summary>
    public int Empleados { get; set; }
}
