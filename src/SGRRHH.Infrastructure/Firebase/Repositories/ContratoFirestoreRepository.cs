using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Contratos para Firestore.
/// Colección: "contratos"
/// 
/// Campos desnormalizados:
/// - empleadoNombre, empleadoCodigo, cargoNombre
/// </summary>
public class ContratoFirestoreRepository : FirestoreRepository<Contrato>, IContratoRepository
{
    private const string COLLECTION_NAME = "contratos";
    
    public ContratoFirestoreRepository(FirebaseInitializer firebase, ILogger<ContratoFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(Contrato entity)
    {
        var doc = base.EntityToDocument(entity);
        
        // Empleado - con datos desnormalizados
        doc["empleadoId"] = entity.EmpleadoId;
        doc["empleadoNombre"] = entity.Empleado?.NombreCompleto;
        doc["empleadoCodigo"] = entity.Empleado?.Codigo;
        doc["empleadoDepartamento"] = entity.Empleado?.Departamento?.Nombre;
        
        // Tipo de contrato
        doc["tipoContrato"] = entity.TipoContrato.ToString();
        
        // Fechas
        doc["fechaInicio"] = Timestamp.FromDateTime(entity.FechaInicio.ToUniversalTime());
        doc["fechaFin"] = entity.FechaFin.HasValue
            ? Timestamp.FromDateTime(entity.FechaFin.Value.ToUniversalTime())
            : null;
        
        // Salario
        doc["salario"] = (double)entity.Salario;
        
        // Cargo - con datos desnormalizados
        doc["cargoId"] = entity.CargoId;
        doc["cargoNombre"] = entity.Cargo?.Nombre;
        
        // Estado
        doc["estado"] = entity.Estado.ToString();
        
        // Archivo adjunto - URL de Firebase Storage
        doc["archivoAdjuntoUrl"] = entity.ArchivoAdjuntoPath;
        
        // Observaciones
        doc["observaciones"] = entity.Observaciones;
        
        return doc;
    }
    
    protected override Contrato DocumentToEntity(DocumentSnapshot document)
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
            
            if (document.TryGetValue<string>("empleadoDepartamento", out var empDep))
                entity.Empleado.Departamento = new Departamento { Nombre = empDep ?? string.Empty };
        }
        
        // Tipo de contrato
        if (document.TryGetValue<string>("tipoContrato", out var tipoStr) && !string.IsNullOrEmpty(tipoStr))
            if (Enum.TryParse<TipoContrato>(tipoStr, out var tipo))
                entity.TipoContrato = tipo;
        
        // Fechas
        if (document.TryGetValue<Timestamp>("fechaInicio", out var fechaInicio))
            entity.FechaInicio = fechaInicio.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp?>("fechaFin", out var fechaFin) && fechaFin.HasValue)
            entity.FechaFin = fechaFin.Value.ToDateTime().ToLocalTime();
        
        // Salario
        if (document.TryGetValue<double>("salario", out var salario))
            entity.Salario = (decimal)salario;
        
        // Cargo
        if (document.TryGetValue<int>("cargoId", out var cargoId))
        {
            entity.CargoId = cargoId;
            if (document.TryGetValue<string>("cargoNombre", out var cargoNombre))
            {
                entity.Cargo = new Cargo
                {
                    Id = cargoId,
                    Nombre = cargoNombre ?? string.Empty
                };
            }
        }
        
        // Estado
        if (document.TryGetValue<string>("estado", out var estadoStr) && !string.IsNullOrEmpty(estadoStr))
            if (Enum.TryParse<EstadoContrato>(estadoStr, out var estado))
                entity.Estado = estado;
        
        // Archivo adjunto
        if (document.TryGetValue<string>("archivoAdjuntoUrl", out var archivoUrl))
            entity.ArchivoAdjuntoPath = archivoUrl;
        
        // Observaciones
        if (document.TryGetValue<string>("observaciones", out var obs))
            entity.Observaciones = obs;
        
        return entity;
    }
    
    #endregion
    
    #region IContratoRepository Implementation
    
    /// <summary>
    /// Obtiene los contratos de un empleado específico
    /// </summary>
    public async Task<IEnumerable<Contrato>> GetByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(c => c.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener contratos del empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene el contrato activo de un empleado
    /// </summary>
    public async Task<Contrato?> GetContratoActivoByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("estado", EstadoContrato.Activo.ToString())
                .WhereEqualTo("activo", true)
                .Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            return doc == null ? null : DocumentToEntity(doc);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener contrato activo del empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene contratos próximos a vencer
    /// </summary>
    public async Task<IEnumerable<Contrato>> GetContratosProximosAVencerAsync(int diasAnticipacion)
    {
        try
        {
            var fechaLimite = DateTime.Today.AddDays(diasAnticipacion);
            
            // Obtener solo contratos activos con fecha fin
            var query = Collection
                .WhereEqualTo("estado", EstadoContrato.Activo.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(c => c.FechaFin.HasValue && 
                           c.FechaFin.Value >= DateTime.Today && 
                           c.FechaFin.Value <= fechaLimite)
                .OrderBy(c => c.FechaFin)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener contratos próximos a vencer");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene contratos por rango de fechas de inicio
    /// </summary>
    public async Task<IEnumerable<Contrato>> GetContratosPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var query = Collection
                .WhereGreaterThanOrEqualTo("fechaInicio", Timestamp.FromDateTime(fechaInicio.ToUniversalTime()))
                .WhereLessThanOrEqualTo("fechaInicio", Timestamp.FromDateTime(fechaFin.ToUniversalTime()))
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(c => c.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener contratos por rango de fechas");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene contratos por tipo
    /// </summary>
    public async Task<IEnumerable<Contrato>> GetByTipoContratoAsync(TipoContrato tipoContrato)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("tipoContrato", tipoContrato.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(c => c.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener contratos de tipo {TipoContrato}", tipoContrato);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene contratos por estado
    /// </summary>
    public async Task<IEnumerable<Contrato>> GetByEstadoAsync(EstadoContrato estado)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("estado", estado.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(c => c.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener contratos con estado {Estado}", estado);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene contratos vencidos (fecha fin pasada pero aún marcados como activos)
    /// </summary>
    public async Task<IEnumerable<Contrato>> GetContratosVencidosAsync()
    {
        try
        {
            var query = Collection
                .WhereEqualTo("estado", EstadoContrato.Activo.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(c => c.FechaFin.HasValue && c.FechaFin.Value < DateTime.Today)
                .OrderBy(c => c.FechaFin)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener contratos vencidos");
            throw;
        }
    }
    
    /// <summary>
    /// Cuenta contratos activos por tipo
    /// </summary>
    public async Task<Dictionary<TipoContrato, int>> CountByTipoContratoAsync()
    {
        try
        {
            var query = Collection
                .WhereEqualTo("estado", EstadoContrato.Activo.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            var contratos = snapshot.Documents.Select(DocumentToEntity);
            
            return contratos
                .GroupBy(c => c.TipoContrato)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al contar contratos por tipo");
            throw;
        }
    }
    
    /// <summary>
    /// Finaliza un contrato cambiando su estado
    /// </summary>
    public async Task FinalizarContratoAsync(int contratoId, DateTime? fechaFin = null)
    {
        try
        {
            var contrato = await GetByIdAsync(contratoId);
            if (contrato == null)
                throw new InvalidOperationException($"Contrato con Id {contratoId} no encontrado");
            
            var docId = contrato.GetFirestoreDocumentId();
            if (string.IsNullOrEmpty(docId))
            {
                var query = Collection.WhereEqualTo("id", contratoId).Limit(1);
                var snapshot = await query.GetSnapshotAsync();
                var doc = snapshot.Documents.FirstOrDefault();
                if (doc == null)
                    throw new InvalidOperationException($"Documento de contrato {contratoId} no encontrado");
                docId = doc.Id;
            }
            
            var updates = new Dictionary<string, object>
            {
                ["estado"] = EstadoContrato.Finalizado.ToString(),
                ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            
            if (fechaFin.HasValue)
                updates["fechaFin"] = Timestamp.FromDateTime(fechaFin.Value.ToUniversalTime());
            
            var docRef = Collection.Document(docId);
            await docRef.UpdateAsync(updates);
            
            _logger?.LogInformation("Contrato {ContratoId} finalizado", contratoId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al finalizar contrato {ContratoId}", contratoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los contratos activos ordenados
    /// </summary>
    public override async Task<IEnumerable<Contrato>> GetAllActiveAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(c => c.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener contratos activos");
            throw;
        }
    }
    
    #endregion
    
    #region Helper Methods - Actualizar datos desnormalizados
    
    /// <summary>
    /// Actualiza el nombre del empleado en todos sus contratos.
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
            _logger?.LogInformation("Actualizado empleadoNombre en {Count} contratos", snapshot.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar empleadoNombre en contratos para {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza el nombre del cargo en todos los contratos que lo usan.
    /// Llamar cuando se cambie el nombre de un cargo.
    /// </summary>
    public async Task ActualizarCargoNombreAsync(int cargoId, string nuevoNombre)
    {
        try
        {
            var query = Collection.WhereEqualTo("cargoId", cargoId);
            var snapshot = await query.GetSnapshotAsync();
            
            var batch = CreateBatch();
            foreach (var doc in snapshot.Documents)
            {
                batch.Update(doc.Reference, new Dictionary<string, object>
                {
                    ["cargoNombre"] = nuevoNombre,
                    ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                });
            }
            
            await batch.CommitAsync();
            _logger?.LogInformation("Actualizado cargoNombre en {Count} contratos", snapshot.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar cargoNombre en contratos para {CargoId}", cargoId);
            throw;
        }
    }
    
    #endregion
}
