namespace PuntoVenta.Web.Infraestructura;

/// <summary>
/// Diálogo de confirmación reutilizable (modal de Bootstrap controlado desde C#).
///   if (await Confirmar.PreguntarAsync("Eliminar puesto", $"¿Eliminar '{p.Puesto}'?"))
///       await Api.DeleteAsync($"api/puestos/{p.IdPuesto}");
/// </summary>
public class ConfirmacionService
{
    private TaskCompletionSource<bool>? _respuesta;

    public event Action? Cambio;
    public bool Visible { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Mensaje { get; private set; } = string.Empty;
    public string TextoAceptar { get; private set; } = "Aceptar";
    public bool EsPeligroso { get; private set; }

    public Task<bool> PreguntarAsync(string titulo, string mensaje, string textoAceptar = "Sí, eliminar", bool esPeligroso = true)
    {
        _respuesta?.TrySetResult(false);
        _respuesta = new TaskCompletionSource<bool>();
        Titulo = titulo;
        Mensaje = mensaje;
        TextoAceptar = textoAceptar;
        EsPeligroso = esPeligroso;
        Visible = true;
        Cambio?.Invoke();
        return _respuesta.Task;
    }

    public void Responder(bool aceptar)
    {
        Visible = false;
        Cambio?.Invoke();
        _respuesta?.TrySetResult(aceptar);
        _respuesta = null;
    }
}
