// Funciones de apoyo que Blazor llama con IJSRuntime (ver Infraestructura/Navegador.cs)
window.pv = {
    descargar: function (nombre, contenido, tipo) {
        const blob = new Blob([contenido], { type: tipo });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = nombre;
        document.body.appendChild(a);
        a.click();
        a.remove();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    },
    imprimir: function () {
        window.print();
    },
    // Imprime SOLO el ticket (.pv-imprimible) en un iframe con tamaño de 80 mm.
    // Así el navegador no reduce el ticket para que quepa toda la página.
    imprimirTicket: function () {
        const ticket = document.querySelector('.pv-imprimible');
        if (!ticket) { window.print(); return; }

        const estilos = `
            @page { size: 80mm auto; margin: 0; }
            * { box-sizing: border-box; }
            html, body { margin: 0; padding: 0; background: #fff; }
            body { width: 72mm; padding: 3mm 2mm 6mm 4mm; color: #000;
                   font-family: "Consolas", "Courier New", monospace; font-size: 12px; line-height: 1.35; }
            .pv-ticket { width: 100%; border: 0; padding: 0; margin: 0; max-width: none; display: block !important; }
            .pv-ticket-titulo { font-weight: 700; font-size: 15px; }
            .pv-ticket-total { font-weight: 700; font-size: 15px; }
            hr { border: 0; border-top: 1px dashed #000; margin: 6px 0; }
            table { width: 100%; border-collapse: collapse; }
            th { font-weight: 700; border-bottom: 1px dashed #000; padding-bottom: 2px; }
            td { padding: 2px 0; vertical-align: top; }
            small { font-size: 11px; }
            strong { font-weight: 700; }
            .d-flex { display: flex; } .justify-content-between { justify-content: space-between; }
            .text-center { text-align: center; } .text-end { text-align: right; } .text-start { text-align: left; }
            .w-100 { width: 100%; } .align-top { vertical-align: top; }`;

        const marco = document.createElement('iframe');
        marco.setAttribute('aria-hidden', 'true');
        marco.style.cssText = 'position:fixed;right:0;bottom:0;width:0;height:0;border:0;visibility:hidden;';
        document.body.appendChild(marco);

        const doc = marco.contentWindow.document;
        doc.open();
        doc.write('<!doctype html><html><head><meta charset="utf-8"><title>Ticket</title><style>' + estilos +
                  '</style></head><body>' + ticket.outerHTML + '</body></html>');
        doc.close();

        setTimeout(function () {
            marco.contentWindow.focus();
            marco.contentWindow.print();
            setTimeout(function () { marco.remove(); }, 2000);
        }, 150);
    },
    enfocar: function (id) {
        const el = document.getElementById(id);
        if (el) { el.focus(); if (el.select) el.select(); }
    },
    sesion: {
        leer: (k) => { try { return sessionStorage.getItem(k); } catch { return null; } },
        guardar: (k, v) => { try { sessionStorage.setItem(k, v); } catch { } },
        eliminar: (k) => { try { sessionStorage.removeItem(k); } catch { } }
    }
};
