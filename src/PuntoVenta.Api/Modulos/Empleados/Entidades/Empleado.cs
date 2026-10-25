using PuntoVenta.Api.Modulos.Puestos.Entidades;

namespace PuntoVenta.Api.Modulos.Empleados.Entidades;

/// <summary>Modelo de la tabla empleados.</summary>
public class Empleado
{
    public int IdEmpleado { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Dpi { get; set; } = string.Empty;
    public bool Genero { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public short IdPuesto { get; set; }
    public DateOnly FechaInicioLabores { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    public Puesto? Puesto { get; set; }
}
