using PuntoVenta.Api.Modulos.Marcas.Entidades;

namespace PuntoVenta.Api.Modulos.Productos.Entidades;

/// <summary>Modelo de la tabla productos.</summary>
public class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public short IdMarca { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    /// <summary>Ruta relativa de la imagen, ej. /images/productos/5-a1b2c3.jpg (la imagen NO se guarda en la BD).</summary>
    public string? Imagen { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Existencia { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    public Marca? Marca { get; set; }
}
