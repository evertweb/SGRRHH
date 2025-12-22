using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el servicio de actividades
/// </summary>
public interface IActividadService
{
    /// <summary>
    /// Obtiene todas las actividades activas
    /// </summary>
    Task<IEnumerable<Actividad>> GetAllAsync();
    
    /// <summary>
    /// Obtiene una actividad por ID
    /// </summary>
    Task<Actividad?> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene actividades por categoría
    /// </summary>
    Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria);
    
    /// <summary>
    /// Obtiene las categorías disponibles
    /// </summary>
    Task<IEnumerable<string>> GetCategoriasAsync();
    
    /// <summary>
    /// Busca actividades por término
    /// </summary>
    Task<IEnumerable<Actividad>> SearchAsync(string searchTerm);
    
    /// <summary>
    /// Crea una nueva actividad
    /// </summary>
    Task<ServiceResult<Actividad>> CreateAsync(Actividad actividad);
    
    /// <summary>
    /// Actualiza una actividad existente
    /// </summary>
    Task<ServiceResult> UpdateAsync(Actividad actividad);
    
    /// <summary>
    /// Elimina (desactiva) una actividad
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Obtiene el siguiente código disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();
    
    /// <summary>
    /// Cuenta actividades activas
    /// </summary>
    Task<int> CountActiveAsync();
}
