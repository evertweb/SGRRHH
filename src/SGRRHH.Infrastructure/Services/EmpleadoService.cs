using Microsoft.Extensions.Logging;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de empleados
/// </summary>
public class EmpleadoService : IEmpleadoService
{
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IDateCalculationService _dateCalculationService;
    private readonly ILogger<EmpleadoService>? _logger;
    private readonly string _fotosPath;
    
    public EmpleadoService(
        IEmpleadoRepository empleadoRepository,
        IDateCalculationService dateCalculationService,
        ILogger<EmpleadoService>? logger = null)
    {
        _empleadoRepository = empleadoRepository;
        _dateCalculationService = dateCalculationService;
        _logger = logger;
        
        // Configurar ruta para fotos en %LOCALAPPDATA%\SGRRHH
        _fotosPath = AppDataPaths.Fotos;
        AppDataPaths.EnsureDirectory(_fotosPath);
    }
    
    public async Task<IEnumerable<Empleado>> GetAllAsync()
    {
        return await _empleadoRepository.GetAllActiveWithRelationsAsync();
    }
    
    public async Task<Empleado?> GetByIdAsync(int id)
    {
        return await _empleadoRepository.GetByIdWithRelationsAsync(id);
    }
    
    public async Task<IEnumerable<Empleado>> SearchAsync(string searchTerm)
    {
        return await _empleadoRepository.SearchAsync(searchTerm);
    }
    
    public async Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId)
    {
        return await _empleadoRepository.GetByDepartamentoAsync(departamentoId);
    }
    
    public async Task<IEnumerable<Empleado>> GetByEstadoAsync(EstadoEmpleado estado)
    {
        return await _empleadoRepository.GetByEstadoAsync(estado);
    }
    
    public async Task<ServiceResult<Empleado>> CreateAsync(Empleado empleado)
    {
        var errors = new List<string>();
        
        // Validaciones
        if (string.IsNullOrWhiteSpace(empleado.Cedula))
            errors.Add("La cédula es obligatoria");
            
        if (string.IsNullOrWhiteSpace(empleado.Nombres))
            errors.Add("Los nombres son obligatorios");
            
        if (string.IsNullOrWhiteSpace(empleado.Apellidos))
            errors.Add("Los apellidos son obligatorios");
            
        if (empleado.FechaIngreso == default)
            errors.Add("La fecha de ingreso es obligatoria");
            
        // Validar unicidad de código
        if (!string.IsNullOrWhiteSpace(empleado.Codigo))
        {
            if (await _empleadoRepository.ExistsCodigoAsync(empleado.Codigo))
                errors.Add($"Ya existe un empleado con el código {empleado.Codigo}");
        }
        else
        {
            // Generar código automático
            empleado.Codigo = await _empleadoRepository.GetNextCodigoAsync();
        }
        
        // Validar unicidad de cédula
        if (await _empleadoRepository.ExistsCedulaAsync(empleado.Cedula))
            errors.Add($"Ya existe un empleado con la cédula {empleado.Cedula}");
            
        if (errors.Any())
            return ServiceResult<Empleado>.Fail(errors);
            
        // El estado se determina según el rol del usuario que crea
        // Si viene con CreadoPorId establecido, verificar rol
        // Nota: El estado se establece en el ViewModel según el rol
        empleado.FechaSolicitud = DateTime.Now;
        empleado.Activo = empleado.Estado == EstadoEmpleado.Activo;
        empleado.FechaCreacion = DateTime.Now;
        
        await _empleadoRepository.AddAsync(empleado);
        await _empleadoRepository.SaveChangesAsync();
        
        var mensaje = empleado.Estado == EstadoEmpleado.PendienteAprobacion 
            ? "Solicitud de empleado enviada. Pendiente de aprobación."
            : "Empleado creado exitosamente";
        
        _logger?.LogInformation("Empleado {Codigo} creado exitosamente", empleado.Codigo);
        return ServiceResult<Empleado>.Ok(empleado, mensaje);
    }
    
    /// <summary>
    /// Crea un empleado determinando el estado según el rol del usuario
    /// Esta lógica de negocio estaba antes en el ViewModel
    /// </summary>
    public async Task<ServiceResult<Empleado>> CreateWithRoleAsync(Empleado empleado, int usuarioId, RolUsuario rolUsuario)
    {
        _logger?.LogInformation("Creando empleado con rol {Rol}", rolUsuario);
        
        // Establecer quién creó el empleado
        empleado.CreadoPorId = usuarioId;
        
        // Determinar estado según rol del usuario
        // Solo Operadores (Secretarias) crean solicitudes pendientes
        // Admin y Aprobadores crean empleados directamente activos
        if (rolUsuario == RolUsuario.Operador)
        {
            empleado.Estado = EstadoEmpleado.PendienteAprobacion;
        }
        else
        {
            empleado.Estado = EstadoEmpleado.Activo;
        }
        
        return await CreateAsync(empleado);
    }
    
    public async Task<ServiceResult> UpdateAsync(Empleado empleado)
    {
        var existing = await _empleadoRepository.GetByIdAsync(empleado.Id);
        if (existing == null)
            return ServiceResult.Fail("Empleado no encontrado");
            
        var errors = new List<string>();
        
        // Validaciones
        if (string.IsNullOrWhiteSpace(empleado.Cedula))
            errors.Add("La cédula es obligatoria");
            
        if (string.IsNullOrWhiteSpace(empleado.Nombres))
            errors.Add("Los nombres son obligatorios");
            
        if (string.IsNullOrWhiteSpace(empleado.Apellidos))
            errors.Add("Los apellidos son obligatorios");
            
        // Validar unicidad de código (excluyendo el actual)
        if (await _empleadoRepository.ExistsCodigoAsync(empleado.Codigo, empleado.Id))
            errors.Add($"Ya existe otro empleado con el código {empleado.Codigo}");
            
        // Validar unicidad de cédula (excluyendo el actual)
        if (await _empleadoRepository.ExistsCedulaAsync(empleado.Cedula, empleado.Id))
            errors.Add($"Ya existe otro empleado con la cédula {empleado.Cedula}");
            
        if (errors.Any())
            return ServiceResult.Fail(errors);
            
        // Actualizar campos
        existing.Codigo = empleado.Codigo;
        existing.Cedula = empleado.Cedula;
        existing.Nombres = empleado.Nombres;
        existing.Apellidos = empleado.Apellidos;
        existing.FechaNacimiento = empleado.FechaNacimiento;
        existing.Genero = empleado.Genero;
        existing.EstadoCivil = empleado.EstadoCivil;
        existing.Direccion = empleado.Direccion;
        existing.Telefono = empleado.Telefono;
        existing.TelefonoEmergencia = empleado.TelefonoEmergencia;
        existing.ContactoEmergencia = empleado.ContactoEmergencia;
        existing.Email = empleado.Email;
        existing.FechaIngreso = empleado.FechaIngreso;
        existing.FechaRetiro = empleado.FechaRetiro;
        existing.Estado = empleado.Estado;
        existing.TipoContrato = empleado.TipoContrato;
        existing.CargoId = empleado.CargoId;
        existing.DepartamentoId = empleado.DepartamentoId;
        existing.SupervisorId = empleado.SupervisorId;
        existing.Observaciones = empleado.Observaciones;
        existing.FechaModificacion = DateTime.Now;
        
        await _empleadoRepository.UpdateAsync(existing);
        await _empleadoRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Empleado actualizado exitosamente");
    }
    
    public async Task<ServiceResult> DeactivateAsync(int id)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(id);
        if (empleado == null)
            return ServiceResult.Fail("Empleado no encontrado");
            
        empleado.Estado = EstadoEmpleado.Retirado;
        empleado.FechaRetiro = DateTime.Now;
        empleado.Activo = false;
        empleado.FechaModificacion = DateTime.Now;
        
        await _empleadoRepository.UpdateAsync(empleado);
        await _empleadoRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Empleado desactivado exitosamente");
    }
    
    public async Task<ServiceResult> DeletePermanentlyAsync(int id)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(id);
        if (empleado == null)
            return ServiceResult.Fail("Empleado no encontrado");
        
        try
        {
            // Eliminar foto si existe
            if (!string.IsNullOrEmpty(empleado.FotoPath) && File.Exists(empleado.FotoPath))
            {
                File.Delete(empleado.FotoPath);
            }
            
            // Eliminar el empleado permanentemente
            await _empleadoRepository.DeleteAsync(id);
            await _empleadoRepository.SaveChangesAsync();
            
            return ServiceResult.Ok("Empleado eliminado permanentemente");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Error al eliminar el empleado: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult> ReactivateAsync(int id)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(id);
        if (empleado == null)
            return ServiceResult.Fail("Empleado no encontrado");
            
        empleado.Estado = EstadoEmpleado.Activo;
        empleado.FechaRetiro = null;
        empleado.Activo = true;
        empleado.FechaModificacion = DateTime.Now;
        
        await _empleadoRepository.UpdateAsync(empleado);
        await _empleadoRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Empleado reactivado exitosamente");
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        return await _empleadoRepository.GetNextCodigoAsync();
    }
    
    public async Task<ServiceResult<string>> SaveFotoAsync(int empleadoId, byte[] fotoData, string extension)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
        if (empleado == null)
            return ServiceResult<string>.Fail("Empleado no encontrado");
            
        try
        {
            // Eliminar foto anterior si existe
            if (!string.IsNullOrEmpty(empleado.FotoPath) && File.Exists(empleado.FotoPath))
            {
                File.Delete(empleado.FotoPath);
            }
            
            // Guardar nueva foto
            var fileName = $"{empleado.Codigo}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(_fotosPath, fileName);
            
            await File.WriteAllBytesAsync(filePath, fotoData);
            
            // Actualizar ruta en el empleado
            empleado.FotoPath = filePath;
            empleado.FechaModificacion = DateTime.Now;
            
            await _empleadoRepository.UpdateAsync(empleado);
            await _empleadoRepository.SaveChangesAsync();
            
            return ServiceResult<string>.Ok(filePath, "Foto guardada exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Fail($"Error al guardar la foto: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult> DeleteFotoAsync(int empleadoId)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
        if (empleado == null)
            return ServiceResult.Fail("Empleado no encontrado");
            
        try
        {
            if (!string.IsNullOrEmpty(empleado.FotoPath) && File.Exists(empleado.FotoPath))
            {
                File.Delete(empleado.FotoPath);
            }
            
            empleado.FotoPath = null;
            empleado.FechaModificacion = DateTime.Now;
            
            await _empleadoRepository.UpdateAsync(empleado);
            await _empleadoRepository.SaveChangesAsync();
            
            return ServiceResult.Ok("Foto eliminada exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Error al eliminar la foto: {ex.Message}");
        }
    }
    
    public async Task<int> CountActiveAsync()
    {
        return await _empleadoRepository.CountActiveAsync();
    }
    
    public async Task<IEnumerable<Empleado>> GetCumpleaniosProximosAsync(int diasAnticipacion = 7)
    {
        var empleados = await _empleadoRepository.GetAllActiveWithRelationsAsync();
        var hoy = DateTime.Today;
        
        return empleados
            .Where(e => e.FechaNacimiento.HasValue)
            .Select(e => new {
                Empleado = e,
                ProximoCumple = _dateCalculationService.GetProximoCumpleanos(e.FechaNacimiento!.Value)
            })
            .Where(x => (x.ProximoCumple - hoy).Days >= 0 && (x.ProximoCumple - hoy).Days <= diasAnticipacion)
            .OrderBy(x => x.ProximoCumple)
            .Select(x => x.Empleado)
            .ToList();
    }
    
    public async Task<IEnumerable<Empleado>> GetAniversariosProximosAsync(int diasAnticipacion = 7)
    {
        var empleados = await _empleadoRepository.GetAllActiveWithRelationsAsync();
        var hoy = DateTime.Today;
        
        return empleados
            .Select(e => new {
                Empleado = e,
                ProximoAniversario = _dateCalculationService.GetProximoAniversario(e.FechaIngreso),
                AnosServicio = _dateCalculationService.CalcularAnosServicio(e.FechaIngreso)
            })
            .Where(x => (x.ProximoAniversario - hoy).Days >= 0 && (x.ProximoAniversario - hoy).Days <= diasAnticipacion)
            .OrderBy(x => x.ProximoAniversario)
            .Select(x => x.Empleado)
            .ToList();
    }
    
    public async Task<IEnumerable<EstadisticaItemDTO>> GetEmpleadosPorDepartamentoAsync()
    {
        var empleados = await _empleadoRepository.GetAllActiveWithRelationsAsync();
        var total = empleados.Count();
        
        if (total == 0)
            return Enumerable.Empty<EstadisticaItemDTO>();
        
        // Colores predefinidos para los departamentos
        var colores = new[] { "#1E88E5", "#43A047", "#FB8C00", "#8E24AA", "#E53935", "#00ACC1", "#5E35B1", "#F4511E" };
        var colorIndex = 0;
        
        var estadisticas = empleados
            .GroupBy(e => e.Departamento?.Nombre ?? "Sin departamento")
            .Select(g => new EstadisticaItemDTO
            {
                Etiqueta = g.Key,
                Cantidad = g.Count(),
                Porcentaje = Math.Round((double)g.Count() / total * 100, 1),
                Color = colores[colorIndex++ % colores.Length]
            })
            .OrderByDescending(e => e.Cantidad)
            .ToList();
        
        return estadisticas;
    }
    
    public async Task<IEnumerable<Empleado>> GetPendientesAprobacionAsync()
    {
        var empleados = await _empleadoRepository.GetAllWithRelationsAsync();
        return empleados.Where(e => e.Estado == EstadoEmpleado.PendienteAprobacion).ToList();
    }
    
    public async Task<ServiceResult> AprobarAsync(int id, int aprobadorId)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(id);
        if (empleado == null)
            return ServiceResult.Fail("Empleado no encontrado");
            
        if (empleado.Estado != EstadoEmpleado.PendienteAprobacion)
            return ServiceResult.Fail("El empleado no está pendiente de aprobación");
            
        empleado.Estado = EstadoEmpleado.Activo;
        empleado.AprobadoPorId = aprobadorId;
        empleado.FechaAprobacion = DateTime.Now;
        empleado.FechaModificacion = DateTime.Now;
        
        await _empleadoRepository.UpdateAsync(empleado);
        await _empleadoRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Empleado aprobado exitosamente");
    }
    
    public async Task<ServiceResult> RechazarAsync(int id, int aprobadorId, string motivo)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(id);
        if (empleado == null)
            return ServiceResult.Fail("Empleado no encontrado");
            
        if (empleado.Estado != EstadoEmpleado.PendienteAprobacion)
            return ServiceResult.Fail("El empleado no está pendiente de aprobación");
            
        empleado.Estado = EstadoEmpleado.Rechazado;
        empleado.AprobadoPorId = aprobadorId;
        empleado.FechaAprobacion = DateTime.Now;
        empleado.MotivoRechazo = motivo;
        empleado.Activo = false;
        empleado.FechaModificacion = DateTime.Now;
        
        await _empleadoRepository.UpdateAsync(empleado);
        await _empleadoRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Empleado rechazado");
    }
    
    public async Task<int> CountPendientesAsync()
    {
        var empleados = await _empleadoRepository.GetAllAsync();
        return empleados.Count(e => e.Estado == EstadoEmpleado.PendienteAprobacion);
    }
}
