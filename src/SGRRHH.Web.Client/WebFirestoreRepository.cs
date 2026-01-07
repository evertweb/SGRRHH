using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SGRRHH.Web.Client;

/// <summary>
/// Implementación base de repositorio Firestore para Blazor WASM.
/// Utiliza FirebaseJsInterop para comunicarse con el JS SDK.
/// </summary>
public abstract class WebFirestoreRepository<T> : IFirestoreRepository<T>, IRepository<T> where T : EntidadBase, new()
{
    protected readonly FirebaseJsInterop _firebase;
    protected readonly string _collectionName;

    protected WebFirestoreRepository(FirebaseJsInterop firebase, string collectionName)
    {
        _firebase = firebase;
        _collectionName = collectionName;
    }

    /// <summary>
    /// Convierte la entidad a un objeto compatible con JS Interop.
    /// Firestore JS solo acepta objetos planos.
    /// </summary>
    protected virtual object EntityToJs(T entity)
    {
        // En una implementación real usaríamos System.Text.Json para serializar/deserializar
        // para asegurar que las fechas y enums se manejen correctamente.
        return entity; 
    }

    /// <summary>
    /// Opciones de serialización JSON con convertidores para Firestore Timestamps.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = 
        {
            new FirestoreTimestampConverter(),
            new FirestoreTimestampConverterNonNullable(),
            new TimeSpanJsonConverter(),
            new TimeSpanJsonConverterNonNullable(),
            new JsonStringEnumConverter()
        }
    };

    /// <summary>
    /// Procesa el resultado crudo de JS (JsonElement o Dictionary) a la entidad T.
    /// </summary>
    protected virtual T MapToEntity(object? data)
    {
        if (data == null) return new T();

        // Si viene como JsonElement (común en JS Interop)
        if (data is JsonElement element)
        {
            var entity = JsonSerializer.Deserialize<T>(element.GetRawText(), _jsonOptions) ?? new T();
            
            // Extraer el _documentId que inyectamos en JS
            if (element.TryGetProperty("_documentId", out var idProp))
            {
                entity.SetFirestoreDocumentId(idProp.GetString() ?? "");
            }
            return entity;
        }

        return new T();
    }

    public virtual async Task<T?> GetByDocumentIdAsync(string documentId)
    {
        var data = await _firebase.GetDocumentAsync<object>(_collectionName, documentId);
        return data != null ? MapToEntity(data) : null;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "id", "==", id);
        return results.Select(MapToEntity).FirstOrDefault();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var results = await _firebase.GetCollectionAsync<object>(_collectionName);
        return results.Select(MapToEntity).ToList();
    }

    public virtual async Task<IEnumerable<T>> GetAllActiveAsync()
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "activo", "==", true);
        return results.Select(MapToEntity).ToList();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        // Generar ID si no tiene
        if (entity.Id <= 0)
        {
            entity.Id = await GetNextIdAsync();
        }

        entity.FechaCreacion = DateTime.Now;
        entity.Activo = true;

        var docId = await _firebase.AddDocumentAsync(_collectionName, entity);
        entity.SetFirestoreDocumentId(docId);
        return entity;
    }

    public virtual async Task<T> AddAsync(T entity, string documentId)
    {
        if (entity.Id <= 0)
        {
            entity.Id = await GetNextIdAsync();
        }

        entity.FechaCreacion = DateTime.Now;
        entity.Activo = true;

        await _firebase.SetDocumentAsync(_collectionName, documentId, entity);
        entity.SetFirestoreDocumentId(documentId);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        entity.FechaModificacion = DateTime.Now;
        var docId = entity.GetFirestoreDocumentId();

        if (string.IsNullOrEmpty(docId))
        {
            // Buscar por ID numérico si no tenemos el DocumentID
            var existing = await GetByIdAsync(entity.Id);
            if (existing == null) throw new Exception($"No se encontró entidad con ID {entity.Id}");
            docId = existing.GetFirestoreDocumentId();
        }

        if (string.IsNullOrEmpty(docId)) throw new Exception("No se pudo obtener el DocumentID para actualizar");

        await _firebase.SetDocumentAsync(_collectionName, docId, entity);
    }

    public virtual async Task UpdateByDocumentIdAsync(string documentId, T entity)
    {
        entity.FechaModificacion = DateTime.Now;
        await _firebase.SetDocumentAsync(_collectionName, documentId, entity);
        entity.SetFirestoreDocumentId(documentId);
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.Activo = false;
            await UpdateAsync(entity);
        }
    }

    public virtual async Task DeleteByDocumentIdAsync(string documentId)
    {
        var entity = await GetByDocumentIdAsync(documentId);
        if (entity != null)
        {
            entity.Activo = false;
            await UpdateByDocumentIdAsync(documentId, entity);
        }
    }

    public virtual async Task HardDeleteAsync(string documentId)
    {
        await _firebase.DeleteDocumentAsync(_collectionName, documentId);
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);

    public string? GetDocumentId(T entity) => entity.GetFirestoreDocumentId();

    public void SetDocumentId(T entity, string documentId) => entity.SetFirestoreDocumentId(documentId);

    protected virtual async Task<int> GetNextIdAsync()
    {
        // Optimization: Use server-side sorting and limiting to get only the last ID
        try 
        {
            var lastItems = await _firebase.QueryCollectionOrderedAsync<object>(
                _collectionName, "id", "desc", 1);
            
            var lastEntity = lastItems.Select(MapToEntity).FirstOrDefault();
            return (lastEntity?.Id ?? 0) + 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebFirestoreRepository] Error getting next ID for {_collectionName}: {ex.Message}. Fallback to GetAllAsync.");
            // Fallback in case index is missing (requires composite index sometimes, though single field desc should work auto)
            var all = await GetAllAsync();
            if (!all.Any()) return 1;
            return all.Max(x => x.Id) + 1;
        }
    }
}
