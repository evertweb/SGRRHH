using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Proyectos para Firestore.
/// Colección: "proyectos"
/// </summary>
public class ProyectoFirestoreRepository : FirestoreRepository<Proyecto>, IProyectoRepository
{
    private const string COLLECTION_NAME = "proyectos";
    private const string CODE_PREFIX = "PRY-";
    
    public ProyectoFirestoreRepository(FirebaseInitializer firebase, ILogger<ProyectoFirestoreRepository>? logger = null) 
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(Proyecto entity)
    {
        var doc = base.EntityToDocument(entity);
        doc["codigo"] = entity.Codigo;
        doc["nombre"] = entity.Nombre;
        doc["descripcion"] = entity.Descripcion;
        doc["cliente"] = entity.Cliente;
        doc["fechaInicio"] = entity.FechaInicio.HasValue 
            ? Timestamp.FromDateTime(entity.FechaInicio.Value.ToUniversalTime()) 
            : null;
        doc["fechaFin"] = entity.FechaFin.HasValue 
            ? Timestamp.FromDateTime(entity.FechaFin.Value.ToUniversalTime()) 
            : null;
        doc["estado"] = (int)entity.Estado;
        doc["estadoNombre"] = entity.Estado.ToString();
        // Campo para búsquedas case-insensitive
        doc["nombreLower"] = entity.Nombre?.ToLowerInvariant();
        doc["clienteLower"] = entity.Cliente?.ToLowerInvariant();
        return doc;
    }
    
    protected override Proyecto DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        if (document.TryGetValue<string>("codigo", out var codigo))
            entity.Codigo = codigo;
        
        if (document.TryGetValue<string>("nombre", out var nombre))
            entity.Nombre = nombre;
        
        if (document.TryGetValue<string>("descripcion", out var descripcion))
            entity.Descripcion = descripcion;
        
        if (document.TryGetValue<string>("cliente", out var cliente))
            entity.Cliente = cliente;
        
        if (document.TryGetValue<Timestamp?>("fechaInicio", out var fechaInicio) && fechaInicio.HasValue)
            entity.FechaInicio = fechaInicio.Value.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp?>("fechaFin", out var fechaFin) && fechaFin.HasValue)
            entity.FechaFin = fechaFin.Value.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<int>("estado", out var estado))
            entity.Estado = (EstadoProyecto)estado;
        
        return entity;
    }
    
    #endregion
    
    #region IProyectoRepository Implementation
    
    /// <summary>
    /// Obtiene todos los proyectos activos ordenados por nombre
    /// </summary>
    public override async Task<IEnumerable<Proyecto>> GetAllActiveAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(p => p.Nombre)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener proyectos activos");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene proyectos por estado
    /// </summary>
    public async Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("activo", true)
                .WhereEqualTo("estado", (int)estado);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(p => p.Nombre)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener proyectos por estado: {Estado}", estado);
            throw;
        }
    }
    
    /// <summary>
    /// Busca proyectos por término
    /// </summary>
    public async Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm)
    {
        try
        {
            var term = searchTerm.ToLowerInvariant().Trim();
            
            // Firestore no tiene búsqueda full-text nativa
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(p => 
                    (p.Codigo?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Nombre?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Cliente?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Descripcion?.ToLowerInvariant().Contains(term) ?? false))
                .OrderBy(p => p.Nombre)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al buscar proyectos con término: {SearchTerm}", searchTerm);
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si existe un proyecto con el código especificado
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
    /// Obtiene el siguiente código disponible (PRY-0001, PRY-0002, etc.)
    /// </summary>
    public async Task<string> GetNextCodigoAsync()
    {
        try
        {
            var snapshot = await Collection.GetSnapshotAsync();
            
            int maxNumber = 0;
            foreach (var doc in snapshot.Documents)
            {
                if (doc.TryGetValue<string>("codigo", out var codigo) && 
                    codigo.StartsWith(CODE_PREFIX))
                {
                    var numStr = codigo.Replace(CODE_PREFIX, "");
                    if (int.TryParse(numStr, out int num) && num > maxNumber)
                    {
                        maxNumber = num;
                    }
                }
            }
            
            return $"{CODE_PREFIX}{(maxNumber + 1):D4}";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener siguiente código de proyecto");
            return $"{CODE_PREFIX}0001";
        }
    }
    
    #endregion
}
