<#
.SYNOPSIS
    SOLO LO EJECUTA ISAAC, UNA VEZ. Crea el repositorio local con la base del
    proyecto (sin los módulos de los demás) y lo sube a la rama main de GitHub.

.EJEMPLO
    1. En GitHub cree un repositorio VACÍO llamado PuntoVenta
       (sin README, sin .gitignore, sin licencia).
    2. Descomprima el proyecto completo en C:\PuntoVenta-completo
    3. En PowerShell:
        cd C:\PuntoVenta-completo
        powershell -ExecutionPolicy Bypass -File .\scripts\git\0-crear-repositorio.ps1 `
            -Destino C:\Repos\PuntoVenta `
            -UrlRemoto https://github.com/abralc/PuntoVenta.git
#>
param(
    [Parameter(Mandatory)] [string] $Destino,
    [Parameter(Mandatory)] [string] $UrlRemoto,
    [string] $Origen = (Resolve-Path "$PSScriptRoot\..\..").Path
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_equipo.ps1"

Test-Git
Test-IdentidadGit

if (Test-Path (Join-Path $Destino '.git')) {
    throw "Ya existe un repositorio en $Destino. Este script solo se ejecuta una vez."
}

Write-Paso "Copiando la base del proyecto (sin los módulos de los demás integrantes)"
New-Item -ItemType Directory -Force -Path $Destino | Out-Null
$excluir = Get-RutasDeOtros 1 | ForEach-Object { Join-Path $Origen $_ }
Write-Host "    Se excluyen las carpetas de: $((2..6 | ForEach-Object { $Equipo[$_].Modulos }) -join ', ')" -ForegroundColor DarkGray
Copy-Carpeta -Desde $Origen -Hacia $Destino -ExcluirDirs $excluir | Out-Null
Write-Ok "Copiado en $Destino"

Set-Location $Destino

Write-Paso "Inicializando Git en la rama main"
git init -b main | Out-Null
git add -A
git commit -m "chore: estructura base del proyecto, base de datos e infraestructura" | Out-Null
Write-Ok "Primer commit creado"

Write-Paso "Conectando con GitHub y subiendo main"
git remote add origin $UrlRemoto
git push -u origin main
if ($LASTEXITCODE -ne 0) { throw "No se pudo subir a GitHub. Verifique la URL y que el repositorio esté vacío." }

Write-Paso "Listo"
Write-Ok "Repositorio: $($UrlRemoto -replace '\.git$', '')"
Write-Host @"

Siguientes pasos:
  1. GitHub > Settings > Collaborators: agregue a sus compañeros por usuario o correo.
  2. Comparta con cada uno la carpeta del proyecto completo (zip) y su comando de README.md.
  3. Integre los Pull Requests en este orden: 2 (Seguridad) -> 3 -> 4 -> 5 -> 6 (POS).
     main compila en cada paso si se respeta ese orden.
"@
