using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para gestión de configuración legal de Colombia (SMLMV, porcentajes, etc.)
/// </summary>
public interface IConfiguracionLegalRepository : IRepository<ConfiguracionLegal>
{
    /// <summary>
    /// Obtiene la configuración legal vigente
    /// </summary>
    Task<ConfiguracionLegal?> GetVigenteAsync();
    
    /// <summary>
    /// Obtiene la configuración legal de un año específico
    /// </summary>
    Task<ConfiguracionLegal?> GetByAñoAsync(int año);
    
    /// <summary>
    /// Establece una configuración como vigente (desactiva las demás)
    /// </summary>
    Task SetVigenteAsync(int id);
    
    /// <summary>
    /// Verifica si existe configuración para un año
    /// </summary>
    Task<bool> ExisteAñoAsync(int año);
    
    /// <summary>
    /// Obtiene el salario mínimo vigente
    /// </summary>
    Task<decimal> GetSalarioMinimoVigenteAsync();
    
    /// <summary>
    /// Obtiene el auxilio de transporte vigente
    /// </summary>
    Task<decimal> GetAuxilioTransporteVigenteAsync();
}
