namespace PuntoVenta.Shared.Comun;

/// <summary>Opción del menú dinámico. Hijos forma el árbol padre-hijo.</summary>
public class MenuItemDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icono { get; set; }
    public int Orden { get; set; }
    public List<MenuItemDto> Hijos { get; set; } = [];
}
