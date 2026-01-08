# Publish-Update.ps1
# Ejecutar en PC de DESARROLLO
# Compila y publica a carpeta compartida para que el servidor lo descargue

param(
    [string]$Version,
    [string]$NetworkShare = "C:\SGRRHHUpdates",  # Cambiar a \\SERVIDOR si es remoto
    [string]$ReleaseNotes = "",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Configuracion
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$serverProject = Join-Path $projectRoot "SGRRHH.Local.Server\SGRRHH.Local.Server.csproj"
$publishPath = Join-Path $projectRoot "publish"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " PUBLICAR ACTUALIZACION - SGRRHH LOCAL" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Obtener version actual del version.json en la carpeta compartida
$currentVersion = "0.0.0"
$versionFile = Join-Path $NetworkShare "version.json"
if (Test-Path $versionFile) {
    $versionData = Get-Content $versionFile | ConvertFrom-Json
    $currentVersion = $versionData.version
}

Write-Host "Version actual publicada: $currentVersion" -ForegroundColor Gray

# Si no se especifico version, incrementar automaticamente
if (-not $Version) {
    $parts = $currentVersion.Split('.')
    $parts[2] = [int]$parts[2] + 1
    $Version = $parts -join '.'
    Write-Host "Nueva version (auto): $Version" -ForegroundColor Yellow
} else {
    Write-Host "Nueva version: $Version" -ForegroundColor Yellow
}

# Pedir notas si no se proporcionaron
if (-not $ReleaseNotes) {
    $ReleaseNotes = Read-Host "Notas de la version (Enter para omitir)"
    if (-not $ReleaseNotes) {
        $ReleaseNotes = "Actualizacion $Version"
    }
}

Write-Host ""

# Paso 1: Compilar
Write-Host "[1/4] Compilando proyecto..." -ForegroundColor Yellow
$buildOutput = dotnet publish $serverProject -c Release -o $publishPath --nologo 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo la compilacion" -ForegroundColor Red
    Write-Host $buildOutput -ForegroundColor Red
    exit 1
}
Write-Host "      Compilacion exitosa" -ForegroundColor Green

# Paso 2: Crear carpeta compartida si no existe
Write-Host "[2/4] Preparando carpeta de actualizaciones..." -ForegroundColor Yellow
$latestPath = Join-Path $NetworkShare "latest"

if (-not (Test-Path $NetworkShare)) {
    New-Item -ItemType Directory -Path $NetworkShare -Force | Out-Null
}
if (-not (Test-Path $latestPath)) {
    New-Item -ItemType Directory -Path $latestPath -Force | Out-Null
}
Write-Host "      Carpeta lista: $NetworkShare" -ForegroundColor Green

# Paso 3: Copiar archivos
Write-Host "[3/4] Copiando archivos..." -ForegroundColor Yellow

# Limpiar carpeta latest
Get-ChildItem $latestPath | Remove-Item -Recurse -Force

# Copiar todos los archivos publicados
Copy-Item -Path "$publishPath\*" -Destination $latestPath -Recurse -Force

# Contar archivos
$fileCount = (Get-ChildItem $latestPath -Recurse -File).Count
Write-Host "      $fileCount archivos copiados" -ForegroundColor Green

# Paso 4: Crear version.json
Write-Host "[4/4] Creando version.json..." -ForegroundColor Yellow

$versionInfo = @{
    version = $Version
    previousVersion = $currentVersion
    publishDate = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss")
    releaseNotes = $ReleaseNotes
    publishedBy = $env:USERNAME
    publishedFrom = $env:COMPUTERNAME
    mandatory = $false
    files = @{
        total = $fileCount
        mainExe = "SGRRHH.Local.Server.exe"
    }
}

$versionInfo | ConvertTo-Json -Depth 3 | Set-Content $versionFile -Encoding UTF8
Write-Host "      version.json creado" -ForegroundColor Green

# Resumen
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host " ACTUALIZACION PUBLICADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Version: $Version" -ForegroundColor White
Write-Host "Ubicacion: $NetworkShare" -ForegroundColor Gray
Write-Host "Notas: $ReleaseNotes" -ForegroundColor Gray
Write-Host ""
Write-Host "El servidor detectara la actualizacion automaticamente." -ForegroundColor Cyan
Write-Host ""
