using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PuntoVenta.Api.Modulos.Configuracion;

/// <summary>Modelo de la tabla configuracion (clave / valor).</summary>
public class ParametroConfiguracion
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class ParametroConfiguracionConfiguracion : IEntityTypeConfiguration<ParametroConfiguracion>
{
    public void Configure(EntityTypeBuilder<ParametroConfiguracion> e)
    {
        e.ToTable("configuracion");
        e.HasKey(x => x.Clave);
        e.Property(x => x.Clave).HasColumnName("clave").HasMaxLength(50);
        e.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(100).IsRequired();
        e.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(150);
    }
}
