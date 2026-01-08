using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de sesiones con timeout y persistencia
/// </summary>
public class SessionService : ISessionService, IDisposable
{
    private readonly ILogger<SessionService> _logger;
    private readonly IConfiguracionRepository _configRepository;
    
    private Usuario? _currentUser;
    private DateTime _sessionStart;
    private DateTime _lastActivity;
    private int _sessionTimeoutMinutes = 30; // Default: 30 minutos
    private int _warningBeforeExpireMinutes = 5; // Avisar 5 minutos antes
    
    private readonly ConcurrentDictionary<string, object?> _sessionData = new();
    private Timer? _sessionTimer;
    private bool _warningFired = false;
    private bool _disposed = false;
    
    public Usuario? CurrentUser => _currentUser;
    public bool IsSessionActive => _currentUser != null && !IsSessionExpired();
    public int SessionTimeRemainingSeconds => CalculateRemainingSeconds();
    
    public event EventHandler? OnSessionExpiring;
    public event EventHandler? OnSessionExpired;
    
    public SessionService(
        ILogger<SessionService> logger,
        IConfiguracionRepository configRepository)
    {
        _logger = logger;
        _configRepository = configRepository;
        
        _ = LoadConfigurationAsync();
    }
    
    private async Task LoadConfigurationAsync()
    {
        try
        {
            var timeoutConfig = await _configRepository.GetByClaveAsync("session.timeout_minutes");
            if (timeoutConfig != null && int.TryParse(timeoutConfig.Valor, out int timeout))
            {
                _sessionTimeoutMinutes = timeout;
                _logger.LogInformation("Session timeout configurado: {Minutes} minutos", _sessionTimeoutMinutes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo cargar configuración de sesión, usando valores por defecto");
        }
    }
    
    public async Task<Result> StartSessionAsync(Usuario user)
    {
        try
        {
            _currentUser = user;
            _sessionStart = DateTime.Now;
            _lastActivity = DateTime.Now;
            _warningFired = false;
            _sessionData.Clear();
            
            // Iniciar timer de verificación de sesión
            StartSessionTimer();
            
            _logger.LogInformation("Sesión iniciada para usuario: {Username}", user.Username);
            
            return await Task.FromResult(Result.Ok("Sesión iniciada"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesión");
            return Result.Fail("Error al iniciar sesión: " + ex.Message);
        }
    }
    
    public Task EndSessionAsync()
    {
        if (_currentUser != null)
        {
            _logger.LogInformation("Sesión finalizada para usuario: {Username}", _currentUser.Username);
        }
        
        _currentUser = null;
        _sessionData.Clear();
        StopSessionTimer();
        
        return Task.CompletedTask;
    }
    
    public Task ExtendSessionAsync()
    {
        if (_currentUser != null)
        {
            _lastActivity = DateTime.Now;
            _warningFired = false;
            _logger.LogDebug("Sesión extendida para usuario: {Username}", _currentUser.Username);
        }
        
        return Task.CompletedTask;
    }
    
    public Task<bool> ValidateSessionAsync()
    {
        if (_currentUser == null)
            return Task.FromResult(false);
        
        if (IsSessionExpired())
        {
            _logger.LogWarning("Sesión expirada por inactividad: {Username}", _currentUser.Username);
            OnSessionExpired?.Invoke(this, EventArgs.Empty);
            return Task.FromResult(false);
        }
        
        return Task.FromResult(true);
    }
    
    public void RegisterActivity()
    {
        if (_currentUser != null)
        {
            _lastActivity = DateTime.Now;
            _warningFired = false;
        }
    }
    
    public void SetSessionData<T>(string key, T value)
    {
        _sessionData[key] = value;
    }
    
    public T? GetSessionData<T>(string key)
    {
        if (_sessionData.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }
    
    public void RemoveSessionData(string key)
    {
        _sessionData.TryRemove(key, out _);
    }
    
    private bool IsSessionExpired()
    {
        var inactiveTime = DateTime.Now - _lastActivity;
        return inactiveTime.TotalMinutes > _sessionTimeoutMinutes;
    }
    
    private int CalculateRemainingSeconds()
    {
        if (_currentUser == null) return 0;
        
        var elapsed = DateTime.Now - _lastActivity;
        var remaining = TimeSpan.FromMinutes(_sessionTimeoutMinutes) - elapsed;
        
        return remaining.TotalSeconds > 0 ? (int)remaining.TotalSeconds : 0;
    }
    
    private void StartSessionTimer()
    {
        StopSessionTimer();
        
        // Verificar cada minuto
        _sessionTimer = new Timer(CheckSessionStatus, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }
    
    private void StopSessionTimer()
    {
        _sessionTimer?.Dispose();
        _sessionTimer = null;
    }
    
    private void CheckSessionStatus(object? state)
    {
        if (_currentUser == null) return;
        
        var remainingMinutes = SessionTimeRemainingSeconds / 60.0;
        
        // Avisar cuando queden 5 minutos
        if (remainingMinutes <= _warningBeforeExpireMinutes && !_warningFired)
        {
            _warningFired = true;
            _logger.LogInformation("Sesión por expirar en {Minutes} minutos: {Username}", 
                remainingMinutes, _currentUser.Username);
            OnSessionExpiring?.Invoke(this, EventArgs.Empty);
        }
        
        // Verificar expiración
        if (IsSessionExpired())
        {
            _logger.LogWarning("Sesión expirada por inactividad: {Username}", _currentUser.Username);
            OnSessionExpired?.Invoke(this, EventArgs.Empty);
            _ = EndSessionAsync();
        }
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            StopSessionTimer();
            _disposed = true;
        }
    }
}
