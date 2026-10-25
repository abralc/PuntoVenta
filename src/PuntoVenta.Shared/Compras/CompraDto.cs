using System.ComponentModel.DataAnnotations;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Shared.Compras;

/// <summary>Compra (maestro) con su detalle, para consultar.</summary>
public class CompraDto
{
    public int IdCompra { get; set; }
    public int NoOrdenCompra { get; set; }
    public int IdProveedor { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public DateOnly FechaOrden { get; set; }
    public DateTime FechaIngreso { get; set; }
    public int Productos { get; set; }
    public int Unidades { get; set; }
    public decimal Total { get; set; }
    public List<CompraDetalleDto> Detalles { get; set; } = [];
}

public class CompraDetalleDto
{
    public long IdCompraDetalle { get; set; }
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioCostoUnitario { get; set; }
    public decimal Subtotal => Math.Round(Cantidad * PrecioCostoUnitario, 2);
}

/// <summary>Datos para registrar o modificar una compra desde una misma interfaz (maestro + detalle).</summary>
public class CompraGuardarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El número de orden debe ser mayor a cero.")]
    public int NoOrdenCompra { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un proveedor.")]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "La fecha de la orden es obligatoria.")]
    [FechaNoFutura(ErrorMessage = "La fecha de la orden no puede ser posterior a hoy.")]
    public DateOnly? FechaOrden { get; set; }

    [MinLength(1, ErrorMessage = "Agregue al menos un producto a la compra.")]
    public List<CompraDetalleGuardarDto> Detalles { get; set; } = [];
}

public class CompraDetalleGuardarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto.")]
    public int IdProducto { get; set; }

    [Range(1, 100_000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; } = 1;

    [Range(typeof(decimal), "0.01", "999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "El precio de costo debe ser mayor a cero.")]
    public decimal PrecioCostoUnitario { get; set; }
}
