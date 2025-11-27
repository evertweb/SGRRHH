using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de cargos
/// </summary>
public class CargoService : ICargoService
{
    private readonly ICargoRepository _cargoRepository;
    
    public CargoService(ICargoRepository cargoRepository)
    {
        _cargoRepository = cargoRepository;
    }
    
    public async Task<IEnumerable<Cargo>> GetAllAsync()
    {
        return await _cargoRepository.GetAllActiveWithDepartamentoAsync();
    }
    
    public async Task<IEnumerable<Cargo>> GetByDepartamentoAsync(int departamentoId)
    {
        return await _cargoRepository.GetByDepartamentoAsync(departamentoId);
    }
    
    public async Task<Cargo?> GetByIdAsync(int id)
    {
        return await _cargoRepository.GetByIdWithDepartamentoAsync(id);
    }
    
    public async Task<ServiceResult<Cargo>> CreateAsync(Cargo cargo)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(cargo.Nombre))
            errors.Add("El nombre es obligatorio");
            
        // Generar código si está vacío
        if (string.IsNullOrWhiteSpace(cargo.Codigo))
        {
            cargo.Codigo = await _cargoRepository.GetNextCodigoAsync();
        }
        else
        {
            if (await _cargoRepository.ExistsCodigoAsync(cargo.Codigo))
                errors.Add($"Ya existe un cargo con el código {cargo.Codigo}");
        }
        
        if (errors.Any())
            return ServiceResult<Cargo>.Fail(errors);
            
        cargo.Activo = true;
        cargo.FechaCreacion = DateTime.Now;
        
        await _cargoRepository.AddAsync(cargo);
        await _cargoRepository.SaveChangesAsync();
        
        return ServiceResult<Cargo>.Ok(cargo, "Cargo creado exitosamente");
    }
    
    public async Task<ServiceResult> UpdateAsync(Cargo cargo)
    {
        var existing = await _cargoRepository.GetByIdAsync(cargo.Id);
        if (existing == null)
            return ServiceResult.Fail("Cargo no encontrado");
            
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(cargo.Nombre))
            errors.Add("El nombre es obligatorio");
            
        if (await _cargoRepository.ExistsCodigoAsync(cargo.Codigo, cargo.Id))
            errors.Add($"Ya existe otro cargo con el código {cargo.Codigo}");
            
        if (errors.Any())
            return ServiceResult.Fail(errors);
            
        existing.Codigo = cargo.Codigo;
        existing.Nombre = cargo.Nombre;
        existing.Descripcion = cargo.Descripcion;
        existing.Nivel = cargo.Nivel;
        existing.DepartamentoId = cargo.DepartamentoId;
        existing.FechaModificacion = DateTime.Now;
        
        await _cargoRepository.UpdateAsync(existing);
        await _cargoRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Cargo actualizado exitosamente");
    }
    
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var cargo = await _cargoRepository.GetByIdAsync(id);
        if (cargo == null)
            return ServiceResult.Fail("Cargo no encontrado");
            
        if (await _cargoRepository.HasEmpleadosAsync(id))
            return ServiceResult.Fail("No se puede eliminar el cargo porque tiene empleados asignados");
            
        cargo.Activo = false;
        cargo.FechaModificacion = DateTime.Now;
        
        await _cargoRepository.UpdateAsync(cargo);
        await _cargoRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Cargo eliminado exitosamente");
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        return await _cargoRepository.GetNextCodigoAsync();
    }
    
    public async Task<int> CountActiveAsync()
    {
        return await _cargoRepository.CountActiveAsync();
    }
}
