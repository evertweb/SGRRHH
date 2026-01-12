<#
.SYNOPSIS
    Configura reglas de firewall para permitir acceso a SGRRHH desde la red local.

.DESCRIPTION
    Crea reglas de entrada en Windows Firewall para los puertos HTTP (5002) y HTTPS (5003).
    Debe ejecutarse con permisos de Administrador en el servidor.

.EXAMPLE
    # En el servidor (como Administrador):
    .\Setup-FirewallRules.ps1
    
    # O remotamente via SSH (si SSH corre como admin):
    ssh equipo1@192.168.1.248 "powershell -ExecutionPolicy Bypass -File C:\SGRRHH\scripts\Setup-FirewallRules.ps1"
#>

param(
    [int]$HttpPort = 5002,
    [int]$HttpsPort = 5003,
    [switch]$Remove
)

$ErrorActionPreference = "Stop"

# Nombres de las reglas
$RuleNameHttp = "SGRRHH - HTTP ($HttpPort)"
$RuleNameHttps = "SGRRHH - HTTPS ($HttpsPort)"

function Write-Status {
    param([string]$Message, [string]$Status = "INFO")
    $color = switch ($Status) {
        "INFO"  { "Cyan" }
        "OK"    { "Green" }
        "WARN"  { "Yellow" }
        "ERROR" { "Red" }
        default { "White" }
    }
    Write-Host "[$Status] " -ForegroundColor $color -NoNewline
    Write-Host $Message
}

# Verificar permisos de administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Status "Este script requiere permisos de Administrador" "ERROR"
    Write-Host ""
    Write-Host "Ejecuta PowerShell como Administrador y vuelve a intentar." -ForegroundColor Yellow
    exit 1
}

if ($Remove) {
    # Eliminar reglas existentes
    Write-Status "Eliminando reglas de firewall..."
    
    Get-NetFirewallRule -DisplayName $RuleNameHttp -ErrorAction SilentlyContinue | Remove-NetFirewallRule
    Get-NetFirewallRule -DisplayName $RuleNameHttps -ErrorAction SilentlyContinue | Remove-NetFirewallRule
    
    Write-Status "Reglas eliminadas correctamente" "OK"
    exit 0
}

Write-Host ""
Write-Host "========================================" -ForegroundColor DarkGray
Write-Host "  CONFIGURACIÓN DE FIREWALL - SGRRHH" -ForegroundColor White
Write-Host "========================================" -ForegroundColor DarkGray
Write-Host ""

# Verificar si ya existen las reglas
$existingHttp = Get-NetFirewallRule -DisplayName $RuleNameHttp -ErrorAction SilentlyContinue
$existingHttps = Get-NetFirewallRule -DisplayName $RuleNameHttps -ErrorAction SilentlyContinue

if ($existingHttp -and $existingHttps) {
    Write-Status "Las reglas de firewall ya existen" "WARN"
    Write-Host ""
    Write-Host "  HTTP:  $RuleNameHttp" -ForegroundColor Gray
    Write-Host "  HTTPS: $RuleNameHttps" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Para eliminarlas, ejecuta: .\Setup-FirewallRules.ps1 -Remove" -ForegroundColor Yellow
    exit 0
}

# Crear regla para HTTP
if (-not $existingHttp) {
    Write-Status "Creando regla para HTTP (puerto $HttpPort)..."
    
    New-NetFirewallRule `
        -DisplayName $RuleNameHttp `
        -Description "Permite acceso HTTP a SGRRHH desde la red local" `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort $HttpPort `
        -Action Allow `
        -Profile Private,Domain `
        -Enabled True | Out-Null
    
    Write-Status "Regla HTTP creada" "OK"
}

# Crear regla para HTTPS
if (-not $existingHttps) {
    Write-Status "Creando regla para HTTPS (puerto $HttpsPort)..."
    
    New-NetFirewallRule `
        -DisplayName $RuleNameHttps `
        -Description "Permite acceso HTTPS a SGRRHH desde la red local" `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort $HttpsPort `
        -Action Allow `
        -Profile Private,Domain `
        -Enabled True | Out-Null
    
    Write-Status "Regla HTTPS creada" "OK"
}

# Verificar que la app puede escuchar
Write-Host ""
Write-Status "Verificando configuración..."

# Mostrar resumen
Write-Host ""
Write-Host "========================================" -ForegroundColor DarkGray
Write-Host "  CONFIGURACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  Puertos abiertos:" -ForegroundColor White
Write-Host "    - HTTP:  $HttpPort" -ForegroundColor Gray
Write-Host "    - HTTPS: $HttpsPort" -ForegroundColor Gray
Write-Host ""
Write-Host "  Perfiles de red: Private, Domain" -ForegroundColor Gray
Write-Host ""
Write-Host "  URLs de acceso desde otros PCs:" -ForegroundColor Yellow
Write-Host "    http://192.168.1.248:$HttpPort" -ForegroundColor Cyan
Write-Host "    https://192.168.1.248:$HttpsPort" -ForegroundColor Cyan
Write-Host ""
Write-Host "  NOTA: Para HTTPS, los clientes deben confiar en el certificado" -ForegroundColor DarkYellow
Write-Host "        o usar HTTP para pruebas internas." -ForegroundColor DarkYellow
Write-Host ""
