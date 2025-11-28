using SGRRHH.Core.Common;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio para validaciones cruzadas entre módulos
/// Evita solapamientos entre vacaciones, permisos y otras ausencias
/// </summary>
public interface IAbsenceValidationService
{
    /// <summary>
    /// Verifica si un empleado tiene alguna ausencia (permiso o vacación) en el rango de fechas
    /// </summary>
    /// <param name="empleadoId">ID del empleado</param>
    /// <param name="fechaInicio">Fecha de inicio</param>
    /// <param name="fechaFin">Fecha de fin</param>
    /// <param name="excludePermisoId">ID de permiso a excluir (para ediciones)</param>
    /// <param name="excludeVacacionId">ID de vacación a excluir (para ediciones)</param>
    /// <returns>Resultado con mensaje descriptivo si hay solapamiento</returns>
    Task<ServiceResult> ValidarDisponibilidadAsync(
        int empleadoId, 
        DateTime fechaInicio, 
        DateTime fechaFin,
        int? excludePermisoId = null,
        int? excludeVacacionId = null);
    
    /// <summary>
    /// Verifica si el empleado tiene permisos en el rango de fechas
    /// </summary>
    Task<ServiceResult<bool>> TienePermisosEnRangoAsync(
        int empleadoId, 
        DateTime fechaInicio, 
        DateTime fechaFin,
        int? excludeId = null);
    
    /// <summary>
    /// Verifica si el empleado tiene vacaciones en el rango de fechas
    /// </summary>
    Task<ServiceResult<bool>> TieneVacacionesEnRangoAsync(
        int empleadoId, 
        DateTime fechaInicio, 
        DateTime fechaFin,
        int? excludeId = null);
}
