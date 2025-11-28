using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

// Configuración
string projectId = "rrhh-forestech";
string databaseId = "rrhh-forestech";
string credentialsPath = @"C:\Users\evert\Documents\rrhh\src\SGRRHH.WPF\firebase-credentials.json";

// UIDs de Firebase Auth
var usuarios = new[]
{
    new { Uid = "6VSFfKaAlAaDOcH40EIzKaTZXBM2", Username = "admin", NombreCompleto = "Administrador del Sistema", Email = "admin@sgrrhh.local", Rol = "Administrador" },
    new { Uid = "Z8JPNioOB5U0O8zMityslj5EjpZ2", Username = "secretaria", NombreCompleto = "Secretaria", Email = "secretaria@sgrrhh.local", Rol = "Operador" },
    new { Uid = "iGpEuajlmjaknDfwBEjBkwtCRyK2", Username = "ingeniera", NombreCompleto = "Ingeniera de Gestión", Email = "ingeniera@sgrrhh.local", Rol = "Aprobador" }
};

Console.WriteLine("=== Creando usuarios en Firestore ===\n");

try
{
    // Cargar credenciales
    GoogleCredential credential;
    using (var stream = File.OpenRead(credentialsPath))
    {
        credential = GoogleCredential.FromStream(stream);
    }
    
    // Conectar a Firestore
    var builder = new FirestoreDbBuilder
    {
        ProjectId = projectId,
        DatabaseId = databaseId,
        Credential = credential
    };
    
    var db = await builder.BuildAsync();
    Console.WriteLine($"Conectado a Firestore: {projectId}/{databaseId}\n");
    
    // Crear usuarios
    foreach (var user in usuarios)
    {
        var docRef = db.Collection("users").Document(user.Uid);
        
        var userData = new Dictionary<string, object?>
        {
            ["username"] = user.Username,
            ["nombreCompleto"] = user.NombreCompleto,
            ["email"] = user.Email,
            ["rol"] = user.Rol,
            ["empleadoId"] = null,
            ["ultimoAcceso"] = null,
            ["activo"] = true,
            ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
        };
        
        await docRef.SetAsync(userData);
        Console.WriteLine($"✓ Usuario creado: {user.Username} ({user.Email}) - Rol: {user.Rol}");
    }
    
    Console.WriteLine("\n=== Usuarios creados exitosamente ===");
    Console.WriteLine("\nAhora puedes iniciar sesión con:");
    Console.WriteLine("  - admin / admin123");
    Console.WriteLine("  - secretaria / secretaria123");
    Console.WriteLine("  - ingeniera / ingeniera123");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

Console.WriteLine("\nPresiona Enter para salir...");
Console.ReadLine();
