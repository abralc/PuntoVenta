using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Api.Modulos.Marcas.Entidades;

namespace PuntoVenta.Api.Modulos.Marcas.Configuraciones;

public class MarcaConfiguracion : IEntityTypeConfiguration<Marca>
{
    public void Configure(EntityTypeBuilder<Marca> e)
    {
        e.ToTable("marcas");
        e.HasKey(x => x.IdMarca);
        e.Property(x => x.IdMarca).HasColumnName("id_marca");
        e.Property(x => x.Nombre).HasColumnName("marca").HasMaxLength(50).IsRequired();
        e.HasIndex(x => x.Nombre).IsUnique();
    }
}
