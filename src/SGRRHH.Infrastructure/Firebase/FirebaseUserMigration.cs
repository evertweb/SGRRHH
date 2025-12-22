using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Enums;
// Alias para evitar conflictos de nombres
using FirebaseAdminAuth = FirebaseAdmin.Auth.FirebaseAuth;
using AdminUserRecordArgs = FirebaseAdmin.Auth.UserRecordArgs;
using AdminAuthErrorCode = FirebaseAdmin.Auth.AuthErrorCode;
using AdminFirebaseAuthException = FirebaseAdmin.Auth.FirebaseAuthException;

namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Clase para migrar usuarios de SQLite a Firebase Auth/Firestore.
/// También inicializa los usuarios por defecto si no existen.
/// </summary>
public class FirebaseUserMigration
{
    private readonly FirebaseInitializer _firebase;
    private readonly ILogger<FirebaseUserMigration>? _logger;
    
    private const string USERS_COLLECTION = "users";
    
    /// <summary>
    /// Usuarios por defecto ya no se definen en código por seguridad.
    /// Los usuarios deben crearse manualmente en Firebase Console o mediante
    /// el panel de administración de la aplicación.
    /// 
    /// NOTA: Las credenciales anteriores (admin123, secretaria123, ingeniera123)
    /// fueron eliminadas del código fuente. Si los usuarios aún existen en
    /// Firebase con esas contraseñas, se recomienda cambiarlas.
    /// </summary>
    public static readonly (string Username, string Password, string NombreCompleto, RolUsuario Rol)[] DefaultUsers = Array.Empty<(string, string, string, RolUsuario)>();
    
    public FirebaseUserMigration(FirebaseInitializer firebase, ILogger<FirebaseUserMigration>? logger = null)
    {
        _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        _logger = logger;
    }
    
    /// <summary>
    /// Asegura que los usuarios por defecto existan en Firebase Auth y Firestore
    /// </summary>
    public async Task<bool> EnsureDefaultUsersExistAsync()
    {
        if (!_firebase.IsInitialized)
        {
            _logger?.LogWarning("Firebase no está inicializado. Inicializando...");
            var initialized = await _firebase.InitializeAsync();
            if (!initialized)
            {
                _logger?.LogError("No se pudo inicializar Firebase");
                return false;
            }
        }
        
        _logger?.LogInformation("Verificando usuarios por defecto en Firebase...");
        
        int usersCreated = 0;
        int usersExisting = 0;
        
        foreach (var (username, password, nombreCompleto, rol) in DefaultUsers)
        {
            try
            {
                var email = $"{username}@sgrrhh.local";
                bool exists = await UserExistsInFirebaseAsync(email);
                
                if (exists)
                {
                    _logger?.LogInformation("Usuario {Username} ya existe en Firebase", username);
                    usersExisting++;
                }
                else
                {
                    _logger?.LogInformation("Creando usuario {Username} en Firebase...", username);
                    var uid = await CreateUserAsync(email, password, nombreCompleto, rol);
                    
                    if (!string.IsNullOrEmpty(uid))
                    {
                        usersCreated++;
                        _logger?.LogInformation("Usuario {Username} creado exitosamente: {Uid}", username, uid);
                    }
                    else
                    {
                        _logger?.LogError("Error al crear usuario {Username}", username);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error procesando usuario {Username}", username);
            }
        }
        
        _logger?.LogInformation("Migración completada. Usuarios creados: {Created}, existentes: {Existing}", 
            usersCreated, usersExisting);
        
        return true;
    }
    
    /// <summary>
    /// Verifica si un usuario existe en Firebase Auth
    /// </summary>
    private async Task<bool> UserExistsInFirebaseAsync(string email)
    {
        try
        {
            await FirebaseAdminAuth.DefaultInstance.GetUserByEmailAsync(email);
            return true;
        }
        catch (AdminFirebaseAuthException ex) when (ex.AuthErrorCode == AdminAuthErrorCode.UserNotFound)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Crea un usuario en Firebase Auth y Firestore
    /// </summary>
    private async Task<string?> CreateUserAsync(string email, string password, string nombreCompleto, RolUsuario rol)
    {
        try
        {
            // Crear en Firebase Auth
            var userRecordArgs = new AdminUserRecordArgs
            {
                Email = email,
                Password = password,
                DisplayName = nombreCompleto,
                EmailVerified = true,
                Disabled = false
            };
            
            var userRecord = await FirebaseAdminAuth.DefaultInstance.CreateUserAsync(userRecordArgs);
            var uid = userRecord.Uid;
            
            // Establecer custom claims (rol)
            var claims = new Dictionary<string, object>
            {
                ["rol"] = rol.ToString(),
                ["rolId"] = (int)rol
            };
            await FirebaseAdminAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, claims);
            
            // Crear documento en Firestore
            if (_firebase.Firestore != null)
            {
                var username = email.Split('@')[0];
                var userData = new Dictionary<string, object?>
                {
                    ["username"] = username,
                    ["nombreCompleto"] = nombreCompleto,
                    ["email"] = email,
                    ["rol"] = rol.ToString(),
                    ["empleadoId"] = null,
                    ["ultimoAcceso"] = null,
                    ["activo"] = true,
                    ["fechaCreacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                };
                
                await _firebase.Firestore.Collection(USERS_COLLECTION).Document(uid).SetAsync(userData);
            }
            
            return uid;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al crear usuario: {Email}", email);
            return null;
        }
    }
    
    /// <summary>
    /// Migra un usuario específico de SQLite a Firebase
    /// </summary>
    public async Task<string?> MigrateUserAsync(string username, string nombreCompleto, string? email, RolUsuario rol, string password)
    {
        var targetEmail = email ?? $"{username}@sgrrhh.local";
        
        // Verificar si ya existe
        if (await UserExistsInFirebaseAsync(targetEmail))
        {
            _logger?.LogWarning("Usuario {Email} ya existe en Firebase", targetEmail);
            
            // Retornar el UID existente
            var existingUser = await FirebaseAdminAuth.DefaultInstance.GetUserByEmailAsync(targetEmail);
            return existingUser.Uid;
        }
        
        return await CreateUserAsync(targetEmail, password, nombreCompleto, rol);
    }
    
    /// <summary>
    /// Lista todos los usuarios en Firebase Firestore
    /// </summary>
    public async Task<List<(string Uid, string Username, string Email, RolUsuario Rol)>> ListFirebaseUsersAsync()
    {
        var users = new List<(string Uid, string Username, string Email, RolUsuario Rol)>();
        
        if (_firebase.Firestore == null) return users;
        
        try
        {
            var snapshot = await _firebase.Firestore.Collection(USERS_COLLECTION).GetSnapshotAsync();
            
            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();
                var username = data.GetValueOrDefault("username")?.ToString() ?? "";
                var email = data.GetValueOrDefault("email")?.ToString() ?? "";
                var rolString = data.GetValueOrDefault("rol")?.ToString() ?? "Operador";
                
                Enum.TryParse<RolUsuario>(rolString, out var rol);
                
                users.Add((doc.Id, username, email, rol));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al listar usuarios de Firebase");
        }
        
        return users;
    }
    
    /// <summary>
    /// Elimina todos los usuarios de prueba (usar con cuidado!)
    /// </summary>
    public async Task<bool> DeleteAllTestUsersAsync()
    {
        if (_firebase.Firestore == null) return false;
        
        try
        {
            foreach (var (username, _, _, _) in DefaultUsers)
            {
                var email = $"{username}@sgrrhh.local";
                try
                {
                    var user = await FirebaseAdminAuth.DefaultInstance.GetUserByEmailAsync(email);
                    
                    // Eliminar de Firestore
                    await _firebase.Firestore.Collection(USERS_COLLECTION).Document(user.Uid).DeleteAsync();
                    
                    // Eliminar de Firebase Auth
                    await FirebaseAdminAuth.DefaultInstance.DeleteUserAsync(user.Uid);
                    
                    _logger?.LogInformation("Usuario eliminado: {Email}", email);
                }
                catch (AdminFirebaseAuthException ex) when (ex.AuthErrorCode == AdminAuthErrorCode.UserNotFound)
                {
                    _logger?.LogInformation("Usuario no existe: {Email}", email);
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar usuarios de prueba");
            return false;
        }
    }
}
