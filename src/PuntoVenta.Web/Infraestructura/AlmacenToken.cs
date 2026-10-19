using Microsoft.JSInterop;

namespace PuntoVenta.Web.Infraestructura;

/// <summary>
/// Guarda el JWT en localStorage del navegador (sobrevive a recargar la página)
/// y lo mantiene en memoria para no leer localStorage en cada petición.
/// </summary>
public class AlmacenToken(IJSRuntime js)
{
    private const string Clave = "pv_token";
    private string? _token;
    private bool _cargado;

    public async Task<string?> ObtenerAsync()
    {
        if (_cargado) return _token;
        try { _token = await js.InvokeAsync<string?>("localStorage.getItem", Clave); }
        catch { _token = null; }
        _cargado = true;
        return _token;
    }

    public async Task GuardarAsync(string token)
    {
        _token = token;
        _cargado = true;
        try { await js.InvokeVoidAsync("localStorage.setItem", Clave, token); } catch { /* modo privado */ }
    }

    public async Task EliminarAsync()
    {
        _token = null;
        _cargado = true;
        try { await js.InvokeVoidAsync("localStorage.removeItem", Clave); } catch { }
    }
}
