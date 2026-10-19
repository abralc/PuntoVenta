using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Shared.Configuracion;

namespace PuntoVenta.Api.Modulos.Configuracion;

public interface IConfiguracionService
{
    Task<ConfiguracionDto> ObtenerAsync();
    Task<ConfiguracionDto> GuardarAsync(ConfiguracionDto datos);
    Task<decimal> PorcentajeGananciaAsync();
    Task<RecalculoPreciosDto> RecalcularPreciosAsync();
}

/// <summary>
/// Parámetros del sistema. La regla del enunciado es precio_venta = costo + 25 %;
/// aquí el 25 queda guardado en la tabla configuracion para que el administrador
/// pueda cambiarlo (valor por defecto: 25).
/// </summary>
public class ConfiguracionService(AppDbContext db) : IConfiguracionService
{
    public const string ClavePorcentaje = "porcentaje_ganancia";
    public const string ClaveSerie = "serie_factura";
    public const string ClaveMinutosReserva = "minutos_reserva_pos";
    public const string ClaveExistenciaMinima = "existencia_minima";

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// precio_venta = precio_costo + porcentaje. Ejemplo: 100.00 + 25 % = 125.00
    /// </summary>
    public static decimal CalcularPrecioVenta(decimal precioCosto, decimal porcentaje) =>
        Math.Round(precioCosto * (1 + porcentaje / 100m), 2, MidpointRounding.AwayFromZero);

    public async Task<ConfiguracionDto> ObtenerAsync()
    {
        var valores = await db.Parametros.AsNoTracking().ToDictionaryAsync(p => p.Clave, p => p.Valor);
        string Valor(string clave, string defecto) => valores.TryGetValue(clave, out var v) ? v : defecto;

        return new ConfiguracionDto
        {
            PorcentajeGanancia = decimal.TryParse(Valor(ClavePorcentaje, "25"), NumberStyles.Number, Inv, out var p) ? p : 25,
            SerieFactura = Valor(ClaveSerie, "A"),
            MinutosReservaPos = int.TryParse(Valor(ClaveMinutosReserva, "15"), out var m) ? m : 15,
            ExistenciaMinima = int.TryParse(Valor(ClaveExistenciaMinima, "5"), out var e) ? e : 5
        };
    }

    public async Task<decimal> PorcentajeGananciaAsync() => (await ObtenerAsync()).PorcentajeGanancia;

    public async Task<ConfiguracionDto> GuardarAsync(ConfiguracionDto datos)
    {
        await Guardar(ClavePorcentaje, datos.PorcentajeGanancia.ToString("0.##", Inv), "Porcentaje que se suma al precio de costo para calcular el precio de venta");
        await Guardar(ClaveSerie, datos.SerieFactura.Trim().ToUpperInvariant(), "Serie por defecto de las facturas de venta");
        await Guardar(ClaveMinutosReserva, datos.MinutosReservaPos.ToString(Inv), "Minutos que dura la reserva de un producto en el POS");
        await Guardar(ClaveExistenciaMinima, datos.ExistenciaMinima.ToString(Inv), "Existencia a partir de la cual se considera producto por agotarse");
        await db.SaveChangesAsync();
        return await ObtenerAsync();
    }

    /// <summary>Aplica el porcentaje actual a TODOS los productos (precio_venta = costo + %).</summary>
    public async Task<RecalculoPreciosDto> RecalcularPreciosAsync()
    {
        var porcentaje = await PorcentajeGananciaAsync();
        var factor = 1 + porcentaje / 100m;
        // SQL directo para no depender del módulo Productos
        var filas = await db.Database.ExecuteSqlAsync(
            $"UPDATE productos SET precio_venta = ROUND(precio_costo * {factor}, 2)");
        return new RecalculoPreciosDto { ProductosActualizados = filas, PorcentajeAplicado = porcentaje };
    }

    private async Task Guardar(string clave, string valor, string descripcion)
    {
        var parametro = await db.Parametros.FindAsync(clave);
        if (parametro is null)
            db.Parametros.Add(new ParametroConfiguracion { Clave = clave, Valor = valor, Descripcion = descripcion });
        else
            parametro.Valor = valor;
    }
}
