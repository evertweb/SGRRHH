using Microsoft.Extensions.Logging;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Firebase.Repositories;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de contratos
/// </summary>
public class ContratoService : IContratoService
{
    private readonly IContratoRepository _contratoRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly ICargoRepository _cargoRepository;
    private readonly IUnitOfWork? _unitOfWork;
    private readonly ILogger<ContratoService>? _logger;
    
    /// <summary>
    /// Salario mínimo legal vigente en Colombia (2025)
    /// </summary>
    private const decimal SALARIO_MINIMO_COLOMBIA = 1_423_500m;
    
    /// <summary>
    /// Duración máxima en meses para contratos de aprendizaje (ley colombiana)
    /// </summary>
    private const int MAX_MESES_APRENDIZAJE = 24;
    
    public ContratoService(
        IContratoRepository contratoRepository, 
        IEmpleadoRepository empleadoRepository,
        ICargoRepository cargoRepository,
        IUnitOfWork? unitOfWork = null,
        ILogger<ContratoService>? logger = null)
    {
        _contratoRepository = contratoRepository;
        _empleadoRepository = empleadoRepository;
        _cargoRepository = cargoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<ServiceResult<IEnumerable<Contrato>>> GetByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var contratos = await _contratoRepository.GetByEmpleadoIdAsync(empleadoId);
            return ServiceResult<IEnumerable<Contrato>>.Ok(contratos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Contrato>>.Fail($"Error al obtener contratos: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Contrato>> GetByIdAsync(int id)
    {
        try
        {
            var contrato = await _contratoRepository.GetByIdAsync(id);
            if (contrato == null)
                return ServiceResult<Contrato>.Fail("Contrato no encontrado");
                
            return ServiceResult<Contrato>.Ok(contrato);
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.Fail($"Error al obtener contrato: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Contrato>> CreateAsync(Contrato contrato)
    {
        try
        {
            var errors = await ValidarContratoAsync(contrato);
            if (errors.Any())
                return ServiceResult<Contrato>.Fail(errors);
            
            // Verificar que el empleado exista
            var empleado = await _empleadoRepository.GetByIdAsync(contrato.EmpleadoId);
            if (empleado == null)
                return ServiceResult<Contrato>.Fail("Empleado no encontrado");
            
            // Verificar si ya tiene un contrato activo
            var contratoActivo = await _contratoRepository.GetContratoActivoByEmpleadoIdAsync(contrato.EmpleadoId);
            if (contratoActivo != null)
            {
                return ServiceResult<Contrato>.Fail(
                    "El empleado ya tiene un contrato activo. Debe finalizarlo o renovarlo primero.");
            }
            
            contrato.Estado = EstadoContrato.Activo;
            contrato.Activo = true;
            contrato.FechaCreacion = DateTime.Now;
            
            await _contratoRepository.AddAsync(contrato);
            await _contratoRepository.SaveChangesAsync();
            
            return ServiceResult<Contrato>.Ok(contrato, "Contrato creado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.Fail($"Error al crear contrato: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Contrato>> UpdateAsync(Contrato contrato)
    {
        try
        {
            var existing = await _contratoRepository.GetByIdAsync(contrato.Id);
            if (existing == null)
                return ServiceResult<Contrato>.Fail("Contrato no encontrado");
            
            var errors = await ValidarContratoAsync(contrato);
            if (errors.Any())
                return ServiceResult<Contrato>.Fail(errors);
            
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
            
            return ServiceResult<Contrato>.Ok(existing, "Contrato actualizado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.Fail($"Error al actualizar contrato: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var contrato = await _contratoRepository.GetByIdAsync(id);
            if (contrato == null)
                return ServiceResult<bool>.Fail("Contrato no encontrado");
            
            // No permitir eliminar contratos finalizados
            if (contrato.Estado == EstadoContrato.Finalizado)
            {
                return ServiceResult<bool>.Fail("No se puede eliminar un contrato finalizado");
            }
            
            await _contratoRepository.DeleteAsync(id);
            await _contratoRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.Ok(true, "Contrato eliminado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error al eliminar contrato: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Contrato?>> GetContratoActivoAsync(int empleadoId)
    {
        try
        {
            var contrato = await _contratoRepository.GetContratoActivoByEmpleadoIdAsync(empleadoId);
            return ServiceResult<Contrato?>.Ok(contrato);
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato?>.Fail($"Error al obtener contrato activo: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Renueva un contrato: finaliza el actual y crea uno nuevo
    /// Usa transacción para garantizar consistencia
    /// </summary>
    public async Task<ServiceResult<Contrato>> RenovarContratoAsync(int contratoActualId, Contrato nuevoContrato)
    {
        try
        {
            _logger?.LogInformation("Iniciando renovación de contrato {ContratoId}", contratoActualId);
            
            var contratoActual = await _contratoRepository.GetByIdAsync(contratoActualId);
            if (contratoActual == null)
                return ServiceResult<Contrato>.Fail("Contrato actual no encontrado");
            
            if (contratoActual.Estado != EstadoContrato.Activo)
                return ServiceResult<Contrato>.Fail("Solo se pueden renovar contratos activos");
            
            var errors = await ValidarContratoAsync(nuevoContrato);
            if (errors.Any())
                return ServiceResult<Contrato>.Fail(errors);
            
            // La fecha de inicio del nuevo contrato debe ser posterior a la fecha fin del actual
            if (contratoActual.FechaFin.HasValue && nuevoContrato.FechaInicio < contratoActual.FechaFin.Value)
            {
                return ServiceResult<Contrato>.Fail(
                    "La fecha de inicio del nuevo contrato debe ser posterior a la fecha de fin del contrato actual");
            }
            
            // Usar transacción si está disponible
            if (_unitOfWork != null)
            {
                await _unitOfWork.BeginTransactionAsync();
            }
            
            try
            {
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
                
                // Confirmar transacción
                if (_unitOfWork != null)
                {
                    await _unitOfWork.CommitAsync();
                }
                
                _logger?.LogInformation("Contrato {ContratoId} renovado exitosamente", contratoActualId);
                return ServiceResult<Contrato>.Ok(nuevoContrato, "Contrato renovado exitosamente");
            }
            catch
            {
                // Revertir transacción en caso de error
                if (_unitOfWork != null)
                {
                    await _unitOfWork.RollbackAsync();
                }
                throw; // Re-lanzar para manejo externo
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al renovar contrato {ContratoId}", contratoActualId);
            return ServiceResult<Contrato>.Fail($"Error al renovar contrato: {ex.Message}");
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
                return ServiceResult<bool>.Fail("Contrato no encontrado");
            
            if (contrato.Estado != EstadoContrato.Activo)
                return ServiceResult<bool>.Fail("Solo se pueden finalizar contratos activos");
            
            if (fechaFin < contrato.FechaInicio)
                return ServiceResult<bool>.Fail("La fecha de finalización no puede ser anterior a la fecha de inicio");
            
            contrato.Estado = EstadoContrato.Finalizado;
            contrato.FechaFin = fechaFin;
            contrato.FechaModificacion = DateTime.Now;
            
            await _contratoRepository.UpdateAsync(contrato);
            await _contratoRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.Ok(true, "Contrato finalizado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error al finalizar contrato: {ex.Message}");
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
            return ServiceResult<IEnumerable<Contrato>>.Ok(contratos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Contrato>>.Fail($"Error al obtener contratos: {ex.Message}");
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
            return ServiceResult<IEnumerable<Contrato>>.Ok(contratos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Contrato>>.Fail($"Error: {ex.Message}");
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
    
    private async Task<List<string>> ValidarContratoAsync(Contrato contrato)
    {
        var errors = new List<string>();
        
        if (contrato.EmpleadoId <= 0)
            errors.Add("Debe seleccionar un empleado");
            
        if (contrato.CargoId <= 0)
            errors.Add("Debe seleccionar un cargo");
        else
        {
            // FIX #3: Validar que el cargo existe
            var cargo = await _cargoRepository.GetByIdAsync(contrato.CargoId);
            if (cargo == null)
                errors.Add("El cargo seleccionado no existe");
        }
            
        if (contrato.FechaInicio == default)
            errors.Add("La fecha de inicio es obligatoria");
            
        if (contrato.Salario <= 0)
            errors.Add("El salario debe ser mayor a cero");
        
        // FIX #7: Validar salario mínimo (excepto Aprendizaje que tiene reglas especiales)
        if (contrato.TipoContrato != TipoContrato.Aprendizaje && 
            contrato.TipoContrato != TipoContrato.PrestacionServicios &&
            contrato.Salario > 0 && contrato.Salario < SALARIO_MINIMO_COLOMBIA)
        {
            errors.Add($"El salario no puede ser menor al salario mínimo legal vigente (${SALARIO_MINIMO_COLOMBIA:N0})");
        }
        
        // FIX #5 & #6: Validar FechaFin según tipo de contrato
        switch (contrato.TipoContrato)
        {
            case TipoContrato.Indefinido:
                // Indefinido NO debe tener fecha fin
                if (contrato.FechaFin.HasValue)
                {
                    _logger?.LogWarning("Contrato indefinido con FechaFin, se ignorará");
                }
                break;
                
            case TipoContrato.Fijo:
            case TipoContrato.ObraLabor:
            case TipoContrato.Aprendizaje:
                // Estos tipos DEBEN tener fecha fin
                if (!contrato.FechaFin.HasValue)
                {
                    errors.Add($"Para contratos de tipo {contrato.TipoContrato}, la fecha de fin es obligatoria");
                }
                break;
                
            case TipoContrato.PrestacionServicios:
                // Prestación de servicios puede o no tener fecha fin
                break;
        }
        
        // FIX #5: Validar FechaFin > FechaInicio (corregido de <= a <)
        if (contrato.FechaFin.HasValue && contrato.FechaFin.Value < contrato.FechaInicio)
        {
            errors.Add("La fecha de fin debe ser igual o posterior a la fecha de inicio");
        }
        
        // FIX #4: Validar duración máxima para contratos de Aprendizaje (24 meses)
        if (contrato.TipoContrato == TipoContrato.Aprendizaje && contrato.FechaFin.HasValue)
        {
            var duracionMeses = ((contrato.FechaFin.Value.Year - contrato.FechaInicio.Year) * 12) + 
                               (contrato.FechaFin.Value.Month - contrato.FechaInicio.Month);
            if (duracionMeses > MAX_MESES_APRENDIZAJE)
            {
                errors.Add($"Los contratos de aprendizaje no pueden exceder {MAX_MESES_APRENDIZAJE} meses (ley colombiana)");
            }
        }
        
        return errors;
    }
    
    /// <summary>
    /// Método sincrónico legacy para compatibilidad
    /// </summary>
    private List<string> ValidarContrato(Contrato contrato)
    {
        return ValidarContratoAsync(contrato).GetAwaiter().GetResult();
    }
    
    #endregion
}
