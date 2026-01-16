# Crear acceso directo para SGRRHH - Consola
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\SGRRHH - Consola.lnk")
$Shortcut.TargetPath = "C:\SGRRHH\IniciarServidor.bat"
$Shortcut.WorkingDirectory = "C:\SGRRHH"
$Shortcut.Description = "Inicia SGRRHH con logs visibles y abre navegador"
$Shortcut.IconLocation = "C:\SGRRHH\SGRRHH.Local.Server.exe,0"
$Shortcut.Save()

Write-Host "Acceso directo creado: SGRRHH - Consola" -ForegroundColor Green

# Eliminar acceso directo antiguo si existe
$oldShortcut = "$env:USERPROFILE\Desktop\SGRRHH - Iniciar.lnk"
if (Test-Path $oldShortcut) {
    Remove-Item $oldShortcut -Force
    Write-Host "Acceso directo antiguo eliminado: SGRRHH - Iniciar" -ForegroundColor Yellow
}
