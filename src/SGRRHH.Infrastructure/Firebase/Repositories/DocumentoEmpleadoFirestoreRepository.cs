using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de DocumentoEmpleado para Firestore.
/// Colección: "documentos_empleado"
/// 
/// Campos desnormalizados:
/// - empleadoNombre, empleadoCodigo
/// </summary>
public class DocumentoEmpleadoFirestoreRepository : FirestoreRepository<DocumentoEmpleado>, IDocumentoEmpleadoRepository
{
    private const string COLLECTION_NAME = "documentos_empleado";
    
    public DocumentoEmpleadoFirestoreRepository(FirebaseInitializer firebase, ILogger<DocumentoEmpleadoFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(DocumentoEmpleado entity)
    {
        var doc = base.EntityToDocument(entity);
        
        // Empleado - con datos desnormalizados
        doc["empleadoId"] = entity.EmpleadoId;
        doc["empleadoNombre"] = entity.Empleado?.NombreCompleto;
        doc["empleadoCodigo"] = entity.Empleado?.Codigo;
        
        // Tipo de documento
        doc["tipoDocumento"] = entity.TipoDocumento.ToString();
        doc["tipoDocumentoInt"] = (int)entity.TipoDocumento;
        
        // Información del documento
        doc["nombre"] = entity.Nombre;
        doc["descripcion"] = entity.Descripcion;
        doc["archivoPath"] = entity.ArchivoPath;
        doc["nombreArchivoOriginal"] = entity.NombreArchivoOriginal;
        doc["tamanoArchivo"] = entity.TamanoArchivo;
        doc["tipoMime"] = entity.TipoMime;
        
        // Fechas
        doc["fechaVencimiento"] = entity.FechaVencimiento.HasValue
            ? Timestamp.FromDateTime(entity.FechaVencimiento.Value.ToUniversalTime())
            : null;
        doc["fechaEmision"] = entity.FechaEmision.HasValue
            ? Timestamp.FromDateTime(entity.FechaEmision.Value.ToUniversalTime())
            : null;
        
        // Usuario que subió
        doc["subidoPorUsuarioId"] = entity.SubidoPorUsuarioId;
        doc["subidoPorNombre"] = entity.SubidoPorNombre;
        
        return doc;
    }
    
    protected override DocumentoEmpleado DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        // Empleado
        if (document.TryGetValue<int>("empleadoId", out var empleadoId))
        {
            entity.EmpleadoId = empleadoId;
            entity.Empleado = new Empleado { Id = empleadoId };
            
            if (document.TryGetValue<string>("empleadoNombre", out var empNombre) && !string.IsNullOrEmpty(empNombre))
            {
                var partes = empNombre.Split(' ', 2);
                entity.Empleado.Nombres = partes.Length > 0 ? partes[0] : string.Empty;
                entity.Empleado.Apellidos = partes.Length > 1 ? partes[1] : string.Empty;
            }
            
            if (document.TryGetValue<string>("empleadoCodigo", out var empCodigo))
                entity.Empleado.Codigo = empCodigo ?? string.Empty;
        }
        
        // Tipo de documento
        if (document.TryGetValue<string>("tipoDocumento", out var tipoStr) && !string.IsNullOrEmpty(tipoStr))
        {
            if (Enum.TryParse<TipoDocumentoEmpleado>(tipoStr, out var tipo))
                entity.TipoDocumento = tipo;
        }
        
        // Información del documento
        if (document.TryGetValue<string>("nombre", out var nombre))
            entity.Nombre = nombre ?? string.Empty;
        
        if (document.TryGetValue<string>("descripcion", out var descripcion))
            entity.Descripcion = descripcion;
        
        if (document.TryGetValue<string>("archivoPath", out var archivoPath))
            entity.ArchivoPath = archivoPath ?? string.Empty;
        
        if (document.TryGetValue<string>("nombreArchivoOriginal", out var nombreOriginal))
            entity.NombreArchivoOriginal = nombreOriginal ?? string.Empty;
        
        if (document.TryGetValue<long>("tamanoArchivo", out var tamano))
            entity.TamanoArchivo = tamano;
        
        if (document.TryGetValue<string>("tipoMime", out var tipoMime))
            entity.TipoMime = tipoMime ?? string.Empty;
        
        // Fechas
        if (document.TryGetValue<Timestamp?>("fechaVencimiento", out var fechaVenc) && fechaVenc.HasValue)
            entity.FechaVencimiento = fechaVenc.Value.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp?>("fechaEmision", out var fechaEmision) && fechaEmision.HasValue)
            entity.FechaEmision = fechaEmision.Value.ToDateTime().ToLocalTime();
        
        // Usuario que subió
        if (document.TryGetValue<int?>("subidoPorUsuarioId", out var subidoPorId))
            entity.SubidoPorUsuarioId = subidoPorId;
        
        if (document.TryGetValue<string>("subidoPorNombre", out var subidoPorNombre))
            entity.SubidoPorNombre = subidoPorNombre;
        
        return entity;
    }
    
    #endregion
    
    #region IDocumentoEmpleadoRepository Implementation
    
    /// <summary>
    /// Obtiene todos los documentos de un empleado
    /// </summary>
    public async Task<IEnumerable<DocumentoEmpleado>> GetByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(d => d.TipoDocumento)
                .ThenByDescending(d => d.FechaCreacion)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener documentos del empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene documentos de un empleado por tipo
    /// </summary>
    public async Task<IEnumerable<DocumentoEmpleado>> GetByEmpleadoIdAndTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("tipoDocumento", tipo.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(d => d.FechaCreacion)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener documentos del empleado {EmpleadoId} tipo {Tipo}", empleadoId, tipo);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene documentos próximos a vencer
    /// </summary>
    public async Task<IEnumerable<DocumentoEmpleado>> GetDocumentosProximosAVencerAsync(int diasAnticipacion)
    {
        try
        {
            var fechaLimite = DateTime.Today.AddDays(diasAnticipacion);
            
            // Obtener todos los documentos activos que tienen fecha de vencimiento
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(d => d.FechaVencimiento.HasValue && 
                           d.FechaVencimiento.Value >= DateTime.Today && 
                           d.FechaVencimiento.Value <= fechaLimite)
                .OrderBy(d => d.FechaVencimiento)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener documentos próximos a vencer");
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si un empleado tiene un tipo específico de documento
    /// </summary>
    public async Task<bool> EmpleadoTieneDocumentoTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("tipoDocumento", tipo.ToString())
                .WhereEqualTo("activo", true)
                .Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents.Any();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar documento tipo {Tipo} del empleado {EmpleadoId}", tipo, empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene el conteo de documentos por empleado
    /// </summary>
    public async Task<int> GetConteoDocumentosByEmpleadoAsync(int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Count;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al contar documentos del empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los documentos activos ordenados
    /// </summary>
    public override async Task<IEnumerable<DocumentoEmpleado>> GetAllActiveAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(d => d.FechaCreacion)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener documentos activos");
            throw;
        }
    }
    
    #endregion
    
    #region Helper Methods - Actualizar datos desnormalizados
    
    /// <summary>
    /// Actualiza el nombre del empleado en todos sus documentos.
    /// Llamar cuando se cambie el nombre de un empleado.
    /// </summary>
    public async Task ActualizarEmpleadoNombreAsync(int empleadoId, string nuevoNombre)
    {
        try
        {
            var query = Collection.WhereEqualTo("empleadoId", empleadoId);
            var snapshot = await query.GetSnapshotAsync();
            
            var batch = CreateBatch();
            foreach (var doc in snapshot.Documents)
            {
                batch.Update(doc.Reference, new Dictionary<string, object>
                {
                    ["empleadoNombre"] = nuevoNombre,
                    ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                });
            }
            
            await batch.CommitAsync();
            _logger?.LogInformation("Actualizado empleadoNombre en {Count} documentos", snapshot.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar empleadoNombre en documentos para {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    #endregion
}
