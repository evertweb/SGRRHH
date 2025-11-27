using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Servicio de negocio para la gestión de tipos de permiso
/// </summary>
public class TipoPermisoService : ITipoPermisoService
{
    private readonly ITipoPermisoRepository _repository;

    public TipoPermisoService(ITipoPermisoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<IEnumerable<TipoPermiso>>> GetAllAsync()
    {
        try
        {
            var tiposPermiso = await _repository.GetAllAsync();
            return ServiceResult<IEnumerable<TipoPermiso>>.SuccessResult(tiposPermiso);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<TipoPermiso>>.FailureResult($"Error al obtener tipos de permiso: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<TipoPermiso>>> GetActivosAsync()
    {
        try
        {
            var tiposPermiso = await _repository.GetActivosAsync();
            return ServiceResult<IEnumerable<TipoPermiso>>.SuccessResult(tiposPermiso);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<TipoPermiso>>.FailureResult($"Error al obtener tipos de permiso activos: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TipoPermiso>> GetByIdAsync(int id)
    {
        try
        {
            var tipoPermiso = await _repository.GetByIdAsync(id);
            if (tipoPermiso == null)
            {
                return ServiceResult<TipoPermiso>.FailureResult("Tipo de permiso no encontrado");
            }

            return ServiceResult<TipoPermiso>.SuccessResult(tipoPermiso);
        }
        catch (Exception ex)
        {
            return ServiceResult<TipoPermiso>.FailureResult($"Error al obtener tipo de permiso: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TipoPermiso>> CreateAsync(TipoPermiso tipoPermiso)
    {
        try
        {
            // Validaciones
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(tipoPermiso.Nombre))
            {
                errores.Add("El nombre es requerido");
            }

            if (await _repository.ExisteNombreAsync(tipoPermiso.Nombre))
            {
                errores.Add("Ya existe un tipo de permiso con ese nombre");
            }

            if (tipoPermiso.DiasPorDefecto < 0)
            {
                errores.Add("Los días por defecto no pueden ser negativos");
            }

            if (errores.Any())
            {
                return ServiceResult<TipoPermiso>.FailureResult(errores);
            }

            // Crear
            var created = await _repository.AddAsync(tipoPermiso);
            return ServiceResult<TipoPermiso>.SuccessResult(created, "Tipo de permiso creado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<TipoPermiso>.FailureResult($"Error al crear tipo de permiso: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TipoPermiso>> UpdateAsync(TipoPermiso tipoPermiso)
    {
        try
        {
            // Validaciones
            var errores = new List<string>();

            var existing = await _repository.GetByIdAsync(tipoPermiso.Id);
            if (existing == null)
            {
                return ServiceResult<TipoPermiso>.FailureResult("Tipo de permiso no encontrado");
            }

            if (string.IsNullOrWhiteSpace(tipoPermiso.Nombre))
            {
                errores.Add("El nombre es requerido");
            }

            if (await _repository.ExisteNombreAsync(tipoPermiso.Nombre, tipoPermiso.Id))
            {
                errores.Add("Ya existe un tipo de permiso con ese nombre");
            }

            if (tipoPermiso.DiasPorDefecto < 0)
            {
                errores.Add("Los días por defecto no pueden ser negativos");
            }

            if (errores.Any())
            {
                return ServiceResult<TipoPermiso>.FailureResult(errores);
            }

            // Actualizar
            await _repository.UpdateAsync(tipoPermiso);
            return ServiceResult<TipoPermiso>.SuccessResult(tipoPermiso, "Tipo de permiso actualizado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<TipoPermiso>.FailureResult($"Error al actualizar tipo de permiso: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var tipoPermiso = await _repository.GetByIdAsync(id);
            if (tipoPermiso == null)
            {
                return ServiceResult<bool>.FailureResult("Tipo de permiso no encontrado");
            }

            // Soft delete - marcar como inactivo en lugar de eliminar
            tipoPermiso.Activo = false;
            await _repository.UpdateAsync(tipoPermiso);

            return ServiceResult<bool>.SuccessResult(true, "Tipo de permiso desactivado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error al eliminar tipo de permiso: {ex.Message}");
        }
    }
}
