using System.Net.Http.Headers;

namespace PuntoVenta.Web.Infraestructura;

/// <summary>Agrega "Authorization: Bearer {token}" a todas las peticiones hacia la API.</summary>
public class AuthHeaderHandler(AlmacenToken almacen) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await almacen.ObtenerAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
