using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Api.Infraestructura;

/// <summary>
/// Convierte cualquier excepción en una respuesta JSON uniforme (ErrorApiDto),
/// así Blazor siempre recibe un mensaje claro en lugar de un error 500 genérico.
/// </summary>
public class ManejoErroresMiddleware(RequestDelegate siguiente, ILogger<ManejoErroresMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await siguiente(contexto);
        }
        catch (ReglaNegocioException ex)
        {
            await Responder(contexto, ex.CodigoHttp, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await Responder(contexto, StatusCodes.Status409Conflict,
                "El registro fue modificado por otro usuario. Actualice la pantalla e intente de nuevo.");
        }
        catch (Exception ex) when ((ex as MySqlException ?? ex.InnerException as MySqlException) is { } mysql)
        {
            var (codigo, mensaje) = mysql.ErrorCode switch
            {
                MySqlErrorCode.DuplicateKeyEntry =>
                    (StatusCodes.Status409Conflict, "Ya existe un registro con esos datos."),
                MySqlErrorCode.RowIsReferenced2 =>
                    (StatusCodes.Status409Conflict, "No se puede eliminar porque tiene registros relacionados."),
                MySqlErrorCode.NoReferencedRow2 =>
                    (StatusCodes.Status400BadRequest, "Uno de los registros seleccionados no existe."),
                (MySqlErrorCode)3819 => // ER_CHECK_CONSTRAINT_VIOLATED
                    (StatusCodes.Status400BadRequest, "Valor fuera de rango (cantidades, precios o existencia no pueden ser negativos)."),
                _ => (StatusCodes.Status500InternalServerError, "Error al guardar en la base de datos.")
            };
            logger.LogWarning(ex, "Error de MySQL {Codigo}", mysql.ErrorCode);
            await Responder(contexto, codigo, mensaje);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado en {Ruta}", contexto.Request.Path);
            await Responder(contexto, StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado en el servidor.");
        }
    }

    private static async Task Responder(HttpContext contexto, int codigo, string mensaje)
    {
        if (contexto.Response.HasStarted) return;
        // No se usa Response.Clear() para conservar los encabezados CORS ya agregados
        contexto.Response.StatusCode = codigo;
        await contexto.Response.WriteAsJsonAsync(new ErrorApiDto { Mensaje = mensaje });
    }
}
