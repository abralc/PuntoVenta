using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Api.Modulos.Seguridad;

public interface IMenuService
{
    Task<List<MenuItemDto>> ObtenerMenuPorRolAsync(string rol);
}

/// <summary>
/// Construye el árbol del menú dinámico (padre → hijos) a partir de la tabla menu,
/// filtrado por las opciones que tiene asignadas el rol en roles_menu.
/// </summary>
public class MenuService(AppDbContext db) : IMenuService
{
    public async Task<List<MenuItemDto>> ObtenerMenuPorRolAsync(string rol)
    {
        var opciones = await db.RolesMenu
            .Where(rm => rm.Rol!.Nombre == rol && rm.Menu!.Activo)
            .Select(rm => rm.Menu!)
            .AsNoTracking()
            .ToListAsync();

        var ids = opciones.Select(o => o.IdMenu).ToHashSet();
        var porPadre = opciones.ToLookup(o => o.IdPadre);

        List<MenuItemDto> Construir(int? idPadre) => porPadre[idPadre]
            .OrderBy(o => o.Orden).ThenBy(o => o.Nombre)
            .Select(o => new MenuItemDto
            {
                Id = o.IdMenu,
                Nombre = o.Nombre,
                Url = o.Url,
                Icono = o.Icono,
                Orden = o.Orden,
                Hijos = Construir(o.IdMenu)
            })
            .ToList();

        // Raíces: opciones sin padre, o cuyo padre no está asignado al rol
        var raices = Construir(null);
        raices.AddRange(opciones
            .Where(o => o.IdPadre is int p && !ids.Contains(p))
            .OrderBy(o => o.Orden)
            .Select(o => new MenuItemDto
            {
                Id = o.IdMenu, Nombre = o.Nombre, Url = o.Url, Icono = o.Icono, Orden = o.Orden,
                Hijos = Construir(o.IdMenu)
            }));

        return raices;
    }
}
