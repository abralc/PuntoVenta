using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Api.Modulos.Clientes.Entidades;

namespace PuntoVenta.Api.Modulos.Clientes.Configuraciones;

public class ClienteConfiguracion : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> e)
    {
        e.ToTable("clientes");
        e.HasKey(x => x.IdCliente);
        e.Property(x => x.IdCliente).HasColumnName("id_cliente");
        e.Property(x => x.Nombres).HasColumnName("nombres").HasMaxLength(60).IsRequired();
        e.Property(x => x.Apellidos).HasColumnName("apellidos").HasMaxLength(60).IsRequired();
        e.Property(x => x.Nit).HasColumnName("nit").HasMaxLength(12).IsRequired();
        e.Property(x => x.Genero).HasColumnName("genero").HasColumnType("bit(1)");
        e.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(25);
        e.Property(x => x.CorreoElectronico).HasColumnName("correo_electronico").HasMaxLength(45);
        e.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso");
        e.HasIndex(x => x.Nit);
    }
}
