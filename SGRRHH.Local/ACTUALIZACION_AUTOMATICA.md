# Sistema de Actualización Automática - SGRRHH

## Arquitectura

```
┌────────────────────┐     ┌──────────────────────┐     ┌─────────────────────┐
│   PC DESARROLLO    │     │   CARPETA DE RED     │     │    PC SERVIDOR      │
│                    │     │                      │     │                     │
│  ┌──────────────┐  │     │  ┌────────────────┐  │     │  ┌───────────────┐  │
│  │ VS Code      │  │     │  │ SGRRHHUpdates  │  │     │  │ SGRRHH        │  │
│  │ Codigo       │──┼────>│  │ ├─version.json │──┼────>│  │ Aplicacion    │  │
│  │ fuente       │  │     │  │ └─latest/      │  │     │  │ en ejecucion  │  │
│  └──────────────┘  │     │  │   └─archivos   │  │     │  └───────────────┘  │
│                    │     │  └────────────────┘  │     │         ↑           │
│  Publish-Update.ps1│     │                      │     │  Tarea Programada   │
│                    │     │                      │     │  (cada hora)        │
└────────────────────┘     └──────────────────────┘     └─────────────────────┘
```

## Flujo de Trabajo

### Desde el PC de Desarrollo

1. **Haces cambios en el código**
2. **Ejecutas la tarea "Publicar Actualización"** en VS Code
3. El script:
   - Compila el proyecto en modo Release
   - Incrementa la versión automáticamente
   - Copia los archivos a la carpeta de red compartida
   - Crea un archivo `version.json` con metadatos

### En el PC Servidor (Automático)

1. **Tarea programada** se ejecuta cada hora (o al intervalo configurado)
2. El script verifica si hay nueva versión comparando `version.json`
3. Si hay actualización:
   - Crea backup de la base de datos
   - Detiene el servidor
   - Aplica los nuevos archivos
   - Reinicia el servidor
4. Todo queda registrado en logs

---

## Instalación Inicial

### Paso 1: Configurar Carpeta Compartida (PC Desarrollo)

```powershell
# En el PC de desarrollo, crear y compartir carpeta:
New-Item -ItemType Directory -Path "C:\SGRRHHUpdates" -Force

# Compartir la carpeta (requiere admin)
New-SmbShare -Name "SGRRHHUpdates" -Path "C:\SGRRHHUpdates" -FullAccess "Everyone"
```

### Paso 2: Verificar Acceso desde Servidor

```powershell
# En el PC servidor, verificar que puede acceder:
Test-Path "\\NOMBRE-PC-DESARROLLO\SGRRHHUpdates"

# Si da False, verificar:
# - Firewall de Windows
# - El PC de desarrollo está encendido
# - Credenciales de red
```

### Paso 3: Instalar en el Servidor

1. **Copia inicial de la aplicación a `C:\SGRRHH`**

2. **Copiar scripts de actualización:**
   ```powershell
   # En el servidor:
   xcopy "\\DESARROLLO\SGRRHHUpdates\scripts\*" "C:\SGRRHH\scripts\" /E /Y
   ```

3. **Configurar tarea programada (como Admin):**
   ```powershell
   cd C:\SGRRHH\scripts
   .\Setup-ScheduledTask.ps1 -Frequency Hourly -UpdateSource "\\DESARROLLO\SGRRHHUpdates"
   ```

---

## Uso Diario

### Publicar una Actualización

**Desde VS Code**, ejecuta la tarea: `Publicar Actualización`

O desde terminal:
```powershell
.\scripts\Publish-Update.ps1 -Version "1.2.0" -ReleaseNotes "Nuevas funciones de reportes"
```

### Verificar Estado en el Servidor

```powershell
# Ver log de actualizaciones
Get-Content "C:\SGRRHH\Logs\autoupdate.log" -Tail 50

# Ver historial de actualizaciones
Get-Content "C:\SGRRHH\Logs\update_history.json" | ConvertFrom-Json

# Ver versión actual
Get-Content "C:\SGRRHH\version.json"

# Ejecutar verificación manual
.\scripts\AutoUpdate-Service.ps1 -CheckOnly
```

### Forzar Actualización Manual

```powershell
# Aplicar aunque sea la misma versión
.\scripts\AutoUpdate-Service.ps1 -Force
```

---

## Scripts Disponibles

| Script | Ubicación | Propósito |
|--------|-----------|-----------|
| `Publish-Update.ps1` | PC Desarrollo | Compila y publica a carpeta de red |
| `AutoUpdate-Service.ps1` | PC Servidor | Verifica y aplica actualizaciones |
| `Setup-ScheduledTask.ps1` | PC Servidor | Configura tarea programada |
| `Backup-Database.ps1` | Ambos | Backup manual de BD |
| `Deploy-Production.ps1` | PC Desarrollo | Despliegue inicial completo |

---

## Estructura de Archivos

### Carpeta de Red (\\DESARROLLO\SGRRHHUpdates)

```
SGRRHHUpdates/
├── version.json          # Versión actual disponible
├── latest/               # Archivos de la última versión
│   ├── SGRRHH.Local.Server.exe
│   ├── SGRRHH.Local.Server.dll
│   ├── wwwroot/
│   └── ...
└── archive/              # Versiones anteriores (opcional)
    ├── 1.0.0/
    └── 1.1.0/
```

### version.json

```json
{
  "version": "1.2.0",
  "releaseNotes": "Correcciones de bugs y mejoras de rendimiento",
  "publishDate": "2025-01-15T10:30:00",
  "mandatory": false,
  "minVersion": "1.0.0"
}
```

### PC Servidor (C:\SGRRHH)

```
SGRRHH/
├── SGRRHH.Local.Server.exe    # Ejecutable principal
├── version.json               # Versión instalada
├── appsettings.json           # Configuración (NO se sobrescribe)
├── Data/                      # Base de datos (NO se sobrescribe)
│   ├── sgrrhh.db
│   └── Backups/               # Backups automáticos
├── Logs/                      # Logs de la aplicación y actualizaciones
│   ├── autoupdate.log
│   └── update_history.json
├── scripts/                   # Scripts de mantenimiento
│   └── AutoUpdate-Service.ps1
└── wwwroot/                   # Archivos web
```

---

## Solución de Problemas

### El servidor no puede acceder a la carpeta de red

```powershell
# Verificar conectividad
Test-NetConnection -ComputerName "NOMBRE-PC-DESARROLLO" -Port 445

# Verificar que el share existe
Get-SmbShare -Name "SGRRHHUpdates" # En PC desarrollo

# Mapear unidad manualmente (temporal)
net use Z: "\\DESARROLLO\SGRRHHUpdates" /persistent:yes
```

### La tarea programada no se ejecuta

```powershell
# Ver estado
Get-ScheduledTask -TaskName "SGRRHH-AutoUpdate"

# Ver último resultado
Get-ScheduledTaskInfo -TaskName "SGRRHH-AutoUpdate"

# Ejecutar manualmente
Start-ScheduledTask -TaskName "SGRRHH-AutoUpdate"

# Ver log de eventos
Get-WinEvent -LogName "Microsoft-Windows-TaskScheduler/Operational" -MaxEvents 20
```

### El servidor no inicia después de actualizar

```powershell
# 1. Verificar proceso
Get-Process -Name "SGRRHH.Local.Server" -ErrorAction SilentlyContinue

# 2. Iniciar manualmente
cd C:\SGRRHH
.\SGRRHH.Local.Server.exe

# 3. Ver logs de la aplicación
Get-Content "C:\SGRRHH\Logs\*" -Tail 100

# 4. Restaurar backup si es necesario
$backups = Get-ChildItem "C:\SGRRHH\Data\Backups" -Filter "*.db" | Sort-Object LastWriteTime -Descending
Copy-Item $backups[0].FullName "C:\SGRRHH\Data\sgrrhh.db" -Force
```

### Rollback a versión anterior

```powershell
# 1. Detener servidor
Stop-Process -Name "SGRRHH.Local.Server" -Force

# 2. Restaurar desde archivo de versiones
# (Si mantienes archive de versiones en la carpeta de red)
Copy-Item "\\DESARROLLO\SGRRHHUpdates\archive\1.1.0\*" "C:\SGRRHH\" -Recurse -Force

# 3. Reiniciar
Start-Process "C:\SGRRHH\SGRRHH.Local.Server.exe"
```

---

## Configuración Avanzada

### Cambiar Frecuencia de Verificación

```powershell
# Cada hora (default)
.\Setup-ScheduledTask.ps1 -Frequency Hourly

# Una vez al día a las 6 AM
.\Setup-ScheduledTask.ps1 -Frequency Daily -Hour 6

# Solo al iniciar Windows
.\Setup-ScheduledTask.ps1 -Frequency Startup
```

### Deshabilitar Actualizaciones Temporalmente

```powershell
# Deshabilitar
Disable-ScheduledTask -TaskName "SGRRHH-AutoUpdate"

# Habilitar de nuevo
Enable-ScheduledTask -TaskName "SGRRHH-AutoUpdate"
```

### Notificaciones por Email (Avanzado)

Puedes modificar `AutoUpdate-Service.ps1` para agregar notificaciones:

```powershell
# Agregar al final del script después de actualización exitosa:
$mailParams = @{
    From = "servidor@tuempresa.com"
    To = "admin@tuempresa.com"
    Subject = "SGRRHH Actualizado a v$($remoteInfo.Version)"
    Body = "Actualización aplicada exitosamente.`nNotas: $($remoteInfo.Notes)"
    SmtpServer = "smtp.tuempresa.com"
}
Send-MailMessage @mailParams
```

---

## Seguridad

1. **La carpeta de red solo debe ser accesible por usuarios autorizados**
2. **El script se ejecuta como SYSTEM** - tiene permisos elevados
3. **Los backups se crean automáticamente** antes de cada actualización
4. **Solo se actualizan archivos de la aplicación** - los datos (DB, configs) se preservan

---

## Tareas VS Code Relacionadas

En VS Code tienes estas tareas disponibles:

- **Publicar Actualización** - Compila y sube a la carpeta de red
- **Ejecutar SGRRHH** - Inicia la aplicación localmente
- **Ver Versiones** - Muestra versiones local/remota

Para ejecutar: `Ctrl+Shift+P` → "Tasks: Run Task" → Seleccionar tarea
