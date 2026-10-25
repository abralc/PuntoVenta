using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Api.Modulos.Compras.Entidades;

namespace PuntoVenta.Api.Modulos.Compras.Configuraciones;

public class CompraConfiguracion : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> e)
    {
        e.ToTable("compras");
        e.HasKey(x => x.IdCompra);
        e.Property(x => x.IdCompra).HasColumnName("id_compra");
        e.Property(x => x.NoOrdenCompra).HasColumnName("no_orden_compra");
        e.Property(x => x.IdProveedor).HasColumnName("id_proveedor");
        e.Property(x => x.FechaOrden).HasColumnName("fecha_orden");
        e.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso");
        e.HasIndex(x => new { x.IdProveedor, x.NoOrdenCompra }).IsUnique();

        e.HasOne(x => x.Proveedor).WithMany().HasForeignKey(x => x.IdProveedor).OnDelete(DeleteBehavior.Restrict);
        e.HasMany(x => x.Detalles).WithOne(d => d.Compra).HasForeignKey(d => d.IdCompra).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CompraDetalleConfiguracion : IEntityTypeConfiguration<CompraDetalle>
{
    public void Configure(EntityTypeBuilder<CompraDetalle> e)
    {
        e.ToTable("compras_detalle");
        e.HasKey(x => x.IdCompraDetalle);
        e.Property(x => x.IdCompraDetalle).HasColumnName("id_compra_detalle");
        e.Property(x => x.IdCompra).HasColumnName("id_compra");
        e.Property(x => x.IdProducto).HasColumnName("id_producto");
        e.Property(x => x.Cantidad).HasColumnName("cantidad");
        e.Property(x => x.PrecioCostoUnitario).HasColumnName("precio_costo_unitario").HasPrecision(8, 2);

        e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.IdProducto).OnDelete(DeleteBehavior.Restrict);
    }
}
