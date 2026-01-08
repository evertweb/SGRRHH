using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface ITipoPermisoRepository : IRepository<TipoPermiso>
{
    Task<IEnumerable<TipoPermiso>> GetActivosAsync();
    
    Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null);
}


