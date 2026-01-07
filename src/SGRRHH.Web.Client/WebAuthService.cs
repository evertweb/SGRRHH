using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using FirebaseAuthResult = SGRRHH.Core.Interfaces.FirebaseAuthResult;


namespace SGRRHH.Web.Client;

/// <summary>
/// Servicio de autenticación para Blazor WASM usando Firebase JS SDK.
/// </summary>
public class WebAuthService : IFirebaseAuthService
{
    private readonly FirebaseJsInterop _firebase;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly HttpClient _http;
    private readonly string _authServerBaseUrl;

    public WebAuthService(FirebaseJsInterop firebase, IUsuarioRepository usuarioRepository, HttpClient http, string authServerBaseUrl)
    {
        _firebase = firebase;
        _usuarioRepository = usuarioRepository;
        _http = http;
        _authServerBaseUrl = authServerBaseUrl ?? string.Empty;
    }

    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        string email = username.Contains("@") ? username : $"{username}@sgrrhh.local";
        return await AuthenticateWithFirebaseAsync(email, password);
    }

    public async Task<FirebaseAuthResult> AuthenticateWithFirebaseAsync(string email, string password)
    {
        try
        {
            var authData = await _firebase.SignInWithEmailAsync(email, password);
            
            if (authData == null || !authData.Success)
            {
                return new FirebaseAuthResult { Success = false, Message = authData?.ErrorMessage ?? "Credenciales inválidas" };
            }

            var usuario = await GetUserByFirebaseUidAsync(authData.Uid);
            
            if (usuario == null)
            {
                await SignOutAsync();
                return new FirebaseAuthResult { Success = false, Message = "Usuario no encontrado en la base de datos" };
            }

            if (!usuario.Activo)
            {
                await SignOutAsync();
                return new FirebaseAuthResult { Success = false, Message = "Usuario desactivado" };
            }

            var result = new FirebaseAuthResult
            {
                Success = true,
                Message = "Login exitoso",
                Usuario = usuario,
                FirebaseUid = authData.Uid,
                IdToken = authData.IdToken
            };

            // If AuthServer base URL is configured, send idToken to create server session
            try
            {
                if (!string.IsNullOrEmpty(_authServerBaseUrl) && !string.IsNullOrEmpty(result.IdToken))
                {
                    var loginUrl = new Uri(new Uri(_authServerBaseUrl), "api/auth/sessionLogin").ToString();
                    var payload = new { IdToken = result.IdToken };
                    // Use FirebaseJsInterop helper that does a fetch with credentials: include to receive cookies cross-origin
                    try
                    {
                        await _firebase.PostWithCredentialsAsync(loginUrl, payload);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Session cookie creation (fetch) failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log/ignore: shouldn't block login on session cookie failure
                Console.WriteLine($"Session cookie creation failed: {ex.Message}");
            }

            return result;        }
        catch (Exception ex)
        {
            return new FirebaseAuthResult { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<Usuario?> GetUserByFirebaseUidAsync(string uid)
    {
        if (_usuarioRepository is IFirestoreRepository<Usuario> firestoreRepo)
        {
            return await firestoreRepo.GetByDocumentIdAsync(uid);
        }
        return null;
    }

    public async Task<Usuario?> GetUserByEmailAsync(string email)
    {
        return await _usuarioRepository.GetByUsernameAsync(email);
    }

    public async Task SignOutAsync()
    {
        // Sign out from client Firebase SDK
        await _firebase.SignOutAsync();

        // Try to notify server to invalidate session cookie
        try
        {
            if (!string.IsNullOrEmpty(_authServerBaseUrl))
            {
                var logoutUrl = new Uri(new Uri(_authServerBaseUrl), "api/auth/sessionLogout").ToString();
                await _firebase.PostWithCredentialsAsync(logoutUrl, new { });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server session logout failed: {ex.Message}");
        }
    }

    public async Task<bool> IsFirebaseAvailableAsync() => true;

    public Task<string?> CreateFirebaseUserAsync(string email, string password, string displayName) => Task.FromResult<string?>(null);
    public Task<bool> DeleteFirebaseUserAsync(string uid) => Task.FromResult(false);
    public Task<bool> UpdatePasswordAsync(string uid, string newPassword) => Task.FromResult(false);
    public Task<string?> MigrateUserToFirebaseAsync(string username, string password, string nombreCompleto, RolUsuario rol) => Task.FromResult<string?>(null);
    public Task<bool> SetUserRoleClaimAsync(string uid, RolUsuario rol) => Task.FromResult(false);
    
    public async Task<FirebaseAuthResult> AuthenticateWithPasskeyAsync(string email, string credentialId) 
        => new FirebaseAuthResult { Success = false, Message = "No implementado en web" };

    public Task<bool> RegisterPasskeyAsync(string firebaseUid, string credentialId, string deviceName) => Task.FromResult(false);
    public Task<List<SGRRHH.Core.Models.PasskeyInfo>> GetUserPasskeysAsync(string firebaseUid) => Task.FromResult(new List<SGRRHH.Core.Models.PasskeyInfo>());
    public Task<bool> RevokePasskeyAsync(string firebaseUid, string credentialId) => Task.FromResult(false);

    public string HashPassword(string password) => password;
    public bool VerifyPassword(string password, string hash) => true;
    public Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword) => Task.FromResult(false);
}
