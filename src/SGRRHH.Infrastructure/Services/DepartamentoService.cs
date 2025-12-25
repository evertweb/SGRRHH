using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Firebase.Repositories;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de departamentos
/// </summary>
public class DepartamentoService : IDepartamentoService
{
    private readonly IDepartamentoRepository _departamentoRepository;

    public DepartamentoService(IDepartamentoRepository departamentoRepository)
    {
        _departamentoRepository = departamentoRepository;
    }
    
    public async Task<IEnumerable<Departamento>> GetAllAsync()
    {
        return await _departamentoRepository.GetAllActiveAsync();
    }
    
    public async Task<Departamento?> GetByIdAsync(int id)
    {
        return await _departamentoRepository.GetByIdWithEmpleadosAsync(id);
    }
    
    public async Task<ServiceResult<Departamento>> CreateAsync(Departamento departamento)
    {
        var errors = new List<string>();

        // Validaciones
        if (string.IsNullOrWhiteSpace(departamento.Nombre))
            errors.Add("El nombre es obligatorio");
        else if (await _departamentoRepository.ExistsByNameAsync(departamento.Nombre))
            errors.Add($"Ya existe un departamento con el nombre '{departamento.Nombre}'");

        // Generar código si está vacío
        if (string.IsNullOrWhiteSpace(departamento.Codigo))
        {
            departamento.Codigo = await _departamentoRepository.GetNextCodigoAsync();
        }
        else
        {
            // Validar unicidad de código
            if (await _departamentoRepository.ExistsCodigoAsync(departamento.Codigo))
                errors.Add($"Ya existe un departamento con el código {departamento.Codigo}");
        }

        if (errors.Any())
            return ServiceResult<Departamento>.Fail(errors);

        departamento.Activo = true;
        departamento.FechaCreacion = DateTime.Now;

        await _departamentoRepository.AddAsync(departamento);
        await _departamentoRepository.SaveChangesAsync();

        // Invalidar cache después de crear
        _departamentoRepository.InvalidateCache();

        return ServiceResult<Departamento>.Ok(departamento, "Departamento creado exitosamente");
    }
    
    public async Task<ServiceResult> UpdateAsync(Departamento departamento)
    {
        var existing = await _departamentoRepository.GetByIdAsync(departamento.Id);
        if (existing == null)
            return ServiceResult.Fail("Departamento no encontrado");

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(departamento.Nombre))
            errors.Add("El nombre es obligatorio");
        else if (await _departamentoRepository.ExistsByNameAsync(departamento.Nombre, departamento.Id))
            errors.Add($"Ya existe otro departamento con el nombre '{departamento.Nombre}'");

        if (await _departamentoRepository.ExistsCodigoAsync(departamento.Codigo, departamento.Id))
            errors.Add($"Ya existe otro departamento con el código {departamento.Codigo}");

        if (errors.Any())
            return ServiceResult.Fail(errors);

        existing.Codigo = departamento.Codigo;
        existing.Nombre = departamento.Nombre;
        existing.Descripcion = departamento.Descripcion;
        existing.JefeId = departamento.JefeId;
        existing.Activo = departamento.Activo;  // FIX 1.2: Agregar el campo Activo que faltaba
        existing.FechaModificacion = DateTime.Now;

        await _departamentoRepository.UpdateAsync(existing);
        await _departamentoRepository.SaveChangesAsync();

        // Invalidar cache después de actualizar
        _departamentoRepository.InvalidateCache();

        return ServiceResult.Ok("Departamento actualizado exitosamente");
    }
    
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var departamento = await _departamentoRepository.GetByIdAsync(id);
        if (departamento == null)
            return ServiceResult.Fail("Departamento no encontrado");

        if (await _departamentoRepository.HasEmpleadosAsync(id))
            return ServiceResult.Fail("No se puede eliminar el departamento porque tiene empleados asignados");

        departamento.Activo = false;
        departamento.FechaModificacion = DateTime.Now;

        await _departamentoRepository.UpdateAsync(departamento);
        await _departamentoRepository.SaveChangesAsync();

        // Invalidar cache después de eliminar
        _departamentoRepository.InvalidateCache();

        return ServiceResult.Ok("Departamento eliminado exitosamente");
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        return await _departamentoRepository.GetNextCodigoAsync();
    }
    
    public async Task<int> CountActiveAsync()
    {
        return await _departamentoRepository.CountActiveAsync();
    }

    public async Task<(IEnumerable<Departamento> Items, int TotalCount)> GetAllPagedAsync(int pageNumber = 1, int pageSize = 50)
    {
        return await _departamentoRepository.GetAllActivePagedAsync(pageNumber, pageSize);
    }
}
