# ============================================================================
# DEPLOY SGRRHH A SERVIDOR REMOTO VIA SSH
# ============================================================================
# Descripcion: Compila y despliega la aplicacion al servidor via SSH/SCP
# Fecha: 2026-01-11
# ============================================================================

param(
    [switch]$SkipBuild,       # Saltar compilacion (usar ultimo build)
    [switch]$Force,           # No preguntar confirmacion
    [switch]$IncludeDatabase  # Incluir base de datos (PELIGROSO - solo primera vez)
)

# ============================================================================
# CONFIGURACION
# ============================================================================
$ErrorActionPreference = "Stop"

# Servidor remoto
$SSH_USER = "equipo1"
$SSH_HOST = "192.168.1.248"
$SSH_TARGET = "${SSH_USER}@${SSH_HOST}"

# Rutas
$PROJECT_ROOT = Split-Path -Parent $PSScriptRoot
$PROJECT_PATH = "$PROJECT_ROOT\SGRRHH.Local.Server\SGRRHH.Local.Server.csproj"
$PUBLISH_PATH = "$PROJECT_ROOT\publish-ssh"
$REMOTE_PATH = "C:\SGRRHH"
$REMOTE_DATA_PATH = "C:\SGRRHH\Data"

# Archivos a excluir del deploy (base de datos, logs, etc.)
$EXCLUDE_FILES = @(
    "sgrrhh.db",
    "sgrrhh.db-shm",
    "sgrrhh.db-wal",
    "*.log",
    "appsettings.Development.json"
)

# Ejecutable
$EXECUTABLE_NAME = "SGRRHH.Local.Server.exe"

# ============================================================================
# FUNCIONES
# ============================================================================

function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host " $Text" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Cyan
}

function Write-Step {
    param([string]$Text)
    Write-Host ""
    Write-Host "[*] $Text" -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Text)
    Write-Host "[OK] $Text" -ForegroundColor Green
}

function Write-Error {
    param([string]$Text)
    Write-Host "[ERROR] $Text" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Text)
    Write-Host "[!] $Text" -ForegroundColor Magenta
}

function Test-SSHConnection {
    Write-Step "Verificando conexion SSH a $SSH_TARGET..."
    
    try {
        $result = ssh $SSH_TARGET "echo OK" 2>&1
        if ($result -match "OK") {
            Write-Success "Conexion SSH establecida"
            return $true
        } else {
            Write-Error "No se pudo conectar: $result"
            return $false
        }
    }
    catch {
        Write-Error "Error de conexion SSH: $_"
        return $false
    }
}

function Test-RemoteDatabase {
    Write-Step "Verificando base de datos en servidor remoto..."
    
    $dbExists = ssh $SSH_TARGET "if exist $REMOTE_DATA_PATH\sgrrhh.db (echo EXISTS) else (echo NOTFOUND)" 2>&1
    
    if ($dbExists -match "EXISTS") {
        Write-Success "Base de datos encontrada en servidor"
        return $true
    } else {
        Write-Warning "Base de datos NO encontrada en servidor"
        Write-Warning "Deberas copiar la base de datos manualmente la primera vez"
        return $false
    }
}

function Ensure-RemoteFolders {
    Write-Step "Verificando carpetas requeridas en servidor..."
    
    # Crear carpetas necesarias si no existen
    ssh $SSH_TARGET "if not exist $REMOTE_DATA_PATH\Fotos mkdir $REMOTE_DATA_PATH\Fotos" 2>&1 | Out-Null
    ssh $SSH_TARGET "if not exist $REMOTE_DATA_PATH\Backups mkdir $REMOTE_DATA_PATH\Backups" 2>&1 | Out-Null
    
    Write-Success "Carpetas verificadas (Data, Fotos, Backups)"
}

function Stop-RemoteProcess {
    Write-Step "Deteniendo proceso remoto si esta corriendo..."
    
    $result = ssh $SSH_TARGET "tasklist /FI `"IMAGENAME eq $EXECUTABLE_NAME`" 2>nul | findstr $EXECUTABLE_NAME" 2>&1
    
    if ($result -match $EXECUTABLE_NAME) {
        Write-Host "    Proceso encontrado, deteniendo..." -ForegroundColor Gray
        ssh $SSH_TARGET "taskkill /F /IM $EXECUTABLE_NAME" 2>&1 | Out-Null
        Start-Sleep -Seconds 2
        Write-Success "Proceso detenido"
    } else {
        Write-Host "    No hay proceso corriendo" -ForegroundColor Gray
    }
}

function Build-Application {
    Write-Step "Compilando aplicacion..."
    
    # Limpiar carpeta de publicacion
    if (Test-Path $PUBLISH_PATH) {
        Remove-Item -Path $PUBLISH_PATH -Recurse -Force
    }
    
    # Compilar
    $buildOutput = dotnet publish $PROJECT_PATH `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -o $PUBLISH_PATH `
        /p:PublishSingleFile=false `
        /p:IncludeNativeLibrariesForSelfExtract=true `
        2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error en compilacion:"
        Write-Host $buildOutput -ForegroundColor Red
        return $false
    }
    
    Write-Success "Compilacion completada"
    return $true
}

function Remove-ExcludedFiles {
    Write-Step "Removiendo archivos excluidos del build..."
    
    foreach ($pattern in $EXCLUDE_FILES) {
        $files = Get-ChildItem -Path $PUBLISH_PATH -Filter $pattern -Recurse -ErrorAction SilentlyContinue
        foreach ($file in $files) {
            Remove-Item -Path $file.FullName -Force
            Write-Host "    Excluido: $($file.Name)" -ForegroundColor Gray
        }
    }
    
    Write-Success "Archivos excluidos removidos"
}

function Deploy-ToServer {
    Write-Step "Copiando archivos al servidor via SCP..."
    
    $startTime = Get-Date
    
    # Contar archivos a copiar
    $fileCount = (Get-ChildItem -Path $PUBLISH_PATH -Recurse -File).Count
    Write-Host "    Archivos a transferir: $fileCount" -ForegroundColor Gray
    
    # Copiar usando SCP
    # Nota: scp -r copia recursivamente
    $scpResult = scp -r "$PUBLISH_PATH\*" "${SSH_TARGET}:$REMOTE_PATH/" 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error en SCP: $scpResult"
        return $false
    }
    
    $elapsed = (Get-Date) - $startTime
    Write-Success "Archivos copiados en $($elapsed.TotalSeconds.ToString('F1')) segundos"
    
    return $true
}

function Copy-DatabaseToServer {
    if (-not $IncludeDatabase) {
        return
    }
    
    Write-Step "Copiando base de datos al servidor (SOLO PRIMERA VEZ)..."
    
    $localDb = "C:\SGRRHH\Data\sgrrhh.db"
    
    if (-not (Test-Path $localDb)) {
        Write-Warning "Base de datos local no encontrada en $localDb"
        return
    }
    
    # Confirmar
    if (-not $Force) {
        $confirm = Read-Host "ADVERTENCIA: Esto SOBRESCRIBIRA la base de datos remota. Continuar? (s/N)"
        if ($confirm -ne "s") {
            Write-Host "    Operacion cancelada" -ForegroundColor Gray
            return
        }
    }
    
    scp $localDb "${SSH_TARGET}:$REMOTE_DATA_PATH/" 2>&1 | Out-Null
    Write-Success "Base de datos copiada"
}

function Show-Summary {
    param([datetime]$StartTime)
    
    $elapsed = (Get-Date) - $StartTime
    
    Write-Header "DEPLOY COMPLETADO"
    
    Write-Host ""
    Write-Host "  Servidor:      $SSH_HOST" -ForegroundColor White
    Write-Host "  Destino:       $REMOTE_PATH" -ForegroundColor White
    Write-Host "  Tiempo total:  $($elapsed.TotalSeconds.ToString('F1')) segundos" -ForegroundColor White
    Write-Host ""
    
    # Verificar archivos en servidor
    Write-Host "  Archivos en servidor:" -ForegroundColor Yellow
    $remoteFiles = ssh $SSH_TARGET "dir /b $REMOTE_PATH 2>nul" 2>&1
    $fileCount = ($remoteFiles | Measure-Object -Line).Lines
    Write-Host "    $fileCount archivos/carpetas en destino" -ForegroundColor Gray
    
    Write-Host ""
    Write-Host "  Para iniciar la app en el servidor:" -ForegroundColor Yellow
    Write-Host "    - Usar el acceso directo 'SGRRHH' en el escritorio" -ForegroundColor Gray
    Write-Host "    - O ejecutar: ssh $SSH_TARGET `"C:\SGRRHH\$EXECUTABLE_NAME`"" -ForegroundColor Gray
    Write-Host ""
}

# ============================================================================
# EJECUCION PRINCIPAL
# ============================================================================

$totalStartTime = Get-Date

Write-Header "DEPLOY SGRRHH A SERVIDOR REMOTO"

# 1. Verificar conexion SSH
if (-not (Test-SSHConnection)) {
    Write-Error "No se puede continuar sin conexion SSH"
    exit 1
}

# 2. Verificar base de datos remota
$dbExists = Test-RemoteDatabase

# 3. Asegurar carpetas requeridas existen
Ensure-RemoteFolders

# 4. Detener proceso remoto
Stop-RemoteProcess

# 5. Compilar (si no se omite)
if (-not $SkipBuild) {
    if (-not (Build-Application)) {
        Write-Error "Error en compilacion, abortando deploy"
        exit 1
    }
} else {
    Write-Step "Saltando compilacion (usando ultimo build)"
    if (-not (Test-Path $PUBLISH_PATH)) {
        Write-Error "No existe build previo en $PUBLISH_PATH"
        exit 1
    }
}

# 6. Remover archivos excluidos
Remove-ExcludedFiles

# 7. Copiar al servidor
if (-not (Deploy-ToServer)) {
    Write-Error "Error en deploy, abortando"
    exit 1
}

# 8. Copiar base de datos si se solicita
if ($IncludeDatabase) {
    Copy-DatabaseToServer
}

# 8. Mostrar resumen
Show-Summary -StartTime $totalStartTime

Write-Host "Deploy completado exitosamente!" -ForegroundColor Green
Write-Host ""
