namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Configuración de Firebase para la aplicación SGRRHH.
/// Estos valores se cargan desde appsettings.json
/// </summary>
public class FirebaseConfig
{
    /// <summary>
    /// ID del proyecto de Firebase (ej: "sgrrhh-app-xxxxx")
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
    
    /// <summary>
    /// Bucket de Firebase Storage (ej: "sgrrhh-app-xxxxx.appspot.com")
    /// </summary>
    public string StorageBucket { get; set; } = string.Empty;
    
    /// <summary>
    /// ID de la base de datos Firestore (ej: "(default)" o nombre personalizado)
    /// Si está vacío, se usa "(default)"
    /// </summary>
    public string DatabaseId { get; set; } = "(default)";
    
    /// <summary>
    /// API Key de Firebase (para autenticación de cliente)
    /// Se obtiene en Firebase Console > Configuración del proyecto > General
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Ruta al archivo de credenciales del service account (JSON)
    /// Si está vacío, se busca en la variable de entorno GOOGLE_APPLICATION_CREDENTIALS
    /// </summary>
    public string CredentialsPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Auth Domain de Firebase (ej: "sgrrhh-app-xxxxx.firebaseapp.com")
    /// </summary>
    public string AuthDomain { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si Firebase está habilitado.
    /// Si es false, se usará SQLite como fallback.
    /// </summary>
    public bool Enabled { get; set; } = false;
    
    /// <summary>
    /// Valida que la configuración esté completa
    /// </summary>
    public bool IsValid()
    {
        return Enabled 
            && !string.IsNullOrWhiteSpace(ProjectId)
            && !string.IsNullOrWhiteSpace(StorageBucket)
            && !string.IsNullOrWhiteSpace(ApiKey);
    }
    
    /// <summary>
    /// Obtiene la ruta completa del archivo de credenciales
    /// </summary>
    public string GetCredentialsFullPath()
    {
        if (string.IsNullOrWhiteSpace(CredentialsPath))
            return string.Empty;
            
        // Si es una ruta relativa, convertirla a absoluta
        if (!Path.IsPathRooted(CredentialsPath))
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CredentialsPath);
        }
        
        return CredentialsPath;
    }
}
