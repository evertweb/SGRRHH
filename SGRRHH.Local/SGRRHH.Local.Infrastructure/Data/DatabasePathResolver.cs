using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SGRRHH.Local.Infrastructure.Data;

public class DatabasePathResolver
{
    private readonly string _databasePath;
    private readonly string _storagePath;
    private readonly string _backupPath;
    private readonly ILogger<DatabasePathResolver> _logger;

    public DatabasePathResolver(IConfiguration configuration, ILogger<DatabasePathResolver> logger)
    {
        _logger = logger;

        var baseDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SGRRHH", "Data");
        _databasePath = configuration["LocalDatabase:DatabasePath"] ?? Path.Combine(baseDataPath, "sgrrhh.db");
        _storagePath = configuration["LocalDatabase:StoragePath"] ?? baseDataPath;
        _backupPath = configuration["LocalDatabase:BackupPath"] ?? Path.Combine(baseDataPath, "Backups");
    }

    public string GetDatabasePath() => _databasePath;

    public string GetStoragePath() => _storagePath;

    public string GetBackupPath() => _backupPath;

    public string GetConnectionString()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            ForeignKeys = true,
            Cache = SqliteCacheMode.Shared,
            Mode = SqliteOpenMode.ReadWriteCreate
        };

        return builder.ToString();
    }

    public void EnsureDirectoriesExist()
    {
        try
        {
            var dbDirectory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            if (!Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring database directories");
            throw;
        }
    }
}
