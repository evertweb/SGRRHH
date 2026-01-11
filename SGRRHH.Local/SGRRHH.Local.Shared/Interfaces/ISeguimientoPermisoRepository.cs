using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para el seguimiento de acciones sobre permisos
/// </summary>
public interface ISeguimientoPermisoRepository : IRepository<SeguimientoPermiso>
{
    /// <summary>Obtiene todos los seguimientos de un permiso</summary>
    Task<IEnumerable<SeguimientoPermiso>> GetByPermisoIdAsync(int permisoId);
    
    /// <summary>Registra una nueva acción de seguimiento</summary>
    Task RegistrarAccionAsync(int permisoId, TipoAccionSeguimiento tipoAccion, 
        string descripcion, int usuarioId, string? datosAdicionales = null);
    
    /// <summary>Obtiene seguimientos por tipo de acción</summary>
    Task<IEnumerable<SeguimientoPermiso>> GetByTipoAccionAsync(TipoAccionSeguimiento tipoAccion);
    
    /// <summary>Obtiene los seguimientos más recientes</summary>
    Task<IEnumerable<SeguimientoPermiso>> GetRecientesAsync(int cantidad = 50);
    
    /// <summary>Obtiene seguimientos de un permiso con información del usuario</summary>
    Task<IEnumerable<SeguimientoPermiso>> GetByPermisoIdWithUsuarioAsync(int permisoId);
}
