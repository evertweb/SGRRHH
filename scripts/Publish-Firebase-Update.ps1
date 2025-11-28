<#
.SYNOPSIS
    Publica una nueva versión de SGRRHH a Firebase Storage.

.DESCRIPTION
    Este script:
    1. Compila la aplicación en modo Release
    2. Crea el archivo version.json con la información de la versión
    3. Sube los archivos a Firebase Storage usando gsutil o el SDK
    4. Calcula los checksums de los archivos

.PARAMETER Version
    Número de versión a publicar (ej: "1.1.0")

.PARAMETER ReleaseNotes
    Notas de la versión (changelog)

.PARAMETER Mandatory
    Si es true, la actualización será obligatoria

.PARAMETER SkipBuild
    Si es true, omite la compilación y usa los archivos ya publicados

.PARAMETER CredentialsPath
    Ruta al archivo de credenciales de Firebase (JSON de service account)
    Por defecto busca en src/SGRRHH.WPF/firebase-credentials.json

.PARAMETER BucketName
    Nombre del bucket de Firebase Storage.
    Por defecto: rrhh-forestech.firebasestorage.app

.EXAMPLE
    .\Publish-Firebase-Update.ps1 -Version "1.1.0" -ReleaseNotes "Corrección de errores"

.EXAMPLE
    .\Publish-Firebase-Update.ps1 -Version "1.2.0" -Mandatory $true -ReleaseNotes "Actualización crítica"

.EXAMPLE
    .\Publish-Firebase-Update.ps1 -Version "1.1.0" -SkipBuild

.EXAMPLE
    .\Publish-Firebase-Update.ps1 -Version "1.1.1" -Incremental -ReleaseNotes "Fix menor"
    # Solo sube archivos que cambiaron (comparando checksums con Firebase)
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    
    [Parameter(Mandatory = $false)]
    [string]$ReleaseNotes = "",
    
    [Parameter(Mandatory = $false)]
    [bool]$Mandatory = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory = $false)]
    [switch]$Incremental,
    
    [Parameter(Mandatory = $false)]
    [string]$CredentialsPath = "",
    
    [Parameter(Mandatory = $false)]
    [string]$BucketName = "rrhh-forestech.firebasestorage.app"
)

$ErrorActionPreference = "Stop"

# Colores para la consola
function Write-Success { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Warn { param($msg) Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Err { param($msg) Write-Host "[ERROR] $msg" -ForegroundColor Red }

Write-Host ""
Write-Host "========================================================" -ForegroundColor Blue
Write-Host "     SGRRHH - Publicador de Actualizaciones Firebase    " -ForegroundColor Blue
Write-Host "========================================================" -ForegroundColor Blue
Write-Host ""

# Rutas del proyecto
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
$srcPath = Join-Path $projectRoot "src"
$wpfProject = Join-Path $srcPath "SGRRHH.WPF\SGRRHH.WPF.csproj"
$publishPath = Join-Path $srcPath "publish\SGRRHH"

# Ruta de credenciales por defecto
if ([string]::IsNullOrEmpty($CredentialsPath)) {
    $CredentialsPath = Join-Path $srcPath "SGRRHH.WPF\firebase-credentials.json"
}

Write-Info "Versión a publicar: $Version"
Write-Info "Bucket Firebase: $BucketName"
Write-Info "Ruta de credenciales: $CredentialsPath"
Write-Info "Ruta del proyecto: $projectRoot"

# Verificar que el proyecto existe
if (-not (Test-Path $wpfProject)) {
    Write-Err "No se encontró el proyecto en: $wpfProject"
    exit 1
}

# Verificar credenciales de Firebase
if (-not (Test-Path $CredentialsPath)) {
    Write-Err "No se encontró el archivo de credenciales: $CredentialsPath"
    Write-Info "Por favor, descargue el archivo de credenciales desde Firebase Console"
    Write-Info "  1. Vaya a Firebase Console > Project Settings > Service accounts"
    Write-Info "  2. Haga clic en 'Generate new private key'"
    Write-Info "  3. Guarde el archivo como firebase-credentials.json"
    exit 1
}

# Configurar variable de entorno para autenticación
$env:GOOGLE_APPLICATION_CREDENTIALS = $CredentialsPath

# Verificar que gsutil está disponible
$gsutilAvailable = $false
try {
    $gsutilVersion = gsutil version 2>$null
    if ($LASTEXITCODE -eq 0) {
        $gsutilAvailable = $true
        Write-Info "gsutil disponible: $gsutilVersion"
    }
} catch {
    $gsutilAvailable = $false
}

if (-not $gsutilAvailable) {
    Write-Warn "gsutil no está instalado. Usando método alternativo con .NET"
    Write-Info "Para mejor rendimiento, instale Google Cloud SDK: https://cloud.google.com/sdk/docs/install"
}

# Paso 1: Actualizar versión en el proyecto
Write-Host ""
Write-Info "Paso 1/6: Actualizando versión en el proyecto..."

$csprojContent = Get-Content $wpfProject -Raw
if ($csprojContent -match '<Version>([^<]+)</Version>') {
    $csprojContent = $csprojContent -replace '<Version>[^<]+</Version>', "<Version>$Version</Version>"
} else {
    if ($csprojContent -match '<AssemblyVersion>') {
        $csprojContent = $csprojContent -replace '(<AssemblyVersion>[^<]+</AssemblyVersion>)', "`$1`n    <Version>$Version</Version>"
    }
}
Set-Content -Path $wpfProject -Value $csprojContent -NoNewline
Write-Success "Versión actualizada en csproj"

# Paso 2: Compilar la aplicación
if (-not $SkipBuild) {
    Write-Host ""
    Write-Info "Paso 2/6: Compilando la aplicación..."
    
    # Limpiar publicación anterior
    if (Test-Path $publishPath) {
        Remove-Item -Path $publishPath -Recurse -Force
    }
    
    # Publicar
    Push-Location $srcPath
    try {
        dotnet publish SGRRHH.WPF/SGRRHH.WPF.csproj -c Release -r win-x64 --self-contained true -o "publish/SGRRHH"
        if ($LASTEXITCODE -ne 0) {
            Write-Err "Error en la compilación"
            exit 1
        }
        Write-Success "Compilación completada"
    }
    finally {
        Pop-Location
    }
} else {
    Write-Warn "Omitiendo compilación (usando archivos existentes)"
    if (-not (Test-Path $publishPath)) {
        Write-Err "No se encontraron archivos publicados en: $publishPath"
        exit 1
    }
}

# Paso 3: Calcular checksums
Write-Host ""
Write-Info "Paso 3/6: Calculando checksums de archivos..."

$files = @()
$totalSize = 0

Get-ChildItem -Path $publishPath -File -Recurse | ForEach-Object {
    $relativePath = $_.FullName.Substring($publishPath.Length + 1)
    
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

Write-Success "$($files.Count) archivos procesados (Total: $([math]::Round($totalSize/1MB, 2)) MB)"

# Paso 4: Crear version.json
Write-Host ""
Write-Info "Paso 4/6: Generando version.json..."

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
$localVersionFile = Join-Path $publishPath "..\version.json"
Set-Content -Path $localVersionFile -Value $versionJson -Encoding UTF8

Write-Success "version.json generado"

# Paso 5: Subir archivos a Firebase Storage
Write-Host ""

# Variables para tracking de archivos a subir
$filesToUpload = @()
$skippedFiles = 0

if ($Incremental) {
    Write-Info "Paso 5/6: Modo INCREMENTAL - Comparando con versión en Firebase..."
    
    # Descargar version.json actual de Firebase para comparar checksums
    $remoteVersionJson = $null
    $remoteFiles = @{}
    
    try {
        $tempVersionFile = Join-Path $env:TEMP "remote_version_$(Get-Random).json"
        gcloud storage cp "gs://$BucketName/updates/version.json" $tempVersionFile 2>$null
        
        if (Test-Path $tempVersionFile) {
            $remoteVersionJson = Get-Content $tempVersionFile -Raw | ConvertFrom-Json
            
            # Crear diccionario de checksums remotos
            foreach ($remoteFile in $remoteVersionJson.files) {
                $remoteFiles[$remoteFile.name] = $remoteFile.checksum
            }
            
            Remove-Item $tempVersionFile -Force -ErrorAction SilentlyContinue
            Write-Info "Versión remota encontrada: $($remoteVersionJson.version)"
        }
    } catch {
        Write-Warn "No se pudo obtener version.json remoto. Subiendo todos los archivos."
    }
    
    # Comparar checksums y determinar qué archivos subir
    foreach ($localFile in $files) {
        $localChecksum = $localFile.checksum
        $fileName = $localFile.name
        
        if ($remoteFiles.ContainsKey($fileName) -and $remoteFiles[$fileName] -eq $localChecksum) {
            $skippedFiles++
        } else {
            $filesToUpload += $localFile
        }
    }
    
    if ($filesToUpload.Count -eq 0) {
        Write-Warn "No hay archivos modificados. Solo se actualizará version.json"
    } else {
        $changedSize = ($filesToUpload | Measure-Object -Property size -Sum).Sum
        Write-Info "Archivos modificados: $($filesToUpload.Count) de $($files.Count)"
        Write-Info "Tamaño a subir: $([math]::Round($changedSize / 1MB, 2)) MB (en lugar de $([math]::Round($totalSize / 1MB, 2)) MB)"
        Write-Info "Archivos sin cambios (omitidos): $skippedFiles"
    }
} else {
    Write-Info "Paso 5/6: Subiendo TODOS los archivos a Firebase Storage..."
    $filesToUpload = $files
}

$storagePath = "gs://$BucketName"
$updatesPath = "$storagePath/updates"

# Subir archivos (solo los que cambiaron en modo incremental)
if ($Incremental -and $filesToUpload.Count -gt 0) {
    Write-Info "Subiendo $($filesToUpload.Count) archivos modificados..."
    
    $uploadCount = 0
    foreach ($fileInfo in $filesToUpload) {
        $localFilePath = Join-Path $publishPath $fileInfo.name
        $remotePath = "gs://$BucketName/updates/latest/$($fileInfo.name)"
        
        $uploadCount++
        Write-Host "  [$uploadCount/$($filesToUpload.Count)] $($fileInfo.name)" -ForegroundColor Yellow
        
        gcloud storage cp $localFilePath $remotePath 2>$null
        if ($LASTEXITCODE -ne 0) {
            Write-Err "Error al subir: $($fileInfo.name)"
        }
    }
    
    # Siempre subir version.json actualizado
    Write-Info "Subiendo version.json..."
    gcloud storage cp $localVersionFile "gs://$BucketName/updates/version.json"
    
    Write-Success "$uploadCount archivos subidos (modo incremental)"
    
} elseif ($Incremental -and $filesToUpload.Count -eq 0) {
    # Solo actualizar version.json
    Write-Info "Subiendo solo version.json..."
    gcloud storage cp $localVersionFile "gs://$BucketName/updates/version.json"
    Write-Success "version.json actualizado"
    
} else {
    # Modo completo (subir todos los archivos)
    # Verificar si gcloud está disponible
    $gcloudAvailable = $false
    try {
        $gcloudVersion = gcloud --version 2>$null | Select-Object -First 1
        if ($LASTEXITCODE -eq 0) {
            $gcloudAvailable = $true
        }
    } catch { }
    
    if ($gcloudAvailable) {
        Write-Info "Usando gcloud storage para subida..."
        
        # Limpiar carpeta latest existente
        Write-Info "Limpiando carpeta updates/latest existente..."
        gcloud storage rm -r "$updatesPath/latest/**" 2>$null
        
        # Subir todos los archivos
        Write-Info "Subiendo $($files.Count) archivos..."
        gcloud storage cp -r "$publishPath/*" "$updatesPath/latest/"
        if ($LASTEXITCODE -ne 0) {
            Write-Err "Error al subir archivos con gcloud"
            exit 1
        }
        
        # Subir version.json
        gcloud storage cp $localVersionFile "$updatesPath/version.json"
        if ($LASTEXITCODE -ne 0) {
            Write-Err "Error al subir version.json"
            exit 1
        }
        
        Write-Success "Archivos subidos con gcloud storage"
    } elseif ($gsutilAvailable) {
        # Usar gsutil para subida más rápida (paralela)
        Write-Info "Usando gsutil para subida paralela..."
        
        # Limpiar carpeta latest existente
        Write-Info "Limpiando carpeta updates/latest existente..."
        gsutil -m rm -r "$updatesPath/latest/" 2>$null
        
        # Subir todos los archivos
        Write-Info "Subiendo archivos..."
        gsutil -m cp -r "$publishPath/*" "$updatesPath/latest/"
        if ($LASTEXITCODE -ne 0) {
            Write-Err "Error al subir archivos con gsutil"
            exit 1
        }
        
        # Subir version.json
        gsutil cp $localVersionFile "$updatesPath/version.json"
        if ($LASTEXITCODE -ne 0) {
            Write-Err "Error al subir version.json"
            exit 1
        }
        
        Write-Success "Archivos subidos con gsutil"
    } else {
    # Usar método alternativo con script de .NET
    Write-Info "Usando método alternativo para subida..."
    
    # Crear script temporal de C# para subir archivos
    $uploadScript = @"
using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var bucketName = args[0];
        var publishPath = args[1];
        var versionFile = args[2];
        
        var storage = await StorageClient.CreateAsync();
        
        Console.WriteLine("Limpiando archivos existentes...");
        try 
        {
            var objects = storage.ListObjects(bucketName, "updates/latest/");
            foreach (var obj in objects)
            {
                await storage.DeleteObjectAsync(bucketName, obj.Name);
            }
        }
        catch { }
        
        Console.WriteLine("Subiendo archivos...");
        var files = Directory.GetFiles(publishPath, "*", SearchOption.AllDirectories);
        int count = 0;
        
        foreach (var file in files)
        {
            var relativePath = file.Substring(publishPath.Length + 1).Replace("\\", "/");
            var storagePath = "updates/latest/" + relativePath;
            
            using var stream = File.OpenRead(file);
            await storage.UploadObjectAsync(bucketName, storagePath, null, stream);
            
            count++;
            Console.WriteLine($"  [{count}/{files.Length}] {relativePath}");
        }
        
        Console.WriteLine("Subiendo version.json...");
        using var versionStream = File.OpenRead(versionFile);
        await storage.UploadObjectAsync(bucketName, "updates/version.json", "application/json", versionStream);
        
        Console.WriteLine($"Completado: {count} archivos subidos");
    }
}
"@
    
    # Crear proyecto temporal
    $tempDir = Join-Path $env:TEMP "sgrrhh_upload_$(Get-Random)"
    New-Item -Path $tempDir -ItemType Directory -Force | Out-Null
    
    $csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Google.Cloud.Storage.V1" Version="4.10.0" />
  </ItemGroup>
</Project>
"@
    
    Set-Content -Path (Join-Path $tempDir "Upload.csproj") -Value $csprojContent
    Set-Content -Path (Join-Path $tempDir "Program.cs") -Value $uploadScript
    
    Push-Location $tempDir
    try {
        dotnet run -- $BucketName $publishPath $localVersionFile
        if ($LASTEXITCODE -ne 0) {
            Write-Err "Error al subir archivos"
            exit 1
        }
    }
    finally {
        Pop-Location
        Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    Write-Success "Archivos subidos"
    }
}

# Paso 6: Verificar subida
Write-Host ""
Write-Info "Paso 6/6: Verificando subida..."

# Verificar con gcloud o gsutil
$verificationDone = $false

try {
    $uploadedCount = (gcloud storage ls "$updatesPath/latest/" 2>$null | Measure-Object -Line).Lines
    if ($uploadedCount -gt 0) {
        Write-Info "Archivos en Storage: $uploadedCount"
        $verificationDone = $true
    }
} catch { }

if (-not $verificationDone -and $gsutilAvailable) {
    $uploadedCount = (gsutil ls "$updatesPath/latest/" 2>$null | Measure-Object -Line).Lines
    Write-Info "Archivos en Storage: $uploadedCount"
}

# Verificar version.json
try {
    gcloud storage cat "$updatesPath/version.json" 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Success "version.json verificado en Storage"
    }
} catch {
    Write-Warn "No se pudo verificar version.json"
}

# Limpiar archivo temporal de version.json
Remove-Item -Path $localVersionFile -Force -ErrorAction SilentlyContinue

# Resumen final
Write-Host ""
Write-Host "========================================================" -ForegroundColor Green
Write-Host "     ACTUALIZACION PUBLICADA EN FIREBASE!               " -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Version:          $Version"
Write-Host "  Bucket:           $BucketName"
Write-Host "  Ruta Storage:     updates/latest/"
Write-Host "  Archivos totales: $($files.Count)"
Write-Host "  Tamano total:     $([math]::Round($totalSize/1MB, 2)) MB"
if ($Incremental) {
    Write-Host "  Modo:             INCREMENTAL" -ForegroundColor Cyan
    Write-Host "  Archivos subidos: $($filesToUpload.Count)"
    Write-Host "  Archivos omitidos:$skippedFiles (sin cambios)"
} else {
    Write-Host "  Modo:             COMPLETO"
}
Write-Host "  Obligatoria:      $Mandatory"
Write-Host ""
Write-Host "  Las PCs clientes detectaran la actualizacion automaticamente"
Write-Host "  la proxima vez que inicien SGRRHH."
Write-Host ""

# Mostrar URLs importantes
Write-Host "URLs de verificacion:" -ForegroundColor Cyan
Write-Host "  version.json: https://firebasestorage.googleapis.com/v0/b/$BucketName/o/updates%2Fversion.json?alt=media"
Write-Host ""

# Mostrar contenido del version.json
Write-Host "Contenido de version.json:" -ForegroundColor Cyan
$versionJson | ConvertFrom-Json | Select-Object version, releaseDate, mandatory, downloadSize | Format-List
