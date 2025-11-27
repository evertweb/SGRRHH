using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de contratos
/// </summary>
public class ContratoService : IContratoService
{
    private readonly IContratoRepository _contratoRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    
    public ContratoService(IContratoRepository contratoRepository, IEmpleadoRepository empleadoRepository)
    {
        _contratoRepository = contratoRepository;
        _empleadoRepository = empleadoRepository;
    }
    
    public async Task<ServiceResult<IEnumerable<Contrato>>> GetByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var contratos = await _contratoRepository.GetByEmpleadoIdAsync(empleadoId);
            return ServiceResult<IEnumerable<Contrato>>.SuccessResult(contratos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Contrato>>.FailureResult($"Error al obtener contratos: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Contrato>> GetByIdAsync(int id)
    {
        try
        {
            var contrato = await _contratoRepository.GetByIdAsync(id);
            if (contrato == null)
                return ServiceResult<Contrato>.FailureResult("Contrato no encontrado");
                
            return ServiceResult<Contrato>.SuccessResult(contrato);
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.FailureResult($"Error al obtener contrato: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Contrato>> CreateAsync(Contrato contrato)
    {
        try
        {
            var errors = ValidarContrato(contrato);
            if (errors.Any())
                return ServiceResult<Contrato>.FailureResult(errors);
            
            // Verificar que el empleado exista
            var empleado = await _empleadoRepository.GetByIdAsync(contrato.EmpleadoId);
            if (empleado == null)
                return ServiceResult<Contrato>.FailureResult("Empleado no encontrado");
            
            // Verificar si ya tiene un contrato activo
            var contratoActivo = await _contratoRepository.GetContratoActivoByEmpleadoIdAsync(contrato.EmpleadoId);
            if (contratoActivo != null)
            {
                return ServiceResult<Contrato>.FailureResult(
                    "El empleado ya tiene un contrato activo. Debe finalizarlo o renovarlo primero.");
            }
            
            contrato.Estado = EstadoContrato.Activo;
            contrato.Activo = true;
            contrato.FechaCreacion = DateTime.Now;
            
            await _contratoRepository.AddAsync(contrato);
            await _contratoRepository.SaveChangesAsync();
            
            return ServiceResult<Contrato>.SuccessResult(contrato, "Contrato creado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.FailureResult($"Error al crear contrato: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Contrato>> UpdateAsync(Contrato contrato)
    {
        try
        {
            var existing = await _contratoRepository.GetByIdAsync(contrato.Id);
            if (existing == null)
                return ServiceResult<Contrato>.FailureResult("Contrato no encontrado");
            
            var errors = ValidarContrato(contrato);
            if (errors.Any())
                return ServiceResult<Contrato>.FailureResult(errors);
            
            // Actualizar campos
            existing.TipoContrato = contrato.TipoContrato;
            existing.FechaInicio = contrato.FechaInicio;
            existing.FechaFin = contrato.FechaFin;
            existing.Salario = contrato.Salario;
            existing.CargoId = contrato.CargoId;
            existing.ArchivoAdjuntoPath = contrato.ArchivoAdjuntoPath;
            existing.Observaciones = contrato.Observaciones;
            existing.FechaModificacion = DateTime.Now;
            
            await _contratoRepository.UpdateAsync(existing);
            await _contratoRepository.SaveChangesAsync();
            
            return ServiceResult<Contrato>.SuccessResult(existing, "Contrato actualizado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.FailureResult($"Error al actualizar contrato: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var contrato = await _contratoRepository.GetByIdAsync(id);
            if (contrato == null)
                return ServiceResult<bool>.FailureResult("Contrato no encontrado");
            
            // No permitir eliminar contratos finalizados
            if (contrato.Estado == EstadoContrato.Finalizado)
            {
                return ServiceResult<bool>.FailureResult("No se puede eliminar un contrato finalizado");
            }
            
            await _contratoRepository.DeleteAsync(id);
            await _contratoRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.SuccessResult(true, "Contrato eliminado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error al eliminar contrato: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Contrato?>> GetContratoActivoAsync(int empleadoId)
    {
        try
        {
            var contrato = await _contratoRepository.GetContratoActivoByEmpleadoIdAsync(empleadoId);
            return ServiceResult<Contrato?>.SuccessResult(contrato);
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato?>.FailureResult($"Error al obtener contrato activo: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Renueva un contrato: finaliza el actual y crea uno nuevo
    /// </summary>
    public async Task<ServiceResult<Contrato>> RenovarContratoAsync(int contratoActualId, Contrato nuevoContrato)
    {
        try
        {
            var contratoActual = await _contratoRepository.GetByIdAsync(contratoActualId);
            if (contratoActual == null)
                return ServiceResult<Contrato>.FailureResult("Contrato actual no encontrado");
            
            if (contratoActual.Estado != EstadoContrato.Activo)
                return ServiceResult<Contrato>.FailureResult("Solo se pueden renovar contratos activos");
            
            var errors = ValidarContrato(nuevoContrato);
            if (errors.Any())
                return ServiceResult<Contrato>.FailureResult(errors);
            
            // La fecha de inicio del nuevo contrato debe ser posterior a la fecha fin del actual
            if (contratoActual.FechaFin.HasValue && nuevoContrato.FechaInicio < contratoActual.FechaFin.Value)
            {
                return ServiceResult<Contrato>.FailureResult(
                    "La fecha de inicio del nuevo contrato debe ser posterior a la fecha de fin del contrato actual");
            }
            
            // Finalizar el contrato actual
            contratoActual.Estado = EstadoContrato.Renovado;
            contratoActual.FechaFin = nuevoContrato.FechaInicio.AddDays(-1);
            contratoActual.Observaciones = $"Renovado el {DateTime.Now:dd/MM/yyyy}. {contratoActual.Observaciones}";
            contratoActual.FechaModificacion = DateTime.Now;
            
            await _contratoRepository.UpdateAsync(contratoActual);
            
            // Crear el nuevo contrato
            nuevoContrato.EmpleadoId = contratoActual.EmpleadoId;
            nuevoContrato.Estado = EstadoContrato.Activo;
            nuevoContrato.Activo = true;
            nuevoContrato.FechaCreacion = DateTime.Now;
            nuevoContrato.Observaciones = $"Renovación de contrato anterior (ID: {contratoActualId}). {nuevoContrato.Observaciones}";
            
            await _contratoRepository.AddAsync(nuevoContrato);
            await _contratoRepository.SaveChangesAsync();
            
            return ServiceResult<Contrato>.SuccessResult(nuevoContrato, "Contrato renovado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.FailureResult($"Error al renovar contrato: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Finaliza un contrato
    /// </summary>
    public async Task<ServiceResult<bool>> FinalizarContratoAsync(int contratoId, DateTime fechaFin)
    {
        try
        {
            var contrato = await _contratoRepository.GetByIdAsync(contratoId);
            if (contrato == null)
                return ServiceResult<bool>.FailureResult("Contrato no encontrado");
            
            if (contrato.Estado != EstadoContrato.Activo)
                return ServiceResult<bool>.FailureResult("Solo se pueden finalizar contratos activos");
            
            if (fechaFin < contrato.FechaInicio)
                return ServiceResult<bool>.FailureResult("La fecha de finalización no puede ser anterior a la fecha de inicio");
            
            contrato.Estado = EstadoContrato.Finalizado;
            contrato.FechaFin = fechaFin;
            contrato.FechaModificacion = DateTime.Now;
            
            await _contratoRepository.UpdateAsync(contrato);
            await _contratoRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.SuccessResult(true, "Contrato finalizado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error al finalizar contrato: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene contratos próximos a vencer
    /// </summary>
    public async Task<ServiceResult<IEnumerable<Contrato>>> GetContratosProximosAVencerAsync(int diasAnticipacion = 30)
    {
        try
        {
            var contratos = await _contratoRepository.GetContratosProximosAVencerAsync(diasAnticipacion);
            return ServiceResult<IEnumerable<Contrato>>.SuccessResult(contratos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Contrato>>.FailureResult($"Error al obtener contratos: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene todos los contratos con relaciones
    /// </summary>
    public async Task<ServiceResult<IEnumerable<Contrato>>> GetAllAsync()
    {
        try
        {
            var contratos = await _contratoRepository.GetAllActiveAsync();
            return ServiceResult<IEnumerable<Contrato>>.SuccessResult(contratos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Contrato>>.FailureResult($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Cuenta contratos próximos a vencer
    /// </summary>
    public async Task<int> CountContratosProximosAVencerAsync(int diasAnticipacion = 30)
    {
        try
        {
            var contratos = await _contratoRepository.GetContratosProximosAVencerAsync(diasAnticipacion);
            return contratos.Count();
        }
        catch
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Calcula días restantes del contrato
    /// </summary>
    public int CalcularDiasRestantes(Contrato contrato)
    {
        if (!contrato.FechaFin.HasValue)
            return -1; // Contrato indefinido
            
        var diasRestantes = (contrato.FechaFin.Value - DateTime.Today).Days;
        return Math.Max(0, diasRestantes);
    }
    
    #region Métodos privados
    
    private List<string> ValidarContrato(Contrato contrato)
    {
        var errors = new List<string>();
        
        if (contrato.EmpleadoId <= 0)
            errors.Add("Debe seleccionar un empleado");
            
        if (contrato.CargoId <= 0)
            errors.Add("Debe seleccionar un cargo");
            
        if (contrato.FechaInicio == default)
            errors.Add("La fecha de inicio es obligatoria");
            
        if (contrato.Salario <= 0)
            errors.Add("El salario debe ser mayor a cero");
            
        // Para contratos a término fijo, la fecha fin es obligatoria
        if (contrato.TipoContrato == TipoContrato.Fijo && !contrato.FechaFin.HasValue)
        {
            errors.Add("Para contratos a término fijo, la fecha de fin es obligatoria");
        }
        
        if (contrato.FechaFin.HasValue && contrato.FechaFin.Value < contrato.FechaInicio)
        {
            errors.Add("La fecha de fin debe ser posterior a la fecha de inicio");
        }
        
        return errors;
    }
    
    #endregion
}
