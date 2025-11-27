using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de actividades
/// </summary>
public class ActividadService : IActividadService
{
    private readonly IActividadRepository _actividadRepository;
    
    public ActividadService(IActividadRepository actividadRepository)
    {
        _actividadRepository = actividadRepository;
    }
    
    public async Task<IEnumerable<Actividad>> GetAllAsync()
    {
        return await _actividadRepository.GetAllActiveAsync();
    }
    
    public async Task<Actividad?> GetByIdAsync(int id)
    {
        return await _actividadRepository.GetByIdAsync(id);
    }
    
    public async Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria)
    {
        return await _actividadRepository.GetByCategoriaAsync(categoria);
    }
    
    public async Task<IEnumerable<string>> GetCategoriasAsync()
    {
        return await _actividadRepository.GetCategoriasAsync();
    }
    
    public async Task<IEnumerable<Actividad>> SearchAsync(string searchTerm)
    {
        return await _actividadRepository.SearchAsync(searchTerm);
    }
    
    public async Task<ServiceResult<Actividad>> CreateAsync(Actividad actividad)
    {
        var errors = new List<string>();
        
        // Validaciones
        if (string.IsNullOrWhiteSpace(actividad.Nombre))
            errors.Add("El nombre de la actividad es obligatorio");
            
        // Generar o validar código
        if (string.IsNullOrWhiteSpace(actividad.Codigo))
        {
            actividad.Codigo = await _actividadRepository.GetNextCodigoAsync();
        }
        else if (await _actividadRepository.ExistsCodigoAsync(actividad.Codigo))
        {
            errors.Add($"Ya existe una actividad con el código {actividad.Codigo}");
        }
        
        if (errors.Any())
            return ServiceResult<Actividad>.Fail(errors);
            
        actividad.Activo = true;
        actividad.FechaCreacion = DateTime.Now;
        
        await _actividadRepository.AddAsync(actividad);
        await _actividadRepository.SaveChangesAsync();
        
        return ServiceResult<Actividad>.Ok(actividad, "Actividad creada exitosamente");
    }
    
    public async Task<ServiceResult> UpdateAsync(Actividad actividad)
    {
        var existing = await _actividadRepository.GetByIdAsync(actividad.Id);
        if (existing == null)
            return ServiceResult.Fail("Actividad no encontrada");
            
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(actividad.Nombre))
            errors.Add("El nombre de la actividad es obligatorio");
            
        if (await _actividadRepository.ExistsCodigoAsync(actividad.Codigo, actividad.Id))
            errors.Add($"Ya existe otra actividad con el código {actividad.Codigo}");
            
        if (errors.Any())
            return ServiceResult.Fail(errors);
            
        existing.Codigo = actividad.Codigo;
        existing.Nombre = actividad.Nombre;
        existing.Descripcion = actividad.Descripcion;
        existing.Categoria = actividad.Categoria;
        existing.RequiereProyecto = actividad.RequiereProyecto;
        existing.Orden = actividad.Orden;
        existing.FechaModificacion = DateTime.Now;
        
        await _actividadRepository.UpdateAsync(existing);
        await _actividadRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Actividad actualizada exitosamente");
    }
    
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var actividad = await _actividadRepository.GetByIdAsync(id);
        if (actividad == null)
            return ServiceResult.Fail("Actividad no encontrada");
            
        actividad.Activo = false;
        actividad.FechaModificacion = DateTime.Now;
        
        await _actividadRepository.UpdateAsync(actividad);
        await _actividadRepository.SaveChangesAsync();
        
        return ServiceResult.Ok("Actividad eliminada exitosamente");
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        return await _actividadRepository.GetNextCodigoAsync();
    }
    
    public async Task<int> CountActiveAsync()
    {
        var activas = await _actividadRepository.GetAllActiveAsync();
        return activas.Count();
    }
}
