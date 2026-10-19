using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Shared.Pos;

/// <summary>Producto en la cuadrícula del POS, con disponibilidad en tiempo real.</summary>
public class PosProductoDto
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public short IdMarca { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Existencia { get; set; }
    /// <summary>Unidades reservadas por OTRAS cajas (reservas vigentes).</summary>
    public int ReservadoOtros { get; set; }
    /// <summary>Unidades que esta caja tiene en su ticket.</summary>
    public int EnMiTicket { get; set; }

    public string Codigo => $"P{IdProducto:0000}";
    /// <summary>Máximo que esta caja puede tener en su ticket.</summary>
    public int DisponibleParaMi => Math.Max(0, Existencia - ReservadoOtros);
    /// <summary>Unidades que aún se pueden agregar.</summary>
    public int Restante => Math.Max(0, DisponibleParaMi - EnMiTicket);
}

public class PosLineaDto
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Cantidad { get; set; }
    public int Maximo { get; set; }
    public decimal Subtotal => Math.Round(Cantidad * PrecioVenta, 2);
}

/// <summary>Ticket de una caja: lo que tiene reservado su sesión.</summary>
public class PosCarritoDto
{
    public string Sesion { get; set; } = string.Empty;
    public List<PosLineaDto> Lineas { get; set; } = [];
    /// <summary>Segundos que faltan para que venza la reserva (0 si el ticket está vacío).</summary>
    public int SegundosRestantes { get; set; }

    public int Unidades => Lineas.Sum(l => l.Cantidad);
    public decimal Total => Lineas.Sum(l => l.Subtotal);
}

/// <summary>PUT /api/pos/reservas: fija la cantidad de un producto en el ticket (0 = quitar).</summary>
public class PosReservaDto
{
    [Required(ErrorMessage = "La sesión del POS es obligatoria.")]
    public string Sesion { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Producto inválido.")]
    public int IdProducto { get; set; }

    [Range(0, 100_000, ErrorMessage = "La cantidad no puede ser negativa.")]
    public int Cantidad { get; set; }
}

/// <summary>POST /api/pos/cobrar: convierte el ticket en una venta.</summary>
public class PosCobrarDto
{
    [Required(ErrorMessage = "La sesión del POS es obligatoria.")]
    public string Sesion { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente.")]
    public int IdCliente { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un empleado.")]
    public int IdEmpleado { get; set; }
}
