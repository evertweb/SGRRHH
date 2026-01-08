# Setup-ScheduledTask.ps1
# EJECUTAR COMO ADMINISTRADOR EN EL PC SERVIDOR
# Configura la tarea programada para verificar actualizaciones automaticamente

param(
    [ValidateSet("Hourly", "Daily", "Startup")]
    [string]$Frequency = "Hourly",
    
    [int]$Hour = 6,  # Para Daily: hora del dia (0-23)
    
    [string]$UpdateSource = "\\DESARROLLO\SGRRHHUpdates",  # Cambiar al nombre de tu PC
    
    [switch]$Remove
)

$ErrorActionPreference = "Stop"
$taskName = "SGRRHH-AutoUpdate"
$scriptPath = "C:\SGRRHH\scripts\AutoUpdate-Service.ps1"

# Verificar si es administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "ERROR: Este script debe ejecutarse como Administrador" -ForegroundColor Red
    Write-Host "Haz clic derecho -> 'Ejecutar como administrador'" -ForegroundColor Yellow
    exit 1
}

if ($Remove) {
    Write-Host "Eliminando tarea programada: $taskName" -ForegroundColor Yellow
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue
    Write-Host "Tarea eliminada correctamente" -ForegroundColor Green
    exit 0
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  CONFIGURACION DE ACTUALIZACION AUTOMATICA " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Verificar que existe el script de auto-update
$localScript = Join-Path $PSScriptRoot "AutoUpdate-Service.ps1"
if (-not (Test-Path $localScript)) {
    Write-Host "ERROR: No se encuentra AutoUpdate-Service.ps1" -ForegroundColor Red
    exit 1
}

# Crear directorio de scripts en la instalacion
$installScriptsPath = "C:\SGRRHH\scripts"
if (-not (Test-Path $installScriptsPath)) {
    New-Item -ItemType Directory -Path $installScriptsPath -Force | Out-Null
}

# Copiar el script
Copy-Item $localScript $scriptPath -Force
Write-Host "[OK] Script copiado a: $scriptPath" -ForegroundColor Green

# Eliminar tarea existente si hay
Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue

# Crear trigger segun frecuencia
$trigger = switch ($Frequency) {
    "Hourly" {
        New-ScheduledTaskTrigger -Once -At "00:00" -RepetitionInterval (New-TimeSpan -Hours 1)
    }
    "Daily" {
        New-ScheduledTaskTrigger -Daily -At "$($Hour):00"
    }
    "Startup" {
        New-ScheduledTaskTrigger -AtStartup
    }
}

# Agregar trigger adicional al inicio (siempre)
$triggers = @($trigger)
if ($Frequency -ne "Startup") {
    $startupTrigger = New-ScheduledTaskTrigger -AtStartup
    $triggers += $startupTrigger
}

# Accion: ejecutar PowerShell con el script
$action = New-ScheduledTaskAction -Execute "powershell.exe" `
    -Argument "-ExecutionPolicy Bypass -WindowStyle Hidden -File `"$scriptPath`" -UpdateSource `"$UpdateSource`""

# Configuracion
$settings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RunOnlyIfNetworkAvailable `
    -RestartInterval (New-TimeSpan -Minutes 5) `
    -RestartCount 3

# Principal (ejecutar como SYSTEM con permisos elevados)
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest

# Registrar tarea
Register-ScheduledTask -TaskName $taskName `
    -Trigger $triggers `
    -Action $action `
    -Settings $settings `
    -Principal $principal `
    -Description "Verifica y aplica actualizaciones de SGRRHH automaticamente"

Write-Host ""
Write-Host "[OK] Tarea programada creada: $taskName" -ForegroundColor Green
Write-Host ""
Write-Host "Configuracion:" -ForegroundColor Cyan
Write-Host "  - Frecuencia: $Frequency"
if ($Frequency -eq "Daily") {
    Write-Host "  - Hora: $Hour`:00"
}
Write-Host "  - Fuente: $UpdateSource"
Write-Host "  - Script: $scriptPath"
Write-Host ""

# Verificar
$task = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
if ($task) {
    Write-Host "Estado de la tarea:" -ForegroundColor Yellow
    Write-Host "  - Estado: $($task.State)"
    Write-Host "  - Triggers: $($task.Triggers.Count)"
    
    # Mostrar proxima ejecucion
    $info = Get-ScheduledTaskInfo -TaskName $taskName
    if ($info.NextRunTime) {
        Write-Host "  - Proxima ejecucion: $($info.NextRunTime)"
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  CONFIGURACION COMPLETADA " -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Comandos utiles:" -ForegroundColor Yellow
Write-Host "  Ver estado:   Get-ScheduledTask -TaskName '$taskName'"
Write-Host "  Ejecutar:     Start-ScheduledTask -TaskName '$taskName'"
Write-Host "  Desactivar:   Disable-ScheduledTask -TaskName '$taskName'"
Write-Host "  Eliminar:     .\Setup-ScheduledTask.ps1 -Remove"
Write-Host ""
Write-Host "Logs de actualizacion:" -ForegroundColor Yellow
Write-Host "  C:\SGRRHH\Logs\autoupdate.log"
Write-Host ""
