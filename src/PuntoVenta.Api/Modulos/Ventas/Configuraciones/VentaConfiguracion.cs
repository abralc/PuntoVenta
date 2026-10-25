using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Api.Modulos.Ventas.Entidades;

namespace PuntoVenta.Api.Modulos.Ventas.Configuraciones;

public class VentaConfiguracion : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> e)
    {
        e.ToTable("ventas");
        e.HasKey(x => x.IdVenta);
        e.Property(x => x.IdVenta).HasColumnName("id_venta");
        e.Property(x => x.NoFactura).HasColumnName("no_factura");
        e.Property(x => x.Serie).HasColumnName("serie").HasMaxLength(1).IsFixedLength().IsRequired();
        e.Property(x => x.FechaFactura).HasColumnName("fecha_factura");
        e.Property(x => x.IdCliente).HasColumnName("id_cliente");
        e.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
        e.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso");
        e.HasIndex(x => new { x.Serie, x.NoFactura }).IsUnique();

        e.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.Restrict);
        e.HasMany(x => x.Detalles).WithOne(d => d.Venta).HasForeignKey(d => d.IdVenta).OnDelete(DeleteBehavior.Cascade);
    }
}

public class VentaDetalleConfiguracion : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> e)
    {
        e.ToTable("ventas_detalle");
        e.HasKey(x => x.IdVentaDetalle);
        e.Property(x => x.IdVentaDetalle).HasColumnName("id_venta_detalle");
        e.Property(x => x.IdVenta).HasColumnName("id_venta");
        e.Property(x => x.IdProducto).HasColumnName("id_producto");
        e.Property(x => x.Cantidad).HasColumnName("cantidad");
        e.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(8, 2);

        e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.IdProducto).OnDelete(DeleteBehavior.Restrict);
    }
}
