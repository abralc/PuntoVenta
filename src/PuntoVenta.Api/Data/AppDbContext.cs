using Microsoft.EntityFrameworkCore;

namespace PuntoVenta.Api.Data;

/// <summary>
/// DbContext PARCIAL. Cada módulo agrega sus DbSet en su propio archivo
/// (por ejemplo Modulos/Ventas/AppDbContext.Ventas.cs) para que nadie
/// tenga que editar este archivo y no haya conflictos en Git.
///
/// Las configuraciones de tablas (IEntityTypeConfiguration) de todos los
/// módulos se cargan automáticamente con ApplyConfigurationsFromAssembly.
/// </summary>
public partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
