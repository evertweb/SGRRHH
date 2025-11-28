# 🔄 Sistema de Actualizaciones Firebase - SGRRHH

## 📋 Resumen

A partir de la **Fase 6** de la migración a Firebase, el sistema SGRRHH soporta actualizaciones desde **Firebase Storage** en la nube. Esto significa que **ya no necesitas tener tu PC encendido** para que otros usuarios puedan actualizar.

### Comparación de Modos

| Característica | Carpeta Compartida (Antiguo) | Firebase Storage (Nuevo) |
|----------------|------------------------------|--------------------------|
| Disponibilidad | Solo cuando el servidor está encendido | **24/7** |
| Velocidad | Red local (muy rápido) | Internet (variable) |
| Requisitos | Red local funcionando | Conexión a internet |
| Costo | Gratis | Gratis (tier gratuito Firebase) |
| Mantenimiento | Bajo | Muy bajo |

---

## ☁️ ¿Cómo Funciona Firebase Storage?

### Estructura en la Nube

```
gs://rrhh-forestech.firebasestorage.app/
└── updates/
    ├── version.json              # Información de la última versión
    └── latest/                   # Archivos de la aplicación
        ├── SGRRHH.exe
        ├── SGRRHH.dll
        ├── SGRRHH.deps.json
        ├── SGRRHH.runtimeconfig.json
        ├── runtimes/
        │   └── win-x64/
        │       └── native/
        └── ... (otros archivos)
```

### Flujo de Actualización

```
┌─────────────────────────────────────────────────────────────────────┐
│                   FLUJO DE ACTUALIZACIÓN FIREBASE                    │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│   1. TÚ (Desarrollador) publicas nueva versión                      │
│      └─► Ejecutas: .\Publish-Firebase-Update.ps1 -Version "1.1.0"   │
│                                                                      │
│   2. Los archivos se suben a Firebase Storage:                      │
│      └─► gs://rrhh-forestech.firebasestorage.app/updates/           │
│          ├── version.json                                           │
│          └── latest/                                                │
│                                                                      │
│   3. Cualquier PC, al iniciar SGRRHH (con modo Firebase):           │
│      └─► Descarga version.json de Firebase Storage                  │
│      └─► Compara con su versión local                               │
│      └─► Si hay nueva versión → muestra diálogo                     │
│                                                                      │
│   4. Usuario acepta actualizar:                                     │
│      └─► Descarga archivos de Firebase a carpeta temporal           │
│      └─► Verifica checksums SHA256                                  │
│      └─► Cierra la app                                              │
│      └─► Ejecuta script que reemplaza archivos                      │
│      └─► Reinicia la app con la nueva versión                       │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🚀 Publicar Nueva Versión (Desarrollador)

### Requisitos Previos

1. **Archivo de credenciales de Firebase** (`firebase-credentials.json`)
   - Ubicación: `src/SGRRHH.WPF/firebase-credentials.json`
   - Obtener desde: Firebase Console > Project Settings > Service accounts > Generate new private key

2. **Google Cloud SDK** (opcional, recomendado para subidas más rápidas)
   - Descarga: https://cloud.google.com/sdk/docs/install
   - Proporciona el comando `gsutil`

### Comando de Publicación

```powershell
# Ir a la carpeta de scripts
cd C:\Users\evert\Documents\rrhh\scripts

# Publicar versión normal
.\Publish-Firebase-Update.ps1 -Version "1.1.0" -ReleaseNotes "Corrección de errores"

# Publicar versión obligatoria (fuerza actualización)
.\Publish-Firebase-Update.ps1 -Version "1.2.0" -Mandatory $true -ReleaseNotes "Actualización de seguridad crítica"

# Publicar sin recompilar (usa archivos existentes)
.\Publish-Firebase-Update.ps1 -Version "1.1.0" -SkipBuild

# Especificar credenciales personalizadas
.\Publish-Firebase-Update.ps1 -Version "1.1.0" `
    -CredentialsPath "C:\ruta\firebase-credentials.json" `
    -BucketName "mi-bucket.firebasestorage.app"
```

### Parámetros del Script

| Parámetro | Tipo | Obligatorio | Descripción |
|-----------|------|-------------|-------------|
| `-Version` | string | ✅ Sí | Número de versión (ej: "1.1.0") |
| `-ReleaseNotes` | string | ❌ No | Notas de la versión (changelog) |
| `-Mandatory` | bool | ❌ No | Si `$true`, actualización obligatoria |
| `-SkipBuild` | switch | ❌ No | Omite compilación, usa archivos existentes |
| `-CredentialsPath` | string | ❌ No | Ruta al archivo de credenciales |
| `-BucketName` | string | ❌ No | Nombre del bucket (por defecto: `rrhh-forestech.firebasestorage.app`) |

### Proceso Interno del Script

```
1. Actualiza versión en SGRRHH.WPF.csproj
         ↓
2. Compila la aplicación (dotnet publish -c Release)
         ↓
3. Calcula checksums SHA256 de cada archivo
         ↓
4. Genera version.json con toda la metadata
         ↓
5. Limpia carpeta updates/latest/ en Firebase Storage
         ↓
6. Sube todos los archivos a Firebase Storage
         ↓
7. Sube version.json
         ↓
✅ Publicación completa - Las PCs detectarán la actualización automáticamente
```

---

## 📄 Estructura de version.json

```json
{
  "version": "1.1.0",
  "releaseDate": "2025-11-27T15:30:00Z",
  "mandatory": false,
  "minimumVersion": "1.0.0",
  "releaseNotes": "## Cambios en v1.1.0\n\n- Nueva funcionalidad X\n- Corrección de error Y",
  "checksum": "sha256:abc123def456...",
  "downloadSize": 45678900,
  "files": [
    {
      "name": "SGRRHH.exe",
      "checksum": "sha256:abc123...",
      "size": 12345678
    },
    {
      "name": "SGRRHH.dll",
      "checksum": "sha256:def456...",
      "size": 5432100
    }
  ]
}
```

### Campos Importantes

| Campo | Descripción |
|-------|-------------|
| `version` | Número de versión (semántico: MAJOR.MINOR.PATCH) |
| `releaseDate` | Fecha de publicación (ISO 8601) |
| `mandatory` | Si `true`, el usuario no puede cancelar la actualización |
| `minimumVersion` | Versión mínima requerida (para actualizaciones obligatorias) |
| `releaseNotes` | Changelog en formato Markdown |
| `checksum` | Hash SHA256 del paquete completo |
| `downloadSize` | Tamaño total en bytes |
| `files` | Lista de archivos con checksums individuales |

---

## 🖥️ Experiencia del Usuario

Cuando un usuario inicia SGRRHH y hay actualización disponible:

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│  🚀 Nueva Versión Disponible                                   │
│                                                                 │
│  Versión actual: 1.0.0  →  Nueva versión: 1.1.0                │
│  📅 Publicada: 27/11/2025 15:30                                │
│  💾 Tamaño: 45.6 MB                                            │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │ 📋 Notas de la versión                                   │ │
│  │                                                           │ │
│  │ ## Cambios en v1.1.0                                     │ │
│  │                                                           │ │
│  │ - Nueva funcionalidad X                                  │ │
│  │ - Corrección de error Y                                  │ │
│  │ - Mejora de rendimiento Z                                │ │
│  │                                                           │ │
│  └───────────────────────────────────────────────────────────┘ │
│                                                                 │
│  [Recordar después]                        [🔄 Actualizar ahora]│
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Progreso de Descarga

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│  ⬇️ Descargando actualización...                               │
│                                                                 │
│  ████████████████████████░░░░░░░░░░░░░░  65%                   │
│                                                                 │
│  📄 Descargando: runtimes/win-x64/native/e_sqlite3.dll         │
│  📊 29.7 MB / 45.6 MB                                          │
│                                                                 │
│                                              [Cancelar]         │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## ⚙️ Configuración

### appsettings.json

Para que una PC use actualizaciones desde Firebase, debe tener:

```json
{
  "DataMode": "Firebase",
  
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true
  },
  
  "Firebase": {
    "Enabled": true,
    "ProjectId": "rrhh-forestech",
    "StorageBucket": "rrhh-forestech.firebasestorage.app",
    "ApiKey": "AIzaSyBxxxxxxxxxxxxxxxxxxxxxxxx",
    "DatabaseId": "rrhh-forestech",
    "CredentialsPath": "firebase-credentials.json"
  }
}
```

### Modos de Actualización

| Configuración | Modo de Actualización |
|---------------|----------------------|
| `DataMode: "Firebase"` | Usa **Firebase Storage** |
| `DataMode: "SQLite"` | Usa **carpeta compartida** |

---

## 🔒 Seguridad

### Verificación de Integridad

- Cada archivo tiene un checksum **SHA256**
- Se verifica después de descargar
- Si falla la verificación → se cancela la actualización

### Backup Automático

- Antes de aplicar la actualización, se crea backup
- Ubicación: `{InstallPath}\backup_YYYYMMDD_HHmmss\`
- Backups mayores a **7 días** se eliminan automáticamente

### Protección de Configuración

- `appsettings.json` **NUNCA se sobrescribe**
- La configuración local del usuario se preserva siempre

### Reglas de Firebase Storage

```javascript
rules_version = '2';
service firebase.storage {
  match /b/{bucket}/o {
    // Solo usuarios autenticados pueden leer actualizaciones
    match /updates/{allPaths=**} {
      allow read: if request.auth != null;
      allow write: if request.auth != null && 
        request.auth.token.rol == 'Administrador';
    }
  }
}
```

---

## 🐛 Solución de Problemas

### La actualización no se detecta

1. **Verificar modo Firebase está activo**: `DataMode: "Firebase"` en appsettings.json
2. **Verificar conexión a internet**
3. **Verificar credenciales**: El archivo `firebase-credentials.json` debe existir
4. **Revisar logs**: `data/logs/error_YYYY-MM-DD.log`

### Error al descargar

1. Verificar que el bucket de Storage es correcto
2. Comprobar que el usuario está autenticado en la app
3. Verificar reglas de seguridad de Storage

### La actualización falla al aplicar

1. Cerrar todas las instancias de SGRRHH
2. Eliminar carpeta `update_temp` manualmente:
   ```powershell
   Remove-Item "C:\SGRRHH\update_temp" -Recurse -Force
   ```
3. Reiniciar la aplicación

### Restaurar versión anterior

Si una actualización causa problemas:

```powershell
# 1. Buscar backup más reciente
Get-ChildItem "C:\SGRRHH" -Directory | Where-Object { $_.Name -like "backup_*" }

# 2. Copiar archivos del backup
$backupDir = "C:\SGRRHH\backup_20251127_153000"  # Ajustar fecha
Copy-Item "$backupDir\*" "C:\SGRRHH\" -Force

# 3. Reiniciar la app
Start-Process "C:\SGRRHH\SGRRHH.exe"
```

---

## 🔄 Compatibilidad con Modo Antiguo

Si necesitas volver al modo de actualización por carpeta compartida:

1. Cambiar `DataMode` a `"SQLite"` en appsettings.json
2. Usar script `Publish-Update.ps1` en lugar de `Publish-Firebase-Update.ps1`
3. El sistema detectará automáticamente el modo y usará la carpeta compartida

---

## 📝 Checklist de Publicación

- [ ] Realizar cambios en el código
- [ ] Probar localmente que funcione
- [ ] Escribir notas de versión claras
- [ ] Verificar que tienes `firebase-credentials.json`
- [ ] Ejecutar `.\Publish-Firebase-Update.ps1 -Version "X.Y.Z" -ReleaseNotes "..."`
- [ ] Verificar en la consola que se subieron los archivos
- [ ] Probar actualización en una PC cliente
- [ ] Comunicar a usuarios (opcional)

---

## 📚 Archivos Relacionados

| Archivo | Descripción |
|---------|-------------|
| `src/SGRRHH.Core/Interfaces/IUpdateService.cs` | Interfaz base |
| `src/SGRRHH.Core/Interfaces/IFirebaseUpdateService.cs` | Interfaz extendida Firebase |
| `src/SGRRHH.Infrastructure/Services/UpdateService.cs` | Implementación carpeta local |
| `src/SGRRHH.Infrastructure/Firebase/FirebaseUpdateService.cs` | Implementación Firebase |
| `scripts/Publish-Update.ps1` | Script para carpeta compartida |
| `scripts/Publish-Firebase-Update.ps1` | Script para Firebase Storage |

---

## 🎉 Ventajas del Nuevo Sistema

1. ✅ **Disponibilidad 24/7**: No necesitas tener tu PC encendido
2. ✅ **Actualización remota**: Puedes publicar actualizaciones desde cualquier lugar
3. ✅ **Verificación de integridad**: Checksums SHA256 para cada archivo
4. ✅ **Backups automáticos**: Siempre puedes volver atrás
5. ✅ **Sin configuración de red**: No hay que compartir carpetas ni configurar permisos

---

*Última actualización: 27 de Noviembre 2025*
