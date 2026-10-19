using System.Globalization;

namespace PuntoVenta.Web.Infraestructura;

/// <summary>Formatos de presentación: moneda en quetzales y fechas dd/MM/yyyy.</summary>
public static class Fmt
{
    private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;

    public static string Q(decimal valor) => "Q " + valor.ToString("N2", Cultura);
    public static string Q(decimal? valor) => valor is null ? "-" : Q(valor.Value);

    public static string Fecha(DateOnly? fecha) => fecha?.ToString("dd/MM/yyyy", Cultura) ?? "-";
    public static string Fecha(DateTime? fecha) => fecha?.ToString("dd/MM/yyyy", Cultura) ?? "-";
    public static string FechaHora(DateTime? fecha) => fecha?.ToString("dd/MM/yyyy HH:mm", Cultura) ?? "-";

    public static string Numero(int valor) => valor.ToString("N0", Cultura);

    /// <summary>Fecha para parámetros de URL (yyyy-MM-dd).</summary>
    public static string Iso(DateOnly fecha) => fecha.ToString("yyyy-MM-dd", Cultura);

    /// <summary>Escapa un valor para CSV (comillas si trae coma, comillas o salto de línea).</summary>
    public static string Csv(object? valor)
    {
        var texto = valor switch
        {
            null => "",
            decimal d => d.ToString("0.00", Cultura),
            DateOnly f => Fecha(f),
            DateTime f => FechaHora(f),
            _ => Convert.ToString(valor, Cultura) ?? ""
        };
        return texto.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? "\"" + texto.Replace("\"", "\"\"") + "\"" : texto;
    }
}
