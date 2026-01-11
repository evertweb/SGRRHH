# Deploy-Production.ps1
# Script seguro para desplegar a produccion con backup automatico

param(
    [switch]$SkipBackup,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# === CONFIGURACION ===
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$serverProject = Join-Path $projectRoot "SGRRHH.Local.Server\SGRRHH.Local.Server.csproj"
$publishPath = Join-Path $projectRoot "publish"
$productionPath = "C:\SGRRHH"
$dataPath = "C:\SGRRHH\Data"
$backupPath = "C:\SGRRHH\Data\Backups"
$rollbackPath = "C:\SGRRHH.rollback"

# === FUNCIONES ===
function Write-Log {
    param([string]$Message, [string]$Color = "White")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] $Message" -ForegroundColor $Color
    
    # Tambien guardar en log
    $logFile = Join-Path $productionPath "Logs\deploy.log"
    if (Test-Path (Split-Path $logFile)) {
        Add-Content -Path $logFile -Value "[$timestamp] $Message"
    }
}

function Test-ServerRunning {
    $proc = Get-Process -Name "SGRRHH.Local.Server" -ErrorAction SilentlyContinue
    return $null -ne $proc
}

function Stop-Server {
    Write-Log "Deteniendo servidor..." "Yellow"
    $proc = Get-Process -Name "SGRRHH.Local.Server" -ErrorAction SilentlyContinue
    if ($proc) {
        $proc | Stop-Process -Force
        Start-Sleep -Seconds 3
        Write-Log "Servidor detenido" "Green"
        return $true
    }
    Write-Log "El servidor no estaba corriendo" "Gray"
    return $false
}

function Start-Server {
    Write-Log "Iniciando servidor..." "Yellow"
    $exePath = Join-Path $productionPath "SGRRHH.Local.Server.exe"
    if (Test-Path $exePath) {
        Start-Process -FilePath $exePath -WorkingDirectory $productionPath
        Start-Sleep -Seconds 3
        if (Test-ServerRunning) {
            Write-Log "Servidor iniciado correctamente" "Green"
            return $true
        }
    }
    Write-Log "Error al iniciar el servidor" "Red"
    return $false
}

function Backup-Database {
    Write-Log "Creando backup de base de datos..." "Yellow"
    
    $dbFile = Join-Path $dataPath "sgrrhh.db"
    if (-not (Test-Path $dbFile)) {
        Write-Log "No existe base de datos para respaldar" "Gray"
        return $true
    }
    
    # Crear directorio de backups si no existe
    if (-not (Test-Path $backupPath)) {
        New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
    }
    
    # Nombre del backup con timestamp
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFile = Join-Path $backupPath "sgrrhh_$timestamp.db"
    
    # Copiar base de datos
    Copy-Item -Path $dbFile -Destination $backupFile -Force
    
    # Verificar integridad del backup
    $originalHash = (Get-FileHash $dbFile -Algorithm SHA256).Hash
    $backupHash = (Get-FileHash $backupFile -Algorithm SHA256).Hash
    
    if ($originalHash -eq $backupHash) {
        Write-Log "Backup creado: $backupFile" "Green"
        
        # Limpiar backups antiguos (mantener ultimos 10)
        $oldBackups = Get-ChildItem $backupPath -Filter "*.db" | 
                      Sort-Object LastWriteTime -Descending | 
                      Select-Object -Skip 10
        if ($oldBackups) {
            $oldBackups | Remove-Item -Force
            Write-Log "Backups antiguos eliminados: $($oldBackups.Count)" "Gray"
        }
        return $true
    } else {
        Write-Log "ERROR: El backup no coincide con el original" "Red"
        Remove-Item $backupFile -Force
        return $false
    }
}

function Backup-Application {
    Write-Log "Creando punto de restauracion de la aplicacion..." "Yellow"
    
    if (Test-Path $rollbackPath) {
        Remove-Item $rollbackPath -Recurse -Force
    }
    
    # Copiar solo archivos de aplicacion (no datos)
    $excludeFolders = @("Data", "Logs", "Storage", "Backups")
    
    New-Item -ItemType Directory -Path $rollbackPath -Force | Out-Null
    
    Get-ChildItem $productionPath -Exclude $excludeFolders | ForEach-Object {
        Copy-Item $_.FullName -Destination $rollbackPath -Recurse -Force
    }
    
    Write-Log "Punto de restauracion creado en $rollbackPath" "Green"
    return $true
}

function Restore-Application {
    Write-Log "RESTAURANDO aplicacion desde punto de restauracion..." "Red"
    
    if (-not (Test-Path $rollbackPath)) {
        Write-Log "No hay punto de restauracion disponible" "Red"
        return $false
    }
    
    # Copiar archivos de rollback
    Get-ChildItem $rollbackPath | ForEach-Object {
        Copy-Item $_.FullName -Destination $productionPath -Recurse -Force
    }
    
    Write-Log "Aplicacion restaurada" "Green"
    return $true
}

function Deploy-NewVersion {
    Write-Log "Desplegando nueva version..." "Yellow"
    
    # Carpetas a preservar
    $preserveFolders = @("Data", "Logs", "Storage")
    
    # Eliminar archivos antiguos (excepto datos)
    Get-ChildItem $productionPath -Exclude $preserveFolders | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    
    # Copiar nuevos archivos
    Copy-Item -Path "$publishPath\*" -Destination $productionPath -Recurse -Force
    
    # Crear carpetas de datos si no existen
    foreach ($folder in $preserveFolders) {
        $folderPath = Join-Path $productionPath $folder
        if (-not (Test-Path $folderPath)) {
            New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
        }
    }
    
    Write-Log "Archivos desplegados correctamente" "Green"
    return $true
}

function Test-Application {
    Write-Log "Verificando aplicacion..." "Yellow"
    
    # Verificar que el ejecutable existe
    $exePath = Join-Path $productionPath "SGRRHH.Local.Server.exe"
    if (-not (Test-Path $exePath)) {
        Write-Log "ERROR: No se encuentra el ejecutable" "Red"
        return $false
    }
    
    # Verificar DLLs principales
    $requiredDlls = @(
        "SGRRHH.Local.Server.dll",
        "SGRRHH.Local.Domain.dll",
        "SGRRHH.Local.Infrastructure.dll"
    )
    
    foreach ($dll in $requiredDlls) {
        $dllPath = Join-Path $productionPath $dll
        if (-not (Test-Path $dllPath)) {
            Write-Log "ERROR: Falta DLL requerida: $dll" "Red"
            return $false
        }
    }
    
    Write-Log "Verificacion completada" "Green"
    return $true
}

# === INICIO DEL DESPLIEGUE ===
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " DESPLIEGUE A PRODUCCION - SGRRHH LOCAL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar proyecto
if (-not (Test-Path $serverProject)) {
    Write-Log "ERROR: No se encuentra el proyecto: $serverProject" "Red"
    exit 1
}

# Confirmar si hay servidor corriendo
if (Test-ServerRunning) {
    if (-not $Force) {
        Write-Host ""
        Write-Host "ADVERTENCIA: El servidor esta corriendo." -ForegroundColor Yellow
        Write-Host "El despliegue lo detendra temporalmente." -ForegroundColor Yellow
        Write-Host ""
        $confirm = Read-Host "Continuar? (S/N)"
        if ($confirm -ne "S" -and $confirm -ne "s") {
            Write-Log "Despliegue cancelado por el usuario" "Yellow"
            exit 0
        }
    }
}

# Paso 1: Compilar
Write-Log "=== PASO 1: COMPILACION ===" "Cyan"
Write-Log "Compilando proyecto en Release..."

$buildResult = dotnet publish $serverProject -c Release -o $publishPath --nologo 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Log "ERROR: Fallo la compilacion" "Red"
    Write-Host $buildResult -ForegroundColor Red
    exit 1
}
Write-Log "Compilacion exitosa" "Green"

# Paso 2: Backup de base de datos
if (-not $SkipBackup) {
    Write-Log "=== PASO 2: BACKUP ===" "Cyan"
    if (-not (Backup-Database)) {
        Write-Log "ERROR: Fallo el backup de la base de datos" "Red"
        exit 1
    }
}

# Paso 3: Detener servidor
Write-Log "=== PASO 3: DETENER SERVIDOR ===" "Cyan"
$wasRunning = Stop-Server

# Paso 4: Backup de aplicacion
Write-Log "=== PASO 4: PUNTO DE RESTAURACION ===" "Cyan"
if (Test-Path $productionPath) {
    if (-not (Backup-Application)) {
        Write-Log "ERROR: No se pudo crear punto de restauracion" "Red"
        if ($wasRunning) { Start-Server }
        exit 1
    }
}

# Paso 5: Desplegar
Write-Log "=== PASO 5: DESPLIEGUE ===" "Cyan"
if (-not (Deploy-NewVersion)) {
    Write-Log "ERROR: Fallo el despliegue. Restaurando..." "Red"
    Restore-Application
    if ($wasRunning) { Start-Server }
    exit 1
}

# Paso 6: Verificar
Write-Log "=== PASO 6: VERIFICACION ===" "Cyan"
if (-not (Test-Application)) {
    Write-Log "ERROR: Verificacion fallida. Restaurando..." "Red"
    Restore-Application
    if ($wasRunning) { Start-Server }
    exit 1
}

# Paso 7: Crear version.json
$versionInfo = @{
    version = "1.0.0"
    deployDate = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    environment = "Production"
}
$versionInfo | ConvertTo-Json | Set-Content (Join-Path $productionPath "version.json")

# Paso 8: Iniciar servidor
Write-Log "=== PASO 7: INICIAR SERVIDOR ===" "Cyan"
if ($wasRunning -or $Force) {
    if (Start-Server) {
        Start-Sleep -Seconds 2
        Start-Process "https://localhost:5003"
    }
} else {
    Write-Host ""
    $startNow = Read-Host "Iniciar el servidor ahora? (S/N)"
    if ($startNow -eq "S" -or $startNow -eq "s") {
        Start-Server
        Start-Sleep -Seconds 2
        Start-Process "https://localhost:5003"
    }
}

# Resumen
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " DESPLIEGUE COMPLETADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Ubicacion: $productionPath" -ForegroundColor Gray
Write-Host "URL: https://localhost:5003" -ForegroundColor Gray
Write-Host ""
Write-Host "Si hay problemas, ejecute:" -ForegroundColor Yellow
Write-Host "  .\Restore-Backup.ps1" -ForegroundColor Yellow
Write-Host ""
