using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el servicio de auditoría
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra una acción en el log de auditoría
    /// </summary>
    Task RegistrarAsync(string accion, string entidad, int? entidadId, string descripcion, string? datosAdicionales = null);
    
    /// <summary>
    /// Registra una acción en el log de auditoría con usuario específico
    /// </summary>
    Task RegistrarAsync(int usuarioId, string usuarioNombre, string accion, string entidad, int? entidadId, string descripcion, string? datosAdicionales = null);
    
    /// <summary>
    /// Obtiene los registros de auditoría filtrados
    /// </summary>
    Task<List<AuditLog>> ObtenerRegistrosAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? entidad = null, int? usuarioId = null, int maxRegistros = 100);
    
    /// <summary>
    /// Obtiene los registros de auditoría de una entidad específica
    /// </summary>
    Task<List<AuditLog>> ObtenerRegistrosPorEntidadAsync(string entidad, int entidadId);
    
    /// <summary>
    /// Limpia registros de auditoría antiguos
    /// </summary>
    Task<int> LimpiarRegistrosAntiguosAsync(int diasAntiguedad);
}
