using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Interfaces;
using System.Collections.Concurrent;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de cache usando IMemoryCache.
/// Proporciona cache en memoria con expiración configurable para reducir
/// llamadas a Firestore en datos que cambian poco.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService>? _logger;
    private readonly ConcurrentDictionary<string, byte> _keys = new();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

    // Prefijos de cache predefinidos para invalidación por grupo
    public static class CacheKeys
    {
        public const string Departamentos = "departamentos";
        public const string Cargos = "cargos";
        public const string TiposPermiso = "tipos_permiso";
        public const string Actividades = "actividades";
        public const string Proyectos = "proyectos";
        public const string Empleados = "empleados";
        public const string Configuracion = "configuracion";
    }

    public CacheService(IMemoryCache cache, ILogger<CacheService>? logger = null)
    {
        _cache = cache;
        _logger = logger;
    }

    public T? Get<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            _logger?.LogDebug("Cache HIT para key: {Key}", key);
            return value;
        }

        _logger?.LogDebug("Cache MISS para key: {Key}", key);
        return null;
    }

    public bool TryGet<T>(string key, out T? value) where T : class
    {
        var result = _cache.TryGetValue(key, out value);
        _logger?.LogDebug("Cache {Result} para key: {Key}", result ? "HIT" : "MISS", key);
        return result;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var exp = expiration ?? _defaultExpiration;

        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(exp)
            .SetSlidingExpiration(TimeSpan.FromMinutes(2)) // Renueva si se accede
            .RegisterPostEvictionCallback((k, v, reason, state) =>
            {
                _keys.TryRemove(k.ToString()!, out _);
                _logger?.LogDebug("Cache entry evicted: {Key}, Reason: {Reason}", k, reason);
            });

        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);

        _logger?.LogDebug("Cache SET para key: {Key}, Expiration: {Expiration}", key, exp);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        if (_cache.TryGetValue(key, out T? cached) && cached != null)
        {
            _logger?.LogDebug("Cache HIT (GetOrCreate) para key: {Key}", key);
            return cached;
        }

        _logger?.LogDebug("Cache MISS (GetOrCreate) para key: {Key}, ejecutando factory...", key);

        var value = await factory();
        Set(key, value, expiration);

        return value;
    }

    public void Invalidate(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        _logger?.LogInformation("Cache invalidado para key: {Key}", key);
    }

    public void InvalidateByPrefix(string prefix)
    {
        var keysToRemove = _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        _logger?.LogInformation("Cache invalidado por prefijo: {Prefix}, {Count} entries removidas", prefix, keysToRemove.Count);
    }

    public void Clear()
    {
        var allKeys = _keys.Keys.ToList();
        foreach (var key in allKeys)
        {
            _cache.Remove(key);
        }
        _keys.Clear();

        _logger?.LogInformation("Cache completamente limpiado, {Count} entries removidas", allKeys.Count);
    }
}
