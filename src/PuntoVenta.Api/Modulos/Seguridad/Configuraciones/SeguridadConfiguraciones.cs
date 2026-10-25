using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Api.Modulos.Seguridad.Entidades;

namespace PuntoVenta.Api.Modulos.Seguridad.Configuraciones;

// Mapeo de las entidades a las tablas reales de MySQL (database/01_esquema.sql).

public class RolConfiguracion : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> e)
    {
        e.ToTable("roles");
        e.HasKey(x => x.IdRol);
        e.Property(x => x.IdRol).HasColumnName("id_rol");
        e.Property(x => x.Nombre).HasColumnName("rol").HasMaxLength(30).IsRequired();
        e.HasIndex(x => x.Nombre).IsUnique();
    }
}

public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> e)
    {
        e.ToTable("usuarios");
        e.HasKey(x => x.IdUsuario);
        e.Property(x => x.IdUsuario).HasColumnName("id_usuario");
        e.Property(x => x.NombreUsuario).HasColumnName("usuario").HasMaxLength(30).IsRequired();
        e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(100).IsRequired();
        e.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
        e.Property(x => x.IdRol).HasColumnName("id_rol");
        e.Property(x => x.Activo).HasColumnName("activo").HasColumnType("bit(1)");
        e.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso");
        e.HasIndex(x => x.NombreUsuario).IsUnique();
        e.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.IdRol).OnDelete(DeleteBehavior.Restrict);
    }
}

public class MenuOpcionConfiguracion : IEntityTypeConfiguration<MenuOpcion>
{
    public void Configure(EntityTypeBuilder<MenuOpcion> e)
    {
        e.ToTable("menu");
        e.HasKey(x => x.IdMenu);
        e.Property(x => x.IdMenu).HasColumnName("id_menu");
        e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
        e.Property(x => x.Url).HasColumnName("url").HasMaxLength(100);
        e.Property(x => x.Icono).HasColumnName("icono").HasMaxLength(50);
        e.Property(x => x.IdPadre).HasColumnName("id_padre");
        e.Property(x => x.Orden).HasColumnName("orden");
        e.Property(x => x.Activo).HasColumnName("activo").HasColumnType("bit(1)");
        e.HasOne(x => x.Padre).WithMany(x => x.Hijos).HasForeignKey(x => x.IdPadre).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RolMenuConfiguracion : IEntityTypeConfiguration<RolMenu>
{
    public void Configure(EntityTypeBuilder<RolMenu> e)
    {
        e.ToTable("roles_menu");
        e.HasKey(x => new { x.IdRol, x.IdMenu });
        e.Property(x => x.IdRol).HasColumnName("id_rol");
        e.Property(x => x.IdMenu).HasColumnName("id_menu");
        e.HasOne(x => x.Rol).WithMany(x => x.Menus).HasForeignKey(x => x.IdRol);
        e.HasOne(x => x.Menu).WithMany().HasForeignKey(x => x.IdMenu);
    }
}
