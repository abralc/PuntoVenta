using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Modulos.Ventas.Entidades;

namespace PuntoVenta.Api.Data;

public partial class AppDbContext
{
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentasDetalle => Set<VentaDetalle>();
}
