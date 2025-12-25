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
            return ServiceResult<IEnumerable<Vacacion>>.Ok(vacaciones);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Vacacion>>.Fail($"Error al obtener vacaciones: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Vacacion>> GetByIdAsync(int id)
    {
        try
        {
            var vacacion = await _vacacionRepository.GetByIdAsync(id);
            if (vacacion == null)
                return ServiceResult<Vacacion>.Fail("Vacación no encontrada");
                
            return ServiceResult<Vacacion>.Ok(vacacion);
        }
        catch (Exception ex)
        {
            return ServiceResult<Vacacion>.Fail($"Error al obtener vacación: {ex.Message}");
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
                return ServiceResult<Vacacion>.Fail(errors);
            }
            
            // Validar solapamiento con permisos (validación cruzada)
            var validacionPermisos = await _absenceValidationService.TienePermisosEnRangoAsync(
                vacacion.EmpleadoId, vacacion.FechaInicio, vacacion.FechaFin);
            if (validacionPermisos.Success && validacionPermisos.Data)
            {
                return ServiceResult<Vacacion>.Fail("El empleado tiene permisos aprobados o pendientes en las fechas seleccionadas");
            }
            
            // Calcular días tomados usando el servicio centralizado (sin festivos colombianos)
            vacacion.DiasTomados = _dateCalculationService.CalcularDiasHabilesSinFestivos(vacacion.FechaInicio, vacacion.FechaFin);
            
            // Verificar que no exceda los días disponibles
            var diasDisponibles = await CalcularDiasDisponiblesAsync(vacacion.EmpleadoId, vacacion.PeriodoCorrespondiente);
            if (!diasDisponibles.Success)
                return ServiceResult<Vacacion>.Fail(diasDisponibles.Message);
                
            if (vacacion.DiasTomados > diasDisponibles.Data)
            {
                return ServiceResult<Vacacion>.Fail(
                    $"Los días solicitados ({vacacion.DiasTomados}) exceden los días disponibles ({diasDisponibles.Data})");
            }
            
            vacacion.Activo = true;
            vacacion.FechaCreacion = DateTime.Now;
            vacacion.FechaSolicitud = DateTime.Now;
            vacacion.Estado = EstadoVacacion.Pendiente; // Inicia como pendiente de aprobación
            
            await _vacacionRepository.AddAsync(vacacion);
            await _vacacionRepository.SaveChangesAsync();
            
            _logger?.LogInformation("Vacación creada exitosamente para empleado {EmpleadoId}", vacacion.EmpleadoId);
            return ServiceResult<Vacacion>.Ok(vacacion, "Vacación registrada exitosamente");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al crear vacación");
            return ServiceResult<Vacacion>.Fail($"Error al crear vacación: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Vacacion>> UpdateAsync(Vacacion vacacion)
    {
        try
        {
            var existing = await _vacacionRepository.GetByIdAsync(vacacion.Id);
            if (existing == null)
                return ServiceResult<Vacacion>.Fail("Vacación no encontrada");
            
            var errors = await ValidarVacacionAsync(vacacion, vacacion.Id);
            if (errors.Any())
                return ServiceResult<Vacacion>.Fail(errors);
            
            // Validar solapamiento con permisos (validación cruzada)
            var validacionPermisos = await _absenceValidationService.TienePermisosEnRangoAsync(
                vacacion.EmpleadoId, vacacion.FechaInicio, vacacion.FechaFin);
            if (validacionPermisos.Success && validacionPermisos.Data)
            {
                return ServiceResult<Vacacion>.Fail("El empleado tiene permisos aprobados o pendientes en las fechas seleccionadas");
            }
            
            // Recalcular días usando el servicio centralizado (sin festivos colombianos)
            vacacion.DiasTomados = _dateCalculationService.CalcularDiasHabilesSinFestivos(vacacion.FechaInicio, vacacion.FechaFin);
            
            // Verificar días disponibles (excluyendo esta vacación)
            var diasDisponibles = await CalcularDiasDisponiblesInternamenteAsync(
                vacacion.EmpleadoId, vacacion.PeriodoCorrespondiente, vacacion.Id);
                
            if (vacacion.DiasTomados > diasDisponibles)
            {
                return ServiceResult<Vacacion>.Fail(
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
            
            return ServiceResult<Vacacion>.Ok(existing, "Vacación actualizada exitosamente");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar vacación");
            return ServiceResult<Vacacion>.Fail($"Error al actualizar vacación: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var vacacion = await _vacacionRepository.GetByIdAsync(id);
            if (vacacion == null)
                return ServiceResult<bool>.Fail("Vacación no encontrada");
            
            // No permitir eliminar vacaciones ya disfrutadas
            if (vacacion.Estado == EstadoVacacion.Disfrutada && vacacion.FechaFin < DateTime.Today)
            {
                return ServiceResult<bool>.Fail("No se puede eliminar una vacación ya disfrutada");
            }
            
            await _vacacionRepository.DeleteAsync(id);
            await _vacacionRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.Ok(true, "Vacación eliminada exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error al eliminar vacación: {ex.Message}");
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
                return ServiceResult<int>.Fail("Empleado no encontrado");
            
            // Calcular días ganados usando el servicio centralizado
            var diasGanados = _dateCalculationService.CalcularDiasVacacionesGanados(empleado.FechaIngreso, periodo);
            
            // Obtener días ya tomados en este periodo
            var diasTomadosResult = await GetDiasTomadosEnPeriodoAsync(empleadoId, periodo);
            if (!diasTomadosResult.Success)
                return ServiceResult<int>.Fail(diasTomadosResult.Message);
            
            var diasDisponibles = diasGanados - diasTomadosResult.Data;
            
            return ServiceResult<int>.Ok(Math.Max(0, diasDisponibles));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al calcular días disponibles");
            return ServiceResult<int>.Fail($"Error al calcular días disponibles: {ex.Message}");
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
                
            return ServiceResult<int>.Ok(diasTomados);
        }
        catch (Exception ex)
        {
            return ServiceResult<int>.Fail($"Error al obtener días tomados: {ex.Message}");
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
                return ServiceResult<ResumenVacaciones>.Fail("Empleado no encontrado");
            
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
            
            return ServiceResult<ResumenVacaciones>.Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener resumen de vacaciones");
            return ServiceResult<ResumenVacaciones>.Fail($"Error al obtener resumen: {ex.Message}");
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
                
            return ServiceResult<IEnumerable<Vacacion>>.Ok(programadas.OrderBy(v => v.FechaInicio));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Vacacion>>.Fail($"Error: {ex.Message}");
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
                return ServiceResult<bool>.Fail("Vacación no encontrada");
                
            vacacion.Estado = EstadoVacacion.Disfrutada;
            vacacion.FechaModificacion = DateTime.Now;
            
            await _vacacionRepository.UpdateAsync(vacacion);
            await _vacacionRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.Ok(true, "Vacación marcada como disfrutada");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error: {ex.Message}");
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
                return ServiceResult<bool>.Fail("Vacación no encontrada");
                
            if (vacacion.Estado == EstadoVacacion.Disfrutada)
                return ServiceResult<bool>.Fail("No se puede cancelar una vacación ya disfrutada");
                
            vacacion.Estado = EstadoVacacion.Cancelada;
            vacacion.Observaciones = $"Cancelada: {motivo}. {vacacion.Observaciones}";
            vacacion.FechaModificacion = DateTime.Now;
            
            await _vacacionRepository.UpdateAsync(vacacion);
            await _vacacionRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.Ok(true, "Vacación cancelada exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Aprueba una solicitud de vacaciones pendiente
    /// </summary>
    public async Task<ServiceResult<bool>> AprobarVacacionAsync(int id, int aprobadorId)
    {
        try
        {
            var vacacion = await _vacacionRepository.GetByIdAsync(id);
            if (vacacion == null)
                return ServiceResult<bool>.Fail("Vacación no encontrada");
                
            if (vacacion.Estado != EstadoVacacion.Pendiente)
                return ServiceResult<bool>.Fail("Solo se pueden aprobar vacaciones pendientes");
            
            vacacion.Estado = EstadoVacacion.Aprobada;
            vacacion.AprobadoPorId = aprobadorId;
            vacacion.FechaAprobacion = DateTime.Now;
            vacacion.FechaModificacion = DateTime.Now;
            
            await _vacacionRepository.UpdateAsync(vacacion);
            await _vacacionRepository.SaveChangesAsync();
            
            _logger?.LogInformation("Vacación {Id} aprobada por usuario {AprobadorId}", id, aprobadorId);
            return ServiceResult<bool>.Ok(true, "Vacación aprobada exitosamente");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al aprobar vacación {Id}", id);
            return ServiceResult<bool>.Fail($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Rechaza una solicitud de vacaciones pendiente
    /// </summary>
    public async Task<ServiceResult<bool>> RechazarVacacionAsync(int id, int aprobadorId, string motivo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return ServiceResult<bool>.Fail("Debe especificar el motivo del rechazo");
                
            var vacacion = await _vacacionRepository.GetByIdAsync(id);
            if (vacacion == null)
                return ServiceResult<bool>.Fail("Vacación no encontrada");
                
            if (vacacion.Estado != EstadoVacacion.Pendiente)
                return ServiceResult<bool>.Fail("Solo se pueden rechazar vacaciones pendientes");
            
            vacacion.Estado = EstadoVacacion.Rechazada;
            vacacion.AprobadoPorId = aprobadorId;
            vacacion.FechaAprobacion = DateTime.Now;
            vacacion.MotivoRechazo = motivo;
            vacacion.FechaModificacion = DateTime.Now;
            
            await _vacacionRepository.UpdateAsync(vacacion);
            await _vacacionRepository.SaveChangesAsync();
            
            _logger?.LogInformation("Vacación {Id} rechazada por usuario {AprobadorId}. Motivo: {Motivo}", id, aprobadorId, motivo);
            return ServiceResult<bool>.Ok(true, "Vacación rechazada");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al rechazar vacación {Id}", id);
            return ServiceResult<bool>.Fail($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Programa una vacación aprobada (confirma las fechas)
    /// </summary>
    public async Task<ServiceResult<bool>> ProgramarVacacionAsync(int id)
    {
        try
        {
            var vacacion = await _vacacionRepository.GetByIdAsync(id);
            if (vacacion == null)
                return ServiceResult<bool>.Fail("Vacación no encontrada");
                
            if (vacacion.Estado != EstadoVacacion.Aprobada)
                return ServiceResult<bool>.Fail("Solo se pueden programar vacaciones aprobadas");
            
            vacacion.Estado = EstadoVacacion.Programada;
            vacacion.FechaModificacion = DateTime.Now;
            
            await _vacacionRepository.UpdateAsync(vacacion);
            await _vacacionRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.Ok(true, "Vacación programada exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene las vacaciones pendientes de aprobación
    /// </summary>
    public async Task<ServiceResult<IEnumerable<Vacacion>>> GetVacacionesPendientesAsync()
    {
        try
        {
            var vacaciones = await _vacacionRepository.GetByEstadoAsync(EstadoVacacion.Pendiente);
            return ServiceResult<IEnumerable<Vacacion>>.Ok(vacaciones.OrderBy(v => v.FechaSolicitud));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Vacacion>>.Fail($"Error: {ex.Message}");
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
        
        // Fix #3: Validar que la fecha de inicio no sea en el pasado (para nuevas solicitudes)
        if (excludeId == null && vacacion.FechaInicio.Date < DateTime.Today)
            errors.Add("La fecha de inicio no puede ser en el pasado");
            
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
        
        // Calcular desde el primer periodo hasta el anterior al actual (máximo 4 años según Art. 187 CST)
        var periodoInicio = Math.Max(primerPeriodo, periodoActual - 4);
        
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
