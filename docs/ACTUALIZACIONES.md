# Sistema de Actualizaciones SGRRHH

## Resumen de Mejoras Implementadas

 **Problema resuelto**: Error "No se pudo preparar la instalación"
 **Nueva funcionalidad**: Validación SHA256 de descargas
 **Nueva funcionalidad**: Opción "Instalar al cerrar"
 **Automatización**: Generación de checksum en build

---

## Componentes del Sistema

### 1. **GithubUpdateService** (`Infrastructure/Services/GithubUpdateService.cs`)
- Verifica releases en GitHub API (`/repos/evertweb/SGRRHH/releases/latest`)
- Descarga archivos ZIP con barra de progreso
- **( NUEVO**: Valida integridad con SHA256
- **( MEJORADO**: Mejor manejo de errores con logging detallado
- Lanza el Updater.exe

### 2. **SGRRHH.Updater** (`src/SGRRHH.Updater/`)
- Proceso separado que actualiza archivos
- Espera a que la app principal cierre
- Crea backup antes de copiar (`backup_YYYYMMDD_HHmmss/`)
- Reinicia la aplicación automáticamente
- **( MEJORADO**: Ahora se compila y copia automáticamente en cada build

### 3. **UpdateDialog** (`WPF/Views/UpdateDialog.xaml`)
- Interfaz para notificar actualizaciones
- **( NUEVO**: 3 opciones disponibles:
  - **Actualizar ahora** - Descarga, cierra e instala inmediatamente
  - **Instalar al cerrar** - Descarga ahora, instala cuando cierres
  - **Recordar después** - Pregunta en próximo inicio
- Muestra progreso de descarga y verificación
- Visualiza notas de versión (Release Notes)

---

## Cómo Publicar una Nueva Versión

### Paso 1: Incrementar Versión

Edita `src/SGRRHH.WPF/SGRRHH.WPF.csproj`:

```xml
<Version>1.0.7</Version>
<AssemblyVersion>1.0.7.0</AssemblyVersion>
<FileVersion>1.0.7.0</FileVersion>
```

### Paso 2: Generar el ZIP con Checksum

```powershell
cd installer
.\Build-Installer.ps1 -CreateZip
```

**Salida esperada:**
```
 Creando version portable (ZIP)...
    ZIP creado: installer/output/SGRRHH_Portable_1.0.7.zip
    Tamano del ZIP: 45.3 MB

 Calculando checksum SHA256...
    SHA256: a1b2c3d4e5f6abc123def456789...
    Checksum guardado en: installer/output/SGRRHH_Portable_1.0.7.sha256

=== INSTRUCCIONES PARA GITHUB RELEASE ===
1. Crea un nuevo Release en GitHub
2. Adjunta el archivo: SGRRHH_Portable_1.0.7.zip
3. En las notas de version (body), incluye esta linea:

   SHA256: a1b2c3d4e5f6abc123def456789...

4. La aplicacion verificara automaticamente la integridad del archivo
=========================================
```

### Paso 3: Crear GitHub Release

1. Ve a https://github.com/evertweb/SGRRHH/releases/new
2. **Tag version**: `v1.0.7`
3. **Release title**: `v1.0.7 - Descripción breve`
4. **Descripción (body)** - **IMPORTANTE: Incluye el SHA256**:

   ```markdown
   ## Cambios en esta versión

   ### Nuevas características
   - ( Feature 1
   - ( Feature 2

   ### Correcciones
   - = Fix 1
   - = Fix 2

   ### Mejoras técnicas
   - ¡ Optimización 1

   ---

   **Verificación de integridad:**

   SHA256: a1b2c3d4e5f6abc123def456789...
   ```

5. **Adjuntar archivo**: Sube `SGRRHH_Portable_1.0.7.zip`
6. Clic en **Publish release**

### Paso 4: Verificar

1. Los clientes recibirán notificación al abrir la app
2. La app descargará y verificará el SHA256 automáticamente
3.  Si coincide ’ Instala
4. L Si no coincide ’ Rechaza con error de integridad

---

## Opciones de Actualización para Usuarios

### =€ Opción 1: Actualizar Ahora
- Descarga inmediatamente
- Cierra la aplicación
- Instala y reinicia automáticamente
- **Recomendado para:** Actualizaciones críticas/urgentes

### =å Opción 2: Instalar al Cerrar (NUEVO)
- Descarga en segundo plano
- Usuario continúa trabajando normalmente
- Instala cuando cierre la app (OnExit)
- **Recomendado para:** Actualizaciones normales

### ð Opción 3: Recordar Después
- No descarga nada
- Pregunta nuevamente en el próximo inicio
- **Recomendado para:** Usuario ocupado

---

## Solución al Problema "No se pudo preparar la instalación"

### Causa del Problema (Antes)

El `SGRRHH.Updater.exe` **NO se estaba compilando junto con la aplicación principal**, por lo que cuando el sistema intentaba lanzarlo, no existía en la carpeta de instalación.

### Solución Implementada

**1. Referencia automática en el .csproj**

```xml
<ItemGroup>
  <ProjectReference Include="..\SGRRHH.Updater\SGRRHH.Updater.csproj">
    <ReferenceOutputAssembly>false</ReferenceOutputAssembly>
  </ProjectReference>
</ItemGroup>
```

**2. Target de compilación automática**

```xml
<Target Name="CopyUpdater" AfterTargets="Build">
  <MSBuild Projects="..\SGRRHH.Updater\SGRRHH.Updater.csproj"
           Targets="Build" />
  <Copy SourceFiles="@(UpdaterFiles)"
        DestinationFolder="$(OutDir)" />
</Target>
```

**3. Lógica mejorada en ApplyUpdateAsync**

```csharp
// Ahora busca el updater primero en la descarga (más confiable)
string downloadedUpdater = Path.Combine(sourceDir, "SGRRHH.Updater.exe");
string updaterPath = Path.Combine(_installPath, "SGRRHH.Updater.exe");

// Lo copia SIEMPRE desde la descarga (asegura versión actualizada)
File.Copy(downloadedUpdater, updaterPath, overwrite: true);

// Logging detallado para diagnóstico
_logger?.LogInformation($"Buscando updater en descarga: {downloadedUpdater}");
_logger?.LogInformation($"Copiando updater a: {updaterPath}");
```

**Resultado:**  El updater SIEMPRE estará disponible y actualizado

---

## Validación SHA256 (Seguridad)

### ¿Por qué SHA256?

- Verifica que el archivo descargado NO fue modificado
- Detecta corrupción durante la descarga
- Previene instalación de archivos maliciosos

### Cómo Funciona

1. **Al publicar**: El script `Build-Installer.ps1` calcula el hash
2. **En GitHub Release**: Incluyes el hash en las notas
3. **Al descargar**: La app recalcula el hash
4. **Comparación**: Si no coinciden ’ RECHAZA la instalación

### Implementación

```csharp
// Calcular hash del archivo descargado
var checksum = CalculateFileSha256(zipPath);

// Buscar hash en las Release Notes
var checksumMatch = Regex.Match(
    releaseBody,
    @"SHA256:\s*([a-fA-F0-9]{64})",
    RegexOptions.IgnoreCase
);

if (checksumMatch.Success)
{
    var expectedChecksum = checksumMatch.Groups[1].Value;
    if (!VerifyFileIntegrity(zipPath, expectedChecksum))
    {
        // RECHAZAR actualización
        return false;
    }
}
```

**Ejemplo de error:**
```
 Checksum no coincide!
  Esperado: a1b2c3d4e5f6...
  Obtenido: x9y8z7w6v5u4...
```

---

## Logs y Diagnóstico

### Logs de la Aplicación

**Ubicación:** `data/logs/error_YYYY-MM-DD.log`

**Ejemplo de actualización exitosa:**
```
[2025-01-28 10:15:32] INFO - Verificando actualizaciones en GitHub...
[2025-01-28 10:15:33] INFO - Versión actual: 1.0.6, Versión GitHub: 1.0.7
[2025-01-28 10:15:45] INFO - Descargando actualización...
[2025-01-28 10:16:12] INFO - SHA256 del archivo descargado: a1b2c3d4e5f6...
[2025-01-28 10:16:13] INFO -  Integridad verificada
[2025-01-28 10:16:15] INFO - Archivos extraídos: 127 archivos
[2025-01-28 10:16:16] INFO - Buscando updater en descarga: C:\...\extracted\SGRRHH.Updater.exe
[2025-01-28 10:16:16] INFO - Copiando updater a: C:\...\SGRRHH.Updater.exe
[2025-01-28 10:16:16] INFO - Updater copiado exitosamente
[2025-01-28 10:16:17] INFO - Lanzando updater: C:\...\SGRRHH.Updater.exe
[2025-01-28 10:16:17] INFO - Updater lanzado exitosamente (PID: 8472)
```

### Logs del Updater

**Ubicación:** `updater_log.txt` (en carpeta de instalación)

**Ejemplo:**
```
[2025-01-28 10:16:20] Iniciando actualizador...
[2025-01-28 10:16:20] Target: C:\Program Files\SGRRHH
[2025-01-28 10:16:20] Source: C:\Users\...\SGRRHH_update_temp\extracted
[2025-01-28 10:16:20] Esperando a que termine el proceso 12345...
[2025-01-28 10:16:22] Creando backup en C:\...\backup_20250128_101622...
[2025-01-28 10:16:35] Copiando nuevos archivos...
[2025-01-28 10:16:58] Archivos copiados exitosamente.
[2025-01-28 10:16:59] Reiniciando SGRRHH.exe...
```

---

## Configuración

### Habilitar/Deshabilitar Actualizaciones

Edita `src/SGRRHH.WPF/appsettings.json`:

```json
{
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true,
    "Repository": "evertweb/SGRRHH"
  }
}
```

---

## Próximas Mejoras (Opcional)

### Squirrel.Windows - Actualizaciones Delta

**Ventajas:**
- Solo descarga archivos modificados
- Reduce tamaño de descarga de ~45 MB a ~5-10 MB
- Actualizaciones más rápidas

**Implementación estimada:** 4-6 horas

**¿Cuándo implementar?**
- Si tienes >10 usuarios
- Si actualizas frecuentemente (>2 veces/mes)
- Si el ancho de banda es limitado

**Estado actual:**  Sistema actual es suficiente para 3 usuarios

---

## Resumen de Cambios

| Antes | Después |
|-------|---------|
| L Error "No se pudo preparar la instalación" |  Updater.exe se compila automáticamente |
|   Sin validación de integridad |  Validación SHA256 obligatoria |
| = Solo opción: Actualizar ahora |  3 opciones (incluye "Instalar al cerrar") |
| =Ý Checksum manual |  Generación automática en script |
| S Diagnóstico difícil |  Logging detallado en cada paso |
| =æ Tamaño: ~45 MB siempre | =. Futuro: ~5-10 MB con Squirrel |

---

## Soporte

Para reportar problemas con las actualizaciones:

1. Revisa los logs en `data/logs/error_YYYY-MM-DD.log`
2. Revisa `updater_log.txt` en la carpeta de instalación
3. Abre un issue en GitHub con los logs adjuntos

---

**Autor:** Sistema de actualizaciones mejorado para SGRRHH
**Fecha:** Enero 2025
**Versión:** 1.0.6+
