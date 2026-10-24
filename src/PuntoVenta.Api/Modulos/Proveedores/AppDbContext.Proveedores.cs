using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Modulos.Proveedores.Entidades;

namespace PuntoVenta.Api.Data;

public partial class AppDbContext
{
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
}
