using System.ComponentModel.DataAnnotations;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Shared.Clientes;

public class ClienteDto
{
    private string? _telefono;
    private string? _correo;

    public int IdCliente { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [MaxLength(60, ErrorMessage = "Los nombres no pueden tener más de 60 caracteres.")]
    [RegularExpression(Patrones.Nombre, ErrorMessage = "Los nombres solo pueden contener letras.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [MaxLength(60, ErrorMessage = "Los apellidos no pueden tener más de 60 caracteres.")]
    [RegularExpression(Patrones.Nombre, ErrorMessage = "Los apellidos solo pueden contener letras.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El NIT es obligatorio (use CF para consumidor final).")]
    [MaxLength(12, ErrorMessage = "El NIT no puede tener más de 12 caracteres.")]
    [Nit]
    public string Nit { get; set; } = "CF";

    /// <summary>true = Masculino, false = Femenino</summary>
    public bool Genero { get; set; } = true;

    [MaxLength(25, ErrorMessage = "El teléfono no puede tener más de 25 caracteres.")]
    [RegularExpression(Patrones.Telefono, ErrorMessage = "Teléfono inválido. Ej. 4410-2201")]
    public string? Telefono
    {
        get => _telefono;
        set => _telefono = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    [MaxLength(45, ErrorMessage = "El correo no puede tener más de 45 caracteres.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    public string? CorreoElectronico
    {
        get => _correo;
        set => _correo = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public DateTime? FechaIngreso { get; set; }

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}
