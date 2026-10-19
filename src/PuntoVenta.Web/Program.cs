using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PuntoVenta.Web;
using PuntoVenta.Web.Infraestructura;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Dirección de la API: wwwroot/appsettings.json (servidor) o appsettings.Development.json (local).
// Nunca "localhost" en el servidor: las otras máquinas no lo encontrarían.
var urlApi = builder.Configuration["ApiUrl"]
    ?? throw new InvalidOperationException("Falta ApiUrl en wwwroot/appsettings.json");
if (!urlApi.EndsWith('/')) urlApi += "/";

// Autenticación JWT
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AlmacenToken>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

// HttpClient hacia la API con el token agregado automáticamente
builder.Services.AddScoped(sp => new HttpClient(
    new AuthHeaderHandler(sp.GetRequiredService<AlmacenToken>()) { InnerHandler = new HttpClientHandler() })
{
    BaseAddress = new Uri(urlApi)
});

builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<NotificacionService>();
builder.Services.AddScoped<ConfirmacionService>();
builder.Services.AddScoped<Navegador>();

await builder.Build().RunAsync();
