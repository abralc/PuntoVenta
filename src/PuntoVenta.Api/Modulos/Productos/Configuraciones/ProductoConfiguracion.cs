using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Api.Modulos.Productos.Entidades;

namespace PuntoVenta.Api.Modulos.Productos.Configuraciones;

public class ProductoConfiguracion : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> e)
    {
        e.ToTable("productos");
        e.HasKey(x => x.IdProducto);
        e.Property(x => x.IdProducto).HasColumnName("id_producto");
        e.Property(x => x.Nombre).HasColumnName("producto").HasMaxLength(50).IsRequired();
        e.Property(x => x.IdMarca).HasColumnName("id_marca");
        e.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        e.Property(x => x.Imagen).HasColumnName("imagen").HasMaxLength(255);
        e.Property(x => x.PrecioCosto).HasColumnName("precio_costo").HasPrecision(8, 2);
        e.Property(x => x.PrecioVenta).HasColumnName("precio_venta").HasPrecision(8, 2);
        e.Property(x => x.Existencia).HasColumnName("existencia");
        e.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso");
        e.HasIndex(x => new { x.Nombre, x.IdMarca }).IsUnique();

        e.HasOne(x => x.Marca).WithMany().HasForeignKey(x => x.IdMarca).OnDelete(DeleteBehavior.Restrict);
    }
}
