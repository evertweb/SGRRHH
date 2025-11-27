$desktop = [Environment]::GetFolderPath('Desktop')
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$desktop\SGRRHH.lnk")
$Shortcut.TargetPath = "C:\SGRRHH\SGRRHH.exe"
$Shortcut.WorkingDirectory = "C:\SGRRHH"
$Shortcut.Description = "Sistema de Gestion de Recursos Humanos"
$Shortcut.Save()
Write-Host "Acceso directo creado en el escritorio"
