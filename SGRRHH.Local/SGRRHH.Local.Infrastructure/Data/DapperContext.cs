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
            
            // =====================================================
            // MIGRACIONES CONTACTO EXPANDIDO (Enero 2026)
            // =====================================================
            
            // Contacto expandido
            await TryAddColumnAsync(connection, "empleados", "telefono_celular", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "telefono_fijo", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "whatsapp", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "municipio", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "barrio", "TEXT");
            
            // Información médica (emergencias)
            await TryAddColumnAsync(connection, "empleados", "tipo_sangre", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "alergias", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "condiciones_medicas", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "medicamentos_actuales", "TEXT");
            
            // Contactos de emergencia expandidos
            await TryAddColumnAsync(connection, "empleados", "parentesco_contacto_emergencia", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "telefono_emergencia_2", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "contacto_emergencia_2", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "telefono_emergencia_2_contacto_2", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "parentesco_contacto_emergencia_2", "TEXT");
            await TryAddColumnAsync(connection, "empleados", "telefono_emergencia_2_alternativo", "TEXT");
            
            // =====================================================
            // MIGRACIONES SILVICULTURALES Y PROYECTOS
            // =====================================================
            
            // Migración: Campos silviculturales para proyectos
            await TryAddColumnAsync(connection, "proyectos", "tipo_proyecto", "INTEGER DEFAULT 1");
            await TryAddColumnAsync(connection, "proyectos", "predio", "TEXT");
            await TryAddColumnAsync(connection, "proyectos", "lote", "TEXT");
            await TryAddColumnAsync(connection, "proyectos", "departamento", "TEXT");
            await TryAddColumnAsync(connection, "proyectos", "municipio", "TEXT");
            await TryAddColumnAsync(connection, "proyectos", "vereda", "TEXT");
            await TryAddColumnAsync(connection, "proyectos", "latitud", "REAL");
            await TryAddColumnAsync(connection, "proyectos", "longitud", "REAL");
            await TryAddColumnAsync(connection, "proyectos", "altitud_msnm", "INTEGER");
            await TryAddColumnAsync(connection, "proyectos", "especie_id", "INTEGER REFERENCES especies_forestales(id)");
            await TryAddColumnAsync(connection, "proyectos", "area_hectareas", "REAL");
            await TryAddColumnAsync(connection, "proyectos", "fecha_siembra", "TEXT");
            await TryAddColumnAsync(connection, "proyectos", "densidad_inicial", "INTEGER");
            await TryAddColumnAsync(connection, "proyectos", "densidad_actual", "INTEGER");
            await TryAddColumnAsync(connection, "proyectos", "turno_cosecha_anios", "INTEGER");
            await TryAddColumnAsync(connection, "proyectos", "tipo_tenencia", "INTEGER DEFAULT 1");
            await TryAddColumnAsync(connection, "proyectos", "certificacion", "TEXT");
            await TryAddColumnAsync(connection, "proyectos", "total_horas_trabajadas", "REAL DEFAULT 0");
            await TryAddColumnAsync(connection, "proyectos", "costo_mano_obra_acumulado", "REAL DEFAULT 0");
            await TryAddColumnAsync(connection, "proyectos", "total_jornales", "INTEGER DEFAULT 0");
            await TryAddColumnAsync(connection, "proyectos", "fecha_ultima_actualizacion_metricas", "TEXT");
            
            // Migración: Campos adicionales para gestión de cuadrillas (proyectos_empleados)
            await TryAddColumnAsync(connection, "proyectos_empleados", "rol_enum", "INTEGER");
            await TryAddColumnAsync(connection, "proyectos_empleados", "es_lider_cuadrilla", "INTEGER DEFAULT 0");
            await TryAddColumnAsync(connection, "proyectos_empleados", "porcentaje_dedicacion", "INTEGER DEFAULT 100");
            await TryAddColumnAsync(connection, "proyectos_empleados", "tipo_vinculacion", "TEXT");
            await TryAddColumnAsync(connection, "proyectos_empleados", "motivo_desasignacion", "TEXT");
            await TryAddColumnAsync(connection, "proyectos_empleados", "total_horas_trabajadas", "REAL DEFAULT 0");
            await TryAddColumnAsync(connection, "proyectos_empleados", "total_jornales", "INTEGER DEFAULT 0");
            await TryAddColumnAsync(connection, "proyectos_empleados", "ultima_fecha_trabajo", "TEXT");
            await TryAddColumnAsync(connection, "proyectos_empleados", "dias_trabajados", "INTEGER DEFAULT 0");
            
            // Migración: Campos avanzados para actividades (snake_case estandarizado)
            // Nuevos nombres de columnas estandarizados
            await TryAddColumnAsync(connection, "actividades", "category_id", "INTEGER REFERENCES activity_categories(id)");
            await TryAddColumnAsync(connection, "actividades", "unit_of_measure", "INTEGER DEFAULT 0");
            await TryAddColumnAsync(connection, "actividades", "unit_abbreviation", "TEXT");
            await TryAddColumnAsync(connection, "actividades", "expected_yield", "REAL");
            await TryAddColumnAsync(connection, "actividades", "minimum_yield", "REAL");
            await TryAddColumnAsync(connection, "actividades", "unit_cost", "REAL");
            await TryAddColumnAsync(connection, "actividades", "requires_quantity", "INTEGER DEFAULT 0");
            await TryAddColumnAsync(connection, "actividades", "applicable_project_types", "TEXT");
            await TryAddColumnAsync(connection, "actividades", "applicable_species", "TEXT");
            await TryAddColumnAsync(connection, "actividades", "is_featured", "INTEGER DEFAULT 0");
            await TryAddColumnAsync(connection, "actividades", "category_text", "TEXT");
            
            // Migración: Campos adicionales para detalles de actividades
            await TryAddColumnAsync(connection, "detalles_actividad", "cantidad", "REAL");
            await TryAddColumnAsync(connection, "detalles_actividad", "unidad_medida", "TEXT");
            await TryAddColumnAsync(connection, "detalles_actividad", "lote_especifico", "TEXT");
            
            // =====================================================
            // MIGRACIONES DE SEGUIMIENTO DE PERMISOS
            // =====================================================
            
            // Migración: Campos de seguimiento para permisos
            await TryAddColumnAsync(connection, "permisos", "tipo_resolucion", "INTEGER DEFAULT 0");
            await TryAddColumnAsync(connection, "permisos", "requiere_documento_posterior", "INTEGER DEFAULT 0");
            await TryAddColumnAsync(connection, "permisos", "fecha_limite_documento", "TEXT");
            await TryAddColumnAsync(connection, "permisos", "fecha_entrega_documento", "TEXT");
            await TryAddColumnAsync(connection, "permisos", "horas_compensar", "INTEGER");
            await TryAddColumnAsync(connection, "permisos", "horas_compensadas", "INTEGER DEFAULT 0");
            await TryAddColumnAsync(connection, "permisos", "fecha_limite_compensacion", "TEXT");
            await TryAddColumnAsync(connection, "permisos", "monto_descuento", "REAL");
            await TryAddColumnAsync(connection, "permisos", "periodo_descuento", "TEXT");
            await TryAddColumnAsync(connection, "permisos", "completado", "INTEGER DEFAULT 0");
            
            // Migración: Campos de seguimiento para tipos_permiso
            await TryAddColumnAsync(connection, "tipos_permiso", "tipo_resolucion_por_defecto", "INTEGER DEFAULT 1");
            await TryAddColumnAsync(connection, "tipos_permiso", "dias_limite_documento", "INTEGER DEFAULT 7");
            await TryAddColumnAsync(connection, "tipos_permiso", "dias_limite_compensacion", "INTEGER DEFAULT 30");
            await TryAddColumnAsync(connection, "tipos_permiso", "horas_compensar_por_dia", "INTEGER DEFAULT 8");
            await TryAddColumnAsync(connection, "tipos_permiso", "genera_descuento", "INTEGER DEFAULT 0");
            await TryAddColumnAsync(connection, "tipos_permiso", "porcentaje_descuento", "REAL");
            
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
