# Backup-Database.ps1
# Crea backup seguro de la base de datos SQLite

param(
    [string]$BackupName,
    [switch]$Verify
)

$ErrorActionPreference = "Stop"

# Configuracion
$dataPath = "C:\SGRRHH\Data"
$backupPath = "C:\SGRRHH\Data\Backups"
$dbFile = Join-Path $dataPath "sgrrhh.db"

Write-Host ""
Write-Host "=== BACKUP DE BASE DE DATOS ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que existe la base de datos
if (-not (Test-Path $dbFile)) {
    Write-Host "No existe la base de datos: $dbFile" -ForegroundColor Red
    exit 1
}

# Crear directorio de backups si no existe
if (-not (Test-Path $backupPath)) {
    New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
    Write-Host "Directorio de backups creado: $backupPath" -ForegroundColor Gray
}

# Verificar si el servidor esta corriendo (advertencia)
$serverRunning = Get-Process -Name "SGRRHH.Local.Server" -ErrorAction SilentlyContinue
if ($serverRunning) {
    Write-Host "ADVERTENCIA: El servidor esta corriendo." -ForegroundColor Yellow
    Write-Host "El backup se realizara, pero es mas seguro con el servidor detenido." -ForegroundColor Yellow
    Write-Host ""
}

# Nombre del backup
if (-not $BackupName) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $BackupName = "sgrrhh_$timestamp"
}

$backupFile = Join-Path $backupPath "$BackupName.db"

# Obtener tamano original
$originalSize = (Get-Item $dbFile).Length
$originalSizeMB = [math]::Round($originalSize / 1MB, 2)

Write-Host "Base de datos: $dbFile" -ForegroundColor Gray
Write-Host "Tamano: $originalSizeMB MB" -ForegroundColor Gray
Write-Host ""

# Crear backup
Write-Host "Creando backup..." -ForegroundColor Yellow
Copy-Item -Path $dbFile -Destination $backupFile -Force

# Verificar integridad
Write-Host "Verificando integridad..." -ForegroundColor Yellow
$originalHash = (Get-FileHash $dbFile -Algorithm SHA256).Hash
$backupHash = (Get-FileHash $backupFile -Algorithm SHA256).Hash

if ($originalHash -eq $backupHash) {
    Write-Host ""
    Write-Host "BACKUP CREADO EXITOSAMENTE" -ForegroundColor Green
    Write-Host ""
    Write-Host "Archivo: $backupFile" -ForegroundColor Gray
    Write-Host "Tamano: $originalSizeMB MB" -ForegroundColor Gray
    Write-Host "Hash SHA256: $backupHash" -ForegroundColor Gray
    
    # Guardar metadatos
    $metadata = @{
        originalFile = $dbFile
        backupFile = $backupFile
        timestamp = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
        sizeBytes = $originalSize
        sha256 = $backupHash
        serverWasRunning = ($null -ne $serverRunning)
    }
    $metadataFile = Join-Path $backupPath "$BackupName.json"
    $metadata | ConvertTo-Json | Set-Content $metadataFile
    
} else {
    Write-Host ""
    Write-Host "ERROR: El backup no coincide con el original!" -ForegroundColor Red
    Write-Host "Hash original: $originalHash" -ForegroundColor Red
    Write-Host "Hash backup:   $backupHash" -ForegroundColor Red
    Remove-Item $backupFile -Force
    exit 1
}

# Listar backups existentes
Write-Host ""
Write-Host "=== BACKUPS DISPONIBLES ===" -ForegroundColor Cyan
$backups = Get-ChildItem $backupPath -Filter "*.db" | Sort-Object LastWriteTime -Descending
foreach ($b in $backups) {
    $sizeMB = [math]::Round($b.Length / 1MB, 2)
    $date = $b.LastWriteTime.ToString("dd/MM/yyyy HH:mm")
    Write-Host "  $($b.Name) - $sizeMB MB - $date" -ForegroundColor Gray
}

# Limpiar backups antiguos (mantener ultimos 10)
$oldBackups = $backups | Select-Object -Skip 10
if ($oldBackups) {
    Write-Host ""
    Write-Host "Eliminando backups antiguos ($($oldBackups.Count))..." -ForegroundColor Yellow
    $oldBackups | ForEach-Object {
        Remove-Item $_.FullName -Force
        $jsonFile = $_.FullName -replace '\.db$', '.json'
        if (Test-Path $jsonFile) { Remove-Item $jsonFile -Force }
    }
}

Write-Host ""
