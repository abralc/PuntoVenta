namespace PuntoVenta.Shared.Reportes;

// Filas de los reportes. Cada uno combina información de varias tablas.

/// <summary>Reporte de productos: Producto | Marca | Existencia | Precio costo | Precio venta</summary>
public class ReporteProductoDto
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Existencia { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal ValorCosto => Existencia * PrecioCosto;
    public decimal ValorVenta => Existencia * PrecioVenta;
}

/// <summary>Reporte de existencias con estado (agotado / por agotarse / disponible).</summary>
public class ReporteExistenciaDto
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public int Existencia { get; set; }
    public int Reservado { get; set; }
    public int Disponible => Math.Max(0, Existencia - Reservado);
    public int UnidadesCompradas { get; set; }
    public int UnidadesVendidas { get; set; }
    public string Estado { get; set; } = string.Empty;
}

/// <summary>Reporte de compras: Fecha | No. orden | Proveedor | Total</summary>
public class ReporteCompraDto
{
    public int IdCompra { get; set; }
    public DateOnly FechaOrden { get; set; }
    public int NoOrdenCompra { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public int Productos { get; set; }
    public int Unidades { get; set; }
    public decimal Total { get; set; }
}

/// <summary>Reporte de ventas: Fecha | Cliente | Empleado | Total venta</summary>
public class ReporteVentaDto
{
    public int IdVenta { get; set; }
    public DateOnly FechaFactura { get; set; }
    public string Serie { get; set; } = string.Empty;
    public int NoFactura { get; set; }
    public string Factura => $"{Serie}-{NoFactura:000000}";
    public string Cliente { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string Empleado { get; set; } = string.Empty;
    public int Unidades { get; set; }
    public decimal Total { get; set; }
}

/// <summary>Indicadores para la pantalla de inicio.</summary>
public class ResumenDto
{
    public int VentasHoy { get; set; }
    public decimal TotalHoy { get; set; }
    public int VentasMes { get; set; }
    public decimal TotalMes { get; set; }
    public int ComprasMes { get; set; }
    public decimal TotalComprasMes { get; set; }
    public int ProductosPorAgotarse { get; set; }
    public int ProductosAgotados { get; set; }
    public decimal ValorInventario { get; set; }
    public List<TopProductoDto> MasVendidos { get; set; } = [];
    public List<VentaDiaDto> UltimosDias { get; set; } = [];
}

public class TopProductoDto
{
    public string Producto { get; set; } = string.Empty;
    public int Unidades { get; set; }
    public decimal Total { get; set; }
}

public class VentaDiaDto
{
    public DateOnly Fecha { get; set; }
    public decimal Total { get; set; }
    public int Ventas { get; set; }
}
