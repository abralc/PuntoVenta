using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Modulos.Productos.Entidades;

namespace PuntoVenta.Api.Data;

public partial class AppDbContext
{
    public DbSet<Producto> Productos => Set<Producto>();
}
