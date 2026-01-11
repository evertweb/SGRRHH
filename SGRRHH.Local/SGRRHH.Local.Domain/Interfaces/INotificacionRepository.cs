using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de notificaciones persistentes
/// </summary>
public interface INotificacionRepository
{
    /// <summary>
    /// Obtiene una notificación por ID
    /// </summary>
    Task<Notificacion?> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene notificaciones según filtro
    /// </summary>
    Task<List<NotificacionDto>> GetNotificacionesAsync(NotificacionFiltroDto filtro);
    
    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas para un usuario
    /// </summary>
    Task<int> GetCountNoLeidasAsync(int? usuarioId);
    
    /// <summary>
    /// Obtiene resumen de notificaciones para un usuario
    /// </summary>
    Task<NotificacionResumenDto> GetResumenAsync(int? usuarioId);
    
    /// <summary>
    /// Crea una nueva notificación
    /// </summary>
    Task<int> CreateAsync(CrearNotificacionDto dto, string? creadoPor = null);
    
    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    Task<bool> MarcarLeidaAsync(int id);
    
    /// <summary>
    /// Marca todas las notificaciones de un usuario como leídas
    /// </summary>
    Task<int> MarcarTodasLeidasAsync(int? usuarioId);
    
    /// <summary>
    /// Elimina una notificación
    /// </summary>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Elimina notificaciones expiradas o antiguas
    /// </summary>
    Task<int> LimpiarAntiguasAsync(int diasAntiguedad = 30);
    
    /// <summary>
    /// Verifica si existe una notificación para una entidad específica
    /// </summary>
    Task<bool> ExisteNotificacionEntidadAsync(string entidadTipo, int entidadId, string tipo);
}
