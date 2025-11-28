# 🔄 Sistema de Actualizaciones Automáticas - SGRRHH

## 📖 Descripción General

El sistema de actualizaciones permite que las 3 PCs (Servidor, Ingeniera, Secretaria) detecten automáticamente cuando hay una nueva versión disponible y la instalen de forma sencilla.

### ¿Cómo funciona?

```
┌─────────────────────────────────────────────────────────────────────┐
│                     FLUJO DE ACTUALIZACIÓN                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│   1. TÚ (Servidor) compilas y publicas nueva versión                │
│      └─► Ejecutas: .\Publish-Update.ps1 -Version "1.1.0"            │
│                                                                      │
│   2. Los archivos se copian a:                                      │
│      └─► C:\SGRRHH_Data\updates\                                    │
│          ├── version.json  (info de la versión)                     │
│          └── latest\       (archivos de la app)                     │
│                                                                      │
│   3. Las otras PCs, al iniciar SGRRHH:                              │
│      └─► Leen version.json de \\SERVIDOR\SGRRHH\updates             │
│      └─► Comparan con su versión local                              │
│      └─► Si hay nueva versión → muestran diálogo                    │
│                                                                      │
│   4. Usuario acepta actualizar:                                     │
│      └─► Descarga archivos a carpeta temporal                       │
│      └─► Cierra la app                                              │
│      └─► Ejecuta script que reemplaza archivos                      │
│      └─► Reinicia la app con la nueva versión                       │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🚀 Cómo Publicar una Nueva Versión

### Paso 1: Realizar cambios en el código

Haz los cambios necesarios en el código fuente.

### Paso 2: Probar localmente

```powershell
cd C:\Users\evert\Documents\rrhh\src
dotnet run --project SGRRHH.WPF/SGRRHH.WPF.csproj
```

### Paso 3: Publicar la actualización

```powershell
# Ir a la carpeta de scripts
cd C:\Users\evert\Documents\rrhh\scripts

# Publicar nueva versión
.\Publish-Update.ps1 -Version "1.1.0" -ReleaseNotes "Descripción de los cambios"

# Para actualizaciones críticas (obligatorias):
.\Publish-Update.ps1 -Version "1.1.0" -Mandatory $true -ReleaseNotes "Corrección de seguridad crítica"
```

### Paso 4: Verificar publicación

```powershell
# Verificar que se creó el archivo version.json
Get-Content "C:\SGRRHH_Data\updates\version.json"

# Verificar archivos en latest
Get-ChildItem "C:\SGRRHH_Data\updates\latest" | Select-Object Name, Length
```

---

## ⚙️ Configuración del Sistema de Actualizaciones

### En appsettings.json

```json
{
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true,
    "UpdatesPath": "C:\\SGRRHH_Data\\updates"
  }
}
```

| Propiedad | Descripción | Valor por defecto |
|-----------|-------------|-------------------|
| `Enabled` | Habilita/deshabilita las actualizaciones | `true` |
| `CheckOnStartup` | Verificar actualizaciones al iniciar | `true` |
| `UpdatesPath` | Ruta de la carpeta de actualizaciones | Carpeta compartida + `/updates` |

### Configuración por PC

**Servidor (tu PC):**
```json
"UpdatesPath": "C:\\SGRRHH_Data\\updates"
```

**Ingeniera/Secretaria:**
```json
"UpdatesPath": "\\\\ELITEBOOK-EVERT\\SGRRHH\\updates"
```

---

## 📁 Estructura de la Carpeta updates

```
C:\SGRRHH_Data\updates\
├── version.json          ← Metadatos de la versión actual
├── latest\               ← Archivos de la última versión
│   ├── SGRRHH.exe
│   ├── SGRRHH.dll
│   ├── SGRRHH.deps.json
│   ├── SGRRHH.runtimeconfig.json
│   └── ... otros archivos
└── history\              ← Historial de versiones (opcional)
    ├── 1.0.0\
    ├── 1.1.0\
    └── ...
```

### Contenido de version.json

```json
{
  "version": "1.1.0",
  "releaseDate": "2025-11-27T15:30:00Z",
  "mandatory": false,
  "minimumVersion": "1.0.0",
  "releaseNotes": "## Cambios en v1.1.0\n\n- Nueva funcionalidad X\n- Corrección de error Y\n- Mejora de rendimiento Z",
  "checksum": "sha256:abc123...",
  "downloadSize": 45678900,
  "files": [
    {"name": "SGRRHH.exe", "checksum": "sha256:...", "size": 12345678},
    {"name": "SGRRHH.dll", "checksum": "sha256:...", "size": 9876543}
  ]
}
```

---

## 🖥️ Experiencia del Usuario

Cuando un usuario inicia SGRRHH y hay una actualización disponible:

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  🚀 Nueva Versión Disponible                               │
│                                                             │
│  Versión actual: 1.0.0  →  Nueva versión: 1.1.0            │
│  📅 Publicada: 27/11/2025 15:30                            │
│  💾 Tamaño: 45.6 MB                                        │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ 📋 Notas de la versión                               │ │
│  │                                                       │ │
│  │ ## Cambios en v1.1.0                                 │ │
│  │                                                       │ │
│  │ - Nueva funcionalidad X                              │ │
│  │ - Corrección de error Y                              │ │
│  │ - Mejora de rendimiento Z                            │ │
│  │                                                       │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  [Omitir versión] [Recordar después] [🔄 Actualizar ahora] │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Botones disponibles:

| Botón | Acción |
|-------|--------|
| **Actualizar ahora** | Descarga e instala la actualización |
| **Recordar después** | Omite por esta vez, preguntará en el próximo inicio |
| **Omitir versión** | No volver a preguntar por esta versión específica |

**Nota:** Si la actualización es **obligatoria**, solo aparece el botón "Actualizar ahora".

---

## 🔧 Proceso de Actualización (Técnico)

### 1. Verificación (al iniciar la app)

```csharp
// Lee version.json de la carpeta compartida
var result = await _updateService.CheckForUpdatesAsync();

if (result.UpdateAvailable) {
    // Mostrar diálogo
}
```

### 2. Descarga

- Los archivos se copian de `\\SERVIDOR\SGRRHH\updates\latest\` a una carpeta temporal local
- Se verifican los checksums para asegurar integridad
- **appsettings.json NO se sobrescribe** (mantiene la configuración local)

### 3. Instalación

1. Se crea un script PowerShell temporal
2. La app se cierra
3. El script:
   - Espera que la app se cierre completamente
   - Crea backup de la versión actual
   - Copia los archivos nuevos
   - Limpia archivos temporales
   - Reinicia la app

### 4. Reinicio

La app inicia con la nueva versión. El usuario debe volver a iniciar sesión.

---

## 🛠️ Comandos Útiles

### Ver versión actual instalada

```powershell
# Ver versión en appsettings.json
Get-Content "C:\SGRRHH\appsettings.json" | ConvertFrom-Json | Select-Object -ExpandProperty Application
```

### Crear carpeta de actualizaciones (primera vez)

```powershell
New-Item -Path "C:\SGRRHH_Data\updates" -ItemType Directory -Force
New-Item -Path "C:\SGRRHH_Data\updates\latest" -ItemType Directory -Force
New-Item -Path "C:\SGRRHH_Data\updates\history" -ItemType Directory -Force
```

### Forzar actualización en un cliente

Si un cliente tiene problemas para actualizar, puedes forzar manualmente:

```powershell
# En la PC cliente
Stop-Process -Name "SGRRHH" -Force -ErrorAction SilentlyContinue

# Copiar archivos manualmente (excepto appsettings.json)
$source = "\\ELITEBOOK-EVERT\SGRRHH\updates\latest"
$dest = "C:\SGRRHH"

Get-ChildItem $source -Recurse | Where-Object { $_.Name -ne "appsettings.json" } | ForEach-Object {
    $destPath = $_.FullName.Replace($source, $dest)
    Copy-Item $_.FullName $destPath -Force
}

# Iniciar la app
Start-Process "C:\SGRRHH\SGRRHH.exe"
```

### Rollback a versión anterior

Si una actualización tiene problemas:

```powershell
# Copiar versión anterior desde historial
$versionAnterior = "1.0.0"
$source = "C:\SGRRHH_Data\updates\history\$versionAnterior"
$dest = "C:\SGRRHH_Data\updates\latest"

# Limpiar latest
Remove-Item "$dest\*" -Recurse -Force

# Copiar versión anterior
Copy-Item "$source\*" $dest -Recurse -Force

# Actualizar version.json
$versionInfo = Get-Content "C:\SGRRHH_Data\updates\version.json" | ConvertFrom-Json
$versionInfo.version = $versionAnterior
$versionInfo | ConvertTo-Json -Depth 10 | Set-Content "C:\SGRRHH_Data\updates\version.json"
```

---

## ⚠️ Consideraciones Importantes

### Archivos que NO se actualizan

- `appsettings.json` - Cada PC mantiene su configuración local
- Archivos en `data/` - Logs y datos locales
- Base de datos - La BD está en la carpeta compartida, no en la carpeta de la app

### Requisitos para que funcione

1. ✅ La carpeta `\\SERVIDOR\SGRRHH\updates` debe ser accesible por todos los usuarios
2. ✅ Los usuarios deben tener permisos de lectura en esa carpeta
3. ✅ El servidor debe estar encendido para que los clientes puedan verificar actualizaciones
4. ✅ La red WiFi/Ethernet debe estar funcionando

### Si falla la actualización

1. Los archivos originales permanecen intactos
2. Se crea un backup antes de actualizar
3. El usuario puede reintentar o continuar con la versión actual

---

## 📝 Checklist para Publicar Actualización

- [ ] Realizar cambios en el código
- [ ] Probar localmente que funcione
- [ ] Actualizar número de versión (si no usas el script)
- [ ] Escribir notas de versión
- [ ] Ejecutar `Publish-Update.ps1`
- [ ] Verificar `version.json` creado
- [ ] Probar actualización en una PC cliente
- [ ] Comunicar a usuarios sobre la actualización

---

*Última actualización: Noviembre 2025*
