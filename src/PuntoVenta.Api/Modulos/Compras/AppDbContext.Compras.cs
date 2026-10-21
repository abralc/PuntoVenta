using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Modulos.Compras.Entidades;

namespace PuntoVenta.Api.Data;

public partial class AppDbContext
{
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<CompraDetalle> ComprasDetalle => Set<CompraDetalle>();
}
