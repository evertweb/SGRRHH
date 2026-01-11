# ============================================================
# Script: Setup-Firewall.ps1
# Propósito: Configurar el firewall de Windows para permitir
#            acceso a SGRRHH desde otros equipos de la red local.
# 
# EJECUTAR COMO ADMINISTRADOR
# ============================================================

param(
    [int]$HttpPort = 5000,
    [int]$HttpsPort = 5003,
    [switch]$Remove
)

$ErrorActionPreference = "Stop"

$ruleName = "SGRRHH Local Server"

if ($Remove) {
    Write-Host "Eliminando reglas de firewall..." -ForegroundColor Yellow
    
    Get-NetFirewallRule -DisplayName "$ruleName*" -ErrorAction SilentlyContinue | 
        Remove-NetFirewallRule
    
    Write-Host "Reglas eliminadas." -ForegroundColor Green
    exit 0
}

Write-Host "=== Configuración de Firewall para SGRRHH ===" -ForegroundColor Cyan
Write-Host ""

# Verificar si se ejecuta como administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: Este script debe ejecutarse como Administrador." -ForegroundColor Red
    Write-Host "Haga clic derecho en PowerShell y seleccione 'Ejecutar como administrador'." -ForegroundColor Yellow
    exit 1
}

# Eliminar reglas existentes
Get-NetFirewallRule -DisplayName "$ruleName*" -ErrorAction SilentlyContinue | 
    Remove-NetFirewallRule -ErrorAction SilentlyContinue

Write-Host "Creando reglas de firewall..." -ForegroundColor Yellow

# Regla para HTTP
New-NetFirewallRule `
    -DisplayName "$ruleName (HTTP $HttpPort)" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort $HttpPort `
    -Action Allow `
    -Profile Domain,Private `
    -Description "Permite acceso a SGRRHH desde la red local (HTTP)" | Out-Null

Write-Host "  [OK] Regla HTTP creada (puerto $HttpPort)" -ForegroundColor Green

# Regla para HTTPS
New-NetFirewallRule `
    -DisplayName "$ruleName (HTTPS $HttpsPort)" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort $HttpsPort `
    -Action Allow `
    -Profile Domain,Private `
    -Description "Permite acceso a SGRRHH desde la red local (HTTPS)" | Out-Null

Write-Host "  [OK] Regla HTTPS creada (puerto $HttpsPort)" -ForegroundColor Green

Write-Host ""
Write-Host "=== Configuración completada ===" -ForegroundColor Cyan
Write-Host ""

# Mostrar IP local
$ipAddresses = Get-NetIPAddress -AddressFamily IPv4 | 
    Where-Object { $_.IPAddress -notlike "127.*" -and $_.IPAddress -notlike "169.*" } |
    Select-Object -ExpandProperty IPAddress

Write-Host "Los usuarios pueden acceder desde otros equipos usando:" -ForegroundColor Yellow
foreach ($ip in $ipAddresses) {
    Write-Host "  HTTP:  http://${ip}:$HttpPort" -ForegroundColor White
    Write-Host "  HTTPS: https://${ip}:$HttpsPort" -ForegroundColor White
}

Write-Host ""
Write-Host "NOTA: Para HTTPS, los navegadores mostrarán una advertencia de certificado." -ForegroundColor Gray
Write-Host "      Los usuarios deben hacer clic en 'Avanzado' > 'Continuar'." -ForegroundColor Gray
