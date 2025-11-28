# =============================================================================
# Script: Publish-Local.ps1
# Descripción: Compila el proyecto y copia automáticamente a C:\SGRRHH
# Uso: .\Publish-Local.ps1 [-Release] [-NoBuild]
# =============================================================================

param(
    [switch]$Release,      # Usar configuración Release en lugar de Debug
    [switch]$NoBuild,      # Saltar la compilación (solo copiar)
    [switch]$Clean         # Limpiar antes de compilar
)

$ErrorActionPreference = "Stop"

# Configuración
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionDir = Join-Path $ScriptDir "..\src"
$ProjectDir = Join-Path $SolutionDir "SGRRHH.WPF"
$Configuration = if ($Release) { "Release" } else { "Debug" }
$OutputDir = Join-Path $ProjectDir "bin\$Configuration\net8.0-windows\win-x64"
$TargetDir = "C:\SGRRHH"

# Colores para la salida
function Write-Info { param($Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-Warn { param($Message) Write-Host "[WARN] $Message" -ForegroundColor Yellow }
function Write-Err { param($Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }

Write-Host ""
Write-Host "=========================================" -ForegroundColor Magenta
Write-Host "   SGRRHH - Publicación Local" -ForegroundColor Magenta
Write-Host "=========================================" -ForegroundColor Magenta
Write-Host ""

# Verificar que el directorio destino existe
if (-not (Test-Path $TargetDir)) {
    Write-Warn "El directorio $TargetDir no existe. Creándolo..."
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
}

# Verificar si la aplicación está en ejecución
$processName = "SGRRHH"
$runningProcess = Get-Process -Name $processName -ErrorAction SilentlyContinue

if ($runningProcess) {
    Write-Warn "La aplicación SGRRHH está en ejecución."
    $response = Read-Host "¿Desea cerrarla para continuar? (S/N)"
    if ($response -eq "S" -or $response -eq "s") {
        Write-Info "Cerrando SGRRHH..."
        Stop-Process -Name $processName -Force
        Start-Sleep -Seconds 2
    } else {
        Write-Err "No se puede continuar mientras la aplicación está en ejecución."
        exit 1
    }
}

# Compilar
if (-not $NoBuild) {
    Write-Info "Compilando proyecto en modo $Configuration..."
    
    Push-Location $SolutionDir
    try {
        if ($Clean) {
            Write-Info "Limpiando solución..."
            dotnet clean SGRRHH.sln --configuration $Configuration --verbosity quiet
        }
        
        # En Release usar publish para incluir self-contained
        if ($Release) {
            Write-Info "Publicando como self-contained (incluye runtime)..."
            $buildOutput = dotnet publish SGRRHH.WPF/SGRRHH.WPF.csproj --configuration $Configuration --runtime win-x64 --self-contained true 2>&1
            $OutputDir = Join-Path $ProjectDir "bin\$Configuration\net8.0-windows\win-x64\publish"
        } else {
            $buildOutput = dotnet build SGRRHH.sln --configuration $Configuration 2>&1
        }
        
        if ($LASTEXITCODE -ne 0) {
            Write-Err "Error en la compilación:"
            Write-Host $buildOutput -ForegroundColor Red
            exit 1
        }
        
        # Verificar errores y warnings
        $errors = $buildOutput | Select-String "error" | Where-Object { $_ -match "^\s*\d+ Error" }
        $warnings = $buildOutput | Select-String "warning" | Where-Object { $_ -match "^\s*\d+ Warning" }
        
        Write-Success "Compilación exitosa"
    }
    finally {
        Pop-Location
    }
} else {
    Write-Info "Saltando compilación (usando binarios existentes)..."
}

# Verificar que existen los archivos compilados
if (-not (Test-Path (Join-Path $OutputDir "SGRRHH.exe"))) {
    Write-Err "No se encontró SGRRHH.exe en $OutputDir"
    Write-Err "Ejecute el script sin -NoBuild para compilar primero."
    exit 1
}

# Copiar archivos
Write-Info "Copiando archivos a $TargetDir..."

# Preservar archivos de configuración local
$preserveFiles = @("appsettings.json", "firebase-credentials.json")
$preservedData = @{}

foreach ($file in $preserveFiles) {
    $filePath = Join-Path $TargetDir $file
    if (Test-Path $filePath) {
        $preservedData[$file] = Get-Content $filePath -Raw
    }
}

# Preservar carpeta data si existe
$dataDir = Join-Path $TargetDir "data"
$hadDataDir = Test-Path $dataDir

# En modo Release, copiar TODO desde publish (self-contained)
if ($Release) {
    Write-Info "Copiando aplicación self-contained completa..."
    
    # Copiar todo excepto la carpeta data
    Get-ChildItem -Path $OutputDir -Recurse | ForEach-Object {
        $relativePath = $_.FullName.Substring($OutputDir.Length + 1)
        $destPath = Join-Path $TargetDir $relativePath
        
        # Saltar carpeta data
        if ($relativePath -like "data*") { return }
        
        if ($_.PSIsContainer) {
            if (-not (Test-Path $destPath)) {
                New-Item -ItemType Directory -Path $destPath -Force | Out-Null
            }
        } else {
            $destDir = Split-Path $destPath -Parent
            if (-not (Test-Path $destDir)) {
                New-Item -ItemType Directory -Path $destDir -Force | Out-Null
            }
            Copy-Item -Path $_.FullName -Destination $destPath -Force
        }
    }
    
    $copiedCount = (Get-ChildItem -Path $OutputDir -Recurse -File).Count
} else {
    # En modo Debug, copiar solo los archivos principales (más rápido)
    $itemsToCopy = @(
        "SGRRHH.exe", "SGRRHH.dll", "SGRRHH.pdb",
        "SGRRHH.deps.json", "SGRRHH.runtimeconfig.json",
        "SGRRHH.Core.dll", "SGRRHH.Core.pdb",
        "SGRRHH.Infrastructure.dll", "SGRRHH.Infrastructure.pdb"
    )
    
    $copiedCount = 0
    foreach ($item in $itemsToCopy) {
        $source = Join-Path $OutputDir $item
        $dest = Join-Path $TargetDir $item
        if (Test-Path $source) {
            Copy-Item -Path $source -Destination $dest -Force
            $copiedCount++
        }
    }
}

# Restaurar archivos de configuración preservados
foreach ($file in $preserveFiles) {
    if ($preservedData.ContainsKey($file)) {
        $filePath = Join-Path $TargetDir $file
        Set-Content -Path $filePath -Value $preservedData[$file] -NoNewline
    }
}

Write-Host ""
Write-Success "Publicación completada!"
Write-Host ""
Write-Host "  Archivos copiados: $copiedCount" -ForegroundColor White
Write-Host "  Destino: $TargetDir" -ForegroundColor White
Write-Host ""

# Mostrar versión del exe
$exePath = Join-Path $TargetDir "SGRRHH.exe"
$exeInfo = Get-Item $exePath
Write-Host "  SGRRHH.exe actualizado: $($exeInfo.LastWriteTime)" -ForegroundColor Green
Write-Host ""

# Preguntar si desea ejecutar
$response = Read-Host "¿Desea ejecutar la aplicación ahora? (S/N)"
if ($response -eq "S" -or $response -eq "s") {
    Write-Info "Iniciando SGRRHH..."
    Start-Process -FilePath $exePath
}
