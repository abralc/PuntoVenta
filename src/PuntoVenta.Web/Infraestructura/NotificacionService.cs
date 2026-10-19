namespace PuntoVenta.Web.Infraestructura;

public enum TipoNotificacion { Exito, Error, Advertencia, Info }

public record Notificacion(Guid Id, TipoNotificacion Tipo, string Mensaje);

/// <summary>
/// Notificaciones tipo "toast" para toda la aplicación.
///   Notificar.Exito("Puesto guardado");
///   Notificar.Error(ex.Message);
/// El componente &lt;Toasts /&gt; del layout las muestra.
/// </summary>
public class NotificacionService
{
    public event Action? Cambio;
    public List<Notificacion> Activas { get; } = [];

    public void Exito(string mensaje) => Agregar(TipoNotificacion.Exito, mensaje);
    public void Error(string mensaje) => Agregar(TipoNotificacion.Error, mensaje, 7000);
    public void Advertencia(string mensaje) => Agregar(TipoNotificacion.Advertencia, mensaje, 6000);
    public void Info(string mensaje) => Agregar(TipoNotificacion.Info, mensaje);

    public void Cerrar(Guid id)
    {
        Activas.RemoveAll(n => n.Id == id);
        Cambio?.Invoke();
    }

    private void Agregar(TipoNotificacion tipo, string mensaje, int milisegundos = 4000)
    {
        var n = new Notificacion(Guid.NewGuid(), tipo, mensaje);
        Activas.Add(n);
        Cambio?.Invoke();
        _ = Task.Delay(milisegundos).ContinueWith(_ => Cerrar(n.Id));
    }
}
