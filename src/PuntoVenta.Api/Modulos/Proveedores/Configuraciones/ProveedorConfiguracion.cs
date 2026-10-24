using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Api.Modulos.Proveedores.Entidades;

namespace PuntoVenta.Api.Modulos.Proveedores.Configuraciones;

public class ProveedorConfiguracion : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> e)
    {
        e.ToTable("proveedores");
        e.HasKey(x => x.IdProveedor);
        e.Property(x => x.IdProveedor).HasColumnName("id_proveedor");
        e.Property(x => x.Nombre).HasColumnName("proveedor").HasMaxLength(60).IsRequired();
        e.Property(x => x.Nit).HasColumnName("nit").HasMaxLength(12).IsRequired();
        e.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(80).IsRequired();
        e.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(25).IsRequired();
        e.HasIndex(x => x.Nit).IsUnique();
    }
}
