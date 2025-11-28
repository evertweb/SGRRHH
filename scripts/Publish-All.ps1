# =============================================================================
# Script: Publish-All.ps1
# Descripcion: Script unificado para publicar a Firebase Y actualizar C:\SGRRHH
# 
# Este script resuelve el problema de tener que ejecutar multiples scripts
# y asegura que tu instalacion local siempre este actualizada.
#
# Uso:
#   .\Publish-All.ps1 -Version "1.0.4" -ReleaseNotes "Descripcion de cambios"
#   .\Publish-All.ps1 -Version "1.0.4" -ReleaseNotes "Cambios" -SkipFirebase
#   .\Publish-All.ps1 -Version "1.0.4" -ReleaseNotes "Cambios" -SkipLocal
# =============================================================================

param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    
    [Parameter(Mandatory = $false)]
    [string]$ReleaseNotes = "",
    
    [Parameter(Mandatory = $false)]
    [bool]$Mandatory = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipFirebase,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipLocal,
    
    [Parameter(Mandatory = $false)]
    [switch]$Incremental
)

$ErrorActionPreference = "Stop"

# Colores
function Write-Success { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Warn { param($msg) Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Err { param($msg) Write-Host "[ERROR] $msg" -ForegroundColor Red }
function Write-Step { param($step, $total, $msg) Write-Host "`n[$step/$total] $msg" -ForegroundColor Magenta }

Write-Host ""
Write-Host "================================================================" -ForegroundColor Blue
Write-Host "     SGRRHH - Publicacion Unificada (Firebase + Local)         " -ForegroundColor Blue
Write-Host "================================================================" -ForegroundColor Blue
Write-Host ""

# Configuracion de rutas
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
$srcPath = Join-Path $projectRoot "src"
$wpfProject = Join-Path $srcPath "SGRRHH.WPF\SGRRHH.WPF.csproj"
$publishPath = Join-Path $srcPath "publish\SGRRHH"
$localInstallPath = "C:\SGRRHH"

Write-Info "Version a publicar: $Version"
Write-Info "Notas: $ReleaseNotes"
Write-Host ""

# Verificar que la app no este corriendo
$runningProcess = Get-Process -Name "SGRRHH" -ErrorAction SilentlyContinue
if ($runningProcess) {
    Write-Warn "SGRRHH esta en ejecucion."
    $response = Read-Host "Desea cerrarla para continuar? (S/N)"
    if ($response -eq "S" -or $response -eq "s") {
        Write-Info "Cerrando SGRRHH..."
        Stop-Process -Name "SGRRHH" -Force
        Start-Sleep -Seconds 2
    } else {
        Write-Err "No se puede continuar mientras la aplicacion esta en ejecucion."
        exit 1
    }
}

$totalSteps = 4
if ($SkipFirebase) { $totalSteps-- }
if ($SkipLocal) { $totalSteps-- }
$currentStep = 0

# ============================================================================
# PASO 1: Actualizar version en el proyecto
# ============================================================================
$currentStep++
Write-Step $currentStep $totalSteps "Actualizando version en el proyecto..."

# Actualizar version en csproj
$csprojContent = Get-Content $wpfProject -Raw
if ($csprojContent -match '<Version>([^<]+)</Version>') {
    $oldVersion = $matches[1]
    $csprojContent = $csprojContent -replace '<Version>[^<]+</Version>', "<Version>$Version</Version>"
    Set-Content -Path $wpfProject -Value $csprojContent -NoNewline
    Write-Success "Version actualizada en csproj: $oldVersion -> $Version"
} else {
    Write-Warn "No se encontro tag Version en el csproj"
}

# Actualizar version en appsettings.json del proyecto
$wpfAppSettings = Join-Path $srcPath "SGRRHH.WPF\appsettings.json"
if (Test-Path $wpfAppSettings) {
    $appSettingsContent = Get-Content $wpfAppSettings -Raw | ConvertFrom-Json
    if ($appSettingsContent.Application) {
        $appSettingsContent.Application.Version = $Version
        $appSettingsContent | ConvertTo-Json -Depth 10 | Set-Content $wpfAppSettings -Encoding UTF8
        Write-Success "Version actualizada en appsettings.json del proyecto"
    }
}

# ============================================================================
# PASO 2: Compilar la aplicacion
# ============================================================================
$currentStep++
Write-Step $currentStep $totalSteps "Compilando la aplicacion (Release, self-contained)..."

# Limpiar publicacion anterior
if (Test-Path $publishPath) {
    Remove-Item -Path $publishPath -Recurse -Force
}

Push-Location $srcPath
try {
    $output = dotnet publish SGRRHH.WPF/SGRRHH.WPF.csproj -c Release -r win-x64 --self-contained true -o "publish/SGRRHH" 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Error en la compilacion:"
        Write-Host $output -ForegroundColor Red
        exit 1
    }
    Write-Success "Compilacion completada"
}
finally {
    Pop-Location
}

# ============================================================================
# PASO 3: Subir a Firebase (si no se omite)
# ============================================================================
if (-not $SkipFirebase) {
    $currentStep++
    Write-Step $currentStep $totalSteps "Publicando a Firebase Storage..."
    
    # Llamar al script de Firebase
    $firebaseScript = Join-Path $scriptPath "Publish-Firebase-Update.ps1"
    
    $firebaseArgs = @{
        Version = $Version
        ReleaseNotes = $ReleaseNotes
        Mandatory = $Mandatory
        SkipBuild = $true
    }
    
    if ($Incremental) {
        & $firebaseScript @firebaseArgs -Incremental
    } else {
        & $firebaseScript @firebaseArgs
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Error al publicar en Firebase"
        exit 1
    }
    
    Write-Success "Publicado en Firebase Storage"
}

# ============================================================================
# PASO 4: Actualizar instalacion local (si no se omite)
# ============================================================================
if (-not $SkipLocal) {
    $currentStep++
    Write-Step $currentStep $totalSteps "Actualizando instalacion local en $localInstallPath..."
    
    # Crear directorio si no existe
    if (-not (Test-Path $localInstallPath)) {
        New-Item -ItemType Directory -Path $localInstallPath -Force | Out-Null
        Write-Info "Creado directorio: $localInstallPath"
    }
    
    # Preservar archivos de configuracion local
    $preserveFiles = @("appsettings.json", "firebase-credentials.json")
    $preservedData = @{}
    
    foreach ($file in $preserveFiles) {
        $filePath = Join-Path $localInstallPath $file
        if (Test-Path $filePath) {
            $preservedData[$file] = Get-Content $filePath -Raw
            Write-Info "Preservando: $file"
        }
    }
    
    # Copiar todos los archivos de la publicacion
    Write-Info "Copiando archivos..."
    
    $sourceFiles = Get-ChildItem -Path $publishPath -Recurse
    $totalFiles = ($sourceFiles | Where-Object { -not $_.PSIsContainer }).Count
    $copiedCount = 0
    
    foreach ($item in $sourceFiles) {
        if ($item.FullName.Length -le $publishPath.Length) { continue }
        
        $relativePath = $item.FullName.Substring($publishPath.Length + 1)
        $destPath = Join-Path $localInstallPath $relativePath
        
        # Saltar carpeta data
        if ($relativePath -like "data*") { continue }
        
        if ($item.PSIsContainer) {
            if (-not (Test-Path $destPath)) {
                New-Item -ItemType Directory -Path $destPath -Force | Out-Null
            }
        } else {
            $destDir = Split-Path $destPath -Parent
            if (-not (Test-Path $destDir)) {
                New-Item -ItemType Directory -Path $destDir -Force | Out-Null
            }
            Copy-Item -Path $item.FullName -Destination $destPath -Force
            $copiedCount++
        }
    }
    
    Write-Success "Copiados $copiedCount archivos"
    
    # Restaurar archivos de configuracion preservados
    foreach ($file in $preserveFiles) {
        if ($preservedData.ContainsKey($file)) {
            $filePath = Join-Path $localInstallPath $file
            Set-Content -Path $filePath -Value $preservedData[$file] -NoNewline
        }
    }
    
    # IMPORTANTE: Actualizar la version en el appsettings.json local
    $localAppSettings = Join-Path $localInstallPath "appsettings.json"
    if (Test-Path $localAppSettings) {
        try {
            $localConfig = Get-Content $localAppSettings -Raw | ConvertFrom-Json
            
            if (-not $localConfig.Application) {
                $localConfig | Add-Member -NotePropertyName "Application" -NotePropertyValue @{} -Force
            }
            
            $localConfig.Application.Version = $Version
            $localConfig | ConvertTo-Json -Depth 10 | Set-Content $localAppSettings -Encoding UTF8
            
            Write-Success "Version actualizada en appsettings.json local: $Version"
        } catch {
            Write-Warn "No se pudo actualizar la version en appsettings.json local: $_"
        }
    }
    
    Write-Success "Instalacion local actualizada"
}

# ============================================================================
# RESUMEN FINAL
# ============================================================================
Write-Host ""
Write-Host "================================================================" -ForegroundColor Green
Write-Host "                 PUBLICACION COMPLETADA                        " -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Version publicada:     $Version" -ForegroundColor White
Write-Host "  Notas:                 $ReleaseNotes" -ForegroundColor White
Write-Host ""

if (-not $SkipFirebase) {
    Write-Host "  Firebase Storage:   ACTUALIZADO" -ForegroundColor Cyan
    Write-Host "  Los usuarios remotos recibiran la actualizacion automaticamente." -ForegroundColor Gray
}

if (-not $SkipLocal) {
    Write-Host "  Instalacion local:  ACTUALIZADA ($localInstallPath)" -ForegroundColor Cyan
}

Write-Host ""

# Preguntar si desea ejecutar
$response = Read-Host "Desea ejecutar la aplicacion ahora? (S/N)"
if ($response -eq "S" -or $response -eq "s") {
    $exePath = Join-Path $localInstallPath "SGRRHH.exe"
    if (Test-Path $exePath) {
        Write-Info "Iniciando SGRRHH..."
        Start-Process -FilePath $exePath
    } else {
        Write-Warn "No se encontro SGRRHH.exe en $localInstallPath"
    }
}
