using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Shared.Comun;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// Base de datos (MySQL 8.0 con Pomelo)
// ---------------------------------------------------------------------
var cadenaConexion = builder.Configuration.GetConnectionString("PuntoVenta")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:PuntoVenta en appsettings.json");

builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseMySql(cadenaConexion, new MySqlServerVersion(new Version(8, 0, 36))));

// ---------------------------------------------------------------------
// Autenticación JWT
// ---------------------------------------------------------------------
var seccionJwt = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOpciones>(seccionJwt);
var jwt = seccionJwt.Get<JwtOpciones>() ?? throw new InvalidOperationException("Falta la sección Jwt en appsettings.json");
if (jwt.Clave.Length < 32)
    throw new InvalidOperationException("Jwt:Clave debe tener al menos 32 caracteres.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        // Se usan los nombres cortos de claims tal cual vienen en el token (sub, name, role...)
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emisor,
            ValidateAudience = true,
            ValidAudience = jwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimsPv.Nombre,
            RoleClaimType = ClaimsPv.Rol
        };
    });
builder.Services.AddAuthorization();

// ---------------------------------------------------------------------
// CORS: Blazor corre en otro sitio/puerto (http://IP y la API en http://IP:5000)
// ---------------------------------------------------------------------
var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? [];
builder.Services.AddCors(opciones => opciones.AddPolicy("Blazor", politica =>
{
    if (origenes.Contains("*"))
        politica.AllowAnyOrigin();
    else
        politica.WithOrigins(origenes);
    politica.AllowAnyHeader().AllowAnyMethod();
}));

// ---------------------------------------------------------------------
// Controladores (la "C" de MVC) + formato uniforme de errores de validación
// ---------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(opciones =>
{
    opciones.InvalidModelStateResponseFactory = contexto =>
    {
        var errores = contexto.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
        return new BadRequestObjectResult(new ErrorApiDto
        {
            Mensaje = "Hay datos inválidos. Revise los campos marcados.",
            Errores = errores
        });
    };
});

// Servicios de todos los módulos (clases *Service en Modulos/*), sin editar este archivo
builder.Services.AgregarServiciosDeModulos(typeof(Program).Assembly);

// ---------------------------------------------------------------------
// Swagger con botón "Authorize" para probar endpoints protegidos
// ---------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Punto de Venta API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue aquí el token obtenido en POST /api/auth/login"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Carpeta física de imágenes de productos (wwwroot/images/productos)
Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "images", "productos"));

// CORS primero: así también las respuestas de error llevan los encabezados CORS
// y Blazor puede leer el mensaje (si no, el navegador lo bloquea).
app.UseCors("Blazor");
app.UseMiddleware<ManejoErroresMiddleware>();

// Swagger habilitado también en IIS para la demo: http://IP:5000/swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.DocumentTitle = "Punto de Venta API");

app.UseStaticFiles();          // sirve /images/productos/*.jpg
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
