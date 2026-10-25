using System.ComponentModel.DataAnnotations;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Shared.Ventas;

/// <summary>Venta (maestro) con su detalle, para consultar e imprimir.</summary>
public class VentaDto
{
    public int IdVenta { get; set; }
    public int NoFactura { get; set; }
    public string Serie { get; set; } = "A";
    public DateOnly FechaFactura { get; set; }
    public int IdCliente { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string NitCliente { get; set; } = "CF";
    public int IdEmpleado { get; set; }
    public string Empleado { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public int Productos { get; set; }
    public int Unidades { get; set; }
    public decimal Total { get; set; }
    public List<VentaDetalleDto> Detalles { get; set; } = [];

    public string Factura => $"{Serie}-{NoFactura:000000}";
}

public class VentaDetalleDto
{
    public long IdVentaDetalle { get; set; }
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public int Cantidad { get; set; }
    /// <summary>precio_venta del producto al momento de la venta.</summary>
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Math.Round(Cantidad * PrecioUnitario, 2);
}

/// <summary>
/// Datos para registrar o modificar una venta (maestro + detalle en una sola operación).
/// El precio NO viaja desde el cliente: la API usa el precio_venta de la tabla productos.
/// </summary>
public class VentaGuardarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente.")]
    public int IdCliente { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un empleado.")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "La fecha de la factura es obligatoria.")]
    [FechaNoFutura(ErrorMessage = "La fecha de la factura no puede ser posterior a hoy.")]
    public DateOnly? FechaFactura { get; set; }

    [MinLength(1, ErrorMessage = "Agregue al menos un producto a la venta.")]
    public List<VentaDetalleGuardarDto> Detalles { get; set; } = [];
}

public class VentaDetalleGuardarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto.")]
    public int IdProducto { get; set; }

    [Range(1, 100_000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; } = 1;
}

/// <summary>Respuesta de GET /api/ventas/disponibilidad/{idProducto}.</summary>
public class DisponibilidadDto
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public decimal PrecioVenta { get; set; }
    public int Existencia { get; set; }
    /// <summary>Unidades apartadas por cajas POS (reservas vigentes de otras sesiones).</summary>
    public int Reservado { get; set; }
    public int Disponible => Math.Max(0, Existencia - Reservado);
}
