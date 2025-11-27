using SGRRHH.Core.Common;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de backup de la base de datos SQLite
/// </summary>
public class BackupService : IBackupService
{
    private readonly AppDbContext _context;
    private readonly string _dbPath;
    private readonly string _backupPath;
    
    public BackupService(AppDbContext context)
    {
        _context = context;
        
        // Obtener la ruta de la base de datos desde la cadena de conexión
        var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        _dbPath = Path.Combine(dataPath, "sgrrhh.db");
        _backupPath = Path.Combine(dataPath, "backups");
        
        // Crear carpeta de backups si no existe
        if (!Directory.Exists(_backupPath))
        {
            Directory.CreateDirectory(_backupPath);
        }
    }
    
    public async Task<ServiceResult<string>> CrearBackupAsync(string? rutaDestino = null)
    {
        try
        {
            // Generar nombre del archivo de backup
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var nombreArchivo = $"sgrrhh_backup_{timestamp}.db";
            
            // Determinar ruta de destino
            var destino = rutaDestino ?? Path.Combine(_backupPath, nombreArchivo);
            
            // Si se proporciona solo un directorio, agregar el nombre del archivo
            if (Directory.Exists(rutaDestino))
            {
                destino = Path.Combine(rutaDestino, nombreArchivo);
            }
            
            // Asegurar que la ruta termina en .db
            if (!destino.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                destino = destino + ".db";
            }
            
            // Crear directorio de destino si no existe
            var directorioDestino = Path.GetDirectoryName(destino);
            if (!string.IsNullOrEmpty(directorioDestino) && !Directory.Exists(directorioDestino))
            {
                Directory.CreateDirectory(directorioDestino);
            }
            
            // Usar SQLite backup API para crear una copia consistente
            await Task.Run(() =>
            {
                using var sourceConnection = new SqliteConnection($"Data Source={_dbPath}");
                using var destinationConnection = new SqliteConnection($"Data Source={destino}");
                
                sourceConnection.Open();
                destinationConnection.Open();
                
                sourceConnection.BackupDatabase(destinationConnection);
            });
            
            return ServiceResult<string>.Ok(destino, $"Backup creado exitosamente: {nombreArchivo}");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Fail($"Error al crear backup: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult> RestaurarBackupAsync(string rutaBackup)
    {
        try
        {
            // Validar que el archivo existe
            if (!File.Exists(rutaBackup))
            {
                return ServiceResult.Fail("El archivo de backup no existe");
            }
            
            // Validar que es un archivo SQLite válido
            var validacion = await ValidarBackupAsync(rutaBackup);
            if (!validacion.Success)
            {
                return validacion;
            }
            
            // Crear backup de la base de datos actual antes de restaurar
            var backupActual = await CrearBackupAsync();
            if (!backupActual.Success)
            {
                return ServiceResult.Fail($"No se pudo crear backup de seguridad antes de restaurar: {backupActual.Message}");
            }
            
            // Cerrar todas las conexiones activas (EF Core debería manejar esto)
            // Nota: En producción, se recomienda reiniciar la aplicación después de restaurar
            
            await Task.Run(() =>
            {
                // Copiar el archivo de backup sobre la base de datos actual
                // SQLite permite esto cuando no hay conexiones activas
                using var sourceConnection = new SqliteConnection($"Data Source={rutaBackup}");
                using var destinationConnection = new SqliteConnection($"Data Source={_dbPath}");
                
                sourceConnection.Open();
                destinationConnection.Open();
                
                // Usar VACUUM INTO para crear una copia limpia o backup directamente
                sourceConnection.BackupDatabase(destinationConnection);
            });
            
            return ServiceResult.Ok("Base de datos restaurada exitosamente. Se recomienda reiniciar la aplicación.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Error al restaurar backup: {ex.Message}");
        }
    }
    
    public async Task<List<BackupInfo>> ListarBackupsAsync()
    {
        var backups = new List<BackupInfo>();
        
        await Task.Run(() =>
        {
            if (Directory.Exists(_backupPath))
            {
                var archivos = Directory.GetFiles(_backupPath, "*.db")
                    .OrderByDescending(f => File.GetCreationTime(f));
                
                foreach (var archivo in archivos)
                {
                    var fileInfo = new FileInfo(archivo);
                    backups.Add(new BackupInfo
                    {
                        NombreArchivo = fileInfo.Name,
                        RutaCompleta = fileInfo.FullName,
                        FechaCreacion = fileInfo.CreationTime,
                        TamañoBytes = fileInfo.Length
                    });
                }
            }
        });
        
        return backups;
    }
    
    public async Task<ServiceResult> EliminarBackupAsync(string rutaBackup)
    {
        try
        {
            if (!File.Exists(rutaBackup))
            {
                return ServiceResult.Fail("El archivo de backup no existe");
            }
            
            await Task.Run(() => File.Delete(rutaBackup));
            
            return ServiceResult.Ok("Backup eliminado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Error al eliminar backup: {ex.Message}");
        }
    }
    
    public string GetRutaBackups()
    {
        return _backupPath;
    }
    
    public async Task<ServiceResult> ValidarBackupAsync(string rutaBackup)
    {
        try
        {
            if (!File.Exists(rutaBackup))
            {
                return ServiceResult.Fail("El archivo no existe");
            }
            
            // Verificar que es un archivo SQLite válido
            await Task.Run(() =>
            {
                using var connection = new SqliteConnection($"Data Source={rutaBackup}");
                connection.Open();
                
                // Ejecutar una consulta simple para verificar integridad
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA integrity_check;";
                var result = command.ExecuteScalar()?.ToString();
                
                if (result != "ok")
                {
                    throw new Exception("El archivo de backup está corrupto");
                }
                
                // Verificar que tiene las tablas esperadas
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Usuarios';";
                var hasUsuarios = command.ExecuteScalar();
                
                if (hasUsuarios == null)
                {
                    throw new Exception("El archivo no parece ser un backup válido del sistema SGRRHH");
                }
            });
            
            return ServiceResult.Ok("El archivo de backup es válido");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Archivo de backup inválido: {ex.Message}");
        }
    }
}
