# Punto de Venta · Proyecto Final de Programación

Solución completa para administrar un punto de venta: catálogos, inventario,
compras y ventas (maestro‑detalle), punto de venta (POS) con reservas entre cajas,
reportes, menú dinámico y autenticación JWT.

| Capa | Tecnología |
|---|---|
| Vista (V) | Blazor WebAssembly · .NET 10 · Bootstrap 5 |
| Controlador (C) | ASP.NET Core Web API · .NET 10 |
| Modelo (M) | Entity Framework Core + Pomelo · MySQL 8.0 |
| Seguridad | JWT (HS256) · contraseñas con BCrypt |
| Servidor | IIS en máquina virtual de Azure Lab (red local) |

---

## 1. Estructura

```
PuntoVenta.slnx
├─ database/                  Scripts SQL: esquema, datos semilla, usuario de la app
├─ scripts/git/               Scripts para crear el repo y que cada quien suba su parte
└─ src/
   ├─ PuntoVenta.Shared/      DTOs con validaciones (se usan en API y Blazor)
   │   └─ <Modulo>/
   ├─ PuntoVenta.Api/         API REST (puerto 5000)
   │   ├─ Data/               AppDbContext (clase parcial)
   │   ├─ Infraestructura/    Errores, JWT, registro automático de servicios
   │   └─ Modulos/<Modulo>/   Entidades, Configuraciones, Service, Controller
   └─ PuntoVenta.Web/         Blazor WebAssembly (puerto 80 en IIS)
       ├─ Infraestructura/    ApiService, token, notificaciones, confirmación
       ├─ Layout/ Componentes/
       └─ Modulos/<Modulo>/   Pantallas .razor del módulo
```

**Cada módulo vive en 3 carpetas con el mismo nombre** (Api, Shared y Web). Así cada
integrante trabaja en sus carpetas y nadie edita archivos de otro.

---

## 2. Equipo y responsabilidades

| # | Integrante | GitHub | Módulos (carpetas) | Rama |
|---|---|---|---|---|
| 1 | Abraham Isaac López Cifuentes | `abralc` | Base: solución, base de datos, infraestructura, layout y menú (vista), `Estado`, `Configuracion` (margen de ganancia), despliegue IIS. Dueño del repositorio | `feature/base-configuracion` |
| 2 | Ángel Andre Barrientos García | `20-Angel-20` | `Seguridad`: login, JWT, API del menú dinámico por rol, CRUD de usuarios | `feature/seguridad` |
| 3 | Sharon Georgette Nahomy Calancho Toledo | `geololzzz` | `Puestos`, `Empleados`, `Clientes`, `Proveedores` | `feature/catalogos` |
| 4 | Eutimio Rafael Zamora Monterroso | `ezamoram5-rgb` | `Marcas`, `Productos` (con imagen), `Compras` (maestro‑detalle) | `feature/productos-compras` |
| 5 | Jhostin Estuardo Quiñonez Aguilar | `Jhosag` | `Ventas` (maestro‑detalle), `Reportes` | `feature/ventas-reportes` |
| 6 | Abraham Isaac López Cifuentes | `abralc` | `Pos` (punto de venta con reservas). Se sube al final porque usa Productos y Ventas | `feature/punto-de-venta` |

### 2.1 Qué debe poder explicar cada integrante en la presentación

**1 · Isaac (base, Configuración, POS)**
- `Program.cs` de la API: conexión MySQL, JWT, CORS, Swagger, middleware de errores y registro automático de servicios.
- `AppDbContext` parcial: cada módulo agrega sus `DbSet` en su carpeta.
- `ApiService` (Blazor): todas las pantallas llaman a la API con GET/POST/PUT/DELETE y el token va en el encabezado.
- Configuración: el 25 % está en la tabla `configuracion`; vista previa Q100.00 → Q125.00; "Recalcular precios".
- POS: reserva temporal (`reservas_pos`), disponibilidad = existencia − reservas de otras cajas, demo con dos pestañas.
- Despliegue en IIS y acceso desde otra máquina virtual.

**2 · Ángel (Seguridad)**
- Login → `POST /api/auth/login` → BCrypt valida la contraseña → `JwtService` genera el token (claims: usuario, rol, empleado).
- Blazor guarda el token (`AlmacenToken`), `JwtAuthStateProvider` lee los claims y `AuthHeaderHandler` lo envía en cada petición.
- Endpoints protegidos con `[Authorize]`; sin token la API responde 401 y Blazor redirige al login.
- Menú dinámico: tablas `menu` (padre‑hijo) y `roles_menu`; `MenuService` arma el árbol; cada rol ve opciones distintas.
- CRUD de usuarios (solo Administrador).

**3 · Sharon (Catálogos)**
- CRUD de Puestos, Empleados, Clientes y Proveedores con búsqueda y paginación.
- Combo de puestos en Empleados, con enlace a Puestos y botón "Regresar a Empleados".
- Validaciones en Blazor y en la API (mismo DTO): DPI de 13 dígitos, NIT con dígito verificador, mayoría de edad, correo, duplicados.

**4 · Eutimio (Productos y Compras)**
- Marcas ↔ Productos con enlaces; combo de marcas.
- Imagen: se sube a `api/wwwroot/images/productos/` y en la BD solo se guarda la ruta.
- Compras maestro‑detalle en una pantalla y en una transacción: existencia += cantidad, precio_costo = precio de compra,
  precio_venta = costo + 25 %. Al editar o eliminar se revierte la existencia.

**5 · Jhostin (Ventas y Reportes)**
- Ventas maestro‑detalle: "Verificar existencia y agregar" (mensaje *Existencia insuficiente*), precio de la tabla productos,
  subtotal y total automáticos, existencia −= cantidad, número de factura automático, anular devuelve la existencia.
- Reportes de productos, existencias, compras y ventas (datos de varias tablas), impresión y exportación a Excel (CSV).
- Indicadores de la pantalla de inicio.

## 3. Requisitos para trabajar en su computadora

1. **.NET 10 SDK** — https://dotnet.microsoft.com/download/dotnet/10.0
2. **MySQL 8.0** (Server + Workbench) — https://dev.mysql.com/downloads/installer/
3. **Git** — https://git-scm.com/download/win
4. Editor: **Visual Studio 2026** (carga de trabajo "Desarrollo de ASP.NET y web") o **VS Code** con *C# Dev Kit*.
5. Cuenta de GitHub (Isaac los agrega como colaboradores).

Verificar: `dotnet --version` debe mostrar `10.x`.

---

## 4. Base de datos

Ejecutar en MySQL Workbench (o consola) **en este orden**:

```
database/01_esquema.sql            Crea db_punto_venta y todas las tablas
database/02_datos_semilla.sql      Datos de prueba en TODAS las tablas
database/03_usuario_aplicacion.sql Usuario pv_app que usa la API
```

```powershell
mysql -u root -p < database\01_esquema.sql
mysql -u root -p < database\02_datos_semilla.sql
mysql -u root -p < database\03_usuario_aplicacion.sql
```

Los datos semilla son consistentes: la existencia de cada producto = total comprado − total
vendido, y el precio de venta = costo + 25 %.

**Usuarios de prueba**

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123*` | Administrador (ve todo el menú) |
| `vendedor` | `Vendedor123*` | Vendedor |
| `compras` | `Compras123*` | Compras |

---

## 5. Ejecutar en modo desarrollo

Dos terminales desde la carpeta raíz:

```powershell
# Terminal 1 - API  → http://localhost:5000/swagger
dotnet run --project src/PuntoVenta.Api

# Terminal 2 - Blazor → http://localhost:5100
dotnet run --project src/PuntoVenta.Web
```

En Visual Studio: clic derecho en la solución → *Configurar proyectos de inicio* →
*Varios proyectos de inicio* → Api y Web en "Iniciar".

- La API lee la conexión de `src/PuntoVenta.Api/appsettings.json`.
- Blazor lee la dirección de la API de `src/PuntoVenta.Web/wwwroot/appsettings.Development.json`
  (local) y `wwwroot/appsettings.json` (servidor, `http://10.0.0.4:5000/`).

**Probar el JWT en Swagger:** `POST /api/auth/login` → copiar `token` → botón **Authorize** →
pegar el token → ya puede llamar a los endpoints protegidos.

---

## 5.1 Probar desde el celular o desde otra computadora (antes de IIS)

1. Conecte la computadora y el celular a la **misma red Wi‑Fi** y vea la IP de la computadora: `ipconfig` (ej. `192.168.1.50`).
2. En `src/PuntoVenta.Web/wwwroot/appsettings.Development.json` cambie `"ApiUrl": "http://192.168.1.50:5000/"`.
3. Ejecute con el perfil de red local:

```powershell
dotnet run --project src/PuntoVenta.Api --launch-profile red-local
dotnet run --project src/PuntoVenta.Web --launch-profile red-local
```

4. Si Windows pregunta, permita el acceso en **redes privadas** (o cree reglas de entrada para TCP 5000 y 5100).
5. En el celular abra `http://192.168.1.50:5100` → iniciar sesión → **Ventas → Punto de Venta**.

> Al terminar, regrese `ApiUrl` a `http://localhost:5000/` si solo va a trabajar en su computadora.

## 5.2 Guion de la demo (flujo que pide el enunciado)

| # | Paso | Dónde | Qué mostrar |
|---|---|---|---|
| 1 | Inicio de sesión + JWT | `/login`, Swagger | Usuario incorrecto → mensaje; correcto → token. En Swagger: sin token 401, con *Authorize* 200 |
| 2 | Menú dinámico | Menú lateral | Entrar como `admin` y como `vendedor`: el menú cambia (tabla `menu` + `roles_menu`) |
| 3 | CRUD | Puestos, Empleados, Clientes, Proveedores, Marcas | Crear, buscar, editar, eliminar; validación (DPI, NIT, correo); eliminar puesto con empleados → mensaje 409 |
| 4 | Producto con imagen | Productos | Crear producto con imagen → se ve en la tabla/tarjetas; mostrar la ruta guardada |
| 5 | Compra | Compras → Nueva compra | *Audífonos Redmi Buds* (existencia 20): comprar 10 a Q100.00 → existencia 30, costo Q100.00, venta Q125.00 |
| 6 | Venta | Ventas → Nueva venta | *Memoria RAM 8 GB* (existencia 5): pedir 7 → *Existencia insuficiente*; vender 5 → existencia 0; total automático |
| 7 | POS con reservas | Punto de Venta en 2 pestañas | Pestaña A agrega las últimas unidades → pestaña B muestra "en otra caja"; cobrar e imprimir ticket |
| 8 | Configuración | Administración → Configuración | Cambiar 25 % y ver la vista previa |
| 9 | Reportes | Reportes | Los 4 reportes, imprimir y exportar CSV |
| 10 | Red local | Otra VM | `http://10.0.0.4` desde otra máquina virtual |

## 6. Flujo de Git del equipo

Repositorio: **https://github.com/abralc/PuntoVenta**

### 6.0 Todos — preparar Git (solo la primera vez)

1. Instalar Git: https://git-scm.com/download/win (opciones por defecto).
2. Aceptar la invitación al repositorio: correo de GitHub o
   https://github.com/abralc/PuntoVenta/invitations
3. Abrir **PowerShell** y configurar su identidad con **el mismo correo de su cuenta de GitHub**
   (GitHub → Settings → Emails). Si el correo no coincide, sus commits no aparecen a su nombre.

```powershell
git config --global user.name  "Su Nombre Completo"
git config --global user.email "correo-de-su-cuenta-github@ejemplo.com"
git config --global init.defaultBranch main
git config --global core.autocrlf true
```

La primera vez que haga `push`, Windows abre una ventana para iniciar sesión en GitHub
(Git Credential Manager). Elija **Sign in with your browser**.

### 6.1 Isaac — subir la base (una sola vez)

El repositorio ya está creado y vacío. Descomprimir el proyecto completo de forma que
`README.md` quede directamente en `C:\PuntoVenta-completo` y ejecutar:

```powershell
cd C:\PuntoVenta-completo
powershell -ExecutionPolicy Bypass -File .\scripts\git\0-crear-repositorio.ps1 `
    -Destino C:\Repos\PuntoVenta `
    -UrlRemoto https://github.com/abralc/PuntoVenta.git
```

Esto sube a `main` la base (solución, BD, infraestructura, layout, scripts) **sin** los
módulos de los compañeros. Después compartir con todos el zip del proyecto completo.

> Si ya subió la base con una versión anterior del zip, no repita este paso: actualice la base con
> `subir-mi-parte.ps1 -Integrante 1` (sección 6.2) y cree el Pull Request.

### 6.2 Cada integrante — subir su parte

1. Clonar el repositorio (solo la primera vez):

```powershell
git clone https://github.com/abralc/PuntoVenta.git C:\Repos\PuntoVenta
```

2. Descomprimir el zip del proyecto completo en `C:\PuntoVenta-completo` (**fuera** del repositorio).
3. Ejecutar **su** comando (cambie el mensaje en cada commit):

```powershell
cd C:\Repos\PuntoVenta

# 2 · Ángel
powershell -ExecutionPolicy Bypass -File .\scripts\git\subir-mi-parte.ps1 -Integrante 2 -Origen C:\PuntoVenta-completo -Mensaje "feat: login con JWT y menú dinámico"
# 3 · Sharon
powershell -ExecutionPolicy Bypass -File .\scripts\git\subir-mi-parte.ps1 -Integrante 3 -Origen C:\PuntoVenta-completo -Mensaje "feat: CRUD de puestos, empleados, clientes y proveedores"
# 4 · Eutimio
powershell -ExecutionPolicy Bypass -File .\scripts\git\subir-mi-parte.ps1 -Integrante 4 -Origen C:\PuntoVenta-completo -Mensaje "feat: marcas, productos con imagen y compras"
# 5 · Jhostin
powershell -ExecutionPolicy Bypass -File .\scripts\git\subir-mi-parte.ps1 -Integrante 5 -Origen C:\PuntoVenta-completo -Mensaje "feat: ventas maestro-detalle y reportes"
# 1 · Isaac (cambios en la base o en Configuración)
powershell -ExecutionPolicy Bypass -File .\scripts\git\subir-mi-parte.ps1 -Integrante 1 -Origen C:\PuntoVenta-completo -Mensaje "feat: configuración del margen de ganancia"
# 6 · Isaac (POS, al final)
powershell -ExecutionPolicy Bypass -File .\scripts\git\subir-mi-parte.ps1 -Integrante 6 -Origen C:\PuntoVenta-completo -Mensaje "feat: punto de venta con reservas"
```

El script:
- se cambia a **su rama** y la actualiza con `main`,
- copia desde `-Origen` **solo sus carpetas**,
- hace `git add` **solo de sus carpetas** (si detecta archivos ajenos, cancela),
- crea el commit, sube la rama y muestra el **enlace para crear el Pull Request**.

4. Abrir el enlace → **Create pull request**.

Cuando Isaac comparta un zip nuevo con mejoras, se repite el paso 3 con otro mensaje.
Si modifica algo de su módulo directamente en el repositorio, ejecute el mismo comando
**sin** `-Origen`.

### 6.3 Isaac — integrar los Pull Requests

Orden: **1 (base) → 2 (Seguridad) → 3 → 4 → 5 → 6 (POS)**. En cada PR: *Files changed*
(revisar) → **Merge pull request**. El proyecto está armado para que `main` **compile en cada paso**
de ese orden (cada módulo solo usa módulos integrados antes). Si se integra en otro orden, puede
no compilar hasta completar los anteriores.

### 6.4 Reglas del equipo

- Nadie hace commits directo a `main`.
- Cada quien modifica **solo sus carpetas**. Si necesita un cambio en la base
  (`Program.cs`, layout, SQL, etc.), se lo pide a Isaac.
- Hagan **varios commits** durante los días de trabajo (`feat:`, `fix:`, `docs:`), no uno solo al final.
- Cada integrante debe **entender y poder explicar su módulo** en la presentación.

---

## 7. Cómo está armado un módulo (guía para todos)

Ejemplo con `Puestos`. Ningún archivo de la base necesita editarse:

```
src/PuntoVenta.Shared/Puestos/PuestoDto.cs                                DTO + [Required], [MaxLength]... (valida en Blazor y en la API)
src/PuntoVenta.Api/Modulos/Puestos/Entidades/Puesto.cs                    Modelo (M)
src/PuntoVenta.Api/Modulos/Puestos/Configuraciones/PuestoConfiguracion.cs Mapeo a la tabla puestos (nombres de columnas)
src/PuntoVenta.Api/Modulos/Puestos/AppDbContext.Puestos.cs                DbSet (partial, namespace PuntoVenta.Api.Data)
src/PuntoVenta.Api/Modulos/Puestos/PuestoService.cs                       Reglas de negocio (IPuestoService)
src/PuntoVenta.Api/Modulos/Puestos/PuestosController.cs                   Controlador (C): GET/POST/PUT/DELETE
src/PuntoVenta.Web/Modulos/Puestos/PuestosLista.razor                     Vista (V): @page "/puestos"
```

- **Servicios:** toda clase `XxxService` dentro de `PuntoVenta.Api.Modulos.*` se registra sola
  (con su interfaz `IXxxService` si existe).
- **Tablas:** las clases `IEntityTypeConfiguration<T>` se cargan solas.
- **Errores:** lanzar `ReglaNegocioException("Existencia insuficiente")` → la API responde con ese mensaje y Blazor lo
  muestra. Duplicados (1062) y borrados con registros relacionados (1451) se traducen solos a 409.
- **Concurrencia:** compras, ventas y POS bloquean las filas de producto (`SELECT ... FOR UPDATE`) dentro de una
  transacción, así dos operaciones simultáneas no dejan la existencia mal.
- **Seguridad:** los controladores llevan `[Authorize]`; en Blazor todas las páginas exigen sesión (`_Imports.razor`).
- **Menú:** la ruta `@page` debe coincidir con la columna `url` de la tabla `menu`.
- **Componentes comunes** (en `Componentes/`): `EncabezadoPagina`, `Buscador`, `Paginacion`, `Modal`, `AlertaErrores`,
  `EstadoVacio`, `Cargando`. Servicios: `ApiService`, `NotificacionService` (toasts), `ConfirmacionService` (diálogo),
  `Navegador` (imprimir, CSV), `Fmt` (formato `Q 1,234.56` y fechas).

### Endpoints principales

| Módulo | Endpoints |
|---|---|
| Seguridad | `POST api/auth/login` · `GET api/auth/perfil` · `GET api/menu` · `api/usuarios` (CRUD, solo Administrador) |
| Catálogos | `api/puestos` · `api/empleados` · `api/clientes` · `api/proveedores` · `api/marcas` (CRUD + `/opciones`) |
| Productos | `api/productos` (CRUD + `/opciones`) · `POST/DELETE api/productos/{id}/imagen` |
| Compras | `api/compras` (CRUD maestro‑detalle) · `GET api/compras/siguiente-orden` |
| Ventas | `api/ventas` (CRUD maestro‑detalle) · `GET api/ventas/disponibilidad/{idProducto}` |
| POS | `GET api/pos/productos` · `GET/DELETE api/pos/carrito/{sesion}` · `PUT api/pos/reservas` · `POST api/pos/cobrar` |
| Reportes | `GET api/reportes/productos · existencias · compras · ventas · resumen` |
| Otros | `GET/PUT api/configuracion` · `POST api/configuracion/recalcular-precios` · `GET api/estado` |

---

## 8. Publicación en IIS (resumen)

La guía paso a paso se agregará en la siguiente entrega. Puntos clave:

1. Instalar en la VM servidor: IIS, **ASP.NET Core Hosting Bundle .NET 10**, **URL Rewrite**, MySQL 8.0.
2. `dotnet publish src/PuntoVenta.Api -c Release -o C:\inetpub\PuntoVenta\api` → sitio en puerto **5000**.
3. `dotnet publish src/PuntoVenta.Web -c Release -o C:\inetpub\PuntoVenta\web-publish` → apuntar el
   sitio del puerto **80** a la carpeta `wwwroot` de esa publicación.
4. Dar permiso de escritura al Application Pool sobre `api\wwwroot\images\productos`.
5. Firewall de Windows: reglas de entrada para TCP 80 y 5000.
6. Revisar `ApiUrl` (Blazor) y `Cors:Origenes` (API) con la IP privada real del servidor.
