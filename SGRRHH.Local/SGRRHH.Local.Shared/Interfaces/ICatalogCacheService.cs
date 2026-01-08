using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Interfaz para el servicio de caché de catálogos
/// </summary>
public interface ICatalogCacheService
{
    /// <summary>
    /// Obtiene todos los cargos (cacheado)
    /// </summary>
    Task<List<Cargo>> GetCargosAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Obtiene todos los departamentos (cacheado)
    /// </summary>
    Task<List<Departamento>> GetDepartamentosAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Obtiene todos los tipos de permiso activos (cacheado)
    /// </summary>
    Task<List<TipoPermiso>> GetTiposPermisoAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Obtiene todos los proyectos activos (cacheado)
    /// </summary>
    Task<List<Proyecto>> GetProyectosAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Obtiene todas las actividades activas (cacheado)
    /// </summary>
    Task<List<Actividad>> GetActividadesAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Obtiene todos los empleados activos (cacheado)
    /// </summary>
    Task<List<Empleado>> GetEmpleadosActivosAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Obtiene todos los empleados (cacheado)
    /// </summary>
    Task<List<Empleado>> GetEmpleadosTodosAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Invalida todo el caché de catálogos
    /// </summary>
    void InvalidateAllCache();
    
    /// <summary>
    /// Precarga todos los catálogos en caché
    /// </summary>
    Task PreloadAllCatalogsAsync();
}
