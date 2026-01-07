# 📁 FASE 2: Sistema de Archivos Local

## 📋 Contexto

Fases anteriores completadas:
- ✅ Estructura base del proyecto
- ✅ Infraestructura SQLite con Dapper

**Objetivo:** Implementar el servicio de almacenamiento local que reemplaza a Firebase Storage.

---

## 🎯 Objetivo de esta Fase

Crear LocalStorageService que gestione fotos de empleados, documentos de permisos/contratos, y archivos generados (PDFs).

---

## 📝 PROMPT PARA CLAUDE

```
Necesito que implementes el sistema de almacenamiento local para SGRRHH.Local que reemplaza a Firebase Storage.

**PROYECTO:** SGRRHH.Local.Infrastructure/Services/

**REFERENCIA:** FirebaseStorageService de SGRRHH actual:
- C:\Users\evert\Documents\rrhh\src\SGRRHH.Infrastructure\Firebase\FirebaseStorageService.cs

---

## ESTRUCTURA DE CARPETAS DE DATOS:

```
C:\SGRRHH\Data\
├── sgrrhh.db                     # Base de datos SQLite
├── Fotos\
│   └── Empleados\
│       └── {empleadoId}\
│           └── foto.{ext}        # foto.jpg, foto.png, etc.
├── Documentos\
│   ├── Permisos\
│   │   └── {permisoId}\
│   │       └── soporte.{ext}     # Documento soporte del permiso
│   ├── Contratos\
│   │   └── {contratoId}\
│   │       └── contrato.{ext}    # Contrato firmado
│   └── Empleados\
│       └── {empleadoId}\
│           └── {tipoDoc}_{id}.{ext}  # Cédula, certificados, etc.
├── Generados\
│   ├── Actas\
│   │   └── PERM-2026-0001.pdf
│   ├── Certificados\
│   │   └── CERT-{empleadoId}-{fecha}.pdf
│   └── Reportes\
│       └── REPORTE-{fecha}.pdf
├── Config\
│   └── logo.png                  # Logo de la empresa
└── Backups\
    └── {yyyy-MM-dd}\
        ├── sgrrhh.db
        └── Fotos.zip
```

---

## ARCHIVOS A CREAR:

### 1. Interfaz ILocalStorageService.cs (en Shared/Interfaces/)

```csharp
public interface ILocalStorageService
{
    // ===== Fotos de Empleados =====
    Task<Result<string>> SaveEmpleadoFotoAsync(int empleadoId, byte[] imageBytes, string extension);
    Task<Result<string>> SaveEmpleadoFotoAsync(int empleadoId, Stream imageStream, string extension);
    Task<Result<byte[]>> GetEmpleadoFotoAsync(int empleadoId);
    Task<Result<string>> GetEmpleadoFotoPathAsync(int empleadoId);
    Task<Result> DeleteEmpleadoFotoAsync(int empleadoId);
    bool EmpleadoFotoExists(int empleadoId);
    
    // ===== Documentos de Permisos =====
    Task<Result<string>> SavePermisoDocumentoAsync(int permisoId, byte[] fileBytes, string fileName);
    Task<Result<byte[]>> GetPermisoDocumentoAsync(int permisoId);
    Task<Result> DeletePermisoDocumentoAsync(int permisoId);
    
    // ===== Documentos de Contratos =====
    Task<Result<string>> SaveContratoArchivoAsync(int contratoId, byte[] fileBytes, string fileName);
    Task<Result<byte[]>> GetContratoArchivoAsync(int contratoId);
    Task<Result> DeleteContratoArchivoAsync(int contratoId);
    
    // ===== Documentos de Empleados (expediente) =====
    Task<Result<string>> SaveDocumentoEmpleadoAsync(int empleadoId, string tipoDocumento, byte[] fileBytes, string fileName);
    Task<Result<byte[]>> GetDocumentoEmpleadoAsync(int documentoId);
    Task<Result<IEnumerable<string>>> GetDocumentosEmpleadoAsync(int empleadoId);
    Task<Result> DeleteDocumentoEmpleadoAsync(int documentoId);
    
    // ===== Documentos Generados (PDFs) =====
    Task<Result<string>> SaveGeneratedPdfAsync(string tipo, string fileName, byte[] pdfBytes);
    Task<Result<byte[]>> GetGeneratedPdfAsync(string tipo, string fileName);
    string GetGeneratedPdfPath(string tipo, string fileName);
    
    // ===== Logo de Empresa =====
    Task<Result<string>> SaveLogoEmpresaAsync(byte[] imageBytes, string extension);
    Task<Result<byte[]>> GetLogoEmpresaAsync();
    string? GetLogoEmpresaPath();
    
    // ===== Utilidades =====
    Task<long> GetTotalStorageSizeAsync();
    Task<Result> CleanupOrphanFilesAsync();
    void EnsureDirectoriesExist();
}
```

---

### 2. LocalStorageService.cs (en Infrastructure/Services/)

```csharp
public class LocalStorageService : ILocalStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalStorageService> _logger;
    
    // Rutas relativas
    private const string FOTOS_EMPLEADOS = "Fotos/Empleados";
    private const string DOCS_PERMISOS = "Documentos/Permisos";
    private const string DOCS_CONTRATOS = "Documentos/Contratos";
    private const string DOCS_EMPLEADOS = "Documentos/Empleados";
    private const string GENERADOS = "Generados";
    private const string CONFIG = "Config";
    
    public LocalStorageService(IConfiguration configuration, ILogger<LocalStorageService> logger)
    {
        _basePath = configuration.GetValue<string>("LocalDatabase:StoragePath") 
            ?? "C:\\SGRRHH\\Data";
        _logger = logger;
        EnsureDirectoriesExist();
    }
    
    public void EnsureDirectoriesExist()
    {
        var directories = new[]
        {
            Path.Combine(_basePath, FOTOS_EMPLEADOS),
            Path.Combine(_basePath, DOCS_PERMISOS),
            Path.Combine(_basePath, DOCS_CONTRATOS),
            Path.Combine(_basePath, DOCS_EMPLEADOS),
            Path.Combine(_basePath, GENERADOS, "Actas"),
            Path.Combine(_basePath, GENERADOS, "Certificados"),
            Path.Combine(_basePath, GENERADOS, "Reportes"),
            Path.Combine(_basePath, CONFIG),
            Path.Combine(_basePath, "Backups")
        };
        
        foreach (var dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                _logger.LogInformation("Directorio creado: {Directory}", dir);
            }
        }
    }
    
    // Implementar todos los métodos de la interfaz...
}
```

---

### 3. Métodos clave a implementar:

**SaveEmpleadoFotoAsync:**
```csharp
public async Task<Result<string>> SaveEmpleadoFotoAsync(int empleadoId, byte[] imageBytes, string extension)
{
    try
    {
        extension = extension.TrimStart('.');
        var directory = Path.Combine(_basePath, FOTOS_EMPLEADOS, empleadoId.ToString());
        
        // Limpiar fotos anteriores
        if (Directory.Exists(directory))
        {
            foreach (var file in Directory.GetFiles(directory, "foto.*"))
            {
                File.Delete(file);
            }
        }
        else
        {
            Directory.CreateDirectory(directory);
        }
        
        var filePath = Path.Combine(directory, $"foto.{extension}");
        await File.WriteAllBytesAsync(filePath, imageBytes);
        
        _logger.LogInformation("Foto guardada para empleado {Id}: {Path}", empleadoId, filePath);
        return Result<string>.Ok(filePath);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error guardando foto del empleado {Id}", empleadoId);
        return Result<string>.Fail($"Error: {ex.Message}");
    }
}
```

**GetEmpleadoFotoAsync:**
```csharp
public async Task<Result<byte[]>> GetEmpleadoFotoAsync(int empleadoId)
{
    try
    {
        var directory = Path.Combine(_basePath, FOTOS_EMPLEADOS, empleadoId.ToString());
        
        if (!Directory.Exists(directory))
            return Result<byte[]>.Fail("No hay foto para este empleado");
        
        var fotoPath = Directory.GetFiles(directory, "foto.*").FirstOrDefault();
        
        if (fotoPath == null)
            return Result<byte[]>.Fail("No hay foto para este empleado");
        
        var bytes = await File.ReadAllBytesAsync(fotoPath);
        return Result<byte[]>.Ok(bytes);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error leyendo foto del empleado {Id}", empleadoId);
        return Result<byte[]>.Fail($"Error: {ex.Message}");
    }
}
```

**EmpleadoFotoExists (Síncrono para UI rápida):**
```csharp
public bool EmpleadoFotoExists(int empleadoId)
{
    var directory = Path.Combine(_basePath, FOTOS_EMPLEADOS, empleadoId.ToString());
    if (!Directory.Exists(directory)) return false;
    return Directory.GetFiles(directory, "foto.*").Any();
}
```

---

### 4. ImageHelper.cs (en Infrastructure/Services/)

Utilidades para procesar imágenes:

```csharp
public static class ImageHelper
{
    /// <summary>
    /// Redimensiona una imagen manteniendo proporción
    /// </summary>
    public static byte[] ResizeImage(byte[] imageBytes, int maxWidth, int maxHeight)
    {
        // Usar System.Drawing o SkiaSharp
        // Para fotos de empleados: maxWidth=400, maxHeight=400
    }
    
    /// <summary>
    /// Convierte imagen a formato optimizado (JPEG)
    /// </summary>
    public static byte[] OptimizeForStorage(byte[] imageBytes, int quality = 85)
    {
        // Convertir a JPEG con calidad especificada
    }
    
    /// <summary>
    /// Valida que el archivo sea una imagen válida
    /// </summary>
    public static bool IsValidImage(byte[] fileBytes)
    {
        // Verificar magic bytes de JPG, PNG, GIF, BMP
    }
    
    /// <summary>
    /// Obtiene la extensión correcta basada en magic bytes
    /// </summary>
    public static string? GetImageExtension(byte[] fileBytes)
    {
        // Detectar tipo real de imagen
    }
}
```

---

### 5. FileHelper.cs (en Infrastructure/Services/)

```csharp
public static class FileHelper
{
    private static readonly Dictionary<string, string> MimeTypes = new()
    {
        { ".pdf", "application/pdf" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
    };
    
    public static string GetMimeType(string extension)
    {
        extension = extension.ToLowerInvariant();
        if (!extension.StartsWith(".")) extension = "." + extension;
        return MimeTypes.TryGetValue(extension, out var mime) ? mime : "application/octet-stream";
    }
    
    public static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
    
    public static string GetUniqueFileName(string directory, string fileName)
    {
        var fullPath = Path.Combine(directory, fileName);
        if (!File.Exists(fullPath)) return fullPath;
        
        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);
        var counter = 1;
        
        while (File.Exists(fullPath))
        {
            fullPath = Path.Combine(directory, $"{name}_{counter}{ext}");
            counter++;
        }
        
        return fullPath;
    }
}
```

---

## INTEGRACIÓN CON EMPLEADO:

El campo `FotoPath` en Empleado ahora será una ruta local:
- Antes: `https://firebasestorage.googleapis.com/v0/b/.../foto.jpg`
- Ahora: `Fotos/Empleados/123/foto.jpg` (relativo a _basePath)

En la UI, para mostrar la foto:
```csharp
// En el componente Blazor:
private string? GetFotoUrl(Empleado empleado)
{
    if (!_storageService.EmpleadoFotoExists(empleado.Id))
        return "/images/default-avatar.png";
    
    // Opción 1: Servir archivos estáticos desde Data folder
    // Opción 2: Usar endpoint API que lee y devuelve bytes
    return $"/api/fotos/empleados/{empleado.Id}";
}
```

---

**IMPORTANTE:**
- Todas las operaciones de archivo deben ser async
- Implementar validación de tipos de archivo permitidos
- Limitar tamaño máximo de archivos (ej: 5MB para fotos, 10MB para documentos)
- Loggear todas las operaciones
- No almacenar rutas absolutas en BD, solo relativas
```

---

## ✅ Checklist de Entregables

- [ ] Shared/Interfaces/ILocalStorageService.cs
- [ ] Infrastructure/Services/LocalStorageService.cs
- [ ] Infrastructure/Services/ImageHelper.cs
- [ ] Infrastructure/Services/FileHelper.cs
- [ ] Actualizar Program.cs para registrar servicio
- [ ] Crear endpoint API para servir fotos (opcional)
- [ ] Agregar imagen default-avatar.png en wwwroot/images/

---

## 🔗 Dependencias NuGet Opcionales

```xml
<!-- Para procesamiento de imágenes (opcional) -->
<PackageReference Include="SkiaSharp" Version="2.88.7" />
<!-- O usar System.Drawing.Common en Windows -->
<PackageReference Include="System.Drawing.Common" Version="8.0.0" />
```

---

## 📝 Notas de Implementación

1. **Rutas Relativas vs Absolutas:**
   - En la BD guardar rutas relativas: `Fotos/Empleados/123/foto.jpg`
   - El servicio resuelve a ruta absoluta: `C:\SGRRHH\Data\Fotos\Empleados\123\foto.jpg`

2. **Servir Archivos Estáticos:**
   En Program.cs, configurar:
   ```csharp
   app.UseStaticFiles(new StaticFileOptions
   {
       FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Configuration["LocalDatabase:StoragePath"], "Fotos")),
       RequestPath = "/fotos"
   });
   ```

3. **Limpieza de Archivos Huérfanos:**
   Implementar job que compare archivos en disco vs referencias en BD y elimine huérfanos.
