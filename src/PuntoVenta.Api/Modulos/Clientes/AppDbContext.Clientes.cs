using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Modulos.Clientes.Entidades;

namespace PuntoVenta.Api.Data;

public partial class AppDbContext
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
}
