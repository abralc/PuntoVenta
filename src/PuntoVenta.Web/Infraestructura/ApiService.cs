using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Web.Infraestructura;

/// <summary>Error devuelto por la API, con el mensaje y los errores por campo.</summary>
public class ApiException(string mensaje, HttpStatusCode codigo, Dictionary<string, string[]>? errores = null)
    : Exception(mensaje)
{
    public HttpStatusCode Codigo { get; } = codigo;
    public Dictionary<string, string[]>? Errores { get; } = errores;
}

/// <summary>
/// Cliente genérico de la API REST. Todas las pantallas lo usan:
///   var lista = await Api.GetAsync&lt;List&lt;PuestoDto&gt;&gt;("api/puestos");
///   await Api.PostAsync("api/puestos", dto);
///   await Api.PutAsync($"api/puestos/{id}", dto);
///   await Api.DeleteAsync($"api/puestos/{id}");
/// Si la API responde 401 (token vencido), cierra la sesión y manda al login.
/// </summary>
public class ApiService(HttpClient http, JwtAuthStateProvider auth, NavigationManager nav)
{
    /// <summary>Dirección base de la API, ej. http://10.0.0.4:5000/</summary>
    public string UrlBase => http.BaseAddress?.ToString() ?? string.Empty;

    public async Task<T?> GetAsync<T>(string url)
    {
        var respuesta = await Enviar(() => http.GetAsync(url));
        return await respuesta.Content.ReadFromJsonAsync<T>();
    }

    public async Task PostAsync<TBody>(string url, TBody cuerpo) =>
        await Enviar(() => http.PostAsJsonAsync(url, cuerpo));

    public async Task<TResp?> PostAsync<TBody, TResp>(string url, TBody cuerpo)
    {
        var respuesta = await Enviar(() => http.PostAsJsonAsync(url, cuerpo));
        return await respuesta.Content.ReadFromJsonAsync<TResp>();
    }

    public async Task PutAsync<TBody>(string url, TBody cuerpo) =>
        await Enviar(() => http.PutAsJsonAsync(url, cuerpo));

    public async Task<TResp?> PutAsync<TBody, TResp>(string url, TBody cuerpo)
    {
        var respuesta = await Enviar(() => http.PutAsJsonAsync(url, cuerpo));
        return await respuesta.Content.ReadFromJsonAsync<TResp>();
    }

    public async Task DeleteAsync(string url) =>
        await Enviar(() => http.DeleteAsync(url));

    /// <summary>
    /// POST para endpoints públicos (ej. login): un 401 aquí significa credenciales
    /// incorrectas, no sesión expirada, así que NO redirige al login.
    /// </summary>
    public async Task<TResp?> PostPublicoAsync<TBody, TResp>(string url, TBody cuerpo)
    {
        var respuesta = await Enviar(() => http.PostAsJsonAsync(url, cuerpo), redirigirSi401: false);
        return await respuesta.Content.ReadFromJsonAsync<TResp>();
    }

    /// <summary>Envío de archivos (ej. imagen del producto) como multipart/form-data.</summary>
    public async Task<TResp?> PostFormAsync<TResp>(string url, MultipartFormDataContent contenido)
    {
        var respuesta = await Enviar(() => http.PostAsync(url, contenido));
        return await respuesta.Content.ReadFromJsonAsync<TResp>();
    }

    /// <summary>
    /// Convierte la ruta guardada en BD (/images/productos/x.jpg) en URL completa
    /// del servidor de la API. Si no hay imagen, devuelve una imagen por defecto.
    /// </summary>
    public string UrlImagen(string? ruta) =>
        string.IsNullOrWhiteSpace(ruta)
            ? "img/sin-imagen.svg"
            : new Uri(new Uri(UrlBase), ruta.TrimStart('/')).ToString();

    private async Task<HttpResponseMessage> Enviar(Func<Task<HttpResponseMessage>> peticion, bool redirigirSi401 = true)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await peticion();
        }
        catch (HttpRequestException)
        {
            throw new ApiException(
                $"No se pudo conectar con la API ({UrlBase}). Verifique que el servidor esté encendido.",
                HttpStatusCode.ServiceUnavailable);
        }

        if (respuesta.IsSuccessStatusCode) return respuesta;

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized && redirigirSi401)
        {
            await auth.CerrarSesionAsync();
            nav.NavigateTo($"login?volver={Uri.EscapeDataString(nav.ToBaseRelativePath(nav.Uri))}");
            throw new ApiException("Su sesión expiró. Inicie sesión nuevamente.", respuesta.StatusCode);
        }

        ErrorApiDto? error = null;
        try { error = await respuesta.Content.ReadFromJsonAsync<ErrorApiDto>(); } catch { }

        var mensaje = error?.Mensaje is { Length: > 0 } m
            ? m
            : respuesta.StatusCode switch
            {
                HttpStatusCode.Forbidden => "No tiene permisos para realizar esta acción.",
                HttpStatusCode.NotFound => "El registro no existe.",
                _ => $"Error {(int)respuesta.StatusCode} al comunicarse con la API."
            };

        throw new ApiException(mensaje, respuesta.StatusCode, error?.Errores);
    }
}
