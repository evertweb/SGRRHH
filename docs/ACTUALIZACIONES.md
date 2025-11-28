# 🔄 Sistema de Actualizaciones SGRRHH - Guía Completa

> **Este es el documento único y oficial** para todo lo relacionado con actualizaciones.
> Documentos obsoletos: `12_SISTEMA_ACTUALIZACIONES.md`, `14_ACTUALIZACIONES_FIREBASE.md`, `15_GUIA_PUBLICACION.md`

---

## 📋 Índice

1. [Resumen del Sistema](#resumen-del-sistema)
2. [Flujo de Trabajo Recomendado](#flujo-de-trabajo-recomendado)
3. [Scripts Disponibles](#scripts-disponibles)
4. [Tasks de VS Code](#tasks-de-vs-code)
5. [Estructura de Directorios](#estructura-de-directorios)
6. [Cómo Funciona la Detección de Actualizaciones](#cómo-funciona-la-detección-de-actualizaciones)
7. [Configuración](#configuración)
8. [Solución de Problemas](#solución-de-problemas)
9. [Comandos Útiles](#comandos-útiles)

---

## 📍 Resumen del Sistema

### Dos Modos de Actualización

| Modo | Cuándo se usa | Disponibilidad |
|------|---------------|----------------|
| **Firebase Storage** | `DataMode: "Firebase"` | 24/7 (internet) |
| **Carpeta Compartida** | `DataMode: "SQLite"` | Solo cuando servidor está encendido |

### Tres Ubicaciones de Versión

Estas **deben estar sincronizadas** para que todo funcione:

| Ubicación | Archivo | Qué contiene |
|-----------|---------|--------------|
| **Proyecto** | `src/SGRRHH.WPF/SGRRHH.WPF.csproj` | `<Version>X.Y.Z</Version>` |
| **Local** | `C:\SGRRHH\appsettings.json` | `Application.Version` |
| **Firebase** | `gs://bucket/updates/version.json` | `version` |

### Flujo General

```
┌─────────────────────────────────────────────────────────────────┐
│ PUBLICAR ACTUALIZACIÓN                                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Haces cambios en el código                                  │
│                     ↓                                           │
│  2. Ejecutas: Publish-All.ps1 -Version "X.Y.Z"                 │
│                     ↓                                           │
│  3. El script automáticamente:                                  │
│     ✓ Actualiza versión en proyecto                            │
│     ✓ Compila la aplicación                                    │
│     ✓ Sube a Firebase Storage                                  │
│     ✓ Actualiza C:\SGRRHH                                      │
│     ✓ Sincroniza todas las versiones                           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ USUARIOS REMOTOS                                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Abren SGRRHH                                                │
│                     ↓                                           │
│  2. App descarga version.json de Firebase                       │
│                     ↓                                           │
│  3. Compara: versión local < versión Firebase?                  │
│                     ↓                                           │
│  4. Si hay nueva versión → Muestra diálogo                     │
│                     ↓                                           │
│  5. Usuario acepta → Descarga, cierra, actualiza, reinicia     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Flujo de Trabajo Recomendado

### Para Publicar una Actualización Completa

```powershell
# Un solo comando que hace TODO
cd c:\Users\evert\Documents\rrhh\scripts
.\Publish-All.ps1 -Version "1.0.4" -ReleaseNotes "Descripción de cambios" -Incremental
```

**Esto hace automáticamente:**
1. ✅ Actualiza versión en `.csproj` y `appsettings.json`
2. ✅ Compila la aplicación (Release, self-contained)
3. ✅ Sube a Firebase Storage (modo incremental = solo archivos que cambiaron)
4. ✅ Copia a `C:\SGRRHH` (tu instalación local)
5. ✅ Sincroniza todas las versiones

### Solo para Desarrollo Local

```powershell
# Si solo quieres probar cambios sin publicar a Firebase
.\Publish-Local.ps1 -Release
```

### Solo Publicar a Firebase (sin actualizar local)

```powershell
.\Publish-All.ps1 -Version "1.0.4" -ReleaseNotes "..." -SkipLocal -Incremental
```

---

## 📜 Scripts Disponibles

### `Publish-All.ps1` ⭐ (Recomendado)

Script unificado que hace todo en un paso.

```powershell
.\Publish-All.ps1 
    -Version "1.0.4"           # Obligatorio: número de versión
    -ReleaseNotes "Cambios..." # Opcional: descripción
    -Incremental               # Opcional: solo sube archivos modificados
    -Mandatory $true           # Opcional: actualización obligatoria
    -SkipFirebase              # Opcional: no subir a Firebase
    -SkipLocal                 # Opcional: no actualizar C:\SGRRHH
```

### `Publish-Local.ps1`

Solo compila y copia a `C:\SGRRHH`. Útil para desarrollo.

```powershell
.\Publish-Local.ps1 -Release   # Compilación Release
.\Publish-Local.ps1            # Compilación Debug (más rápido)
.\Publish-Local.ps1 -NoBuild   # Solo copiar, no compilar
```

### `Publish-Firebase-Update.ps1`

Solo sube a Firebase (no actualiza local). **Usar `Publish-All.ps1` en su lugar.**

---

## 🖥️ Tasks de VS Code

Presiona `Ctrl+Shift+B` o usa Terminal > Run Task:

| Task | Descripción |
|------|-------------|
| **1. Build + Actualizar Local** | Compila y copia a `C:\SGRRHH` |
| **2. Publicar a Firebase** | Solo sube a Firebase |
| **2b. Publicar TODO** ⭐ | Firebase + Local (RECOMENDADO) |
| **3. Ejecutar SGRRHH** | Abre la app |
| **4. Ver Versiones** | Muestra versiones actuales |

---

## 📁 Estructura de Directorios

```
📦 Proyecto (c:\Users\evert\Documents\rrhh\)
├── 📁 src/
│   ├── 📁 SGRRHH.WPF/
│   │   ├── SGRRHH.WPF.csproj      ← <Version>X.Y.Z</Version>
│   │   └── appsettings.json       ← Application.Version (default)
│   └── 📁 publish/
│       ├── version.json           ← Metadata para Firebase
│       └── 📁 SGRRHH/             ← Archivos compilados
│
├── 📁 scripts/
│   ├── Publish-All.ps1            ← ⭐ Script principal
│   ├── Publish-Local.ps1
│   └── Publish-Firebase-Update.ps1
│
📦 Instalación Local (C:\SGRRHH\)
├── SGRRHH.exe
├── appsettings.json               ← Application.Version (local)
├── firebase-credentials.json
└── 📁 data/                       ← Datos locales
│
☁️ Firebase Storage (gs://rrhh-forestech.firebasestorage.app/)
└── 📁 updates/
    ├── version.json               ← Lo que ven los clientes
    └── 📁 latest/                 ← Archivos para descargar
```

---

## 🔍 Cómo Funciona la Detección de Actualizaciones

### En el Código (`FirebaseUpdateService.cs`)

```csharp
public async Task<UpdateCheckResult> CheckForUpdatesAsync()
{
    // 1. Lee version.json de Firebase Storage
    var serverVersion = await GetRemoteVersionInfoAsync();
    
    // 2. Lee versión local de appsettings.json
    var currentVer = ParseVersion(_currentVersion);  // Ej: "1.0.3"
    var serverVer = ParseVersion(serverVersion.Version);  // Ej: "1.0.4"
    
    // 3. Compara
    if (serverVer > currentVer) {
        // HAY ACTUALIZACIÓN
        result.UpdateAvailable = true;
    }
}
```

### ¿Por qué no detecta mi actualización?

| Causa | Solución |
|-------|----------|
| Versión local = versión Firebase | Incrementa la versión al publicar |
| `appsettings.json` local no actualizado | Usa `Publish-All.ps1` que sincroniza |
| Firebase no actualizado | Verifica con `gcloud storage cat gs://bucket/updates/version.json` |

---

## ⚙️ Configuración

### appsettings.json (en cada PC)

```json
{
  "Firebase": {
    "Enabled": true,
    "ProjectId": "rrhh-forestech",
    "StorageBucket": "rrhh-forestech.firebasestorage.app",
    "CredentialsPath": "firebase-credentials.json"
  },
  
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true
  },
  
  "Application": {
    "Name": "SGRRHH",
    "Version": "1.0.3",    // ← IMPORTANTE: debe ser menor que Firebase para actualizar
    "Company": "Forestech"
  }
}
```

### version.json (en Firebase)

```json
{
  "version": "1.0.4",
  "releaseDate": "2025-11-28T10:30:00Z",
  "mandatory": false,
  "minimumVersion": "1.0.0",
  "releaseNotes": "Cambios en esta versión...",
  "checksum": "sha256:...",
  "downloadSize": 45678900,
  "files": [
    {"name": "SGRRHH.exe", "checksum": "sha256:...", "size": 12345}
  ]
}
```

---

## 🐛 Solución de Problemas

### Mi app local no se actualiza después de publicar

**Causa:** Usaste solo `Publish-Firebase-Update.ps1` que no actualiza `C:\SGRRHH`

**Solución:** 
```powershell
# Usa el script unificado
.\Publish-All.ps1 -Version "1.0.4" -ReleaseNotes "..." -Incremental
```

### Las versiones están desincronizadas

**Verificar:**
```powershell
# Ejecuta la task "4. Ver Versiones" o:
Write-Host "Proyecto:"; (Get-Content "src\SGRRHH.WPF\SGRRHH.WPF.csproj" -Raw) -match '<Version>([^<]+)</Version>'; $matches[1]
Write-Host "Local:"; (Get-Content "C:\SGRRHH\appsettings.json" | ConvertFrom-Json).Application.Version
Write-Host "Firebase:"; (Get-Content "src\publish\version.json" | ConvertFrom-Json).version
```

**Solución:** Publica con `Publish-All.ps1` para sincronizar todo.

### PCs remotas no detectan la actualización

**Verificar:**
1. ¿La versión en Firebase es MAYOR que la local del cliente?
2. ¿El cliente tiene conexión a internet?
3. ¿`Updates.Enabled = true` en su appsettings.json?
4. ¿`firebase-credentials.json` existe?

### Error al subir a Firebase

```powershell
# Verificar autenticación
gcloud auth list

# Re-autenticar si es necesario
gcloud auth activate-service-account --key-file="src\SGRRHH.WPF\firebase-credentials.json"
```

### La actualización falla al aplicarse

1. Cierra todas las instancias de SGRRHH
2. Elimina carpeta temporal:
   ```powershell
   Remove-Item "$env:TEMP\SGRRHH_update_temp" -Recurse -Force
   ```
3. Reinicia la aplicación

---

## 🛠️ Comandos Útiles

### Ver versiones actuales

```powershell
# Via task de VS Code
Ctrl+Shift+B → "4. Ver Versiones"

# O manualmente
(Get-Content "C:\SGRRHH\appsettings.json" | ConvertFrom-Json).Application.Version
```

### Publicación rápida

```powershell
cd c:\Users\evert\Documents\rrhh\scripts
.\Publish-All.ps1 -Version "1.0.4" -ReleaseNotes "Fix de bugs" -Incremental
```

### Ver qué hay en Firebase

```powershell
gcloud storage cat gs://rrhh-forestech.firebasestorage.app/updates/version.json
gcloud storage ls gs://rrhh-forestech.firebasestorage.app/updates/latest/
```

### Forzar actualización en cliente

Si un cliente tiene problemas, actualizar manualmente:

```powershell
# En la PC cliente (como admin)
Stop-Process -Name "SGRRHH" -Force -ErrorAction SilentlyContinue

# Copiar desde servidor o descargar
# ... luego iniciar
Start-Process "C:\SGRRHH\SGRRHH.exe"
```

---

## ✅ Checklist de Publicación

- [ ] Realizar cambios en el código
- [ ] Probar que funcione localmente
- [ ] Decidir número de versión (MAJOR.MINOR.PATCH)
- [ ] Escribir notas de versión claras
- [ ] Ejecutar: `.\Publish-All.ps1 -Version "X.Y.Z" -ReleaseNotes "..." -Incremental`
- [ ] Verificar que todas las versiones coincidan (task "4. Ver Versiones")
- [ ] Probar la app localmente
- [ ] (Opcional) Probar actualización en una PC cliente

---

## 📚 Archivos Relacionados

| Archivo | Descripción |
|---------|-------------|
| `scripts/Publish-All.ps1` | ⭐ Script principal de publicación |
| `scripts/Publish-Local.ps1` | Solo actualiza local |
| `scripts/Publish-Firebase-Update.ps1` | Solo sube a Firebase |
| `src/SGRRHH.Infrastructure/Firebase/FirebaseUpdateService.cs` | Lógica de actualizaciones |
| `src/SGRRHH.Core/Interfaces/IFirebaseUpdateService.cs` | Interfaz del servicio |
| `.vscode/tasks.json` | Tasks de VS Code |

---

*Última actualización: 28 de Noviembre 2025*
