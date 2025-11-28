using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Cargos para Firestore.
/// Colección: "cargos"
/// </summary>
public class CargoFirestoreRepository : FirestoreRepository<Cargo>, ICargoRepository
{
    private const string COLLECTION_NAME = "cargos";
    private const string CODE_PREFIX = "CAR";
    
    public CargoFirestoreRepository(FirebaseInitializer firebase, ILogger<CargoFirestoreRepository>? logger = null) 
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(Cargo entity)
    {
        var doc = base.EntityToDocument(entity);
        doc["codigo"] = entity.Codigo;
        doc["nombre"] = entity.Nombre;
        doc["descripcion"] = entity.Descripcion;
        doc["nivel"] = entity.Nivel;
        doc["departamentoId"] = entity.DepartamentoId;
        // Desnormalizar nombre del departamento para consultas
        doc["departamentoNombre"] = entity.Departamento?.Nombre;
        return doc;
    }
    
    protected override Cargo DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        if (document.TryGetValue<string>("codigo", out var codigo))
            entity.Codigo = codigo;
        
        if (document.TryGetValue<string>("nombre", out var nombre))
            entity.Nombre = nombre;
        
        if (document.TryGetValue<string>("descripcion", out var descripcion))
            entity.Descripcion = descripcion;
        
        if (document.TryGetValue<int>("nivel", out var nivel))
            entity.Nivel = nivel;
        
        if (document.TryGetValue<int?>("departamentoId", out var departamentoId))
            entity.DepartamentoId = departamentoId;
        
        // Reconstruir referencia al departamento con el nombre desnormalizado
        if (document.TryGetValue<string>("departamentoNombre", out var depNombre) && entity.DepartamentoId.HasValue)
        {
            entity.Departamento = new Departamento
            {
                Id = entity.DepartamentoId.Value,
                Nombre = depNombre ?? string.Empty
            };
        }
        
        return entity;
    }
    
    #endregion
    
    #region ICargoRepository Implementation
    
    /// <summary>
    /// Obtiene un cargo por su código
    /// </summary>
    public async Task<Cargo?> GetByCodigoAsync(string codigo)
    {
        try
        {
            var query = Collection.WhereEqualTo("codigo", codigo).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            if (doc == null) return null;
            
            var cargo = DocumentToEntity(doc);
            
            // Cargar departamento completo si existe
            if (cargo.DepartamentoId.HasValue)
            {
                cargo.Departamento = await GetDepartamentoAsync(cargo.DepartamentoId.Value);
            }
            
            return cargo;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener cargo por código: {Codigo}", codigo);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un cargo con su departamento
    /// </summary>
    public async Task<Cargo?> GetByIdWithDepartamentoAsync(int id)
    {
        try
        {
            var cargo = await GetByIdAsync(id);
            if (cargo == null) return null;
            
            if (cargo.DepartamentoId.HasValue)
            {
                cargo.Departamento = await GetDepartamentoAsync(cargo.DepartamentoId.Value);
            }
            
            return cargo;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener cargo {Id} con departamento", id);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un cargo con sus empleados
    /// </summary>
    public async Task<Cargo?> GetByIdWithEmpleadosAsync(int id)
    {
        try
        {
            var cargo = await GetByIdWithDepartamentoAsync(id);
            if (cargo == null) return null;
            
            // Consultar empleados con este cargo
            var empleadosRef = Firestore.Collection("empleados");
            var empleadosQuery = empleadosRef
                .WhereEqualTo("cargoId", id)
                .WhereEqualTo("activo", true);
            var empleadosSnapshot = await empleadosQuery.GetSnapshotAsync();
            
            cargo.Empleados = empleadosSnapshot.Documents.Select(doc =>
            {
                var emp = new Empleado { Id = doc.GetValue<int>("id") };
                if (doc.TryGetValue<string>("nombres", out var nombres))
                    emp.Nombres = nombres;
                if (doc.TryGetValue<string>("apellidos", out var apellidos))
                    emp.Apellidos = apellidos;
                emp.SetFirestoreDocumentId(doc.Id);
                return emp;
            }).ToList();
            
            return cargo;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener cargo {Id} con empleados", id);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los cargos con su departamento
    /// </summary>
    public async Task<IEnumerable<Cargo>> GetAllWithDepartamentoAsync()
    {
        try
        {
            var snapshot = await Collection.GetSnapshotAsync();
            var cargos = snapshot.Documents.Select(DocumentToEntity).ToList();
            
            // Los departamentos ya vienen desnormalizados en el documento
            // Si se necesita información completa, cargarlos
            var departamentosIds = cargos
                .Where(c => c.DepartamentoId.HasValue)
                .Select(c => c.DepartamentoId!.Value)
                .Distinct()
                .ToList();
            
            var departamentos = await GetDepartamentosAsync(departamentosIds);
            var depDict = departamentos.ToDictionary(d => d.Id);
            
            foreach (var cargo in cargos.Where(c => c.DepartamentoId.HasValue))
            {
                if (depDict.TryGetValue(cargo.DepartamentoId!.Value, out var dep))
                {
                    cargo.Departamento = dep;
                }
            }
            
            return cargos
                .OrderBy(c => c.Departamento?.Nombre ?? "")
                .ThenBy(c => c.Nivel)
                .ThenBy(c => c.Nombre)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener todos los cargos con departamento");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los cargos activos con su departamento
    /// </summary>
    public async Task<IEnumerable<Cargo>> GetAllActiveWithDepartamentoAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            var cargos = snapshot.Documents.Select(DocumentToEntity).ToList();
            
            var departamentosIds = cargos
                .Where(c => c.DepartamentoId.HasValue)
                .Select(c => c.DepartamentoId!.Value)
                .Distinct()
                .ToList();
            
            var departamentos = await GetDepartamentosAsync(departamentosIds);
            var depDict = departamentos.ToDictionary(d => d.Id);
            
            foreach (var cargo in cargos.Where(c => c.DepartamentoId.HasValue))
            {
                if (depDict.TryGetValue(cargo.DepartamentoId!.Value, out var dep))
                {
                    cargo.Departamento = dep;
                }
            }
            
            return cargos
                .OrderBy(c => c.Departamento?.Nombre ?? "")
                .ThenBy(c => c.Nivel)
                .ThenBy(c => c.Nombre)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener cargos activos con departamento");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene cargos por departamento
    /// </summary>
    public async Task<IEnumerable<Cargo>> GetByDepartamentoAsync(int departamentoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("departamentoId", departamentoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(c => c.Nivel)
                .ThenBy(c => c.Nombre)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener cargos del departamento {DepartamentoId}", departamentoId);
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si existe un cargo con el código dado
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
    /// Obtiene el siguiente código de cargo disponible
    /// </summary>
    public async Task<string> GetNextCodigoAsync()
    {
        try
        {
            var query = Collection.OrderByDescending("codigo").Limit(10);
            var snapshot = await query.GetSnapshotAsync();
            
            int maxNumber = 0;
            foreach (var doc in snapshot.Documents)
            {
                if (doc.TryGetValue<string>("codigo", out var codigo) && 
                    codigo.StartsWith(CODE_PREFIX))
                {
                    if (int.TryParse(codigo.Replace(CODE_PREFIX, ""), out int num))
                    {
                        if (num > maxNumber) maxNumber = num;
                    }
                }
            }
            
            return $"{CODE_PREFIX}{(maxNumber + 1):D3}";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener siguiente código de cargo");
            return $"{CODE_PREFIX}001";
        }
    }
    
    /// <summary>
    /// Verifica si el cargo tiene empleados asignados
    /// </summary>
    public async Task<bool> HasEmpleadosAsync(int id)
    {
        try
        {
            var empleadosRef = Firestore.Collection("empleados");
            var query = empleadosRef
                .WhereEqualTo("cargoId", id)
                .WhereEqualTo("activo", true)
                .Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents.Any();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar empleados del cargo {Id}", id);
            throw;
        }
    }
    
    /// <summary>
    /// Cuenta el total de cargos activos
    /// </summary>
    public new async Task<int> CountActiveAsync()
    {
        return await base.CountActiveAsync();
    }
    
    /// <summary>
    /// Obtiene todos los cargos activos ordenados
    /// </summary>
    public override async Task<IEnumerable<Cargo>> GetAllActiveAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            var cargos = snapshot.Documents.Select(DocumentToEntity).ToList();
            
            // Cargar departamentos
            var departamentosIds = cargos
                .Where(c => c.DepartamentoId.HasValue)
                .Select(c => c.DepartamentoId!.Value)
                .Distinct()
                .ToList();
            
            var departamentos = await GetDepartamentosAsync(departamentosIds);
            var depDict = departamentos.ToDictionary(d => d.Id);
            
            foreach (var cargo in cargos.Where(c => c.DepartamentoId.HasValue))
            {
                if (depDict.TryGetValue(cargo.DepartamentoId!.Value, out var dep))
                {
                    cargo.Departamento = dep;
                }
            }
            
            return cargos.OrderBy(c => c.Nombre).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener cargos activos");
            throw;
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Obtiene un departamento por Id
    /// </summary>
    private async Task<Departamento?> GetDepartamentoAsync(int id)
    {
        try
        {
            var depRef = Firestore.Collection("departamentos");
            var query = depRef.WhereEqualTo("id", id).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            if (doc == null) return null;
            
            var dep = new Departamento { Id = doc.GetValue<int>("id") };
            if (doc.TryGetValue<string>("nombre", out var nombre))
                dep.Nombre = nombre;
            if (doc.TryGetValue<string>("codigo", out var codigo))
                dep.Codigo = codigo;
            dep.SetFirestoreDocumentId(doc.Id);
            
            return dep;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Obtiene múltiples departamentos por sus Ids
    /// </summary>
    private async Task<List<Departamento>> GetDepartamentosAsync(List<int> ids)
    {
        var departamentos = new List<Departamento>();
        if (!ids.Any()) return departamentos;
        
        try
        {
            // Firestore tiene límite de 30 elementos en WhereIn
            // Dividir en chunks si es necesario
            var chunks = ids.Chunk(30);
            
            foreach (var chunk in chunks)
            {
                var depRef = Firestore.Collection("departamentos");
                var query = depRef.WhereIn("id", chunk.Cast<object>().ToList());
                var snapshot = await query.GetSnapshotAsync();
                
                foreach (var doc in snapshot.Documents)
                {
                    var dep = new Departamento { Id = doc.GetValue<int>("id") };
                    if (doc.TryGetValue<string>("nombre", out var nombre))
                        dep.Nombre = nombre;
                    if (doc.TryGetValue<string>("codigo", out var codigo))
                        dep.Codigo = codigo;
                    dep.SetFirestoreDocumentId(doc.Id);
                    departamentos.Add(dep);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener departamentos por IDs");
        }
        
        return departamentos;
    }
    
    #endregion
}
