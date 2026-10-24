namespace PuntoVenta.Api.Modulos.Clientes.Entidades;

/// <summary>Modelo de la tabla clientes.</summary>
public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Nit { get; set; } = "CF";
    public bool Genero { get; set; }
    public string? Telefono { get; set; }
    public string? CorreoElectronico { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;
}
