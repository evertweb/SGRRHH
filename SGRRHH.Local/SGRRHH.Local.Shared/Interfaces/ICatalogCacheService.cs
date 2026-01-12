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
    /// Obtiene todas las EPS vigentes (cacheado)
    /// </summary>
    Task<List<Eps>> GetEpsAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Obtiene todas las AFP vigentes (cacheado)
    /// </summary>
    Task<List<Afp>> GetAfpAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Obtiene todas las ARL vigentes (cacheado)
    /// </summary>
    Task<List<Arl>> GetArlAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Obtiene todas las Cajas de Compensación vigentes (cacheado)
    /// </summary>
    Task<List<CajaCompensacion>> GetCajasCompensacionAsync(bool forceRefresh = false);
    
    /// <summary>
    /// Invalida el caché de un catálogo específico
    /// </summary>
    void InvalidateCache(CatalogType catalogType);
    
    /// <summary>
    /// Invalida todo el caché de catálogos
    /// </summary>
    void InvalidateAllCache();
    
    /// <summary>
    /// Precarga todos los catálogos en caché
    /// </summary>
    Task PreloadAllCatalogsAsync();
}

/// <summary>
/// Tipos de catálogos disponibles para invalidación de caché
/// </summary>
public enum CatalogType
{
    Cargos,
    Departamentos,
    TiposPermiso,
    Proyectos,
    Actividades,
    EmpleadosActivos,
    EmpleadosTodos,
    Eps,
    Afp,
    Arl,
    CajasCompensacion
}
