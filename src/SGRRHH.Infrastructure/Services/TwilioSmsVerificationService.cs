using Microsoft.Extensions.Logging;
using SGRRHH.Core.Interfaces;
using System.Management;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de verificación SMS usando Twilio Verify
/// </summary>
public class TwilioSmsVerificationService : ISmsVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _verifyServiceSid;
    private readonly ILogger<TwilioSmsVerificationService>? _logger;
    private readonly string _trustedDevicesPath;
    
    private const string TWILIO_VERIFY_URL = "https://verify.twilio.com/v2/Services";
    
    public TwilioSmsVerificationService(
        string accountSid,
        string authToken,
        string verifyServiceSid,
        ILogger<TwilioSmsVerificationService>? logger = null)
    {
        _accountSid = accountSid ?? throw new ArgumentNullException(nameof(accountSid));
        _authToken = authToken ?? throw new ArgumentNullException(nameof(authToken));
        _verifyServiceSid = verifyServiceSid ?? throw new ArgumentNullException(nameof(verifyServiceSid));
        _logger = logger;
        
        _httpClient = new HttpClient();
        
        // Configurar autenticación básica de Twilio
        var authValue = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_accountSid}:{_authToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
        
        // Ruta para guardar dispositivos confiables
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _trustedDevicesPath = Path.Combine(appData, "SGRRHH", "trusted_devices.json");
        
        // Crear directorio si no existe
        var dir = Path.GetDirectoryName(_trustedDevicesPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
    
    /// <summary>
    /// Envía un código de verificación SMS
    /// </summary>
    public async Task<SmsVerificationResult> SendVerificationCodeAsync(string phoneNumber)
    {
        try
        {
            if (string.IsNullOrEmpty(_verifyServiceSid))
            {
                _logger?.LogError("Twilio Verify Service SID no está configurado");
                return new SmsVerificationResult
                {
                    Success = false,
                    Message = "Servicio SMS no configurado"
                };
            }
            
            var url = $"{TWILIO_VERIFY_URL}/{_verifyServiceSid}/Verifications";
            
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To", phoneNumber),
                new KeyValuePair<string, string>("Channel", "whatsapp")  // WhatsApp es más barato que SMS
            });
            
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var json = JsonDocument.Parse(responseContent);
                var sid = json.RootElement.GetProperty("sid").GetString();
                
                _logger?.LogInformation("Código SMS enviado a {Phone}", MaskPhoneNumber(phoneNumber));
                
                return new SmsVerificationResult
                {
                    Success = true,
                    Message = "Código enviado",
                    VerificationSid = sid
                };
            }
            else
            {
                _logger?.LogError("Error al enviar SMS: {Response}", responseContent);
                
                // Parsear error de Twilio
                try
                {
                    var errorJson = JsonDocument.Parse(responseContent);
                    var errorMessage = errorJson.RootElement.GetProperty("message").GetString();
                    
                    return new SmsVerificationResult
                    {
                        Success = false,
                        Message = GetFriendlyTwilioError(errorMessage)
                    };
                }
                catch
                {
                    return new SmsVerificationResult
                    {
                        Success = false,
                        Message = "Error al enviar código SMS"
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al enviar código SMS");
            return new SmsVerificationResult
            {
                Success = false,
                Message = "Error de conexión al servicio SMS"
            };
        }
    }
    
    /// <summary>
    /// Verifica el código ingresado
    /// </summary>
    public async Task<SmsVerifyResult> VerifyCodeAsync(string phoneNumber, string code)
    {
        try
        {
            var url = $"{TWILIO_VERIFY_URL}/{_verifyServiceSid}/VerificationCheck";
            
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To", phoneNumber),
                new KeyValuePair<string, string>("Code", code)
            });
            
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var json = JsonDocument.Parse(responseContent);
                var status = json.RootElement.GetProperty("status").GetString();
                var isValid = status == "approved";
                
                _logger?.LogInformation("Verificación SMS: {Status}", status);
                
                return new SmsVerifyResult
                {
                    Success = true,
                    IsValid = isValid,
                    Message = isValid ? "Código verificado" : "Código incorrecto o expirado"
                };
            }
            else
            {
                _logger?.LogWarning("Código SMS inválido para {Phone}", MaskPhoneNumber(phoneNumber));
                
                return new SmsVerifyResult
                {
                    Success = true, // La llamada funcionó, pero el código es inválido
                    IsValid = false,
                    Message = "Código incorrecto o expirado"
                };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar código SMS");
            return new SmsVerifyResult
            {
                Success = false,
                IsValid = false,
                Message = "Error al verificar código"
            };
        }
    }
    
    /// <summary>
    /// Verifica si el dispositivo actual está registrado como confiable
    /// </summary>
    public Task<bool> IsDeviceTrustedAsync(string userId)
    {
        try
        {
            var deviceId = GetDeviceId();
            var trustedDevices = LoadTrustedDevices();
            
            if (trustedDevices.TryGetValue(userId, out var deviceInfo))
            {
                // Verificar que el deviceId coincida y no haya expirado
                if (deviceInfo.DeviceId == deviceId && deviceInfo.ExpiresAt > DateTime.UtcNow)
                {
                    _logger?.LogInformation("Dispositivo confiable encontrado para usuario {UserId}", userId);
                    return Task.FromResult(true);
                }
                
                // Si expiró, remover
                if (deviceInfo.ExpiresAt <= DateTime.UtcNow)
                {
                    trustedDevices.Remove(userId);
                    SaveTrustedDevices(trustedDevices);
                    _logger?.LogInformation("Confianza de dispositivo expirada para {UserId}", userId);
                }
            }
            
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar dispositivo confiable");
            return Task.FromResult(false);
        }
    }
    
    /// <summary>
    /// Registra el dispositivo actual como confiable
    /// </summary>
    public Task TrustDeviceAsync(string userId, int durationDays = 30)
    {
        try
        {
            var deviceId = GetDeviceId();
            var trustedDevices = LoadTrustedDevices();
            
            trustedDevices[userId] = new TrustedDeviceInfo
            {
                DeviceId = deviceId,
                TrustedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(durationDays)
            };
            
            SaveTrustedDevices(trustedDevices);
            
            _logger?.LogInformation("Dispositivo registrado como confiable para {UserId}, expira en {Days} días", 
                userId, durationDays);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al registrar dispositivo confiable");
            return Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Elimina la confianza del dispositivo
    /// </summary>
    public Task RemoveDeviceTrustAsync(string userId)
    {
        try
        {
            var trustedDevices = LoadTrustedDevices();
            
            if (trustedDevices.Remove(userId))
            {
                SaveTrustedDevices(trustedDevices);
                _logger?.LogInformation("Confianza de dispositivo eliminada para {UserId}", userId);
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar confianza de dispositivo");
            return Task.CompletedTask;
        }
    }
    
    #region Private Methods
    
    /// <summary>
    /// Genera un ID único para este dispositivo basado en hardware
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private string GetDeviceId()
    {
        try
        {
            var sb = new StringBuilder();
            
            // Obtener ID de la placa base
            using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
            {
                foreach (var obj in searcher.Get())
                {
                    sb.Append(obj["SerialNumber"]?.ToString() ?? "");
                    break;
                }
            }
            
            // Obtener ID del procesador
            using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
            {
                foreach (var obj in searcher.Get())
                {
                    sb.Append(obj["ProcessorId"]?.ToString() ?? "");
                    break;
                }
            }
            
            // Obtener MAC de la primera interfaz de red
            using (var searcher = new ManagementObjectSearcher("SELECT MACAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE"))
            {
                foreach (var obj in searcher.Get())
                {
                    sb.Append(obj["MACAddress"]?.ToString() ?? "");
                    break;
                }
            }
            
            // Hashear para obtener un ID consistente
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToBase64String(hash);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "No se pudo obtener ID de hardware, usando fallback");
            
            // Fallback: usar nombre de máquina + usuario
            var fallback = $"{Environment.MachineName}_{Environment.UserName}";
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(fallback));
            return Convert.ToBase64String(hash);
        }
    }
    
    private Dictionary<string, TrustedDeviceInfo> LoadTrustedDevices()
    {
        try
        {
            if (File.Exists(_trustedDevicesPath))
            {
                var json = File.ReadAllText(_trustedDevicesPath);
                return JsonSerializer.Deserialize<Dictionary<string, TrustedDeviceInfo>>(json) 
                       ?? new Dictionary<string, TrustedDeviceInfo>();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cargar dispositivos confiables");
        }
        
        return new Dictionary<string, TrustedDeviceInfo>();
    }
    
    private void SaveTrustedDevices(Dictionary<string, TrustedDeviceInfo> devices)
    {
        try
        {
            var json = JsonSerializer.Serialize(devices, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_trustedDevicesPath, json);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al guardar dispositivos confiables");
        }
    }
    
    private string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 6)
            return "***";
        
        return $"{phone[..4]}****{phone[^2..]}";
    }
    
    private string GetFriendlyTwilioError(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return "Error al enviar SMS";
        
        if (message.Contains("Invalid parameter"))
            return "Número de teléfono inválido";
        
        if (message.Contains("unverified"))
            return "Número de teléfono no verificado en Twilio";
        
        if (message.Contains("rate limit"))
            return "Demasiados intentos. Espere unos minutos.";
        
        return "Error al enviar código SMS";
    }
    
    #endregion
}

/// <summary>
/// Información de un dispositivo confiable
/// </summary>
internal class TrustedDeviceInfo
{
    public string DeviceId { get; set; } = string.Empty;
    public DateTime TrustedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
