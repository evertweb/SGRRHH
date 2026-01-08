using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SGRRHH.Local.Infrastructure.Data;
using Xunit;

namespace SGRRHH.Local.Tests;

/// <summary>
/// Fixture para crear una base de datos SQLite TEMPORAL para tests.
/// NO usa la base de datos de producción.
/// Cada test obtiene una DB limpia y temporal que se elimina al finalizar.
/// </summary>
public class TestDatabaseFixture : IDisposable
{
    public string DatabasePath { get; }
    public DapperContext Context { get; }
    private readonly DatabasePathResolver _pathResolver;

    public TestDatabaseFixture()
    {
        // Crear base de datos TEMPORAL en carpeta de temp del sistema
        DatabasePath = Path.Combine(Path.GetTempPath(), $"sgrrhh_test_{Guid.NewGuid()}.db");
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LocalDatabase:DatabasePath"] = DatabasePath,
                ["LocalDatabase:StoragePath"] = Path.Combine(Path.GetTempPath(), "sgrrhh_storage"),
                ["LocalDatabase:BackupPath"] = Path.Combine(Path.GetTempPath(), "sgrrhh_backups")
            })
            .Build();

        _pathResolver = new DatabasePathResolver(configuration, NullLogger<DatabasePathResolver>.Instance);
        
        var logger = NullLogger<DapperContext>.Instance;
        Context = new DapperContext(_pathResolver, logger);
        
        // Inicializar esquema de base de datos
        InitializeDatabaseAsync().Wait();
    }

    private async Task InitializeDatabaseAsync()
    {
        using var connection = Context.CreateConnection();
        
        // Aplicar PRAGMAs
        await connection.ExecuteAsync(DatabaseInitializer.Pragmas);
        
        // Crear esquema completo
        await connection.ExecuteAsync(DatabaseInitializer.Schema);
        await connection.ExecuteAsync(DatabaseInitializer.PerformanceIndexes);
        
        // NO aplicamos seed data por defecto para tener control total en los tests
        // Cada test creará sus propios datos
    }

    /// <summary>
    /// Crea el usuario administrador para tests de autenticación
    /// </summary>
    public async Task SeedAdminUserAsync()
    {
        using var connection = Context.CreateConnection();
        
        // Usuario admin con password: Admin123!
        // Hash generado con BCrypt workfactor 11
        await connection.ExecuteAsync(@"
            INSERT OR IGNORE INTO usuarios (id, username, password_hash, nombre_completo, rol, activo)
            VALUES (1, 'admin', '$2a$11$XBN0YhJz5xJ8vE0F8XQKL.rNJE5qxQJvJ8VF3xLZqyK5Z5tHqLXm2', 'Administrador del Sistema', 1, 1)
        ");
    }

    /// <summary>
    /// Limpia todas las tablas para empezar un test limpio
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        using var connection = Context.CreateConnection();
        
        await connection.ExecuteAsync(@"
            DELETE FROM detalles_actividad;
            DELETE FROM registros_diarios;
            DELETE FROM proyectos_empleados;
            DELETE FROM proyectos;
            DELETE FROM actividades;
            DELETE FROM documentos_empleado;
            DELETE FROM vacaciones;
            DELETE FROM permisos;
            DELETE FROM contratos;
            DELETE FROM usuarios;
            DELETE FROM empleados;
            DELETE FROM cargos;
            DELETE FROM departamentos;
            DELETE FROM tipos_permiso;
            DELETE FROM audit_logs;
            DELETE FROM configuracion_sistema;
        ");
    }

    public void Dispose()
    {
        try
        {
            // Cerrar todas las conexiones
            SqliteConnection.ClearAllPools();
            
            // Intentar eliminar el archivo de base de datos temporal
            if (File.Exists(DatabasePath))
            {
                File.Delete(DatabasePath);
            }
            
            // Eliminar archivos auxiliares de SQLite
            var walFile = $"{DatabasePath}-wal";
            var shmFile = $"{DatabasePath}-shm";
            
            if (File.Exists(walFile))
                File.Delete(walFile);
                
            if (File.Exists(shmFile))
                File.Delete(shmFile);
        }
        catch
        {
            // Ignorar errores de limpieza
        }
    }
}

/// <summary>
/// Colección para compartir el fixture entre tests
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
}
