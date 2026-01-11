# Restore-Backup.ps1
# Restaura la base de datos o la aplicacion desde un backup

param(
    [string]$BackupFile,
    [switch]$RestoreApp,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Configuracion
$dataPath = "C:\SGRRHH\Data"
$backupPath = "C:\SGRRHH\Data\Backups"
$dbFile = Join-Path $dataPath "sgrrhh.db"
$productionPath = "C:\SGRRHH"
$rollbackPath = "C:\SGRRHH.rollback"

Write-Host ""
Write-Host "=== RESTAURACION DE BACKUP ===" -ForegroundColor Cyan
Write-Host ""

# Verificar si el servidor esta corriendo
$serverRunning = Get-Process -Name "SGRRHH.Local.Server" -ErrorAction SilentlyContinue
if ($serverRunning -and -not $Force) {
    Write-Host "ADVERTENCIA: El servidor esta corriendo." -ForegroundColor Yellow
    Write-Host "Debe detenerlo antes de restaurar." -ForegroundColor Yellow
    Write-Host ""
    $confirm = Read-Host "Detener el servidor y continuar? (S/N)"
    if ($confirm -ne "S" -and $confirm -ne "s") {
        Write-Host "Restauracion cancelada" -ForegroundColor Yellow
        exit 0
    }
    
    Write-Host "Deteniendo servidor..." -ForegroundColor Yellow
    $serverRunning | Stop-Process -Force
    Start-Sleep -Seconds 3
}

# Opcion 1: Restaurar aplicacion
if ($RestoreApp) {
    Write-Host "=== RESTAURAR APLICACION ===" -ForegroundColor Yellow
    
    if (-not (Test-Path $rollbackPath)) {
        Write-Host "No hay punto de restauracion disponible en: $rollbackPath" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Punto de restauracion encontrado: $rollbackPath" -ForegroundColor Gray
    
    if (-not $Force) {
        $confirm = Read-Host "Restaurar la aplicacion? Se perderan los cambios actuales (S/N)"
        if ($confirm -ne "S" -and $confirm -ne "s") {
            Write-Host "Restauracion cancelada" -ForegroundColor Yellow
            exit 0
        }
    }
    
    # Restaurar archivos de aplicacion
    $excludeFolders = @("Data", "Logs", "Storage", "Backups")
    Get-ChildItem $productionPath -Exclude $excludeFolders | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    Get-ChildItem $rollbackPath | ForEach-Object {
        Copy-Item $_.FullName -Destination $productionPath -Recurse -Force
    }
    
    Write-Host ""
    Write-Host "APLICACION RESTAURADA EXITOSAMENTE" -ForegroundColor Green
    Write-Host ""
    
    $startNow = Read-Host "Iniciar el servidor? (S/N)"
    if ($startNow -eq "S" -or $startNow -eq "s") {
        Start-Process -FilePath (Join-Path $productionPath "SGRRHH.Local.Server.exe") -WorkingDirectory $productionPath
        Start-Sleep -Seconds 2
        Start-Process "https://localhost:5003"
    }
    
    exit 0
}

# Opcion 2: Restaurar base de datos
Write-Host "=== RESTAURAR BASE DE DATOS ===" -ForegroundColor Yellow

# Listar backups disponibles si no se especifico uno
if (-not $BackupFile) {
    $backups = Get-ChildItem $backupPath -Filter "*.db" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
    
    if (-not $backups -or $backups.Count -eq 0) {
        Write-Host "No hay backups disponibles en: $backupPath" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Backups disponibles:" -ForegroundColor Gray
    Write-Host ""
    
    $i = 1
    foreach ($b in $backups) {
        $sizeMB = [math]::Round($b.Length / 1MB, 2)
        $date = $b.LastWriteTime.ToString("dd/MM/yyyy HH:mm")
        Write-Host "  [$i] $($b.Name) - $sizeMB MB - $date" -ForegroundColor White
        $i++
    }
    
    Write-Host ""
    $selection = Read-Host "Seleccione el numero del backup a restaurar (1-$($backups.Count))"
    
    $index = [int]$selection - 1
    if ($index -lt 0 -or $index -ge $backups.Count) {
        Write-Host "Seleccion invalida" -ForegroundColor Red
        exit 1
    }
    
    $BackupFile = $backups[$index].FullName
}

# Verificar que el backup existe
if (-not (Test-Path $BackupFile)) {
    Write-Host "No se encuentra el backup: $BackupFile" -ForegroundColor Red
    exit 1
}

$backupSize = [math]::Round((Get-Item $BackupFile).Length / 1MB, 2)
Write-Host ""
Write-Host "Backup seleccionado: $BackupFile" -ForegroundColor Gray
Write-Host "Tamano: $backupSize MB" -ForegroundColor Gray

# Confirmar
if (-not $Force) {
    Write-Host ""
    Write-Host "ADVERTENCIA: Esto reemplazara la base de datos actual!" -ForegroundColor Red
    $confirm = Read-Host "Continuar? (escriba SI para confirmar)"
    if ($confirm -ne "SI") {
        Write-Host "Restauracion cancelada" -ForegroundColor Yellow
        exit 0
    }
}

# Crear backup de la base actual antes de restaurar
if (Test-Path $dbFile) {
    $preRestoreBackup = Join-Path $backupPath "pre_restore_$(Get-Date -Format 'yyyyMMdd_HHmmss').db"
    Write-Host "Creando backup de la base actual..." -ForegroundColor Yellow
    Copy-Item $dbFile $preRestoreBackup -Force
    Write-Host "Backup pre-restauracion: $preRestoreBackup" -ForegroundColor Gray
}

# Restaurar
Write-Host "Restaurando base de datos..." -ForegroundColor Yellow
Copy-Item $BackupFile $dbFile -Force

# Verificar
$restoredHash = (Get-FileHash $dbFile -Algorithm SHA256).Hash
$backupHash = (Get-FileHash $BackupFile -Algorithm SHA256).Hash

if ($restoredHash -eq $backupHash) {
    Write-Host ""
    Write-Host "BASE DE DATOS RESTAURADA EXITOSAMENTE" -ForegroundColor Green
    Write-Host ""
    Write-Host "Archivo: $dbFile" -ForegroundColor Gray
    Write-Host "Desde: $BackupFile" -ForegroundColor Gray
    
    $startNow = Read-Host "Iniciar el servidor? (S/N)"
    if ($startNow -eq "S" -or $startNow -eq "s") {
        Start-Process -FilePath (Join-Path $productionPath "SGRRHH.Local.Server.exe") -WorkingDirectory $productionPath
        Start-Sleep -Seconds 2
        Start-Process "https://localhost:5003"
    }
} else {
    Write-Host ""
    Write-Host "ERROR: La restauracion fallo! Los hashes no coinciden." -ForegroundColor Red
    Write-Host "Intentando restaurar desde backup pre-restauracion..." -ForegroundColor Yellow
    
    if (Test-Path $preRestoreBackup) {
        Copy-Item $preRestoreBackup $dbFile -Force
        Write-Host "Base de datos original restaurada" -ForegroundColor Green
    }
    exit 1
}

Write-Host ""
