using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Api.Modulos.Puestos.Entidades;

namespace PuntoVenta.Api.Modulos.Puestos.Configuraciones;

public class PuestoConfiguracion : IEntityTypeConfiguration<Puesto>
{
    public void Configure(EntityTypeBuilder<Puesto> e)
    {
        e.ToTable("puestos");
        e.HasKey(x => x.IdPuesto);
        e.Property(x => x.IdPuesto).HasColumnName("id_puesto");
        e.Property(x => x.Nombre).HasColumnName("puesto").HasMaxLength(50).IsRequired();
        e.HasIndex(x => x.Nombre).IsUnique();
    }
}
