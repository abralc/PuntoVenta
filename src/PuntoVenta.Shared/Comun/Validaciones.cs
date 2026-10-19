using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PuntoVenta.Shared.Comun;

// Validaciones personalizadas. Al estar en Shared funcionan igual en Blazor
// (<DataAnnotationsValidator />) y en la API ([ApiController]).

/// <summary>
/// NIT de Guatemala con dígito verificador (módulo 11). Acepta "1234567-8", "12345678" o "CF".
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public partial class NitAttribute : ValidationAttribute
{
    public bool PermitirConsumidorFinal { get; set; } = true;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not string texto || string.IsNullOrWhiteSpace(texto))
            return ValidationResult.Success; // la obligatoriedad la controla [Required]

        if (string.Equals(texto.Trim(), "CF", StringComparison.OrdinalIgnoreCase))
            return PermitirConsumidorFinal
                ? ValidationResult.Success
                : new ValidationResult("Este registro requiere un NIT, no se permite CF.", [context.MemberName!]);

        return EsNitValido(texto)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage ?? "El NIT no es válido (revise el dígito verificador).", [context.MemberName!]);
    }

    /// <summary>Formato uniforme para guardar y comparar: "CF" o "1234567-8".</summary>
    public static string Normalizar(string nit)
    {
        var limpio = nit.Replace("-", "").Replace(" ", "").Trim().ToUpperInvariant();
        return limpio == "CF" || limpio.Length < 2 ? limpio : $"{limpio[..^1]}-{limpio[^1]}";
    }

    public static bool EsNitValido(string nit)
    {
        var limpio = nit.Replace("-", "").Replace(" ", "").ToUpperInvariant();
        if (!FormatoNit().IsMatch(limpio)) return false;

        var numero = limpio[..^1];
        var verificador = limpio[^1];

        var suma = 0;
        for (var i = 0; i < numero.Length; i++)
            suma += (numero[numero.Length - 1 - i] - '0') * (i + 2);

        var resultado = (11 - suma % 11) % 11;
        var esperado = resultado == 10 ? 'K' : (char)('0' + resultado);
        return verificador == esperado;
    }

    [GeneratedRegex(@"^\d{1,12}[0-9K]$")]
    private static partial Regex FormatoNit();
}

/// <summary>La fecha no puede ser posterior a hoy.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class FechaNoFuturaAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var fecha = Fechas.ComoDateOnly(value);
        if (fecha is null) return ValidationResult.Success;
        if (fecha.Value.Year < 1900)
            return new ValidationResult("La fecha no es válida.", [context.MemberName!]);
        return fecha.Value > DateOnly.FromDateTime(DateTime.Today)
            ? new ValidationResult(ErrorMessage ?? "La fecha no puede ser posterior a hoy.", [context.MemberName!])
            : ValidationResult.Success;
    }
}

/// <summary>Edad mínima a partir de una fecha de nacimiento.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class EdadMinimaAttribute(int anios) : ValidationAttribute
{
    public int Anios { get; } = anios;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var fecha = Fechas.ComoDateOnly(value);
        if (fecha is null) return ValidationResult.Success;
        if (fecha.Value.Year < 1900)
            return new ValidationResult("La fecha de nacimiento no es válida.", [context.MemberName!]);
        var limite = DateOnly.FromDateTime(DateTime.Today).AddYears(-Anios);
        return fecha.Value > limite
            ? new ValidationResult(ErrorMessage ?? $"Debe tener al menos {Anios} años.", [context.MemberName!])
            : ValidationResult.Success;
    }
}

internal static class Fechas
{
    public static DateOnly? ComoDateOnly(object? value) => value switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        _ => null
    };
}

/// <summary>Expresiones regulares comunes para no repetirlas en cada DTO.</summary>
public static class Patrones
{
    /// <summary>8 a 20 caracteres: dígitos, espacios, guiones, paréntesis y + inicial.</summary>
    public const string Telefono = @"^\+?[0-9\s\-()]{8,20}$";

    /// <summary>DPI de Guatemala: 13 dígitos.</summary>
    public const string Dpi = @"^\d{13}$";

    /// <summary>Texto con letras (incluye tildes y ñ), espacios, punto, apóstrofo y guion.</summary>
    public const string Nombre = @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ][A-Za-zÁÉÍÓÚÜÑáéíóúüñ .'\-]*$";
}
