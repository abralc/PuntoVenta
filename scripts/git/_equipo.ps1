# =====================================================================
#  Definición del equipo: qué módulos (carpetas) le corresponden a cada
#  integrante y en qué rama los sube. Los demás scripts leen este archivo.
#
#  Cada módulo vive en 3 carpetas:
#     src/PuntoVenta.Api/Modulos/<Modulo>     (Controller, Service, Entidades)
#     src/PuntoVenta.Shared/<Modulo>          (DTOs con validaciones)
#     src/PuntoVenta.Web/Modulos/<Modulo>     (Pantallas Blazor)
#
#  El integrante 1 (base) sube TODO lo que no sea módulo de otro integrante:
#  solución, Program.cs, DbContext base, infraestructura, layout, base de datos,
#  scripts y documentación.
#
#  La entrada 6 también es de Isaac: el POS se sube aparte y al final porque
#  depende de Productos y Ventas.
# =====================================================================

# Hashtable normal (NO [ordered]): con [ordered], $Equipo[1] devolvería la POSICIÓN 1, no la clave 1.
$Equipo = @{
    1 = @{ Nombre = 'Abraham Isaac López Cifuentes';           Rama = 'feature/base-configuracion'; Modulos = @('Estado', 'Configuracion');                          EsBase = $true }
    2 = @{ Nombre = 'Ángel Andre Barrientos García';           Rama = 'feature/seguridad';          Modulos = @('Seguridad');                                         EsBase = $false }
    3 = @{ Nombre = 'Sharon Georgette Nahomy Calancho Toledo'; Rama = 'feature/catalogos';          Modulos = @('Puestos', 'Empleados', 'Clientes', 'Proveedores');    EsBase = $false }
    4 = @{ Nombre = 'Eutimio Rafael Zamora Monterroso';        Rama = 'feature/productos-compras';  Modulos = @('Marcas', 'Productos', 'Compras');                     EsBase = $false }
    5 = @{ Nombre = 'Jhostin Estuardo Quiñonez Aguilar';       Rama = 'feature/ventas-reportes';    Modulos = @('Ventas', 'Reportes');                                 EsBase = $false }
    6 = @{ Nombre = 'Abraham Isaac López Cifuentes (POS)';     Rama = 'feature/punto-de-venta';     Modulos = @('Pos');                                                EsBase = $false }
}

function Get-RutasModulo([string]$Modulo) {
    @(
        "src/PuntoVenta.Api/Modulos/$Modulo",
        "src/PuntoVenta.Shared/$Modulo",
        "src/PuntoVenta.Web/Modulos/$Modulo"
    )
}

function Get-RutasIntegrante([int]$Numero) {
    $datos = $Equipo[$Numero]
    if (-not $datos) { throw "No existe el integrante $Numero en scripts/git/_equipo.ps1" }
    foreach ($m in $datos.Modulos) { if ($m) { Get-RutasModulo $m } }
}

# Rutas que pertenecen a los demás integrantes (las que el integrante base NO sube)
function Get-RutasDeOtros([int]$Numero) {
    foreach ($k in ($Equipo.Keys | Sort-Object)) {
        if ($k -ne $Numero -and -not $Equipo[$k].EsBase) { Get-RutasIntegrante $k }
    }
}

function Write-Paso([string]$Texto) { Write-Host "`n==> $Texto" -ForegroundColor Cyan }
function Write-Ok([string]$Texto)   { Write-Host "    $Texto" -ForegroundColor Green }
function Write-Aviso([string]$Texto){ Write-Host "    $Texto" -ForegroundColor Yellow }

function Test-Git {
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        throw "Git no está instalado. Descárguelo de https://git-scm.com/download/win y vuelva a abrir PowerShell."
    }
}

function Test-IdentidadGit {
    $nombre = git config user.name
    $correo = git config user.email
    if ([string]::IsNullOrWhiteSpace($nombre) -or [string]::IsNullOrWhiteSpace($correo)) {
        throw @"
Git no tiene configurado su nombre y correo. Ejecute (con SU correo de GitHub):
    git config --global user.name  "Su Nombre"
    git config --global user.email "su-correo@ejemplo.com"
"@
    }
    Write-Ok "Commits a nombre de: $nombre <$correo>"
}

# Copia una carpeta (PowerShell puro, sin robocopy). No entra a .git, bin, obj, .vs ni node_modules.
#   -ExcluirDirs : rutas ABSOLUTAS de carpetas que no se copian
#   -Espejo      : borra primero el destino (así los archivos eliminados también desaparecen)
# Devuelve $true si la carpeta de origen existe.
function Copy-Carpeta([string]$Desde, [string]$Hacia, [switch]$Espejo, [string[]]$ExcluirDirs = @()) {
    if (-not (Test-Path -LiteralPath $Desde)) { return $false }
    # GetFullPath normaliza los separadores (/ y \) para comparar rutas de forma segura
    $raiz = [IO.Path]::GetFullPath((Resolve-Path -LiteralPath $Desde).Path).TrimEnd('\', '/')
    $excluir = @($ExcluirDirs | Where-Object { $_ } | ForEach-Object { [IO.Path]::GetFullPath($_).TrimEnd('\', '/') })
    $Hacia = [IO.Path]::GetFullPath($Hacia)
    $ignorar = @('.git', 'bin', 'obj', '.vs', 'node_modules')

    if ($Espejo -and (Test-Path -LiteralPath $Hacia)) { Remove-Item -LiteralPath $Hacia -Recurse -Force }
    New-Item -ItemType Directory -Path $Hacia -Force | Out-Null

    $copiados = 0
    $pendientes = New-Object System.Collections.Stack
    $pendientes.Push($raiz)
    while ($pendientes.Count -gt 0) {
        $carpetaActual = $pendientes.Pop()
        foreach ($item in Get-ChildItem -LiteralPath $carpetaActual -Force) {
            if ($item.PSIsContainer) {
                if ($ignorar -contains $item.Name) { continue }
                if ($excluir | Where-Object { $_ -ieq $item.FullName.TrimEnd('\', '/') }) { continue }
                $pendientes.Push($item.FullName)
                continue
            }
            $relativo = $item.FullName.Substring($raiz.Length).TrimStart('\', '/')
            $destino = Join-Path $Hacia $relativo
            $carpetaDestino = Split-Path $destino -Parent
            if (-not (Test-Path -LiteralPath $carpetaDestino)) { New-Item -ItemType Directory -Path $carpetaDestino -Force | Out-Null }
            Copy-Item -LiteralPath $item.FullName -Destination $destino -Force
            $copiados++
        }
    }
    Write-Host "    $copiados archivos copiados de $raiz" -ForegroundColor DarkGray
    return $true
}

function Get-UrlPullRequest([string]$Rama) {
    $remoto = $null
    try { $remoto = git remote get-url origin } catch { }
    if (-not $remoto) { return $null }
    $remoto = $remoto -replace '\.git$', '' -replace '^git@github\.com:', 'https://github.com/'
    return "$remoto/compare/main...$($Rama)?expand=1"
}
