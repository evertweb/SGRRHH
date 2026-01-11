using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Infrastructure.Data;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio para mantenimiento de la base de datos.
/// Permite limpiar datos o resetear para distribución.
/// </summary>
public class DatabaseMaintenanceService : IDatabaseMaintenanceService
{
    private readonly DatabasePathResolver _pathResolver;
    private readonly ILogger<DatabaseMaintenanceService> _logger;
    
    // Tablas a limpiar (en orden para respetar foreign keys)
    private static readonly string[] TablasParaLimpiar = new[]
    {
        "detalles_actividad", "registros_diarios", "documentos_empleado",
        "proyectos_empleados", "nominas", "prestaciones", "permisos",
        "vacaciones", "contratos", "empleados", "proyectos", "actividades",
        "cargos", "departamentos", "tipos_permiso", "festivos_colombia",
        "configuracion_legal", "configuracion_sistema", "audit_logs"
    };
    
    // Hash BCrypt para "Admin123!" (generado con work factor 11)
    private const string DefaultAdminPasswordHash = "$2a$11$OWtWjbnlNgdhoar3QcVu2OjSnFAzqytgUxFjEYZF4CYAp3vuB/4CW";

    public DatabaseMaintenanceService(
        DatabasePathResolver pathResolver,
        ILogger<DatabaseMaintenanceService> logger)
    {
        _pathResolver = pathResolver;
        _logger = logger;
    }

    public async Task<DatabaseCleanupResult> LimpiarBaseDatosAsync()
    {
        var result = new DatabaseCleanupResult();
        
        try
        {
            _logger.LogInformation("Iniciando limpieza de base de datos (preservando usuarios)...");
            
            var connectionString = _pathResolver.GetConnectionString();
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            await using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                // Deshabilitar foreign keys temporalmente
                await ExecuteNonQueryAsync(connection, "PRAGMA foreign_keys = OFF;", transaction);
                
                foreach (var tabla in TablasParaLimpiar)
                {
                    try
                    {
                        // Contar registros antes de eliminar
                        var count = await ExecuteScalarAsync(connection, $"SELECT COUNT(*) FROM {tabla};", transaction);
                        
                        if (count > 0)
                        {
                            await ExecuteNonQueryAsync(connection, $"DELETE FROM {tabla};", transaction);
                            result.TablasAfectadas.Add($"{tabla} ({count} registros)");
                            result.TotalRegistrosEliminados += count;
                            result.TablasLimpiadas++;
                            _logger.LogInformation("Tabla {Tabla}: {Count} registros eliminados", tabla, count);
                        }
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 1) // Table not found
                    {
                        _logger.LogDebug("Tabla {Tabla} no existe, omitiendo", tabla);
                    }
                }
                
                // Eliminar flag de seed data para que se regeneren los datos base
                await ExecuteNonQueryAsync(connection, 
                    "DELETE FROM configuracion_sistema WHERE clave = 'seed_data_initialized';", 
                    transaction);
                
                await transaction.CommitAsync();
                
                // Reactivar foreign keys
                await ExecuteNonQueryAsync(connection, "PRAGMA foreign_keys = ON;");
                
                // Contar usuarios preservados
                result.UsuariosPreservados = await ExecuteScalarAsync(connection, 
                    "SELECT COUNT(*) FROM usuarios WHERE activo = 1;");
                
                // Optimizar base de datos
                await ExecuteNonQueryAsync(connection, "VACUUM;");
                
                result.Exitoso = true;
                _logger.LogInformation("Limpieza completada: {Total} registros eliminados, {Usuarios} usuarios preservados",
                    result.TotalRegistrosEliminados, result.UsuariosPreservados);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante limpieza de base de datos");
            result.Exitoso = false;
            result.MensajeError = ex.Message;
        }
        
        return result;
    }

    public async Task<DatabaseCleanupResult> ResetearParaDistribucionAsync()
    {
        var result = new DatabaseCleanupResult();
        
        try
        {
            _logger.LogWarning("Iniciando RESET COMPLETO de base de datos para distribución...");
            
            var connectionString = _pathResolver.GetConnectionString();
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            await using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                // Deshabilitar foreign keys temporalmente
                await ExecuteNonQueryAsync(connection, "PRAGMA foreign_keys = OFF;", transaction);
                
                // Limpiar TODAS las tablas incluyendo usuarios
                var todasLasTablas = TablasParaLimpiar.Concat(new[] { "usuarios" }).ToArray();
                
                foreach (var tabla in todasLasTablas)
                {
                    try
                    {
                        var count = await ExecuteScalarAsync(connection, $"SELECT COUNT(*) FROM {tabla};", transaction);
                        
                        if (count > 0)
                        {
                            await ExecuteNonQueryAsync(connection, $"DELETE FROM {tabla};", transaction);
                            result.TablasAfectadas.Add($"{tabla} ({count} registros)");
                            result.TotalRegistrosEliminados += count;
                            result.TablasLimpiadas++;
                        }
                        
                        // Resetear autoincrement
                        await ExecuteNonQueryAsync(connection, 
                            $"DELETE FROM sqlite_sequence WHERE name = '{tabla}';", 
                            transaction);
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
                    {
                        _logger.LogDebug("Tabla {Tabla} no existe, omitiendo", tabla);
                    }
                }
                
                // Crear usuario admin por defecto
                var insertAdmin = @"
                    INSERT INTO usuarios (id, username, password_hash, nombre_completo, rol, activo, fecha_creacion)
                    VALUES (1, 'admin', @hash, 'Administrador del Sistema', 1, 1, datetime('now'));
                ";
                
                await using var cmd = connection.CreateCommand();
                cmd.Transaction = (SqliteTransaction?)transaction;
                cmd.CommandText = insertAdmin;
                cmd.Parameters.AddWithValue("@hash", DefaultAdminPasswordHash);
                await cmd.ExecuteNonQueryAsync();
                
                _logger.LogInformation("Usuario admin por defecto creado (password: Admin123!)");
                
                // Insertar datos semilla básicos
                await InsertarDatosSemillaAsync(connection, transaction);
                
                await transaction.CommitAsync();
                
                // Reactivar foreign keys
                await ExecuteNonQueryAsync(connection, "PRAGMA foreign_keys = ON;");
                
                // Optimizar base de datos
                await ExecuteNonQueryAsync(connection, "VACUUM;");
                
                result.Exitoso = true;
                result.UsuariosPreservados = 1; // El admin por defecto
                
                _logger.LogWarning("RESET COMPLETO completado. Usuario: admin / Password: Admin123!");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante reset de base de datos");
            result.Exitoso = false;
            result.MensajeError = ex.Message;
        }
        
        return result;
    }

    public async Task<DatabaseStats> ObtenerEstadisticasAsync()
    {
        var stats = new DatabaseStats();
        
        try
        {
            var dbPath = _pathResolver.GetDatabasePath();
            if (File.Exists(dbPath))
            {
                stats.TamanoBytes = new FileInfo(dbPath).Length;
            }
            
            var connectionString = _pathResolver.GetConnectionString();
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            // Contar usuarios
            stats.TotalUsuarios = await ExecuteScalarAsync(connection, 
                "SELECT COUNT(*) FROM usuarios WHERE activo = 1;");
            
            // Contar empleados
            try
            {
                stats.TotalEmpleados = await ExecuteScalarAsync(connection, 
                    "SELECT COUNT(*) FROM empleados WHERE activo = 1;");
            }
            catch { stats.TotalEmpleados = 0; }
            
            // Contar registros por tabla
            foreach (var tabla in TablasParaLimpiar)
            {
                try
                {
                    var count = await ExecuteScalarAsync(connection, $"SELECT COUNT(*) FROM {tabla};");
                    stats.RegistrosPorTabla[tabla] = count;
                    stats.TotalRegistros += count;
                }
                catch { /* tabla no existe */ }
            }
            
            stats.RegistrosPorTabla["usuarios"] = stats.TotalUsuarios;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo estadísticas de base de datos");
        }
        
        return stats;
    }

    private async Task InsertarDatosSemillaAsync(SqliteConnection connection, System.Data.Common.DbTransaction transaction)
    {
        // NOTA: Los catálogos (departamentos, tipos_permiso, cargos, etc.) deben ser creados
        // por el administrador según las necesidades específicas de cada empresa.
        
        // Configuración legal 2026
        var configLegal = @"
            INSERT INTO configuracion_legal (
                año, salario_minimo_mensual, auxilio_transporte,
                porcentaje_salud_empleado, porcentaje_salud_empleador,
                porcentaje_pension_empleado, porcentaje_pension_empleador,
                porcentaje_caja_compensacion, porcentaje_icbf, porcentaje_sena,
                arl_clase1_min, arl_clase5_max, porcentaje_intereses_cesantias,
                dias_vacaciones_año, horas_maximas_semanales, horas_ordinarias_diarias,
                recargo_hora_extra_diurna, recargo_hora_extra_nocturna,
                recargo_hora_nocturna, recargo_hora_dominical_festivo,
                edad_minima_laboral, observaciones, es_vigente
            ) VALUES (
                2026, 1423500, 200000,
                4.0, 8.5, 4.0, 12.0,
                4.0, 3.0, 2.0,
                0.522, 6.96, 12.0,
                15, 48, 8,
                25.0, 75.0, 35.0, 75.0,
                18, 'Configuración legal Colombia 2026', 1
            );
        ";
        await ExecuteNonQueryAsync(connection, configLegal, transaction);
        
        _logger.LogInformation("Datos semilla básicos insertados");
    }

    private async Task ExecuteNonQueryAsync(SqliteConnection connection, string sql, System.Data.Common.DbTransaction? transaction = null)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        if (transaction != null)
            cmd.Transaction = (SqliteTransaction?)transaction;
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<int> ExecuteScalarAsync(SqliteConnection connection, string sql, System.Data.Common.DbTransaction? transaction = null)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        if (transaction != null)
            cmd.Transaction = (SqliteTransaction?)transaction;
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
