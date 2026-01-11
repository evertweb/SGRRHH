# ==============================================
# SCRIPT: Separar Markup y Lógica de Blazor
# Extrae @code { } de .razor a .razor.cs
# ==============================================

param(
    [string]$ComponentsPath = "c:\Users\evert\Documents\rrhh\SGRRHH.Local\SGRRHH.Local.Server\Components"
)

$ErrorActionPreference = "Stop"

# Contadores
$processed = 0
$skipped = 0
$errors = @()

# Obtener namespace base del proyecto
$namespace = "SGRRHH.Local.Server.Components"

function Get-ClassNameFromFile {
    param([string]$FilePath)
    return [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
}

function Get-NamespaceFromPath {
    param([string]$FilePath)
    $relativePath = $FilePath.Replace($ComponentsPath, "").TrimStart("\", "/")
    $folder = [System.IO.Path]::GetDirectoryName($relativePath)
    if ($folder) {
        return "$namespace.$($folder.Replace('\', '.'))"
    }
    return $namespace
}

function Extract-CodeBlock {
    param([string]$Content)
    
    # Buscar el inicio de @code {
    $codeMatch = [regex]::Match($Content, '(?s)@code\s*\{')
    
    if (-not $codeMatch.Success) {
        return @{ HasCode = $false }
    }
    
    $startIndex = $codeMatch.Index
    $braceStart = $codeMatch.Index + $codeMatch.Length - 1
    
    # Encontrar el cierre balanceado de llaves
    $braceCount = 1
    $currentIndex = $braceStart + 1
    $contentLength = $Content.Length
    
    while ($braceCount -gt 0 -and $currentIndex -lt $contentLength) {
        $char = $Content[$currentIndex]
        if ($char -eq '{') { $braceCount++ }
        elseif ($char -eq '}') { $braceCount-- }
        $currentIndex++
    }
    
    # Extraer el contenido del bloque @code (sin las llaves externas)
    $codeContent = $Content.Substring($braceStart + 1, $currentIndex - $braceStart - 2)
    
    # Markup es todo antes de @code
    $markup = $Content.Substring(0, $startIndex).TrimEnd()
    
    return @{
        HasCode = $true
        Markup = $markup
        CodeContent = $codeContent
        FullCodeBlock = $Content.Substring($startIndex, $currentIndex - $startIndex)
    }
}

function Extract-Usings {
    param([string]$Content)
    
    $usings = @()
    $matches = [regex]::Matches($Content, '@using\s+([^\r\n]+)')
    foreach ($match in $matches) {
        $usingStatement = $match.Groups[1].Value.Trim()
        $usings += "using $usingStatement;"
    }
    return $usings
}

function Extract-Injects {
    param([string]$Content)
    
    $injects = @()
    $matches = [regex]::Matches($Content, '@inject\s+(\S+)\s+(\S+)')
    foreach ($match in $matches) {
        $type = $match.Groups[1].Value
        $name = $match.Groups[2].Value
        $injects += "    [Inject] private $type $name { get; set; } = default!;"
    }
    return $injects
}

function Extract-Implements {
    param([string]$Content)
    
    $implements = @()
    $matches = [regex]::Matches($Content, '@implements\s+(\S+)')
    foreach ($match in $matches) {
        $implements += $match.Groups[1].Value
    }
    return $implements
}

function Create-CodeBehindFile {
    param(
        [string]$RazorPath,
        [string]$CodeContent,
        [string]$OriginalContent
    )
    
    $className = Get-ClassNameFromFile -FilePath $RazorPath
    $ns = Get-NamespaceFromPath -FilePath $RazorPath
    
    # Extraer usings del archivo original
    $usings = Extract-Usings -Content $OriginalContent
    
    # Extraer injects para convertir a [Inject]
    $injects = Extract-Injects -Content $OriginalContent
    
    # Extraer implements
    $implements = Extract-Implements -Content $OriginalContent
    
    # Construir la declaración de clase
    $classDeclaration = "public partial class $className"
    if ($implements.Count -gt 0) {
        $classDeclaration += " : " + ($implements -join ", ")
    }
    
    # Construir el archivo .razor.cs
    $sb = [System.Text.StringBuilder]::new()
    
    # Usings estándar
    [void]$sb.AppendLine("using Microsoft.AspNetCore.Components;")
    [void]$sb.AppendLine("using Microsoft.JSInterop;")
    
    # Usings extraídos del archivo
    foreach ($using in $usings) {
        if ($using -notmatch "Microsoft.AspNetCore.Components") {
            [void]$sb.AppendLine($using)
        }
    }
    
    [void]$sb.AppendLine("")
    [void]$sb.AppendLine("namespace $ns;")
    [void]$sb.AppendLine("")
    [void]$sb.AppendLine($classDeclaration)
    [void]$sb.AppendLine("{")
    
    # Agregar Injects como propiedades
    if ($injects.Count -gt 0) {
        [void]$sb.AppendLine("    // Servicios inyectados")
        foreach ($inject in $injects) {
            [void]$sb.AppendLine($inject)
        }
        [void]$sb.AppendLine("")
    }
    
    # Agregar el contenido del código (indentado)
    $codeLines = $CodeContent -split "`n"
    foreach ($line in $codeLines) {
        # Mantener la línea tal cual (ya tiene indentación)
        [void]$sb.AppendLine($line.TrimEnd())
    }
    
    [void]$sb.AppendLine("}")
    
    return $sb.ToString()
}

function Process-RazorFile {
    param([string]$FilePath)
    
    Write-Host "Procesando: $FilePath" -ForegroundColor Cyan
    
    # Leer contenido
    $content = Get-Content -Path $FilePath -Raw -Encoding UTF8
    
    # Extraer bloque @code
    $extracted = Extract-CodeBlock -Content $content
    
    if (-not $extracted.HasCode) {
        Write-Host "  -> Sin bloque @code, omitiendo" -ForegroundColor Yellow
        return $false
    }
    
    # Crear archivo .razor.cs
    $codeBehindPath = "$FilePath.cs"
    $codeBehindContent = Create-CodeBehindFile -RazorPath $FilePath -CodeContent $extracted.CodeContent -OriginalContent $content
    
    # Guardar .razor.cs
    Set-Content -Path $codeBehindPath -Value $codeBehindContent -Encoding UTF8
    Write-Host "  -> Creado: $codeBehindPath" -ForegroundColor Green
    
    # Actualizar .razor (remover bloque @code)
    $newMarkup = $extracted.Markup
    Set-Content -Path $FilePath -Value $newMarkup -Encoding UTF8
    Write-Host "  -> Actualizado: $FilePath (solo markup)" -ForegroundColor Green
    
    return $true
}

# ==============================================
# EJECUCIÓN PRINCIPAL
# ==============================================

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "SEPARACIÓN DE MARKUP Y LÓGICA BLAZOR" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

# Carpetas a procesar
$folders = @(
    "$ComponentsPath\Pages",
    "$ComponentsPath\Shared",
    "$ComponentsPath\Layout"
)

foreach ($folder in $folders) {
    if (-not (Test-Path $folder)) {
        Write-Host "Carpeta no encontrada: $folder" -ForegroundColor Red
        continue
    }
    
    Write-Host ""
    Write-Host "=== Procesando: $folder ===" -ForegroundColor Yellow
    
    $razorFiles = Get-ChildItem -Path $folder -Filter "*.razor" -File
    
    foreach ($file in $razorFiles) {
        # Omitir archivos que ya tienen code-behind
        $codeBehindExists = Test-Path "$($file.FullName).cs"
        if ($codeBehindExists) {
            Write-Host "  $($file.Name) -> Ya tiene .razor.cs, omitiendo" -ForegroundColor DarkGray
            $skipped++
            continue
        }
        
        try {
            $result = Process-RazorFile -FilePath $file.FullName
            if ($result) { $processed++ } else { $skipped++ }
        }
        catch {
            Write-Host "  ERROR en $($file.Name): $_" -ForegroundColor Red
            $errors += $file.FullName
        }
    }
}

# Resumen
Write-Host ""
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "RESUMEN" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "Procesados: $processed" -ForegroundColor Green
Write-Host "Omitidos: $skipped" -ForegroundColor Yellow
Write-Host "Errores: $($errors.Count)" -ForegroundColor Red

if ($errors.Count -gt 0) {
    Write-Host ""
    Write-Host "Archivos con errores:" -ForegroundColor Red
    foreach ($err in $errors) {
        Write-Host "  - $err" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Proceso completado. Ejecuta 'dotnet build' para verificar." -ForegroundColor Cyan
