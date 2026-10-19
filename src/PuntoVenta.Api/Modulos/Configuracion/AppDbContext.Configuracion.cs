using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Modulos.Configuracion;

namespace PuntoVenta.Api.Data;

public partial class AppDbContext
{
    public DbSet<ParametroConfiguracion> Parametros => Set<ParametroConfiguracion>();
}
