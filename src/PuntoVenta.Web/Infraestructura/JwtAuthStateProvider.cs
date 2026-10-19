using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace PuntoVenta.Web.Infraestructura;

/// <summary>
/// Le dice a Blazor quién es el usuario actual leyendo los claims del JWT.
/// Si no hay token o ya expiró, el usuario es anónimo y las páginas con
/// [Authorize] redirigen al login.
/// </summary>
public class JwtAuthStateProvider(AlmacenToken almacen) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonimo = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await almacen.ObtenerAsync();
        if (string.IsNullOrWhiteSpace(token)) return Anonimo;

        var claims = LeerClaims(token);
        if (claims is null || TokenExpirado(claims))
        {
            await almacen.EliminarAsync();
            return Anonimo;
        }

        var identidad = new ClaimsIdentity(claims, "jwt", nameType: "name", roleType: "role");
        return new AuthenticationState(new ClaimsPrincipal(identidad));
    }

    public async Task IniciarSesionAsync(string token)
    {
        await almacen.GuardarAsync(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task CerrarSesionAsync()
    {
        await almacen.EliminarAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));
    }

    private static bool TokenExpirado(IEnumerable<Claim> claims)
    {
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        return !long.TryParse(exp, out var segundos)
               || DateTimeOffset.FromUnixTimeSeconds(segundos) <= DateTimeOffset.UtcNow;
    }

    /// <summary>Decodifica la parte "payload" del JWT (base64url → JSON → claims).</summary>
    private static List<Claim>? LeerClaims(string token)
    {
        try
        {
            var partes = token.Split('.');
            if (partes.Length != 3) return null;

            var base64 = partes[1].Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

            using var doc = JsonDocument.Parse(json);
            var claims = new List<Claim>();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Array)
                    claims.AddRange(prop.Value.EnumerateArray().Select(v => new Claim(prop.Name, v.ToString())));
                else
                    claims.Add(new Claim(prop.Name, prop.Value.ToString()));
            }
            return claims;
        }
        catch
        {
            return null;
        }
    }
}
