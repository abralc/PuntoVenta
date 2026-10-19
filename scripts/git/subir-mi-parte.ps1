<#
.SYNOPSIS
    Sube a GitHub SOLO las carpetas de los módulos del integrante, en su
    propia rama, y muestra el enlace para crear el Pull Request.

.PARAMETER Integrante
    Número del integrante (ver tabla en README.md):
      1 Isaac (base)  2 Ángel  3 Sharon  4 Eutimio  5 Jhostin  6 Isaac (POS)

.PARAMETER Origen
    Carpeta donde está el proyecto completo descomprimido. Si se indica, el
    script copia de ahí SOLO las carpetas del integrante hacia el repositorio.

.PARAMETER Mensaje
    Mensaje del commit, ej. "feat: CRUD de puestos y empleados"

.EJEMPLO
    cd C:\Repos\PuntoVenta
    powershell -ExecutionPolicy Bypass -File .\scripts\git\subir-mi-parte.ps1 `
        -Integrante 3 -Origen C:\PuntoVenta-completo -Mensaje "feat: CRUD de puestos"
#>
param(
    [Parameter(Mandatory)] [ValidateRange(1, 6)] [int] $Integrante,
    [Parameter(Mandatory)] [string] $Mensaje,
    [string] $Origen
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_equipo.ps1"

$datos = $Equipo[$Integrante]
Test-Git

$raiz = $null
try { $raiz = git rev-parse --show-toplevel } catch { }
if (-not $raiz) { throw "Ejecute este script dentro de la carpeta del repositorio clonado." }
Set-Location $raiz

Write-Host "Integrante $Integrante · $($datos.Nombre)" -ForegroundColor White
Write-Host "Módulos: $($datos.Modulos -join ', ')   Rama: $($datos.Rama)" -ForegroundColor White
Test-IdentidadGit

# ---------------------------------------------------------------------
Write-Paso "Actualizando desde GitHub"
git fetch origin --prune
$existeLocal  = git branch --list $datos.Rama
$existeRemota = git branch -r --list "origin/$($datos.Rama)"

if ($existeLocal) {
    git checkout $datos.Rama | Out-Null
    if ($existeRemota) { git pull --no-edit origin $datos.Rama | Out-Null }
} elseif ($existeRemota) {
    git checkout -b $datos.Rama --track "origin/$($datos.Rama)" | Out-Null
} else {
    git checkout -b $datos.Rama origin/main | Out-Null
}
if ($LASTEXITCODE -ne 0) { throw "No se pudo cambiar a la rama $($datos.Rama). ¿Tiene cambios sin guardar? Revise con: git status" }
# Traer lo último de main para trabajar sobre la versión integrada
git merge --no-edit origin/main | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Conflicto al actualizar con main. Avise a Isaac antes de continuar." }
Write-Ok "En la rama $($datos.Rama), actualizada con main"

# ---------------------------------------------------------------------
if ($Origen) {
    Write-Paso "Copiando sus carpetas desde $Origen"
    if ($datos.EsBase) {
        $excluir = Get-RutasDeOtros $Integrante | ForEach-Object { Join-Path $Origen $_ }
        Copy-Carpeta -Desde $Origen -Hacia $raiz -ExcluirDirs $excluir | Out-Null
        Write-Ok "Base del proyecto copiada"
    } else {
        foreach ($ruta in Get-RutasIntegrante $Integrante) {
            $desde = Join-Path $Origen $ruta
            $hacia = Join-Path $raiz $ruta
            if (Copy-Carpeta -Desde $desde -Hacia $hacia -Espejo) { Write-Ok $ruta }
        }
    }
}

# ---------------------------------------------------------------------
Write-Paso "Agregando SOLO sus archivos"
if ($datos.EsBase) {
    $pathspec = @('--', '.') + (Get-RutasDeOtros $Integrante | ForEach-Object { ":(exclude)$_" })
} else {
    $rutas = Get-RutasIntegrante $Integrante | Where-Object { Test-Path (Join-Path $raiz $_) }
    if (-not $rutas) { throw "No se encontró ninguna carpeta de sus módulos. Use -Origen para copiarlas." }
    $pathspec = @('--') + $rutas
}
git add -A @pathspec

$cambios = git diff --cached --name-only
if (-not $cambios) {
    Write-Aviso "No hay cambios nuevos en sus carpetas. Nada que subir."
    exit 0
}
git diff --cached --stat

# Seguridad: si hay archivos de otros integrantes en el área de preparación, no continuar
if (-not $datos.EsBase) {
    $ajenos = $cambios | Where-Object { $archivo = $_; -not (Get-RutasIntegrante $Integrante | Where-Object { $archivo.StartsWith("$_/") }) }
    if ($ajenos) {
        git reset -q
        throw "Se detectaron archivos fuera de sus módulos:`n$($ajenos -join "`n")`nNo se subió nada."
    }
}

# ---------------------------------------------------------------------
Write-Paso "Creando commit y subiendo la rama"
git commit -m $Mensaje | Out-Null
git push -u origin $datos.Rama
if ($LASTEXITCODE -ne 0) { throw "No se pudo subir. ¿Isaac ya lo agregó como colaborador en GitHub?" }

Write-Paso "Listo"
$url = Get-UrlPullRequest $datos.Rama
if ($url) {
    Write-Ok "Cree (o actualice) su Pull Request aquí:"
    Write-Host "    $url" -ForegroundColor White
}
