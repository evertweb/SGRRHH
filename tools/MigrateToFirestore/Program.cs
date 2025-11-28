using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Infrastructure.Data;
using SGRRHH.Infrastructure.Firebase;

namespace SGRRHH.Tools.MigrateToFirestore;

/// <summary>
/// Herramienta de migración de datos de SQLite a Firebase Firestore.
/// FASE 2: Migra catálogos (Departamentos, Cargos, Actividades, Proyectos, TiposPermiso, Configuracion)
/// </summary>
class Program
{
    private static FirebaseInitializer? _firebase;
    private static AppDbContext? _sqliteContext;
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    SGRRHH - Herramienta de Migración a Firebase Firestore     ║");
        Console.WriteLine("║                    FASE 2: Catálogos                          ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        try
        {
            // Inicializar conexiones
            await InitializeConnectionsAsync();
            
            // Menú de opciones
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("Seleccione una opción:");
                Console.WriteLine("  1. Migrar TODOS los catálogos (recomendado)");
                Console.WriteLine("  2. Migrar Departamentos");
                Console.WriteLine("  3. Migrar Cargos");
                Console.WriteLine("  4. Migrar Actividades");
                Console.WriteLine("  5. Migrar Proyectos");
                Console.WriteLine("  6. Migrar Tipos de Permiso");
                Console.WriteLine("  7. Migrar Configuración del Sistema");
                Console.WriteLine("  8. Ver estadísticas de migración");
                Console.WriteLine("  9. Limpiar colecciones de Firestore (¡CUIDADO!)");
                Console.WriteLine("  0. Salir");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.Write("Opción: ");
                
                var input = Console.ReadLine()?.Trim();
                
                switch (input)
                {
                    case "1":
                        await MigrateAllCatalogsAsync();
                        break;
                    case "2":
                        await MigrateDepartamentosAsync();
                        break;
                    case "3":
                        await MigrateCargosAsync();
                        break;
                    case "4":
                        await MigrateActividadesAsync();
                        break;
                    case "5":
                        await MigrateProyectosAsync();
                        break;
                    case "6":
                        await MigrateTiposPermisoAsync();
                        break;
                    case "7":
                        await MigrateConfiguracionAsync();
                        break;
                    case "8":
                        await ShowStatisticsAsync();
                        break;
                    case "9":
                        await CleanFirestoreCollectionsAsync();
                        break;
                    case "0":
                        Console.WriteLine("\n¡Hasta luego!");
                        return;
                    default:
                        Console.WriteLine("Opción no válida");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Error fatal: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            Console.ResetColor();
        }
        finally
        {
            _firebase?.Dispose();
            _sqliteContext?.Dispose();
        }
    }
    
    /// <summary>
    /// Inicializa las conexiones a SQLite y Firebase
    /// </summary>
    static async Task InitializeConnectionsAsync()
    {
        Console.WriteLine("🔄 Inicializando conexiones...");
        
        // Leer configuración
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        if (!File.Exists(configPath))
        {
            // Copiar del proyecto WPF
            var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "src", "SGRRHH.WPF", "appsettings.json");
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, configPath);
                Console.WriteLine($"  ✓ Copiado appsettings.json desde {sourcePath}");
            }
            else
            {
                throw new FileNotFoundException("No se encontró appsettings.json");
            }
        }
        
        // Copiar credenciales de Firebase si no existen
        var credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-credentials.json");
        if (!File.Exists(credentialsPath))
        {
            var sourceCredentials = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "src", "SGRRHH.WPF", "firebase-credentials.json");
            if (File.Exists(sourceCredentials))
            {
                File.Copy(sourceCredentials, credentialsPath);
                Console.WriteLine($"  ✓ Copiado firebase-credentials.json");
            }
        }
        
        // Leer configuración JSON manualmente
        var configJson = File.ReadAllText(configPath);
        var firebaseConfig = ParseFirebaseConfig(configJson);
        
        // Inicializar Firebase
        Console.WriteLine("  🔄 Conectando a Firebase...");
        _firebase = new FirebaseInitializer(firebaseConfig);
        var initialized = await _firebase.InitializeAsync();
        
        if (!initialized)
            throw new Exception("No se pudo inicializar Firebase. Verifique las credenciales.");
        
        var connected = await _firebase.TestConnectionAsync();
        if (!connected)
            throw new Exception("No se pudo verificar la conexión con Firebase.");
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ Conectado a Firebase");
        Console.ResetColor();
        
        // Inicializar SQLite
        Console.WriteLine("  🔄 Conectando a SQLite...");
        var dbPath = ParseDatabasePath(configJson);
        
        // Si la ruta es relativa, convertirla
        if (!Path.IsPathRooted(dbPath))
        {
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "src", "SGRRHH.WPF", dbPath);
        }
        
        if (!File.Exists(dbPath))
        {
            // Buscar en rutas alternativas
            var altPaths = new[]
            {
                @"C:\SGRRHH\data\sgrrhh.db",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "sgrrhh.db")
            };
            
            dbPath = altPaths.FirstOrDefault(File.Exists) ?? dbPath;
        }
        
        Console.WriteLine($"  📁 Ruta SQLite: {dbPath}");
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        
        _sqliteContext = new AppDbContext(options);
        await _sqliteContext.Database.EnsureCreatedAsync();
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ Conectado a SQLite");
        Console.ResetColor();
    }
    
    /// <summary>
    /// Migra todos los catálogos
    /// </summary>
    static async Task MigrateAllCatalogsAsync()
    {
        Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         MIGRACIÓN COMPLETA DE CATÁLOGOS                       ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        
        Console.Write("\n⚠️  Esto migrará todos los catálogos a Firestore. ¿Continuar? (s/n): ");
        var confirm = Console.ReadLine()?.Trim().ToLower();
        if (confirm != "s" && confirm != "si")
        {
            Console.WriteLine("Operación cancelada.");
            return;
        }
        
        var startTime = DateTime.Now;
        
        // Orden de migración respetando dependencias
        await MigrateDepartamentosAsync();
        await MigrateCargosAsync();
        await MigrateActividadesAsync();
        await MigrateProyectosAsync();
        await MigrateTiposPermisoAsync();
        await MigrateConfiguracionAsync();
        
        var elapsed = DateTime.Now - startTime;
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Migración completa finalizada en {elapsed.TotalSeconds:F1} segundos");
        Console.ResetColor();
    }
    
    /// <summary>
    /// Migra departamentos
    /// </summary>
    static async Task MigrateDepartamentosAsync()
    {
        Console.WriteLine("\n📦 Migrando Departamentos...");
        
        var departamentos = await _sqliteContext!.Departamentos.ToListAsync();
        var collection = _firebase!.Firestore!.Collection("departamentos");
        var count = 0;
        var errors = 0;
        
        foreach (var dep in departamentos)
        {
            try
            {
                var docId = $"dep_{dep.Id:D4}";
                var data = new Dictionary<string, object?>
                {
                    ["id"] = dep.Id,
                    ["codigo"] = dep.Codigo,
                    ["nombre"] = dep.Nombre,
                    ["descripcion"] = dep.Descripcion,
                    ["jefeId"] = dep.JefeId,
                    ["activo"] = dep.Activo,
                    ["fechaCreacion"] = Timestamp.FromDateTime(dep.FechaCreacion.ToUniversalTime()),
                    ["fechaModificacion"] = dep.FechaModificacion.HasValue 
                        ? Timestamp.FromDateTime(dep.FechaModificacion.Value.ToUniversalTime()) 
                        : null,
                    ["_migratedAt"] = Timestamp.FromDateTime(DateTime.UtcNow),
                    ["_sourceId"] = dep.Id
                };
                
                await collection.Document(docId).SetAsync(data);
                count++;
                Console.Write($"\r  ✓ Migrados: {count}/{departamentos.Count}");
            }
            catch (Exception ex)
            {
                errors++;
                Console.WriteLine($"\n  ❌ Error en departamento {dep.Id}: {ex.Message}");
            }
        }
        
        Console.WriteLine();
        PrintMigrationResult("Departamentos", count, errors, departamentos.Count);
    }
    
    /// <summary>
    /// Migra cargos
    /// </summary>
    static async Task MigrateCargosAsync()
    {
        Console.WriteLine("\n📦 Migrando Cargos...");
        
        var cargos = await _sqliteContext!.Cargos
            .Include(c => c.Departamento)
            .ToListAsync();
        var collection = _firebase!.Firestore!.Collection("cargos");
        var count = 0;
        var errors = 0;
        
        foreach (var cargo in cargos)
        {
            try
            {
                var docId = $"car_{cargo.Id:D4}";
                var data = new Dictionary<string, object?>
                {
                    ["id"] = cargo.Id,
                    ["codigo"] = cargo.Codigo,
                    ["nombre"] = cargo.Nombre,
                    ["descripcion"] = cargo.Descripcion,
                    ["nivel"] = cargo.Nivel,
                    ["departamentoId"] = cargo.DepartamentoId,
                    ["departamentoNombre"] = cargo.Departamento?.Nombre,
                    ["activo"] = cargo.Activo,
                    ["fechaCreacion"] = Timestamp.FromDateTime(cargo.FechaCreacion.ToUniversalTime()),
                    ["fechaModificacion"] = cargo.FechaModificacion.HasValue 
                        ? Timestamp.FromDateTime(cargo.FechaModificacion.Value.ToUniversalTime()) 
                        : null,
                    ["_migratedAt"] = Timestamp.FromDateTime(DateTime.UtcNow),
                    ["_sourceId"] = cargo.Id
                };
                
                await collection.Document(docId).SetAsync(data);
                count++;
                Console.Write($"\r  ✓ Migrados: {count}/{cargos.Count}");
            }
            catch (Exception ex)
            {
                errors++;
                Console.WriteLine($"\n  ❌ Error en cargo {cargo.Id}: {ex.Message}");
            }
        }
        
        Console.WriteLine();
        PrintMigrationResult("Cargos", count, errors, cargos.Count);
    }
    
    /// <summary>
    /// Migra actividades
    /// </summary>
    static async Task MigrateActividadesAsync()
    {
        Console.WriteLine("\n📦 Migrando Actividades...");
        
        var actividades = await _sqliteContext!.Actividades.ToListAsync();
        var collection = _firebase!.Firestore!.Collection("actividades");
        var count = 0;
        var errors = 0;
        
        foreach (var act in actividades)
        {
            try
            {
                var docId = $"act_{act.Id:D4}";
                var data = new Dictionary<string, object?>
                {
                    ["id"] = act.Id,
                    ["codigo"] = act.Codigo,
                    ["nombre"] = act.Nombre,
                    ["descripcion"] = act.Descripcion,
                    ["categoria"] = act.Categoria,
                    ["requiereProyecto"] = act.RequiereProyecto,
                    ["orden"] = act.Orden,
                    ["nombreLower"] = act.Nombre?.ToLowerInvariant(),
                    ["categoriaLower"] = act.Categoria?.ToLowerInvariant(),
                    ["activo"] = act.Activo,
                    ["fechaCreacion"] = Timestamp.FromDateTime(act.FechaCreacion.ToUniversalTime()),
                    ["fechaModificacion"] = act.FechaModificacion.HasValue 
                        ? Timestamp.FromDateTime(act.FechaModificacion.Value.ToUniversalTime()) 
                        : null,
                    ["_migratedAt"] = Timestamp.FromDateTime(DateTime.UtcNow),
                    ["_sourceId"] = act.Id
                };
                
                await collection.Document(docId).SetAsync(data);
                count++;
                Console.Write($"\r  ✓ Migrados: {count}/{actividades.Count}");
            }
            catch (Exception ex)
            {
                errors++;
                Console.WriteLine($"\n  ❌ Error en actividad {act.Id}: {ex.Message}");
            }
        }
        
        Console.WriteLine();
        PrintMigrationResult("Actividades", count, errors, actividades.Count);
    }
    
    /// <summary>
    /// Migra proyectos
    /// </summary>
    static async Task MigrateProyectosAsync()
    {
        Console.WriteLine("\n📦 Migrando Proyectos...");
        
        var proyectos = await _sqliteContext!.Proyectos.ToListAsync();
        var collection = _firebase!.Firestore!.Collection("proyectos");
        var count = 0;
        var errors = 0;
        
        foreach (var proy in proyectos)
        {
            try
            {
                var docId = $"pry_{proy.Id:D4}";
                var data = new Dictionary<string, object?>
                {
                    ["id"] = proy.Id,
                    ["codigo"] = proy.Codigo,
                    ["nombre"] = proy.Nombre,
                    ["descripcion"] = proy.Descripcion,
                    ["cliente"] = proy.Cliente,
                    ["fechaInicio"] = proy.FechaInicio.HasValue 
                        ? Timestamp.FromDateTime(proy.FechaInicio.Value.ToUniversalTime()) 
                        : null,
                    ["fechaFin"] = proy.FechaFin.HasValue 
                        ? Timestamp.FromDateTime(proy.FechaFin.Value.ToUniversalTime()) 
                        : null,
                    ["estado"] = (int)proy.Estado,
                    ["estadoNombre"] = proy.Estado.ToString(),
                    ["nombreLower"] = proy.Nombre?.ToLowerInvariant(),
                    ["clienteLower"] = proy.Cliente?.ToLowerInvariant(),
                    ["activo"] = proy.Activo,
                    ["fechaCreacion"] = Timestamp.FromDateTime(proy.FechaCreacion.ToUniversalTime()),
                    ["fechaModificacion"] = proy.FechaModificacion.HasValue 
                        ? Timestamp.FromDateTime(proy.FechaModificacion.Value.ToUniversalTime()) 
                        : null,
                    ["_migratedAt"] = Timestamp.FromDateTime(DateTime.UtcNow),
                    ["_sourceId"] = proy.Id
                };
                
                await collection.Document(docId).SetAsync(data);
                count++;
                Console.Write($"\r  ✓ Migrados: {count}/{proyectos.Count}");
            }
            catch (Exception ex)
            {
                errors++;
                Console.WriteLine($"\n  ❌ Error en proyecto {proy.Id}: {ex.Message}");
            }
        }
        
        Console.WriteLine();
        PrintMigrationResult("Proyectos", count, errors, proyectos.Count);
    }
    
    /// <summary>
    /// Migra tipos de permiso
    /// </summary>
    static async Task MigrateTiposPermisoAsync()
    {
        Console.WriteLine("\n📦 Migrando Tipos de Permiso...");
        
        var tiposPermiso = await _sqliteContext!.TiposPermiso.ToListAsync();
        var collection = _firebase!.Firestore!.Collection("tipos-permiso");
        var count = 0;
        var errors = 0;
        
        foreach (var tipo in tiposPermiso)
        {
            try
            {
                var docId = $"tp_{tipo.Id:D4}";
                var data = new Dictionary<string, object?>
                {
                    ["id"] = tipo.Id,
                    ["nombre"] = tipo.Nombre,
                    ["descripcion"] = tipo.Descripcion,
                    ["color"] = tipo.Color,
                    ["requiereAprobacion"] = tipo.RequiereAprobacion,
                    ["requiereDocumento"] = tipo.RequiereDocumento,
                    ["diasPorDefecto"] = tipo.DiasPorDefecto,
                    ["esCompensable"] = tipo.EsCompensable,
                    ["nombreLower"] = tipo.Nombre?.ToLowerInvariant(),
                    ["activo"] = tipo.Activo,
                    ["fechaCreacion"] = Timestamp.FromDateTime(tipo.FechaCreacion.ToUniversalTime()),
                    ["fechaModificacion"] = tipo.FechaModificacion.HasValue 
                        ? Timestamp.FromDateTime(tipo.FechaModificacion.Value.ToUniversalTime()) 
                        : null,
                    ["_migratedAt"] = Timestamp.FromDateTime(DateTime.UtcNow),
                    ["_sourceId"] = tipo.Id
                };
                
                await collection.Document(docId).SetAsync(data);
                count++;
                Console.Write($"\r  ✓ Migrados: {count}/{tiposPermiso.Count}");
            }
            catch (Exception ex)
            {
                errors++;
                Console.WriteLine($"\n  ❌ Error en tipo permiso {tipo.Id}: {ex.Message}");
            }
        }
        
        Console.WriteLine();
        PrintMigrationResult("Tipos de Permiso", count, errors, tiposPermiso.Count);
    }
    
    /// <summary>
    /// Migra configuración del sistema
    /// </summary>
    static async Task MigrateConfiguracionAsync()
    {
        Console.WriteLine("\n📦 Migrando Configuración del Sistema...");
        
        var configs = await _sqliteContext!.Configuraciones.ToListAsync();
        var collection = _firebase!.Firestore!.Collection("config");
        var count = 0;
        var errors = 0;
        
        foreach (var config in configs)
        {
            try
            {
                // Usar la clave sanitizada como Document ID
                var docId = config.Clave
                    .Replace("/", "_")
                    .Replace("\\", "_")
                    .Replace(" ", "_");
                    
                var data = new Dictionary<string, object?>
                {
                    ["id"] = config.Id,
                    ["clave"] = config.Clave,
                    ["valor"] = config.Valor,
                    ["descripcion"] = config.Descripcion,
                    ["categoria"] = config.Categoria,
                    ["activo"] = config.Activo,
                    ["fechaCreacion"] = Timestamp.FromDateTime(config.FechaCreacion.ToUniversalTime()),
                    ["fechaModificacion"] = config.FechaModificacion.HasValue 
                        ? Timestamp.FromDateTime(config.FechaModificacion.Value.ToUniversalTime()) 
                        : null,
                    ["_migratedAt"] = Timestamp.FromDateTime(DateTime.UtcNow),
                    ["_sourceId"] = config.Id
                };
                
                await collection.Document(docId).SetAsync(data);
                count++;
                Console.Write($"\r  ✓ Migrados: {count}/{configs.Count}");
            }
            catch (Exception ex)
            {
                errors++;
                Console.WriteLine($"\n  ❌ Error en config {config.Clave}: {ex.Message}");
            }
        }
        
        Console.WriteLine();
        PrintMigrationResult("Configuración", count, errors, configs.Count);
    }
    
    /// <summary>
    /// Muestra estadísticas de migración
    /// </summary>
    static async Task ShowStatisticsAsync()
    {
        Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                 ESTADÍSTICAS DE MIGRACIÓN                     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        
        // SQLite
        Console.WriteLine("\n📊 SQLite (origen):");
        Console.WriteLine($"   Departamentos:     {await _sqliteContext!.Departamentos.CountAsync()}");
        Console.WriteLine($"   Cargos:            {await _sqliteContext.Cargos.CountAsync()}");
        Console.WriteLine($"   Actividades:       {await _sqliteContext.Actividades.CountAsync()}");
        Console.WriteLine($"   Proyectos:         {await _sqliteContext.Proyectos.CountAsync()}");
        Console.WriteLine($"   Tipos Permiso:     {await _sqliteContext.TiposPermiso.CountAsync()}");
        Console.WriteLine($"   Configuraciones:   {await _sqliteContext.Configuraciones.CountAsync()}");
        
        // Firestore
        Console.WriteLine("\n📊 Firestore (destino):");
        var collections = new[] { "departamentos", "cargos", "actividades", "proyectos", "tipos-permiso", "config" };
        
        foreach (var col in collections)
        {
            try
            {
                var snapshot = await _firebase!.Firestore!.Collection(col).GetSnapshotAsync();
                Console.WriteLine($"   {col,-18} {snapshot.Count}");
            }
            catch
            {
                Console.WriteLine($"   {col,-18} Error al leer");
            }
        }
    }
    
    /// <summary>
    /// Limpia las colecciones de Firestore
    /// </summary>
    static async Task CleanFirestoreCollectionsAsync()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n⚠️  ADVERTENCIA: Esta operación eliminará TODOS los datos de los catálogos en Firestore.");
        Console.ResetColor();
        Console.Write("Escriba 'ELIMINAR' para confirmar: ");
        
        var confirm = Console.ReadLine()?.Trim();
        if (confirm != "ELIMINAR")
        {
            Console.WriteLine("Operación cancelada.");
            return;
        }
        
        var collections = new[] { "departamentos", "cargos", "actividades", "proyectos", "tipos-permiso", "config" };
        
        foreach (var colName in collections)
        {
            Console.Write($"  🗑️  Limpiando {colName}... ");
            
            try
            {
                var collection = _firebase!.Firestore!.Collection(colName);
                var snapshot = await collection.GetSnapshotAsync();
                
                // Eliminar en batches de 500 (límite de Firestore)
                var batch = _firebase.Firestore.StartBatch();
                var count = 0;
                
                foreach (var doc in snapshot.Documents)
                {
                    batch.Delete(doc.Reference);
                    count++;
                    
                    if (count % 500 == 0)
                    {
                        await batch.CommitAsync();
                        batch = _firebase.Firestore.StartBatch();
                    }
                }
                
                if (count % 500 != 0)
                {
                    await batch.CommitAsync();
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ {count} documentos eliminados");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
    
    /// <summary>
    /// Imprime el resultado de una migración
    /// </summary>
    static void PrintMigrationResult(string entity, int success, int errors, int total)
    {
        if (errors == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ {entity}: {success}/{total} migrados correctamente");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠️ {entity}: {success}/{total} migrados, {errors} errores");
        }
        Console.ResetColor();
    }
    
    #region Helpers para parsear configuración
    
    static FirebaseConfig ParseFirebaseConfig(string json)
    {
        var config = new FirebaseConfig();
        
        // Parseo simple de JSON (sin dependencias adicionales)
        config.Enabled = GetJsonBool(json, "Enabled", true);
        config.ProjectId = GetJsonString(json, "ProjectId") ?? "";
        config.StorageBucket = GetJsonString(json, "StorageBucket") ?? "";
        config.DatabaseId = GetJsonString(json, "DatabaseId") ?? "(default)";
        config.ApiKey = GetJsonString(json, "ApiKey") ?? "";
        config.CredentialsPath = GetJsonString(json, "CredentialsPath") ?? "firebase-credentials.json";
        config.AuthDomain = GetJsonString(json, "AuthDomain") ?? "";
        
        return config;
    }
    
    static string ParseDatabasePath(string json)
    {
        return GetJsonString(json, "DatabasePath") ?? "data/sgrrhh.db";
    }
    
    static string? GetJsonString(string json, string key)
    {
        var pattern = $"\"{key}\"\\s*:\\s*\"([^\"]+)\"";
        var match = System.Text.RegularExpressions.Regex.Match(json, pattern);
        return match.Success ? match.Groups[1].Value : null;
    }
    
    static bool GetJsonBool(string json, string key, bool defaultValue = false)
    {
        var pattern = $"\"{key}\"\\s*:\\s*(true|false)";
        var match = System.Text.RegularExpressions.Regex.Match(json, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? bool.Parse(match.Groups[1].Value) : defaultValue;
    }
    
    #endregion
}
