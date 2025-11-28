using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio para gestionar la presencia de usuarios (online/offline)
/// </summary>
public interface IPresenceService
{
    /// <summary>
    /// Usuario actual del servicio
    /// </summary>
    Usuario? CurrentUser { get; }
    
    /// <summary>
    /// Indica si el servicio de presencia está activo
    /// </summary>
    bool IsActive { get; }
    
    /// <summary>
    /// Inicializa el servicio de presencia para el usuario actual
    /// </summary>
    Task StartAsync(Usuario user);
    
    /// <summary>
    /// Detiene el servicio de presencia (marca al usuario como offline)
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// Obtiene todos los usuarios actualmente online
    /// </summary>
    Task<IEnumerable<UserPresence>> GetOnlineUsersAsync();
    
    /// <summary>
    /// Obtiene el estado de presencia de un usuario específico
    /// </summary>
    Task<UserPresence?> GetUserPresenceAsync(int userId);
    
    /// <summary>
    /// Actualiza el estado del usuario actual
    /// </summary>
    Task UpdateStatusAsync(string status, string? statusMessage = null);
    
    /// <summary>
    /// Evento que se dispara cuando hay cambios en los usuarios online
    /// </summary>
    event EventHandler<IEnumerable<UserPresence>>? OnlineUsersChanged;
    
    /// <summary>
    /// Escucha cambios en tiempo real de los usuarios online
    /// </summary>
    IDisposable ListenToOnlineUsers(Action<IEnumerable<UserPresence>> onChanged);
}
