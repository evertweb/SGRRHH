using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class LocalStorageService : ILocalStorageService
{
    private const string FOTOS_EMPLEADOS = "Fotos/Empleados";
    private const string DOCS_PERMISOS = "Documentos/Permisos";
    private const string DOCS_CONTRATOS = "Documentos/Contratos";
    private const string DOCS_EMPLEADOS = "Documentos/Empleados";
    private const string GENERADOS = "Generados";
    private const string CONFIG = "Config";

    private const long MaxImageBytes = 5 * 1024 * 1024; // 5MB
    private const long MaxDocumentBytes = 10 * 1024 * 1024; // 10MB

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp"
    };

    private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif", ".bmp"
    };

    private static readonly HashSet<string> AllowedGeneratedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Actas", "Certificados", "Reportes"
    };

    private readonly string _basePath;
    private readonly ILogger<LocalStorageService> _logger;
    private readonly DapperContext _dapperContext;

    public LocalStorageService(DatabasePathResolver pathResolver, ILogger<LocalStorageService> logger, DapperContext dapperContext)
    {
        _basePath = pathResolver.GetStoragePath();
        _logger = logger;
        _dapperContext = dapperContext;
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

    public async Task<Result<string>> SaveEmpleadoFotoAsync(int empleadoId, byte[] imageBytes, string extension)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return Result<string>.Fail("La imagen está vacía");

        if (imageBytes.Length > MaxImageBytes)
            return Result<string>.Fail("La imagen supera el tamaño máximo (5MB)");

        if (!ImageHelper.IsValidImage(imageBytes))
            return Result<string>.Fail("El archivo no es una imagen válida");

        var detectedExt = ImageHelper.GetImageExtension(imageBytes);
        var ext = detectedExt ?? FileHelper.NormalizeExtension(extension);

        if (!AllowedImageExtensions.Contains(ext))
            return Result<string>.Fail("Tipo de imagen no permitido");

        var resized = ImageHelper.ResizeImage(imageBytes, 400, 400);
        var optimized = ImageHelper.OptimizeForStorage(resized, 85);
        ext = ".jpg"; // Optimización fuerza JPEG

        var directory = Path.Combine(_basePath, FOTOS_EMPLEADOS, empleadoId.ToString());
        Directory.CreateDirectory(directory);

        foreach (var file in Directory.GetFiles(directory, "foto.*"))
        {
            File.Delete(file);
        }

        var fileName = $"foto{ext}";
        var absolutePath = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(absolutePath, optimized);

        var relativePath = NormalizeRelative(Path.Combine(FOTOS_EMPLEADOS, empleadoId.ToString(), fileName));
        _logger.LogInformation("Foto guardada para empleado {Id}: {Path}", empleadoId, absolutePath);
        return Result<string>.Ok(relativePath);
    }

    public async Task<Result<string>> SaveEmpleadoFotoAsync(int empleadoId, Stream imageStream, string extension)
    {
        await using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms);
        return await SaveEmpleadoFotoAsync(empleadoId, ms.ToArray(), extension);
    }

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

    public async Task<Result<string>> GetEmpleadoFotoPathAsync(int empleadoId)
    {
        var directory = Path.Combine(_basePath, FOTOS_EMPLEADOS, empleadoId.ToString());
        if (!Directory.Exists(directory))
            return Result<string>.Fail("No hay foto para este empleado");

        var fotoPath = Directory.GetFiles(directory, "foto.*").FirstOrDefault();
        if (fotoPath == null)
            return Result<string>.Fail("No hay foto para este empleado");

        var relative = NormalizeRelative(Path.GetRelativePath(_basePath, fotoPath));
        return Result<string>.Ok(relative);
    }

    public async Task<Result> DeleteEmpleadoFotoAsync(int empleadoId)
    {
        try
        {
            var directory = Path.Combine(_basePath, FOTOS_EMPLEADOS, empleadoId.ToString());
            if (!Directory.Exists(directory))
                return Result.Ok();

            foreach (var file in Directory.GetFiles(directory, "foto.*"))
            {
                File.Delete(file);
            }

            _logger.LogInformation("Foto eliminada para empleado {Id}", empleadoId);
            return await Task.FromResult(Result.Ok());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando foto del empleado {Id}", empleadoId);
            return Result.Fail($"Error: {ex.Message}");
        }
    }

    public bool EmpleadoFotoExists(int empleadoId)
    {
        var directory = Path.Combine(_basePath, FOTOS_EMPLEADOS, empleadoId.ToString());
        return Directory.Exists(directory) && Directory.GetFiles(directory, "foto.*").Any();
    }

    public async Task<Result<string>> SavePermisoDocumentoAsync(int permisoId, byte[] fileBytes, string fileName)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return Result<string>.Fail("El archivo está vacío");

        if (fileBytes.Length > MaxDocumentBytes)
            return Result<string>.Fail("El archivo supera el tamaño máximo (10MB)");

        var rawExt = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(rawExt))
            return Result<string>.Fail("El archivo no tiene extensión");

        var ext = FileHelper.NormalizeExtension(rawExt);
        if (!AllowedDocumentExtensions.Contains(ext))
            return Result<string>.Fail("Tipo de archivo no permitido");

        var directory = Path.Combine(_basePath, DOCS_PERMISOS, permisoId.ToString());
        Directory.CreateDirectory(directory);

        foreach (var file in Directory.GetFiles(directory, "soporte.*"))
        {
            File.Delete(file);
        }

        var absolutePath = Path.Combine(directory, $"soporte{ext}");
        await File.WriteAllBytesAsync(absolutePath, fileBytes);

        var relativePath = NormalizeRelative(Path.Combine(DOCS_PERMISOS, permisoId.ToString(), $"soporte{ext}"));
        _logger.LogInformation("Soporte de permiso guardado: {Path}", absolutePath);
        return Result<string>.Ok(relativePath);
    }

    public async Task<Result<byte[]>> GetPermisoDocumentoAsync(int permisoId)
    {
        var directory = Path.Combine(_basePath, DOCS_PERMISOS, permisoId.ToString());
        if (!Directory.Exists(directory))
            return Result<byte[]>.Fail("No hay documento para este permiso");

        var filePath = Directory.GetFiles(directory, "soporte.*").FirstOrDefault();
        if (filePath == null)
            return Result<byte[]>.Fail("No hay documento para este permiso");

        var bytes = await File.ReadAllBytesAsync(filePath);
        return Result<byte[]>.Ok(bytes);
    }

    public async Task<Result> DeletePermisoDocumentoAsync(int permisoId)
    {
        try
        {
            var directory = Path.Combine(_basePath, DOCS_PERMISOS, permisoId.ToString());
            if (!Directory.Exists(directory))
                return Result.Ok();

            foreach (var file in Directory.GetFiles(directory))
            {
                File.Delete(file);
            }

            _logger.LogInformation("Documento de permiso eliminado {Id}", permisoId);
            return await Task.FromResult(Result.Ok());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando documento de permiso {Id}", permisoId);
            return Result.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveContratoArchivoAsync(int contratoId, byte[] fileBytes, string fileName)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return Result<string>.Fail("El archivo está vacío");

        if (fileBytes.Length > MaxDocumentBytes)
            return Result<string>.Fail("El archivo supera el tamaño máximo (10MB)");

        var rawExt = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(rawExt))
            return Result<string>.Fail("El archivo no tiene extensión");

        var ext = FileHelper.NormalizeExtension(rawExt);
        if (!AllowedDocumentExtensions.Contains(ext))
            return Result<string>.Fail("Tipo de archivo no permitido");

        var directory = Path.Combine(_basePath, DOCS_CONTRATOS, contratoId.ToString());
        Directory.CreateDirectory(directory);

        foreach (var file in Directory.GetFiles(directory, "contrato.*"))
        {
            File.Delete(file);
        }

        var absolutePath = Path.Combine(directory, $"contrato{ext}");
        await File.WriteAllBytesAsync(absolutePath, fileBytes);

        var relativePath = NormalizeRelative(Path.Combine(DOCS_CONTRATOS, contratoId.ToString(), $"contrato{ext}"));
        _logger.LogInformation("Contrato guardado: {Path}", absolutePath);
        return Result<string>.Ok(relativePath);
    }

    public async Task<Result<byte[]>> GetContratoArchivoAsync(int contratoId)
    {
        var directory = Path.Combine(_basePath, DOCS_CONTRATOS, contratoId.ToString());
        if (!Directory.Exists(directory))
            return Result<byte[]>.Fail("No hay archivo para este contrato");

        var filePath = Directory.GetFiles(directory, "contrato.*").FirstOrDefault();
        if (filePath == null)
            return Result<byte[]>.Fail("No hay archivo para este contrato");

        var bytes = await File.ReadAllBytesAsync(filePath);
        return Result<byte[]>.Ok(bytes);
    }

    public async Task<Result> DeleteContratoArchivoAsync(int contratoId)
    {
        try
        {
            var directory = Path.Combine(_basePath, DOCS_CONTRATOS, contratoId.ToString());
            if (!Directory.Exists(directory))
                return Result.Ok();

            foreach (var file in Directory.GetFiles(directory))
            {
                File.Delete(file);
            }

            _logger.LogInformation("Archivo de contrato eliminado {Id}", contratoId);
            return await Task.FromResult(Result.Ok());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando archivo de contrato {Id}", contratoId);
            return Result.Fail($"Error: {ex.Message}");
        }
    }

    // ===== Archivos genéricos =====
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, params string[] pathParts)
    {
        var directory = Path.Combine(_basePath, Path.Combine(pathParts));
        Directory.CreateDirectory(directory);
        
        var sanitizedName = FileHelper.SanitizeFileName(fileName);
        var absolutePath = FileHelper.GetUniqueFileName(directory, sanitizedName);
        
        await using var fs = new FileStream(absolutePath, FileMode.Create);
        await fileStream.CopyToAsync(fs);
        
        var relativePath = NormalizeRelative(Path.GetRelativePath(_basePath, absolutePath));
        _logger.LogInformation("Archivo genérico guardado: {Path}", absolutePath);
        return relativePath;
    }
    
    public async Task<byte[]?> GetFileAsync(string relativePath)
    {
        try
        {
            var absolutePath = Path.Combine(_basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(absolutePath))
                return null;
            
            return await File.ReadAllBytesAsync(absolutePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leyendo archivo {Path}", relativePath);
            return null;
        }
    }

    public async Task<Result<string>> SaveDocumentoEmpleadoAsync(int empleadoId, string tipoDocumento, byte[] fileBytes, string fileName)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return Result<string>.Fail("El archivo está vacío");

        if (fileBytes.Length > MaxDocumentBytes)
            return Result<string>.Fail("El archivo supera el tamaño máximo (10MB)");

        var rawExt = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(rawExt))
            return Result<string>.Fail("El archivo no tiene extensión");

        var ext = FileHelper.NormalizeExtension(rawExt);
        if (!AllowedDocumentExtensions.Contains(ext))
            return Result<string>.Fail("Tipo de archivo no permitido");

        var directory = Path.Combine(_basePath, DOCS_EMPLEADOS, empleadoId.ToString());
        Directory.CreateDirectory(directory);

        var sanitizedName = FileHelper.SanitizeFileName(Path.GetFileNameWithoutExtension(fileName));
        var baseName = string.IsNullOrWhiteSpace(tipoDocumento)
            ? sanitizedName
            : FileHelper.SanitizeFileName(tipoDocumento);

        var finalName = string.IsNullOrWhiteSpace(baseName)
            ? $"documento_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}"
            : $"{baseName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";

        var absolutePath = FileHelper.GetUniqueFileName(directory, finalName);
        await File.WriteAllBytesAsync(absolutePath, fileBytes);

        var relativePath = NormalizeRelative(Path.GetRelativePath(_basePath, absolutePath));
        _logger.LogInformation("Documento de empleado guardado: {Path}", absolutePath);
        return Result<string>.Ok(relativePath);
    }

    public async Task<Result<byte[]>> GetDocumentoEmpleadoAsync(int documentoId)
    {
        try
        {
            const string sql = "SELECT archivo_path FROM documentos_empleado WHERE id = @Id AND activo = 1";
            using var connection = _dapperContext.CreateConnection();
            var path = await connection.ExecuteScalarAsync<string?>(sql, new { Id = documentoId });

            if (string.IsNullOrWhiteSpace(path))
                return Result<byte[]>.Fail("No se encontró el documento en base de datos");

            var absolutePath = Path.IsPathRooted(path) ? path : Path.Combine(_basePath, path);
            if (!File.Exists(absolutePath))
                return Result<byte[]>.Fail("El archivo no existe en disco");

            var bytes = await File.ReadAllBytesAsync(absolutePath);
            return Result<byte[]>.Ok(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leyendo documento de empleado {Id}", documentoId);
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<string>>> GetDocumentosEmpleadoAsync(int empleadoId)
    {
        try
        {
            const string sql = "SELECT archivo_path FROM documentos_empleado WHERE empleado_id = @EmpleadoId AND activo = 1";
            using var connection = _dapperContext.CreateConnection();
            var paths = await connection.QueryAsync<string>(sql, new { EmpleadoId = empleadoId });
            var normalized = paths
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => Path.IsPathRooted(p) ? Path.GetRelativePath(_basePath, p) : p)
                .Select(NormalizeRelative)
                .ToList();

            return Result<IEnumerable<string>>.Ok(normalized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo documentos del empleado {Id}", empleadoId);
            return Result<IEnumerable<string>>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteDocumentoEmpleadoAsync(int documentoId)
    {
        try
        {
            const string sql = "SELECT archivo_path FROM documentos_empleado WHERE id = @Id";
            using var connection = _dapperContext.CreateConnection();
            var path = await connection.ExecuteScalarAsync<string?>(sql, new { Id = documentoId });

            if (string.IsNullOrWhiteSpace(path))
                return Result.Ok();

            var absolutePath = Path.IsPathRooted(path) ? path : Path.Combine(_basePath, path);
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
                _logger.LogInformation("Archivo de documento de empleado eliminado: {Path}", absolutePath);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando documento de empleado {Id}", documentoId);
            return Result.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveGeneratedPdfAsync(string tipo, string fileName, byte[] pdfBytes)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
            return Result<string>.Fail("El PDF está vacío");

        if (!AllowedGeneratedTypes.Contains(tipo))
            return Result<string>.Fail("Tipo de documento generado no soportado");

        var ext = FileHelper.NormalizeExtension(Path.GetExtension(fileName));
        if (!string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase))
            return Result<string>.Fail("Solo se permiten archivos PDF");

        var directory = Path.Combine(_basePath, GENERADOS, tipo);
        Directory.CreateDirectory(directory);

        var sanitizedName = FileHelper.SanitizeFileName(Path.GetFileNameWithoutExtension(fileName));
        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            sanitizedName = $"documento_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }

        var absolutePath = FileHelper.GetUniqueFileName(directory, $"{sanitizedName}{ext}");
        await File.WriteAllBytesAsync(absolutePath, pdfBytes);

        var relativePath = NormalizeRelative(Path.GetRelativePath(_basePath, absolutePath));
        _logger.LogInformation("PDF generado guardado: {Path}", absolutePath);
        return Result<string>.Ok(relativePath);
    }

    public async Task<Result<byte[]>> GetGeneratedPdfAsync(string tipo, string fileName)
    {
        if (!AllowedGeneratedTypes.Contains(tipo))
            return Result<byte[]>.Fail("Tipo de documento generado no soportado");

        var directory = Path.Combine(_basePath, GENERADOS, tipo);
        var target = Path.Combine(directory, FileHelper.SanitizeFileName(fileName));

        if (!File.Exists(target))
            return Result<byte[]>.Fail("El archivo generado no existe");

        var bytes = await File.ReadAllBytesAsync(target);
        return Result<byte[]>.Ok(bytes);
    }

    public string GetGeneratedPdfPath(string tipo, string fileName)
    {
        var sanitized = FileHelper.SanitizeFileName(fileName);
        return Path.Combine(_basePath, GENERADOS, tipo, sanitized);
    }

    public async Task<Result<string>> SaveLogoEmpresaAsync(byte[] imageBytes, string extension)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return Result<string>.Fail("La imagen está vacía");

        if (imageBytes.Length > MaxImageBytes)
            return Result<string>.Fail("La imagen supera el tamaño máximo (5MB)");

        if (!ImageHelper.IsValidImage(imageBytes))
            return Result<string>.Fail("El archivo no es una imagen válida");

        var detectedExt = ImageHelper.GetImageExtension(imageBytes);
        var ext = detectedExt ?? FileHelper.NormalizeExtension(extension);

        if (!AllowedImageExtensions.Contains(ext))
            return Result<string>.Fail("Tipo de imagen no permitido");

        var shouldOptimize = string.Equals(ext, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(ext, ".jpeg", StringComparison.OrdinalIgnoreCase);

        var optimized = shouldOptimize ? ImageHelper.OptimizeForStorage(imageBytes, 80) : imageBytes;
        var directory = Path.Combine(_basePath, CONFIG);
        Directory.CreateDirectory(directory);

        foreach (var file in Directory.GetFiles(directory, "logo.*"))
        {
            File.Delete(file);
        }

        var absolutePath = Path.Combine(directory, $"logo{ext}");
        await File.WriteAllBytesAsync(absolutePath, optimized);

        var relativePath = NormalizeRelative(Path.Combine(CONFIG, $"logo{ext}"));
        _logger.LogInformation("Logo de empresa guardado: {Path}", absolutePath);
        return Result<string>.Ok(relativePath);
    }

    public async Task<Result<byte[]>> GetLogoEmpresaAsync()
    {
        var directory = Path.Combine(_basePath, CONFIG);
        if (!Directory.Exists(directory))
            return Result<byte[]>.Fail("No hay logo configurado");

        var path = Directory.GetFiles(directory, "logo.*").FirstOrDefault();
        if (path == null)
            return Result<byte[]>.Fail("No hay logo configurado");

        var bytes = await File.ReadAllBytesAsync(path);
        return Result<byte[]>.Ok(bytes);
    }

    public string? GetLogoEmpresaPath()
    {
        var directory = Path.Combine(_basePath, CONFIG);
        if (!Directory.Exists(directory)) return null;
        var path = Directory.GetFiles(directory, "logo.*").FirstOrDefault();
        return path;
    }

    public async Task<long> GetTotalStorageSizeAsync()
    {
        long total = 0;
        try
        {
            foreach (var file in Directory.EnumerateFiles(_basePath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    total += new FileInfo(file).Length;
                }
                catch (IOException)
                {
                    // Ignore files that disappear during iteration
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando el tamaño total de almacenamiento");
        }

        return await Task.FromResult(total);
    }

    public async Task<Result> CleanupOrphanFilesAsync()
    {
        try
        {
            var referencedPaths = await GetReferencedPathsAsync();
            var deleted = 0;

            foreach (var file in Directory.EnumerateFiles(_basePath, "*", SearchOption.AllDirectories))
            {
                var relative = NormalizeRelative(Path.GetRelativePath(_basePath, file));
                var topFolder = relative.Split('/').FirstOrDefault();

                if (string.Equals(topFolder, "Backups", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(topFolder, GENERADOS, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(topFolder, CONFIG, StringComparison.OrdinalIgnoreCase))
                {
                    continue; // never clean these
                }

                if (referencedPaths.Contains(relative))
                    continue;

                File.Delete(file);
                deleted++;
            }

            _logger.LogInformation("Limpieza de archivos completada. Eliminados: {Count}", deleted);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante limpieza de archivos huérfanos");
            return Result.Fail($"Error: {ex.Message}");
        }
    }

    private async Task<HashSet<string>> GetReferencedPathsAsync()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        const string sql = @"SELECT foto_path FROM empleados WHERE foto_path IS NOT NULL AND foto_path <> '' AND activo = 1;
SELECT documento_soporte_path FROM permisos WHERE documento_soporte_path IS NOT NULL AND documento_soporte_path <> '' AND activo = 1;
SELECT archivo_adjunto_path FROM contratos WHERE archivo_adjunto_path IS NOT NULL AND archivo_adjunto_path <> '' AND activo = 1;
SELECT archivo_path FROM documentos_empleado WHERE archivo_path IS NOT NULL AND archivo_path <> '' AND activo = 1;";

        using var connection = _dapperContext.CreateConnection();
        using var reader = await connection.QueryMultipleAsync(sql);

        void AddRange(IEnumerable<string?> values)
        {
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value)) continue;
                var relative = Path.IsPathRooted(value)
                    ? Path.GetRelativePath(_basePath, value)
                    : value;

                paths.Add(NormalizeRelative(relative));
            }
        }

        AddRange(await reader.ReadAsync<string?>());
        AddRange(await reader.ReadAsync<string?>());
        AddRange(await reader.ReadAsync<string?>());
        AddRange(await reader.ReadAsync<string?>());

        return paths;
    }

    private static string NormalizeRelative(string path)
    {
        return path.Replace('\\', '/');
    }
}