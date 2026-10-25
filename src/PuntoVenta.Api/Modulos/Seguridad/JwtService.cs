using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Seguridad.Entidades;

namespace PuntoVenta.Api.Modulos.Seguridad;

public interface IJwtService
{
    (string Token, DateTime Expira) GenerarToken(Usuario usuario, string rol, string nombre);
}

/// <summary>Genera el JSON Web Token firmado (HS256) que recibe Blazor al iniciar sesión.</summary>
public class JwtService(IOptions<JwtOpciones> opciones) : IJwtService
{
    private readonly JwtOpciones _jwt = opciones.Value;

    public (string Token, DateTime Expira) GenerarToken(Usuario usuario, string rol, string nombre)
    {
        var expira = DateTime.UtcNow.AddMinutes(_jwt.MinutosExpiracion);

        var claims = new List<Claim>
        {
            new(ClaimsPv.IdUsuario, usuario.IdUsuario.ToString()),
            new(ClaimsPv.Usuario, usuario.NombreUsuario),
            new(ClaimsPv.Nombre, nombre),
            new(ClaimsPv.Rol, rol)
        };
        if (usuario.IdEmpleado is int idEmpleado)
            claims.Add(new Claim(ClaimsPv.IdEmpleado, idEmpleado.ToString()));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwt.Emisor,
            Audience = _jwt.Audiencia,
            Expires = expira,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Clave)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expira);
    }
}
