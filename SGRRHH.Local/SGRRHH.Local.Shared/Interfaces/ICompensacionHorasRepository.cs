using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para las compensaciones de horas de permisos
/// </summary>
public interface ICompensacionHorasRepository : IRepository<CompensacionHoras>
{
    /// <summary>Obtiene todas las compensaciones de un permiso</summary>
    Task<IEnumerable<CompensacionHoras>> GetByPermisoIdAsync(int permisoId);
    
    /// <summary>Obtiene el total de horas compensadas para un permiso</summary>
    Task<int> GetTotalHorasCompensadasAsync(int permisoId);
    
    /// <summary>Registra una nueva compensación de horas</summary>
    Task RegistrarCompensacionAsync(int permisoId, DateTime fecha, int horas, 
        string descripcion, int aprobadoPorId);
    
    /// <summary>Obtiene compensaciones pendientes de aprobación</summary>
    Task<IEnumerable<CompensacionHoras>> GetPendientesAprobacionAsync();
    
    /// <summary>Obtiene compensaciones con información del aprobador</summary>
    Task<IEnumerable<CompensacionHoras>> GetByPermisoIdWithAprobadorAsync(int permisoId);
}
