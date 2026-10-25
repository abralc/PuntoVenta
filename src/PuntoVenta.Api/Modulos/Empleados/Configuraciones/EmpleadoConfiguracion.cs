using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Api.Modulos.Empleados.Entidades;

namespace PuntoVenta.Api.Modulos.Empleados.Configuraciones;

public class EmpleadoConfiguracion : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> e)
    {
        e.ToTable("empleados");
        e.HasKey(x => x.IdEmpleado);
        e.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
        e.Property(x => x.Nombres).HasColumnName("nombres").HasMaxLength(60).IsRequired();
        e.Property(x => x.Apellidos).HasColumnName("apellidos").HasMaxLength(60).IsRequired();
        e.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(80).IsRequired();
        e.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(25).IsRequired();
        e.Property(x => x.Dpi).HasColumnName("dpi").HasMaxLength(15).IsRequired();
        e.Property(x => x.Genero).HasColumnName("genero").HasColumnType("bit(1)");
        e.Property(x => x.FechaNacimiento).HasColumnName("fecha_nacimiento");
        e.Property(x => x.IdPuesto).HasColumnName("id_puesto");
        e.Property(x => x.FechaInicioLabores).HasColumnName("fecha_inicio_labores");
        e.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso");
        e.HasIndex(x => x.Dpi).IsUnique();

        e.HasOne(x => x.Puesto).WithMany().HasForeignKey(x => x.IdPuesto).OnDelete(DeleteBehavior.Restrict);
    }
}
