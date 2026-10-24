using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Infraestructura;
using PuntoVenta.Api.Modulos.Clientes.Entidades;
using PuntoVenta.Shared.Clientes;
using PuntoVenta.Shared.Comun;

namespace PuntoVenta.Api.Modulos.Clientes;

public interface IClienteService
{
    Task<List<ClienteDto>> ListarAsync(string? buscar);
    Task<ClienteDto> ObtenerAsync(int id);
    Task<List<OpcionDto>> OpcionesAsync();
    Task<ClienteDto> CrearAsync(ClienteDto datos);
    Task<ClienteDto> ActualizarAsync(int id, ClienteDto datos);
    Task EliminarAsync(int id);
}

public class ClienteService(AppDbContext db) : IClienteService
{
    public async Task<List<ClienteDto>> ListarAsync(string? buscar)
    {
        var consulta = db.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            consulta = consulta.Where(c => c.Nombres.Contains(t) || c.Apellidos.Contains(t) || c.Nit.Contains(t)
                                           || (c.Telefono != null && c.Telefono.Contains(t))
                                           || (c.CorreoElectronico != null && c.CorreoElectronico.Contains(t)));
        }
        return await consulta.OrderBy(c => c.Nombres).ThenBy(c => c.Apellidos).Select(c => new ClienteDto
        {
            IdCliente = c.IdCliente,
            Nombres = c.Nombres,
            Apellidos = c.Apellidos,
            Nit = c.Nit,
            Genero = c.Genero,
            Telefono = c.Telefono,
            CorreoElectronico = c.CorreoElectronico,
            FechaIngreso = c.FechaIngreso
        }).ToListAsync();
    }

    public async Task<ClienteDto> ObtenerAsync(int id)
    {
        var c = await db.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.IdCliente == id)
                ?? throw new NoEncontradoException("El cliente no existe.");
        return new ClienteDto
        {
            IdCliente = c.IdCliente, Nombres = c.Nombres, Apellidos = c.Apellidos, Nit = c.Nit, Genero = c.Genero,
            Telefono = c.Telefono, CorreoElectronico = c.CorreoElectronico, FechaIngreso = c.FechaIngreso
        };
    }

    public async Task<List<OpcionDto>> OpcionesAsync() =>
        await db.Clientes.AsNoTracking()
            .OrderBy(c => c.Nit == "CF" ? 0 : 1).ThenBy(c => c.Nombres)
            .Select(c => new OpcionDto { Id = c.IdCliente, Texto = c.Nit + " · " + c.Nombres + " " + c.Apellidos })
            .ToListAsync();

    public async Task<ClienteDto> CrearAsync(ClienteDto datos)
    {
        var cliente = new Cliente { FechaIngreso = DateTime.Now };
        await Copiar(datos, cliente, null);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
        return await ObtenerAsync(cliente.IdCliente);
    }

    public async Task<ClienteDto> ActualizarAsync(int id, ClienteDto datos)
    {
        var cliente = await db.Clientes.FindAsync(id) ?? throw new NoEncontradoException("El cliente no existe.");
        await Copiar(datos, cliente, id);
        await db.SaveChangesAsync();
        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(int id)
    {
        var cliente = await db.Clientes.FindAsync(id) ?? throw new NoEncontradoException("El cliente no existe.");
        db.Clientes.Remove(cliente);
        await db.SaveChangesAsync(); // si tiene ventas, la FK lo impide → 409
    }

    private async Task Copiar(ClienteDto d, Cliente c, int? idActual)
    {
        var nit = NitAttribute.Normalizar(d.Nit);

        // El NIT no se repite, excepto "CF" (consumidor final)
        if (nit != "CF" && await db.Clientes.AnyAsync(x => x.Nit == nit && x.IdCliente != idActual))
            throw new ReglaNegocioException($"Ya existe un cliente con el NIT {nit}.", StatusCodes.Status409Conflict);

        if (d.CorreoElectronico is { } correo
            && await db.Clientes.AnyAsync(x => x.CorreoElectronico == correo && x.IdCliente != idActual))
            throw new ReglaNegocioException($"Ya existe un cliente con el correo {correo}.", StatusCodes.Status409Conflict);

        c.Nombres = d.Nombres.Trim();
        c.Apellidos = d.Apellidos.Trim();
        c.Nit = nit;
        c.Genero = d.Genero;
        c.Telefono = d.Telefono;
        c.CorreoElectronico = d.CorreoElectronico?.ToLowerInvariant();
    }
}
