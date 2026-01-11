#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Limpia todos los registros de todas las tablas excepto la tabla de usuarios
.DESCRIPTION
    Este script usa la herramienta ClearDatabaseTool para limpiar la base de datos.
    ADVERTENCIA: Esta operación no se puede deshacer. Se recomienda hacer un backup primero.
.PARAMETER DatabasePath
    Ruta a la base de datos SQLite. Por defecto usa C:\SGRRHH\Data\sgrrhh.db
.PARAMETER Force
    Omite la confirmación de seguridad
.EXAMPLE
    .\Clear-Database.ps1
    Limpia todas las tablas excepto usuarios con confirmación
.EXAMPLE
    .\Clear-Database.ps1 -Force
    Limpia todas las tablas excepto usuarios sin confirmación
#>

param(
    [string]$DatabasePath = "C:\SGRRHH\Data\sgrrhh.db",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Verificar si existe la base de datos
if (-not (Test-Path $DatabasePath)) {
    Write-Host "ERROR: No se encuentra la base de datos en: $DatabasePath" -ForegroundColor Red
    Write-Host "Verifica la ruta e intenta nuevamente." -ForegroundColor Yellow
    exit 1
}

# Confirmación de seguridad
if (-not $Force) {
    Write-Host ""
    Write-Host "[!] ADVERTENCIA: Esta operacion eliminara TODOS los datos excepto usuarios" -ForegroundColor Red
    Write-Host "[!] Esta accion NO SE PUEDE DESHACER" -ForegroundColor Red
    Write-Host ""
    Write-Host "Base de datos: $DatabasePath" -ForegroundColor Yellow
    Write-Host ""
    $confirmation = Read-Host "Estas seguro de continuar? (escribe 'SI' para confirmar)"
    
    if ($confirmation -ne "SI") {
        Write-Host ""
        Write-Host "Operacion cancelada por el usuario." -ForegroundColor Yellow
        exit 0
    }
}

# Ruta al proyecto de la herramienta
$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$toolProject = Join-Path $projectRoot "tools\ClearDatabaseTool\ClearDatabaseTool.csproj"

if (-not (Test-Path $toolProject)) {
    Write-Host "ERROR: No se encuentra el proyecto ClearDatabaseTool" -ForegroundColor Red
    Write-Host "Ruta esperada: $toolProject" -ForegroundColor Yellow
    exit 1
}

try {
    # Ejecutar la herramienta
    Write-Host ""
    dotnet run --project $toolProject --configuration Release -- $DatabasePath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
    }
    else {
        throw "La herramienta de limpieza termino con errores"
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: No se pudo completar la limpieza" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
