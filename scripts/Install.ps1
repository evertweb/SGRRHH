# SGRRHH Local - Script de Instalación
$InstallPath = "C:\SGRRHH.Local"
$AppName = "SGRRHH Local"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Instalador de $AppName" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Verificar .NET 8 Runtime
$dotnetVersion = dotnet --list-runtimes | Select-String "Microsoft.AspNetCore.App 8"
if (-not $dotnetVersion) {
    Write-Host "ERROR: Se requiere .NET 8 Runtime" -ForegroundColor Red
    Write-Host "Descargue de: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

# Crear directorio de instalación
Write-Host "Creando directorio de instalación..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null

# Copiar archivos
Write-Host "Copiando archivos..." -ForegroundColor Yellow
Copy-Item -Path "./publish/*" -Destination $InstallPath -Recurse -Force

# Crear acceso directo en escritorio
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\$AppName.lnk")
$Shortcut.TargetPath = "$InstallPath\SGRRHH.Local.Server.exe"
$Shortcut.WorkingDirectory = $InstallPath
$Shortcut.IconLocation = "$InstallPath\wwwroot\favicon.ico"
$Shortcut.Save()

Write-Host "" 
Write-Host "Instalación completada!" -ForegroundColor Green
Write-Host "   Ubicación: $InstallPath" -ForegroundColor White
Write-Host "   Acceso directo creado en el escritorio" -ForegroundColor White
Write-Host "" 
Write-Host "Para iniciar, ejecute: $InstallPath\SGRRHH.Local.Server.exe" -ForegroundColor Yellow
