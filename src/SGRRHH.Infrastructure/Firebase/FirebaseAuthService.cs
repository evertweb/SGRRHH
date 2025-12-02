using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
// Alias para evitar conflictos de nombres
using FirebaseAdminAuth = FirebaseAdmin.Auth.FirebaseAuth;
using AdminUserRecordArgs = FirebaseAdmin.Auth.UserRecordArgs;
using AdminAuthErrorCode = FirebaseAdmin.Auth.AuthErrorCode;
using AdminFirebaseAuthException = FirebaseAdmin.Auth.FirebaseAuthException;

namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Servicio de autenticación usando Firebase Auth.
/// Implementa IFirebaseAuthService para autenticación con Firebase.
/// </summary>
public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly FirebaseInitializer _firebase;
    private readonly FirebaseAuthClient _authClient;
    private readonly ILogger<FirebaseAuthService>? _logger;
    
    private const string USERS_COLLECTION = "users";
    
    public FirebaseAuthService(FirebaseInitializer firebase, ILogger<FirebaseAuthService>? logger = null)
    {
        _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        _logger = logger;
        
        // Configurar el cliente de autenticación de Firebase (para login desde cliente)
        var config = new FirebaseAuthConfig
        {
            ApiKey = _firebase.Config.ApiKey,
            AuthDomain = _firebase.Config.AuthDomain,
            Providers = new FirebaseAuthProvider[]
            {
                new EmailProvider()
            }
        };
        
        _authClient = new FirebaseAuthClient(config);
    }
    
    /// <summary>
    /// Autentica usando username o email. 
    /// Primero busca el usuario en Firestore, luego autentica con Firebase Auth.
    /// </summary>
    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Usuario y contraseña son requeridos"
            };
        }
        
        try
        {
            // Validar que el username tenga formato email completo (@sgrrhh.local)
            if (!username.Contains("@") || !username.EndsWith("@sgrrhh.local", StringComparison.OrdinalIgnoreCase))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Ingrese su usuario completo (ej: secretaria@sgrrhh.local)"
                };
            }
            
            string email = username.ToLower();
            
            // Intentar autenticar con Firebase Auth
            var result = await AuthenticateWithFirebaseAsync(email, password);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en autenticación para usuario: {Username}", username);
            return new AuthResult
            {
                Success = false,
                Message = GetFriendlyErrorMessage(ex)
            };
        }
    }
    
    /// <summary>
    /// Autenticación con Firebase Auth usando email y contraseña
    /// </summary>
    public async Task<FirebaseAuthResult> AuthenticateWithFirebaseAsync(string email, string password)
    {
        try
        {
            _logger?.LogInformation("Intentando autenticar con Firebase: {Email}", email);
            
            // Autenticar con Firebase Auth
            var userCredential = await _authClient.SignInWithEmailAndPasswordAsync(email, password);
            
            if (userCredential?.User == null)
            {
                return new FirebaseAuthResult
                {
                    Success = false,
                    Message = "Error al autenticar con Firebase"
                };
            }
            
            var firebaseUser = userCredential.User;
            _logger?.LogInformation("Usuario autenticado en Firebase: {Uid}", firebaseUser.Uid);
            
            // Obtener usuario de Firestore (NO crear automáticamente)
            var usuario = await GetUserFromFirestoreAsync(firebaseUser);
            
            if (usuario == null)
            {
                return new FirebaseAuthResult
                {
                    Success = false,
                    Message = "Usuario no autorizado. Contacte al administrador para obtener acceso al sistema."
                };
            }
            
            if (!usuario.Activo)
            {
                return new FirebaseAuthResult
                {
                    Success = false,
                    Message = "Usuario desactivado"
                };
            }
            
            // Actualizar último acceso
            await UpdateLastAccessAsync(firebaseUser.Uid);
            usuario.UltimoAcceso = DateTime.Now;
            
            // Obtener el token
            var token = await firebaseUser.GetIdTokenAsync();
            
            return new FirebaseAuthResult
            {
                Success = true,
                Message = "Autenticación exitosa",
                Usuario = usuario,
                FirebaseUid = firebaseUser.Uid,
                IdToken = token,
                RefreshToken = userCredential.User.Credential?.RefreshToken,
                ExpiresIn = 3600 // 1 hora por defecto
            };
        }
        catch (FirebaseAuthException ex)
        {
            _logger?.LogWarning("Error de Firebase Auth: {Reason}", ex.Reason);
            return new FirebaseAuthResult
            {
                Success = false,
                Message = GetFriendlyFirebaseError(ex.Reason)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en autenticación Firebase");
            return new FirebaseAuthResult
            {
                Success = false,
                Message = GetFriendlyErrorMessage(ex)
            };
        }
    }
    
    /// <summary>
    /// Obtiene el usuario de Firestore. NO crea usuarios automáticamente.
    /// El usuario debe existir tanto en Firebase Auth como en Firestore.
    /// </summary>
    private async Task<Usuario?> GetUserFromFirestoreAsync(User firebaseUser)
    {
        if (_firebase.Firestore == null) return null;
        
        try
        {
            var userRef = _firebase.Firestore.Collection(USERS_COLLECTION).Document(firebaseUser.Uid);
            var snapshot = await userRef.GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                return MapFirestoreToUsuario(snapshot);
            }
            
            // El usuario existe en Firebase Auth pero NO en Firestore
            // Esto significa que el administrador debe crear el documento en Firestore
            _logger?.LogWarning("Usuario {Uid} ({Email}) existe en Auth pero NO en Firestore. Debe ser registrado por un administrador.", 
                firebaseUser.Uid, firebaseUser.Info?.Email);
            
            return null; // No crear automáticamente
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuario de Firestore: {Uid}", firebaseUser.Uid);
            return null;
        }
    }
    
    /// <summary>
    /// Crea un documento de usuario en Firestore
    /// </summary>
    private async Task CreateUserInFirestoreAsync(string uid, Usuario usuario)
    {
        if (_firebase.Firestore == null) return;
        
        var data = new Dictionary<string, object?>
        {
            ["username"] = usuario.Username,
            ["nombreCompleto"] = usuario.NombreCompleto,
            ["email"] = usuario.Email ?? "",
            ["rol"] = usuario.Rol.ToString(),
            ["empleadoId"] = usuario.EmpleadoFirestoreId,
            ["ultimoAcceso"] = Timestamp.FromDateTime(DateTime.UtcNow),
            ["activo"] = usuario.Activo,
            ["fechaCreacion"] = Timestamp.FromDateTime(usuario.FechaCreacion.ToUniversalTime())
        };
        
        await _firebase.Firestore.Collection(USERS_COLLECTION).Document(uid).SetAsync(data);
        _logger?.LogInformation("Usuario creado en Firestore: {Uid}", uid);
    }
    
    /// <summary>
    /// Actualiza el último acceso del usuario
    /// </summary>
    private async Task UpdateLastAccessAsync(string uid)
    {
        if (_firebase.Firestore == null) return;
        
        try
        {
            await _firebase.Firestore.Collection(USERS_COLLECTION).Document(uid)
                .UpdateAsync("ultimoAcceso", Timestamp.FromDateTime(DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al actualizar último acceso para: {Uid}", uid);
        }
    }
    
    /// <summary>
    /// Crea un nuevo usuario en Firebase Auth
    /// </summary>
    public async Task<string?> CreateFirebaseUserAsync(string email, string password, string displayName)
    {
        try
        {
            var userRecordArgs = new AdminUserRecordArgs
            {
                Email = email,
                Password = password,
                DisplayName = displayName,
                EmailVerified = true, // Usuarios internos, no necesitan verificación
                Disabled = false
            };
            
            var userRecord = await FirebaseAdminAuth.DefaultInstance.CreateUserAsync(userRecordArgs);
            _logger?.LogInformation("Usuario creado en Firebase Auth: {Uid}", userRecord.Uid);
            
            return userRecord.Uid;
        }
        catch (AdminFirebaseAuthException ex)
        {
            _logger?.LogError(ex, "Error al crear usuario en Firebase Auth: {Email}", email);
            return null;
        }
    }
    
    /// <summary>
    /// Elimina un usuario de Firebase Auth
    /// </summary>
    public async Task<bool> DeleteFirebaseUserAsync(string uid)
    {
        try
        {
            await FirebaseAdminAuth.DefaultInstance.DeleteUserAsync(uid);
            _logger?.LogInformation("Usuario eliminado de Firebase Auth: {Uid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar usuario de Firebase Auth: {Uid}", uid);
            return false;
        }
    }
    
    /// <summary>
    /// Actualiza la contraseña de un usuario en Firebase Auth
    /// </summary>
    public async Task<bool> UpdatePasswordAsync(string uid, string newPassword)
    {
        try
        {
            var userRecordArgs = new AdminUserRecordArgs
            {
                Uid = uid,
                Password = newPassword
            };
            
            await FirebaseAdminAuth.DefaultInstance.UpdateUserAsync(userRecordArgs);
            _logger?.LogInformation("Contraseña actualizada para: {Uid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar contraseña: {Uid}", uid);
            return false;
        }
    }
    
    /// <summary>
    /// Migra un usuario local (SQLite) a Firebase Auth
    /// </summary>
    public async Task<string?> MigrateUserToFirebaseAsync(string username, string password, string nombreCompleto, RolUsuario rol)
    {
        try
        {
            // Convertir username a email
            string email = username.Contains("@") ? username : $"{username}@sgrrhh.local";
            
            // Verificar si el usuario ya existe en Firebase Auth
            try
            {
                var existingUser = await FirebaseAdminAuth.DefaultInstance.GetUserByEmailAsync(email);
                if (existingUser != null)
                {
                    _logger?.LogWarning("Usuario ya existe en Firebase: {Email}", email);
                    return existingUser.Uid;
                }
            }
            catch (AdminFirebaseAuthException ex) when (ex.AuthErrorCode == AdminAuthErrorCode.UserNotFound)
            {
                // Usuario no existe, lo crearemos
            }
            
            // Crear usuario en Firebase Auth
            var uid = await CreateFirebaseUserAsync(email, password, nombreCompleto);
            
            if (uid == null)
            {
                return null;
            }
            
            // Establecer rol como custom claim
            await SetUserRoleClaimAsync(uid, rol);
            
            // Crear documento en Firestore
            var usuario = new Usuario
            {
                Username = username,
                NombreCompleto = nombreCompleto,
                Email = email,
                FirebaseUid = uid,
                Rol = rol,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            
            await CreateUserInFirestoreAsync(uid, usuario);
            
            _logger?.LogInformation("Usuario migrado exitosamente: {Username} -> {Uid}", username, uid);
            return uid;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al migrar usuario: {Username}", username);
            return null;
        }
    }
    
    /// <summary>
    /// Establece claims personalizados (rol) para un usuario
    /// </summary>
    public async Task<bool> SetUserRoleClaimAsync(string uid, RolUsuario rol)
    {
        try
        {
            var claims = new Dictionary<string, object>
            {
                ["rol"] = rol.ToString(),
                ["rolId"] = (int)rol
            };
            
            await FirebaseAdminAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, claims);
            _logger?.LogInformation("Claims establecidos para {Uid}: rol={Rol}", uid, rol);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al establecer claims: {Uid}", uid);
            return false;
        }
    }
    
    /// <summary>
    /// Obtiene el usuario de Firestore por UID
    /// </summary>
    public async Task<Usuario?> GetUserByFirebaseUidAsync(string uid)
    {
        if (_firebase.Firestore == null) return null;
        
        try
        {
            var snapshot = await _firebase.Firestore.Collection(USERS_COLLECTION).Document(uid).GetSnapshotAsync();
            return snapshot.Exists ? MapFirestoreToUsuario(snapshot) : null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuario por UID: {Uid}", uid);
            return null;
        }
    }
    
    /// <summary>
    /// Obtiene el usuario por email
    /// </summary>
    public async Task<Usuario?> GetUserByEmailAsync(string email)
    {
        if (_firebase.Firestore == null) return null;
        
        try
        {
            var query = _firebase.Firestore.Collection(USERS_COLLECTION)
                .WhereEqualTo("email", email)
                .Limit(1);
            
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Count > 0 ? MapFirestoreToUsuario(snapshot.Documents[0]) : null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuario por email: {Email}", email);
            return null;
        }
    }
    
    /// <summary>
    /// Cierra la sesión actual
    /// </summary>
    public Task SignOutAsync()
    {
        try
        {
            _authClient.SignOut();
            _logger?.LogInformation("Sesión cerrada");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cerrar sesión");
        }
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Verifica si Firebase Auth está disponible
    /// </summary>
    public async Task<bool> IsFirebaseAvailableAsync()
    {
        try
        {
            if (!_firebase.IsInitialized)
            {
                await _firebase.InitializeAsync();
            }
            return _firebase.IsInitialized && _firebase.Firestore != null;
        }
        catch
        {
            return false;
        }
    }
    
    // ========================
    // Métodos de IAuthService (compatibilidad)
    // ========================
    
    public string HashPassword(string password)
    {
        // Firebase Auth maneja los hashes internamente
        // Este método se mantiene por compatibilidad
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        // Firebase Auth maneja la verificación internamente
        // Este método se mantiene por compatibilidad
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
    
    public Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        // Para Firebase, necesitamos el UID, no el userId int
        // Este método se mantiene por compatibilidad con la interfaz
        _logger?.LogWarning("ChangePasswordAsync con int userId no soportado en Firebase. Use ChangePasswordByUidAsync");
        return Task.FromResult(false);
    }
    
    /// <summary>
    /// Cambia la contraseña usando el UID de Firebase
    /// </summary>
    public async Task<bool> ChangePasswordByUidAsync(string uid, string currentPassword, string newPassword)
    {
        try
        {
            // Primero verificar la contraseña actual autenticando
            var user = await GetUserByFirebaseUidAsync(uid);
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return false;
            }
            
            // Intentar autenticar con la contraseña actual
            var authResult = await AuthenticateWithFirebaseAsync(user.Email, currentPassword);
            if (!authResult.Success)
            {
                return false;
            }
            
            // Actualizar la contraseña
            return await UpdatePasswordAsync(uid, newPassword);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al cambiar contraseña por UID: {Uid}", uid);
            return false;
        }
    }
    
    // ========================
    // Métodos auxiliares
    // ========================
    
    private Usuario MapFirestoreToUsuario(DocumentSnapshot snapshot)
    {
        var data = snapshot.ToDictionary();
        
        var usuario = new Usuario
        {
            FirebaseUid = snapshot.Id,
            Username = data.GetValueOrDefault("username")?.ToString() ?? "",
            NombreCompleto = data.GetValueOrDefault("nombreCompleto")?.ToString() ?? "",
            Email = data.GetValueOrDefault("email")?.ToString(),
            Activo = data.GetValueOrDefault("activo") as bool? ?? true,
            EmpleadoFirestoreId = data.GetValueOrDefault("empleadoId")?.ToString()
        };
        
        // Parsear rol
        var rolString = data.GetValueOrDefault("rol")?.ToString();
        if (!string.IsNullOrEmpty(rolString) && Enum.TryParse<RolUsuario>(rolString, out var rol))
        {
            usuario.Rol = rol;
        }
        
        // Parsear fechas
        if (data.TryGetValue("ultimoAcceso", out var ultimoAcceso) && ultimoAcceso is Timestamp ts)
        {
            usuario.UltimoAcceso = ts.ToDateTime().ToLocalTime();
        }
        
        if (data.TryGetValue("fechaCreacion", out var fechaCreacion) && fechaCreacion is Timestamp tsCreacion)
        {
            usuario.FechaCreacion = tsCreacion.ToDateTime().ToLocalTime();
        }
        
        return usuario;
    }
    
    private string ExtractUsernameFromEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return "user";
        var atIndex = email.IndexOf('@');
        return atIndex > 0 ? email.Substring(0, atIndex) : email;
    }
    
    private RolUsuario DetermineRoleFromEmail(string email)
    {
        var username = ExtractUsernameFromEmail(email).ToLower();
        
        return username switch
        {
            "admin" => RolUsuario.Administrador,
            "ingeniera" => RolUsuario.Aprobador,
            "secretaria" => RolUsuario.Operador,
            _ => RolUsuario.Operador // Rol por defecto
        };
    }
    
    private string GetFriendlyFirebaseError(AuthErrorReason reason)
    {
        return reason switch
        {
            AuthErrorReason.WrongPassword => "Contraseña incorrecta",
            AuthErrorReason.UserNotFound => "Usuario no encontrado",
            AuthErrorReason.InvalidEmailAddress => "Correo electrónico inválido",
            AuthErrorReason.UserDisabled => "Usuario desactivado",
            AuthErrorReason.TooManyAttemptsTryLater => "Demasiados intentos. Intente más tarde",
            AuthErrorReason.WeakPassword => "La contraseña es muy débil",
            AuthErrorReason.EmailExists => "El correo electrónico ya está en uso",
            AuthErrorReason.OperationNotAllowed => "Operación no permitida",
            AuthErrorReason.Unknown => "Error desconocido de autenticación",
            _ => $"Error de autenticación: {reason}"
        };
    }
    
    private string GetFriendlyErrorMessage(Exception ex)
    {
        if (ex is FirebaseAuthException firebaseEx)
        {
            return GetFriendlyFirebaseError(firebaseEx.Reason);
        }

        if (ex.Message.Contains("network") || ex.Message.Contains("connection"))
        {
            return "Error de conexión. Verifique su conexión a internet.";
        }

        return $"Error al autenticar: {ex.Message}";
    }

    #region Métodos para Windows Hello / Passkeys

    /// <summary>
    /// Autentica usando Windows Hello / Passkey
    /// </summary>
    public async Task<FirebaseAuthResult> AuthenticateWithPasskeyAsync(string email, string credentialId)
    {
        try
        {
            // 1. Buscar usuario por email
            var usuario = await GetUserByEmailAsync(email);
            if (usuario == null || string.IsNullOrEmpty(usuario.FirebaseUid))
            {
                return new FirebaseAuthResult
                {
                    Success = false,
                    Message = "Usuario no encontrado"
                };
            }

            // 2. Verificar que la credencial existe y está activa en Firestore
            var passkeyDoc = await _firebase.Firestore
                .Collection(USERS_COLLECTION)
                .Document(usuario.FirebaseUid)
                .Collection("passkeys")
                .Document(credentialId)
                .GetSnapshotAsync();

            if (!passkeyDoc.Exists)
            {
                return new FirebaseAuthResult
                {
                    Success = false,
                    Message = "Credencial no autorizada. Contacte al administrador."
                };
            }

            var passkeyData = passkeyDoc.ToDictionary();
            var isActive = passkeyData.GetValueOrDefault("isActive") as bool? ?? false;

            if (!isActive)
            {
                return new FirebaseAuthResult
                {
                    Success = false,
                    Message = "Esta credencial ha sido revocada"
                };
            }

            // 3. Generar Custom Token con Firebase Admin SDK
            var customToken = await FirebaseAdminAuth.DefaultInstance.CreateCustomTokenAsync(
                usuario.FirebaseUid,
                new Dictionary<string, object>
                {
                    ["authMethod"] = "windows_hello",
                    ["credentialId"] = credentialId
                }
            );

            // 4. Actualizar último acceso y último uso de passkey
            await UpdateLastAccessAsync(usuario.FirebaseUid);
            await UpdatePasskeyLastUsedAsync(usuario.FirebaseUid, credentialId);

            _logger?.LogInformation("Autenticación con passkey exitosa: {Email}", email);

            // Nota: FirebaseAuthClient no soporta SignInWithCustomToken
            // El custom token se puede usar directamente en las aplicaciones cliente
            // Para efectos de esta aplicación, consideramos la autenticación exitosa
            // ya que validamos la passkey en Firestore
            return new FirebaseAuthResult
            {
                Success = true,
                Message = "Autenticación exitosa",
                Usuario = usuario,
                FirebaseUid = usuario.FirebaseUid,
                IdToken = customToken, // El custom token sirve como IdToken
                RefreshToken = null, // No tenemos refresh token en este flujo
                ExpiresIn = 3600
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en autenticación con passkey");
            return new FirebaseAuthResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Registra una nueva passkey para un usuario
    /// </summary>
    public async Task<bool> RegisterPasskeyAsync(string firebaseUid, string credentialId, string deviceName)
    {
        try
        {
            var passkey = new Dictionary<string, object>
            {
                ["credentialId"] = credentialId,
                ["deviceName"] = deviceName,
                ["createdAt"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["lastUsedAt"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["isActive"] = true
            };

            await _firebase.Firestore
                .Collection(USERS_COLLECTION)
                .Document(firebaseUid)
                .Collection("passkeys")
                .Document(credentialId)
                .SetAsync(passkey);

            _logger?.LogInformation("Passkey registrada: {Uid}/{CredentialId}", firebaseUid, credentialId);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al registrar passkey");
            return false;
        }
    }

    /// <summary>
    /// Obtiene la lista de passkeys registradas para un usuario
    /// </summary>
    public async Task<List<SGRRHH.Core.Models.PasskeyInfo>> GetUserPasskeysAsync(string firebaseUid)
    {
        try
        {
            var snapshot = await _firebase.Firestore
                .Collection(USERS_COLLECTION)
                .Document(firebaseUid)
                .Collection("passkeys")
                .WhereEqualTo("isActive", true)
                .GetSnapshotAsync();

            var passkeys = new List<SGRRHH.Core.Models.PasskeyInfo>();

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();

                var createdAtValue = data.GetValueOrDefault("createdAt");
                var lastUsedAtValue = data.GetValueOrDefault("lastUsedAt");

                passkeys.Add(new SGRRHH.Core.Models.PasskeyInfo
                {
                    CredentialId = doc.Id,
                    DeviceName = data.GetValueOrDefault("deviceName")?.ToString() ?? "Unknown",
                    CreatedAt = createdAtValue is Timestamp createdAt ? createdAt.ToDateTime() : null,
                    LastUsedAt = lastUsedAtValue is Timestamp lastUsedAt ? lastUsedAt.ToDateTime() : null,
                    IsCurrent = false // Se establece fuera de este método
                });
            }

            return passkeys;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener passkeys del usuario");
            return new List<SGRRHH.Core.Models.PasskeyInfo>();
        }
    }

    /// <summary>
    /// Revoca (desactiva) una passkey
    /// </summary>
    public async Task<bool> RevokePasskeyAsync(string firebaseUid, string credentialId)
    {
        try
        {
            await _firebase.Firestore
                .Collection(USERS_COLLECTION)
                .Document(firebaseUid)
                .Collection("passkeys")
                .Document(credentialId)
                .UpdateAsync("isActive", false);

            _logger?.LogInformation("Passkey revocada: {Uid}/{CredentialId}", firebaseUid, credentialId);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al revocar passkey");
            return false;
        }
    }

    /// <summary>
    /// Actualiza el timestamp de último uso de una passkey
    /// </summary>
    private async Task UpdatePasskeyLastUsedAsync(string uid, string credentialId)
    {
        try
        {
            await _firebase.Firestore
                .Collection(USERS_COLLECTION)
                .Document(uid)
                .Collection("passkeys")
                .Document(credentialId)
                .UpdateAsync("lastUsedAt", Timestamp.FromDateTime(DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al actualizar lastUsedAt de passkey");
        }
    }

    #endregion
}
