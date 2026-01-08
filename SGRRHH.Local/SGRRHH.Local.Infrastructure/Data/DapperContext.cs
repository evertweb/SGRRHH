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
        
        // Aplicar pragmas de sesión para cada conexión
        connection.Execute(SessionPragmas);
        
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
            await connection.ExecuteAsync(DatabaseInitializer.SeedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running SQLite migrations");
            throw;
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
