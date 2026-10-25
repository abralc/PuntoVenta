namespace PuntoVenta.Api.Modulos.Seguridad.Entidades;

// Modelos (la "M" de MVC) del módulo de seguridad.
// Tablas: roles, usuarios, menu, roles_menu

public class Rol
{
    public short IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public List<RolMenu> Menus { get; set; } = [];
}

public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int? IdEmpleado { get; set; }
    public short IdRol { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    public Rol? Rol { get; set; }
}

public class MenuOpcion
{
    public int IdMenu { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icono { get; set; }
    public int? IdPadre { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;

    public MenuOpcion? Padre { get; set; }
    public List<MenuOpcion> Hijos { get; set; } = [];
}

public class RolMenu
{
    public short IdRol { get; set; }
    public int IdMenu { get; set; }

    public Rol? Rol { get; set; }
    public MenuOpcion? Menu { get; set; }
}
