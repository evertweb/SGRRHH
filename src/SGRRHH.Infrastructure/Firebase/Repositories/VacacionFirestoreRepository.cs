using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Vacaciones para Firestore.
/// Colección: "vacaciones"
/// 
/// Campos desnormalizados:
/// - empleadoNombre, empleadoCodigo, departamentoNombre
/// </summary>
public class VacacionFirestoreRepository : FirestoreRepository<Vacacion>, IVacacionRepository
{
    private const string COLLECTION_NAME = "vacaciones";
    
    public VacacionFirestoreRepository(FirebaseInitializer firebase, ILogger<VacacionFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(Vacacion entity)
    {
        var doc = base.EntityToDocument(entity);
        
        // Empleado - con datos desnormalizados
        doc["empleadoId"] = entity.EmpleadoId;
        doc["empleadoNombre"] = entity.Empleado?.NombreCompleto;
        doc["empleadoCodigo"] = entity.Empleado?.Codigo;
        doc["empleadoDepartamento"] = entity.Empleado?.Departamento?.Nombre;
        
        // Fechas
        doc["fechaInicio"] = Timestamp.FromDateTime(entity.FechaInicio.ToUniversalTime());
        doc["fechaFin"] = Timestamp.FromDateTime(entity.FechaFin.ToUniversalTime());
        
        // Días
        doc["diasTomados"] = entity.DiasTomados;
        doc["periodoCorrespondiente"] = entity.PeriodoCorrespondiente;
        
        // Estado
        doc["estado"] = entity.Estado.ToString();
        
        // Observaciones
        doc["observaciones"] = entity.Observaciones;
        
        // Campos de auditoría (Fix #11)
        doc["fechaSolicitud"] = Timestamp.FromDateTime(entity.FechaSolicitud.ToUniversalTime());
        doc["solicitadoPorId"] = entity.SolicitadoPorId;
        doc["aprobadoPorId"] = entity.AprobadoPorId;
        if (entity.FechaAprobacion.HasValue)
            doc["fechaAprobacion"] = Timestamp.FromDateTime(entity.FechaAprobacion.Value.ToUniversalTime());
        doc["motivoRechazo"] = entity.MotivoRechazo;
        
        return doc;
    }
    
    protected override Vacacion DocumentToEntity(DocumentSnapshot document)
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
        
        // Fechas
        if (document.TryGetValue<Timestamp>("fechaInicio", out var fechaInicio))
            entity.FechaInicio = fechaInicio.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp>("fechaFin", out var fechaFin))
            entity.FechaFin = fechaFin.ToDateTime().ToLocalTime();
        
        // Días
        if (document.TryGetValue<int>("diasTomados", out var diasTomados))
            entity.DiasTomados = diasTomados;
        
        if (document.TryGetValue<int>("periodoCorrespondiente", out var periodo))
            entity.PeriodoCorrespondiente = periodo;
        
        // Estado
        if (document.TryGetValue<string>("estado", out var estadoStr) && !string.IsNullOrEmpty(estadoStr))
            if (Enum.TryParse<EstadoVacacion>(estadoStr, out var estado))
                entity.Estado = estado;
        
        // Observaciones
        if (document.TryGetValue<string>("observaciones", out var obs))
            entity.Observaciones = obs;
        
        // Campos de auditoría (Fix #11)
        if (document.TryGetValue<Timestamp>("fechaSolicitud", out var fechaSolicitud))
            entity.FechaSolicitud = fechaSolicitud.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<int>("solicitadoPorId", out var solicitadoPorId))
            entity.SolicitadoPorId = solicitadoPorId;
        
        if (document.TryGetValue<int>("aprobadoPorId", out var aprobadoPorId))
            entity.AprobadoPorId = aprobadoPorId;
        
        if (document.TryGetValue<Timestamp>("fechaAprobacion", out var fechaAprobacion))
            entity.FechaAprobacion = fechaAprobacion.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<string>("motivoRechazo", out var motivoRechazo))
            entity.MotivoRechazo = motivoRechazo;
        
        return entity;
    }
    
    #endregion
    
    #region IVacacionRepository Implementation
    
    /// <summary>
    /// Obtiene las vacaciones de un empleado específico
    /// </summary>
    public async Task<IEnumerable<Vacacion>> GetByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(v => v.PeriodoCorrespondiente)
                .ThenByDescending(v => v.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener vacaciones del empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene las vacaciones de un empleado en un periodo específico
    /// </summary>
    public async Task<IEnumerable<Vacacion>> GetByEmpleadoYPeriodoAsync(int empleadoId, int periodo)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("periodoCorrespondiente", periodo)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(v => v.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener vacaciones del empleado {EmpleadoId} periodo {Periodo}", empleadoId, periodo);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene las vacaciones en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<Vacacion>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            // Firestore no soporta múltiples range queries en diferentes campos
            // Filtramos por fechaInicio >= fechaInicio requerida
            var query = Collection
                .WhereGreaterThanOrEqualTo("fechaInicio", Timestamp.FromDateTime(fechaInicio.ToUniversalTime()))
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            // Filtrar en memoria las que terminan antes de fechaFin
            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(v => v.FechaFin <= fechaFin)
                .OrderByDescending(v => v.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener vacaciones en rango de fechas");
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si existe traslape de fechas para un empleado
    /// </summary>
    public async Task<bool> ExisteTraslapeAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? vacacionIdExcluir = null)
    {
        try
        {
            // Obtener todas las vacaciones del empleado que no estén canceladas
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            foreach (var doc in snapshot.Documents)
            {
                // Excluir la vacación especificada
                if (vacacionIdExcluir.HasValue)
                {
                    if (doc.TryGetValue<int>("id", out var id) && id == vacacionIdExcluir.Value)
                        continue;
                }
                
                // Verificar estado - ignorar canceladas
                if (doc.TryGetValue<string>("estado", out var estadoStr))
                {
                    if (Enum.TryParse<EstadoVacacion>(estadoStr, out var estado) && estado == EstadoVacacion.Cancelada)
                        continue;
                }
                
                if (doc.TryGetValue<Timestamp>("fechaInicio", out var fInicio) &&
                    doc.TryGetValue<Timestamp>("fechaFin", out var fFin))
                {
                    var vacInicio = fInicio.ToDateTime().ToLocalTime();
                    var vacFin = fFin.ToDateTime().ToLocalTime();
                    
                    // Verificar solapamiento: (A.inicio <= B.fin) && (A.fin >= B.inicio)
                    if (fechaInicio <= vacFin && fechaFin >= vacInicio)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar traslape de vacaciones para empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene vacaciones por estado
    /// </summary>
    public async Task<IEnumerable<Vacacion>> GetByEstadoAsync(EstadoVacacion estado)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("estado", estado.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(v => v.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener vacaciones con estado {Estado}", estado);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene el total de días tomados por un empleado en un periodo
    /// </summary>
    public async Task<int> GetDiasTomadosEnPeriodoAsync(int empleadoId, int periodo)
    {
        try
        {
            var vacaciones = await GetByEmpleadoYPeriodoAsync(empleadoId, periodo);
            return vacaciones
                .Where(v => v.Estado == EstadoVacacion.Disfrutada || v.Estado == EstadoVacacion.Programada)
                .Sum(v => v.DiasTomados);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al calcular días tomados para empleado {EmpleadoId} periodo {Periodo}", empleadoId, periodo);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene vacaciones próximas a iniciar
    /// </summary>
    public async Task<IEnumerable<Vacacion>> GetProximasAsync(int diasAnticipacion = 7)
    {
        try
        {
            var fechaInicio = DateTime.Today;
            var fechaLimite = DateTime.Today.AddDays(diasAnticipacion);
            
            var query = Collection
                .WhereEqualTo("estado", EstadoVacacion.Programada.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(v => v.FechaInicio >= fechaInicio && v.FechaInicio <= fechaLimite)
                .OrderBy(v => v.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener vacaciones próximas");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todas las vacaciones activas ordenadas
    /// </summary>
    public override async Task<IEnumerable<Vacacion>> GetAllActiveAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(v => v.PeriodoCorrespondiente)
                .ThenByDescending(v => v.FechaInicio)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener vacaciones activas");
            throw;
        }
    }
    
    #endregion
    
    #region Helper Methods - Actualizar datos desnormalizados
    
    /// <summary>
    /// Actualiza el nombre del empleado en todas sus vacaciones.
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
            _logger?.LogInformation("Actualizado empleadoNombre en {Count} vacaciones", snapshot.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar empleadoNombre en vacaciones para {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    #endregion
}
