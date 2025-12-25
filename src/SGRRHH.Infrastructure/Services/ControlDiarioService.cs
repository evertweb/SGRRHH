using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de control diario
/// </summary>
public class ControlDiarioService : IControlDiarioService
{
    private readonly IRegistroDiarioRepository _registroRepository;
    private readonly IActividadService? _actividadService;
    private readonly IProyectoService? _proyectoService;
    
    public ControlDiarioService(
        IRegistroDiarioRepository registroRepository,
        IActividadService? actividadService = null,
        IProyectoService? proyectoService = null)
    {
        _registroRepository = registroRepository;
        _actividadService = actividadService;
        _proyectoService = proyectoService;
    }
    
    public async Task<RegistroDiario?> GetRegistroByFechaEmpleadoAsync(DateTime fecha, int empleadoId)
    {
        return await _registroRepository.GetByFechaEmpleadoAsync(fecha, empleadoId);
    }
    
    public async Task<RegistroDiario?> GetByIdAsync(int id)
    {
        return await _registroRepository.GetByIdWithDetallesAsync(id);
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoRangoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        return await _registroRepository.GetByEmpleadoRangoFechasAsync(empleadoId, fechaInicio, fechaFin);
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoMesActualAsync(int empleadoId)
    {
        return await _registroRepository.GetByEmpleadoMesActualAsync(empleadoId);
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByFechaAsync(DateTime fecha)
    {
        return await _registroRepository.GetByFechaAsync(fecha);
    }
    
    public async Task<ServiceResult<RegistroDiario>> SaveRegistroAsync(RegistroDiario registro)
    {
        var errors = new List<string>();
        
        // Validaciones básicas
        if (registro.EmpleadoId <= 0)
            errors.Add("Debe seleccionar un empleado");
            
        if (registro.Fecha == default)
            errors.Add("Debe especificar una fecha");
            
        // Validar que la fecha no sea futura
        if (registro.Fecha.Date > DateTime.Today)
            errors.Add("No se pueden registrar actividades para fechas futuras");
            
        // Validar horas de entrada/salida
        decimal horasTrabajadas = 24m; // Default si no hay horas
        if (registro.HoraEntrada.HasValue && registro.HoraSalida.HasValue)
        {
            if (registro.HoraSalida.Value <= registro.HoraEntrada.Value)
            {
                errors.Add("La hora de salida debe ser posterior a la hora de entrada");
            }
            else
            {
                horasTrabajadas = (decimal)(registro.HoraSalida.Value - registro.HoraEntrada.Value).TotalHours;
            }
        }
        
        // FIX #1 y #3: Validar que la suma de horas de actividades no exceda las horas trabajadas
        if (registro.DetallesActividades?.Any() == true)
        {
            var totalHorasActividades = registro.DetallesActividades.Sum(d => d.Horas);
            if (totalHorasActividades > horasTrabajadas)
            {
                errors.Add($"El total de horas de actividades ({totalHorasActividades:N1}h) excede las horas trabajadas ({horasTrabajadas:N1}h)");
            }
        }
        
        if (errors.Any())
            return ServiceResult<RegistroDiario>.Fail(errors);
        
        // Verificar si ya existe un registro para esta fecha y empleado
        var existente = await _registroRepository.GetByFechaEmpleadoAsync(registro.Fecha, registro.EmpleadoId);
        
        if (existente != null && registro.Id == 0)
        {
            // Ya existe un registro, actualizarlo
            existente.HoraEntrada = registro.HoraEntrada;
            existente.HoraSalida = registro.HoraSalida;
            existente.Observaciones = registro.Observaciones;
            existente.FechaModificacion = DateTime.Now;
            
            await _registroRepository.UpdateAsync(existente);
            await _registroRepository.SaveChangesAsync();
            
            return ServiceResult<RegistroDiario>.Ok(existente, "Registro actualizado exitosamente");
        }
        else if (registro.Id > 0)
        {
            // Actualizar registro existente por ID
            var registroExistente = await _registroRepository.GetByIdAsync(registro.Id);
            if (registroExistente == null)
                return ServiceResult<RegistroDiario>.Fail("Registro no encontrado");
                
            registroExistente.HoraEntrada = registro.HoraEntrada;
            registroExistente.HoraSalida = registro.HoraSalida;
            registroExistente.Observaciones = registro.Observaciones;
            registroExistente.FechaModificacion = DateTime.Now;
            
            await _registroRepository.UpdateAsync(registroExistente);
            await _registroRepository.SaveChangesAsync();
            
            return ServiceResult<RegistroDiario>.Ok(registroExistente, "Registro actualizado exitosamente");
        }
        else
        {
            // Crear nuevo registro
            registro.Activo = true;
            registro.FechaCreacion = DateTime.Now;
            registro.Estado = EstadoRegistroDiario.Borrador;
            
            await _registroRepository.AddAsync(registro);
            await _registroRepository.SaveChangesAsync();
            
            return ServiceResult<RegistroDiario>.Ok(registro, "Registro creado exitosamente");
        }
    }
    
    public async Task<ServiceResult<DetalleActividad>> AddActividadAsync(int registroId, DetalleActividad detalle)
    {
        var errors = new List<string>();
        
        var registro = await _registroRepository.GetByIdWithDetallesAsync(registroId);
        if (registro == null)
            return ServiceResult<DetalleActividad>.Fail("Registro no encontrado");
            
        if (registro.Estado == EstadoRegistroDiario.Completado || registro.Estado == EstadoRegistroDiario.Aprobado)
            return ServiceResult<DetalleActividad>.Fail("No se pueden agregar actividades a un registro completado");
            
        // Validaciones
        if (detalle.ActividadId <= 0)
            errors.Add("Debe seleccionar una actividad");
        
        // FIX #4: Validar que ActividadId exista
        if (detalle.ActividadId > 0 && _actividadService != null)
        {
            var actividad = await _actividadService.GetByIdAsync(detalle.ActividadId);
            if (actividad == null)
                errors.Add($"La actividad con Id {detalle.ActividadId} no existe");
        }
        
        // FIX #5: Validar que ProyectoId exista (si se proporciona)
        if (detalle.ProyectoId.HasValue && detalle.ProyectoId.Value > 0 && _proyectoService != null)
        {
            var proyecto = await _proyectoService.GetByIdAsync(detalle.ProyectoId.Value);
            if (proyecto == null)
                errors.Add($"El proyecto con Id {detalle.ProyectoId} no existe");
        }
            
        if (detalle.Horas <= 0)
            errors.Add("Las horas deben ser mayores a 0");
            
        if (detalle.Horas > 24)
            errors.Add("Las horas no pueden exceder 24");
        
        // FIX #3: Calcular horas trabajadas reales desde HoraEntrada y HoraSalida
        decimal horasTrabajadasMax = 24m; // Default
        if (registro.HoraEntrada.HasValue && registro.HoraSalida.HasValue && 
            registro.HoraSalida.Value > registro.HoraEntrada.Value)
        {
            horasTrabajadasMax = (decimal)(registro.HoraSalida.Value - registro.HoraEntrada.Value).TotalHours;
        }
            
        // Validar que el total de horas no exceda las horas trabajadas
        var totalHorasActuales = registro.DetallesActividades?.Sum(d => d.Horas) ?? 0;
        if (totalHorasActuales + detalle.Horas > horasTrabajadasMax)
            errors.Add($"El total de horas ({totalHorasActuales + detalle.Horas:N1}h) excede las horas trabajadas ({horasTrabajadasMax:N1}h)");
            
        if (errors.Any())
            return ServiceResult<DetalleActividad>.Fail(errors);
            
        detalle.RegistroDiarioId = registroId;
        detalle.Activo = true;
        detalle.FechaCreacion = DateTime.Now;
        detalle.Orden = (registro.DetallesActividades?.Count ?? 0) + 1;
        
        await _registroRepository.AddDetalleAsync(registroId, detalle);
        await _registroRepository.SaveChangesAsync();
        
        return ServiceResult<DetalleActividad>.Ok(detalle, "Actividad agregada exitosamente");
    }
    
    public async Task<ServiceResult> UpdateActividadAsync(DetalleActividad detalle)
    {
        var errors = new List<string>();
        
        // Obtener el detalle existente a través del repositorio
        var existing = await _registroRepository.GetDetalleByIdAsync(detalle.Id);
            
        if (existing == null)
            return ServiceResult.Fail("Detalle de actividad no encontrado");
        
        // Obtener el registro para verificar el estado
        var registro = await _registroRepository.GetByIdWithDetallesAsync(existing.RegistroDiarioId);
            
        if (registro?.Estado == EstadoRegistroDiario.Completado || 
            registro?.Estado == EstadoRegistroDiario.Aprobado)
            return ServiceResult.Fail("No se pueden modificar actividades de un registro completado");
            
        // Validaciones
        if (detalle.ActividadId <= 0)
            errors.Add("Debe seleccionar una actividad");
            
        if (detalle.Horas <= 0)
            errors.Add("Las horas deben ser mayores a 0");
            
        if (detalle.Horas > 24)
            errors.Add("Las horas no pueden exceder 24");
            
        if (errors.Any())
            return ServiceResult.Fail(errors);
            
        existing.ActividadId = detalle.ActividadId;
        existing.ProyectoId = detalle.ProyectoId;
        existing.Horas = detalle.Horas;
        existing.Descripcion = detalle.Descripcion;
        existing.HoraInicio = detalle.HoraInicio;
        existing.HoraFin = detalle.HoraFin;
        existing.FechaModificacion = DateTime.Now;
        
        await _registroRepository.UpdateDetalleAsync(existing);
        await _registroRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Actividad actualizada exitosamente");
    }
    
    public async Task<ServiceResult> DeleteActividadAsync(int detalleId)
    {
        // Obtener el detalle existente a través del repositorio
        var detalle = await _registroRepository.GetDetalleByIdAsync(detalleId);
            
        if (detalle == null)
            return ServiceResult.Fail("Detalle de actividad no encontrado");
            
        // Obtener el registro para verificar el estado
        var registro = await _registroRepository.GetByIdWithDetallesAsync(detalle.RegistroDiarioId);
            
        if (registro?.Estado == EstadoRegistroDiario.Completado || 
            registro?.Estado == EstadoRegistroDiario.Aprobado)
            return ServiceResult.Fail("No se pueden eliminar actividades de un registro completado");
            
        await _registroRepository.DeleteDetalleAsync(detalle.RegistroDiarioId, detalleId);
        await _registroRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Actividad eliminada exitosamente");
    }
    
    public async Task<ServiceResult> CompletarRegistroAsync(int registroId)
    {
        var registro = await _registroRepository.GetByIdWithDetallesAsync(registroId);
        if (registro == null)
            return ServiceResult.Fail("Registro no encontrado");
            
        var errors = new List<string>();
        
        // Validar que tenga hora de entrada y salida
        if (!registro.HoraEntrada.HasValue)
            errors.Add("Debe registrar la hora de entrada");
            
        if (!registro.HoraSalida.HasValue)
            errors.Add("Debe registrar la hora de salida");
            
        // Validar que tenga al menos una actividad
        if (registro.DetallesActividades == null || !registro.DetallesActividades.Any())
            errors.Add("Debe agregar al menos una actividad");
            
        if (errors.Any())
            return ServiceResult.Fail(errors);
            
        registro.Estado = EstadoRegistroDiario.Completado;
        registro.FechaModificacion = DateTime.Now;
        
        await _registroRepository.UpdateAsync(registro);
        await _registroRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Registro completado exitosamente");
    }
    
    /// <summary>
    /// FIX #6: Aprobar un registro completado (solo supervisores/admin)
    /// </summary>
    public async Task<ServiceResult> AprobarRegistroAsync(int registroId)
    {
        var registro = await _registroRepository.GetByIdWithDetallesAsync(registroId);
        if (registro == null)
            return ServiceResult.Fail("Registro no encontrado");
            
        if (registro.Estado != EstadoRegistroDiario.Completado)
            return ServiceResult.Fail("Solo se pueden aprobar registros en estado Completado");
            
        registro.Estado = EstadoRegistroDiario.Aprobado;
        registro.FechaModificacion = DateTime.Now;
        
        await _registroRepository.UpdateAsync(registro);
        await _registroRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Registro aprobado exitosamente");
    }
    
    /// <summary>
    /// FIX #6: Rechazar un registro completado (vuelve a Borrador)
    /// </summary>
    public async Task<ServiceResult> RechazarRegistroAsync(int registroId, string? motivo = null)
    {
        var registro = await _registroRepository.GetByIdWithDetallesAsync(registroId);
        if (registro == null)
            return ServiceResult.Fail("Registro no encontrado");
            
        if (registro.Estado != EstadoRegistroDiario.Completado && registro.Estado != EstadoRegistroDiario.Aprobado)
            return ServiceResult.Fail("Solo se pueden rechazar registros en estado Completado o Aprobado");
            
        registro.Estado = EstadoRegistroDiario.Borrador;
        registro.Observaciones = string.IsNullOrEmpty(motivo) 
            ? registro.Observaciones 
            : $"{registro.Observaciones}\n[RECHAZADO] {motivo}";
        registro.FechaModificacion = DateTime.Now;
        
        await _registroRepository.UpdateAsync(registro);
        await _registroRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Registro rechazado y devuelto a Borrador");
    }
    
    public async Task<decimal> GetTotalHorasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        return await _registroRepository.GetTotalHorasByEmpleadoRangoAsync(empleadoId, fechaInicio, fechaFin);
    }
    
    public async Task<decimal> GetHorasMesActualAsync(int empleadoId)
    {
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        return await GetTotalHorasAsync(empleadoId, inicioMes, finMes);
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByMesAnioAsync(int mes, int anio)
    {
        var fechaInicio = new DateTime(anio, mes, 1);
        var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
        return await _registroRepository.GetByRangoFechasAsync(fechaInicio, fechaFin);
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoMesAnioAsync(int empleadoId, int mes, int anio)
    {
        var fechaInicio = new DateTime(anio, mes, 1);
        var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
        return await _registroRepository.GetByEmpleadoRangoFechasAsync(empleadoId, fechaInicio, fechaFin);
    }
    
    public async Task<ServiceResult> DeleteRegistroAsync(int registroId)
    {
        var registro = await _registroRepository.GetByIdWithDetallesAsync(registroId);
        if (registro == null)
            return ServiceResult.Fail("Registro no encontrado");
            
        if (registro.Estado == EstadoRegistroDiario.Completado || 
            registro.Estado == EstadoRegistroDiario.Aprobado)
            return ServiceResult.Fail("No se puede eliminar un registro completado o aprobado");
            
        await _registroRepository.DeleteAsync(registroId);
        await _registroRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Registro eliminado exitosamente");
    }
}
