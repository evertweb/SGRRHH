using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de proyectos
/// </summary>
public class ProyectoService : IProyectoService
{
    private readonly IProyectoRepository _proyectoRepository;
    
    public ProyectoService(IProyectoRepository proyectoRepository)
    {
        _proyectoRepository = proyectoRepository;
    }
    
    public async Task<IEnumerable<Proyecto>> GetAllAsync()
    {
        return await _proyectoRepository.GetAllActiveAsync();
    }
    
    public async Task<Proyecto?> GetByIdAsync(int id)
    {
        return await _proyectoRepository.GetByIdAsync(id);
    }
    
    public async Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm)
    {
        return await _proyectoRepository.SearchAsync(searchTerm);
    }
    
    public async Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado)
    {
        return await _proyectoRepository.GetByEstadoAsync(estado);
    }
    
    public async Task<ServiceResult<Proyecto>> CreateAsync(Proyecto proyecto)
    {
        var errors = new List<string>();
        
        // Validaciones
        if (string.IsNullOrWhiteSpace(proyecto.Nombre))
            errors.Add("El nombre del proyecto es obligatorio");
            
        // Generar o validar código
        if (string.IsNullOrWhiteSpace(proyecto.Codigo))
        {
            proyecto.Codigo = await _proyectoRepository.GetNextCodigoAsync();
        }
        else if (await _proyectoRepository.ExistsCodigoAsync(proyecto.Codigo))
        {
            errors.Add($"Ya existe un proyecto con el código {proyecto.Codigo}");
        }
        
        if (errors.Any())
            return ServiceResult<Proyecto>.Fail(errors);
            
        proyecto.Activo = true;
        proyecto.FechaCreacion = DateTime.Now;
        proyecto.Estado = EstadoProyecto.Activo;
        
        await _proyectoRepository.AddAsync(proyecto);
        await _proyectoRepository.SaveChangesAsync();
        
        return ServiceResult<Proyecto>.Ok(proyecto, "Proyecto creado exitosamente");
    }
    
    public async Task<ServiceResult> UpdateAsync(Proyecto proyecto)
    {
        var existing = await _proyectoRepository.GetByIdAsync(proyecto.Id);
        if (existing == null)
            return ServiceResult.Fail("Proyecto no encontrado");
            
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(proyecto.Nombre))
            errors.Add("El nombre del proyecto es obligatorio");
            
        if (await _proyectoRepository.ExistsCodigoAsync(proyecto.Codigo, proyecto.Id))
            errors.Add($"Ya existe otro proyecto con el código {proyecto.Codigo}");
            
        if (errors.Any())
            return ServiceResult.Fail(errors);
            
        existing.Codigo = proyecto.Codigo;
        existing.Nombre = proyecto.Nombre;
        existing.Descripcion = proyecto.Descripcion;
        existing.Cliente = proyecto.Cliente;
        existing.FechaInicio = proyecto.FechaInicio;
        existing.FechaFin = proyecto.FechaFin;
        existing.Estado = proyecto.Estado;
        existing.FechaModificacion = DateTime.Now;
        
        await _proyectoRepository.UpdateAsync(existing);
        await _proyectoRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Proyecto actualizado exitosamente");
    }
    
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(id);
        if (proyecto == null)
            return ServiceResult.Fail("Proyecto no encontrado");
            
        proyecto.Activo = false;
        proyecto.FechaModificacion = DateTime.Now;
        
        await _proyectoRepository.UpdateAsync(proyecto);
        await _proyectoRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Proyecto eliminado exitosamente");
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        return await _proyectoRepository.GetNextCodigoAsync();
    }
    
    public async Task<int> CountActiveAsync()
    {
        var activos = await _proyectoRepository.GetAllActiveAsync();
        return activos.Count();
    }
}
