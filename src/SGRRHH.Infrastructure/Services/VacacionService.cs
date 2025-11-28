using Microsoft.Extensions.Logging;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de vacaciones
/// Regla Colombia: 15 días hábiles por año trabajado
/// </summary>
public class VacacionService : IVacacionService
{
    private readonly IVacacionRepository _vacacionRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IDateCalculationService _dateCalculationService;
    private readonly IAbsenceValidationService _absenceValidationService;
    private readonly ILogger<VacacionService>? _logger;
    
    public VacacionService(
        IVacacionRepository vacacionRepository, 
        IEmpleadoRepository empleadoRepository,
        IDateCalculationService dateCalculationService,
        IAbsenceValidationService absenceValidationService,
        ILogger<VacacionService>? logger = null)
    {
        _vacacionRepository = vacacionRepository;
        _empleadoRepository = empleadoRepository;
        _dateCalculationService = dateCalculationService;
        _absenceValidationService = absenceValidationService;
        _logger = logger;
    }
    
    public async Task<ServiceResult<IEnumerable<Vacacion>>> GetByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var vacaciones = await _vacacionRepository.GetByEmpleadoIdAsync(empleadoId);
            return ServiceResult<IEnumerable<Vacacion>>.SuccessResult(vacaciones);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Vacacion>>.FailureResult($"Error al obtener vacaciones: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Vacacion>> GetByIdAsync(int id)
    {
        try
        {
            var vacacion = await _vacacionRepository.GetByIdAsync(id);
            if (vacacion == null)
                return ServiceResult<Vacacion>.FailureResult("Vacación no encontrada");
                
            return ServiceResult<Vacacion>.SuccessResult(vacacion);
        }
        catch (Exception ex)
        {
            return ServiceResult<Vacacion>.FailureResult($"Error al obtener vacación: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Vacacion>> CreateAsync(Vacacion vacacion)
    {
        try
        {
            _logger?.LogInformation("Creando vacación para empleado {EmpleadoId}", vacacion.EmpleadoId);
            
            var errors = await ValidarVacacionAsync(vacacion);
            if (errors.Any())
            {
                _logger?.LogWarning("Validación fallida: {Errores}", string.Join(", ", errors));
                return ServiceResult<Vacacion>.FailureResult(errors);
            }
            
            // Validar solapamiento con permisos (validación cruzada)
            var validacionPermisos = await _absenceValidationService.TienePermisosEnRangoAsync(
                vacacion.EmpleadoId, vacacion.FechaInicio, vacacion.FechaFin);
            if (validacionPermisos.Success && validacionPermisos.Data)
            {
                return ServiceResult<Vacacion>.FailureResult("El empleado tiene permisos aprobados o pendientes en las fechas seleccionadas");
            }
            
            // Calcular días tomados usando el servicio centralizado
            vacacion.DiasTomados = _dateCalculationService.CalcularDiasHabiles(vacacion.FechaInicio, vacacion.FechaFin);
            
            // Verificar que no exceda los días disponibles
            var diasDisponibles = await CalcularDiasDisponiblesAsync(vacacion.EmpleadoId, vacacion.PeriodoCorrespondiente);
            if (!diasDisponibles.Success)
                return ServiceResult<Vacacion>.FailureResult(diasDisponibles.Message);
                
            if (vacacion.DiasTomados > diasDisponibles.Data)
            {
                return ServiceResult<Vacacion>.FailureResult(
                    $"Los días solicitados ({vacacion.DiasTomados}) exceden los días disponibles ({diasDisponibles.Data})");
            }
            
            vacacion.Activo = true;
            vacacion.FechaCreacion = DateTime.Now;
            
            await _vacacionRepository.AddAsync(vacacion);
            await _vacacionRepository.SaveChangesAsync();
            
            _logger?.LogInformation("Vacación creada exitosamente para empleado {EmpleadoId}", vacacion.EmpleadoId);
            return ServiceResult<Vacacion>.SuccessResult(vacacion, "Vacación registrada exitosamente");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al crear vacación");
            return ServiceResult<Vacacion>.FailureResult($"Error al crear vacación: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Vacacion>> UpdateAsync(Vacacion vacacion)
    {
        try
        {
            var existing = await _vacacionRepository.GetByIdAsync(vacacion.Id);
            if (existing == null)
                return ServiceResult<Vacacion>.FailureResult("Vacación no encontrada");
            
            var errors = await ValidarVacacionAsync(vacacion, vacacion.Id);
            if (errors.Any())
                return ServiceResult<Vacacion>.FailureResult(errors);
            
            // Validar solapamiento con permisos (validación cruzada)
            var validacionPermisos = await _absenceValidationService.TienePermisosEnRangoAsync(
                vacacion.EmpleadoId, vacacion.FechaInicio, vacacion.FechaFin);
            if (validacionPermisos.Success && validacionPermisos.Data)
            {
                return ServiceResult<Vacacion>.FailureResult("El empleado tiene permisos aprobados o pendientes en las fechas seleccionadas");
            }
            
            // Recalcular días usando el servicio centralizado
            vacacion.DiasTomados = _dateCalculationService.CalcularDiasHabiles(vacacion.FechaInicio, vacacion.FechaFin);
            
            // Verificar días disponibles (excluyendo esta vacación)
            var diasDisponibles = await CalcularDiasDisponiblesInternamenteAsync(
                vacacion.EmpleadoId, vacacion.PeriodoCorrespondiente, vacacion.Id);
                
            if (vacacion.DiasTomados > diasDisponibles)
            {
                return ServiceResult<Vacacion>.FailureResult(
                    $"Los días solicitados ({vacacion.DiasTomados}) exceden los días disponibles ({diasDisponibles})");
            }
            
            // Actualizar campos
            existing.FechaInicio = vacacion.FechaInicio;
            existing.FechaFin = vacacion.FechaFin;
            existing.DiasTomados = vacacion.DiasTomados;
            existing.PeriodoCorrespondiente = vacacion.PeriodoCorrespondiente;
            existing.Estado = vacacion.Estado;
            existing.Observaciones = vacacion.Observaciones;
            existing.FechaModificacion = DateTime.Now;
            
            await _vacacionRepository.UpdateAsync(existing);
            await _vacacionRepository.SaveChangesAsync();
            
            return ServiceResult<Vacacion>.SuccessResult(existing, "Vacación actualizada exitosamente");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar vacación");
            return ServiceResult<Vacacion>.FailureResult($"Error al actualizar vacación: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var vacacion = await _vacacionRepository.GetByIdAsync(id);
            if (vacacion == null)
                return ServiceResult<bool>.FailureResult("Vacación no encontrada");
            
            // No permitir eliminar vacaciones ya disfrutadas
            if (vacacion.Estado == EstadoVacacion.Disfrutada && vacacion.FechaFin < DateTime.Today)
            {
                return ServiceResult<bool>.FailureResult("No se puede eliminar una vacación ya disfrutada");
            }
            
            await _vacacionRepository.DeleteAsync(id);
            await _vacacionRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.SuccessResult(true, "Vacación eliminada exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error al eliminar vacación: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Calcula días de vacaciones disponibles para un empleado en un periodo.
    /// En Colombia: 15 días hábiles por año completo trabajado.
    /// </summary>
    public async Task<ServiceResult<int>> CalcularDiasDisponiblesAsync(int empleadoId, int periodo)
    {
        try
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return ServiceResult<int>.FailureResult("Empleado no encontrado");
            
            // Calcular días ganados usando el servicio centralizado
            var diasGanados = _dateCalculationService.CalcularDiasVacacionesGanados(empleado.FechaIngreso, periodo);
            
            // Obtener días ya tomados en este periodo
            var diasTomadosResult = await GetDiasTomadosEnPeriodoAsync(empleadoId, periodo);
            if (!diasTomadosResult.Success)
                return ServiceResult<int>.FailureResult(diasTomadosResult.Message);
            
            var diasDisponibles = diasGanados - diasTomadosResult.Data;
            
            return ServiceResult<int>.SuccessResult(Math.Max(0, diasDisponibles));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al calcular días disponibles");
            return ServiceResult<int>.FailureResult($"Error al calcular días disponibles: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<int>> GetDiasTomadosEnPeriodoAsync(int empleadoId, int periodo)
    {
        try
        {
            var vacaciones = await _vacacionRepository.GetByEmpleadoYPeriodoAsync(empleadoId, periodo);
            var diasTomados = vacaciones
                .Where(v => v.Estado == EstadoVacacion.Disfrutada || v.Estado == EstadoVacacion.Programada)
                .Sum(v => v.DiasTomados);
                
            return ServiceResult<int>.SuccessResult(diasTomados);
        }
        catch (Exception ex)
        {
            return ServiceResult<int>.FailureResult($"Error al obtener días tomados: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene el resumen de vacaciones de un empleado
    /// </summary>
    public async Task<ServiceResult<ResumenVacaciones>> GetResumenVacacionesAsync(int empleadoId)
    {
        try
        {
            var empleado = await _empleadoRepository.GetByIdWithRelationsAsync(empleadoId);
            if (empleado == null)
                return ServiceResult<ResumenVacaciones>.FailureResult("Empleado no encontrado");
            
            var periodoActual = DateTime.Today.Year;
            var diasGanados = _dateCalculationService.CalcularDiasVacacionesGanados(empleado.FechaIngreso, periodoActual);
            
            var diasTomadosResult = await GetDiasTomadosEnPeriodoAsync(empleadoId, periodoActual);
            var diasTomados = diasTomadosResult.Success ? diasTomadosResult.Data : 0;
            
            // Calcular días acumulados de periodos anteriores
            var diasAcumulados = await CalcularDiasAcumuladosAsync(empleadoId, periodoActual);
            
            var resumen = new ResumenVacaciones
            {
                EmpleadoId = empleadoId,
                NombreEmpleado = empleado.NombreCompleto,
                FechaIngreso = empleado.FechaIngreso,
                AntiguedadAnos = empleado.Antiguedad,
                PeriodoActual = periodoActual,
                DiasGanadosPeriodo = diasGanados,
                DiasTomadosPeriodo = diasTomados,
                DiasDisponiblesPeriodo = Math.Max(0, diasGanados - diasTomados),
                DiasAcumuladosAnteriores = diasAcumulados,
                TotalDiasDisponibles = Math.Max(0, (diasGanados - diasTomados) + diasAcumulados)
            };
            
            return ServiceResult<ResumenVacaciones>.SuccessResult(resumen);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener resumen de vacaciones");
            return ServiceResult<ResumenVacaciones>.FailureResult($"Error al obtener resumen: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene vacaciones programadas (futuras)
    /// </summary>
    public async Task<ServiceResult<IEnumerable<Vacacion>>> GetVacacionesProgramadasAsync(int? empleadoId = null)
    {
        try
        {
            var vacaciones = await _vacacionRepository.GetAllActiveAsync();
            var programadas = vacaciones
                .Where(v => v.Estado == EstadoVacacion.Programada && v.FechaInicio >= DateTime.Today);
                
            if (empleadoId.HasValue)
                programadas = programadas.Where(v => v.EmpleadoId == empleadoId.Value);
                
            return ServiceResult<IEnumerable<Vacacion>>.SuccessResult(programadas.OrderBy(v => v.FechaInicio));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Vacacion>>.FailureResult($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Marca vacaciones como disfrutadas (cuando pasa la fecha)
    /// </summary>
    public async Task<ServiceResult<bool>> MarcarComoDisfrutadaAsync(int id)
    {
        try
        {
            var vacacion = await _vacacionRepository.GetByIdAsync(id);
            if (vacacion == null)
                return ServiceResult<bool>.FailureResult("Vacación no encontrada");
                
            vacacion.Estado = EstadoVacacion.Disfrutada;
            vacacion.FechaModificacion = DateTime.Now;
            
            await _vacacionRepository.UpdateAsync(vacacion);
            await _vacacionRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.SuccessResult(true, "Vacación marcada como disfrutada");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Cancela una vacación programada
    /// </summary>
    public async Task<ServiceResult<bool>> CancelarVacacionAsync(int id, string motivo)
    {
        try
        {
            var vacacion = await _vacacionRepository.GetByIdAsync(id);
            if (vacacion == null)
                return ServiceResult<bool>.FailureResult("Vacación no encontrada");
                
            if (vacacion.Estado == EstadoVacacion.Disfrutada)
                return ServiceResult<bool>.FailureResult("No se puede cancelar una vacación ya disfrutada");
                
            vacacion.Estado = EstadoVacacion.Cancelada;
            vacacion.Observaciones = $"Cancelada: {motivo}. {vacacion.Observaciones}";
            vacacion.FechaModificacion = DateTime.Now;
            
            await _vacacionRepository.UpdateAsync(vacacion);
            await _vacacionRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.SuccessResult(true, "Vacación cancelada exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error: {ex.Message}");
        }
    }
    
    #region Métodos privados
    
    private async Task<List<string>> ValidarVacacionAsync(Vacacion vacacion, int? excludeId = null)
    {
        var errors = new List<string>();
        
        if (vacacion.EmpleadoId <= 0)
            errors.Add("Debe seleccionar un empleado");
            
        if (vacacion.FechaInicio == default)
            errors.Add("La fecha de inicio es obligatoria");
            
        if (vacacion.FechaFin == default)
            errors.Add("La fecha de fin es obligatoria");
            
        if (vacacion.FechaFin < vacacion.FechaInicio)
            errors.Add("La fecha de fin debe ser mayor o igual a la fecha de inicio");
            
        if (vacacion.PeriodoCorrespondiente <= 0)
            errors.Add("Debe especificar el periodo correspondiente");
            
        // Verificar traslape con otras vacaciones
        if (await _vacacionRepository.ExisteTraslapeAsync(
            vacacion.EmpleadoId, vacacion.FechaInicio, vacacion.FechaFin, excludeId))
        {
            errors.Add("Las fechas se traslapan con otra vacación existente");
        }
        
        return errors;
    }
    
    /// <summary>
    /// Calcula días acumulados de periodos anteriores
    /// </summary>
    private async Task<int> CalcularDiasAcumuladosAsync(int empleadoId, int periodoActual)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
        if (empleado == null) return 0;
        
        var totalAcumulado = 0;
        var primerPeriodo = empleado.FechaIngreso.Year;
        
        // Calcular desde el primer periodo hasta el anterior al actual (máximo 2 años según ley)
        var periodoInicio = Math.Max(primerPeriodo, periodoActual - 2);
        
        for (int periodo = periodoInicio; periodo < periodoActual; periodo++)
        {
            var diasGanados = _dateCalculationService.CalcularDiasVacacionesGanados(empleado.FechaIngreso, periodo);
            var diasTomadosResult = await GetDiasTomadosEnPeriodoAsync(empleadoId, periodo);
            var diasTomados = diasTomadosResult.Success ? diasTomadosResult.Data : 0;
            
            totalAcumulado += Math.Max(0, diasGanados - diasTomados);
        }
        
        return totalAcumulado;
    }
    
    private async Task<int> CalcularDiasDisponiblesInternamenteAsync(int empleadoId, int periodo, int vacacionIdExcluir)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
        if (empleado == null) return 0;
        
        var diasGanados = _dateCalculationService.CalcularDiasVacacionesGanados(empleado.FechaIngreso, periodo);
        
        var vacaciones = await _vacacionRepository.GetByEmpleadoYPeriodoAsync(empleadoId, periodo);
        var diasTomados = vacaciones
            .Where(v => v.Id != vacacionIdExcluir && 
                       (v.Estado == EstadoVacacion.Disfrutada || v.Estado == EstadoVacacion.Programada))
            .Sum(v => v.DiasTomados);
            
        return Math.Max(0, diasGanados - diasTomados);
    }
    
    #endregion
}
