using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Repositorio para gestionar la presencia de usuarios en Firestore
/// </summary>
public interface IPresenceRepository : IRepository<UserPresence>
{
    /// <summary>
    /// Obtiene la presencia de un usuario por su UserId
    /// </summary>
    Task<UserPresence?> GetByUserIdAsync(int userId);
    
    /// <summary>
    /// Obtiene la presencia de un usuario por su FirebaseUid
    /// </summary>
    Task<UserPresence?> GetByFirebaseUidAsync(string firebaseUid);
    
    /// <summary>
    /// Obtiene todos los usuarios que están actualmente online
    /// </summary>
    Task<IEnumerable<UserPresence>> GetOnlineUsersAsync();
    
    /// <summary>
    /// Actualiza el estado online de un usuario
    /// </summary>
    Task UpdateOnlineStatusAsync(int userId, bool isOnline);
    
    /// <summary>
    /// Actualiza el heartbeat del usuario (para detectar desconexiones)
    /// </summary>
    Task UpdateHeartbeatAsync(int userId, string? firebaseUid = null);
    
    /// <summary>
    /// Marca como offline a usuarios con heartbeat antiguo
    /// </summary>
    Task MarkInactiveUsersOfflineAsync(TimeSpan timeout);
}
