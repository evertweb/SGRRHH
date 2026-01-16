using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para gestionar las cuentas bancarias de empleados
/// </summary>
public interface ICuentaBancariaRepository : IRepository<CuentaBancaria>
{
    /// <summary>
    /// Obtiene todas las cuentas bancarias de un empleado
    /// </summary>
    Task<IEnumerable<CuentaBancaria>> GetByEmpleadoIdAsync(int empleadoId);
    
    /// <summary>
    /// Obtiene la cuenta de nómina activa de un empleado (si existe)
    /// </summary>
    Task<CuentaBancaria?> GetCuentaNominaActivaAsync(int empleadoId);
    
    /// <summary>
    /// Marca una cuenta como cuenta de nómina principal
    /// (desmarca las demás del mismo empleado)
    /// </summary>
    Task<bool> SetCuentaNominaPrincipalAsync(int cuentaBancariaId);
    
    /// <summary>
    /// Obtiene cuenta bancaria con su documento de certificación (si existe)
    /// </summary>
    Task<CuentaBancaria?> GetByIdWithDocumentoAsync(int id);
}
