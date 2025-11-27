using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio para la gestión de tipos de permiso
/// </summary>
public interface ITipoPermisoService
{
    Task<ServiceResult<IEnumerable<TipoPermiso>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<TipoPermiso>>> GetActivosAsync();
    Task<ServiceResult<TipoPermiso>> GetByIdAsync(int id);
    Task<ServiceResult<TipoPermiso>> CreateAsync(TipoPermiso tipoPermiso);
    Task<ServiceResult<TipoPermiso>> UpdateAsync(TipoPermiso tipoPermiso);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
