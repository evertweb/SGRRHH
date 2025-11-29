# 🚀 Guía de Instalación Rápida - SGRRHH

Esta guía te permite instalar SGRRHH en los 3 PCs de manera sencilla.

---

## 📋 Antes de Empezar

### Requisitos:
- ✅ Windows 10 o superior
- ✅ Conexión a internet (para Firebase y actualizaciones)
- ✅ .NET 8 Runtime instalado ([Descargar aquí](https://dotnet.microsoft.com/download/dotnet/8.0))

### Archivos necesarios:
- ZIP de distribución (`SGRRHH_v1.1.x_Distribucion.zip`)
- Credenciales Firebase (`firebase-credentials.json`)

---

## 🖥️ INSTALACIÓN EN CUALQUIER PC

### Paso 1: Instalar .NET 8 Runtime

Si no está instalado, descarga e instala desde:
https://dotnet.microsoft.com/download/dotnet/8.0

Descarga **".NET Desktop Runtime 8.x"** (no el SDK completo).

### Paso 2: Crear carpeta de instalación

```powershell
# Crear carpeta
New-Item -Path "C:\SGRRHH" -ItemType Directory -Force
```

### Paso 3: Descomprimir la aplicación

1. Descomprime el ZIP de distribución en `C:\SGRRHH`
2. Verifica que existan estos archivos:
   - `SGRRHH.exe`
   - `SGRRHH.dll`
   - `appsettings.json`
   - `SGRRHH.Updater.exe`

### Paso 4: Configurar credenciales Firebase

1. Copia `firebase-credentials.json` a `C:\SGRRHH\`
2. Verifica que `appsettings.json` tenga la ruta correcta:

```json
{
  "Firebase": {
    "ProjectId": "sgrrhh-xxxxx",
    "CredentialsPath": "firebase-credentials.json"
  },
  "Application": {
    "Name": "SGRRHH",
    "Version": "1.1.3",
    "Company": "Mi Empresa"
  },
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true,
    "Repository": "evertweb/SGRRHH"
  }
}
```

### Paso 5: Crear acceso directo

```powershell
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\SGRRHH.lnk")
$Shortcut.TargetPath = "C:\SGRRHH\SGRRHH.exe"
$Shortcut.WorkingDirectory = "C:\SGRRHH"
$Shortcut.Save()
```

### Paso 6: Primera ejecución

1. Ejecuta **SGRRHH.exe**
2. Si hay una actualización disponible, se mostrará un diálogo
3. Inicia sesión con el usuario correspondiente:

| PC | Usuario | Contraseña |
|----|---------|------------|
| Servidor | `admin` | `admin123` |
| Ingeniera | `ingeniera` | `ingeniera123` |
| Secretaria | `secretaria` | `secretaria123` |

⚠️ **Importante:** Cambia las contraseñas por defecto después del primer inicio.

---

## 🔄 Actualizaciones Automáticas

A partir de la instalación inicial, las actualizaciones son **completamente automáticas**:

1. Al abrir la app, verifica si hay nueva versión en GitHub
2. Si hay actualización, muestra un diálogo con las opciones:
   - **Actualizar ahora** - Descarga e instala inmediatamente
   - **Recordar después** - Pregunta en el próximo inicio
3. La actualización se descarga (~12 MB) y se aplica automáticamente
4. La app se reinicia con la nueva versión

**No necesitas hacer nada manualmente** - las actualizaciones llegan solas.

---

## ✅ Verificación Final

### Lista de verificación:

| Verificación | ☐ |
|--------------|---|
| .NET 8 Runtime instalado | ☐ |
| Archivos copiados en C:\SGRRHH | ☐ |
| firebase-credentials.json presente | ☐ |
| Acceso directo creado | ☐ |
| Puede iniciar sesión | ☐ |
| Ve el Dashboard | ☐ |

---

## 🆘 Solución de Problemas Rápida

### "La aplicación no inicia"

1. Verifica que .NET 8 Runtime esté instalado:
   ```powershell
   dotnet --list-runtimes
   ```
   Debe mostrar `Microsoft.NETCore.App 8.x.x`

2. Verifica los archivos en C:\SGRRHH

### "Error de Firebase / No se puede conectar"

1. Verifica conexión a internet
2. Verifica que `firebase-credentials.json` exista
3. Verifica que `appsettings.json` tenga el `ProjectId` correcto

### "La actualización falla"

1. Cierra todas las instancias de SGRRHH
2. Revisa `C:\SGRRHH\updater_log.txt` para ver el error
3. Si persiste, descarga el ZIP manualmente de GitHub Releases

---

*Última actualización: Enero 2025*
