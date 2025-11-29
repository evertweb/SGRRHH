using System.Timers;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Firebase.Repositories;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Servicio para gestionar la presencia de usuarios (online/offline).
/// Utiliza un sistema de heartbeat para detectar desconexiones.
/// </summary>
public class PresenceService : IPresenceService, IDisposable
{
    private readonly PresenceFirestoreRepository _presenceRepository;
    private readonly ILogger<PresenceService>? _logger;
    
    private System.Timers.Timer? _heartbeatTimer;
    private System.Timers.Timer? _cleanupTimer;
    private IDisposable? _onlineUsersListener;
    private bool _disposed;
    
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan InactivityTimeout = TimeSpan.FromMinutes(2);
    
    public Usuario? CurrentUser { get; private set; }
    public bool IsActive { get; private set; }
    
    public event EventHandler<IEnumerable<UserPresence>>? OnlineUsersChanged;
    
    public PresenceService(PresenceFirestoreRepository presenceRepository, ILogger<PresenceService>? logger = null)
    {
        _presenceRepository = presenceRepository;
        _logger = logger;
    }
    
    /// <summary>
    /// Inicializa el servicio de presencia para el usuario actual
    /// </summary>
    public async Task StartAsync(Usuario user)
    {
        if (IsActive)
        {
            _logger?.LogWarning("El servicio de presencia ya está activo para {Username}", CurrentUser?.Username);
            return;
        }
        
        CurrentUser = user;
        
        try
        {
            _logger?.LogInformation("Iniciando servicio de presencia para usuario {Username} (Id: {Id}, FirebaseUid: {FirebaseUid})", 
                user.Username, user.Id, user.FirebaseUid);
            
            // Crear o actualizar registro de presencia
            var presence = new UserPresence
            {
                UserId = user.Id,
                FirebaseUid = user.FirebaseUid,
                Username = user.Username,
                NombreCompleto = user.NombreCompleto,
                IsOnline = true,
                LastSeen = DateTime.Now,
                LastHeartbeat = DateTime.Now,
                DeviceName = Environment.MachineName,
                Status = "Disponible"
            };
            
            await _presenceRepository.UpsertAsync(presence);
            
            // Iniciar timer de heartbeat
            _heartbeatTimer = new System.Timers.Timer(HeartbeatInterval.TotalMilliseconds);
            _heartbeatTimer.Elapsed += OnHeartbeatTimer;
            _heartbeatTimer.AutoReset = true;
            _heartbeatTimer.Start();
            
            // Iniciar timer de limpieza de usuarios inactivos
            _cleanupTimer = new System.Timers.Timer(InactivityTimeout.TotalMilliseconds);
            _cleanupTimer.Elapsed += OnCleanupTimer;
            _cleanupTimer.AutoReset = true;
            _cleanupTimer.Start();
            
            IsActive = true;
            
            _logger?.LogInformation("Servicio de presencia iniciado correctamente para usuario {Username}", user.Username);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al iniciar servicio de presencia para {Username}", user.Username);
            throw;
        }
    }
    
    /// <summary>
    /// Detiene el servicio de presencia (marca al usuario como offline)
    /// </summary>
    public async Task StopAsync()
    {
        // Evitar que se llame más de una vez
        if (_disposed || !IsActive || CurrentUser == null)
            return;

        try
        {
            // Detener timers
            _heartbeatTimer?.Stop();
            _cleanupTimer?.Stop();

            // Detener listener
            _onlineUsersListener?.Dispose();
            _onlineUsersListener = null;

            // Marcar como offline
            await _presenceRepository.UpdateOnlineStatusAsync(CurrentUser.Id, false);

            IsActive = false;

            _logger?.LogInformation("Servicio de presencia detenido para usuario {Username}", CurrentUser.Username);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al detener servicio de presencia");
        }
    }
    
    /// <summary>
    /// Obtiene todos los usuarios actualmente online
    /// </summary>
    public async Task<IEnumerable<UserPresence>> GetOnlineUsersAsync()
    {
        try
        {
            return await _presenceRepository.GetOnlineUsersAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuarios online");
            return Enumerable.Empty<UserPresence>();
        }
    }
    
    /// <summary>
    /// Obtiene el estado de presencia de un usuario específico
    /// </summary>
    public async Task<UserPresence?> GetUserPresenceAsync(int userId)
    {
        try
        {
            return await _presenceRepository.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener presencia del usuario: {UserId}", userId);
            return null;
        }
    }
    
    /// <summary>
    /// Actualiza el estado del usuario actual
    /// </summary>
    public async Task UpdateStatusAsync(string status, string? statusMessage = null)
    {
        if (CurrentUser == null)
            return;
        
        try
        {
            var presence = await _presenceRepository.GetByUserIdAsync(CurrentUser.Id);
            if (presence != null)
            {
                presence.Status = status;
                presence.StatusMessage = statusMessage;
                presence.FechaModificacion = DateTime.Now;
                await _presenceRepository.UpdateAsync(presence);
                
                _logger?.LogInformation("Estado actualizado para usuario {Username}: {Status}", 
                    CurrentUser.Username, status);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar estado del usuario");
        }
    }
    
    /// <summary>
    /// Escucha cambios en tiempo real de los usuarios online
    /// </summary>
    public IDisposable ListenToOnlineUsers(Action<IEnumerable<UserPresence>> onChanged)
    {
        _onlineUsersListener?.Dispose();
        
        _onlineUsersListener = _presenceRepository.ListenToOnlineUsers(users =>
        {
            onChanged(users);
            OnlineUsersChanged?.Invoke(this, users);
        });
        
        return _onlineUsersListener;
    }
    
    private async void OnHeartbeatTimer(object? sender, ElapsedEventArgs e)
    {
        if (CurrentUser == null)
            return;
        
        try
        {
            await _presenceRepository.UpdateHeartbeatAsync(CurrentUser.Id, CurrentUser.FirebaseUid);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en heartbeat");
        }
    }
    
    private async void OnCleanupTimer(object? sender, ElapsedEventArgs e)
    {
        try
        {
            await _presenceRepository.MarkInactiveUsersOfflineAsync(InactivityTimeout);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en limpieza de usuarios inactivos");
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            _heartbeatTimer?.Stop();
            _heartbeatTimer?.Dispose();
            _cleanupTimer?.Stop();
            _cleanupTimer?.Dispose();
            _onlineUsersListener?.Dispose();
        }
        
        _disposed = true;
    }
}
