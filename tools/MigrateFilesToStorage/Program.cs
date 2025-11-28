using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;

namespace MigrateFilesToStorage;

/// <summary>
/// Herramienta de migración de archivos locales a Firebase Storage.
/// 
/// Estructura de carpetas local esperada:
/// data/
/// ├── fotos/
/// │   └── empleados/
/// │       └── {empleadoId}/
/// │           └── foto.{ext}
/// ├── documentos/
/// │   ├── permisos/
/// │   │   └── {permisoId}/
/// │   │       └── soporte.{ext}
/// │   ├── contratos/
/// │   │   └── {contratoId}/
/// │   │       └── contrato.{ext}
/// │   └── generados/
/// │       ├── actas/
/// │       │   └── {filename}.pdf
/// │       └── certificados/
/// │           └── {filename}.pdf
/// └── config/
///     └── logo.{ext}
/// </summary>
class Program
{
    private static IConfiguration _configuration = null!;
    private static StorageClient? _storageClient;
    private static FirestoreDb? _firestoreDb;
    private static string _bucketName = null!;
    
    // Contadores para estadísticas
    private static int _filesUploaded = 0;
    private static int _filesSkipped = 0;
    private static int _filesFailed = 0;
    private static long _bytesUploaded = 0;
    
    // Content Types
    private static readonly Dictionary<string, string> ExtensionToContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".webp", "image/webp" },
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".txt", "text/plain" },
        { ".json", "application/json" },
    };
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     SGRRHH - Migración de Archivos a Firebase Storage        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        try
        {
            // Cargar configuración
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            
            // Inicializar Firebase
            if (!await InitializeFirebaseAsync())
            {
                Console.WriteLine("\n❌ No se pudo inicializar Firebase. Verifique las credenciales.");
                Console.WriteLine("   Asegúrese de copiar firebase-credentials.json a esta carpeta.");
                return;
            }
            
            Console.WriteLine("\n✅ Firebase inicializado correctamente\n");
            
            // Menú principal
            while (true)
            {
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("                    MENÚ PRINCIPAL");
                Console.WriteLine("═══════════════════════════════════════════════════════════");
                Console.WriteLine("1. Migrar fotos de empleados");
                Console.WriteLine("2. Migrar documentos de permisos");
                Console.WriteLine("3. Migrar documentos de contratos");
                Console.WriteLine("4. Migrar documentos generados (actas, certificados)");
                Console.WriteLine("5. Migrar configuración (logo)");
                Console.WriteLine("6. Migrar TODO (ejecutar 1-5)");
                Console.WriteLine("7. Listar archivos en Storage");
                Console.WriteLine("8. Ver estadísticas de migración");
                Console.WriteLine("0. Salir");
                Console.WriteLine("═══════════════════════════════════════════════════════════");
                Console.Write("\nSeleccione una opción: ");
                
                var option = Console.ReadLine();
                Console.WriteLine();
                
                switch (option)
                {
                    case "1":
                        await MigrateFotosEmpleadosAsync();
                        break;
                    case "2":
                        await MigrateDocumentosPermisosAsync();
                        break;
                    case "3":
                        await MigrateDocumentosContratosAsync();
                        break;
                    case "4":
                        await MigrateDocumentosGeneradosAsync();
                        break;
                    case "5":
                        await MigrateConfiguracionAsync();
                        break;
                    case "6":
                        await MigrateAllAsync();
                        break;
                    case "7":
                        await ListStorageFilesAsync();
                        break;
                    case "8":
                        ShowStatistics();
                        break;
                    case "0":
                        Console.WriteLine("\n👋 ¡Hasta pronto!");
                        return;
                    default:
                        Console.WriteLine("\n⚠️ Opción no válida");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error fatal: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
    
    private static async Task<bool> InitializeFirebaseAsync()
    {
        try
        {
            var projectId = _configuration["Firebase:ProjectId"];
            var databaseId = _configuration["Firebase:DatabaseId"] ?? "(default)";
            _bucketName = _configuration["Firebase:StorageBucket"] ?? "";
            var credentialsPath = _configuration["Firebase:CredentialsPath"] ?? "firebase-credentials.json";
            
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(_bucketName))
            {
                Console.WriteLine("❌ Configuración de Firebase incompleta en appsettings.json");
                return false;
            }
            
            // Buscar archivo de credenciales
            var fullCredentialsPath = Path.GetFullPath(credentialsPath);
            if (!File.Exists(fullCredentialsPath))
            {
                fullCredentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, credentialsPath);
            }
            
            if (!File.Exists(fullCredentialsPath))
            {
                Console.WriteLine($"❌ Archivo de credenciales no encontrado: {credentialsPath}");
                return false;
            }
            
            Console.WriteLine($"📁 Credenciales: {fullCredentialsPath}");
            Console.WriteLine($"🔥 Proyecto: {projectId}");
            Console.WriteLine($"📦 Storage Bucket: {_bucketName}");
            Console.WriteLine($"🗄️ Database: {databaseId}");
            
            // Cargar credenciales
            GoogleCredential credential;
            using (var stream = File.OpenRead(fullCredentialsPath))
            {
                credential = await GoogleCredential.FromStreamAsync(stream, CancellationToken.None);
            }
            
            // Inicializar Storage
            _storageClient = await StorageClient.CreateAsync(credential);
            
            // Inicializar Firestore (para actualizar URLs)
            var firestoreBuilder = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                DatabaseId = databaseId,
                Credential = credential
            };
            _firestoreDb = await firestoreBuilder.BuildAsync();
            
            // Probar conexión
            var testObjects = _storageClient.ListObjects(_bucketName, null, new ListObjectsOptions { PageSize = 1 });
            // Si no hay error, la conexión es exitosa
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al inicializar Firebase: {ex.Message}");
            return false;
        }
    }
    
    private static async Task MigrateFotosEmpleadosAsync()
    {
        Console.WriteLine("📸 Migrando fotos de empleados...");
        
        var localPath = GetLocalPath("LocalPaths:FotosEmpleados");
        if (!Directory.Exists(localPath))
        {
            Console.WriteLine($"⚠️ Carpeta no encontrada: {localPath}");
            Console.WriteLine("   Creando estructura de carpetas...");
            Directory.CreateDirectory(localPath);
            Console.WriteLine($"   Carpeta creada. Coloque las fotos en subcarpetas con el ID del empleado.");
            return;
        }
        
        var employeeDirs = Directory.GetDirectories(localPath);
        Console.WriteLine($"   Encontradas {employeeDirs.Length} carpetas de empleados");
        
        foreach (var empDir in employeeDirs)
        {
            var empleadoId = Path.GetFileName(empDir);
            var files = Directory.GetFiles(empDir);
            
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var storagePath = $"fotos/empleados/{empleadoId}/{fileName}";
                
                await UploadFileAsync(file, storagePath, async () =>
                {
                    // Actualizar URL en Firestore
                    await UpdateEmpleadoFotoUrlAsync(empleadoId, GetPublicUrl(storagePath));
                });
            }
        }
        
        Console.WriteLine("✅ Migración de fotos completada");
    }
    
    private static async Task MigrateDocumentosPermisosAsync()
    {
        Console.WriteLine("📄 Migrando documentos de permisos...");
        
        var localPath = GetLocalPath("LocalPaths:DocumentosPermisos");
        if (!Directory.Exists(localPath))
        {
            Console.WriteLine($"⚠️ Carpeta no encontrada: {localPath}");
            Directory.CreateDirectory(localPath);
            Console.WriteLine($"   Carpeta creada. Coloque los documentos en subcarpetas con el ID del permiso.");
            return;
        }
        
        var permisoDirs = Directory.GetDirectories(localPath);
        Console.WriteLine($"   Encontradas {permisoDirs.Length} carpetas de permisos");
        
        foreach (var permisoDir in permisoDirs)
        {
            var permisoId = Path.GetFileName(permisoDir);
            var files = Directory.GetFiles(permisoDir);
            
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var storagePath = $"documentos/permisos/{permisoId}/{fileName}";
                
                await UploadFileAsync(file, storagePath, async () =>
                {
                    await UpdatePermisoDocumentoUrlAsync(permisoId, GetPublicUrl(storagePath));
                });
            }
        }
        
        Console.WriteLine("✅ Migración de documentos de permisos completada");
    }
    
    private static async Task MigrateDocumentosContratosAsync()
    {
        Console.WriteLine("📑 Migrando documentos de contratos...");
        
        var localPath = GetLocalPath("LocalPaths:DocumentosContratos");
        if (!Directory.Exists(localPath))
        {
            Console.WriteLine($"⚠️ Carpeta no encontrada: {localPath}");
            Directory.CreateDirectory(localPath);
            Console.WriteLine($"   Carpeta creada. Coloque los documentos en subcarpetas con el ID del contrato.");
            return;
        }
        
        var contratoDirs = Directory.GetDirectories(localPath);
        Console.WriteLine($"   Encontradas {contratoDirs.Length} carpetas de contratos");
        
        foreach (var contratoDir in contratoDirs)
        {
            var contratoId = Path.GetFileName(contratoDir);
            var files = Directory.GetFiles(contratoDir);
            
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var storagePath = $"documentos/contratos/{contratoId}/{fileName}";
                
                await UploadFileAsync(file, storagePath, async () =>
                {
                    await UpdateContratoArchivoUrlAsync(contratoId, GetPublicUrl(storagePath));
                });
            }
        }
        
        Console.WriteLine("✅ Migración de documentos de contratos completada");
    }
    
    private static async Task MigrateDocumentosGeneradosAsync()
    {
        Console.WriteLine("📝 Migrando documentos generados...");
        
        var localPath = GetLocalPath("LocalPaths:DocumentosGenerados");
        if (!Directory.Exists(localPath))
        {
            Console.WriteLine($"⚠️ Carpeta no encontrada: {localPath}");
            Directory.CreateDirectory(localPath);
            
            // Crear subcarpetas
            Directory.CreateDirectory(Path.Combine(localPath, "actas"));
            Directory.CreateDirectory(Path.Combine(localPath, "certificados"));
            Console.WriteLine($"   Carpetas creadas (actas, certificados).");
            return;
        }
        
        // Migrar actas
        var actasPath = Path.Combine(localPath, "actas");
        if (Directory.Exists(actasPath))
        {
            var actas = Directory.GetFiles(actasPath, "*.pdf");
            Console.WriteLine($"   Encontradas {actas.Length} actas");
            
            foreach (var file in actas)
            {
                var fileName = Path.GetFileName(file);
                var storagePath = $"documentos/generados/actas/{fileName}";
                await UploadFileAsync(file, storagePath);
            }
        }
        
        // Migrar certificados
        var certificadosPath = Path.Combine(localPath, "certificados");
        if (Directory.Exists(certificadosPath))
        {
            var certificados = Directory.GetFiles(certificadosPath, "*.pdf");
            Console.WriteLine($"   Encontradas {certificados.Length} certificados");
            
            foreach (var file in certificados)
            {
                var fileName = Path.GetFileName(file);
                var storagePath = $"documentos/generados/certificados/{fileName}";
                await UploadFileAsync(file, storagePath);
            }
        }
        
        Console.WriteLine("✅ Migración de documentos generados completada");
    }
    
    private static async Task MigrateConfiguracionAsync()
    {
        Console.WriteLine("⚙️ Migrando configuración...");
        
        var localPath = GetLocalPath("LocalPaths:ConfigFolder");
        if (!Directory.Exists(localPath))
        {
            Console.WriteLine($"⚠️ Carpeta no encontrada: {localPath}");
            Directory.CreateDirectory(localPath);
            Console.WriteLine($"   Carpeta creada. Coloque el logo de la empresa aquí.");
            return;
        }
        
        // Buscar logo
        var logoExtensions = new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp" };
        foreach (var ext in logoExtensions)
        {
            var logos = Directory.GetFiles(localPath, "logo" + ext.Substring(1));
            foreach (var logo in logos)
            {
                var fileName = Path.GetFileName(logo);
                var storagePath = $"config/{fileName}";
                await UploadFileAsync(logo, storagePath);
            }
        }
        
        // Migrar otros archivos de configuración
        var configFiles = Directory.GetFiles(localPath, "*.json");
        foreach (var file in configFiles)
        {
            var fileName = Path.GetFileName(file);
            var storagePath = $"config/{fileName}";
            await UploadFileAsync(file, storagePath);
        }
        
        Console.WriteLine("✅ Migración de configuración completada");
    }
    
    private static async Task MigrateAllAsync()
    {
        Console.WriteLine("🚀 Iniciando migración completa...\n");
        
        await MigrateFotosEmpleadosAsync();
        Console.WriteLine();
        
        await MigrateDocumentosPermisosAsync();
        Console.WriteLine();
        
        await MigrateDocumentosContratosAsync();
        Console.WriteLine();
        
        await MigrateDocumentosGeneradosAsync();
        Console.WriteLine();
        
        await MigrateConfiguracionAsync();
        
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        ShowStatistics();
        Console.WriteLine("═══════════════════════════════════════════════════════════");
    }
    
    private static async Task ListStorageFilesAsync()
    {
        Console.WriteLine("📂 Listando archivos en Storage...\n");
        
        try
        {
            var objects = _storageClient!.ListObjects(_bucketName);
            var folders = new Dictionary<string, List<(string Name, long Size)>>();
            
            foreach (var obj in objects)
            {
                var parts = obj.Name.Split('/');
                var folder = parts.Length > 1 ? string.Join("/", parts.Take(parts.Length - 1)) : "(raíz)";
                
                if (!folders.ContainsKey(folder))
                    folders[folder] = new List<(string, long)>();
                
                folders[folder].Add((obj.Name, (long)(obj.Size ?? 0)));
            }
            
            Console.WriteLine($"Total de carpetas: {folders.Count}");
            Console.WriteLine($"Total de archivos: {folders.Values.Sum(f => f.Count)}");
            Console.WriteLine();
            
            foreach (var folder in folders.OrderBy(f => f.Key))
            {
                Console.WriteLine($"📁 {folder.Key}/");
                foreach (var file in folder.Value.OrderBy(f => f.Name))
                {
                    var size = FormatFileSize(file.Size);
                    Console.WriteLine($"   📄 {Path.GetFileName(file.Name)} ({size})");
                }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al listar archivos: {ex.Message}");
        }
        
        await Task.CompletedTask;
    }
    
    private static void ShowStatistics()
    {
        Console.WriteLine("\n📊 ESTADÍSTICAS DE MIGRACIÓN");
        Console.WriteLine("─────────────────────────────────────");
        Console.WriteLine($"   Archivos subidos:    {_filesUploaded}");
        Console.WriteLine($"   Archivos omitidos:   {_filesSkipped}");
        Console.WriteLine($"   Archivos fallidos:   {_filesFailed}");
        Console.WriteLine($"   Bytes transferidos:  {FormatFileSize(_bytesUploaded)}");
        Console.WriteLine("─────────────────────────────────────");
    }
    
    private static async Task UploadFileAsync(string localPath, string storagePath, Func<Task>? onSuccess = null)
    {
        try
        {
            var fileInfo = new FileInfo(localPath);
            var contentType = GetContentType(localPath);
            
            Console.Write($"   📤 {Path.GetFileName(localPath)} -> {storagePath}... ");
            
            // Verificar si ya existe
            try
            {
                var existing = await _storageClient!.GetObjectAsync(_bucketName, storagePath);
                if (existing != null && existing.Size == (ulong)fileInfo.Length)
                {
                    Console.WriteLine("⏭️ Ya existe");
                    _filesSkipped++;
                    return;
                }
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // No existe, continuar con la subida
            }
            
            // Subir archivo
            await using var stream = File.OpenRead(localPath);
            await _storageClient!.UploadObjectAsync(_bucketName, storagePath, contentType, stream);
            
            Console.WriteLine($"✅ ({FormatFileSize(fileInfo.Length)})");
            _filesUploaded++;
            _bytesUploaded += fileInfo.Length;
            
            // Ejecutar callback si existe
            if (onSuccess != null)
            {
                await onSuccess();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            _filesFailed++;
        }
    }
    
    private static async Task UpdateEmpleadoFotoUrlAsync(string empleadoId, string url)
    {
        try
        {
            if (_firestoreDb == null) return;
            
            // Buscar documento por id numérico
            if (int.TryParse(empleadoId, out var id))
            {
                var query = _firestoreDb.Collection("empleados").WhereEqualTo("id", id);
                var snapshot = await query.GetSnapshotAsync();
                
                foreach (var doc in snapshot.Documents)
                {
                    await doc.Reference.UpdateAsync(new Dictionary<string, object>
                    {
                        ["fotoUrl"] = url,
                        ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"      ⚠️ No se pudo actualizar Firestore: {ex.Message}");
        }
    }
    
    private static async Task UpdatePermisoDocumentoUrlAsync(string permisoId, string url)
    {
        try
        {
            if (_firestoreDb == null) return;
            
            if (int.TryParse(permisoId, out var id))
            {
                var query = _firestoreDb.Collection("permisos").WhereEqualTo("id", id);
                var snapshot = await query.GetSnapshotAsync();
                
                foreach (var doc in snapshot.Documents)
                {
                    await doc.Reference.UpdateAsync(new Dictionary<string, object>
                    {
                        ["documentoSoporteUrl"] = url,
                        ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"      ⚠️ No se pudo actualizar Firestore: {ex.Message}");
        }
    }
    
    private static async Task UpdateContratoArchivoUrlAsync(string contratoId, string url)
    {
        try
        {
            if (_firestoreDb == null) return;
            
            if (int.TryParse(contratoId, out var id))
            {
                var query = _firestoreDb.Collection("contratos").WhereEqualTo("id", id);
                var snapshot = await query.GetSnapshotAsync();
                
                foreach (var doc in snapshot.Documents)
                {
                    await doc.Reference.UpdateAsync(new Dictionary<string, object>
                    {
                        ["archivoAdjuntoUrl"] = url,
                        ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"      ⚠️ No se pudo actualizar Firestore: {ex.Message}");
        }
    }
    
    private static string GetLocalPath(string configKey)
    {
        var relativePath = _configuration[configKey] ?? "";
        
        // Buscar en varias ubicaciones
        var possiblePaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath),
            Path.GetFullPath(relativePath),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "src", "SGRRHH.WPF", "bin", "Debug", "net8.0-windows", "win-x64", relativePath),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "src", "SGRRHH.WPF", relativePath),
        };
        
        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                return path;
            }
        }
        
        // Retornar la primera opción si ninguna existe
        return possiblePaths[0];
    }
    
    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
            return "application/octet-stream";
        
        return ExtensionToContentType.TryGetValue(extension, out var contentType)
            ? contentType
            : "application/octet-stream";
    }
    
    private static string GetPublicUrl(string storagePath)
    {
        var encodedPath = Uri.EscapeDataString(storagePath);
        return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{encodedPath}?alt=media";
    }
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }
}
