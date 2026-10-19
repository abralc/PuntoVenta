namespace PuntoVenta.Api.Infraestructura;

/// <summary>
/// Error de regla de negocio (ej. "Existencia insuficiente").
/// El middleware la convierte en una respuesta JSON con el código indicado.
/// </summary>
public class ReglaNegocioException(string mensaje, int codigoHttp = StatusCodes.Status400BadRequest)
    : Exception(mensaje)
{
    public int CodigoHttp { get; } = codigoHttp;
}

/// <summary>Registro no encontrado → 404.</summary>
public class NoEncontradoException(string mensaje)
    : ReglaNegocioException(mensaje, StatusCodes.Status404NotFound);
