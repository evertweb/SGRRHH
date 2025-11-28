using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz base para repositorios Firestore.
/// Difiere de IRepository en que usa string para IDs (Document IDs de Firestore).
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public interface IFirestoreRepository<T> where T : EntidadBase
{
    /// <summary>
    /// Obtiene una entidad por su Document ID de Firestore
    /// </summary>
    Task<T?> GetByDocumentIdAsync(string documentId);
    
    /// <summary>
    /// Obtiene una entidad por su ID (int) - para compatibilidad
    /// </summary>
    Task<T?> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene todas las entidades
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Obtiene todas las entidades activas
    /// </summary>
    Task<IEnumerable<T>> GetAllActiveAsync();
    
    /// <summary>
    /// Agrega una nueva entidad
    /// </summary>
    /// <returns>La entidad con el Document ID asignado</returns>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Agrega una nueva entidad con un Document ID específico
    /// </summary>
    Task<T> AddAsync(T entity, string documentId);
    
    /// <summary>
    /// Actualiza una entidad existente
    /// </summary>
    Task UpdateAsync(T entity);
    
    /// <summary>
    /// Actualiza una entidad por su Document ID
    /// </summary>
    Task UpdateByDocumentIdAsync(string documentId, T entity);
    
    /// <summary>
    /// Elimina (desactiva) una entidad por ID
    /// </summary>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Elimina (desactiva) una entidad por Document ID
    /// </summary>
    Task DeleteByDocumentIdAsync(string documentId);
    
    /// <summary>
    /// Elimina permanentemente una entidad (hard delete)
    /// </summary>
    Task HardDeleteAsync(string documentId);
    
    /// <summary>
    /// No aplica en Firestore (las operaciones son atómicas)
    /// Se mantiene por compatibilidad con IRepository
    /// </summary>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// Obtiene el Document ID de Firestore para una entidad
    /// </summary>
    string? GetDocumentId(T entity);
    
    /// <summary>
    /// Establece el Document ID de Firestore para una entidad
    /// </summary>
    void SetDocumentId(T entity, string documentId);
}

/// <summary>
/// Extensión de EntidadBase para almacenar el Document ID de Firestore.
/// Este campo NO se persiste en Firestore, solo se usa en memoria.
/// </summary>
public static class FirestoreEntityExtensions
{
    private static readonly Dictionary<object, string> _documentIds = new();
    
    /// <summary>
    /// Obtiene el Document ID de Firestore asociado a esta entidad
    /// </summary>
    public static string? GetFirestoreDocumentId(this EntidadBase entity)
    {
        return _documentIds.TryGetValue(entity, out var id) ? id : null;
    }
    
    /// <summary>
    /// Establece el Document ID de Firestore para esta entidad
    /// </summary>
    public static void SetFirestoreDocumentId(this EntidadBase entity, string documentId)
    {
        _documentIds[entity] = documentId;
    }
    
    /// <summary>
    /// Limpia el Document ID de memoria
    /// </summary>
    public static void ClearFirestoreDocumentId(this EntidadBase entity)
    {
        _documentIds.Remove(entity);
    }
}
