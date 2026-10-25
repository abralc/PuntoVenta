using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Modulos.Empleados.Entidades;

namespace PuntoVenta.Api.Data;

public partial class AppDbContext
{
    public DbSet<Empleado> Empleados => Set<Empleado>();
}
