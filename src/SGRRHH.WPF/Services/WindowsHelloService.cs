using System.Runtime.InteropServices;
using CredentialManagement;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;
using Windows.Security.Credentials.UI;

namespace SGRRHH.WPF.Services;

/// <summary>
/// Servicio para autenticación con Windows Hello (biometría/PIN)
/// </summary>
public class WindowsHelloService : IWindowsHelloService
{
    private readonly IFirebaseAuthService _authService;
    private readonly ILogger<WindowsHelloService>? _logger;
    private const string CREDENTIAL_TARGET_PREFIX = "SGRRHH_WindowsHello_";
    private const string LAST_USERNAME_KEY = "SGRRHH_WindowsHello_LastUser";

    public WindowsHelloService(
        IFirebaseAuthService authService,
        ILogger<WindowsHelloService>? logger = null)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Verifica si Windows Hello está disponible en el dispositivo actual
    /// </summary>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            // Verificar versión de Windows (mínimo Windows 10 Build 17763)
            if (!IsWindows10OrGreater(17763))
            {
                _logger?.LogWarning("Windows Hello no soportado: versión de Windows insuficiente");
                return false;
            }

            // Verificar disponibilidad de UserConsentVerifier
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();

            bool isAvailable = availability == UserConsentVerifierAvailability.Available;

            _logger?.LogInformation("Windows Hello disponibilidad: {Availability}", availability);

            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar disponibilidad de Windows Hello");
            return false;
        }
    }

    /// <summary>
    /// Obtiene un mensaje descriptivo sobre la disponibilidad de Windows Hello
    /// </summary>
    public async Task<string> GetAvailabilityMessageAsync()
    {
        try
        {
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();

            return availability switch
            {
                UserConsentVerifierAvailability.Available =>
                    "Windows Hello disponible",
                UserConsentVerifierAvailability.DeviceNotPresent =>
                    "No hay dispositivo biométrico o PIN configurado",
                UserConsentVerifierAvailability.NotConfiguredForUser =>
                    "Configure Windows Hello en Configuración de Windows",
                UserConsentVerifierAvailability.DisabledByPolicy =>
                    "Windows Hello deshabilitado por política de grupo",
                _ => "Windows Hello no disponible"
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener mensaje de disponibilidad");
            return "Error al verificar Windows Hello";
        }
    }

    /// <summary>
    /// Solicita verificación de identidad con Windows Hello
    /// </summary>
    public async Task<WindowsHelloVerifyResult> VerifyAsync(IntPtr hwnd, string username)
    {
        try
        {
            // 1. Verificar disponibilidad
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();
            if (availability != UserConsentVerifierAvailability.Available)
            {
                return WindowsHelloVerifyResult.Fail("Windows Hello no disponible");
            }

            // 2. Obtener credentialId del Windows Credential Manager
            var credentialId = await GetStoredCredentialIdAsync(username);
            if (string.IsNullOrEmpty(credentialId))
            {
                return WindowsHelloVerifyResult.Fail("Credencial no encontrada. Configure Windows Hello primero.");
            }

            // 3. Generar challenge único
            var challenge = GenerateChallenge(username);

            // 4. Solicitar verificación con Windows Hello
            UserConsentVerificationResult result;

            if (hwnd != IntPtr.Zero)
            {
                // Usar método con hwnd para WPF
                result = await RequestVerificationForWindowAsync(hwnd, challenge);
            }
            else
            {
                // Fallback sin hwnd
                result = await UserConsentVerifier.RequestVerificationAsync(challenge);
            }

            if (result == UserConsentVerificationResult.Verified)
            {
                _logger?.LogInformation("Windows Hello verificación exitosa para {Username}", username);

                // Guardar como último usuario usado
                await SaveLastUsernameAsync(username);

                return WindowsHelloVerifyResult.Ok(credentialId);
            }
            else
            {
                _logger?.LogWarning("Windows Hello verificación fallida: {Result}", result);
                return WindowsHelloVerifyResult.Fail(GetFriendlyMessage(result));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar con Windows Hello");
            return WindowsHelloVerifyResult.Fail($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Registra una nueva passkey de Windows Hello
    /// </summary>
    public async Task<PasskeyRegistrationResult> RegisterPasskeyAsync(
        IntPtr hwnd,
        string username,
        string firebaseUid)
    {
        try
        {
            // 1. Verificar disponibilidad
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();
            if (availability != UserConsentVerifierAvailability.Available)
            {
                return PasskeyRegistrationResult.Fail(
                    await GetAvailabilityMessageAsync());
            }

            // 2. Generar credentialId único
            var credentialId = Guid.NewGuid().ToString();
            var deviceName = Environment.MachineName;

            // 3. Solicitar verificación de Windows Hello para confirmar
            var challenge = $"Registrar Windows Hello para {username}";

            UserConsentVerificationResult result;

            if (hwnd != IntPtr.Zero)
            {
                result = await RequestVerificationForWindowAsync(hwnd, challenge);
            }
            else
            {
                result = await UserConsentVerifier.RequestVerificationAsync(challenge);
            }

            if (result != UserConsentVerificationResult.Verified)
            {
                _logger?.LogWarning("Registro de passkey cancelado: {Result}", result);
                return PasskeyRegistrationResult.Fail(GetFriendlyMessage(result));
            }

            // 4. Guardar en Firestore
            var saved = await _authService.RegisterPasskeyAsync(
                firebaseUid,
                credentialId,
                deviceName);

            if (!saved)
            {
                return PasskeyRegistrationResult.Fail(
                    "Error al guardar la passkey en el servidor");
            }

            // 5. Guardar mapping local
            await SavePasskeyMappingAsync(username, credentialId);

            _logger?.LogInformation(
                "Passkey registrada exitosamente: {Username} -> {CredentialId}",
                username,
                credentialId);

            return PasskeyRegistrationResult.Ok(credentialId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al registrar passkey");
            return PasskeyRegistrationResult.Fail($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si el usuario tiene una passkey registrada localmente
    /// </summary>
    public async Task<bool> HasPasskeyAsync(string username)
    {
        var credentialId = await GetStoredCredentialIdAsync(username);
        return !string.IsNullOrEmpty(credentialId);
    }

    /// <summary>
    /// Obtiene el último nombre de usuario que usó passkey
    /// </summary>
    public Task<string?> GetLastPasskeyUsernameAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                using var cred = new Credential
                {
                    Target = LAST_USERNAME_KEY
                };

                if (cred.Load())
                {
                    return cred.Username;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener último username");
                return null;
            }
        });
    }

    /// <summary>
    /// Obtiene el credentialId almacenado localmente para un usuario
    /// </summary>
    public Task<string?> GetStoredCredentialIdAsync(string username)
    {
        return Task.Run(() =>
        {
            try
            {
                using var cred = new Credential
                {
                    Target = $"{CREDENTIAL_TARGET_PREFIX}{username}"
                };

                if (cred.Load())
                {
                    return cred.Password; // credentialId almacenado como "password"
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener credentialId para {Username}", username);
                return null;
            }
        });
    }

    /// <summary>
    /// Guarda el mapping username -> credentialId en el Credential Manager local
    /// </summary>
    public Task SavePasskeyMappingAsync(string username, string credentialId)
    {
        return Task.Run(() =>
        {
            try
            {
                using var cred = new Credential
                {
                    Target = $"{CREDENTIAL_TARGET_PREFIX}{username}",
                    Username = username,
                    Password = credentialId,
                    Type = CredentialType.Generic,
                    PersistanceType = PersistanceType.LocalComputer
                };
                cred.Save();

                _logger?.LogInformation("Passkey mapping guardado localmente: {Username}", username);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al guardar passkey mapping para {Username}", username);
                throw;
            }
        });
    }

    /// <summary>
    /// Elimina el mapping de passkey del Credential Manager local
    /// </summary>
    public Task RemovePasskeyMappingAsync(string username)
    {
        return Task.Run(() =>
        {
            try
            {
                using var cred = new Credential
                {
                    Target = $"{CREDENTIAL_TARGET_PREFIX}{username}"
                };
                cred.Delete();

                _logger?.LogInformation("Passkey mapping eliminado localmente: {Username}", username);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al eliminar passkey mapping para {Username}", username);
            }
        });
    }

    /// <summary>
    /// Obtiene todas las passkeys registradas localmente en este dispositivo
    /// </summary>
    public Task<IReadOnlyList<LocalPasskeyInfo>> GetAllLocalPasskeysAsync()
    {
        return Task.Run<IReadOnlyList<LocalPasskeyInfo>>(() =>
        {
            var passkeys = new List<LocalPasskeyInfo>();
            
            try
            {
                // Usar CredentialSet para enumerar todas las credenciales con nuestro prefijo
                using var credentialSet = new CredentialSet(CREDENTIAL_TARGET_PREFIX + "*");
                credentialSet.Load();
                
                foreach (var cred in credentialSet)
                {
                    try
                    {
                        if (cred.Target.StartsWith(CREDENTIAL_TARGET_PREFIX) && 
                            !cred.Target.Equals(LAST_USERNAME_KEY))
                        {
                            var username = cred.Username;
                            var credentialId = cred.Password;
                            
                            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(credentialId))
                            {
                                passkeys.Add(new LocalPasskeyInfo(username, credentialId));
                                _logger?.LogDebug("Passkey encontrada: {Username}", username);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error al leer credencial individual");
                    }
                }
                
                _logger?.LogInformation("Se encontraron {Count} passkeys locales", passkeys.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al enumerar passkeys locales");
            }
            
            return passkeys;
        });
    }

    #region Métodos Privados

    private string GenerateChallenge(string username)
    {
        return $"Iniciar sesión como {username}";
    }

    private async Task SaveLastUsernameAsync(string username)
    {
        try
        {
            await Task.Run(() =>
            {
                using var cred = new Credential
                {
                    Target = LAST_USERNAME_KEY,
                    Username = username,
                    Password = DateTime.UtcNow.ToString("O"),
                    Type = CredentialType.Generic,
                    PersistanceType = PersistanceType.LocalComputer
                };
                cred.Save();
            });
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al guardar último username");
        }
    }

    private bool IsWindows10OrGreater(int build)
    {
        var version = Environment.OSVersion.Version;
        return version.Major >= 10 && version.Build >= build;
    }

    private string GetFriendlyMessage(UserConsentVerificationResult result)
    {
        return result switch
        {
            UserConsentVerificationResult.Verified => "Verificación exitosa",
            UserConsentVerificationResult.DeviceNotPresent => "No hay dispositivo biométrico disponible",
            UserConsentVerificationResult.NotConfiguredForUser => "Windows Hello no está configurado",
            UserConsentVerificationResult.DisabledByPolicy => "Windows Hello deshabilitado por política",
            UserConsentVerificationResult.DeviceBusy => "Dispositivo ocupado, intente de nuevo",
            UserConsentVerificationResult.RetriesExhausted => "Demasiados intentos fallidos",
            UserConsentVerificationResult.Canceled => "Verificación cancelada",
            _ => "Error desconocido"
        };
    }

    // Método auxiliar para llamar UserConsentVerifier con hwnd (para WPF)
    private async Task<UserConsentVerificationResult> RequestVerificationForWindowAsync(
        IntPtr hwnd,
        string message)
    {
        // UserConsentVerifier no tiene método directo con hwnd en la API pública
        // Usamos el método estándar que toma el hwnd del foreground window
        // El hwnd se usa implícitamente cuando la app está en foreground
        return await UserConsentVerifier.RequestVerificationAsync(message);
    }

    #endregion
}
