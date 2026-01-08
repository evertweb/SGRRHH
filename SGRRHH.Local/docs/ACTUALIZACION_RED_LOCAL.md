# Estrategias de Actualización en Red Local - SGRRHH Local

## Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────────────────┐
│                         RED LOCAL                                │
│                                                                  │
│   ┌─────────────┐         ┌─────────────┐                       │
│   │   SERVIDOR  │         │   CLIENTES  │                       │
│   │ (Tu PC Dev) │ ◄─────► │ (Navegador) │                       │
│   │             │         │             │                       │
│   │ Blazor      │  HTTP   │ Chrome/Edge │                       │
│   │ Server      │ :5001   │ Firefox     │                       │
│   └─────────────┘         └─────────────┘                       │
│         │                                                        │
│         │ Carpeta Compartida                                     │
│         ▼                                                        │
│   \\SERVIDOR\SGRRHH\Updates                                     │
│   ├── version.json                                               │
│   ├── SGRRHH.Local-v1.2.0.zip                                   │
│   └── latest/                                                    │
│       └── (archivos descomprimidos)                             │
└─────────────────────────────────────────────────────────────────┘
```

## Escenario: Un Solo Servidor en Red Local

Tu aplicación Blazor Server se ejecuta en **un solo PC** que actúa como servidor. Los demás equipos de la red acceden mediante navegador web a `https://SERVIDOR:5001`.

### Ventajas de Blazor Server para este escenario:
- ✅ **Actualización centralizada**: Solo actualizas el servidor, todos los clientes ven la nueva versión
- ✅ **Sin instalación en clientes**: Solo necesitan un navegador
- ✅ **Datos centralizados**: La base de datos SQLite está en el servidor
- ✅ **Control total**: Puedes actualizar fuera de horario laboral

---

## Estrategia 1: Actualización Manual (Recomendada para Iniciar)

### Flujo de trabajo:
1. Desarrollas en tu PC con VS Code
2. Ejecutas `Task: Build + Actualizar Local`
3. Se compila y copia a `C:\SGRRHH`
4. Reinicias el servidor (automático en el script)
5. Los clientes recargan el navegador (F5) y ven la nueva versión

### Comandos:
```powershell
# Desde VS Code: Ctrl+Shift+B (Build + Actualizar Local)
# O manualmente:
.\SGRRHH.Local\scripts\Publish-Local.ps1 -Release
```

---

## Estrategia 2: Carpeta Compartida en Red

### Configuración:
1. Crea una carpeta compartida en el servidor: `C:\SGRRHH\Updates`
2. Compártela como: `\\SERVIDOR\SGRRHHUpdates`
3. Da permisos de lectura a los usuarios de la red

### Flujo:
```powershell
# En el servidor (tu PC de desarrollo):
.\Create-UpdatePackage.ps1 -Version "1.2.0" -NetworkPath "\\SERVIDOR\SGRRHHUpdates"

# En cualquier cliente que necesite actualizar (si hubiera instalación local):
.\Update-FromNetwork.ps1 -ServerPath "\\SERVIDOR\SGRRHHUpdates"
```

---

## Estrategia 3: Servicio Windows con Auto-Reinicio

Para producción, convierte la aplicación en un **Servicio de Windows** que se reinicia automáticamente:

### Instalación como Servicio:
```powershell
# Crear el servicio
sc.exe create "SGRRHH" binPath="C:\SGRRHH\SGRRHH.Local.Server.exe" start=auto

# Configurar recuperación (reiniciar si falla)
sc.exe failure "SGRRHH" reset=86400 actions=restart/60000/restart/60000/restart/60000

# Iniciar el servicio
sc.exe start "SGRRHH"
```

### Script de actualización con servicio:
```powershell
# Detener servicio
Stop-Service -Name "SGRRHH" -Force

# Actualizar archivos
Copy-Item -Path "\\SERVIDOR\SGRRHHUpdates\latest\*" -Destination "C:\SGRRHH" -Recurse -Force

# Iniciar servicio
Start-Service -Name "SGRRHH"
```

---

## Estrategia 4: Actualización Automática con Verificación

Crea una tarea programada que verifique actualizaciones:

### Script de verificación automática (Check-Updates.ps1):
```powershell
# Se ejecuta cada hora o al inicio de Windows
$serverPath = "\\SERVIDOR\SGRRHHUpdates"
$localVersionFile = "C:\SGRRHH\version.json"

$local = (Get-Content $localVersionFile | ConvertFrom-Json).version
$remote = (Get-Content "$serverPath\version.json" | ConvertFrom-Json).version

if ($remote -gt $local) {
    # Hay actualización disponible
    # Mostrar notificación o actualizar automáticamente
    Write-EventLog -LogName Application -Source "SGRRHH" -EventId 1001 -Message "Nueva versión disponible: $remote"
}
```

### Tarea programada:
```powershell
# Crear tarea que verifica cada hora
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\SGRRHH\scripts\Check-Updates.ps1"
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Hours 1)
Register-ScheduledTask -TaskName "SGRRHH-CheckUpdates" -Action $action -Trigger $trigger
```

---

## Estrategia 5: Panel de Administración en la App

Implementa un endpoint en tu aplicación Blazor para gestionar actualizaciones:

### Componente de Administración (UpdateManager.razor):
```razor
@page "/admin/updates"
@attribute [Authorize(Roles = "Administrador")]

<h3>Gestión de Actualizaciones</h3>

<div class="panel">
    <p>Versión actual: @currentVersion</p>
    <p>Última verificación: @lastCheck</p>
    
    @if (updateAvailable)
    {
        <div class="alert-warning">
            Nueva versión disponible: @newVersion
            <button @onclick="ApplyUpdate">Aplicar Actualización</button>
        </div>
    }
</div>

@code {
    private async Task ApplyUpdate()
    {
        // 1. Mostrar mensaje a usuarios conectados
        // 2. Esperar 5 minutos
        // 3. Descargar actualización
        // 4. Reiniciar aplicación
    }
}
```

---

## Recomendación Final para Tu Caso

Dado que es **un solo servidor** y los clientes acceden por navegador:

### Fase 1 (Desarrollo):
- Usa `dotnet watch run` para desarrollo con Hot Reload
- Los cambios CSS se reflejan automáticamente

### Fase 2 (Pruebas):
- Usa la tarea "Build + Actualizar Local"
- Prueba con `C:\SGRRHH\SGRRHH.Local.Server.exe`

### Fase 3 (Producción):
1. Instala como **Servicio de Windows**
2. Configura el firewall para el puerto 5001
3. Los clientes acceden a `https://NOMBRE-SERVIDOR:5001`
4. Para actualizar:
   - Ejecuta `Create-UpdatePackage.ps1`
   - Detén el servicio
   - Copia los archivos
   - Reinicia el servicio

### Configuración del Firewall:
```powershell
# Permitir acceso al puerto 5001
New-NetFirewallRule -DisplayName "SGRRHH Local Server" -Direction Inbound -Protocol TCP -LocalPort 5001 -Action Allow
```

---

## Estructura de Archivos

```
C:\SGRRHH\
├── SGRRHH.Local.Server.exe    # Ejecutable principal
├── appsettings.json           # Configuración
├── version.json               # Versión actual
├── wwwroot/                   # Archivos estáticos (CSS, JS)
├── Data/                      # Base de datos SQLite (NO sobrescribir)
├── Storage/                   # Archivos subidos (NO sobrescribir)
├── Logs/                      # Logs de la aplicación
└── Backups/                   # Backups automáticos
```

## Notas Importantes

1. **NUNCA sobrescribas**: `Data/`, `Storage/`, `Logs/`, `Backups/`
2. **Siempre haz backup** antes de actualizar
3. **Verifica la versión** después de actualizar
4. **Prueba en desarrollo** antes de producción
