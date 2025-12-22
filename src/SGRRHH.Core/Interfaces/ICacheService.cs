namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio de cache en memoria para reducir round-trips a Firestore.
/// Ideal para datos que cambian poco (catálogos: departamentos, cargos, tipos permiso).
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtiene un valor del cache
    /// </summary>
    T? Get<T>(string key) where T : class;

    /// <summary>
    /// Intenta obtener un valor del cache
    /// </summary>
    bool TryGet<T>(string key, out T? value) where T : class;

    /// <summary>
    /// Almacena un valor en el cache con expiración opcional
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Obtiene o crea un valor en el cache usando una factory function
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Invalida una entrada específica del cache
    /// </summary>
    void Invalidate(string key);

    /// <summary>
    /// Invalida todas las entradas que comienzan con el prefijo dado
    /// </summary>
    void InvalidateByPrefix(string prefix);

    /// <summary>
    /// Limpia todo el cache
    /// </summary>
    void Clear();
}
