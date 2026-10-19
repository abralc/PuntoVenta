using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Shared.Configuracion;

/// <summary>Parámetros generales del sistema (tabla configuracion).</summary>
public class ConfiguracionDto
{
    [Required(ErrorMessage = "El porcentaje de ganancia es obligatorio.")]
    [Range(typeof(decimal), "0", "500", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "El porcentaje de ganancia debe estar entre 0 y 500.")]
    public decimal PorcentajeGanancia { get; set; } = 25;

    [Required(ErrorMessage = "La serie es obligatoria.")]
    [RegularExpression("^[A-Z]$", ErrorMessage = "La serie debe ser una letra mayúscula (A-Z).")]
    public string SerieFactura { get; set; } = "A";

    [Range(1, 120, ErrorMessage = "Los minutos de reserva deben estar entre 1 y 120.")]
    public int MinutosReservaPos { get; set; } = 15;

    [Range(0, 1000, ErrorMessage = "La existencia mínima debe estar entre 0 y 1000.")]
    public int ExistenciaMinima { get; set; } = 5;
}

public class RecalculoPreciosDto
{
    public int ProductosActualizados { get; set; }
    public decimal PorcentajeAplicado { get; set; }
}
