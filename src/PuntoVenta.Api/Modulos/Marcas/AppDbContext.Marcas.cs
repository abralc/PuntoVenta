using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Modulos.Marcas.Entidades;

namespace PuntoVenta.Api.Data;

public partial class AppDbContext
{
    public DbSet<Marca> Marcas => Set<Marca>();
}
