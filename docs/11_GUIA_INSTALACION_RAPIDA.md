# 🚀 Guía de Instalación Rápida - SGRRHH

Esta guía te permite instalar SGRRHH en los 3 PCs de manera sencilla.

---

## 📋 Antes de Empezar

### Requisitos:
- ✅ Windows 10 o superior
- ✅ Todos los PCs en la **misma red WiFi**
- ✅ El servidor debe estar **encendido** mientras los otros PCs usen la app

### Orden de Instalación:
1. **Primero:** PC Servidor (tu PC)
2. **Después:** PC Ingeniera y PC Secretaria (pueden ser en paralelo)

---

## 🖥️ INSTALACIÓN EN PC SERVIDOR (Tu PC)

### Paso 1: Crear la carpeta de datos

Abre PowerShell como Administrador y ejecuta:

```powershell
# Crear estructura de carpetas
New-Item -Path "C:\SGRRHH_Data" -ItemType Directory -Force
New-Item -Path "C:\SGRRHH_Data\fotos" -ItemType Directory -Force
New-Item -Path "C:\SGRRHH_Data\documentos" -ItemType Directory -Force
New-Item -Path "C:\SGRRHH_Data\backups" -ItemType Directory -Force
New-Item -Path "C:\SGRRHH_Data\config" -ItemType Directory -Force
New-Item -Path "C:\SGRRHH_Data\logs" -ItemType Directory -Force
New-Item -Path "C:\SGRRHH_Data\updates" -ItemType Directory -Force
New-Item -Path "C:\SGRRHH_Data\updates\latest" -ItemType Directory -Force
```

### Paso 2: Compartir la carpeta en red

**Método GUI (más fácil):**
1. Abre **Explorador de archivos** → `C:\SGRRHH_Data`
2. Clic derecho → **Propiedades** → pestaña **Compartir**
3. Clic en **Uso compartido avanzado...**
4. ☑️ Marcar **"Compartir esta carpeta"**
5. Nombre del recurso: `SGRRHH`
6. Clic en **Permisos** → **Todos** → marcar **Control total**
7. **Aceptar** todo

**O método PowerShell (automático):**
```powershell
# Compartir carpeta (requiere admin)
New-SmbShare -Name "SGRRHH" -Path "C:\SGRRHH_Data" -FullAccess "Everyone"

# Verificar que se compartió
Get-SmbShare -Name "SGRRHH"
```

### Paso 3: Obtener IP y nombre del PC

```powershell
# Ver nombre del PC
hostname

# Ver IP (buscar la de WiFi o Ethernet)
Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -match 'Wi-Fi|Ethernet' } | Select-Object IPAddress, InterfaceAlias
```

**Anota estos datos** (los necesitarás para las otras PCs):
- Nombre del PC: `_____________________`
- IP del PC: `_____________________`

### Paso 4: Instalar SGRRHH

**Opción A - Versión Portable:**
```powershell
# Crear carpeta de instalación
New-Item -Path "C:\SGRRHH" -ItemType Directory -Force

# Copiar archivos publicados
Copy-Item -Path "C:\Users\evert\Documents\rrhh\src\publish\SGRRHH\*" -Destination "C:\SGRRHH" -Recurse -Force
```

**Opción B - Usar el instalador (si lo tienes):**
```powershell
# Ejecutar instalador
Start-Process "C:\Users\evert\Documents\rrhh\installer\output\SGRRHH_Setup_1.0.0.exe"
```

### Paso 5: Configurar appsettings.json

Crea/edita el archivo `C:\SGRRHH\appsettings.json`:

```json
{
  "Database": {
    "Path": "C:\\SGRRHH_Data\\sgrrhh.db",
    "EnableWalMode": true,
    "BusyTimeout": 30000
  },
  "Network": {
    "IsNetworkMode": true,
    "SharedFolder": "C:\\SGRRHH_Data"
  },
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true,
    "UpdatesPath": "C:\\SGRRHH_Data\\updates"
  },
  "Application": {
    "Name": "SGRRHH",
    "Version": "1.0.0",
    "Company": "Mi Empresa"
  }
}
```

### Paso 6: Crear acceso directo

```powershell
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\SGRRHH.lnk")
$Shortcut.TargetPath = "C:\SGRRHH\SGRRHH.exe"
$Shortcut.WorkingDirectory = "C:\SGRRHH"
$Shortcut.Save()
```

### Paso 7: Primera ejecución

1. Ejecuta **SGRRHH.exe**
2. Inicia sesión con: `admin` / `admin123`
3. ✅ Si ves el Dashboard, ¡está funcionando!

---

## 👩‍💼 INSTALACIÓN EN PC INGENIERA

### Paso 1: Verificar conexión al servidor

Abre el **Explorador de archivos** y escribe en la barra de direcciones:
```
\\NOMBRE_PC_SERVIDOR\SGRRHH
```
Por ejemplo: `\\ELITEBOOK-EVERT\SGRRHH` o `\\192.168.1.76\SGRRHH`

Si ves las carpetas (backups, config, documentos, etc.), ¡la conexión funciona!

### Paso 2: Instalar SGRRHH

**Opción más fácil - Copiar desde servidor:**
```powershell
# Crear carpeta local
New-Item -Path "C:\SGRRHH" -ItemType Directory -Force

# Copiar desde el servidor (ajustar nombre/IP del servidor)
Copy-Item -Path "\\ELITEBOOK-EVERT\SGRRHH\..\SGRRHH_App\*" -Destination "C:\SGRRHH" -Recurse -Force
```

**O copiar manualmente** el contenido de la carpeta SGRRHH desde USB.

### Paso 3: Configurar appsettings.json

Crea el archivo `C:\SGRRHH\appsettings.json`:

```json
{
  "Database": {
    "Path": "\\\\ELITEBOOK-EVERT\\SGRRHH\\sgrrhh.db",
    "EnableWalMode": true,
    "BusyTimeout": 30000
  },
  "Network": {
    "IsNetworkMode": true,
    "SharedFolder": "\\\\ELITEBOOK-EVERT\\SGRRHH"
  },
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true,
    "UpdatesPath": "\\\\ELITEBOOK-EVERT\\SGRRHH\\updates"
  },
  "Application": {
    "Name": "SGRRHH",
    "Version": "1.0.0",
    "Company": "Mi Empresa"
  }
}
```

⚠️ **Importante:** Reemplaza `ELITEBOOK-EVERT` con el nombre o IP real del servidor.

### Paso 4: Crear acceso directo

```powershell
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\SGRRHH.lnk")
$Shortcut.TargetPath = "C:\SGRRHH\SGRRHH.exe"
$Shortcut.WorkingDirectory = "C:\SGRRHH"
$Shortcut.Save()
```

### Paso 5: Iniciar sesión

- **Usuario:** `ingeniera`
- **Contraseña:** `ingeniera123`

---

## 👩‍💻 INSTALACIÓN EN PC SECRETARIA

Sigue **exactamente los mismos pasos que PC Ingeniera**, pero usa estas credenciales:

- **Usuario:** `secretaria`
- **Contraseña:** `secretaria123`

El archivo `appsettings.json` es idéntico al de la ingeniera.

---

## ✅ Verificación Final

### Lista de verificación:

| Verificación | Servidor | Ingeniera | Secretaria |
|--------------|:--------:|:---------:|:----------:|
| SGRRHH.exe instalado | ☐ | ☐ | ☐ |
| appsettings.json configurado | ☐ | ☐ | ☐ |
| Puede iniciar sesión | ☐ | ☐ | ☐ |
| Ve el Dashboard | ☐ | ☐ | ☐ |
| Puede crear/ver empleados | ☐ | ☐ | ☐ |

### Prueba de concurrencia:

1. En **PC Secretaria**: Crea un empleado nuevo
2. En **PC Ingeniera**: Refresca la lista de empleados
3. ✅ El nuevo empleado debe aparecer inmediatamente

---

## 🆘 Solución de Problemas Rápida

### "No puedo acceder a \\SERVIDOR\SGRRHH"

1. Verifica que el servidor está encendido
2. Verifica que están en la misma red WiFi
3. En el servidor, ejecuta: `Get-SmbShare -Name "SGRRHH"`
4. Prueba con IP en vez del nombre: `\\192.168.1.x\SGRRHH`

### "La base de datos está bloqueada"

- Espera 5 segundos e intenta de nuevo
- Es normal si otro usuario está guardando cambios

### "Error de conexión a la base de datos"

- Verifica el archivo `appsettings.json`
- Asegúrate de usar **4 barras invertidas** (`\\\\`) para rutas de red
- Verifica que el archivo `sgrrhh.db` existe en el servidor

### "La app no inicia"

1. Abre PowerShell en la carpeta de SGRRHH
2. Ejecuta: `.\SGRRHH.exe`
3. Lee el mensaje de error que aparece

---

## 📦 Paquete de Instalación para Clientes

Para facilitar la instalación en las PCs de Ingeniera y Secretaria, puedes crear un paquete:

### Crear paquete de instalación:

```powershell
# En el servidor, crear carpeta con todo lo necesario
$packagePath = "C:\SGRRHH_Data\SGRRHH_Instalacion"
New-Item -Path $packagePath -ItemType Directory -Force

# Copiar archivos de la app
Copy-Item -Path "C:\SGRRHH\*" -Destination $packagePath -Recurse -Force

# Crear script de instalación
@"
# Script de instalación para PC cliente
# Ejecutar como Administrador

# Crear carpeta
New-Item -Path "C:\SGRRHH" -ItemType Directory -Force

# Copiar archivos
Copy-Item -Path ".\*" -Destination "C:\SGRRHH" -Recurse -Force

# Crear acceso directo
`$WshShell = New-Object -comObject WScript.Shell
`$Shortcut = `$WshShell.CreateShortcut("`$env:USERPROFILE\Desktop\SGRRHH.lnk")
`$Shortcut.TargetPath = "C:\SGRRHH\SGRRHH.exe"
`$Shortcut.WorkingDirectory = "C:\SGRRHH"
`$Shortcut.Save()

Write-Host "✅ Instalación completada. Edita C:\SGRRHH\appsettings.json para configurar la conexión al servidor."
"@ | Out-File -FilePath "$packagePath\Instalar.ps1" -Encoding UTF8
```

Luego, desde las otras PCs:
```powershell
# Acceder al paquete desde red
cd "\\ELITEBOOK-EVERT\SGRRHH\SGRRHH_Instalacion"

# Ejecutar instalador
powershell -ExecutionPolicy Bypass -File .\Instalar.ps1
```

---

*Última actualización: Noviembre 2025*
