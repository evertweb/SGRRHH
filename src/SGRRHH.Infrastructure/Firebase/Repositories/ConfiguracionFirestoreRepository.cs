using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Configuración del Sistema para Firestore.
/// Colección: "config"
/// Nota: Usa la clave como Document ID para acceso directo.
/// </summary>
public class ConfiguracionFirestoreRepository : FirestoreRepository<ConfiguracionSistema>, IConfiguracionRepository
{
    private const string COLLECTION_NAME = "config";
    
    public ConfiguracionFirestoreRepository(FirebaseInitializer firebase, ILogger<ConfiguracionFirestoreRepository>? logger = null) 
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(ConfiguracionSistema entity)
    {
        var doc = base.EntityToDocument(entity);
        doc["clave"] = entity.Clave;
        doc["valor"] = entity.Valor;
        doc["descripcion"] = entity.Descripcion;
        doc["categoria"] = entity.Categoria;
        return doc;
    }
    
    protected override ConfiguracionSistema DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        if (document.TryGetValue<string>("clave", out var clave))
            entity.Clave = clave;
        
        if (document.TryGetValue<string>("valor", out var valor))
            entity.Valor = valor;
        
        if (document.TryGetValue<string>("descripcion", out var descripcion))
            entity.Descripcion = descripcion;
        
        if (document.TryGetValue<string>("categoria", out var categoria))
            entity.Categoria = categoria;
        
        return entity;
    }
    
    #endregion
    
    #region IConfiguracionRepository Implementation
    
    /// <summary>
    /// Obtiene una configuración por su clave.
    /// Primero intenta buscar usando la clave como Document ID,
    /// si no existe, busca por el campo clave.
    /// </summary>
    public async Task<ConfiguracionSistema?> GetByClaveAsync(string clave)
    {
        try
        {
            // Intentar obtener directamente por Document ID (más eficiente)
            var docRef = Collection.Document(clave);
            var snapshot = await docRef.GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                return DocumentToEntity(snapshot);
            }
            
            // Si no existe, buscar por campo clave
            var query = Collection.WhereEqualTo("clave", clave).Limit(1);
            var querySnapshot = await query.GetSnapshotAsync();
            
            var doc = querySnapshot.Documents.FirstOrDefault();
            return doc != null ? DocumentToEntity(doc) : null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener configuración por clave: {Clave}", clave);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todas las configuraciones de una categoría
    /// </summary>
    public async Task<List<ConfiguracionSistema>> GetByCategoriaAsync(string categoria)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("categoria", categoria)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(c => c.Clave)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener configuraciones por categoría: {Categoria}", categoria);
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si existe una configuración con la clave especificada
    /// </summary>
    public async Task<bool> ExistsClaveAsync(string clave)
    {
        try
        {
            // Verificar como Document ID primero
            var docRef = Collection.Document(clave);
            var snapshot = await docRef.GetSnapshotAsync();
            
            if (snapshot.Exists)
                return true;
            
            // Buscar por campo clave
            var query = Collection.WhereEqualTo("clave", clave).Limit(1);
            var querySnapshot = await query.GetSnapshotAsync();
            
            return querySnapshot.Documents.Any();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar existencia de clave: {Clave}", clave);
            throw;
        }
    }
    
    /// <summary>
    /// Agrega una nueva configuración usando la clave como Document ID
    /// </summary>
    public override async Task<ConfiguracionSistema> AddAsync(ConfiguracionSistema entity)
    {
        try
        {
            entity.FechaCreacion = DateTime.Now;
            entity.Activo = true;
            
            // Generar ID si no tiene
            if (entity.Id == 0)
            {
                entity.Id = await GetNextIdAsync();
            }
            
            // Usar la clave como Document ID para acceso directo
            var documentId = SanitizeDocumentId(entity.Clave);
            var data = EntityToDocument(entity);
            var docRef = Collection.Document(documentId);
            await docRef.SetAsync(data);
            
            entity.SetFirestoreDocumentId(documentId);
            
            _logger?.LogInformation("Configuración creada: {Clave}", entity.Clave);
            
            return entity;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al agregar configuración: {Clave}", entity.Clave);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todas las configuraciones activas
    /// </summary>
    public override async Task<IEnumerable<ConfiguracionSistema>> GetAllActiveAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(c => c.Categoria)
                .ThenBy(c => c.Clave)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener configuraciones activas");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene o crea una configuración con valor por defecto
    /// </summary>
    public async Task<ConfiguracionSistema> GetOrCreateAsync(string clave, string valorDefecto, string? descripcion = null, string categoria = "General")
    {
        var config = await GetByClaveAsync(clave);
        
        if (config != null)
            return config;
        
        // Crear nueva configuración
        config = new ConfiguracionSistema
        {
            Clave = clave,
            Valor = valorDefecto,
            Descripcion = descripcion,
            Categoria = categoria,
            Activo = true
        };
        
        return await AddAsync(config);
    }
    
    /// <summary>
    /// Actualiza el valor de una configuración
    /// </summary>
    public async Task UpdateValorAsync(string clave, string valor)
    {
        try
        {
            var config = await GetByClaveAsync(clave);
            
            if (config == null)
            {
                throw new InvalidOperationException($"No se encontró la configuración con clave: {clave}");
            }
            
            config.Valor = valor;
            config.FechaModificacion = DateTime.Now;
            
            await UpdateAsync(config);
            
            _logger?.LogInformation("Configuración actualizada: {Clave} = {Valor}", clave, valor);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar valor de configuración: {Clave}", clave);
            throw;
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Sanitiza una clave para usarla como Document ID de Firestore.
    /// Firestore no permite '/' en los Document IDs.
    /// </summary>
    private static string SanitizeDocumentId(string clave)
    {
        return clave
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(" ", "_");
    }
    
    #endregion
}
