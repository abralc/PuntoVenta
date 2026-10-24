namespace PuntoVenta.Api.Modulos.Proveedores.Entidades;

/// <summary>Modelo de la tabla proveedores.</summary>
public class Proveedor
{
    public int IdProveedor { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}
