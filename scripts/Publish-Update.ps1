<#
.SYNOPSIS
    Publica una nueva versión de SGRRHH en la carpeta de actualizaciones compartida.

.DESCRIPTION
    Este script:
    1. Compila la aplicación en modo Release
    2. Crea el archivo version.json con la información de la versión
    3. Copia los archivos a la carpeta de actualizaciones compartida
    4. Calcula los checksums de los archivos

.PARAMETER Version
    Número de versión a publicar (ej: "1.1.0")

.PARAMETER UpdatesPath
    Ruta de la carpeta de actualizaciones. Por defecto: C:\SGRRHH_Data\updates

.PARAMETER ReleaseNotes
    Notas de la versión (changelog)

.PARAMETER Mandatory
    Si es true, la actualización será obligatoria

.PARAMETER SkipBuild
    Si es true, omite la compilación y usa los archivos ya publicados

.EXAMPLE
    .\Publish-Update.ps1 -Version "1.1.0" -ReleaseNotes "Corrección de errores"

.EXAMPLE
    .\Publish-Update.ps1 -Version "1.2.0" -Mandatory $true -ReleaseNotes "Actualización de seguridad crítica"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    
    [Parameter(Mandatory = $false)]
    [string]$UpdatesPath = "C:\SGRRHH_Data\updates",
    
    [Parameter(Mandatory = $false)]
    [string]$ReleaseNotes = "",
    
    [Parameter(Mandatory = $false)]
    [bool]$Mandatory = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

# Colores para la consola
function Write-Success { param($msg) Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "ℹ️ $msg" -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host "⚠️ $msg" -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host "❌ $msg" -ForegroundColor Red }

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║        SGRRHH - Publicador de Actualizaciones                ║" -ForegroundColor Blue
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Blue
Write-Host ""

# Rutas del proyecto
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
$srcPath = Join-Path $projectRoot "src"
$wpfProject = Join-Path $srcPath "SGRRHH.WPF\SGRRHH.WPF.csproj"
$publishPath = Join-Path $srcPath "publish\SGRRHH"

Write-Info "Versión a publicar: $Version"
Write-Info "Ruta de actualizaciones: $UpdatesPath"
Write-Info "Ruta del proyecto: $projectRoot"

# Verificar que el proyecto existe
if (-not (Test-Path $wpfProject)) {
    Write-Error "No se encontró el proyecto en: $wpfProject"
    exit 1
}

# Paso 1: Actualizar versión en el proyecto
Write-Host ""
Write-Info "Paso 1/5: Actualizando versión en el proyecto..."

$csprojContent = Get-Content $wpfProject -Raw
if ($csprojContent -match '<Version>([^<]+)</Version>') {
    $csprojContent = $csprojContent -replace '<Version>[^<]+</Version>', "<Version>$Version</Version>"
} else {
    # Si no existe, agregar después de <AssemblyVersion> o al final del primer PropertyGroup
    if ($csprojContent -match '<AssemblyVersion>') {
        $csprojContent = $csprojContent -replace '(<AssemblyVersion>[^<]+</AssemblyVersion>)', "`$1`n    <Version>$Version</Version>"
    }
}
Set-Content -Path $wpfProject -Value $csprojContent -NoNewline
Write-Success "Versión actualizada en csproj"

# Paso 2: Compilar la aplicación
if (-not $SkipBuild) {
    Write-Host ""
    Write-Info "Paso 2/5: Compilando la aplicación..."
    
    # Limpiar publicación anterior
    if (Test-Path $publishPath) {
        Remove-Item -Path $publishPath -Recurse -Force
    }
    
    # Publicar
    Push-Location $srcPath
    try {
        dotnet publish SGRRHH.WPF/SGRRHH.WPF.csproj -c Release -o "publish/SGRRHH" --self-contained false
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Error en la compilación"
            exit 1
        }
        Write-Success "Compilación completada"
    }
    finally {
        Pop-Location
    }
} else {
    Write-Warning "Omitiendo compilación (usando archivos existentes)"
    if (-not (Test-Path $publishPath)) {
        Write-Error "No se encontraron archivos publicados en: $publishPath"
        exit 1
    }
}

# Paso 3: Crear carpetas de actualización
Write-Host ""
Write-Info "Paso 3/5: Preparando carpeta de actualizaciones..."

$latestPath = Join-Path $UpdatesPath "latest"
$historyPath = Join-Path $UpdatesPath "history\$Version"

# Crear carpetas si no existen
New-Item -Path $UpdatesPath -ItemType Directory -Force | Out-Null
New-Item -Path $latestPath -ItemType Directory -Force | Out-Null
New-Item -Path $historyPath -ItemType Directory -Force | Out-Null

# Limpiar carpeta latest
Get-ChildItem -Path $latestPath | Remove-Item -Recurse -Force
Write-Success "Carpetas preparadas"

# Paso 4: Copiar archivos y calcular checksums
Write-Host ""
Write-Info "Paso 4/5: Copiando archivos y calculando checksums..."

$files = @()
$totalSize = 0

Get-ChildItem -Path $publishPath -File -Recurse | ForEach-Object {
    $relativePath = $_.FullName.Substring($publishPath.Length + 1)
    $destPath = Join-Path $latestPath $relativePath
    
    # Crear directorio de destino si no existe
    $destDir = Split-Path -Parent $destPath
    if (-not (Test-Path $destDir)) {
        New-Item -Path $destDir -ItemType Directory -Force | Out-Null
    }
    
    # Copiar archivo
    Copy-Item -Path $_.FullName -Destination $destPath -Force
    
    # Calcular checksum
    $hash = Get-FileHash -Path $_.FullName -Algorithm SHA256
    
    $files += @{
        name = $relativePath.Replace("\", "/")
        checksum = "sha256:$($hash.Hash.ToLower())"
        size = $_.Length
    }
    
    $totalSize += $_.Length
    
    Write-Host "  📄 $relativePath ($([math]::Round($_.Length/1KB, 1)) KB)"
}

# Copiar también al historial
Copy-Item -Path "$latestPath\*" -Destination $historyPath -Recurse -Force
Write-Success "$($files.Count) archivos copiados (Total: $([math]::Round($totalSize/1MB, 2)) MB)"

# Paso 5: Crear version.json
Write-Host ""
Write-Info "Paso 5/5: Generando version.json..."

# Calcular checksum del paquete completo
$allHashes = ($files | ForEach-Object { $_.checksum }) -join ""
$packageHashBytes = [System.Text.Encoding]::UTF8.GetBytes($allHashes)
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$packageHash = [System.BitConverter]::ToString($sha256.ComputeHash($packageHashBytes)).Replace("-", "").ToLower()

$versionInfo = @{
    version = $Version
    releaseDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    mandatory = $Mandatory
    minimumVersion = "1.0.0"
    releaseNotes = $ReleaseNotes
    checksum = "sha256:$packageHash"
    downloadSize = $totalSize
    files = $files
}

$versionJson = $versionInfo | ConvertTo-Json -Depth 10
$versionFilePath = Join-Path $UpdatesPath "version.json"
Set-Content -Path $versionFilePath -Value $versionJson -Encoding UTF8

Write-Success "version.json generado"

# Resumen final
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              ¡ACTUALIZACIÓN PUBLICADA!                       ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "  📦 Versión:          $Version"
Write-Host "  📂 Ubicación:        $latestPath"
Write-Host "  📋 Archivos:         $($files.Count)"
Write-Host "  💾 Tamaño total:     $([math]::Round($totalSize/1MB, 2)) MB"
Write-Host "  🔒 Obligatoria:      $Mandatory"
Write-Host ""
Write-Host "  Las PCs clientes detectarán la actualización automáticamente"
Write-Host "  la próxima vez que inicien SGRRHH."
Write-Host ""

# Mostrar contenido del version.json
Write-Host "Contenido de version.json:" -ForegroundColor Cyan
Write-Host $versionJson
