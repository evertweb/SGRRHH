using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Departamentos para Firestore.
/// Colección: "departamentos"
/// Incluye cache en memoria para reducir round-trips.
/// </summary>
public class DepartamentoFirestoreRepository : FirestoreRepository<Departamento>, IDepartamentoRepository
{
    private const string COLLECTION_NAME = "departamentos";
    private const string CODE_PREFIX = "DEP";
    private const string CACHE_KEY_ALL_ACTIVE = "departamentos_all_active";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    private readonly ICacheService? _cache;

    public DepartamentoFirestoreRepository(
        FirebaseInitializer firebase,
        ICacheService? cache = null,
        ILogger<DepartamentoFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
        _cache = cache;
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(Departamento entity)
    {
        var doc = base.EntityToDocument(entity);
        doc["codigo"] = entity.Codigo;
        doc["nombre"] = entity.Nombre;
        doc["descripcion"] = entity.Descripcion;
        doc["jefeId"] = entity.JefeId;
        // Nota: No guardamos las colecciones de navegación (Empleados, Cargos)
        // Esas relaciones se manejan por referencia en Firestore
        return doc;
    }
    
    protected override Departamento DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        if (document.TryGetValue<string>("codigo", out var codigo))
            entity.Codigo = codigo;
        
        if (document.TryGetValue<string>("nombre", out var nombre))
            entity.Nombre = nombre;
        
        if (document.TryGetValue<string>("descripcion", out var descripcion))
            entity.Descripcion = descripcion;
        
        if (document.TryGetValue<int?>("jefeId", out var jefeId))
            entity.JefeId = jefeId;
        
        return entity;
    }
    
    #endregion
    
    #region IDepartamentoRepository Implementation
    
    /// <summary>
    /// Obtiene un departamento por su código
    /// </summary>
    public async Task<Departamento?> GetByCodigoAsync(string codigo)
    {
        try
        {
            var query = Collection.WhereEqualTo("codigo", codigo).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            return doc != null ? DocumentToEntity(doc) : null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener departamento por código: {Codigo}", codigo);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un departamento con sus empleados.
    /// Nota: En Firestore hacemos una segunda consulta a la colección empleados.
    /// </summary>
    public async Task<Departamento?> GetByIdWithEmpleadosAsync(int id)
    {
        try
        {
            var departamento = await GetByIdAsync(id);
            if (departamento == null) return null;
            
            // Consultar empleados de este departamento
            var empleadosRef = Firestore.Collection("empleados");
            var empleadosQuery = empleadosRef
                .WhereEqualTo("departamentoId", id)
                .WhereEqualTo("activo", true);
            var empleadosSnapshot = await empleadosQuery.GetSnapshotAsync();
            
            // Crear lista de empleados básicos (sin todos los datos)
            departamento.Empleados = empleadosSnapshot.Documents.Select(doc =>
            {
                var emp = new Empleado { Id = doc.GetValue<int>("id") };
                if (doc.TryGetValue<string>("nombres", out var nombres))
                    emp.Nombres = nombres;
                if (doc.TryGetValue<string>("apellidos", out var apellidos))
                    emp.Apellidos = apellidos;
                emp.SetFirestoreDocumentId(doc.Id);
                return emp;
            }).ToList();
            
            return departamento;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener departamento {Id} con empleados", id);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un departamento con sus cargos.
    /// Nota: En Firestore hacemos una segunda consulta a la colección cargos.
    /// </summary>
    public async Task<Departamento?> GetByIdWithCargosAsync(int id)
    {
        try
        {
            var departamento = await GetByIdAsync(id);
            if (departamento == null) return null;
            
            // Consultar cargos de este departamento
            var cargosRef = Firestore.Collection("cargos");
            var cargosQuery = cargosRef
                .WhereEqualTo("departamentoId", id)
                .WhereEqualTo("activo", true);
            var cargosSnapshot = await cargosQuery.GetSnapshotAsync();
            
            departamento.Cargos = cargosSnapshot.Documents.Select(doc =>
            {
                var cargo = new Cargo { Id = doc.GetValue<int>("id") };
                if (doc.TryGetValue<string>("nombre", out var nombre))
                    cargo.Nombre = nombre;
                if (doc.TryGetValue<string>("codigo", out var codigo))
                    cargo.Codigo = codigo;
                cargo.SetFirestoreDocumentId(doc.Id);
                return cargo;
            }).ToList();
            
            return departamento;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener departamento {Id} con cargos", id);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los departamentos activos con conteo de empleados
    /// </summary>
    public async Task<IEnumerable<Departamento>> GetAllWithEmpleadosCountAsync()
    {
        try
        {
            // Obtener departamentos activos ordenados por nombre
            var query = Collection.WhereEqualTo("activo", true).OrderBy("nombre");
            var snapshot = await query.GetSnapshotAsync();
            var departamentos = snapshot.Documents.Select(DocumentToEntity).ToList();
            
            // Para cada departamento, contar sus empleados activos
            var empleadosRef = Firestore.Collection("empleados");
            
            foreach (var dep in departamentos)
            {
                var empleadosQuery = empleadosRef
                    .WhereEqualTo("departamentoId", dep.Id)
                    .WhereEqualTo("activo", true);
                var empleadosSnapshot = await empleadosQuery.GetSnapshotAsync();
                
                // Llenar la colección con entidades vacías solo para el conteo
                dep.Empleados = Enumerable.Range(0, empleadosSnapshot.Count)
                    .Select(_ => new Empleado())
                    .ToList();
            }
            
            return departamentos;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener departamentos con conteo de empleados");
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si existe un departamento con el código dado
    /// </summary>
    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        try
        {
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
    /// Obtiene el siguiente código de departamento disponible (DEP001, DEP002, etc.)
    /// </summary>
    public async Task<string> GetNextCodigoAsync()
    {
        return await GetNextCodigoAsync(CODE_PREFIX);
    }
    
    /// <summary>
    /// Verifica si el departamento tiene empleados asignados
    /// </summary>
    public async Task<bool> HasEmpleadosAsync(int id)
    {
        try
        {
            var empleadosRef = Firestore.Collection("empleados");
            var query = empleadosRef
                .WhereEqualTo("departamentoId", id)
                .WhereEqualTo("activo", true)
                .Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents.Any();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar empleados del departamento {Id}", id);
            throw;
        }
    }
    
    /// <summary>
    /// Cuenta el total de departamentos activos
    /// </summary>
    public new async Task<int> CountActiveAsync()
    {
        return await base.CountActiveAsync();
    }
    
    /// <summary>
    /// Obtiene todos los departamentos activos ordenados por nombre.
    /// Usa cache en memoria con expiración de 10 minutos.
    /// </summary>
    public override async Task<IEnumerable<Departamento>> GetAllActiveAsync()
    {
        try
        {
            // Intentar obtener del cache primero
            if (_cache != null)
            {
                return await _cache.GetOrCreateAsync(
                    CACHE_KEY_ALL_ACTIVE,
                    async () => await FetchAllActiveFromFirestoreAsync(),
                    CacheExpiration);
            }

            // Sin cache, ir directo a Firestore
            return await FetchAllActiveFromFirestoreAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener departamentos activos");
            throw;
        }
    }

    private async Task<IEnumerable<Departamento>> FetchAllActiveFromFirestoreAsync()
    {
        var query = Collection
            .WhereEqualTo("activo", true)
            .OrderBy("nombre");
        var snapshot = await query.GetSnapshotAsync();

        return snapshot.Documents.Select(DocumentToEntity).ToList();
    }

    /// <summary>
    /// Invalida el cache de departamentos. Llamar después de Add/Update/Delete.
    /// </summary>
    public void InvalidateCache()
    {
        _cache?.InvalidateByPrefix("departamentos");
    }
    
    #endregion
}
