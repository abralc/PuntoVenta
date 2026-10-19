using System.Text;
using Microsoft.JSInterop;

namespace PuntoVenta.Web.Infraestructura;

/// <summary>Funciones del navegador (wwwroot/js/app.js): descargar archivos, imprimir, sessionStorage.</summary>
public class Navegador(IJSRuntime js)
{
    /// <summary>Descarga un CSV que Excel abre con tildes correctas (UTF-8 con BOM).</summary>
    public ValueTask DescargarCsvAsync(string nombreArchivo, IEnumerable<IEnumerable<object?>> filas)
    {
        var sb = new StringBuilder("﻿");
        foreach (var fila in filas)
            sb.AppendLine(string.Join(",", fila.Select(Fmt.Csv)));
        return js.InvokeVoidAsync("pv.descargar", nombreArchivo, sb.ToString(), "text/csv;charset=utf-8");
    }

    public ValueTask ImprimirAsync() => js.InvokeVoidAsync("pv.imprimir");

    /// <summary>Imprime solo el ticket de 80 mm (elemento .pv-imprimible) en la impresora térmica.</summary>
    public ValueTask ImprimirTicketAsync() => js.InvokeVoidAsync("pv.imprimirTicket");

    public async ValueTask<string?> LeerSesionAsync(string clave)
    {
        try { return await js.InvokeAsync<string?>("pv.sesion.leer", clave); }
        catch { return null; }
    }

    public async ValueTask GuardarSesionAsync(string clave, string valor)
    {
        try { await js.InvokeVoidAsync("pv.sesion.guardar", clave, valor); } catch { }
    }

    public async ValueTask EliminarSesionAsync(string clave)
    {
        try { await js.InvokeVoidAsync("pv.sesion.eliminar", clave); } catch { }
    }

    public ValueTask EnfocarAsync(string idElemento) => js.InvokeVoidAsync("pv.enfocar", idElemento);
}
