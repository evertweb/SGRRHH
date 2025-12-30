using System.Text.Json.Serialization;

namespace SGRRHH.Web.Client;

/// <summary>
/// Configuración de Firebase para cliente web.
/// SOLO contiene datos PÚBLICOS que pueden exponerse en el navegador.
/// NO incluye Service Account ni claves privadas.
/// </summary>
public class FirebaseWebConfig
{
    /// <summary>
    /// API Key pública de Firebase (visible en cualquier app web)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Auth Domain (ej: "proyecto.firebaseapp.com")
    /// </summary>
    public string AuthDomain { get; set; } = string.Empty;
    
    /// <summary>
    /// Project ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
    
    /// <summary>
    /// Storage Bucket
    /// </summary>
    public string StorageBucket { get; set; } = string.Empty;
    
    /// <summary>
    /// Messaging Sender ID
    /// </summary>
    public string MessagingSenderId { get; set; } = string.Empty;
    
    /// <summary>
    /// App ID
    /// </summary>
    public string AppId { get; set; } = string.Empty;
    
    /// <summary>
    /// Database ID de Firestore (opcional, default si no se especifica)
    /// </summary>
    public string? DatabaseId { get; set; }
    
    /// <summary>
    /// Genera el objeto de configuración para Firebase JS SDK.
    /// Usa una clase concreta en lugar de tipo anónimo para evitar problemas de trimming.
    /// </summary>
    public FirebaseJsConfig ToJsConfig() => new()
    {
        apiKey = ApiKey,
        authDomain = AuthDomain,
        projectId = ProjectId,
        storageBucket = StorageBucket,
        messagingSenderId = MessagingSenderId,
        appId = AppId,
        databaseId = DatabaseId
    };
    
    /// <summary>
    /// Valida que la configuración mínima esté presente
    /// </summary>
    public bool IsValid() => 
        !string.IsNullOrWhiteSpace(ApiKey) && 
        !string.IsNullOrWhiteSpace(ProjectId);
}

/// <summary>
/// Clase concreta para la configuración JS de Firebase.
/// Evita problemas de IL Trimming con tipos anónimos en Blazor WASM.
/// </summary>
public class FirebaseJsConfig
{
    [JsonPropertyName("apiKey")]
    public string apiKey { get; set; } = string.Empty;
    
    [JsonPropertyName("authDomain")]
    public string authDomain { get; set; } = string.Empty;
    
    [JsonPropertyName("projectId")]
    public string projectId { get; set; } = string.Empty;
    
    [JsonPropertyName("storageBucket")]
    public string storageBucket { get; set; } = string.Empty;
    
    [JsonPropertyName("messagingSenderId")]
    public string messagingSenderId { get; set; } = string.Empty;
    
    [JsonPropertyName("appId")]
    public string appId { get; set; } = string.Empty;
    
    [JsonPropertyName("databaseId")]
    public string? databaseId { get; set; }
}
