using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Tipos de Permiso para Firestore.
/// Colección: "tipos-permiso"
/// Incluye cache en memoria con expiración de 30 minutos (cambia muy poco).
/// </summary>
public class TipoPermisoFirestoreRepository : FirestoreRepository<TipoPermiso>, ITipoPermisoRepository
{
    private const string COLLECTION_NAME = "tipos-permiso";
    private const string CACHE_KEY_ALL_ACTIVE = "tipos_permiso_all_active";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    private readonly ICacheService? _cache;

    public TipoPermisoFirestoreRepository(
        FirebaseInitializer firebase,
        ICacheService? cache = null,
        ILogger<TipoPermisoFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
        _cache = cache;
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(TipoPermiso entity)
    {
        var doc = base.EntityToDocument(entity);
        doc["nombre"] = entity.Nombre;
        doc["descripcion"] = entity.Descripcion;
        doc["color"] = entity.Color;
        doc["requiereAprobacion"] = entity.RequiereAprobacion;
        doc["requiereDocumento"] = entity.RequiereDocumento;
        doc["diasPorDefecto"] = entity.DiasPorDefecto;
        doc["esCompensable"] = entity.EsCompensable;
        // Campo para búsquedas case-insensitive
        doc["nombreLower"] = entity.Nombre?.ToLowerInvariant();
        return doc;
    }
    
    protected override TipoPermiso DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        if (document.TryGetValue<string>("nombre", out var nombre))
            entity.Nombre = nombre;
        
        if (document.TryGetValue<string>("descripcion", out var descripcion))
            entity.Descripcion = descripcion;
        
        if (document.TryGetValue<string>("color", out var color))
            entity.Color = color;
        
        if (document.TryGetValue<bool>("requiereAprobacion", out var requiereAprobacion))
            entity.RequiereAprobacion = requiereAprobacion;
        
        if (document.TryGetValue<bool>("requiereDocumento", out var requiereDocumento))
            entity.RequiereDocumento = requiereDocumento;
        
        if (document.TryGetValue<int>("diasPorDefecto", out var diasPorDefecto))
            entity.DiasPorDefecto = diasPorDefecto;
        
        if (document.TryGetValue<bool>("esCompensable", out var esCompensable))
            entity.EsCompensable = esCompensable;
        
        return entity;
    }
    
    #endregion
    
    #region ITipoPermisoRepository Implementation

    /// <summary>
    /// Obtiene todos los tipos de permiso activos ordenados por nombre.
    /// Usa cache en memoria con expiración de 30 minutos.
    /// </summary>
    public async Task<IEnumerable<TipoPermiso>> GetActivosAsync()
    {
        try
        {
            if (_cache != null)
            {
                return await _cache.GetOrCreateAsync(
                    CACHE_KEY_ALL_ACTIVE,
                    FetchActivosFromFirestoreAsync,
                    CacheExpiration);
            }

            return await FetchActivosFromFirestoreAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener tipos de permiso activos");
            throw;
        }
    }

    private async Task<IEnumerable<TipoPermiso>> FetchActivosFromFirestoreAsync()
    {
        var query = Collection.WhereEqualTo("activo", true);
        var snapshot = await query.GetSnapshotAsync();

        return snapshot.Documents
            .Select(DocumentToEntity)
            .OrderBy(tp => tp.Nombre)
            .ToList();
    }

    /// <summary>
    /// Invalida el cache de tipos de permiso.
    /// </summary>
    public void InvalidateCache()
    {
        _cache?.InvalidateByPrefix("tipos_permiso");
    }
    
    /// <summary>
    /// Verifica si existe un tipo de permiso con el nombre especificado
    /// </summary>
    public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
    {
        try
        {
            var nombreLower = nombre.ToLowerInvariant();
            var query = Collection.WhereEqualTo("nombreLower", nombreLower);
            var snapshot = await query.GetSnapshotAsync();
            
            if (!excludeId.HasValue)
                return snapshot.Documents.Any();
            
            return snapshot.Documents.Any(doc => 
                doc.TryGetValue<int>("id", out var id) && id != excludeId.Value);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar nombre existente: {Nombre}", nombre);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los tipos de permiso activos (override del base)
    /// </summary>
    public override async Task<IEnumerable<TipoPermiso>> GetAllActiveAsync()
    {
        return await GetActivosAsync();
    }
    
    #endregion
}
