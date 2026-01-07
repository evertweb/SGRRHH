using Microsoft.JSInterop;

namespace SGRRHH.Web.Client;

/// <summary>
/// Interoperabilidad con Firebase JS SDK.
/// Toda la comunicación con Firebase pasa por aquí.
/// </summary>
public class FirebaseJsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _firebaseModule;
    private bool _isInitialized;
    
    public bool IsInitialized => _isInitialized;
    
    public FirebaseJsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    /// <summary>
    /// Inicializa Firebase con la configuración pública
    /// </summary>
    public async Task InitializeAsync(FirebaseWebConfig config)
    {
        if (_isInitialized) return;
        
        _firebaseModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/firebase-interop.js");
        
        await _firebaseModule.InvokeVoidAsync("initializeFirebase", config.ToJsConfig());
        _isInitialized = true;
    }
    
    /// <summary>
    /// Autenticación con email/password
    /// </summary>
    public async Task<FirebaseJsAuthResult> SignInWithEmailAsync(string email, string password)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<FirebaseJsAuthResult>(
            "signInWithEmail", email, password);
    }
    
    /// <summary>
    /// Cierra sesión
    /// </summary>
    public async Task SignOutAsync()
    {
        if (_firebaseModule != null)
            await _firebaseModule.InvokeVoidAsync("signOut");
    }
    
    /// <summary>
    /// Obtiene el usuario actual autenticado
    /// </summary>
    public async Task<FirebaseJsAuthResult?> GetCurrentUserAsync()
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<FirebaseJsAuthResult?>("getCurrentUser");
    }

    public async Task<object?> PostWithCredentialsAsync(string url, object payload)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<object?>("postWithCredentials", url, payload);
    }
    /// <summary>
    /// Registra un callback .NET para que JS invoque onAuthStateChanged
    /// dotNetRef debe ser un DotNetObjectReference de un objeto con el método
    /// public Task NotifyAuthStateChanged(object? payload)
    /// </summary>
    public async Task RegisterAuthStateCallbackAsync(DotNetObjectReference<object> dotNetRef)
    {
        EnsureInitialized();
        await _firebaseModule!.InvokeVoidAsync("registerAuthStateChangedCallback", dotNetRef);
    }

    /// <summary>
    /// Desregistra el callback (.NET -> JS)
    /// </summary>
    public async Task UnregisterAuthStateCallbackAsync()
    {
        if (_isInitialized && _firebaseModule != null)
        {
            try
            {
                await _firebaseModule.InvokeVoidAsync("unregisterAuthStateChangedCallback");
            }
            catch { }
        }
    }
    
    /// <summary>
    /// Obtiene documentos de una colección
    /// </summary>
    public async Task<List<T>> GetCollectionAsync<T>(string collectionPath)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<List<T>>(
            "getCollection", collectionPath);
    }
    
    /// <summary>
    /// Obtiene un documento por ID
    /// </summary>
    public async Task<T?> GetDocumentAsync<T>(string collectionPath, string documentId)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<T?>(
            "getDocument", collectionPath, documentId);
    }
    
    /// <summary>
    /// Guarda un documento
    /// </summary>
    public async Task SetDocumentAsync<T>(string collectionPath, string documentId, T data)
    {
        EnsureInitialized();
        await _firebaseModule!.InvokeVoidAsync("setDocument", collectionPath, documentId, data);
    }
    
    /// <summary>
    /// Agrega un documento con ID auto-generado
    /// </summary>
    public async Task<string> AddDocumentAsync<T>(string collectionPath, T data)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<string>("addDocument", collectionPath, data);
    }
    
    /// <summary>
    /// Elimina un documento
    /// </summary>
    public async Task DeleteDocumentAsync(string collectionPath, string documentId)
    {
        EnsureInitialized();
        await _firebaseModule!.InvokeVoidAsync("deleteDocument", collectionPath, documentId);
    }
    
    /// <summary>
    /// Consulta documentos con filtro
    /// </summary>
    public async Task<List<T>> QueryCollectionAsync<T>(string collectionPath, string field, string op, object value)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<List<T>>(
            "queryCollection", collectionPath, field, op, value);
    }

    /// <summary>
    /// Consulta documentos con ordenamiento y límite
    /// </summary>
    public async Task<List<T>> QueryCollectionOrderedAsync<T>(string collectionPath, string orderByField, string direction = "asc", int limit = 0)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<List<T>>(
            "queryCollectionOrdered", collectionPath, orderByField, direction, limit);
    }

    /// <summary>
    /// Consulta documentos con múltiples filtros (composite query)
    /// </summary>
    public async Task<List<T>> QueryCollectionCompositeAsync<T>(string collectionPath, List<FirestoreFilter> filters, string? orderByField = null, string direction = "asc", int limit = 0)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<List<T>>(
            "queryCollectionComposite", collectionPath, filters, orderByField, direction, limit);
    }
    
    // ===== FIREBASE STORAGE METHODS =====
    
    /// <summary>
    /// Sube un archivo a Firebase Storage desde Base64
    /// </summary>
    public async Task<FirebaseStorageResult> UploadFileBase64Async(string storagePath, string base64Data, string contentType)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<FirebaseStorageResult>(
            "uploadFileBase64", storagePath, base64Data, contentType);
    }
    
    /// <summary>
    /// Obtiene la URL de descarga de un archivo
    /// </summary>
    public async Task<string?> GetStorageUrlAsync(string storagePath)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<string?>("getStorageUrl", storagePath);
    }
    
    /// <summary>
    /// Elimina un archivo de Firebase Storage
    /// </summary>
    public async Task<bool> DeleteStorageFileAsync(string storagePath)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<bool>("deleteStorageFile", storagePath);
    }
    
    /// <summary>
    /// Descarga un archivo de Firebase Storage como Base64
    /// </summary>
    public async Task<FirebaseDownloadResult> DownloadFileBase64Async(string storagePath)
    {
        EnsureInitialized();
        return await _firebaseModule!.InvokeAsync<FirebaseDownloadResult>(
            "downloadFileBase64", storagePath);
    }
    
    private void EnsureInitialized()
    {
        if (!_isInitialized || _firebaseModule == null)
            throw new InvalidOperationException("Firebase no inicializado. Llame InitializeAsync primero.");
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_firebaseModule != null)
        {
            await _firebaseModule.DisposeAsync();
            _firebaseModule = null;
        }
        _isInitialized = false;
    }
}

/// <summary>
/// Resultado de autenticación de Firebase (desde JS).
/// Usa clase en lugar de record posicional para evitar problemas de IL Trimming.
/// </summary>
public class FirebaseJsAuthResult
{
    [System.Text.Json.Serialization.JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("idToken")]
    public string? IdToken { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Resultado de subida de archivo a Firebase Storage (desde JS).
/// </summary>
public class FirebaseStorageResult
{
    [System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("downloadUrl")]
    public string? DownloadUrl { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("fullPath")]
    public string? FullPath { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Resultado de descarga de archivo de Firebase Storage (desde JS).
/// </summary>
public class FirebaseDownloadResult
{
    [System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("base64Data")]
    public string? Base64Data { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("contentType")]
    public string? ContentType { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("size")]
    public long Size { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Filtro para consultar Firestore
/// </summary>
public class FirestoreFilter
{
    [System.Text.Json.Serialization.JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("op")]
    public string Op { get; set; } = "==";

    [System.Text.Json.Serialization.JsonPropertyName("value")]
    public object? Value { get; set; }

    public FirestoreFilter() { }
    public FirestoreFilter(string field, string op, object? value)
    {
        Field = field;
        Op = op;
        Value = value;
    }
}

