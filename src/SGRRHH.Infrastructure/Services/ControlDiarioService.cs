using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de control diario
/// </summary>
public class ControlDiarioService : IControlDiarioService
{
    private readonly IRegistroDiarioRepository _registroRepository;
    private readonly AppDbContext _context;
    
    public ControlDiarioService(IRegistroDiarioRepository registroRepository, AppDbContext context)
    {
        _registroRepository = registroRepository;
        _context = context;
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
        if (registro.HoraEntrada.HasValue && registro.HoraSalida.HasValue)
        {
            if (registro.HoraSalida.Value <= registro.HoraEntrada.Value)
            {
                errors.Add("La hora de salida debe ser posterior a la hora de entrada");
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
            
        if (detalle.Horas <= 0)
            errors.Add("Las horas deben ser mayores a 0");
            
        if (detalle.Horas > 24)
            errors.Add("Las horas no pueden exceder 24");
            
        // Validar que el total de horas no exceda 24
        var totalHorasActuales = registro.DetallesActividades?.Sum(d => d.Horas) ?? 0;
        if (totalHorasActuales + detalle.Horas > 24)
            errors.Add($"El total de horas del día no puede exceder 24. Horas actuales: {totalHorasActuales}");
            
        if (errors.Any())
            return ServiceResult<DetalleActividad>.Fail(errors);
            
        detalle.RegistroDiarioId = registroId;
        detalle.Activo = true;
        detalle.FechaCreacion = DateTime.Now;
        detalle.Orden = (registro.DetallesActividades?.Count ?? 0) + 1;
        
        _context.Set<DetalleActividad>().Add(detalle);
        await _context.SaveChangesAsync();
        
        return ServiceResult<DetalleActividad>.Ok(detalle, "Actividad agregada exitosamente");
    }
    
    public async Task<ServiceResult> UpdateActividadAsync(DetalleActividad detalle)
    {
        var errors = new List<string>();
        
        var existing = await _context.Set<DetalleActividad>()
            .Include(d => d.RegistroDiario)
            .FirstOrDefaultAsync(d => d.Id == detalle.Id);
            
        if (existing == null)
            return ServiceResult.Fail("Detalle de actividad no encontrado");
            
        if (existing.RegistroDiario?.Estado == EstadoRegistroDiario.Completado || 
            existing.RegistroDiario?.Estado == EstadoRegistroDiario.Aprobado)
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
        
        await _context.SaveChangesAsync();
        
        return ServiceResult.Ok("Actividad actualizada exitosamente");
    }
    
    public async Task<ServiceResult> DeleteActividadAsync(int detalleId)
    {
        var detalle = await _context.Set<DetalleActividad>()
            .Include(d => d.RegistroDiario)
            .FirstOrDefaultAsync(d => d.Id == detalleId);
            
        if (detalle == null)
            return ServiceResult.Fail("Detalle de actividad no encontrado");
            
        if (detalle.RegistroDiario?.Estado == EstadoRegistroDiario.Completado || 
            detalle.RegistroDiario?.Estado == EstadoRegistroDiario.Aprobado)
            return ServiceResult.Fail("No se pueden eliminar actividades de un registro completado");
            
        _context.Set<DetalleActividad>().Remove(detalle);
        await _context.SaveChangesAsync();
        
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
}
