using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IConfiguracionRepository : IRepository<ConfiguracionSistema>
{
    Task<ConfiguracionSistema?> GetByClaveAsync(string clave);
    
    Task<List<ConfiguracionSistema>> GetByCategoriaAsync(string categoria);
    
    Task<bool> ExistsClaveAsync(string clave);
}


