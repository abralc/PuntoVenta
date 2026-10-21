using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Shared.Marcas;

public class MarcaDto
{
    public short IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [MaxLength(50, ErrorMessage = "La marca no puede tener más de 50 caracteres.")]
    public string Marca { get; set; } = string.Empty;

    /// <summary>Solo lectura: cuántos productos tienen esta marca.</summary>
    public int Productos { get; set; }
}
