namespace PuntoVenta.Shared.Comun;

/// <summary>Elemento para combos / listas desplegables (GET api/xxx/opciones).</summary>
public class OpcionDto
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
}
