using System.Reflection;

namespace PuntoVenta.Api.Infraestructura;

/// <summary>
/// Registra AUTOMÁTICAMENTE los servicios de todos los módulos.
///
/// Convención: toda clase pública dentro de PuntoVenta.Api.Modulos.* cuyo
/// nombre termine en "Service" se registra como Scoped. Si implementa la
/// interfaz I{NombreClase} (ej. VentaService : IVentaService), se registra
/// con esa interfaz.
///
/// Así ningún integrante necesita editar Program.cs para agregar su módulo.
/// </summary>
public static class RegistroServicios
{
    public static IServiceCollection AgregarServiciosDeModulos(this IServiceCollection services, Assembly ensamblado)
    {
        var tipos = ensamblado.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true }
                        && t.Name.EndsWith("Service", StringComparison.Ordinal)
                        && t.Namespace is not null
                        && t.Namespace.StartsWith("PuntoVenta.Api.Modulos", StringComparison.Ordinal));

        foreach (var tipo in tipos)
        {
            var interfaz = tipo.GetInterfaces().FirstOrDefault(i => i.Name == "I" + tipo.Name);
            if (interfaz is not null)
                services.AddScoped(interfaz, tipo);
            else
                services.AddScoped(tipo);
        }

        return services;
    }
}
