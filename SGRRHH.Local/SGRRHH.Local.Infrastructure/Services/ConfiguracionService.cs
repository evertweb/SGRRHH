using System.Text.Json;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Configuration;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class ConfiguracionService : IConfiguracionService
{
    private readonly IConfiguracionRepository _repository;
    private readonly ILogger<ConfiguracionService> _logger;
    private Dictionary<string, string>? _cache;
    
    public ConfiguracionService(
        IConfiguracionRepository repository,
        ILogger<ConfiguracionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    private async Task EnsureCacheLoadedAsync()
    {
        if (_cache == null)
        {
            await LoadCacheAsync();
        }
    }
    
    private async Task LoadCacheAsync()
    {
        try
        {
            var configs = await _repository.GetAllAsync();
            _cache = configs.ToDictionary(c => c.Clave, c => c.Valor);
            _logger.LogInformation("Configuraciones cargadas en cache: {Count}", _cache.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cargando configuraciones");
            _cache = new Dictionary<string, string>();
        }
    }
    
    public async Task<string?> GetAsync(string clave)
    {
        try
        {
            await EnsureCacheLoadedAsync();
            
            if (_cache!.TryGetValue(clave, out var valor))
            {
                return valor;
            }
            
            // Retornar valor por defecto si existe
            if (ConfigKeys.DefaultValues.TryGetValue(clave, out var defaultValue))
            {
                _logger.LogInformation("Usando valor por defecto para {Clave}: {Value}", 
                    clave, defaultValue);
                return defaultValue;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo configuración: {Clave}", clave);
            
            // Intentar retornar valor por defecto
            if (ConfigKeys.DefaultValues.TryGetValue(clave, out var defaultValue))
            {
                return defaultValue;
            }
            
            return null;
        }
    }
    
    public async Task<T?> GetAsync<T>(string clave)
    {
        try
        {
            var valor = await GetAsync(clave);
            
            if (string.IsNullOrWhiteSpace(valor))
                return default;
            
            // Conversiones especiales
            if (typeof(T) == typeof(bool))
            {
                return (T)(object)(valor.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                                   valor == "1");
            }
            
            if (typeof(T) == typeof(int))
            {
                return int.TryParse(valor, out var intValue) 
                    ? (T)(object)intValue 
                    : default;
            }
            
            if (typeof(T) == typeof(decimal))
            {
                return decimal.TryParse(valor, out var decValue) 
                    ? (T)(object)decValue 
                    : default;
            }
            
            if (typeof(T) == typeof(DateTime))
            {
                return DateTime.TryParse(valor, out var dateValue) 
                    ? (T)(object)dateValue 
                    : default;
            }
            
            // Para tipos complejos, intentar deserializar JSON
            if (!typeof(T).IsPrimitive && typeof(T) != typeof(string))
            {
                return JsonSerializer.Deserialize<T>(valor);
            }
            
            return (T)Convert.ChangeType(valor, typeof(T));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo configuración tipada: {Clave}", clave);
            return default;
        }
    }
    
    public async Task<Result> SetAsync(string clave, string valor)
    {
        try
        {
            var config = (await _repository.GetAllAsync())
                .FirstOrDefault(c => c.Clave == clave);
            
            if (config == null)
            {
                config = new Domain.Entities.ConfiguracionSistema
                {
                    Clave = clave,
                    Valor = valor
                };
                await _repository.AddAsync(config);
            }
            else
            {
                config.Valor = valor;
                await _repository.UpdateAsync(config);
            }
            
            // Actualizar cache
            await LoadCacheAsync();
            
            _logger.LogInformation("Configuración actualizada: {Clave} = {Valor}", clave, valor);
            return Result.Ok("Configuración guardada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando configuración: {Clave}", clave);
            return Result.Fail($"Error guardando configuración: {ex.Message}");
        }
    }
    
    public async Task<Result> SetAsync<T>(string clave, T valor)
    {
        try
        {
            string valorString;
            
            if (typeof(T) == typeof(bool))
            {
                valorString = (bool)(object)valor! ? "true" : "false";
            }
            else if (typeof(T).IsPrimitive || typeof(T) == typeof(string) || 
                     typeof(T) == typeof(decimal) || typeof(T) == typeof(DateTime))
            {
                valorString = valor?.ToString() ?? "";
            }
            else
            {
                // Para tipos complejos, serializar a JSON
                valorString = JsonSerializer.Serialize(valor);
            }
            
            return await SetAsync(clave, valorString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando configuración tipada: {Clave}", clave);
            return Result.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        try
        {
            await EnsureCacheLoadedAsync();
            return new Dictionary<string, string>(_cache!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo todas las configuraciones");
            return new Dictionary<string, string>();
        }
    }
    
    public async Task<Dictionary<string, string>> GetByCategoriaAsync(string categoria)
    {
        try
        {
            var configs = await _repository.GetAllAsync();
            return configs
                .Where(c => c.Categoria == categoria)
                .ToDictionary(c => c.Clave, c => c.Valor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo configuraciones por categoría: {Categoria}", 
                categoria);
            return new Dictionary<string, string>();
        }
    }
}
