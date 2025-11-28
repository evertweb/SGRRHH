using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Usuarios para Firestore.
/// Colección: "users" (sincronizada con Firebase Auth)
/// 
/// El Document ID es el UID de Firebase Auth del usuario.
/// </summary>
public class UsuarioFirestoreRepository : FirestoreRepository<Usuario>, IUsuarioRepository
{
    private const string COLLECTION_NAME = "users";
    
    public UsuarioFirestoreRepository(FirebaseInitializer firebase, ILogger<UsuarioFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(Usuario entity)
    {
        var doc = base.EntityToDocument(entity);
        
        doc["username"] = entity.Username;
        doc["nombreCompleto"] = entity.NombreCompleto;
        doc["email"] = entity.Email;
        doc["rol"] = entity.Rol.ToString();
        
        // Referencias a empleado
        doc["empleadoId"] = entity.EmpleadoId;
        doc["empleadoFirestoreId"] = entity.EmpleadoFirestoreId;
        
        // Firebase Auth UID
        doc["firebaseUid"] = entity.FirebaseUid;
        
        // Último acceso
        doc["ultimoAcceso"] = entity.UltimoAcceso.HasValue
            ? Timestamp.FromDateTime(entity.UltimoAcceso.Value.ToUniversalTime())
            : null;
        
        // No guardamos el PasswordHash en Firestore - está en Firebase Auth
        
        return doc;
    }
    
    protected override Usuario DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        if (document.TryGetValue<string>("username", out var username))
            entity.Username = username ?? string.Empty;
        
        if (document.TryGetValue<string>("nombreCompleto", out var nombreCompleto))
            entity.NombreCompleto = nombreCompleto ?? string.Empty;
        
        if (document.TryGetValue<string>("email", out var email))
            entity.Email = email;
        
        if (document.TryGetValue<string>("rol", out var rolStr) && !string.IsNullOrEmpty(rolStr))
            if (Enum.TryParse<RolUsuario>(rolStr, out var rol))
                entity.Rol = rol;
        
        if (document.TryGetValue<int?>("empleadoId", out var empleadoId))
            entity.EmpleadoId = empleadoId;
        
        if (document.TryGetValue<string>("empleadoFirestoreId", out var empFirestoreId))
            entity.EmpleadoFirestoreId = empFirestoreId;
        
        // El FirebaseUid es el Document ID, pero también puede estar como campo
        // Priorizar el Document ID ya que es más confiable
        if (document.TryGetValue<string>("firebaseUid", out var firebaseUid) && !string.IsNullOrEmpty(firebaseUid))
            entity.FirebaseUid = firebaseUid;
        else
            entity.FirebaseUid = document.Id; // Usar Document ID como fallback (más confiable)
        
        if (document.TryGetValue<Timestamp?>("ultimoAcceso", out var ultimoAcceso) && ultimoAcceso.HasValue)
            entity.UltimoAcceso = ultimoAcceso.Value.ToDateTime().ToLocalTime();
        
        // El PasswordHash no se almacena en Firestore
        entity.PasswordHash = string.Empty;
        
        return entity;
    }
    
    #endregion
    
    #region IUsuarioRepository Implementation
    
    /// <summary>
    /// Obtiene un usuario por su nombre de usuario
    /// </summary>
    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        try
        {
            var query = Collection.WhereEqualTo("username", username).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            return doc == null ? null : DocumentToEntity(doc);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuario por username: {Username}", username);
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si existe un usuario con el nombre de usuario especificado
    /// </summary>
    public async Task<bool> ExistsUsernameAsync(string username)
    {
        try
        {
            var query = Collection.WhereEqualTo("username", username).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents.Any();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar existencia de username: {Username}", username);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un usuario por su Firebase UID (que también es el Document ID)
    /// </summary>
    public async Task<Usuario?> GetByFirebaseUidAsync(string firebaseUid)
    {
        try
        {
            // El Document ID es el Firebase UID
            return await GetByDocumentIdAsync(firebaseUid);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuario por Firebase UID: {FirebaseUid}", firebaseUid);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un usuario por su email
    /// </summary>
    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        try
        {
            var query = Collection.WhereEqualTo("email", email).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            return doc == null ? null : DocumentToEntity(doc);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuario por email: {Email}", email);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene usuarios por rol
    /// </summary>
    public async Task<IEnumerable<Usuario>> GetByRolAsync(RolUsuario rol)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("rol", rol.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(u => u.NombreCompleto)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuarios con rol: {Rol}", rol);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza el último acceso del usuario
    /// </summary>
    public async Task UpdateUltimoAccesoAsync(string firebaseUid, DateTime ultimoAcceso)
    {
        try
        {
            var docRef = Collection.Document(firebaseUid);
            await docRef.UpdateAsync(new Dictionary<string, object>
            {
                ["ultimoAcceso"] = Timestamp.FromDateTime(ultimoAcceso.ToUniversalTime()),
                ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
            });
            
            _logger?.LogInformation("Actualizado último acceso para usuario {FirebaseUid}", firebaseUid);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar último acceso para {FirebaseUid}", firebaseUid);
            throw;
        }
    }
    
    /// <summary>
    /// Crea o actualiza un usuario usando su Firebase UID como Document ID
    /// </summary>
    public async Task<Usuario> UpsertByFirebaseUidAsync(Usuario entity)
    {
        try
        {
            if (string.IsNullOrEmpty(entity.FirebaseUid))
                throw new ArgumentException("FirebaseUid es requerido para crear/actualizar usuario");
            
            entity.FechaModificacion = DateTime.Now;
            
            // Verificar si existe
            var existing = await GetByDocumentIdAsync(entity.FirebaseUid);
            if (existing == null)
            {
                entity.FechaCreacion = DateTime.Now;
                entity.Activo = true;
            }
            
            var data = EntityToDocument(entity);
            var docRef = Collection.Document(entity.FirebaseUid);
            await docRef.SetAsync(data, SetOptions.MergeAll);
            
            entity.SetFirestoreDocumentId(entity.FirebaseUid);
            
            _logger?.LogInformation("Usuario upserted con Firebase UID: {FirebaseUid}", entity.FirebaseUid);
            
            return entity;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al upsert usuario con Firebase UID: {FirebaseUid}", entity.FirebaseUid);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los usuarios activos ordenados por nombre
    /// </summary>
    public override async Task<IEnumerable<Usuario>> GetAllActiveAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(u => u.NombreCompleto)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuarios activos");
            throw;
        }
    }
    
    #endregion
}
