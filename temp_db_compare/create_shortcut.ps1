$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("C:\Users\Public\Desktop\SGRRHH.lnk")
$Shortcut.TargetPath = "C:\SGRRHH\SGRRHH.Local.Server.exe"
$Shortcut.WorkingDirectory = "C:\SGRRHH"
$Shortcut.Description = "SGRRHH Local Server"
$Shortcut.Save()
Write-Host "Acceso directo creado en el escritorio publico"
