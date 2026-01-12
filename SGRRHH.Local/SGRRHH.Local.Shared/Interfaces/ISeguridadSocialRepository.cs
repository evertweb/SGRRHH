using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para Entidades Promotoras de Salud (EPS)
/// </summary>
public interface IEpsRepository : IRepository<Eps>
{
    /// <summary>Obtiene todas las EPS vigentes para nuevas afiliaciones</summary>
    Task<List<Eps>> GetVigentesAsync();
    
    /// <summary>Busca una EPS por su código oficial</summary>
    Task<Eps?> GetByCodigoAsync(string codigo);
}

/// <summary>
/// Repositorio para Administradoras de Fondos de Pensiones (AFP)
/// </summary>
public interface IAfpRepository : IRepository<Afp>
{
    /// <summary>Obtiene todas las AFP vigentes</summary>
    Task<List<Afp>> GetVigentesAsync();
    
    /// <summary>Busca una AFP por su código oficial</summary>
    Task<Afp?> GetByCodigoAsync(string codigo);
}

/// <summary>
/// Repositorio para Administradoras de Riesgos Laborales (ARL)
/// </summary>
public interface IArlRepository : IRepository<Arl>
{
    /// <summary>Obtiene todas las ARL vigentes</summary>
    Task<List<Arl>> GetVigentesAsync();
    
    /// <summary>Busca una ARL por su código oficial</summary>
    Task<Arl?> GetByCodigoAsync(string codigo);
}

/// <summary>
/// Repositorio para Cajas de Compensación Familiar
/// </summary>
public interface ICajaCompensacionRepository : IRepository<CajaCompensacion>
{
    /// <summary>Obtiene todas las Cajas de Compensación vigentes</summary>
    Task<List<CajaCompensacion>> GetVigentesAsync();
    
    /// <summary>Obtiene las Cajas de Compensación que operan en un departamento</summary>
    Task<List<CajaCompensacion>> GetByDepartamentoAsync(string departamento);
    
    /// <summary>Busca una Caja de Compensación por su código</summary>
    Task<CajaCompensacion?> GetByCodigoAsync(string codigo);
}
