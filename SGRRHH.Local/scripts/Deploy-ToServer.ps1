<#
.SYNOPSIS
    Deploy de SGRRHH a servidor remoto via SSH.

.DESCRIPTION
    Compila la aplicación en modo Release (self-contained win-x64),
    la empaqueta y la despliega al servidor via SSH/SCP.
    NO configura servicios - la app debe arrancarse manualmente.

.PARAMETER SkipBuild
    Omite la compilación y usa el último build existente.

.PARAMETER Force
    No pide confirmación antes de desplegar.

.PARAMETER IncludeDatabase
    Incluye la base de datos en el deploy (¡CUIDADO! Sobrescribe datos).

.EXAMPLE
    .\Deploy-ToServer.ps1
    .\Deploy-ToServer.ps1 -SkipBuild
    .\Deploy-ToServer.ps1 -Force -IncludeDatabase
#>

param(
    [switch]$SkipBuild,
    [switch]$Force,
    [switch]$IncludeDatabase
)

# ============================================================================
# CONFIGURACIÓN
# ============================================================================
$ErrorActionPreference = "Stop"

$Config = @{
    # Servidor remoto
    SshUser        = "equipo1"
    SshHost        = "192.168.1.248"
    RemotePath     = "C:\SGRRHH"
    
    # Rutas locales
    SolutionDir    = Split-Path -Parent $PSScriptRoot
    ProjectDir     = Join-Path (Split-Path -Parent $PSScriptRoot) "SGRRHH.Local.Server"
    PublishDir     = Join-Path (Split-Path -Parent $PSScriptRoot) "publish-ssh"
    
    # Archivos/carpetas a excluir del deploy
    ExcludeItems   = @(
        "appsettings.Development.json",
        "*.pdb"
    )
    
    # Carpetas a preservar en el servidor (no se eliminan)
    PreserveDirs   = @("Data", "certs", "logs")
}

$Config.SshTarget = "$($Config.SshUser)@$($Config.SshHost)"

# ============================================================================
# FUNCIONES
# ============================================================================

function Write-Step {
    param([string]$Message, [string]$Status = "INFO")
    $color = switch ($Status) {
        "INFO"    { "Cyan" }
        "OK"      { "Green" }
        "WARN"    { "Yellow" }
        "ERROR"   { "Red" }
        default   { "White" }
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

function Test-SshConnection {
    Write-Step "Verificando conexión SSH a $($Config.SshTarget)..."
    
    $result = ssh $Config.SshTarget "echo OK" 2>&1
    if ($LASTEXITCODE -ne 0 -or $result -ne "OK") {
        throw "No se puede conectar al servidor. Verifica que SSH esté configurado."
    }
    
    Write-Step "Conexión SSH establecida" "OK"
}

function Stop-RemoteApp {
    Write-Step "Deteniendo aplicación remota (si está corriendo)..."
    
    # Intentar detener servicio nssm si existe
    ssh $Config.SshTarget "nssm stop SGRRHH_Local 2>nul" 2>$null
    
    # Matar proceso directamente
    ssh $Config.SshTarget "taskkill /F /IM SGRRHH.Local.Server.exe 2>nul" 2>$null
    
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
        # Limpiar directorio de publicación
        if (Test-Path $Config.PublishDir) {
            Remove-Item -Path $Config.PublishDir -Recurse -Force
        }
        
        # Publicar
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
    Write-Step "Eliminando archivos excluidos del paquete..."
    
    foreach ($pattern in $Config.ExcludeItems) {
        $files = Get-ChildItem -Path $Config.PublishDir -Filter $pattern -Recurse -ErrorAction SilentlyContinue
        foreach ($file in $files) {
            Remove-Item $file.FullName -Force
            Write-Host "  - Eliminado: $($file.Name)" -ForegroundColor DarkGray
        }
    }
}

function Create-DeployPackage {
    Write-Step "Creando paquete de deploy..."
    
    $zipPath = Join-Path $Config.SolutionDir "deploy-package.zip"
    
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    
    Compress-Archive -Path "$($Config.PublishDir)\*" -DestinationPath $zipPath -Force
    
    $sizeMB = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
    Write-Step "Paquete creado: $sizeMB MB" "OK"
    
    return $zipPath
}

function Deploy-ToServer {
    param([string]$ZipPath)
    
    $remoteZip = "$($Config.RemotePath)\deploy-package.zip"
    
    # Crear directorio remoto si no existe
    Write-Step "Preparando directorio remoto..."
    ssh $Config.SshTarget "if not exist `"$($Config.RemotePath)`" mkdir `"$($Config.RemotePath)`""
    
    # Copiar ZIP al servidor
    Write-Step "Copiando paquete al servidor..."
    scp $ZipPath "$($Config.SshTarget):$($Config.RemotePath.Replace('\', '/'))/"
    
    if ($LASTEXITCODE -ne 0) {
        throw "Error al copiar el paquete"
    }
    
    # Limpiar archivos antiguos (preservando Data, certs, logs)
    Write-Step "Limpiando archivos antiguos (preservando Data, certs, logs)..."
    
    $preserveList = ($Config.PreserveDirs + @("deploy-package.zip")) -join "|"
    
    $cleanupScript = @"
cd /d "$($Config.RemotePath)"
for /D %%d in (*) do (
    echo %%d | findstr /i /r "$preserveList" >nul || rd /s /q "%%d"
)
for %%f in (*) do (
    echo %%f | findstr /i /r "$preserveList" >nul || del /q "%%f"
)
"@
    
    ssh $Config.SshTarget "cmd /c `"$cleanupScript`"" 2>$null
    
    # Descomprimir
    Write-Step "Descomprimiendo en servidor..."
    ssh $Config.SshTarget "powershell -Command `"Expand-Archive -Path '$remoteZip' -DestinationPath '$($Config.RemotePath)' -Force`""
    
    # Eliminar ZIP remoto
    ssh $Config.SshTarget "del `"$remoteZip`""
    
    Write-Step "Archivos desplegados correctamente" "OK"
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
    
    # Crear directorio Data si no existe
    ssh $Config.SshTarget "if not exist `"$remoteDataDir`" mkdir `"$remoteDataDir`""
    
    # Copiar DB
    scp $localDb "$($Config.SshTarget):$($remoteDataDir.Replace('\', '/'))/"
    
    Write-Step "Base de datos copiada" "OK"
}

function Show-Summary {
    Write-Header "DEPLOY COMPLETADO"
    
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
    Write-Host "  O remotamente:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "    ssh $($Config.SshTarget) `"cd /d $($Config.RemotePath) && start SGRRHH.Local.Server.exe`"" -ForegroundColor Cyan
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
    Write-Header "DEPLOY SGRRHH A SERVIDOR"
    Write-Host "  Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
    Write-Host "  Destino: $($Config.SshTarget):$($Config.RemotePath)" -ForegroundColor Gray
    
    if (-not $Force) {
        Write-Host ""
        $confirm = Read-Host "¿Continuar con el deploy? (S/N)"
        if ($confirm -notin @("S", "s", "Y", "y", "Si", "si", "SI")) {
            Write-Step "Deploy cancelado por el usuario" "WARN"
            exit 0
        }
    }
    
    # Paso 1: Verificar conexión SSH
    Test-SshConnection
    
    # Paso 2: Detener app remota
    Stop-RemoteApp
    
    # Paso 3: Compilar
    Build-Application
    
    # Paso 4: Limpiar archivos excluidos
    Remove-ExcludedFiles
    
    # Paso 5: Crear paquete
    $zipPath = Create-DeployPackage
    
    # Paso 6: Desplegar
    Deploy-ToServer -ZipPath $zipPath
    
    # Paso 7: Copiar DB si se solicitó
    Copy-Database
    
    # Paso 8: Limpiar ZIP local
    Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
    
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
