using System.Text.Json;
using Microsoft.JSInterop;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Entities;

namespace SGRRHH.Web;

/// <summary>
/// Puente para recibir notificaciones de onAuthStateChanged desde JS y sincronizar AppState
/// </summary>
public class AuthJsBridge
{
    private readonly IFirebaseAuthService _authService;
    private readonly AppStateService _appState;

    public AuthJsBridge(IFirebaseAuthService authService, AppStateService appState)
    {
        _authService = authService;
        _appState = appState;
    }

    [JSInvokable("NotifyAuthStateChanged")]
    public async Task NotifyAuthStateChanged(JsonElement? payload)
    {
        try
        {
            if (payload == null || payload.Value.ValueKind == JsonValueKind.Null)
            {
                // Usuario desconectado en otra pestaña
                _appState.Logout();
                return;
            }

            // Leer campos mínimos
            bool success = false;
            string? uid = null;
            string? idToken = null;

            if (payload.Value.TryGetProperty("success", out var successProp) && successProp.ValueKind == JsonValueKind.True)
                success = true;

            if (payload.Value.TryGetProperty("uid", out var uidProp) && uidProp.ValueKind == JsonValueKind.String)
                uid = uidProp.GetString();

            if (payload.Value.TryGetProperty("idToken", out var tokenProp) && tokenProp.ValueKind == JsonValueKind.String)
                idToken = tokenProp.GetString();

            if (!success || string.IsNullOrEmpty(uid))
            {
                _appState.Logout();
                return;
            }

            // Obtener usuario desde repositorio/Firebase
            var usuario = await _authService.GetUserByFirebaseUidAsync(uid);
            if (usuario == null)
            {
                _appState.Logout();
                return;
            }

            var auth = new FirebaseAuthResult
            {
                Success = true,
                Usuario = usuario,
                FirebaseUid = uid,
                IdToken = idToken
            };

            _appState.SetAuth(auth);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AuthJsBridge.NotifyAuthStateChanged error: {ex}");
        }
    }
}