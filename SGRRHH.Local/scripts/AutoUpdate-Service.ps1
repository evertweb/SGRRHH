# AutoUpdate-Service.ps1
# EJECUTAR EN EL PC SERVIDOR
# Este script verifica actualizaciones y las aplica automaticamente
# Debe ejecutarse como Tarea Programada cada hora (o al intervalo deseado)

param(
    [string]$UpdateSource = "\\DESARROLLO\SGRRHHUpdates",  # Cambiar al nombre de tu PC
    [string]$InstallPath = "C:\SGRRHH",
    [switch]$Force,
    [switch]$CheckOnly
)

$ErrorActionPreference = "Stop"

# Configuracion
$dataPath = Join-Path $InstallPath "Data"
$backupPath = Join-Path $dataPath "Backups"
$logsPath = Join-Path $InstallPath "Logs"
$logFile = Join-Path $logsPath "autoupdate.log"
$localVersionFile = Join-Path $InstallPath "version.json"
$remoteVersionFile = Join-Path $UpdateSource "version.json"
$remoteLatesPath = Join-Path $UpdateSource "latest"

# Funcion de logging
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    # Crear directorio de logs si no existe
    if (-not (Test-Path $logsPath)) {
        New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
    }
    
    Add-Content -Path $logFile -Value $logMessage -ErrorAction SilentlyContinue
    
    $color = switch ($Level) {
        "ERROR" { "Red" }
        "WARN"  { "Yellow" }
        "OK"    { "Green" }
        default { "White" }
    }
    Write-Host $logMessage -ForegroundColor $color
}

function Get-LocalVersion {
    if (Test-Path $localVersionFile) {
        $data = Get-Content $localVersionFile -Raw | ConvertFrom-Json
        return $data.version
    }
    return "0.0.0"
}

function Get-RemoteVersion {
    if (Test-Path $remoteVersionFile) {
        $data = Get-Content $remoteVersionFile -Raw | ConvertFrom-Json
        return @{
            Version = $data.version
            Notes = $data.releaseNotes
            Date = $data.publishDate
            Mandatory = $data.mandatory
        }
    }
    return $null
}

function Compare-Versions {
    param([string]$Local, [string]$Remote)
    
    $localParts = $Local.Split('.') | ForEach-Object { [int]$_ }
    $remoteParts = $Remote.Split('.') | ForEach-Object { [int]$_ }
    
    for ($i = 0; $i -lt 3; $i++) {
        $l = if ($i -lt $localParts.Count) { $localParts[$i] } else { 0 }
        $r = if ($i -lt $remoteParts.Count) { $remoteParts[$i] } else { 0 }
        
        if ($r -gt $l) { return 1 }   # Hay actualizacion
        if ($r -lt $l) { return -1 }  # Local es mas reciente
    }
    return 0  # Misma version
}

function Backup-Database {
    $dbFile = Join-Path $dataPath "sgrrhh.db"
    
    if (-not (Test-Path $dbFile)) {
        Write-Log "No hay base de datos para respaldar" "WARN"
        return $true
    }
    
    if (-not (Test-Path $backupPath)) {
        New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
    }
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFile = Join-Path $backupPath "sgrrhh_pre_update_$timestamp.db"
    
    Copy-Item $dbFile $backupFile -Force
    
    # Verificar
    $origHash = (Get-FileHash $dbFile -Algorithm MD5).Hash
    $backHash = (Get-FileHash $backupFile -Algorithm MD5).Hash
    
    if ($origHash -eq $backHash) {
        Write-Log "Backup creado: $backupFile" "OK"
        
        # Limpiar backups antiguos (mantener 20)
        Get-ChildItem $backupPath -Filter "*.db" | 
            Sort-Object LastWriteTime -Descending | 
            Select-Object -Skip 20 | 
            Remove-Item -Force
        
        return $true
    }
    
    Write-Log "ERROR: Backup corrupto" "ERROR"
    return $false
}

function Stop-AppServer {
    $proc = Get-Process -Name "SGRRHH.Local.Server" -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Log "Deteniendo servidor (PID: $($proc.Id))..."
        $proc | Stop-Process -Force
        Start-Sleep -Seconds 3
        
        # Verificar que se detuvo
        $proc = Get-Process -Name "SGRRHH.Local.Server" -ErrorAction SilentlyContinue
        if ($proc) {
            Write-Log "ERROR: No se pudo detener el servidor" "ERROR"
            return $false
        }
        Write-Log "Servidor detenido" "OK"
    }
    return $true
}

function Start-AppServer {
    $exePath = Join-Path $InstallPath "SGRRHH.Local.Server.exe"
    
    if (-not (Test-Path $exePath)) {
        Write-Log "ERROR: No se encuentra el ejecutable" "ERROR"
        return $false
    }
    
    Write-Log "Iniciando servidor..."
    Start-Process -FilePath $exePath -WorkingDirectory $InstallPath
    Start-Sleep -Seconds 5
    
    $proc = Get-Process -Name "SGRRHH.Local.Server" -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Log "Servidor iniciado (PID: $($proc.Id))" "OK"
        return $true
    }
    
    Write-Log "ERROR: El servidor no inicio correctamente" "ERROR"
    return $false
}

function Apply-Update {
    Write-Log "Aplicando actualizacion..."
    
    # Carpetas a preservar
    $preserve = @("Data", "Logs", "Storage", "Backups")
    
    # Eliminar archivos antiguos (excepto datos)
    Get-ChildItem $InstallPath -Exclude $preserve | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    
    # Copiar nuevos archivos
    Copy-Item -Path "$remoteLatesPath\*" -Destination $InstallPath -Recurse -Force
    
    # Copiar version.json
    Copy-Item $remoteVersionFile $localVersionFile -Force
    
    # Verificar archivos criticos
    $requiredFiles = @(
        "SGRRHH.Local.Server.exe",
        "SGRRHH.Local.Server.dll"
    )
    
    foreach ($file in $requiredFiles) {
        $filePath = Join-Path $InstallPath $file
        if (-not (Test-Path $filePath)) {
            Write-Log "ERROR: Falta archivo critico: $file" "ERROR"
            return $false
        }
    }
    
    Write-Log "Archivos actualizados correctamente" "OK"
    return $true
}

# ============================================
# INICIO DEL PROCESO
# ============================================

Write-Log "========== AUTO-UPDATE CHECK =========="

# Verificar acceso a la fuente de actualizaciones
if (-not (Test-Path $UpdateSource)) {
    Write-Log "No se puede acceder a: $UpdateSource" "ERROR"
    Write-Log "Verifique la conexion de red" "ERROR"
    exit 1
}

# Obtener versiones
$localVersion = Get-LocalVersion
$remoteInfo = Get-RemoteVersion

if (-not $remoteInfo) {
    Write-Log "No hay version remota disponible" "WARN"
    exit 0
}

Write-Log "Version local: $localVersion"
Write-Log "Version remota: $($remoteInfo.Version)"

# Comparar
$comparison = Compare-Versions $localVersion $remoteInfo.Version

if ($comparison -eq 0 -and -not $Force) {
    Write-Log "Sistema actualizado. No hay cambios." "OK"
    exit 0
}

if ($comparison -lt 0 -and -not $Force) {
    Write-Log "La version local es mas reciente que la remota" "WARN"
    exit 0
}

# Hay actualizacion disponible
Write-Log "========================================" 
Write-Log "ACTUALIZACION DISPONIBLE" "OK"
Write-Log "De: $localVersion -> A: $($remoteInfo.Version)"
Write-Log "Notas: $($remoteInfo.Notes)"
Write-Log "========================================"

if ($CheckOnly) {
    Write-Log "Modo solo verificacion. No se aplicara la actualizacion."
    exit 0
}

# Proceso de actualizacion
Write-Log "Iniciando proceso de actualizacion..."

# 1. Backup
Write-Log "[1/4] Creando backup de seguridad..."
if (-not (Backup-Database)) {
    Write-Log "ABORTANDO: No se pudo crear backup" "ERROR"
    exit 1
}

# 2. Detener servidor
Write-Log "[2/4] Deteniendo servidor..."
$wasRunning = (Get-Process -Name "SGRRHH.Local.Server" -ErrorAction SilentlyContinue) -ne $null
if (-not (Stop-AppServer)) {
    Write-Log "ABORTANDO: No se pudo detener el servidor" "ERROR"
    exit 1
}

# 3. Aplicar actualizacion
Write-Log "[3/4] Aplicando actualizacion..."
if (-not (Apply-Update)) {
    Write-Log "ERROR en actualizacion. Intentando restaurar..." "ERROR"
    # Aqui podriamos agregar logica de rollback
    exit 1
}

# 4. Iniciar servidor
Write-Log "[4/4] Iniciando servidor..."
if (-not (Start-AppServer)) {
    Write-Log "ERROR: El servidor no inicio. Verifique manualmente." "ERROR"
    exit 1
}

# Exito
Write-Log "========================================"
Write-Log "ACTUALIZACION COMPLETADA EXITOSAMENTE" "OK"
Write-Log "Version: $($remoteInfo.Version)"
Write-Log "========================================"

# Crear registro de actualizacion
$updateLog = @{
    timestamp = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    fromVersion = $localVersion
    toVersion = $remoteInfo.Version
    notes = $remoteInfo.Notes
    success = $true
}

$updateHistoryFile = Join-Path $logsPath "update_history.json"
$history = @()
if (Test-Path $updateHistoryFile) {
    $history = Get-Content $updateHistoryFile | ConvertFrom-Json
}
$history += $updateLog
$history | ConvertTo-Json -Depth 3 | Set-Content $updateHistoryFile

exit 0
