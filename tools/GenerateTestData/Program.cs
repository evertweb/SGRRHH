using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using SGRRHH.Infrastructure.Firebase;

namespace SGRRHH.Tools.GenerateTestData;

/// <summary>
/// Herramienta para generar datos de prueba en Firebase Firestore
/// </summary>
class Program
{
    private static FirebaseConfig? _config;
    private static FirestoreDb? _firestore;
    private static Random _random = new Random(42); // Seed fijo para reproducibilidad

    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("   SGRRHH - Generador de Datos de Prueba para Firebase");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();

        // Cargar configuración
        if (!LoadConfiguration())
        {
            Console.WriteLine("❌ Error: No se pudo cargar la configuración");
            return 1;
        }

        // Inicializar Firebase
        if (!await InitializeFirebaseAsync())
        {
            Console.WriteLine("❌ Error: No se pudo conectar a Firebase");
            return 1;
        }

        Console.WriteLine($"✅ Conectado a Firebase: {_config!.ProjectId}");
        Console.WriteLine($"   Base de datos: {_config.DatabaseId}");
        Console.WriteLine();

        // Si se pasan argumentos, ejecutar modo no interactivo
        if (args.Length > 0)
        {
            return await ExecuteCommandAsync(args[0]);
        }

        // Menú principal interactivo
        while (true)
        {
            ShowMenu();
            var option = Console.ReadLine()?.Trim();

            switch (option)
            {
                case "1":
                    await GenerateAllDataAsync();
                    break;
                case "2":
                    await GenerateCatalogosAsync();
                    break;
                case "3":
                    await GenerateEmpleadosAsync();
                    break;
                case "4":
                    await GeneratePermisosVacacionesAsync();
                    break;
                case "5":
                    await GenerateContratosRegistrosDiariosAsync();
                    break;
                case "6":
                    await CleanAllDataAsync();
                    break;
                case "7":
                    await ShowCollectionStatsAsync();
                    break;
                case "0":
                    Console.WriteLine("\n👋 ¡Hasta pronto!");
                    return 0;
                default:
                    Console.WriteLine("\n⚠️ Opción no válida");
                    break;
            }
        }
    }

    static async Task<int> ExecuteCommandAsync(string command)
    {
        try
        {
            switch (command.ToLower())
            {
                case "all":
                case "1":
                    await GenerateAllDataAsync();
                    break;
                case "catalogos":
                case "2":
                    await GenerateCatalogosAsync();
                    break;
                case "empleados":
                case "3":
                    await GenerateEmpleadosAsync();
                    break;
                case "permisos":
                case "4":
                    await GeneratePermisosVacacionesAsync();
                    break;
                case "contratos":
                case "5":
                    await GenerateContratosRegistrosDiariosAsync();
                    break;
                case "clean":
                case "limpiar":
                    await ForceCleanAllDataAsync();
                    break;
                case "stats":
                case "7":
                    await ShowCollectionStatsAsync();
                    break;
                default:
                    Console.WriteLine($"⚠️ Comando no reconocido: {command}");
                    Console.WriteLine("Comandos disponibles: all, catalogos, empleados, permisos, contratos, clean, stats");
                    return 1;
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return 1;
        }
    }

    static async Task ForceCleanAllDataAsync()
    {
        Console.WriteLine("\n🗑️  Limpiando colecciones (modo forzado)...");

        var collections = new[]
        {
            "departamentos", "cargos", "actividades", "proyectos", "tipos-permiso",
            "config", "empleados", "permisos", "vacaciones", "contratos",
            "registros-diarios", "audit-logs"
        };

        foreach (var collectionName in collections)
        {
            Console.Write($"   → {collectionName}... ");
            var deleted = await DeleteCollectionAsync(collectionName);
            Console.WriteLine($"✅ {deleted} eliminados");
        }

        Console.WriteLine("\n✅ Todas las colecciones limpiadas");
    }

    static void ShowMenu()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│           MENÚ PRINCIPAL                │");
        Console.WriteLine("├─────────────────────────────────────────┤");
        Console.WriteLine("│ 1. Generar TODOS los datos de prueba    │");
        Console.WriteLine("│ 2. Solo catálogos (Dep, Cargo, etc.)    │");
        Console.WriteLine("│ 3. Solo empleados                       │");
        Console.WriteLine("│ 4. Solo permisos y vacaciones           │");
        Console.WriteLine("│ 5. Solo contratos y registros diarios   │");
        Console.WriteLine("│ 6. ⚠️  Limpiar TODAS las colecciones    │");
        Console.WriteLine("│ 7. Ver estadísticas de colecciones      │");
        Console.WriteLine("│ 0. Salir                                │");
        Console.WriteLine("└─────────────────────────────────────────┘");
        Console.Write("\n Seleccione una opción: ");
    }

    static bool LoadConfiguration()
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _config = new FirebaseConfig();
            configuration.GetSection("Firebase").Bind(_config);

            if (string.IsNullOrWhiteSpace(_config.ProjectId))
            {
                Console.WriteLine("❌ Error: ProjectId no configurado");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al cargar configuración: {ex.Message}");
            return false;
        }
    }

    static async Task<bool> InitializeFirebaseAsync()
    {
        try
        {
            var credentialsPath = _config!.GetCredentialsFullPath();
            
            if (!File.Exists(credentialsPath))
            {
                Console.WriteLine($"❌ Error: No se encontró el archivo de credenciales: {credentialsPath}");
                Console.WriteLine("   Copie firebase-credentials.json a la carpeta del proyecto");
                return false;
            }

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            var builder = new FirestoreDbBuilder
            {
                ProjectId = _config.ProjectId,
                DatabaseId = string.IsNullOrWhiteSpace(_config.DatabaseId) ? "(default)" : _config.DatabaseId
            };

            _firestore = await builder.BuildAsync();
            
            // Test de conexión
            var testDoc = _firestore.Collection("_test").Document("connection");
            await testDoc.SetAsync(new { timestamp = DateTime.UtcNow });
            await testDoc.DeleteAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al inicializar Firebase: {ex.Message}");
            return false;
        }
    }

    #region Generación de Datos

    static async Task GenerateAllDataAsync()
    {
        Console.WriteLine("\n🚀 Generando TODOS los datos de prueba...\n");

        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 1. Catálogos
        await GenerateCatalogosAsync();

        // 2. Empleados
        await GenerateEmpleadosAsync();

        // 3. Permisos y Vacaciones
        await GeneratePermisosVacacionesAsync();

        // 4. Contratos y Registros Diarios
        await GenerateContratosRegistrosDiariosAsync();

        sw.Stop();
        Console.WriteLine($"\n✅ Todos los datos generados en {sw.Elapsed.TotalSeconds:F1} segundos");
    }

    static async Task GenerateCatalogosAsync()
    {
        Console.WriteLine("\n📁 Generando catálogos...");

        await GenerateDepartamentosAsync();
        await GenerateCargosAsync();
        await GenerateActividadesAsync();
        await GenerateProyectosAsync();
        await GenerateTiposPermisoAsync();
        await GenerateConfiguracionAsync();
    }

    #endregion

    #region Departamentos

    static async Task GenerateDepartamentosAsync()
    {
        Console.Write("   → Departamentos... ");
        
        var departamentos = new List<Dictionary<string, object>>
        {
            new()
            {
                ["codigo"] = "DEP-001",
                ["nombre"] = "Gerencia General",
                ["descripcion"] = "Dirección general y toma de decisiones estratégicas",
                ["jefeId"] = null!,
                ["activo"] = true,
                ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["fechaModificacion"] = null!
            },
            new()
            {
                ["codigo"] = "DEP-002",
                ["nombre"] = "Ingeniería",
                ["descripcion"] = "Desarrollo de proyectos técnicos y ambientales",
                ["jefeId"] = null!,
                ["activo"] = true,
                ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["fechaModificacion"] = null!
            },
            new()
            {
                ["codigo"] = "DEP-003",
                ["nombre"] = "Operaciones",
                ["descripcion"] = "Ejecución de labores de campo y operativas",
                ["jefeId"] = null!,
                ["activo"] = true,
                ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["fechaModificacion"] = null!
            },
            new()
            {
                ["codigo"] = "DEP-004",
                ["nombre"] = "Administración",
                ["descripcion"] = "Gestión administrativa, financiera y de recursos humanos",
                ["jefeId"] = null!,
                ["activo"] = true,
                ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["fechaModificacion"] = null!
            },
            new()
            {
                ["codigo"] = "DEP-005",
                ["nombre"] = "Vivero",
                ["descripcion"] = "Producción y mantenimiento de material vegetal",
                ["jefeId"] = null!,
                ["activo"] = true,
                ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["fechaModificacion"] = null!
            }
        };

        var collection = _firestore!.Collection("departamentos");
        var batch = _firestore.StartBatch();
        int count = 0;

        foreach (var dep in departamentos)
        {
            count++;
            var docRef = collection.Document($"dep_{count:D4}");
            batch.Set(docRef, dep);
        }

        await batch.CommitAsync();
        Console.WriteLine($"✅ {count} creados");
    }

    #endregion

    #region Cargos

    static async Task GenerateCargosAsync()
    {
        Console.Write("   → Cargos... ");

        var cargos = new List<Dictionary<string, object>>
        {
            // Gerencia (DEP-001)
            new() { ["codigo"] = "CAR-001", ["nombre"] = "Gerente General", ["descripcion"] = "Dirección y representación legal de la empresa", ["nivel"] = 1, ["departamentoId"] = "dep_0001", ["departamentoNombre"] = "Gerencia General" },
            
            // Ingeniería (DEP-002)
            new() { ["codigo"] = "CAR-002", ["nombre"] = "Ingeniero Forestal", ["descripcion"] = "Planificación y supervisión de proyectos forestales", ["nivel"] = 2, ["departamentoId"] = "dep_0002", ["departamentoNombre"] = "Ingeniería" },
            new() { ["codigo"] = "CAR-003", ["nombre"] = "Ingeniero Ambiental", ["descripcion"] = "Gestión ambiental y estudios de impacto", ["nivel"] = 2, ["departamentoId"] = "dep_0002", ["departamentoNombre"] = "Ingeniería" },
            new() { ["codigo"] = "CAR-004", ["nombre"] = "Técnico Forestal", ["descripcion"] = "Apoyo técnico en labores forestales", ["nivel"] = 3, ["departamentoId"] = "dep_0002", ["departamentoNombre"] = "Ingeniería" },
            
            // Operaciones (DEP-003)
            new() { ["codigo"] = "CAR-005", ["nombre"] = "Supervisor de Campo", ["descripcion"] = "Supervisión de cuadrillas y labores operativas", ["nivel"] = 3, ["departamentoId"] = "dep_0003", ["departamentoNombre"] = "Operaciones" },
            new() { ["codigo"] = "CAR-006", ["nombre"] = "Operario Forestal", ["descripcion"] = "Ejecución de labores de siembra, mantenimiento y cosecha", ["nivel"] = 4, ["departamentoId"] = "dep_0003", ["departamentoNombre"] = "Operaciones" },
            new() { ["codigo"] = "CAR-007", ["nombre"] = "Conductor", ["descripcion"] = "Transporte de personal y materiales", ["nivel"] = 4, ["departamentoId"] = "dep_0003", ["departamentoNombre"] = "Operaciones" },
            new() { ["codigo"] = "CAR-008", ["nombre"] = "Motosierrista", ["descripcion"] = "Operación de motosierras para aprovechamiento forestal", ["nivel"] = 4, ["departamentoId"] = "dep_0003", ["departamentoNombre"] = "Operaciones" },
            
            // Administración (DEP-004)
            new() { ["codigo"] = "CAR-009", ["nombre"] = "Secretaria Administrativa", ["descripcion"] = "Gestión documental y apoyo administrativo", ["nivel"] = 3, ["departamentoId"] = "dep_0004", ["departamentoNombre"] = "Administración" },
            new() { ["codigo"] = "CAR-010", ["nombre"] = "Auxiliar Contable", ["descripcion"] = "Apoyo en labores contables y financieras", ["nivel"] = 4, ["departamentoId"] = "dep_0004", ["departamentoNombre"] = "Administración" },
            
            // Vivero (DEP-005)
            new() { ["codigo"] = "CAR-011", ["nombre"] = "Jefe de Vivero", ["descripcion"] = "Dirección del área de vivero", ["nivel"] = 3, ["departamentoId"] = "dep_0005", ["departamentoNombre"] = "Vivero" },
            new() { ["codigo"] = "CAR-012", ["nombre"] = "Operario de Vivero", ["descripcion"] = "Labores de propagación y mantenimiento de plántulas", ["nivel"] = 4, ["departamentoId"] = "dep_0005", ["departamentoNombre"] = "Vivero" }
        };

        var collection = _firestore!.Collection("cargos");
        var batch = _firestore.StartBatch();
        int count = 0;

        foreach (var cargo in cargos)
        {
            count++;
            cargo["activo"] = true;
            cargo["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow);
            cargo["fechaModificacion"] = null!;
            
            var docRef = collection.Document($"car_{count:D4}");
            batch.Set(docRef, cargo);
        }

        await batch.CommitAsync();
        Console.WriteLine($"✅ {count} creados");
    }

    #endregion

    #region Actividades

    static async Task GenerateActividadesAsync()
    {
        Console.Write("   → Actividades... ");

        var actividades = new List<Dictionary<string, object>>
        {
            // Actividades de campo
            new() { ["codigo"] = "ACT-001", ["nombre"] = "Preparación de terreno", ["descripcion"] = "Limpieza y adecuación del terreno para siembra", ["categoria"] = "Campo", ["requiereProyecto"] = true, ["orden"] = 1 },
            new() { ["codigo"] = "ACT-002", ["nombre"] = "Siembra", ["descripcion"] = "Establecimiento de plántulas en campo", ["categoria"] = "Campo", ["requiereProyecto"] = true, ["orden"] = 2 },
            new() { ["codigo"] = "ACT-003", ["nombre"] = "Fertilización", ["descripcion"] = "Aplicación de fertilizantes", ["categoria"] = "Campo", ["requiereProyecto"] = true, ["orden"] = 3 },
            new() { ["codigo"] = "ACT-004", ["nombre"] = "Control de malezas", ["descripcion"] = "Plateo, guadaña y control químico", ["categoria"] = "Campo", ["requiereProyecto"] = true, ["orden"] = 4 },
            new() { ["codigo"] = "ACT-005", ["nombre"] = "Control fitosanitario", ["descripcion"] = "Control de plagas y enfermedades", ["categoria"] = "Campo", ["requiereProyecto"] = true, ["orden"] = 5 },
            new() { ["codigo"] = "ACT-006", ["nombre"] = "Poda y raleo", ["descripcion"] = "Podas de formación y raleos", ["categoria"] = "Campo", ["requiereProyecto"] = true, ["orden"] = 6 },
            new() { ["codigo"] = "ACT-007", ["nombre"] = "Cosecha", ["descripcion"] = "Aprovechamiento forestal", ["categoria"] = "Campo", ["requiereProyecto"] = true, ["orden"] = 7 },
            
            // Actividades de vivero
            new() { ["codigo"] = "ACT-008", ["nombre"] = "Preparación de sustrato", ["descripcion"] = "Mezcla y preparación de sustrato para siembra", ["categoria"] = "Vivero", ["requiereProyecto"] = false, ["orden"] = 10 },
            new() { ["codigo"] = "ACT-009", ["nombre"] = "Siembra en vivero", ["descripcion"] = "Siembra de semillas en bandejas o bolsas", ["categoria"] = "Vivero", ["requiereProyecto"] = false, ["orden"] = 11 },
            new() { ["codigo"] = "ACT-010", ["nombre"] = "Riego", ["descripcion"] = "Riego de plántulas en vivero", ["categoria"] = "Vivero", ["requiereProyecto"] = false, ["orden"] = 12 },
            new() { ["codigo"] = "ACT-011", ["nombre"] = "Mantenimiento de vivero", ["descripcion"] = "Limpieza, clasificación y organización", ["categoria"] = "Vivero", ["requiereProyecto"] = false, ["orden"] = 13 },
            
            // Actividades administrativas
            new() { ["codigo"] = "ACT-012", ["nombre"] = "Reunión", ["descripcion"] = "Reuniones de trabajo y capacitaciones", ["categoria"] = "Administrativo", ["requiereProyecto"] = false, ["orden"] = 20 },
            new() { ["codigo"] = "ACT-013", ["nombre"] = "Capacitación", ["descripcion"] = "Capacitaciones y entrenamientos", ["categoria"] = "Administrativo", ["requiereProyecto"] = false, ["orden"] = 21 },
            new() { ["codigo"] = "ACT-014", ["nombre"] = "Gestión documental", ["descripcion"] = "Elaboración de informes y documentos", ["categoria"] = "Administrativo", ["requiereProyecto"] = false, ["orden"] = 22 },
            new() { ["codigo"] = "ACT-015", ["nombre"] = "Supervisión", ["descripcion"] = "Supervisión de labores y personal", ["categoria"] = "Administrativo", ["requiereProyecto"] = true, ["orden"] = 23 },
            
            // Transporte
            new() { ["codigo"] = "ACT-016", ["nombre"] = "Transporte de personal", ["descripcion"] = "Traslado de trabajadores a campo", ["categoria"] = "Transporte", ["requiereProyecto"] = true, ["orden"] = 30 },
            new() { ["codigo"] = "ACT-017", ["nombre"] = "Transporte de insumos", ["descripcion"] = "Traslado de materiales e insumos", ["categoria"] = "Transporte", ["requiereProyecto"] = true, ["orden"] = 31 },
            new() { ["codigo"] = "ACT-018", ["nombre"] = "Transporte de madera", ["descripcion"] = "Traslado de productos forestales", ["categoria"] = "Transporte", ["requiereProyecto"] = true, ["orden"] = 32 }
        };

        var collection = _firestore!.Collection("actividades");
        var batch = _firestore.StartBatch();
        int count = 0;

        foreach (var actividad in actividades)
        {
            count++;
            actividad["activo"] = true;
            actividad["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow);
            actividad["fechaModificacion"] = null!;
            
            var docRef = collection.Document($"act_{count:D4}");
            batch.Set(docRef, actividad);
        }

        await batch.CommitAsync();
        Console.WriteLine($"✅ {count} creadas");
    }

    #endregion

    #region Proyectos

    static async Task GenerateProyectosAsync()
    {
        Console.Write("   → Proyectos... ");

        var proyectos = new List<Dictionary<string, object>>
        {
            new()
            {
                ["codigo"] = "PROY-001",
                ["nombre"] = "Reforestación Finca El Roble",
                ["descripcion"] = "Establecimiento de 50 hectáreas de Eucalyptus grandis",
                ["cliente"] = "Finca El Roble S.A.S.",
                ["fechaInicio"] = Timestamp.FromDateTime(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
                ["fechaFin"] = Timestamp.FromDateTime(new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)),
                ["estado"] = "Activo"
            },
            new()
            {
                ["codigo"] = "PROY-002",
                ["nombre"] = "Mantenimiento Predio La Esperanza",
                ["descripcion"] = "Mantenimiento de plantación de 80 hectáreas de Pino",
                ["cliente"] = "Inversiones La Esperanza",
                ["fechaInicio"] = Timestamp.FromDateTime(new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
                ["fechaFin"] = Timestamp.FromDateTime(new DateTime(2025, 11, 30, 0, 0, 0, DateTimeKind.Utc)),
                ["estado"] = "Activo"
            },
            new()
            {
                ["codigo"] = "PROY-003",
                ["nombre"] = "Cosecha Finca San José",
                ["descripcion"] = "Aprovechamiento forestal de 30 hectáreas de Eucalipto",
                ["cliente"] = "Agropecuaria San José",
                ["fechaInicio"] = Timestamp.FromDateTime(new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                ["fechaFin"] = Timestamp.FromDateTime(new DateTime(2025, 6, 30, 0, 0, 0, DateTimeKind.Utc)),
                ["estado"] = "Activo"
            },
            new()
            {
                ["codigo"] = "PROY-004",
                ["nombre"] = "Siembra Hacienda El Porvenir",
                ["descripcion"] = "Establecimiento de 100 hectáreas de sistema silvopastoril",
                ["cliente"] = "Hacienda El Porvenir Ltda.",
                ["fechaInicio"] = Timestamp.FromDateTime(new DateTime(2025, 4, 15, 0, 0, 0, DateTimeKind.Utc)),
                ["fechaFin"] = Timestamp.FromDateTime(new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc)),
                ["estado"] = "Activo"
            },
            new()
            {
                ["codigo"] = "PROY-005",
                ["nombre"] = "Vivero Comercial 2025",
                ["descripcion"] = "Producción de 500.000 plántulas para comercialización",
                ["cliente"] = "Interno",
                ["fechaInicio"] = Timestamp.FromDateTime(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                ["fechaFin"] = Timestamp.FromDateTime(new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)),
                ["estado"] = "Activo"
            },
            new()
            {
                ["codigo"] = "PROY-006",
                ["nombre"] = "Inventario Forestal Regional",
                ["descripcion"] = "Inventario de 500 hectáreas para empresa maderera",
                ["cliente"] = "Maderas del Valle S.A.",
                ["fechaInicio"] = Timestamp.FromDateTime(new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc)),
                ["fechaFin"] = Timestamp.FromDateTime(new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc)),
                ["estado"] = "Finalizado"
            }
        };

        var collection = _firestore!.Collection("proyectos");
        var batch = _firestore.StartBatch();
        int count = 0;

        foreach (var proyecto in proyectos)
        {
            count++;
            proyecto["activo"] = true;
            proyecto["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow);
            proyecto["fechaModificacion"] = null!;
            
            var docRef = collection.Document($"proy_{count:D4}");
            batch.Set(docRef, proyecto);
        }

        await batch.CommitAsync();
        Console.WriteLine($"✅ {count} creados");
    }

    #endregion

    #region Tipos de Permiso

    static async Task GenerateTiposPermisoAsync()
    {
        Console.Write("   → Tipos de permiso... ");

        var tipos = new List<Dictionary<string, object>>
        {
            new() { ["nombre"] = "Cita médica", ["descripcion"] = "Permiso para asistir a cita médica programada", ["color"] = "#1E88E5", ["requiereAprobacion"] = true, ["requiereDocumento"] = true, ["diasPorDefecto"] = 1, ["esCompensable"] = false },
            new() { ["nombre"] = "Calamidad doméstica", ["descripcion"] = "Permiso por emergencia familiar o del hogar", ["color"] = "#E53935", ["requiereAprobacion"] = true, ["requiereDocumento"] = false, ["diasPorDefecto"] = 1, ["esCompensable"] = false },
            new() { ["nombre"] = "Diligencia personal", ["descripcion"] = "Permiso para trámites personales (banco, notaría, etc.)", ["color"] = "#FB8C00", ["requiereAprobacion"] = true, ["requiereDocumento"] = false, ["diasPorDefecto"] = 1, ["esCompensable"] = true },
            new() { ["nombre"] = "Licencia de maternidad", ["descripcion"] = "Licencia legal por maternidad (18 semanas)", ["color"] = "#EC407A", ["requiereAprobacion"] = false, ["requiereDocumento"] = true, ["diasPorDefecto"] = 126, ["esCompensable"] = false },
            new() { ["nombre"] = "Licencia de paternidad", ["descripcion"] = "Licencia legal por paternidad (2 semanas)", ["color"] = "#7E57C2", ["requiereAprobacion"] = false, ["requiereDocumento"] = true, ["diasPorDefecto"] = 14, ["esCompensable"] = false },
            new() { ["nombre"] = "Licencia de luto", ["descripcion"] = "Licencia por fallecimiento de familiar cercano", ["color"] = "#546E7A", ["requiereAprobacion"] = false, ["requiereDocumento"] = true, ["diasPorDefecto"] = 5, ["esCompensable"] = false },
            new() { ["nombre"] = "Licencia no remunerada", ["descripcion"] = "Licencia sin goce de salario acordada", ["color"] = "#78909C", ["requiereAprobacion"] = true, ["requiereDocumento"] = false, ["diasPorDefecto"] = 1, ["esCompensable"] = false },
            new() { ["nombre"] = "Incapacidad", ["descripcion"] = "Incapacidad médica certificada por EPS", ["color"] = "#43A047", ["requiereAprobacion"] = false, ["requiereDocumento"] = true, ["diasPorDefecto"] = 3, ["esCompensable"] = false },
            new() { ["nombre"] = "Capacitación", ["descripcion"] = "Permiso para asistir a capacitación o curso", ["color"] = "#00ACC1", ["requiereAprobacion"] = true, ["requiereDocumento"] = false, ["diasPorDefecto"] = 1, ["esCompensable"] = false },
            new() { ["nombre"] = "Compensatorio", ["descripcion"] = "Día compensatorio por tiempo extra trabajado", ["color"] = "#8BC34A", ["requiereAprobacion"] = true, ["requiereDocumento"] = false, ["diasPorDefecto"] = 1, ["esCompensable"] = false }
        };

        var collection = _firestore!.Collection("tipos-permiso");
        var batch = _firestore.StartBatch();
        int count = 0;

        foreach (var tipo in tipos)
        {
            count++;
            tipo["activo"] = true;
            tipo["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow);
            tipo["fechaModificacion"] = null!;
            
            var docRef = collection.Document($"tipo_{count:D4}");
            batch.Set(docRef, tipo);
        }

        await batch.CommitAsync();
        Console.WriteLine($"✅ {count} creados");
    }

    #endregion

    #region Configuración del Sistema

    static async Task GenerateConfiguracionAsync()
    {
        Console.Write("   → Configuración del sistema... ");

        var configs = new Dictionary<string, Dictionary<string, object>>
        {
            ["empresa_nombre"] = new() { ["valor"] = "Forestech S.A.S.", ["descripcion"] = "Nombre de la empresa", ["tipo"] = "string" },
            ["empresa_nit"] = new() { ["valor"] = "900.123.456-7", ["descripcion"] = "NIT de la empresa", ["tipo"] = "string" },
            ["empresa_direccion"] = new() { ["valor"] = "Calle 100 #15-20, Bogotá D.C.", ["descripcion"] = "Dirección de la empresa", ["tipo"] = "string" },
            ["empresa_telefono"] = new() { ["valor"] = "+57 601 234 5678", ["descripcion"] = "Teléfono de la empresa", ["tipo"] = "string" },
            ["empresa_email"] = new() { ["valor"] = "info@forestech.com.co", ["descripcion"] = "Email de contacto", ["tipo"] = "string" },
            ["vacaciones_dias_anuales"] = new() { ["valor"] = "15", ["descripcion"] = "Días de vacaciones por año", ["tipo"] = "int" },
            ["jornada_hora_entrada"] = new() { ["valor"] = "07:00", ["descripcion"] = "Hora de entrada por defecto", ["tipo"] = "time" },
            ["jornada_hora_salida"] = new() { ["valor"] = "17:00", ["descripcion"] = "Hora de salida por defecto", ["tipo"] = "time" },
            ["jornada_horas_dia"] = new() { ["valor"] = "9", ["descripcion"] = "Horas laborales por día", ["tipo"] = "int" },
            ["permisos_requiere_aprobacion"] = new() { ["valor"] = "true", ["descripcion"] = "Los permisos requieren aprobación", ["tipo"] = "bool" }
        };

        var collection = _firestore!.Collection("config");
        var batch = _firestore.StartBatch();

        foreach (var (key, config) in configs)
        {
            config["clave"] = key;
            config["activo"] = true;
            config["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow);
            config["fechaModificacion"] = null!;
            
            var docRef = collection.Document(key);
            batch.Set(docRef, config);
        }

        await batch.CommitAsync();
        Console.WriteLine($"✅ {configs.Count} configuraciones");
    }

    #endregion

    #region Empleados

    // Datos para generar nombres colombianos realistas
    private static readonly string[] NombresMasculinos = {
        "Juan Carlos", "Andrés Felipe", "Carlos Alberto", "José Luis", "Miguel Ángel",
        "Luis Fernando", "Diego Alejandro", "Jorge Enrique", "Pedro Pablo", "Rafael Antonio",
        "Óscar Eduardo", "Fabián Mauricio", "Héctor Julián", "Nelson Ricardo", "Germán Darío",
        "William Alexánder", "Jhon Jairo", "Edwin Camilo", "Freddy Hernán", "Gustavo Adolfo"
    };

    private static readonly string[] NombresFemeninos = {
        "María Fernanda", "Ana María", "Laura Catalina", "Diana Carolina", "Sandra Milena",
        "Patricia Elena", "Claudia Marcela", "Mónica Liliana", "Luz Dary", "Adriana Lucía",
        "Paola Andrea", "Natalia Isabel", "Lorena Estefanía", "Carolina Andrea", "Viviana del Pilar",
        "Jenny Alexandra", "Marisol", "Yolanda Patricia", "Gloria Esperanza", "Martha Cecilia"
    };

    private static readonly string[] Apellidos = {
        "García", "Rodríguez", "Martínez", "López", "González", "Hernández", "Pérez", "Sánchez",
        "Ramírez", "Torres", "Flores", "Rivera", "Gómez", "Díaz", "Cruz", "Reyes", "Morales",
        "Jiménez", "Ruiz", "Vargas", "Castro", "Rojas", "Ortiz", "Silva", "Medina", "Gutiérrez",
        "Ramos", "Mendoza", "Castillo", "Santos", "Romero", "Herrera", "Aguilar", "Muñoz", "Vega",
        "Cortés", "Guerrero", "Parra", "Acosta", "Cardona", "Ospina", "Valencia", "Arango", "Mejía"
    };

    private static readonly string[] Direcciones = {
        "Calle {0} #{1}-{2}, Barrio {3}",
        "Carrera {0} #{1}-{2}, {3}",
        "Diagonal {0} #{1}-{2}, {3}",
        "Transversal {0} #{1}-{2}, {3}",
        "Avenida {0} #{1}-{2}, {3}"
    };

    private static readonly string[] Barrios = {
        "Centro", "El Poblado", "La Castellana", "San Fernando", "El Jardín",
        "La Aurora", "Santa Rosa", "El Prado", "Villa María", "Los Álamos",
        "San Antonio", "La Esperanza", "El Bosque", "Santa Lucía", "La Victoria"
    };

    static async Task GenerateEmpleadosAsync()
    {
        Console.WriteLine("\n👤 Generando empleados...");

        // Lista de empleados a generar con datos específicos para cada cargo
        var empleadosData = new List<(string cargoId, string departamentoId, string cargoNombre, string departamentoNombre, bool esMujer)>
        {
            // Gerencia
            ("car_0001", "dep_0001", "Gerente General", "Gerencia General", false),
            
            // Ingeniería - 3 personas
            ("car_0002", "dep_0002", "Ingeniero Forestal", "Ingeniería", true),  // Ingeniera
            ("car_0003", "dep_0002", "Ingeniero Ambiental", "Ingeniería", false),
            ("car_0004", "dep_0002", "Técnico Forestal", "Ingeniería", false),
            
            // Administración - 2 personas
            ("car_0009", "dep_0004", "Secretaria Administrativa", "Administración", true), // Secretaria
            ("car_0010", "dep_0004", "Auxiliar Contable", "Administración", true),
            
            // Operaciones - 10 personas
            ("car_0005", "dep_0003", "Supervisor de Campo", "Operaciones", false),
            ("car_0006", "dep_0003", "Operario Forestal", "Operaciones", false),
            ("car_0006", "dep_0003", "Operario Forestal", "Operaciones", false),
            ("car_0006", "dep_0003", "Operario Forestal", "Operaciones", false),
            ("car_0006", "dep_0003", "Operario Forestal", "Operaciones", false),
            ("car_0006", "dep_0003", "Operario Forestal", "Operaciones", true),
            ("car_0007", "dep_0003", "Conductor", "Operaciones", false),
            ("car_0007", "dep_0003", "Conductor", "Operaciones", false),
            ("car_0008", "dep_0003", "Motosierrista", "Operaciones", false),
            ("car_0008", "dep_0003", "Motosierrista", "Operaciones", false),
            
            // Vivero - 4 personas
            ("car_0011", "dep_0005", "Jefe de Vivero", "Vivero", false),
            ("car_0012", "dep_0005", "Operario de Vivero", "Vivero", true),
            ("car_0012", "dep_0005", "Operario de Vivero", "Vivero", false),
            ("car_0012", "dep_0005", "Operario de Vivero", "Vivero", true)
        };

        var collection = _firestore!.Collection("empleados");
        var empleadosCreados = new List<(string id, string nombre, string cargoId, string departamentoId)>();
        int count = 0;

        // Primero crear el Gerente (será supervisor de los demás)
        string gerenteId = "";

        foreach (var (cargoId, departamentoId, cargoNombre, departamentoNombre, esMujer) in empleadosData)
        {
            count++;
            var empId = $"emp_{count:D4}";
            
            var nombres = esMujer ? NombresFemeninos : NombresMasculinos;
            var nombre = nombres[_random.Next(nombres.Length)];
            var apellido1 = Apellidos[_random.Next(Apellidos.Length)];
            var apellido2 = Apellidos[_random.Next(Apellidos.Length)];
            var apellidos = $"{apellido1} {apellido2}";
            var nombreCompleto = $"{nombre} {apellidos}";

            // Generar cédula colombiana (10 dígitos)
            var cedula = $"{_random.Next(10, 99)}{_random.Next(100000, 999999)}{_random.Next(100, 999)}";

            // Generar fecha de nacimiento (25-55 años)
            var edadBase = count == 1 ? 45 : _random.Next(25, 55); // Gerente más mayor
            var fechaNac = DateTime.Today.AddYears(-edadBase).AddDays(-_random.Next(0, 365));

            // Generar fecha de ingreso (1-10 años de antigüedad, gerente más antiguo)
            var antiguedad = count == 1 ? _random.Next(8, 12) : _random.Next(1, 8);
            var fechaIngreso = DateTime.Today.AddYears(-antiguedad).AddDays(-_random.Next(0, 365));

            // Generar dirección
            var formatoDireccion = Direcciones[_random.Next(Direcciones.Length)];
            var barrio = Barrios[_random.Next(Barrios.Length)];
            var direccion = string.Format(formatoDireccion, 
                _random.Next(1, 100), 
                _random.Next(1, 50), 
                _random.Next(1, 99),
                barrio);

            // Generar teléfono celular colombiano
            var telefono = $"+57 3{_random.Next(10, 25)}{_random.Next(1000000, 9999999)}";
            var telEmergencia = $"+57 3{_random.Next(10, 25)}{_random.Next(1000000, 9999999)}";

            // Generar email
            var emailBase = $"{nombre.Split(' ')[0].ToLower()}.{apellido1.ToLower()}";
            emailBase = RemoverAcentos(emailBase);
            var email = $"{emailBase}@forestech.com.co";

            // Estado civil
            var estadosCiviles = new[] { "Soltero", "Casado", "UnionLibre", "Divorciado" };
            var estadoCivil = estadosCiviles[_random.Next(estadosCiviles.Length)];

            // Tipo de contrato
            var tiposContrato = count <= 6 
                ? new[] { "Indefinido" } // Administrativos e ingenieros
                : new[] { "Indefinido", "Fijo", "ObraLabor" }; // Operativos
            var tipoContrato = tiposContrato[_random.Next(tiposContrato.Length)];

            // Supervisor (el gerente supervisa a jefes, jefes supervisan operarios)
            string? supervisorId = null;
            string? supervisorNombre = null;
            
            if (count > 1) // Todos excepto el gerente
            {
                if (cargoId == "car_0002" || cargoId == "car_0003" || cargoId == "car_0005" || 
                    cargoId == "car_0009" || cargoId == "car_0011") // Jefes y profesionales
                {
                    supervisorId = gerenteId;
                    supervisorNombre = empleadosCreados[0].nombre;
                }
                else // Operarios supervisados por sus jefes de área
                {
                    var jefe = empleadosCreados.FirstOrDefault(e => 
                        (e.departamentoId == departamentoId && 
                         (e.cargoId == "car_0002" || e.cargoId == "car_0005" || e.cargoId == "car_0011")));
                    
                    if (jefe.id != null)
                    {
                        supervisorId = jefe.id;
                        supervisorNombre = jefe.nombre;
                    }
                    else
                    {
                        supervisorId = gerenteId;
                        supervisorNombre = empleadosCreados[0].nombre;
                    }
                }
            }

            var empleado = new Dictionary<string, object>
            {
                ["codigo"] = $"EMP-{count:D3}",
                ["cedula"] = cedula,
                ["nombres"] = nombre,
                ["apellidos"] = apellidos,
                ["nombreCompleto"] = nombreCompleto,
                ["fechaNacimiento"] = Timestamp.FromDateTime(DateTime.SpecifyKind(fechaNac, DateTimeKind.Utc)),
                ["genero"] = esMujer ? "Femenino" : "Masculino",
                ["estadoCivil"] = estadoCivil,
                ["direccion"] = direccion,
                ["telefono"] = telefono,
                ["telefonoEmergencia"] = telEmergencia,
                ["contactoEmergencia"] = $"{NombresFemeninos[_random.Next(NombresFemeninos.Length)].Split(' ')[0]} {apellido1}",
                ["email"] = email,
                ["fotoUrl"] = null!,
                ["fechaIngreso"] = Timestamp.FromDateTime(DateTime.SpecifyKind(fechaIngreso, DateTimeKind.Utc)),
                ["fechaRetiro"] = null!,
                ["estado"] = "Activo",
                ["tipoContrato"] = tipoContrato,
                ["cargoId"] = cargoId,
                ["cargoNombre"] = cargoNombre,
                ["departamentoId"] = departamentoId,
                ["departamentoNombre"] = departamentoNombre,
                ["supervisorId"] = supervisorId!,
                ["supervisorNombre"] = supervisorNombre!,
                ["observaciones"] = null!,
                ["creadoPorId"] = null!,
                ["aprobadoPorId"] = null!,
                ["fechaSolicitud"] = Timestamp.FromDateTime(DateTime.SpecifyKind(fechaIngreso.AddDays(-15), DateTimeKind.Utc)),
                ["fechaAprobacion"] = Timestamp.FromDateTime(DateTime.SpecifyKind(fechaIngreso.AddDays(-7), DateTimeKind.Utc)),
                ["motivoRechazo"] = null!,
                ["activo"] = true,
                ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["fechaModificacion"] = null!
            };

            var docRef = collection.Document(empId);
            await docRef.SetAsync(empleado);

            empleadosCreados.Add((empId, nombreCompleto, cargoId, departamentoId));
            
            if (count == 1) gerenteId = empId;

            Console.WriteLine($"   ✅ {count:D2}. {nombreCompleto,-35} ({cargoNombre})");
        }

        // Guardar IDs de empleados para usar en otras funciones
        _empleadosGenerados = empleadosCreados;

        Console.WriteLine($"\n   ✅ {count} empleados creados");
    }

    // Lista de empleados generados para usar en otras funciones
    private static List<(string id, string nombre, string cargoId, string departamentoId)> _empleadosGenerados = new();

    static string RemoverAcentos(string texto)
    {
        return texto
            .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
            .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
            .Replace("ñ", "n").Replace("Ñ", "N")
            .Replace("ü", "u").Replace("Ü", "U");
    }

    #endregion

    #region Permisos y Vacaciones

    static async Task GeneratePermisosVacacionesAsync()
    {
        Console.WriteLine("\n📋 Generando permisos y vacaciones...");

        // Cargar empleados si no están en memoria
        if (_empleadosGenerados.Count == 0)
        {
            await LoadEmpleadosAsync();
        }

        if (_empleadosGenerados.Count == 0)
        {
            Console.WriteLine("   ⚠️  No hay empleados. Primero genere los empleados.");
            return;
        }

        await GeneratePermisosAsync();
        await GenerateVacacionesAsync();
    }

    static async Task LoadEmpleadosAsync()
    {
        Console.Write("   → Cargando empleados existentes... ");
        
        var snapshot = await _firestore!.Collection("empleados").GetSnapshotAsync();
        _empleadosGenerados = snapshot.Documents
            .Select(d => (
                id: d.Id,
                nombre: d.GetValue<string>("nombreCompleto"),
                cargoId: d.GetValue<string>("cargoId"),
                departamentoId: d.GetValue<string>("departamentoId")
            ))
            .ToList();
        
        Console.WriteLine($"✅ {_empleadosGenerados.Count} encontrados");
    }

    static async Task GeneratePermisosAsync()
    {
        Console.Write("   → Permisos... ");

        // Motivos de permiso por tipo
        var motivosPorTipo = new Dictionary<string, string[]>
        {
            ["tipo_0001"] = new[] { "Cita médica general", "Control médico", "Cita odontológica", "Cita con especialista", "Exámenes de laboratorio" },
            ["tipo_0002"] = new[] { "Enfermedad de familiar", "Daño en vivienda", "Accidente de familiar", "Emergencia familiar" },
            ["tipo_0003"] = new[] { "Trámite bancario", "Diligencia en notaría", "Trámite de documentos", "Cita en entidad gubernamental" },
            ["tipo_0008"] = new[] { "Incapacidad por gripa", "Incapacidad por lesión", "Incapacidad post-quirúrgica" },
            ["tipo_0009"] = new[] { "Curso de seguridad industrial", "Capacitación en primeros auxilios", "Taller de manejo de químicos" },
            ["tipo_0010"] = new[] { "Compensatorio por trabajo dominical", "Compensatorio por trabajo en festivo", "Compensatorio por horas extra" }
        };

        var tiposPermiso = new[] { "tipo_0001", "tipo_0002", "tipo_0003", "tipo_0008", "tipo_0009", "tipo_0010" };
        var estados = new[] { "Aprobado", "Aprobado", "Aprobado", "Pendiente", "Rechazado" }; // 60% aprobado, 20% pendiente, 20% rechazado

        var collection = _firestore!.Collection("permisos");
        int count = 0;
        int permisoNumero = 0;

        // Generar permisos para algunos empleados (no todos)
        var empleadosConPermisos = _empleadosGenerados
            .Where(_ => _random.NextDouble() > 0.2) // 80% de empleados tendrán permisos
            .ToList();

        foreach (var empleado in empleadosConPermisos)
        {
            // Cada empleado puede tener 1-4 permisos
            var numPermisos = _random.Next(1, 5);

            for (int i = 0; i < numPermisos; i++)
            {
                count++;
                permisoNumero++;

                var tipoPermiso = tiposPermiso[_random.Next(tiposPermiso.Length)];
                var tipoIndex = int.Parse(tipoPermiso.Split('_')[1]);
                var tipoNombres = new[] { "Cita médica", "Calamidad doméstica", "Diligencia personal", "", "", "", "", "Incapacidad", "Capacitación", "Compensatorio" };
                var tipoNombre = tipoNombres[tipoIndex - 1];

                var motivos = motivosPorTipo.GetValueOrDefault(tipoPermiso, new[] { "Permiso personal" });
                var motivo = motivos[_random.Next(motivos.Length)];

                var estado = estados[_random.Next(estados.Length)];

                // Fecha de solicitud (últimos 6 meses)
                var fechaSolicitud = DateTime.Today.AddDays(-_random.Next(1, 180));
                
                // Fecha del permiso (puede ser pasada o futura según el estado)
                var diasOffset = estado == "Pendiente" ? _random.Next(1, 30) : -_random.Next(1, 60);
                var fechaInicio = DateTime.Today.AddDays(diasOffset);
                
                // Días del permiso (1-3 días normalmente)
                var diasSolicitados = tipoPermiso == "tipo_0008" ? _random.Next(1, 8) : _random.Next(1, 3);
                var fechaFin = fechaInicio.AddDays(diasSolicitados - 1);

                // Horas (para permisos de medio día)
                string? horaSalida = null;
                string? horaRegreso = null;
                
                if (diasSolicitados == 1 && _random.NextDouble() > 0.5)
                {
                    // Permiso de medio día
                    var esMañana = _random.NextDouble() > 0.5;
                    if (esMañana)
                    {
                        horaSalida = "07:00";
                        horaRegreso = "12:00";
                    }
                    else
                    {
                        horaSalida = "12:00";
                        horaRegreso = "17:00";
                    }
                }

                // Usuario que solicita (usamos el mismo empleado como referencia)
                var solicitadoPorId = "6VSFfKaAlAaDOcH40EIzKaTZXBM2"; // admin

                // Usuario que aprueba (si está aprobado)
                string? aprobadoPorId = null;
                DateTime? fechaAprobacion = null;
                string? motivoRechazo = null;

                if (estado == "Aprobado")
                {
                    aprobadoPorId = "iGpEuajlmjaknDfwBEjBkwtCRyK2"; // ingeniera
                    fechaAprobacion = fechaSolicitud.AddDays(_random.Next(1, 3));
                }
                else if (estado == "Rechazado")
                {
                    aprobadoPorId = "iGpEuajlmjaknDfwBEjBkwtCRyK2"; // ingeniera
                    fechaAprobacion = fechaSolicitud.AddDays(_random.Next(1, 3));
                    motivoRechazo = new[] {
                        "No hay disponibilidad de personal para cubrir",
                        "Falta documentación soporte",
                        "Permiso solicitado con muy poca anticipación",
                        "Ya hay otros permisos aprobados para esa fecha"
                    }[_random.Next(4)];
                }

                var permiso = new Dictionary<string, object>
                {
                    ["numeroActa"] = $"PERM-2025-{permisoNumero:D4}",
                    ["empleadoId"] = empleado.id,
                    ["empleadoNombre"] = empleado.nombre,
                    ["empleadoCodigo"] = $"EMP-{int.Parse(empleado.id.Split('_')[1]):D3}",
                    ["tipoPermisoId"] = tipoPermiso,
                    ["tipoPermisoNombre"] = tipoNombre,
                    ["motivo"] = motivo,
                    ["fechaSolicitud"] = Timestamp.FromDateTime(DateTime.SpecifyKind(fechaSolicitud, DateTimeKind.Utc)),
                    ["fechaInicio"] = Timestamp.FromDateTime(DateTime.SpecifyKind(fechaInicio, DateTimeKind.Utc)),
                    ["fechaFin"] = Timestamp.FromDateTime(DateTime.SpecifyKind(fechaFin, DateTimeKind.Utc)),
                    ["horaSalida"] = horaSalida!,
                    ["horaRegreso"] = horaRegreso!,
                    ["diasSolicitados"] = diasSolicitados,
                    ["esRemunerado"] = tipoPermiso != "tipo_0007", // Licencia no remunerada es la única no remunerada
                    ["estado"] = estado,
                    ["solicitadoPorId"] = solicitadoPorId,
                    ["aprobadoPorId"] = aprobadoPorId!,
                    ["fechaAprobacion"] = fechaAprobacion.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(fechaAprobacion.Value, DateTimeKind.Utc)) : null!,
                    ["documentoSoporteUrl"] = null!,
                    ["observaciones"] = null!,
                    ["motivoRechazo"] = motivoRechazo!,
                    ["activo"] = true,
                    ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow),
                    ["fechaModificacion"] = null!
                };

                var docRef = collection.Document($"perm_{count:D4}");
                await docRef.SetAsync(permiso);
            }
        }

        Console.WriteLine($"✅ {count} creados");
    }

    static async Task GenerateVacacionesAsync()
    {
        Console.Write("   → Vacaciones... ");

        var collection = _firestore!.Collection("vacaciones");
        int count = 0;

        foreach (var empleado in _empleadosGenerados)
        {
            count++;

            // Calcular días disponibles basados en antigüedad (15 días por año)
            var diasDisponibles = 15; // Simplificado, asumimos un año completo
            var diasTomados = _random.Next(0, diasDisponibles + 1);
            var diasPendientes = diasDisponibles - diasTomados;

            // Si tomó días, generar fechas
            DateTime? fechaInicio = null;
            DateTime? fechaFin = null;
            var estado = "Disponible";

            if (diasTomados > 0)
            {
                // Vacaciones tomadas en los últimos 12 meses
                var mesInicio = _random.Next(1, 12);
                fechaInicio = new DateTime(2025, mesInicio, _random.Next(1, 28));
                fechaFin = fechaInicio.Value.AddDays(diasTomados - 1);
                
                if (fechaFin.Value < DateTime.Today)
                {
                    estado = "Tomadas";
                }
                else if (fechaInicio.Value <= DateTime.Today)
                {
                    estado = "EnCurso";
                }
                else
                {
                    estado = "Programadas";
                }
            }

            var vacacion = new Dictionary<string, object>
            {
                ["empleadoId"] = empleado.id,
                ["empleadoNombre"] = empleado.nombre,
                ["empleadoCodigo"] = $"EMP-{int.Parse(empleado.id.Split('_')[1]):D3}",
                ["periodo"] = 2025,
                ["diasDisponibles"] = diasDisponibles,
                ["diasTomados"] = diasTomados,
                ["diasPendientes"] = diasPendientes,
                ["fechaInicio"] = fechaInicio.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(fechaInicio.Value, DateTimeKind.Utc)) : null!,
                ["fechaFin"] = fechaFin.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(fechaFin.Value, DateTimeKind.Utc)) : null!,
                ["estado"] = estado,
                ["observaciones"] = diasTomados > 0 ? "Vacaciones del período 2025" : null!,
                ["activo"] = true,
                ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["fechaModificacion"] = null!
            };

            var docRef = collection.Document($"vac_{count:D4}");
            await docRef.SetAsync(vacacion);
        }

        Console.WriteLine($"✅ {count} creadas");
    }

    #endregion

    #region Contratos y Registros Diarios

    static async Task GenerateContratosRegistrosDiariosAsync()
    {
        Console.WriteLine("\n📝 Generando contratos y registros diarios...");

        // Cargar empleados si no están en memoria
        if (_empleadosGenerados.Count == 0)
        {
            await LoadEmpleadosAsync();
        }

        if (_empleadosGenerados.Count == 0)
        {
            Console.WriteLine("   ⚠️  No hay empleados. Primero genere los empleados.");
            return;
        }

        await GenerateContratosAsync();
        await GenerateRegistrosDiariosAsync();
    }

    static async Task GenerateContratosAsync()
    {
        Console.Write("   → Contratos... ");

        // Salarios base por nivel de cargo (en COP)
        var salariosPorCargo = new Dictionary<string, (decimal min, decimal max)>
        {
            ["car_0001"] = (8000000, 12000000),  // Gerente
            ["car_0002"] = (5000000, 7000000),   // Ingeniero Forestal
            ["car_0003"] = (4500000, 6500000),   // Ingeniero Ambiental
            ["car_0004"] = (2500000, 3500000),   // Técnico Forestal
            ["car_0005"] = (2200000, 3000000),   // Supervisor de Campo
            ["car_0006"] = (1300000, 1800000),   // Operario Forestal
            ["car_0007"] = (1500000, 2000000),   // Conductor
            ["car_0008"] = (1600000, 2200000),   // Motosierrista
            ["car_0009"] = (2000000, 2800000),   // Secretaria
            ["car_0010"] = (1800000, 2500000),   // Auxiliar Contable
            ["car_0011"] = (2500000, 3500000),   // Jefe de Vivero
            ["car_0012"] = (1300000, 1800000)    // Operario de Vivero
        };

        // Tipos de contrato
        var tiposContratoPorCargo = new Dictionary<string, string[]>
        {
            ["car_0001"] = new[] { "Indefinido" },
            ["car_0002"] = new[] { "Indefinido" },
            ["car_0003"] = new[] { "Indefinido", "PrestacionServicios" },
            ["car_0004"] = new[] { "Indefinido", "Fijo" },
            ["car_0005"] = new[] { "Indefinido", "Fijo" },
            ["car_0006"] = new[] { "Fijo", "ObraLabor" },
            ["car_0007"] = new[] { "Indefinido", "Fijo" },
            ["car_0008"] = new[] { "Fijo", "ObraLabor" },
            ["car_0009"] = new[] { "Indefinido" },
            ["car_0010"] = new[] { "Indefinido", "Fijo" },
            ["car_0011"] = new[] { "Indefinido" },
            ["car_0012"] = new[] { "Fijo", "ObraLabor" }
        };

        var collection = _firestore!.Collection("contratos");
        int count = 0;

        // Obtener datos adicionales de empleados de Firestore
        var empleadosSnapshot = await _firestore.Collection("empleados").GetSnapshotAsync();
        var empleadosData = empleadosSnapshot.Documents.ToDictionary(
            d => d.Id,
            d => new {
                FechaIngreso = d.GetValue<Timestamp>("fechaIngreso").ToDateTime(),
                CargoNombre = d.GetValue<string>("cargoNombre")
            }
        );

        foreach (var empleado in _empleadosGenerados)
        {
            count++;

            var rangSalario = salariosPorCargo.GetValueOrDefault(empleado.cargoId, (1300000, 1800000));
            var salario = _random.Next((int)rangSalario.min, (int)rangSalario.max);
            salario = (salario / 10000) * 10000; // Redondear a decenas de miles

            var tiposDisponibles = tiposContratoPorCargo.GetValueOrDefault(empleado.cargoId, new[] { "Indefinido" });
            var tipoContrato = tiposDisponibles[_random.Next(tiposDisponibles.Length)];

            var fechaIngreso = empleadosData.TryGetValue(empleado.id, out var empData) 
                ? empData.FechaIngreso 
                : DateTime.Today.AddYears(-1);

            var cargoNombre = empData?.CargoNombre ?? empleado.cargoId;

            // Fecha fin (solo para contratos fijos u obra labor)
            DateTime? fechaFin = null;
            if (tipoContrato == "Fijo")
            {
                // Contrato fijo de 6 meses a 2 años
                fechaFin = fechaIngreso.AddMonths(_random.Next(6, 24));
            }
            else if (tipoContrato == "ObraLabor")
            {
                // Contrato por obra puede variar según el proyecto
                fechaFin = fechaIngreso.AddMonths(_random.Next(3, 12));
            }

            // Estado del contrato
            var estado = "Activo";
            if (fechaFin.HasValue && fechaFin.Value < DateTime.Today)
            {
                estado = _random.NextDouble() > 0.7 ? "Vencido" : "Activo"; // Algunos renovados
                if (estado == "Activo")
                {
                    fechaFin = fechaFin.Value.AddYears(1); // Renovado por un año más
                }
            }

            var contrato = new Dictionary<string, object>
            {
                ["empleadoId"] = empleado.id,
                ["empleadoNombre"] = empleado.nombre,
                ["empleadoCodigo"] = $"EMP-{int.Parse(empleado.id.Split('_')[1]):D3}",
                ["tipoContrato"] = tipoContrato,
                ["fechaInicio"] = Timestamp.FromDateTime(DateTime.SpecifyKind(fechaIngreso, DateTimeKind.Utc)),
                ["fechaFin"] = fechaFin.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(fechaFin.Value, DateTimeKind.Utc)) : null!,
                ["salario"] = salario,
                ["cargoId"] = empleado.cargoId,
                ["cargoNombre"] = cargoNombre,
                ["estado"] = estado,
                ["archivoUrl"] = null!,
                ["observaciones"] = tipoContrato == "ObraLabor" ? "Contrato vinculado a proyecto específico" : null!,
                ["activo"] = true,
                ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["fechaModificacion"] = null!
            };

            var docRef = collection.Document($"cont_{count:D4}");
            await docRef.SetAsync(contrato);
        }

        Console.WriteLine($"✅ {count} creados");
    }

    static async Task GenerateRegistrosDiariosAsync()
    {
        Console.Write("   → Registros diarios... ");

        // Cargar actividades y proyectos
        var actividadesSnapshot = await _firestore!.Collection("actividades").GetSnapshotAsync();
        var actividades = actividadesSnapshot.Documents
            .Select(d => (id: d.Id, nombre: d.GetValue<string>("nombre"), requiereProyecto: d.GetValue<bool>("requiereProyecto"), categoria: d.GetValue<string>("categoria")))
            .ToList();

        var proyectosSnapshot = await _firestore!.Collection("proyectos").GetSnapshotAsync();
        var proyectos = proyectosSnapshot.Documents
            .Where(d => d.GetValue<string>("estado") == "Activo")
            .Select(d => (id: d.Id, nombre: d.GetValue<string>("nombre")))
            .ToList();

        // Obtener datos adicionales de empleados
        var empleadosSnapshot = await _firestore.Collection("empleados").GetSnapshotAsync();
        var empleadosData = empleadosSnapshot.Documents.ToDictionary(
            d => d.Id,
            d => new {
                Codigo = d.GetValue<string>("codigo"),
                Departamento = d.GetValue<string>("departamentoNombre")
            }
        );

        var collection = _firestore!.Collection("registros-diarios");
        int totalRegistros = 0;
        int totalDetalles = 0;

        // Generar registros para los últimos 30 días laborables
        var fechasLaborables = new List<DateTime>();
        var fecha = DateTime.Today;
        while (fechasLaborables.Count < 25) // ~5 semanas
        {
            fecha = fecha.AddDays(-1);
            if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
            {
                fechasLaborables.Add(fecha);
            }
        }

        Console.WriteLine();

        foreach (var fechaRegistro in fechasLaborables.Take(20)) // 4 semanas de datos
        {
            // Seleccionar empleados que trabajan ese día (80-95% de asistencia)
            var empleadosTrabajando = _empleadosGenerados
                .Where(_ => _random.NextDouble() > 0.1)
                .ToList();

            foreach (var empleado in empleadosTrabajando)
            {
                totalRegistros++;

                var empData = empleadosData.GetValueOrDefault(empleado.id);
                var empCodigo = empData?.Codigo ?? $"EMP-{int.Parse(empleado.id.Split('_')[1]):D3}";
                var empDepto = empData?.Departamento ?? "Sin departamento";

                // Hora de entrada (7:00 - 7:30 normalmente)
                var minutosEntrada = _random.Next(0, 45);
                var horaEntrada = new TimeSpan(7, minutosEntrada, 0);

                // Hora de salida (16:30 - 17:30 normalmente)
                var minutosSalida = _random.Next(30, 90);
                var horaSalida = new TimeSpan(16, minutosSalida, 0);

                // Total de horas trabajadas
                var horasTotales = (horaSalida - horaEntrada).TotalHours - 1; // Menos 1 hora de almuerzo

                var registro = new Dictionary<string, object>
                {
                    ["empleadoId"] = empleado.id,
                    ["empleadoNombre"] = empleado.nombre,
                    ["empleadoCodigo"] = empCodigo,
                    ["empleadoDepartamento"] = empDepto,
                    ["fecha"] = Timestamp.FromDateTime(DateTime.SpecifyKind(fechaRegistro, DateTimeKind.Utc)),
                    ["horaEntrada"] = horaEntrada.ToString(@"hh\:mm"),
                    ["horaSalida"] = horaSalida.ToString(@"hh\:mm"),
                    ["horasTotales"] = Math.Round(horasTotales, 1),
                    ["observaciones"] = null!,
                    ["activo"] = true,
                    ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow),
                    ["fechaModificacion"] = null!
                };

                var registroId = $"reg_{totalRegistros:D5}";
                var registroRef = collection.Document(registroId);
                await registroRef.SetAsync(registro);

                // Generar detalles de actividades (2-4 actividades por día)
                var numActividades = _random.Next(2, 5);
                var horasRestantes = horasTotales;
                var detallesCollection = registroRef.Collection("detalles");

                // Filtrar actividades según departamento
                var actividadesFiltradas = actividades;
                if (empDepto == "Vivero")
                {
                    actividadesFiltradas = actividades.Where(a => a.categoria == "Vivero" || a.categoria == "Administrativo").ToList();
                }
                else if (empDepto == "Operaciones")
                {
                    actividadesFiltradas = actividades.Where(a => a.categoria == "Campo" || a.categoria == "Transporte" || a.categoria == "Administrativo").ToList();
                }
                else if (empDepto == "Administración" || empDepto == "Gerencia General")
                {
                    actividadesFiltradas = actividades.Where(a => a.categoria == "Administrativo").ToList();
                }

                if (!actividadesFiltradas.Any())
                {
                    actividadesFiltradas = actividades;
                }

                for (int i = 0; i < numActividades && horasRestantes > 0.5; i++)
                {
                    totalDetalles++;
                    var actividad = actividadesFiltradas[_random.Next(actividadesFiltradas.Count)];

                    // Calcular horas para esta actividad
                    double horas;
                    if (i == numActividades - 1 || horasRestantes < 2)
                    {
                        horas = Math.Round(horasRestantes, 1);
                    }
                    else
                    {
                        horas = Math.Round(_random.NextDouble() * (horasRestantes * 0.6) + 1, 1);
                    }
                    horasRestantes -= horas;

                    // Asignar proyecto si la actividad lo requiere
                    string? proyectoId = null;
                    string? proyectoNombre = null;
                    if (actividad.requiereProyecto && proyectos.Any())
                    {
                        var proyecto = proyectos[_random.Next(proyectos.Count)];
                        proyectoId = proyecto.id;
                        proyectoNombre = proyecto.nombre;
                    }

                    var descripcionesActividad = new Dictionary<string, string[]>
                    {
                        ["Preparación de terreno"] = new[] { "Limpieza de malezas", "Trazado de líneas de siembra", "Ahoyado manual" },
                        ["Siembra"] = new[] { "Siembra de plántulas de eucalipto", "Siembra de pinos", "Resiembra de mortalidad" },
                        ["Fertilización"] = new[] { "Aplicación de fertilizante granulado", "Fertilización foliar", "Segunda fertilización" },
                        ["Control de malezas"] = new[] { "Plateo con azadón", "Guadañada entre líneas", "Aplicación de herbicida" },
                        ["Riego"] = new[] { "Riego de camas de germinación", "Riego de plántulas en bolsa", "Verificación de sistema de riego" },
                        ["Supervisión"] = new[] { "Supervisión de cuadrilla en campo", "Verificación de avance de labores", "Control de calidad de siembra" },
                        ["Reunión"] = new[] { "Reunión de coordinación semanal", "Reunión con cliente", "Planeación de actividades" },
                        ["Gestión documental"] = new[] { "Elaboración de informe de avance", "Actualización de registros", "Preparación de facturación" }
                    };

                    var descripciones = descripcionesActividad.GetValueOrDefault(actividad.nombre, new[] { $"Desarrollo de {actividad.nombre.ToLower()}" });
                    var descripcion = descripciones[_random.Next(descripciones.Length)];

                    var detalle = new Dictionary<string, object>
                    {
                        ["actividadId"] = actividad.id,
                        ["actividadNombre"] = actividad.nombre,
                        ["proyectoId"] = proyectoId!,
                        ["proyectoNombre"] = proyectoNombre!,
                        ["horas"] = horas,
                        ["descripcion"] = descripcion,
                        ["orden"] = i + 1,
                        ["activo"] = true
                    };

                    await detallesCollection.Document($"det_{i + 1:D2}").SetAsync(detalle);
                }
            }

            // Mostrar progreso
            Console.Write($"\r   → Registros diarios... {fechaRegistro:dd/MM/yyyy} ({totalRegistros} registros, {totalDetalles} detalles)");
        }

        Console.WriteLine($"\r   → Registros diarios... ✅ {totalRegistros} registros, {totalDetalles} detalles                    ");
    }

    #endregion

    #region Utilidades

    static async Task CleanAllDataAsync()
    {
        Console.WriteLine("\n⚠️  ¿Está seguro que desea ELIMINAR todos los datos de prueba?");
        Console.Write("   Escriba 'SI' para confirmar: ");
        
        if (Console.ReadLine()?.Trim().ToUpper() != "SI")
        {
            Console.WriteLine("   ❌ Operación cancelada");
            return;
        }

        Console.WriteLine("\n🗑️  Limpiando colecciones...");

        var collections = new[]
        {
            "departamentos", "cargos", "actividades", "proyectos", "tipos-permiso",
            "config", "empleados", "permisos", "vacaciones", "contratos",
            "registros-diarios", "audit-logs"
        };

        foreach (var collectionName in collections)
        {
            Console.Write($"   → {collectionName}... ");
            var deleted = await DeleteCollectionAsync(collectionName);
            Console.WriteLine($"✅ {deleted} eliminados");
        }

        Console.WriteLine("\n✅ Todas las colecciones limpiadas");
    }

    static async Task<int> DeleteCollectionAsync(string collectionName, int batchSize = 100)
    {
        var collection = _firestore!.Collection(collectionName);
        int totalDeleted = 0;

        while (true)
        {
            var snapshot = await collection.Limit(batchSize).GetSnapshotAsync();
            
            if (snapshot.Count == 0)
                break;

            var batch = _firestore.StartBatch();
            foreach (var doc in snapshot.Documents)
            {
                batch.Delete(doc.Reference);
            }
            
            await batch.CommitAsync();
            totalDeleted += snapshot.Count;
        }

        return totalDeleted;
    }

    static async Task ShowCollectionStatsAsync()
    {
        Console.WriteLine("\n📊 Estadísticas de colecciones:\n");

        var collections = new[]
        {
            "departamentos", "cargos", "actividades", "proyectos", "tipos-permiso",
            "config", "empleados", "users", "permisos", "vacaciones", "contratos",
            "registros-diarios", "audit-logs"
        };

        foreach (var collectionName in collections)
        {
            var snapshot = await _firestore!.Collection(collectionName).GetSnapshotAsync();
            var icon = snapshot.Count > 0 ? "📁" : "📂";
            Console.WriteLine($"   {icon} {collectionName,-20} : {snapshot.Count,5} documentos");
        }
    }

    #endregion
}
