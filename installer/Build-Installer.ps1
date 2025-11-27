# ============================================================
# SGRRHH - Script de Construcción del Instalador (PowerShell)
# ============================================================

param(
    [switch]$SkipPublish,
    [switch]$CreateZip,
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

# Colores para la consola
function Write-Step { param($Message) Write-Host "`n[$([char]8730)] $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "    $Message" -ForegroundColor Cyan }
function Write-Warn { param($Message) Write-Host "[!] $Message" -ForegroundColor Yellow }
function Write-Err { param($Message) Write-Host "[X] $Message" -ForegroundColor Red }

Write-Host @"
============================================================
   SGRRHH - Construccion del Instalador
   Version 1.0.0
============================================================
"@ -ForegroundColor Cyan

# Configurar rutas
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$srcDir = Join-Path $projectRoot "src"
$publishDir = Join-Path $srcDir "publish\SGRRHH"
$installerDir = Join-Path $projectRoot "installer"
$outputDir = Join-Path $installerDir "output"

# Verificar estructura del proyecto
if (-not (Test-Path (Join-Path $srcDir "SGRRHH.sln"))) {
    Write-Err "No se encontro la solucion SGRRHH.sln"
    Write-Err "Asegurese de ejecutar este script desde la carpeta installer"
    exit 1
}

# Crear directorio de salida si no existe
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Paso 1: Limpiar publicación anterior
Write-Step "Limpiando publicacion anterior..."
if (Test-Path $publishDir) {
    Remove-Item -Path $publishDir -Recurse -Force
    Write-Info "Carpeta de publicacion eliminada"
}

if (-not $SkipPublish) {
    # Paso 2: Compilar proyecto
    Write-Step "Compilando proyecto en modo Release..."
    Set-Location $srcDir
    $buildResult = dotnet build --configuration Release 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Error en la compilacion:"
        Write-Host $buildResult
        exit 1
    }
    Write-Info "Compilacion exitosa"

    # Paso 3: Publicar aplicación
    Write-Step "Publicando aplicacion (self-contained)..."
    Set-Location (Join-Path $srcDir "SGRRHH.WPF")
    $publishResult = dotnet publish --configuration Release --runtime win-x64 --self-contained true --output $publishDir 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Error en la publicacion:"
        Write-Host $publishResult
        exit 1
    }
    Write-Info "Publicacion exitosa"
    
    # Mostrar tamaño de la publicación
    $publishSize = (Get-ChildItem $publishDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Info "Tamano de la publicacion: $([math]::Round($publishSize, 2)) MB"
}

# Paso 4: Crear versión portable (ZIP)
if ($CreateZip) {
    Write-Step "Creando version portable (ZIP)..."
    $zipPath = Join-Path $outputDir "SGRRHH_Portable_1.0.0.zip"
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath -CompressionLevel Optimal
    $zipSize = (Get-Item $zipPath).Length / 1MB
    Write-Info "ZIP creado: $zipPath"
    Write-Info "Tamano del ZIP: $([math]::Round($zipSize, 2)) MB"
}

# Paso 5: Crear instalador con Inno Setup
if (-not $SkipInstaller) {
    Write-Step "Buscando Inno Setup..."
    $isccPaths = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe"
    )
    
    $isccPath = $null
    foreach ($path in $isccPaths) {
        if (Test-Path $path) {
            $isccPath = $path
            break
        }
    }
    
    if ($isccPath) {
        Write-Info "Inno Setup encontrado: $isccPath"
        Write-Step "Creando instalador..."
        
        Set-Location $installerDir
        $issFile = Join-Path $installerDir "SGRRHH_Setup.iss"
        
        & $isccPath $issFile
        if ($LASTEXITCODE -ne 0) {
            Write-Err "Error al crear el instalador"
            exit 1
        }
        
        $installerPath = Join-Path $outputDir "SGRRHH_Setup_1.0.0.exe"
        if (Test-Path $installerPath) {
            $installerSize = (Get-Item $installerPath).Length / 1MB
            Write-Info "Instalador creado: $installerPath"
            Write-Info "Tamano del instalador: $([math]::Round($installerSize, 2)) MB"
        }
    } else {
        Write-Warn "Inno Setup no esta instalado"
        Write-Host ""
        Write-Host "Para crear el instalador, necesita:" -ForegroundColor Yellow
        Write-Host "  1. Descargar Inno Setup 6 desde: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
        Write-Host "  2. Instalarlo en su sistema" -ForegroundColor Yellow
        Write-Host "  3. Ejecutar este script nuevamente" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Alternativa: Use el parametro -CreateZip para crear una version portable" -ForegroundColor Cyan
    }
}

# Resumen final
Write-Host @"

============================================================
   CONSTRUCCION COMPLETADA
============================================================
"@ -ForegroundColor Green

Write-Host "Archivos generados en: $outputDir" -ForegroundColor Cyan
Get-ChildItem $outputDir -File | ForEach-Object {
    $sizeMB = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  - $($_.Name) ($sizeMB MB)" -ForegroundColor White
}

Write-Host ""
Write-Host "La aplicacion publicada esta en: $publishDir" -ForegroundColor Cyan
Write-Host ""
