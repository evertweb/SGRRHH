# ============================================================
#  SINCRONIZAR DB DE PRODUCCION PARA DESARROLLO
# ============================================================
# Este script copia la base de datos de produccion del servidor
# remoto a la ubicacion local para desarrollo.
#
# USO:
#   .\Sync-ProductionDb.ps1           # Sincroniza y confirma
#   .\Sync-ProductionDb.ps1 -Force    # Sincroniza sin preguntar
#   .\Sync-ProductionDb.ps1 -Watch    # Sincroniza e inicia dotnet watch
# ============================================================

param(
    [switch]$Force,
    [switch]$Watch
)

$ErrorActionPreference = "Stop"

# Configuracion
$RemoteServer = "192.168.1.248"
$RemoteUser = "equipo1"
$LocalDbDir = "C:\SGRRHH\Data"
$LocalDbPath = "$LocalDbDir\sgrrhh.db"
$BackupDir = "$LocalDbDir\Backups\Dev"
$ProjectPath = "C:\Users\evert\Documents\rrhh\SGRRHH.Local\SGRRHH.Local.Server"

# Colores
function Write-Info { param($msg) Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-OK { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Warn { param($msg) Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Err { param($msg) Write-Host "[ERROR] $msg" -ForegroundColor Red }

Write-Host ""
Write-Host "============================================================" -ForegroundColor White
Write-Host "  SINCRONIZACION DB PRODUCCION -> DESARROLLO" -ForegroundColor White
Write-Host "============================================================" -ForegroundColor White
Write-Host "  Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host "  Servidor: $RemoteUser@$RemoteServer"
Write-Host "  Destino:  $LocalDbPath"
Write-Host ""

# Verificar conexion SSH
Write-Info "Verificando conexion SSH..."
try {
    $sshTest = ssh $RemoteUser@$RemoteServer "echo OK" 2>&1
    if ($sshTest -match "OK") {
        Write-OK "Conexion SSH establecida"
    }
    else {
        Write-Err "No se pudo conectar al servidor"
        exit 1
    }
}
catch {
    Write-Err "Error de conexion SSH: $_"
    exit 1
}

# Crear directorios si no existen
if (-not (Test-Path $LocalDbDir)) {
    New-Item -ItemType Directory -Path $LocalDbDir -Force | Out-Null
}
if (-not (Test-Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir -Force | Out-Null
}

# Backup de DB local si existe
if (Test-Path $LocalDbPath) {
    $localSize = (Get-Item $LocalDbPath).Length
    $localSizeMB = [math]::Round($localSize / 1MB, 2)
    
    if (-not $Force) {
        Write-Warn "Ya existe una DB local ($localSizeMB MB)"
        $confirm = Read-Host "Desea crear backup y reemplazar? (S/N)"
        if ($confirm -notmatch "^[Ss]$") {
            Write-Info "Operacion cancelada"
            exit 0
        }
    }
    
    # Crear backup con timestamp
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupPath = "$BackupDir\sgrrhh_$timestamp.db"
    Write-Info "Creando backup: $backupPath"
    Copy-Item $LocalDbPath $backupPath -Force
    Write-OK "Backup creado"
    
    # Limpiar backups antiguos (mantener ultimos 5)
    $oldBackups = Get-ChildItem $BackupDir -Filter "*.db" | Sort-Object LastWriteTime -Descending | Select-Object -Skip 5
    foreach ($old in $oldBackups) {
        Remove-Item $old.FullName -Force
        Write-Info "Backup antiguo eliminado: $($old.Name)"
    }
}

# Eliminar archivos WAL/SHM si existen
Remove-Item "$LocalDbPath-wal" -Force -ErrorAction SilentlyContinue
Remove-Item "$LocalDbPath-shm" -Force -ErrorAction SilentlyContinue

# Copiar DB de produccion usando SCP
Write-Info "Descargando base de datos de produccion..."
Write-Info "Esto puede tomar unos segundos..."

$remoteDbPath = "${RemoteUser}@${RemoteServer}:C:/SGRRHH/Data/sgrrhh.db"
scp $remoteDbPath $LocalDbPath

if ($LASTEXITCODE -ne 0) {
    Write-Err "Error al copiar la base de datos (codigo: $LASTEXITCODE)"
    exit 1
}

if (-not (Test-Path $LocalDbPath)) {
    Write-Err "La base de datos no se descargo correctamente"
    exit 1
}

Write-OK "Base de datos descargada"

# Verificar integridad
Write-Info "Verificando integridad de la base de datos..."
try {
    $integrityCheck = sqlite3 $LocalDbPath "PRAGMA integrity_check;" 2>&1
    if ($integrityCheck -eq "ok") {
        Write-OK "Base de datos verificada correctamente"
    }
    else {
        Write-Warn "Advertencia en verificacion: $integrityCheck"
    }
}
catch {
    Write-Warn "No se pudo verificar integridad (sqlite3 no disponible)"
}

# Mostrar estadisticas
$newSize = (Get-Item $LocalDbPath).Length
$newSizeMB = [math]::Round($newSize / 1MB, 2)

try {
    $tableCount = sqlite3 $LocalDbPath "SELECT COUNT(*) FROM sqlite_master WHERE type='table';" 2>&1
}
catch {
    $tableCount = "N/A"
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host "  SINCRONIZACION COMPLETADA" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host "  Tamano: $newSizeMB MB"
Write-Host "  Tablas: $tableCount"
Write-Host "  Ubicacion: $LocalDbPath"
Write-Host ""

# Si se pidio watch, iniciar dotnet watch
if ($Watch) {
    Write-Info "Iniciando dotnet watch..."
    Set-Location $ProjectPath
    dotnet watch
}
