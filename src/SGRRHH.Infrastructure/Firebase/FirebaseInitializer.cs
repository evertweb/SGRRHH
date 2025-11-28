using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;

namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Inicializador y contenedor de las instancias de Firebase.
/// Patrón Singleton para asegurar una única instancia de cada servicio.
/// </summary>
public class FirebaseInitializer : IDisposable
{
    private readonly FirebaseConfig _config;
    private readonly ILogger<FirebaseInitializer>? _logger;
    
    private FirebaseApp? _firebaseApp;
    private FirestoreDb? _firestoreDb;
    private StorageClient? _storageClient;
    private bool _isInitialized;
    private bool _disposed;
    
    /// <summary>
    /// Indica si Firebase ha sido inicializado correctamente
    /// </summary>
    public bool IsInitialized => _isInitialized;
    
    /// <summary>
    /// Instancia de FirebaseApp (administración)
    /// </summary>
    public FirebaseApp? App => _firebaseApp;
    
    /// <summary>
    /// Instancia de FirestoreDb (base de datos)
    /// </summary>
    public FirestoreDb? Firestore => _firestoreDb;
    
    /// <summary>
    /// Instancia de StorageClient (archivos)
    /// </summary>
    public StorageClient? Storage => _storageClient;
    
    /// <summary>
    /// Configuración de Firebase
    /// </summary>
    public FirebaseConfig Config => _config;
    
    public FirebaseInitializer(FirebaseConfig config, ILogger<FirebaseInitializer>? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
    }
    
    /// <summary>
    /// Inicializa los servicios de Firebase.
    /// Debe llamarse una vez al inicio de la aplicación.
    /// </summary>
    /// <returns>True si la inicialización fue exitosa</returns>
    public async Task<bool> InitializeAsync()
    {
        if (_isInitialized)
        {
            _logger?.LogWarning("Firebase ya está inicializado");
            return true;
        }
        
        if (!_config.Enabled)
        {
            _logger?.LogInformation("Firebase está deshabilitado en la configuración");
            return false;
        }
        
        if (!_config.IsValid())
        {
            _logger?.LogError("La configuración de Firebase no es válida. Verifique ProjectId, StorageBucket y ApiKey");
            return false;
        }
        
        try
        {
            _logger?.LogInformation("Inicializando Firebase para proyecto: {ProjectId}", _config.ProjectId);
            
            // Configurar credenciales
            GoogleCredential? credential = await GetCredentialAsync();
            
            if (credential == null)
            {
                _logger?.LogError("No se pudieron obtener las credenciales de Firebase");
                return false;
            }
            
            // Inicializar FirebaseApp (Admin SDK)
            var existingApp = FirebaseApp.GetInstance("[DEFAULT]");
            if (existingApp != null)
            {
                _firebaseApp = existingApp;
                _logger?.LogInformation("Usando instancia existente de FirebaseApp");
            }
            else
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = _config.ProjectId
                });
                _logger?.LogInformation("FirebaseApp creado exitosamente");
            }
            
            // Inicializar Firestore
            var firestoreBuilder = new FirestoreDbBuilder
            {
                ProjectId = _config.ProjectId,
                DatabaseId = string.IsNullOrWhiteSpace(_config.DatabaseId) ? "(default)" : _config.DatabaseId,
                Credential = credential
            };
            _firestoreDb = await firestoreBuilder.BuildAsync();
            _logger?.LogInformation("FirestoreDb conectado exitosamente (DatabaseId: {DatabaseId})", _config.DatabaseId);
            
            // Inicializar Storage
            _storageClient = await StorageClient.CreateAsync(credential);
            _logger?.LogInformation("StorageClient creado exitosamente");
            
            _isInitialized = true;
            _logger?.LogInformation("Firebase inicializado correctamente");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al inicializar Firebase: {Message}", ex.Message);
            return false;
        }
    }
    
    /// <summary>
    /// Obtiene las credenciales de Google desde el archivo o variable de entorno
    /// </summary>
    private async Task<GoogleCredential?> GetCredentialAsync()
    {
        try
        {
            // Primero intentar con el archivo de credenciales configurado
            string credentialsPath = _config.GetCredentialsFullPath();
            
            if (!string.IsNullOrWhiteSpace(credentialsPath) && File.Exists(credentialsPath))
            {
                _logger?.LogInformation("Cargando credenciales desde: {Path}", credentialsPath);
                using var stream = File.OpenRead(credentialsPath);
                return await GoogleCredential.FromStreamAsync(stream, CancellationToken.None);
            }
            
            // Intentar con la variable de entorno
            var envPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (!string.IsNullOrWhiteSpace(envPath) && File.Exists(envPath))
            {
                _logger?.LogInformation("Cargando credenciales desde variable de entorno GOOGLE_APPLICATION_CREDENTIALS");
                using var stream = File.OpenRead(envPath);
                return await GoogleCredential.FromStreamAsync(stream, CancellationToken.None);
            }
            
            // Intentar obtener credenciales por defecto (útil en GCP)
            _logger?.LogInformation("Intentando obtener credenciales por defecto de Google Cloud");
            return await GoogleCredential.GetApplicationDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener credenciales de Google: {Message}", ex.Message);
            return null;
        }
    }
    
    /// <summary>
    /// Verifica la conexión con Firebase realizando una operación simple
    /// </summary>
    /// <returns>True si la conexión está activa</returns>
    public async Task<bool> TestConnectionAsync()
    {
        if (!_isInitialized || _firestoreDb == null)
        {
            return false;
        }
        
        try
        {
            // Intentar leer un documento de prueba
            var testRef = _firestoreDb.Collection("_connection_test").Document("test");
            await testRef.SetAsync(new { timestamp = DateTime.UtcNow, test = true });
            await testRef.DeleteAsync();
            
            _logger?.LogInformation("Conexión con Firebase verificada exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar conexión con Firebase: {Message}", ex.Message);
            return false;
        }
    }
    
    /// <summary>
    /// Obtiene una referencia a una colección de Firestore
    /// </summary>
    public CollectionReference? GetCollection(string collectionName)
    {
        return _firestoreDb?.Collection(collectionName);
    }
    
    /// <summary>
    /// Obtiene una referencia a un documento de Firestore
    /// </summary>
    public DocumentReference? GetDocument(string collectionName, string documentId)
    {
        return _firestoreDb?.Collection(collectionName).Document(documentId);
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            _storageClient?.Dispose();
            _firebaseApp?.Delete();
            
            _storageClient = null;
            _firebaseApp = null;
            _firestoreDb = null;
            _isInitialized = false;
        }
        
        _disposed = true;
    }
}
