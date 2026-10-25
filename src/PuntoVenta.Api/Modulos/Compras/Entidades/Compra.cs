using PuntoVenta.Api.Modulos.Productos.Entidades;
using PuntoVenta.Api.Modulos.Proveedores.Entidades;

namespace PuntoVenta.Api.Modulos.Compras.Entidades;

/// <summary>Maestro: tabla compras.</summary>
public class Compra
{
    public int IdCompra { get; set; }
    public int NoOrdenCompra { get; set; }
    public int IdProveedor { get; set; }
    public DateOnly FechaOrden { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    public Proveedor? Proveedor { get; set; }
    public List<CompraDetalle> Detalles { get; set; } = [];
}

/// <summary>Detalle: tabla compras_detalle.</summary>
public class CompraDetalle
{
    public long IdCompraDetalle { get; set; }
    public int IdCompra { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioCostoUnitario { get; set; }

    public Compra? Compra { get; set; }
    public Producto? Producto { get; set; }
}
