using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Modulos.Puestos.Entidades;

namespace PuntoVenta.Api.Data;

public partial class AppDbContext
{
    public DbSet<Puesto> Puestos => Set<Puesto>();
}
