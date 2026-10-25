using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Modulos.Seguridad.Entidades;

// Parte del DbContext que pertenece al módulo de Seguridad.
// Mismo namespace que Data/AppDbContext.cs para que sea la misma clase parcial.
namespace PuntoVenta.Api.Data;

public partial class AppDbContext
{
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<MenuOpcion> Menu => Set<MenuOpcion>();
    public DbSet<RolMenu> RolesMenu => Set<RolMenu>();
}
