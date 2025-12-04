using Microsoft.Extensions.Logging;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Servicio de negocio para la gestión de permisos
/// </summary>
public class PermisoService : IPermisoService
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly ITipoPermisoRepository _tipoPermisoRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IDateCalculationService _dateCalculationService;
    private readonly IAbsenceValidationService _absenceValidationService;
    private readonly ILogger<PermisoService>? _logger;

    public PermisoService(
        IPermisoRepository permisoRepository,
        ITipoPermisoRepository tipoPermisoRepository,
        IEmpleadoRepository empleadoRepository,
        IDateCalculationService dateCalculationService,
        IAbsenceValidationService absenceValidationService,
        ILogger<PermisoService>? logger = null)
    {
        _permisoRepository = permisoRepository;
        _tipoPermisoRepository = tipoPermisoRepository;
        _empleadoRepository = empleadoRepository;
        _dateCalculationService = dateCalculationService;
        _absenceValidationService = absenceValidationService;
        _logger = logger;
    }

    public async Task<ServiceResult<IEnumerable<Permiso>>> GetAllAsync(
        int? empleadoId = null, 
        EstadoPermiso? estado = null, 
        DateTime? fechaDesde = null, 
        DateTime? fechaHasta = null)
    {
        try
        {
            var permisos = await _permisoRepository.GetAllWithFiltersAsync(empleadoId, estado, fechaDesde, fechaHasta);
            return ServiceResult<IEnumerable<Permiso>>.Ok(permisos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Permiso>>.Fail($"Error al obtener permisos: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Permiso>> GetByIdAsync(int id)
    {
        try
        {
            var permiso = await _permisoRepository.GetByIdAsync(id);
            if (permiso == null)
            {
                return ServiceResult<Permiso>.Fail("Permiso no encontrado");
            }

            return ServiceResult<Permiso>.Ok(permiso);
        }
        catch (Exception ex)
        {
            return ServiceResult<Permiso>.Fail($"Error al obtener permiso: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<Permiso>>> GetPendientesAsync()
    {
        try
        {
            var permisos = await _permisoRepository.GetPendientesAsync();
            return ServiceResult<IEnumerable<Permiso>>.Ok(permisos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Permiso>>.Fail($"Error al obtener permisos pendientes: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<Permiso>>> GetByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var permisos = await _permisoRepository.GetByEmpleadoIdAsync(empleadoId);
            return ServiceResult<IEnumerable<Permiso>>.Ok(permisos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Permiso>>.Fail($"Error al obtener permisos del empleado: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Permiso>> SolicitarPermisoAsync(Permiso permiso, int usuarioSolicitanteId)
    {
        try
        {
            _logger?.LogInformation("Solicitando permiso para empleado {EmpleadoId}", permiso.EmpleadoId);
            
            // Validaciones
            var errores = await ValidarPermisoAsync(permiso);
            if (errores.Any())
            {
                _logger?.LogWarning("Validación fallida: {Errores}", string.Join(", ", errores));
                return ServiceResult<Permiso>.Fail(errores);
            }

            // Validar solapamiento con permisos existentes
            if (await _permisoRepository.ExisteSolapamientoAsync(permiso.EmpleadoId, permiso.FechaInicio, permiso.FechaFin))
            {
                return ServiceResult<Permiso>.Fail("El empleado ya tiene un permiso en las fechas seleccionadas");
            }
            
            // Validar solapamiento con vacaciones (validación cruzada)
            var validacionVacaciones = await _absenceValidationService.TieneVacacionesEnRangoAsync(
                permiso.EmpleadoId, permiso.FechaInicio, permiso.FechaFin);
            if (validacionVacaciones.Success && validacionVacaciones.Data)
            {
                return ServiceResult<Permiso>.Fail("El empleado tiene vacaciones programadas en las fechas seleccionadas");
            }

            // Generar número de acta
            permiso.NumeroActa = await _permisoRepository.GetProximoNumeroActaAsync();

            // Calcular días usando el servicio centralizado
            permiso.TotalDias = _dateCalculationService.CalcularDiasHabiles(permiso.FechaInicio, permiso.FechaFin);

            // Establecer valores predeterminados
            permiso.Estado = EstadoPermiso.Pendiente;
            permiso.FechaSolicitud = DateTime.Now;
            permiso.SolicitadoPorId = usuarioSolicitanteId;

            // Crear
            var created = await _permisoRepository.AddAsync(permiso);
            
            _logger?.LogInformation("Permiso {NumeroActa} creado exitosamente", created.NumeroActa);
            return ServiceResult<Permiso>.Ok(created, $"Permiso solicitado exitosamente. Número de acta: {created.NumeroActa}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al solicitar permiso");
            return ServiceResult<Permiso>.Fail($"Error al solicitar permiso: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Permiso>> AprobarPermisoAsync(int permisoId, int usuarioAprobadorId, string? observaciones = null)
    {
        try
        {
            _logger?.LogInformation("Aprobando permiso {PermisoId} por usuario {UsuarioId}", permisoId, usuarioAprobadorId);
            
            var permiso = await _permisoRepository.GetByIdAsync(permisoId);
            if (permiso == null)
            {
                return ServiceResult<Permiso>.Fail("Permiso no encontrado");
            }

            if (permiso.Estado != EstadoPermiso.Pendiente)
            {
                return ServiceResult<Permiso>.Fail("Solo se pueden aprobar permisos pendientes");
            }

            // Aprobar
            permiso.Estado = EstadoPermiso.Aprobado;
            permiso.AprobadoPorId = usuarioAprobadorId;
            permiso.FechaAprobacion = DateTime.Now;
            
            if (!string.IsNullOrWhiteSpace(observaciones))
            {
                permiso.Observaciones = observaciones;
            }

            await _permisoRepository.UpdateAsync(permiso);
            
            _logger?.LogInformation("Permiso {NumeroActa} aprobado exitosamente", permiso.NumeroActa);
            return ServiceResult<Permiso>.Ok(permiso, "Permiso aprobado exitosamente");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al aprobar permiso {PermisoId}", permisoId);
            return ServiceResult<Permiso>.Fail($"Error al aprobar permiso: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Permiso>> RechazarPermisoAsync(int permisoId, int usuarioAprobadorId, string motivoRechazo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(motivoRechazo))
            {
                return ServiceResult<Permiso>.Fail("Debe proporcionar un motivo de rechazo");
            }

            var permiso = await _permisoRepository.GetByIdAsync(permisoId);
            if (permiso == null)
            {
                return ServiceResult<Permiso>.Fail("Permiso no encontrado");
            }

            if (permiso.Estado != EstadoPermiso.Pendiente)
            {
                return ServiceResult<Permiso>.Fail("Solo se pueden rechazar permisos pendientes");
            }

            // Rechazar
            permiso.Estado = EstadoPermiso.Rechazado;
            permiso.AprobadoPorId = usuarioAprobadorId;
            permiso.FechaAprobacion = DateTime.Now;
            permiso.MotivoRechazo = motivoRechazo;

            await _permisoRepository.UpdateAsync(permiso);
            return ServiceResult<Permiso>.Ok(permiso, "Permiso rechazado");
        }
        catch (Exception ex)
        {
            return ServiceResult<Permiso>.Fail($"Error al rechazar permiso: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Permiso>> CancelarPermisoAsync(int permisoId, int usuarioId)
    {
        try
        {
            var permiso = await _permisoRepository.GetByIdAsync(permisoId);
            if (permiso == null)
            {
                return ServiceResult<Permiso>.Fail("Permiso no encontrado");
            }

            if (permiso.Estado != EstadoPermiso.Pendiente)
            {
                return ServiceResult<Permiso>.Fail("Solo se pueden cancelar permisos pendientes");
            }

            permiso.Estado = EstadoPermiso.Cancelado;
            await _permisoRepository.UpdateAsync(permiso);
            
            return ServiceResult<Permiso>.Ok(permiso, "Permiso cancelado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Permiso>.Fail($"Error al cancelar permiso: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Permiso>> UpdateAsync(Permiso permiso)
    {
        try
        {
            var existing = await _permisoRepository.GetByIdAsync(permiso.Id);
            if (existing == null)
            {
                return ServiceResult<Permiso>.Fail("Permiso no encontrado");
            }

            if (existing.Estado != EstadoPermiso.Pendiente)
            {
                return ServiceResult<Permiso>.Fail("Solo se pueden editar permisos pendientes");
            }

            // Validaciones
            var errores = await ValidarPermisoAsync(permiso);
            if (errores.Any())
            {
                return ServiceResult<Permiso>.Fail(errores);
            }

            // Validar solapamiento excluyendo el permiso actual
            if (await _permisoRepository.ExisteSolapamientoAsync(permiso.EmpleadoId, permiso.FechaInicio, permiso.FechaFin, permiso.Id))
            {
                return ServiceResult<Permiso>.Fail("El empleado ya tiene un permiso en las fechas seleccionadas");
            }
            
            // Validar solapamiento con vacaciones (validación cruzada)
            var validacionVacaciones = await _absenceValidationService.TieneVacacionesEnRangoAsync(
                permiso.EmpleadoId, permiso.FechaInicio, permiso.FechaFin);
            if (validacionVacaciones.Success && validacionVacaciones.Data)
            {
                return ServiceResult<Permiso>.Fail("El empleado tiene vacaciones programadas en las fechas seleccionadas");
            }

            // Recalcular días usando el servicio centralizado
            permiso.TotalDias = _dateCalculationService.CalcularDiasHabiles(permiso.FechaInicio, permiso.FechaFin);

            await _permisoRepository.UpdateAsync(permiso);
            return ServiceResult<Permiso>.Ok(permiso, "Permiso actualizado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Permiso>.Fail($"Error al actualizar permiso: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var permiso = await _permisoRepository.GetByIdAsync(id);
            if (permiso == null)
            {
                return ServiceResult<bool>.Fail("Permiso no encontrado");
            }

            if (permiso.Estado != EstadoPermiso.Pendiente)
            {
                return ServiceResult<bool>.Fail("Solo se pueden eliminar permisos pendientes");
            }

            await _permisoRepository.DeleteAsync(id);
            return ServiceResult<bool>.Ok(true, "Permiso eliminado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error al eliminar permiso: {ex.Message}");
        }
    }

    #region Métodos Privados

    private async Task<List<string>> ValidarPermisoAsync(Permiso permiso)
    {
        var errores = new List<string>();

        // Validar empleado
        if (permiso.EmpleadoId <= 0)
        {
            errores.Add("Debe seleccionar un empleado");
        }
        else
        {
            var empleado = await _empleadoRepository.GetByIdAsync(permiso.EmpleadoId);
            if (empleado == null)
            {
                errores.Add("Empleado no encontrado");
            }
            else if (empleado.Estado != EstadoEmpleado.Activo)
            {
                errores.Add("El empleado no está activo");
            }
        }

        // Validar tipo de permiso
        if (permiso.TipoPermisoId <= 0)
        {
            errores.Add("Debe seleccionar un tipo de permiso");
        }
        else
        {
            var tipoPermiso = await _tipoPermisoRepository.GetByIdAsync(permiso.TipoPermisoId);
            if (tipoPermiso == null)
            {
                errores.Add("Tipo de permiso no encontrado");
            }
            else if (!tipoPermiso.Activo)
            {
                errores.Add("El tipo de permiso no está activo");
            }
        }

        // Validar fechas
        if (permiso.FechaInicio == default)
        {
            errores.Add("Debe especificar la fecha de inicio");
        }

        if (permiso.FechaFin == default)
        {
            errores.Add("Debe especificar la fecha de fin");
        }

        if (permiso.FechaInicio > permiso.FechaFin)
        {
            errores.Add("La fecha de inicio no puede ser posterior a la fecha de fin");
        }

        // Validar motivo
        if (string.IsNullOrWhiteSpace(permiso.Motivo))
        {
            errores.Add("Debe especificar el motivo del permiso");
        }

        return errores;
    }

    #endregion
}
