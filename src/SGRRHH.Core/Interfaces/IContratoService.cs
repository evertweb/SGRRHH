using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

public interface IContratoService
{
    Task<ServiceResult<IEnumerable<Contrato>>> GetByEmpleadoIdAsync(int empleadoId);
    Task<ServiceResult<Contrato>> GetByIdAsync(int id);
    Task<ServiceResult<Contrato>> CreateAsync(Contrato contrato);
    Task<ServiceResult<Contrato>> UpdateAsync(Contrato contrato);
    Task<ServiceResult<bool>> DeleteAsync(int id);
    
    /// <summary>
    /// Obtiene el contrato activo de un empleado
    /// </summary>
    Task<ServiceResult<Contrato?>> GetContratoActivoAsync(int empleadoId);
    
    /// <summary>
    /// Renueva un contrato: finaliza el actual y crea uno nuevo
    /// </summary>
    Task<ServiceResult<Contrato>> RenovarContratoAsync(int contratoActualId, Contrato nuevoContrato);
    
    /// <summary>
    /// Finaliza un contrato
    /// </summary>
    Task<ServiceResult<bool>> FinalizarContratoAsync(int contratoId, DateTime fechaFin);
    
    /// <summary>
    /// Obtiene contratos próximos a vencer
    /// </summary>
    Task<ServiceResult<IEnumerable<Contrato>>> GetContratosProximosAVencerAsync(int diasAnticipacion = 30);
}
