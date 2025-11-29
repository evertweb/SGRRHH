# Sistema de Actualizaciones SGRRHH

## Resumen del Sistema

El sistema de actualizaciones utiliza **GitHub Releases** para distribuir nuevas versiones automáticamente. Cuando el usuario abre la aplicación, se verifica si hay una versión más reciente disponible.

---

## Arquitectura

```
┌─────────────────────────────────────────────────────────────────────┐
│              FLUJO DE ACTUALIZACIÓN                                  │
└─────────────────────────────────────────────────────────────────────┘

   Desarrollador                    GitHub                      Usuario
   ────────────                    ──────                      ───────
        │                              │                           │
        │  1. Push + Tag (v1.1.x)      │                           │
        ├─────────────────────────────►│                           │
        │                              │                           │
        │  2. GitHub Actions compila   │                           │
        │     y publica release        │                           │
        │                              │                           │
        │                              │  3. Al abrir SGRRHH.exe   │
        │                              │◄──────────────────────────┤
        │                              │     GET /releases/latest  │
        │                              │                           │
        │                              │  4. Responde versión      │
        │                              ├──────────────────────────►│
        │                              │                           │
        │                              │  5. Descarga ZIP si hay   │
        │                              │     nueva versión         │
        │                              │◄──────────────────────────┤
        │                              │                           │
        │                              │  6. Updater.exe aplica    │
        │                              │     la actualización      │
        │                              ├──────────────────────────►│
```

---

## Componentes del Sistema

### 1. **GithubUpdateService** (`Infrastructure/Services/GithubUpdateService.cs`)

Servicio principal que:
- Consulta la API de GitHub (`/repos/evertweb/SGRRHH/releases/latest`)
- Compara versiones (local vs GitHub)
- Descarga el ZIP con barra de progreso
- Extrae archivos a carpeta temporal
- Lanza el `SGRRHH.Updater.exe`

### 2. **SGRRHH.Updater** (`src/SGRRHH.Updater/Program.cs`)

Proceso externo que aplica la actualización:
- **Mata todos los procesos SGRRHH** agresivamente
- **Excluye sus propios archivos** (SGRRHH.Updater.*) para evitar "archivo en uso"
- Copia archivos desde la carpeta temporal a la carpeta de instalación
- **Retry con delay incremental** si encuentra archivos bloqueados
- Reinicia la aplicación automáticamente
- **Logging detallado** en `updater_log.txt`

### 3. **UpdateDialog** (`WPF/Views/UpdateDialog.xaml`)

Interfaz de usuario que muestra:
- Versión actual vs nueva versión
- Notas de la versión (Release Notes)
- Barra de progreso de descarga
- Dos botones: **"Actualizar ahora"** y **"Recordar después"**

### 4. **GitHub Actions** (`.github/workflows/release.yml`)

Workflow automático que:
- Se activa al crear un tag (`v*`)
- Compila con `dotnet publish --self-contained false`
- Crea ZIP (~12 MB)
- Publica GitHub Release con el ZIP adjunto

---

## Cómo Publicar una Nueva Versión

### Método 1: Automático con GitHub Actions (Recomendado)

```bash
# 1. Actualizar versión en csproj
# Editar src/SGRRHH.WPF/SGRRHH.WPF.csproj
<Version>1.1.5</Version>
<AssemblyVersion>1.1.5.0</AssemblyVersion>
<FileVersion>1.1.5.0</FileVersion>

# 2. Commit y push
git add .
git commit -m "Release v1.1.5: descripción de cambios"
git push

# 3. Crear y push tag
git tag v1.1.5
git push origin v1.1.5

# GitHub Actions hace el resto automáticamente
```

### Método 2: Manual (para distribución inicial o emergencias)

```powershell
# En VS Code, usar las tareas predefinidas:
# Ctrl+Shift+P → "Tasks: Run Task"

# Opción A: Solo compilar y actualizar local
Task: "1. Build + Actualizar Local"

# Opción B: Publicar a GitHub y actualizar local
Task: "2b. Publicar TODO (Firebase + Local)"
```

---

## Flujo de Actualización para el Usuario

1. **Usuario abre SGRRHH.exe**
2. La app consulta GitHub API en segundo plano
3. Si hay nueva versión, aparece el diálogo:

   ```
   ┌─────────────────────────────────────┐
   │ 🚀 Nueva Versión Disponible         │
   │                                      │
   │ Versión actual: 1.1.2               │
   │ Nueva versión: 1.1.4                │
   │                                      │
   │ ## Cambios:                         │
   │ - Nueva funcionalidad X             │
   │ - Corrección de bug Y               │
   │                                      │
   │ [Actualizar ahora] [Recordar después]│
   └─────────────────────────────────────┘
   ```

4. Si el usuario hace clic en **"Actualizar ahora"**:
   - Se descarga el ZIP (~12 MB)
   - Se extrae en carpeta temporal
   - Se lanza SGRRHH.Updater.exe
   - La app se cierra
   - Updater copia los archivos
   - La app se reinicia con la nueva versión

---

## Logs y Diagnóstico

### Log de la Aplicación

**Ubicación:** `C:\SGRRHH\data\logs\error_YYYY-MM-DD.log`

```
[2025-01-28 10:15:32] INFO - Verificando actualizaciones en GitHub...
[2025-01-28 10:15:33] INFO - Versión actual: 1.1.2, Versión GitHub: 1.1.4
[2025-01-28 10:15:45] INFO - Descargando actualización...
[2025-01-28 10:16:12] INFO - Archivos extraídos correctamente
[2025-01-28 10:16:15] INFO - Lanzando SGRRHH.Updater.exe...
```

### Log del Updater

**Ubicación:** `C:\SGRRHH\updater_log.txt`

```
[2025-01-28 10:16:20] Iniciando actualización...
[2025-01-28 10:16:20] Target: C:\SGRRHH
[2025-01-28 10:16:20] Source: C:\Users\...\Temp\SGRRHH_update_temp\extracted
[2025-01-28 10:16:21] Matando procesos SGRRHH...
[2025-01-28 10:16:22] Copiando archivos (excluyendo SGRRHH.Updater.*)...
[2025-01-28 10:16:35] 127 archivos copiados exitosamente
[2025-01-28 10:16:36] Reiniciando aplicación...
```

---

## Configuración

### appsettings.json

```json
{
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true,
    "Repository": "evertweb/SGRRHH"
  }
}
```

| Propiedad | Descripción |
|-----------|-------------|
| `Enabled` | `true` para habilitar actualizaciones automáticas |
| `CheckOnStartup` | `true` para verificar al iniciar la app |
| `Repository` | Repositorio GitHub en formato `owner/repo` |

---

## Características Técnicas

### Non-Self-Contained

El sistema usa compilación **non-self-contained** para reducir el tamaño del ZIP:

| Tipo | Tamaño | Requisito |
|------|--------|-----------|
| Non-self-contained | ~12 MB | .NET 8 Runtime debe estar instalado |
| Self-contained (antiguo) | ~82 MB | Sin requisitos adicionales |

### Exclusión de Archivos del Updater

El `SGRRHH.Updater.exe` **excluye sus propios archivos** al copiar para evitar el error "archivo en uso":

```csharp
// SGRRHH.Updater/Program.cs
var excludePatterns = new[] { "SGRRHH.Updater.exe", "SGRRHH.Updater.dll", 
                               "SGRRHH.Updater.deps.json", "SGRRHH.Updater.runtimeconfig.json" };

foreach (var file in sourceFiles)
{
    if (excludePatterns.Any(p => file.Name.Equals(p, StringComparison.OrdinalIgnoreCase)))
        continue; // No copiar archivos del propio updater
    
    // Copiar el resto...
}
```

---

## Distribución Manual

Para distribuir la aplicación en nuevos equipos (sin actualización previa):

1. Descargar el ZIP de GitHub Releases
2. Descomprimir en `C:\SGRRHH`
3. Instalar .NET 8 Runtime si no está instalado
4. Configurar `appsettings.json` con credenciales Firebase
5. Crear acceso directo en el escritorio
6. Las actualizaciones futuras serán automáticas

---

## Solución de Problemas

### "La actualización no se aplica"

1. Verificar que no haya procesos SGRRHH ejecutándose (Task Manager)
2. Revisar `updater_log.txt` para ver el error
3. Ejecutar manualmente como administrador

### "Error al descargar"

1. Verificar conexión a internet
2. Verificar que el repositorio GitHub sea accesible
3. Revisar los logs en `data/logs/`

### "El updater no puede copiar archivos"

El updater tiene retry automático con delay incremental. Si persiste:
1. Cerrar cualquier explorador de archivos apuntando a C:\SGRRHH
2. Reiniciar el PC y volver a intentar

---

*Última actualización: Enero 2025*
*Versión del sistema: 1.1.x*
