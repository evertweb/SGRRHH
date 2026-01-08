using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly DatabasePathResolver _pathResolver;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupPath;
    private readonly string _databasePath;
    private readonly string _storagePath;
    
    public BackupService(
        DatabasePathResolver pathResolver,
        IConfiguration configuration,
        ILogger<BackupService> logger)
    {
        _pathResolver = pathResolver;
        _logger = logger;
        _backupPath = pathResolver.GetBackupPath();
        _databasePath = pathResolver.GetDatabasePath();
        _storagePath = pathResolver.GetStoragePath();
        
        // Asegurar que el directorio de backups existe
        if (!Directory.Exists(_backupPath))
        {
            Directory.CreateDirectory(_backupPath);
        }
    }
    
    public async Task<Result<string>> CreateBackupAsync(string? description = null)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupFolder = Path.Combine(_backupPath, timestamp);
            Directory.CreateDirectory(backupFolder);
            
            _logger.LogInformation("Iniciando backup en: {BackupFolder}", backupFolder);
            
            // 1. Copiar base de datos SQLite
            var dbBackupPath = Path.Combine(backupFolder, "sgrrhh.db");
            File.Copy(_databasePath, dbBackupPath, overwrite: true);
            _logger.LogInformation("Base de datos copiada");
            
            // 2. Comprimir carpeta de fotos
            var fotosPath = Path.Combine(_storagePath, "Fotos");
            if (Directory.Exists(fotosPath) && Directory.GetFiles(fotosPath, "*", SearchOption.AllDirectories).Length > 0)
            {
                var fotosZip = Path.Combine(backupFolder, "Fotos.zip");
                ZipFile.CreateFromDirectory(fotosPath, fotosZip, CompressionLevel.Optimal, false);
                _logger.LogInformation("Fotos comprimidas");
            }
            
            // 3. Comprimir documentos
            var docsPath = Path.Combine(_storagePath, "Documentos");
            if (Directory.Exists(docsPath) && Directory.GetFiles(docsPath, "*", SearchOption.AllDirectories).Length > 0)
            {
                var docsZip = Path.Combine(backupFolder, "Documentos.zip");
                ZipFile.CreateFromDirectory(docsPath, docsZip, CompressionLevel.Optimal, false);
                _logger.LogInformation("Documentos comprimidos");
            }
            
            // 4. Guardar metadatos
            var metadata = new
            {
                CreatedAt = DateTime.Now,
                Description = description,
                Version = "1.0.0",
                DatabaseSize = new FileInfo(_databasePath).Length,
                IncludesFiles = true,
                MachineName = Environment.MachineName,
                UserName = Environment.UserName
            };
            
            var metadataPath = Path.Combine(backupFolder, "backup.json");
            await File.WriteAllTextAsync(metadataPath, 
                JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                }));
            
            _logger.LogInformation("Backup creado exitosamente: {BackupFolder}", backupFolder);
            return Result<string>.Ok(timestamp, "Backup creado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando backup");
            return Result<string>.Fail($"Error creando backup: {ex.Message}");
        }
    }
    
    public async Task<Result> CreateScheduledBackupAsync()
    {
        var result = await CreateBackupAsync("Backup automático programado");
        return result.IsSuccess 
            ? Result.Ok(result.Message) 
            : Result.Fail(result.Error ?? "Error desconocido");
    }
    
    public async Task<Result> RestoreFromBackupAsync(string backupFolderName)
    {
        try
        {
            var backupFolder = Path.Combine(_backupPath, backupFolderName);
            
            if (!Directory.Exists(backupFolder))
                return Result.Fail("Backup no encontrado");
            
            _logger.LogInformation("Iniciando restauración desde: {BackupFolder}", backupFolder);
            
            // 1. Restaurar base de datos
            var dbBackupPath = Path.Combine(backupFolder, "sgrrhh.db");
            if (File.Exists(dbBackupPath))
            {
                // Hacer backup de seguridad antes de restaurar
                var safetyBackup = _databasePath + ".before-restore";
                if (File.Exists(_databasePath))
                {
                    File.Copy(_databasePath, safetyBackup, overwrite: true);
                    _logger.LogInformation("Backup de seguridad creado");
                }
                
                File.Copy(dbBackupPath, _databasePath, overwrite: true);
                _logger.LogInformation("Base de datos restaurada");
            }
            
            // 2. Restaurar fotos
            var fotosZip = Path.Combine(backupFolder, "Fotos.zip");
            if (File.Exists(fotosZip))
            {
                var fotosPath = Path.Combine(_storagePath, "Fotos");
                if (Directory.Exists(fotosPath))
                {
                    Directory.Delete(fotosPath, recursive: true);
                }
                Directory.CreateDirectory(fotosPath);
                
                ZipFile.ExtractToDirectory(fotosZip, fotosPath, overwriteFiles: true);
                _logger.LogInformation("Fotos restauradas");
            }
            
            // 3. Restaurar documentos
            var docsZip = Path.Combine(backupFolder, "Documentos.zip");
            if (File.Exists(docsZip))
            {
                var docsPath = Path.Combine(_storagePath, "Documentos");
                if (Directory.Exists(docsPath))
                {
                    Directory.Delete(docsPath, recursive: true);
                }
                Directory.CreateDirectory(docsPath);
                
                ZipFile.ExtractToDirectory(docsZip, docsPath, overwriteFiles: true);
                _logger.LogInformation("Documentos restaurados");
            }
            
            _logger.LogInformation("Backup restaurado exitosamente: {BackupFolder}", backupFolder);
            return Result.Ok("Backup restaurado exitosamente. Se recomienda reiniciar la aplicación.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restaurando backup");
            return Result.Fail($"Error restaurando backup: {ex.Message}");
        }
    }
    
    public async Task<Result<IEnumerable<BackupInfo>>> GetAvailableBackupsAsync()
    {
        try
        {
            if (!Directory.Exists(_backupPath))
                return Result<IEnumerable<BackupInfo>>.Ok(Enumerable.Empty<BackupInfo>());
            
            var backups = new List<BackupInfo>();
            
            foreach (var dir in Directory.GetDirectories(_backupPath).OrderByDescending(d => d))
            {
                var metadataPath = Path.Combine(dir, "backup.json");
                var info = new BackupInfo
                {
                    FileName = Path.GetFileName(dir),
                    CreatedAt = Directory.GetCreationTime(dir)
                };
                
                // Calcular tamaño total
                info.SizeBytes = Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
                    .Sum(f => new FileInfo(f).Length);
                
                // Leer metadatos si existen
                if (File.Exists(metadataPath))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(metadataPath);
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        
                        if (root.TryGetProperty("Description", out var desc))
                            info.Description = desc.GetString();
                        
                        if (root.TryGetProperty("IncludesFiles", out var includes))
                            info.IncludesFiles = includes.GetBoolean();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error leyendo metadatos de backup: {Dir}", dir);
                    }
                }
                
                backups.Add(info);
            }
            
            return Result<IEnumerable<BackupInfo>>.Ok(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listando backups");
            return Result<IEnumerable<BackupInfo>>.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Result<int>> CleanupOldBackupsAsync(int keepDays = 30)
    {
        try
        {
            if (!Directory.Exists(_backupPath))
                return Result<int>.Ok(0, "No hay backups para limpiar");
            
            var cutoffDate = DateTime.Now.AddDays(-keepDays);
            var deleted = 0;
            
            foreach (var dir in Directory.GetDirectories(_backupPath))
            {
                var createdAt = Directory.GetCreationTime(dir);
                if (createdAt < cutoffDate)
                {
                    Directory.Delete(dir, recursive: true);
                    deleted++;
                    _logger.LogInformation("Backup antiguo eliminado: {Dir}", dir);
                }
            }
            
            _logger.LogInformation("Limpieza de backups completada: {Deleted} eliminados", deleted);
            return Result<int>.Ok(deleted, $"{deleted} backup(s) antiguo(s) eliminado(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error limpiando backups antiguos");
            return Result<int>.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Result<long>> GetBackupsFolderSizeAsync()
    {
        try
        {
            if (!Directory.Exists(_backupPath))
                return Result<long>.Ok(0);
            
            var totalSize = Directory.GetFiles(_backupPath, "*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);
            
            return Result<long>.Ok(totalSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando tamaño de backups");
            return Result<long>.Fail($"Error: {ex.Message}");
        }
    }
}
