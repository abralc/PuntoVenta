using System.ComponentModel.DataAnnotations;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Shared.Proveedores;

public class ProveedorDto
{
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El proveedor no puede tener más de 60 caracteres.")]
    [MinLength(3, ErrorMessage = "El proveedor debe tener al menos 3 caracteres.")]
    public string Proveedor { get; set; } = string.Empty;

    [Required(ErrorMessage = "El NIT es obligatorio.")]
    [MaxLength(12, ErrorMessage = "El NIT no puede tener más de 12 caracteres.")]
    [Nit(PermitirConsumidorFinal = false)]
    public string Nit { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [MaxLength(80, ErrorMessage = "La dirección no puede tener más de 80 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [MaxLength(25, ErrorMessage = "El teléfono no puede tener más de 25 caracteres.")]
    [RegularExpression(Patrones.Telefono, ErrorMessage = "Teléfono inválido. Ej. 2201-1001")]
    public string Telefono { get; set; } = string.Empty;

    /// <summary>Solo lectura: cantidad de compras registradas.</summary>
    public int Compras { get; set; }
}
