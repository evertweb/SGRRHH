#if !BROWSER
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Implementación base genérica de repositorio para Firestore.
/// Proporciona operaciones CRUD básicas contra una colección de Firestore.
/// </summary>
/// <typeparam name="T">Tipo de entidad que hereda de EntidadBase</typeparam>
public abstract class FirestoreRepository<T> : IFirestoreRepository<T>, IRepository<T> where T : EntidadBase, new()
{
    protected readonly FirebaseInitializer _firebase;
    protected readonly ILogger? _logger;
    protected readonly string _collectionName;
    
    /// <summary>
    /// Acceso a la base de datos Firestore
    /// </summary>
    protected FirestoreDb Firestore => _firebase.Firestore 
        ?? throw new InvalidOperationException("Firestore no está inicializado");
    
    /// <summary>
    /// Referencia a la colección
    /// </summary>
    protected CollectionReference Collection => Firestore.Collection(_collectionName);
    
    /// <summary>
    /// Constructor base
    /// </summary>
    /// <param name="firebase">Inicializador de Firebase</param>
    /// <param name="collectionName">Nombre de la colección en Firestore</param>
    /// <param name="logger">Logger opcional</param>
    protected FirestoreRepository(FirebaseInitializer firebase, string collectionName, ILogger? logger = null)
    {
        _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        _collectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
        _logger = logger;
    }
    
    #region Conversión Entity <-> Dictionary
    
    /// <summary>
    /// Convierte una entidad a un diccionario para guardar en Firestore.
    /// Las clases derivadas pueden sobrescribir para personalizar el mapeo.
    /// </summary>
    protected virtual Dictionary<string, object?> EntityToDocument(T entity)
    {
        return new Dictionary<string, object?>
        {
            ["id"] = entity.Id,
            ["activo"] = entity.Activo,
            ["fechaCreacion"] = Timestamp.FromDateTime(entity.FechaCreacion.ToUniversalTime()),
            ["fechaModificacion"] = entity.FechaModificacion.HasValue 
                ? Timestamp.FromDateTime(entity.FechaModificacion.Value.ToUniversalTime()) 
                : null
        };
    }
    
    /// <summary>
    /// Convierte un documento de Firestore a una entidad.
    /// Las clases derivadas pueden sobrescribir para personalizar el mapeo.
    /// </summary>
    protected virtual T DocumentToEntity(DocumentSnapshot document)
    {
        var entity = new T();
        
        if (document.TryGetValue<int>("id", out var id))
            entity.Id = id;
        
        if (document.TryGetValue<bool>("activo", out var activo))
            entity.Activo = activo;
        
        if (document.TryGetValue<Timestamp>("fechaCreacion", out var fechaCreacion))
            entity.FechaCreacion = fechaCreacion.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp?>("fechaModificacion", out var fechaMod) && fechaMod.HasValue)
            entity.FechaModificacion = fechaMod.Value.ToDateTime().ToLocalTime();
        
        // Guardar el Document ID en memoria
        entity.SetFirestoreDocumentId(document.Id);
        
        return entity;
    }
    
    #endregion
    
    #region IFirestoreRepository Implementation
    
    /// <summary>
    /// Obtiene una entidad por su Document ID de Firestore
    /// </summary>
    public virtual async Task<T?> GetByDocumentIdAsync(string documentId)
    {
        try
        {
            var docRef = Collection.Document(documentId);
            var snapshot = await docRef.GetSnapshotAsync();
            
            if (!snapshot.Exists)
                return null;
            
            return DocumentToEntity(snapshot);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener documento {DocumentId} de {Collection}", 
                documentId, _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene una entidad por su ID (int) buscando en el campo 'id'
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            var query = Collection.WhereEqualTo("id", id).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            if (doc == null)
                return null;
            
            return DocumentToEntity(doc);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener entidad con id {Id} de {Collection}", 
                id, _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todas las entidades de la colección
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            var snapshot = await Collection.GetSnapshotAsync();
            return snapshot.Documents.Select(DocumentToEntity).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener todas las entidades de {Collection}", _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todas las entidades activas
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllActiveAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(DocumentToEntity).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener entidades activas de {Collection}", _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Agrega una nueva entidad con Document ID auto-generado
    /// </summary>
    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            entity.FechaCreacion = DateTime.Now;
            entity.Activo = true;

            // Si no tiene ID asignado, generar uno basado en el último
            if (entity.Id == 0)
            {
                entity.Id = await GetNextIdAsync();
            }

            var data = EntityToDocument(entity);
            var docRef = await Collection.AddAsync(data);

            entity.SetFirestoreDocumentId(docRef.Id);

            _logger?.LogInformation("Entidad creada en {Collection} con DocumentId: {DocumentId}",
                _collectionName, docRef.Id);
            InvalidateCache();

            return entity;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al agregar entidad a {Collection}", _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Agrega una nueva entidad con un Document ID específico
    /// </summary>
    public virtual async Task<T> AddAsync(T entity, string documentId)
    {
        try
        {
            entity.FechaCreacion = DateTime.Now;
            entity.Activo = true;

            // Si no tiene ID asignado, generar uno basado en el último
            if (entity.Id == 0)
            {
                entity.Id = await GetNextIdAsync();
            }

            var data = EntityToDocument(entity);
            var docRef = Collection.Document(documentId);
            await docRef.SetAsync(data);

            entity.SetFirestoreDocumentId(documentId);

            _logger?.LogInformation("Entidad creada en {Collection} con DocumentId específico: {DocumentId}",
                _collectionName, documentId);
            InvalidateCache();

            return entity;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al agregar entidad a {Collection} con DocumentId {DocumentId}",
                _collectionName, documentId);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza una entidad existente (busca por Id)
    /// </summary>
    public virtual async Task UpdateAsync(T entity)
    {
        try
        {
            entity.FechaModificacion = DateTime.Now;

            // Intentar obtener el DocumentId de la entidad
            var documentId = entity.GetFirestoreDocumentId();

            if (string.IsNullOrEmpty(documentId))
            {
                // Buscar por Id
                var query = Collection.WhereEqualTo("id", entity.Id).Limit(1);
                var snapshot = await query.GetSnapshotAsync();
                var doc = snapshot.Documents.FirstOrDefault();

                if (doc == null)
                    throw new InvalidOperationException($"No se encontró la entidad con Id {entity.Id}");

                documentId = doc.Id;
            }

            await UpdateByDocumentIdAsync(documentId, entity);
            InvalidateCache();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar entidad con Id {Id} en {Collection}",
                entity.Id, _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza una entidad por su Document ID
    /// </summary>
    public virtual async Task UpdateByDocumentIdAsync(string documentId, T entity)
    {
        try
        {
            entity.FechaModificacion = DateTime.Now;
            
            var data = EntityToDocument(entity);
            var docRef = Collection.Document(documentId);
            await docRef.SetAsync(data, SetOptions.MergeAll);
            
            entity.SetFirestoreDocumentId(documentId);
            
            _logger?.LogInformation("Entidad actualizada en {Collection} con DocumentId: {DocumentId}", 
                _collectionName, documentId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar documento {DocumentId} en {Collection}", 
                documentId, _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Elimina (desactiva) una entidad por Id (soft delete)
    /// </summary>
    public virtual async Task DeleteAsync(int id)
    {
        try
        {
            var query = Collection.WhereEqualTo("id", id).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            var doc = snapshot.Documents.FirstOrDefault();

            if (doc == null)
            {
                _logger?.LogWarning("No se encontró entidad con Id {Id} para eliminar en {Collection}",
                    id, _collectionName);
                return;
            }

            await DeleteByDocumentIdAsync(doc.Id);
            InvalidateCache();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar entidad con Id {Id} de {Collection}",
                id, _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Elimina (desactiva) una entidad por Document ID (soft delete)
    /// </summary>
    public virtual async Task DeleteByDocumentIdAsync(string documentId)
    {
        try
        {
            var docRef = Collection.Document(documentId);
            await docRef.UpdateAsync(new Dictionary<string, object>
            {
                ["activo"] = false,
                ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
            });

            _logger?.LogInformation("Entidad desactivada (soft delete) en {Collection} con DocumentId: {DocumentId}",
                _collectionName, documentId);
            InvalidateCache();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al desactivar documento {DocumentId} en {Collection}",
                documentId, _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Elimina permanentemente una entidad (hard delete)
    /// </summary>
    public virtual async Task HardDeleteAsync(string documentId)
    {
        try
        {
            var docRef = Collection.Document(documentId);
            await docRef.DeleteAsync();
            
            _logger?.LogInformation("Entidad eliminada permanentemente de {Collection} con DocumentId: {DocumentId}", 
                _collectionName, documentId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar permanentemente documento {DocumentId} de {Collection}", 
                documentId, _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// No aplica en Firestore - las operaciones son atómicas
    /// </summary>
    public Task<int> SaveChangesAsync()
    {
        // En Firestore las operaciones son atómicas, no se necesita "guardar"
        return Task.FromResult(0);
    }
    
    /// <summary>
    /// Obtiene el Document ID de Firestore de una entidad
    /// </summary>
    public string? GetDocumentId(T entity)
    {
        return entity.GetFirestoreDocumentId();
    }
    
    /// <summary>
    /// Establece el Document ID de Firestore en una entidad
    /// </summary>
    public void SetDocumentId(T entity, string documentId)
    {
        entity.SetFirestoreDocumentId(documentId);
    }
    
    #endregion
    
    #region Métodos auxiliares
    
    /// <summary>
    /// Obtiene el siguiente ID disponible para nuevas entidades
    /// </summary>
    protected virtual async Task<int> GetNextIdAsync()
    {
        try
        {
            var query = Collection.OrderByDescending("id").Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            if (!snapshot.Documents.Any())
                return 1;
            
            var doc = snapshot.Documents.First();
            if (doc.TryGetValue<int>("id", out var maxId))
                return maxId + 1;
            
            return 1;
        }
        catch
        {
            return 1;
        }
    }
    
    /// <summary>
    /// Cuenta el total de documentos activos
    /// </summary>
    public virtual async Task<int> CountActiveAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Count;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al contar entidades activas en {Collection}", _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si existe un documento con el campo y valor especificado
    /// </summary>
    protected virtual async Task<bool> ExistsAsync(string field, object value, string? excludeDocumentId = null)
    {
        try
        {
            var query = Collection.WhereEqualTo(field, value);
            var snapshot = await query.GetSnapshotAsync();
            
            if (string.IsNullOrEmpty(excludeDocumentId))
                return snapshot.Documents.Any();
            
            return snapshot.Documents.Any(d => d.Id != excludeDocumentId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar existencia de {Field}={Value} en {Collection}", 
                field, value, _collectionName);
            throw;
        }
    }
    
    /// <summary>
    /// Ejecuta una transacción de Firestore
    /// </summary>
    protected async Task<TResult> RunTransactionAsync<TResult>(Func<Transaction, Task<TResult>> updateFunction)
    {
        return await Firestore.RunTransactionAsync(updateFunction);
    }
    
    /// <summary>
    /// Ejecuta un batch de escrituras
    /// </summary>
    protected WriteBatch CreateBatch()
    {
        return Firestore.StartBatch();
    }
    
    /// <summary>
    /// Obtiene el siguiente código disponible con formato PREFIX + número (ej: DEP001, CAR002).
    /// Este método está en la clase base para evitar duplicación en cada repositorio.
    /// </summary>
    /// <param name="prefix">Prefijo del código (ej: "DEP", "CAR", "EMP")</param>
    /// <param name="digits">Cantidad de dígitos para el número (default: 3)</param>
    /// <returns>Siguiente código disponible</returns>
    protected async Task<string> GetNextCodigoAsync(string prefix, int digits = 3)
    {
        try
        {
            var query = Collection.OrderByDescending("codigo").Limit(10);
            var snapshot = await query.GetSnapshotAsync();
            
            int maxNumber = 0;
            foreach (var doc in snapshot.Documents)
            {
                if (doc.TryGetValue<string>("codigo", out var codigo) && 
                    !string.IsNullOrEmpty(codigo) && 
                    codigo.StartsWith(prefix))
                {
                    var numberPart = codigo.Substring(prefix.Length);
                    if (int.TryParse(numberPart, out int num) && num > maxNumber)
                    {
                        maxNumber = num;
                    }
                }
            }
            
            return $"{prefix}{(maxNumber + 1).ToString($"D{digits}")}";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener siguiente código con prefijo {Prefix} en {Collection}", 
                prefix, _collectionName);
            return $"{prefix}{"1".PadLeft(digits, '0')}";
        }
    }

    /// <summary>
    /// Invalida el caché de la entidad (para ser sobrescrito en subclases que usan caché)
    /// </summary>
    protected virtual void InvalidateCache()
    {
        // Implementación por defecto: no hacer nada
        // Las subclases que usan caché pueden sobrescribir este método
    }

    #endregion
}
#endif
