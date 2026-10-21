using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Shared.Productos;

public class ProductoDto
{
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El producto no puede tener más de 50 caracteres.")]
    public string Producto { get; set; } = string.Empty;

    [Range(1, short.MaxValue, ErrorMessage = "Seleccione una marca.")]
    public short IdMarca { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [MaxLength(100, ErrorMessage = "La descripción no puede tener más de 100 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "El precio de costo debe ser mayor a cero (máximo 999,999.99).")]
    public decimal PrecioCosto { get; set; }

    /// <summary>Calculado por la API: precio de costo + porcentaje de ganancia (25 %).</summary>
    public decimal PrecioVenta { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "La existencia no puede ser negativa.")]
    public int Existencia { get; set; }

    // ---- Solo lectura ----
    public string? Imagen { get; set; }
    public string? Marca { get; set; }
    public DateTime? FechaIngreso { get; set; }
    /// <summary>Unidades apartadas en cajas POS (reservas vigentes).</summary>
    public int Reservado { get; set; }
    public int Disponible => Math.Max(0, Existencia - Reservado);
}

/// <summary>Producto para combos de compras y ventas (incluye precio y disponibilidad).</summary>
public class ProductoOpcionDto
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public short IdMarca { get; set; }
    public string Marca { get; set; } = string.Empty;
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Existencia { get; set; }
    public int Reservado { get; set; }
    public string? Imagen { get; set; }

    public int Disponible => Math.Max(0, Existencia - Reservado);
    public string Texto => $"{Producto} · {Marca}";
}

public class ImagenProductoDto
{
    public string? Imagen { get; set; }
}
