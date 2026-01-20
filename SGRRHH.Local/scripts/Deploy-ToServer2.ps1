<#
.SYNOPSIS
    Deploy INCREMENTAL SEGURO de SGRRHH a servidor secundario via SMB/Robocopy.

.DESCRIPTION
    Script personalizado para el servidor secundario en 192.168.1.72
    Usuario: fores
    
    PROTECCIONES DE SEGURIDAD:
    - NUNCA elimina archivos del servidor que no existen en source
    - NUNCA toca las carpetas: Data, certs, logs, backups
    - NUNCA sobrescribe: *.db, *.bat, *.ps1, *.lnk, appsettings.json

.PARAMETER SkipBuild
    Omite la compilación y usa el último build existente.

.PARAMETER Force
    No pide confirmación antes de desplegar.

.PARAMETER IncludeDatabase
    Incluye la base de datos en el deploy (¡CUIDADO! Sobrescribe datos).
#>

param(
    [switch]$SkipBuild,
    [switch]$Force,
    [switch]$IncludeDatabase,
    [switch]$FullSync
)

# ============================================================================
# CONFIGURACIÓN - SERVIDOR SECUNDARIO
# ============================================================================
$ErrorActionPreference = "Stop"

$Config = @{
    # Servidor 2 (fores)
    SshUser      = "fores"
    SshHost      = "192.168.1.72"
    RemotePath   = "C:\SGRRHH"
    SharePath    = "\\192.168.1.72\SGRRHH"
    
    # Rutas locales (iguales para ambos servidores)
    SolutionDir  = Split-Path -Parent $PSScriptRoot
    ProjectDir   = Join-Path (Split-Path -Parent $PSScriptRoot) "SGRRHH.Local.Server"
    PublishDir   = Join-Path (Split-Path -Parent $PSScriptRoot) "publish-ssh"
    
    # PROTECCIÓN DEL SERVIDOR - NUNCA eliminar estos archivos/carpetas
    ExcludeFiles = @(
        "appsettings.Development.json",
        "appsettings.json",
        "*.pdb",
        "*.bat",
        "*.ps1",
        "sqlite3.exe",
        "*.lnk",
        "*.db",
        "*.db-shm",
        "*.db-wal"
    )
    
    PreserveDirs = @(
        "Data",
        "certs",
        "logs",
        "backups"
    )
}

$Config.SshTarget = "$($Config.SshUser)@$($Config.SshHost)"

# ============================================================================
# FUNCIONES
# ============================================================================

function Write-Step {
    param([string]$Message, [string]$Status = "INFO")
    $color = switch ($Status) {
        "INFO" { "Cyan" }
        "OK" { "Green" }
        "WARN" { "Yellow" }
        "ERROR" { "Red" }
        default { "White" }
    }
    Write-Host "[$Status] " -ForegroundColor $color -NoNewline
    Write-Host $Message
}

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host ("=" * 60) -ForegroundColor DarkGray
    Write-Host "  $Title" -ForegroundColor White
    Write-Host ("=" * 60) -ForegroundColor DarkGray
}

function Test-SmbConnection {
    Write-Step "Verificando conexión SMB a $($Config.SharePath)..."
    
    # Verificar si ya está conectado
    $existingConnection = net use 2>&1 | Select-String $Config.SshHost
    
    if (-not $existingConnection) {
        Write-Step "Estableciendo conexión SMB..." "WARN"
        $result = net use $Config.SharePath /persistent:yes 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "No se puede conectar al share SMB. Verifica credenciales con cmdkey."
        }
    }
    
    # Verificar acceso
    if (-not (Test-Path $Config.SharePath)) {
        throw "No se puede acceder al share: $($Config.SharePath)"
    }
    
    Write-Step "Conexión SMB establecida" "OK"
}

function Show-ServerProtectedAssets {
    Write-Step "Verificando archivos protegidos en el servidor..."
    
    $protected = @()
    
    foreach ($dir in $Config.PreserveDirs) {
        $path = Join-Path $Config.SharePath $dir
        if (Test-Path $path) {
            $itemCount = (Get-ChildItem $path -Recurse -File -ErrorAction SilentlyContinue | Measure-Object).Count
            $protected += "  [D] $dir/ ($itemCount archivos)"
        }
    }
    
    $serverFiles = Get-ChildItem $Config.SharePath -File -ErrorAction SilentlyContinue
    foreach ($file in $serverFiles) {
        $isProtected = $false
        foreach ($pattern in $Config.ExcludeFiles) {
            if ($file.Name -like $pattern) {
                $isProtected = $true
                break
            }
        }
        if ($isProtected) {
            $protected += "  [F] $($file.Name)"
        }
    }
    
    if ($protected.Count -gt 0) {
        Write-Host ""
        Write-Host "  ARCHIVOS PROTEGIDOS (no serán modificados):" -ForegroundColor Green
        foreach ($item in $protected) {
            Write-Host $item -ForegroundColor DarkGreen
        }
        Write-Host ""
    }
    
    Write-Step "Verificación completada" "OK"
}

function Test-SshConnection {
    Write-Step "Verificando conexión SSH a $($Config.SshTarget)..."
    
    $result = ssh $Config.SshTarget "echo OK" 2>&1
    if ($LASTEXITCODE -ne 0 -or $result -ne "OK") {
        throw "No se puede conectar al servidor via SSH."
    }
    
    Write-Step "Conexión SSH establecida" "OK"
}

function Stop-RemoteApp {
    Write-Step "Deteniendo aplicación remota (si está corriendo)..."
    
    # Intentar con nssm si existe
    ssh $Config.SshTarget "if (Get-Command nssm -ErrorAction SilentlyContinue) { nssm stop SGRRHH_Local 2>`$null } else { Write-Host 'NSSM no instalado, omitiendo...' }" 2>$null
    
    # Siempre intentar con taskkill como fallback
    ssh $Config.SshTarget "taskkill /F /IM SGRRHH.Local.Server.exe 2>`$null; exit 0" 2>$null
    
    Start-Sleep -Seconds 2
    Write-Step "Aplicación detenida" "OK"
}

function Build-Application {
    if ($SkipBuild) {
        Write-Step "Omitiendo compilación (usando build existente)" "WARN"
        
        if (-not (Test-Path $Config.PublishDir)) {
            throw "No existe el directorio de publicación. Ejecuta sin -SkipBuild."
        }
        return
    }
    
    Write-Step "Compilando aplicación (Release, win-x64, self-contained)..."
    
    Push-Location $Config.ProjectDir
    try {
        if (Test-Path $Config.PublishDir) {
            Remove-Item -Path $Config.PublishDir -Recurse -Force
        }
        
        $publishArgs = @(
            "publish"
            "-c", "Release"
            "-r", "win-x64"
            "--self-contained", "true"
            "-o", $Config.PublishDir
            "/p:PublishSingleFile=false"
            "/p:IncludeNativeLibrariesForSelfExtract=true"
        )
        
        & dotnet @publishArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Error en la compilación"
        }
        
        Write-Step "Compilación exitosa" "OK"
    }
    finally {
        Pop-Location
    }
}

function Remove-ExcludedFiles {
    Write-Step "Eliminando archivos excluidos del paquete local..."
    
    foreach ($pattern in $Config.ExcludeFiles) {
        $files = Get-ChildItem -Path $Config.PublishDir -Filter $pattern -Recurse -ErrorAction SilentlyContinue
        foreach ($file in $files) {
            Remove-Item $file.FullName -Force
            Write-Host "  - Eliminado: $($file.Name)" -ForegroundColor DarkGray
        }
    }
}

function Deploy-WithRobocopy {
    Write-Step "Sincronizando archivos con robocopy (solo cambios)..."
    
    $source = $Config.PublishDir
    $dest = $Config.SharePath
    
    $robocopyCmd = "robocopy `"$source`" `"$dest`" /E /XO /XX /NP /NDL /NJH /R:2 /W:1 /IT"
    
    foreach ($dir in $Config.PreserveDirs) {
        $robocopyCmd += " /XD `"$dest\$dir`""
    }
    
    foreach ($file in $Config.ExcludeFiles) {
        $robocopyCmd += " /XF `"$file`""
    }
    
    if ($FullSync) {
        Write-Step "[!] Modo FULL SYNC - usando /MIR (puede eliminar extras)" "WARN"
        $robocopyCmd = $robocopyCmd -replace "/E /XO /XX", "/MIR"
    }
    
    Write-Host ""
    if ($FullSync) {
        Write-Host "  Modo: FULL SYNC (/MIR) - PELIGROSO" -ForegroundColor Yellow
    }
    else {
        Write-Host "  Modo: INCREMENTAL SEGURO (/E /XO /XX)" -ForegroundColor Green
    }
    Write-Host ""
    
    $output = cmd /c $robocopyCmd 2>&1
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -ge 8) {
        Write-Host ($output -join "`n") -ForegroundColor Red
        throw "Error en robocopy (código $exitCode)"
    }
    
    $copied = ($output | Select-String "Newer|New File|Modified|newer" | Measure-Object).Count
    $skipped = ($output | Select-String "same|older" | Measure-Object).Count
    
    Write-Host ""
    Write-Host "  Archivos actualizados: $copied" -ForegroundColor Green
    Write-Host "  Archivos sin cambio:   $skipped" -ForegroundColor Gray  
    Write-Host ""
    
    Write-Step "Sincronización completada" "OK"
}

function Copy-Database {
    if (-not $IncludeDatabase) {
        return
    }
    
    Write-Step "Copiando base de datos..." "WARN"
    
    $localDb = "C:\SGRRHH\Data\sgrrhh.db"
    $remoteDataDir = "$($Config.RemotePath)\Data"
    
    if (-not (Test-Path $localDb)) {
        Write-Step "No se encontró base de datos local en $localDb" "WARN"
        return
    }
    
    ssh $Config.SshTarget "if not exist `"$remoteDataDir`" mkdir `"$remoteDataDir`""
    scp $localDb "$($Config.SshTarget):$($remoteDataDir.Replace('\', '/'))/  "
    
    Write-Step "Base de datos copiada" "OK"
}

function Show-Summary {
    Write-Header "DEPLOY COMPLETADO - SERVIDOR 2"
    
    Write-Host ""
    Write-Host "  Servidor: " -NoNewline -ForegroundColor Gray
    Write-Host $Config.SshTarget -ForegroundColor White
    
    Write-Host "  Ruta:     " -NoNewline -ForegroundColor Gray
    Write-Host $Config.RemotePath -ForegroundColor White
    
    Write-Host ""
    Write-Host "  Para iniciar la aplicación:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "    ssh $($Config.SshTarget)" -ForegroundColor Cyan
    Write-Host "    cd $($Config.RemotePath)" -ForegroundColor Cyan
    Write-Host "    .\SGRRHH.Local.Server.exe" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  URLs:" -ForegroundColor Yellow
    Write-Host "    HTTP:  http://$($Config.SshHost):5002" -ForegroundColor Gray
    Write-Host "    HTTPS: https://$($Config.SshHost):5003" -ForegroundColor Gray
    Write-Host ""
}

# ============================================================================
# EJECUCIÓN PRINCIPAL
# ============================================================================

try {
    Write-Header "DEPLOY INCREMENTAL SGRRHH - SERVIDOR 2"
    Write-Host "  Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
    Write-Host "  Destino: $($Config.SharePath)" -ForegroundColor Gray
    Write-Host "  Método: Robocopy (solo archivos modificados)" -ForegroundColor Gray
    
    if ($FullSync) {
        Write-Host ""
        Write-Host "  [!] ADVERTENCIA: Modo -FullSync activado" -ForegroundColor Red
        Write-Host "      Esto usará /MIR que puede ELIMINAR archivos del servidor." -ForegroundColor Red
        Write-Host "      Las carpetas protegidas ($($Config.PreserveDirs -join ', ')) seguirán excluidas." -ForegroundColor Yellow
        Write-Host ""
        
        if (-not $Force) {
            $confirmFull = Read-Host "¿Estás SEGURO de usar FullSync? (escribe 'SI' para confirmar)"
            if ($confirmFull -ne "SI") {
                Write-Step "Deploy cancelado por el usuario" "WARN"
                exit 0
            }
        }
    }
    
    if (-not $Force) {
        Write-Host ""
        $confirm = Read-Host "¿Continuar con el deploy? (S/N)"
        if ($confirm -notin @("S", "s", "Y", "y", "Si", "si", "SI")) {
            Write-Step "Deploy cancelado por el usuario" "WARN"
            exit 0
        }
    }
    
    # Paso 1: Verificar conexión SMB
    Test-SmbConnection
    
    # Paso 1.5: Mostrar archivos protegidos del servidor
    Show-ServerProtectedAssets
    
    # Paso 2: Verificar conexión SSH
    Test-SshConnection
    
    # Paso 3: Detener app remota
    Stop-RemoteApp
    
    # Paso 4: Compilar
    Build-Application
    
    # Paso 5: Limpiar archivos excluidos localmente
    Remove-ExcludedFiles
    
    # Paso 6: Sincronizar con robocopy
    Deploy-WithRobocopy
    
    # Paso 7: Copiar DB si se solicitó
    Copy-Database
    
    # Resumen
    Show-Summary
    
    exit 0
}
catch {
    Write-Host ""
    Write-Step $_.Exception.Message "ERROR"
    Write-Host ""
    exit 1
}
