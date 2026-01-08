using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Interfaz para el servicio de gestión de sesiones
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Usuario actual de la sesión
    /// </summary>
    Usuario? CurrentUser { get; }
    
    /// <summary>
    /// Indica si hay una sesión activa
    /// </summary>
    bool IsSessionActive { get; }
    
    /// <summary>
    /// Tiempo restante de la sesión en segundos
    /// </summary>
    int SessionTimeRemainingSeconds { get; }
    
    /// <summary>
    /// Evento que se dispara cuando la sesión está por expirar (5 minutos antes)
    /// </summary>
    event EventHandler? OnSessionExpiring;
    
    /// <summary>
    /// Evento que se dispara cuando la sesión ha expirado
    /// </summary>
    event EventHandler? OnSessionExpired;
    
    /// <summary>
    /// Inicia una nueva sesión para el usuario
    /// </summary>
    Task<Result> StartSessionAsync(Usuario user);
    
    /// <summary>
    /// Finaliza la sesión actual
    /// </summary>
    Task EndSessionAsync();
    
    /// <summary>
    /// Extiende la sesión actual (refresh)
    /// </summary>
    Task ExtendSessionAsync();
    
    /// <summary>
    /// Verifica si la sesión sigue activa
    /// </summary>
    Task<bool> ValidateSessionAsync();
    
    /// <summary>
    /// Registra actividad del usuario (para mantener la sesión activa)
    /// </summary>
    void RegisterActivity();
    
    /// <summary>
    /// Guarda datos en la sesión
    /// </summary>
    void SetSessionData<T>(string key, T value);
    
    /// <summary>
    /// Obtiene datos de la sesión
    /// </summary>
    T? GetSessionData<T>(string key);
    
    /// <summary>
    /// Elimina datos de la sesión
    /// </summary>
    void RemoveSessionData(string key);
}
