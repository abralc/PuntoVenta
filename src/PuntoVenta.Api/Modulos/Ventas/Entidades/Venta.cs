using PuntoVenta.Api.Modulos.Clientes.Entidades;
using PuntoVenta.Api.Modulos.Empleados.Entidades;
using PuntoVenta.Api.Modulos.Productos.Entidades;

namespace PuntoVenta.Api.Modulos.Ventas.Entidades;

/// <summary>Maestro: tabla ventas.</summary>
public class Venta
{
    public int IdVenta { get; set; }
    public int NoFactura { get; set; }
    public string Serie { get; set; } = "A";
    public DateOnly FechaFactura { get; set; }
    public int IdCliente { get; set; }
    public int IdEmpleado { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    public Cliente? Cliente { get; set; }
    public Empleado? Empleado { get; set; }
    public List<VentaDetalle> Detalles { get; set; } = [];
}

/// <summary>Detalle: tabla ventas_detalle.</summary>
public class VentaDetalle
{
    public long IdVentaDetalle { get; set; }
    public int IdVenta { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public Venta? Venta { get; set; }
    public Producto? Producto { get; set; }
}
