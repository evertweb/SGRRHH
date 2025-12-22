using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Actividades para Firestore.
/// Colección: "actividades"
/// Incluye cache en memoria para reducir round-trips.
/// </summary>
public class ActividadFirestoreRepository : FirestoreRepository<Actividad>, IActividadRepository
{
    private const string COLLECTION_NAME = "actividades";
    private const string CODE_PREFIX = "ACT-";
    private const string CACHE_KEY_ALL_ACTIVE = "actividades_all_active";
    private const string CACHE_KEY_CATEGORIAS = "actividades_categorias";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    private readonly ICacheService? _cache;

    public ActividadFirestoreRepository(
        FirebaseInitializer firebase,
        ICacheService? cache = null,
        ILogger<ActividadFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
        _cache = cache;
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(Actividad entity)
    {
        var doc = base.EntityToDocument(entity);
        doc["codigo"] = entity.Codigo;
        doc["nombre"] = entity.Nombre;
        doc["descripcion"] = entity.Descripcion;
        doc["categoria"] = entity.Categoria;
        doc["requiereProyecto"] = entity.RequiereProyecto;
        doc["orden"] = entity.Orden;
        // Campo para búsquedas case-insensitive
        doc["nombreLower"] = entity.Nombre?.ToLowerInvariant();
        doc["categoriaLower"] = entity.Categoria?.ToLowerInvariant();
        return doc;
    }
    
    protected override Actividad DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        if (document.TryGetValue<string>("codigo", out var codigo))
            entity.Codigo = codigo;
        
        if (document.TryGetValue<string>("nombre", out var nombre))
            entity.Nombre = nombre;
        
        if (document.TryGetValue<string>("descripcion", out var descripcion))
            entity.Descripcion = descripcion;
        
        if (document.TryGetValue<string>("categoria", out var categoria))
            entity.Categoria = categoria;
        
        if (document.TryGetValue<bool>("requiereProyecto", out var requiereProyecto))
            entity.RequiereProyecto = requiereProyecto;
        
        if (document.TryGetValue<int>("orden", out var orden))
            entity.Orden = orden;
        
        return entity;
    }
    
    #endregion
    
    #region IActividadRepository Implementation

    /// <summary>
    /// Obtiene todas las actividades activas ordenadas por categoría, orden y nombre.
    /// Usa cache en memoria con expiración de 10 minutos.
    /// </summary>
    public override async Task<IEnumerable<Actividad>> GetAllActiveAsync()
    {
        try
        {
            if (_cache != null)
            {
                return await _cache.GetOrCreateAsync(
                    CACHE_KEY_ALL_ACTIVE,
                    FetchAllActiveFromFirestoreAsync,
                    CacheExpiration);
            }

            return await FetchAllActiveFromFirestoreAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener actividades activas");
            throw;
        }
    }

    private async Task<IEnumerable<Actividad>> FetchAllActiveFromFirestoreAsync()
    {
        var query = Collection.WhereEqualTo("activo", true);
        var snapshot = await query.GetSnapshotAsync();

        return snapshot.Documents
            .Select(DocumentToEntity)
            .OrderBy(a => a.Categoria ?? "")
            .ThenBy(a => a.Orden)
            .ThenBy(a => a.Nombre)
            .ToList();
    }

    /// <summary>
    /// Invalida el cache de actividades.
    /// </summary>
    public void InvalidateCache()
    {
        _cache?.InvalidateByPrefix("actividades");
    }
    
    /// <summary>
    /// Obtiene actividades por categoría
    /// </summary>
    public async Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("activo", true)
                .WhereEqualTo("categoria", categoria);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(a => a.Orden)
                .ThenBy(a => a.Nombre)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener actividades por categoría: {Categoria}", categoria);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene las categorías disponibles (distintas)
    /// </summary>
    public async Task<IEnumerable<string>> GetCategoriasAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(doc => doc.TryGetValue<string>("categoria", out var cat) ? cat : null)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList()!;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener categorías de actividades");
            throw;
        }
    }
    
    /// <summary>
    /// Busca actividades por término
    /// </summary>
    public async Task<IEnumerable<Actividad>> SearchAsync(string searchTerm)
    {
        try
        {
            var term = searchTerm.ToLowerInvariant().Trim();
            
            // Firestore no tiene búsqueda full-text nativa, así que obtenemos todas y filtramos
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(a => 
                    (a.Codigo?.ToLowerInvariant().Contains(term) ?? false) ||
                    (a.Nombre?.ToLowerInvariant().Contains(term) ?? false) ||
                    (a.Categoria?.ToLowerInvariant().Contains(term) ?? false) ||
                    (a.Descripcion?.ToLowerInvariant().Contains(term) ?? false))
                .OrderBy(a => a.Categoria ?? "")
                .ThenBy(a => a.Orden)
                .ThenBy(a => a.Nombre)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al buscar actividades con término: {SearchTerm}", searchTerm);
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si existe una actividad con el código especificado
    /// </summary>
    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        try
        {
            var codigoLower = codigo.ToLowerInvariant();
            var query = Collection.WhereEqualTo("codigo", codigo);
            var snapshot = await query.GetSnapshotAsync();
            
            if (!excludeId.HasValue)
                return snapshot.Documents.Any();
            
            return snapshot.Documents.Any(doc => 
                doc.TryGetValue<int>("id", out var id) && id != excludeId.Value);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar código existente: {Codigo}", codigo);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene el siguiente código disponible (ACT-0001, ACT-0002, etc.)
    /// </summary>
    public async Task<string> GetNextCodigoAsync()
    {
        return await GetNextCodigoAsync(CODE_PREFIX, 4);
    }
    
    #endregion
}
