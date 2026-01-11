using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio de caché para catálogos frecuentemente consultados
/// </summary>
public class CatalogCacheService : ICatalogCacheService
{
    private readonly ICargoRepository _cargoRepository;
    private readonly IDepartamentoRepository _departamentoRepository;
    private readonly ITipoPermisoRepository _tipoPermisoRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IActividadRepository _actividadRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CatalogCacheService> _logger;
    
    // Cache keys
    private const string CARGOS_KEY = "catalog_cargos";
    private const string DEPARTAMENTOS_KEY = "catalog_departamentos";
    private const string TIPOS_PERMISO_KEY = "catalog_tipos_permiso";
    private const string PROYECTOS_KEY = "catalog_proyectos";
    private const string ACTIVIDADES_KEY = "catalog_actividades";
    private const string EMPLEADOS_ACTIVOS_KEY = "catalog_empleados_activos";
    private const string EMPLEADOS_TODOS_KEY = "catalog_empleados_todos";
    
    // Default cache duration
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
    
    public CatalogCacheService(
        ICargoRepository cargoRepository,
        IDepartamentoRepository departamentoRepository,
        ITipoPermisoRepository tipoPermisoRepository,
        IProyectoRepository proyectoRepository,
        IActividadRepository actividadRepository,
        IEmpleadoRepository empleadoRepository,
        IMemoryCache cache,
        ILogger<CatalogCacheService> logger)
    {
        _cargoRepository = cargoRepository;
        _departamentoRepository = departamentoRepository;
        _tipoPermisoRepository = tipoPermisoRepository;
        _proyectoRepository = proyectoRepository;
        _actividadRepository = actividadRepository;
        _empleadoRepository = empleadoRepository;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Obtiene todos los cargos (cacheado)
    /// </summary>
    public async Task<List<Cargo>> GetCargosAsync(bool forceRefresh = false)
    {
        return await GetOrLoadAsync(CARGOS_KEY, async () =>
        {
            var cargos = await _cargoRepository.GetAllAsync();
            return cargos.OrderBy(c => c.Nombre).ToList();
        }, forceRefresh);
    }
    
    /// <summary>
    /// Obtiene todos los departamentos (cacheado)
    /// </summary>
    public async Task<List<Departamento>> GetDepartamentosAsync(bool forceRefresh = false)
    {
        return await GetOrLoadAsync(DEPARTAMENTOS_KEY, async () =>
        {
            var departamentos = await _departamentoRepository.GetAllAsync();
            return departamentos.OrderBy(d => d.Nombre).ToList();
        }, forceRefresh);
    }
    
    /// <summary>
    /// Obtiene todos los tipos de permiso activos (cacheado)
    /// </summary>
    public async Task<List<TipoPermiso>> GetTiposPermisoAsync(bool forceRefresh = false)
    {
        return await GetOrLoadAsync(TIPOS_PERMISO_KEY, async () =>
        {
            var tipos = await _tipoPermisoRepository.GetAllAsync();
            return tipos.Where(t => t.Activo).OrderBy(t => t.Nombre).ToList();
        }, forceRefresh);
    }
    
    /// <summary>
    /// Obtiene todos los proyectos activos (cacheado)
    /// </summary>
    public async Task<List<Proyecto>> GetProyectosAsync(bool forceRefresh = false)
    {
        return await GetOrLoadAsync(PROYECTOS_KEY, async () =>
        {
            var proyectos = await _proyectoRepository.GetAllAsync();
            return proyectos.Where(p => p.Activo).OrderBy(p => p.Nombre).ToList();
        }, forceRefresh);
    }
    
    /// <summary>
    /// Obtiene todas las actividades (cacheado)
    /// </summary>
    public async Task<List<Actividad>> GetActividadesAsync(bool forceRefresh = false)
    {
        return await GetOrLoadAsync(ACTIVIDADES_KEY, async () =>
        {
            var actividades = await _actividadRepository.GetAllAsync();
            return actividades.OrderBy(a => a.Nombre).ToList();
        }, forceRefresh);
    }
    
    /// <summary>
    /// Obtiene todos los empleados activos (cacheado)
    /// </summary>
    public async Task<List<Empleado>> GetEmpleadosActivosAsync(bool forceRefresh = false)
    {
        return await GetOrLoadAsync(EMPLEADOS_ACTIVOS_KEY, async () =>
        {
            var empleados = await _empleadoRepository.GetAllAsync();
            return empleados
                .Where(e => e.Estado == Domain.Enums.EstadoEmpleado.Activo)
                .OrderBy(e => e.NombreCompleto)
                .ToList();
        }, forceRefresh);
    }
    
    /// <summary>
    /// Obtiene todos los empleados (cacheado)
    /// </summary>
    public async Task<List<Empleado>> GetEmpleadosTodosAsync(bool forceRefresh = false)
    {
        return await GetOrLoadAsync(EMPLEADOS_TODOS_KEY, async () =>
        {
            var empleados = await _empleadoRepository.GetAllAsync();
            return empleados.OrderBy(e => e.NombreCompleto).ToList();
        }, forceRefresh);
    }
    
    /// <summary>
    /// Invalida el caché de un catálogo específico
    /// </summary>
    public void InvalidateCache(CatalogType catalogType)
    {
        var key = catalogType switch
        {
            CatalogType.Cargos => CARGOS_KEY,
            CatalogType.Departamentos => DEPARTAMENTOS_KEY,
            CatalogType.TiposPermiso => TIPOS_PERMISO_KEY,
            CatalogType.Proyectos => PROYECTOS_KEY,
            CatalogType.Actividades => ACTIVIDADES_KEY,
            CatalogType.EmpleadosActivos => EMPLEADOS_ACTIVOS_KEY,
            CatalogType.EmpleadosTodos => EMPLEADOS_TODOS_KEY,
            _ => null
        };
        
        if (key != null)
        {
            _cache.Remove(key);
            _logger.LogInformation("Cache invalidado: {Key}", key);
        }
    }
    
    /// <summary>
    /// Invalida todo el caché de catálogos
    /// </summary>
    public void InvalidateAllCache()
    {
        _cache.Remove(CARGOS_KEY);
        _cache.Remove(DEPARTAMENTOS_KEY);
        _cache.Remove(TIPOS_PERMISO_KEY);
        _cache.Remove(PROYECTOS_KEY);
        _cache.Remove(ACTIVIDADES_KEY);
        _cache.Remove(EMPLEADOS_ACTIVOS_KEY);
        _cache.Remove(EMPLEADOS_TODOS_KEY);
        
        _logger.LogInformation("Todo el caché de catálogos ha sido invalidado");
    }
    
    /// <summary>
    /// Precarga todos los catálogos en caché
    /// </summary>
    public async Task PreloadAllCatalogsAsync()
    {
        _logger.LogInformation("Precargando catálogos en caché...");
        
        var tasks = new List<Task>
        {
            GetCargosAsync(true),
            GetDepartamentosAsync(true),
            GetTiposPermisoAsync(true),
            GetProyectosAsync(true),
            GetActividadesAsync(true),
            GetEmpleadosActivosAsync(true)
        };
        
        await Task.WhenAll(tasks);
        
        _logger.LogInformation("Catálogos precargados exitosamente");
    }
    
    private async Task<List<T>> GetOrLoadAsync<T>(string key, Func<Task<List<T>>> loader, bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            _cache.Remove(key);
        }
        
        if (!_cache.TryGetValue(key, out List<T>? cachedData) || cachedData == null)
        {
            _logger.LogDebug("Caché miss para {Key}, cargando desde base de datos", key);
            
            cachedData = await loader();
            
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_cacheDuration)
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));
            
            _cache.Set(key, cachedData, cacheOptions);
            
            _logger.LogDebug("Datos cargados en caché: {Key} ({Count} registros)", key, cachedData.Count);
        }
        else
        {
            _logger.LogDebug("Caché hit para {Key}", key);
        }
        
        return cachedData;
    }
}
