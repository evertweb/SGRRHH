using SGRRHH.Core.Common;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de validación de ausencias
/// Valida solapamientos entre permisos y vacaciones
/// </summary>
public class AbsenceValidationService : IAbsenceValidationService
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly IVacacionRepository _vacacionRepository;

    public AbsenceValidationService(
        IPermisoRepository permisoRepository,
        IVacacionRepository vacacionRepository)
    {
        _permisoRepository = permisoRepository;
        _vacacionRepository = vacacionRepository;
    }

    public async Task<ServiceResult> ValidarDisponibilidadAsync(
        int empleadoId,
        DateTime fechaInicio,
        DateTime fechaFin,
        int? excludePermisoId = null,
        int? excludeVacacionId = null)
    {
        // Verificar permisos
        var tienePermiso = await TienePermisosEnRangoAsync(empleadoId, fechaInicio, fechaFin, excludePermisoId);
        if (tienePermiso.Success && tienePermiso.Data)
        {
            return ServiceResult.Fail("El empleado ya tiene un permiso aprobado o pendiente en las fechas seleccionadas");
        }

        // Verificar vacaciones
        var tieneVacacion = await TieneVacacionesEnRangoAsync(empleadoId, fechaInicio, fechaFin, excludeVacacionId);
        if (tieneVacacion.Success && tieneVacacion.Data)
        {
            return ServiceResult.Fail("El empleado ya tiene vacaciones programadas en las fechas seleccionadas");
        }

        return ServiceResult.Ok("Fechas disponibles");
    }

    public async Task<ServiceResult<bool>> TienePermisosEnRangoAsync(
        int empleadoId,
        DateTime fechaInicio,
        DateTime fechaFin,
        int? excludeId = null)
    {
        try
        {
            var existeSolapamiento = await _permisoRepository.ExisteSolapamientoAsync(
                empleadoId, fechaInicio, fechaFin, excludeId);
            
            return ServiceResult<bool>.Ok(existeSolapamiento);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error al verificar permisos: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> TieneVacacionesEnRangoAsync(
        int empleadoId,
        DateTime fechaInicio,
        DateTime fechaFin,
        int? excludeId = null)
    {
        try
        {
            var existeTraslape = await _vacacionRepository.ExisteTraslapeAsync(
                empleadoId, fechaInicio, fechaFin, excludeId);
            
            return ServiceResult<bool>.Ok(existeTraslape);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error al verificar vacaciones: {ex.Message}");
        }
    }
}
