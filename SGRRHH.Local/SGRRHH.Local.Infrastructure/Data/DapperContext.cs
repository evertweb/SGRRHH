using System.Data;
using System.Diagnostics;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Infrastructure.Data;

/// <summary>
/// Contexto de acceso a datos centralizado.
/// IMPORTANTE: Todas las conexiones a SQLite pasan por aquí.
/// Configurado para concurrencia multiusuario con WAL y busy_timeout.
/// </summary>
public class DapperContext
{
    private readonly DatabasePathResolver _pathResolver;
    private readonly ILogger<DapperContext> _logger;

    // Pragmas de sesión que deben ejecutarse en cada conexión
    private const string SessionPragmas = @"
        PRAGMA busy_timeout = 5000;
        PRAGMA cache_size = -20000;
        PRAGMA temp_store = MEMORY;
    ";

    static DapperContext()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public DapperContext(DatabasePathResolver pathResolver, ILogger<DapperContext> logger)
    {
        _pathResolver = pathResolver;
        _logger = logger;
    }

    /// <summary>
    /// Crea una conexión configurada para concurrencia.
    /// USAR CON 'using' para asegurar que se cierra correctamente.
    /// </summary>
    public IDbConnection CreateConnection()
    {
        _pathResolver.EnsureDirectoriesExist();
        var connection = new SqliteConnection(_pathResolver.GetConnectionString());
        connection.Open();
        
        // CRÍTICO: Aplicar pragmas DESPUÉS de abrir la conexión
        // SQLite ignora pragmas si se pasan en el connection string
        try
        {
            connection.Execute(SessionPragmas);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error applying session pragmas");
        }
        
        return connection;
    }
    
    /// <summary>
    /// Crea una conexión con transacción para operaciones atómicas.
    /// </summary>
    public (IDbConnection connection, IDbTransaction transaction) CreateTransactionScope()
    {
        var connection = CreateConnection();
        var transaction = connection.BeginTransaction();
        return (connection, transaction);
    }

    public async Task RunMigrationsAsync()
    {
        try
        {
            using var connection = (SqliteConnection)CreateConnection();
            await connection.ExecuteAsync(DatabaseInitializer.Pragmas);
            await connection.ExecuteAsync(DatabaseInitializer.Schema);
            await connection.ExecuteAsync(DatabaseInitializer.PerformanceIndexes);
            
            // Migración: Agregar campos de incapacidad a permisos (si no existen)
            await TryAddColumnAsync(connection, "permisos", "incapacidad_id", "INTEGER REFERENCES incapacidades(id)");
            await TryAddColumnAsync(connection, "permisos", "convertido_a_incapacidad", "INTEGER DEFAULT 0");
            
            // Migración: Agregar campos de seguridad social a empleados (si no existen)
            await TryAddColumnAsync(connection, "empleados", "codigo_eps", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "codigo_arl", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "clase_riesgo_arl", "INTEGER DEFAULT 1");
            await TryAddColumnAsync(connection, "empleados", "codigo_afp", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "caja_compensacion", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "codigo_caja_compensacion", "TEXT");
            
            // Solo ejecutar SeedData si no se ha ejecutado antes
            // Verificamos si existe la configuración "seed_data_initialized"
            var seedInitialized = await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT valor FROM configuracion_sistema WHERE clave = 'seed_data_initialized' LIMIT 1");
            
            if (string.IsNullOrEmpty(seedInitialized))
            {
                await connection.ExecuteAsync(DatabaseInitializer.SeedData);
                
                // Marcar que el seed data ya fue inicializado
                await connection.ExecuteAsync(@"
                    INSERT OR REPLACE INTO configuracion_sistema (clave, valor, descripcion, categoria, activo) 
                    VALUES ('seed_data_initialized', 'true', 'Indica si los datos semilla ya fueron insertados', 'Sistema', 1);
                ");
                
                _logger.LogInformation("Seed data initialized successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running SQLite migrations");
            throw;
        }
    }
    
    /// <summary>
    /// Intenta agregar una columna a una tabla. Si la columna ya existe, no hace nada.
    /// </summary>
    private async Task TryAddColumnAsync(SqliteConnection connection, string tableName, string columnName, string columnDefinition)
    {
        try
        {
            // Verificar si la columna ya existe
            var columns = await connection.QueryAsync<dynamic>($"PRAGMA table_info({tableName})");
            var columnExists = columns.Any(c => ((string)c.name).Equals(columnName, StringComparison.OrdinalIgnoreCase));
            
            if (!columnExists)
            {
                await connection.ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition}");
                _logger.LogInformation("Columna {Column} agregada a tabla {Table}", columnName, tableName);
            }
        }
        catch (Exception ex)
        {
            // Si falla, puede ser porque la columna ya existe o hay otro problema
            _logger.LogDebug("No se pudo agregar columna {Column} a {Table}: {Error}", columnName, tableName, ex.Message);
        }
    }

    public async Task<PagedResult<T>> QueryPagedAsync<T>(string sql, object? parameters, int page, int pageSize, int slowQueryThresholdMs = 500, string? operationName = null)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 500);

        using var connection = CreateConnection();
        var stopwatch = Stopwatch.StartNew();

        var countSql = $"SELECT COUNT(*) FROM ({sql})";
        var pagedSql = $"{sql} LIMIT @PageSize OFFSET @Offset";

        var parameterBag = new DynamicParameters(parameters);
        parameterBag.Add("PageSize", safePageSize);
        parameterBag.Add("Offset", (safePage - 1) * safePageSize);

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<T>(pagedSql, parameterBag);

        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds > slowQueryThresholdMs)
        {
            _logger.LogWarning("Slow query detected ({Operation}): {Elapsed} ms (page {Page}, size {PageSize})", operationName ?? typeof(T).Name, stopwatch.ElapsedMilliseconds, safePage, safePageSize);
        }

        return new PagedResult<T>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = safePage,
            PageSize = safePageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)safePageSize)
        };
    }
}
