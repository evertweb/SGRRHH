# Crear acceso directo para SGRRHH
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\SGRRHH - Iniciar.lnk")
$Shortcut.TargetPath = "C:\SGRRHH\IniciarServidor.bat"
$Shortcut.WorkingDirectory = "C:\SGRRHH"
$Shortcut.Description = "Inicia SGRRHH con logs visibles"
$Shortcut.Save()

Write-Host "Acceso directo creado en el escritorio: SGRRHH - Iniciar" -ForegroundColor Green
