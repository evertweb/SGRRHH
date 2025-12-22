using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Permisos para Firestore.
/// Colección: "permisos"
/// 
/// Campos desnormalizados:
/// - empleadoNombre, tipoPermisoNombre, solicitadoPorNombre, aprobadoPorNombre
/// </summary>
public class PermisoFirestoreRepository : FirestoreRepository<Permiso>, IPermisoRepository
{
    private const string COLLECTION_NAME = "permisos";
    private const string ACTA_PREFIX = "PERM";
    
    public PermisoFirestoreRepository(FirebaseInitializer firebase, ILogger<PermisoFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(Permiso entity)
    {
        var doc = base.EntityToDocument(entity);
        
        // Datos básicos
        doc["numeroActa"] = entity.NumeroActa;
        doc["motivo"] = entity.Motivo;
        doc["estado"] = entity.Estado.ToString();
        doc["observaciones"] = entity.Observaciones;
        doc["motivoRechazo"] = entity.MotivoRechazo;
        
        // Fechas
        doc["fechaSolicitud"] = Timestamp.FromDateTime(entity.FechaSolicitud.ToUniversalTime());
        doc["fechaInicio"] = Timestamp.FromDateTime(entity.FechaInicio.ToUniversalTime());
        doc["fechaFin"] = Timestamp.FromDateTime(entity.FechaFin.ToUniversalTime());
        doc["totalDias"] = entity.TotalDias;
        
        doc["fechaAprobacion"] = entity.FechaAprobacion.HasValue
            ? Timestamp.FromDateTime(entity.FechaAprobacion.Value.ToUniversalTime())
            : null;
        
        // Compensación
        doc["diasPendientesCompensacion"] = entity.DiasPendientesCompensacion;
        doc["fechaCompensacion"] = entity.FechaCompensacion.HasValue
            ? Timestamp.FromDateTime(entity.FechaCompensacion.Value.ToUniversalTime())
            : null;
        
        // Documento soporte - URL de Firebase Storage
        doc["documentoSoporteUrl"] = entity.DocumentoSoportePath;
        
        // Empleado - con datos desnormalizados
        doc["empleadoId"] = entity.EmpleadoId;
        doc["empleadoNombre"] = entity.Empleado?.NombreCompleto;
        doc["empleadoCodigo"] = entity.Empleado?.Codigo;
        doc["empleadoDepartamento"] = entity.Empleado?.Departamento?.Nombre;
        
        // Tipo de permiso - con datos desnormalizados
        doc["tipoPermisoId"] = entity.TipoPermisoId;
        doc["tipoPermisoNombre"] = entity.TipoPermiso?.Nombre;
        doc["tipoPermisoEsCompensable"] = entity.TipoPermiso?.EsCompensable;
        doc["tipoPermisoRequiereDocumento"] = entity.TipoPermiso?.RequiereDocumento;
        
        // Usuario solicitante - con datos desnormalizados
        doc["solicitadoPorId"] = entity.SolicitadoPorId;
        doc["solicitadoPorNombre"] = entity.SolicitadoPor?.NombreCompleto;
        
        // Usuario aprobador - con datos desnormalizados
        doc["aprobadoPorId"] = entity.AprobadoPorId;
        doc["aprobadoPorNombre"] = entity.AprobadoPor?.NombreCompleto;
        
        return doc;
    }
    
    protected override Permiso DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        // Datos básicos
        if (document.TryGetValue<string>("numeroActa", out var numeroActa))
            entity.NumeroActa = numeroActa ?? string.Empty;
        
        if (document.TryGetValue<string>("motivo", out var motivo))
            entity.Motivo = motivo ?? string.Empty;
        
        if (document.TryGetValue<string>("estado", out var estadoStr) && !string.IsNullOrEmpty(estadoStr))
            if (Enum.TryParse<EstadoPermiso>(estadoStr, out var estado))
                entity.Estado = estado;
        
        if (document.TryGetValue<string>("observaciones", out var obs))
            entity.Observaciones = obs;
        
        if (document.TryGetValue<string>("motivoRechazo", out var motivoRechazo))
            entity.MotivoRechazo = motivoRechazo;
        
        // Fechas
        if (document.TryGetValue<Timestamp>("fechaSolicitud", out var fechaSol))
            entity.FechaSolicitud = fechaSol.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp>("fechaInicio", out var fechaInicio))
            entity.FechaInicio = fechaInicio.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp>("fechaFin", out var fechaFin))
            entity.FechaFin = fechaFin.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<int>("totalDias", out var totalDias))
            entity.TotalDias = totalDias;
        
        if (document.TryGetValue<Timestamp?>("fechaAprobacion", out var fechaAprob) && fechaAprob.HasValue)
            entity.FechaAprobacion = fechaAprob.Value.ToDateTime().ToLocalTime();
        
        // Compensación
        if (document.TryGetValue<int?>("diasPendientesCompensacion", out var diasComp))
            entity.DiasPendientesCompensacion = diasComp;
        
        if (document.TryGetValue<Timestamp?>("fechaCompensacion", out var fechaComp) && fechaComp.HasValue)
            entity.FechaCompensacion = fechaComp.Value.ToDateTime().ToLocalTime();
        
        // Documento soporte
        if (document.TryGetValue<string>("documentoSoporteUrl", out var docUrl))
            entity.DocumentoSoportePath = docUrl;
        
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
        
        // Tipo de permiso
        if (document.TryGetValue<int>("tipoPermisoId", out var tipoPermisoId))
        {
            entity.TipoPermisoId = tipoPermisoId;
            entity.TipoPermiso = new TipoPermiso { Id = tipoPermisoId };
            
            if (document.TryGetValue<string>("tipoPermisoNombre", out var tipoNombre))
                entity.TipoPermiso.Nombre = tipoNombre ?? string.Empty;
            
            if (document.TryGetValue<bool?>("tipoPermisoEsCompensable", out var esComp))
                entity.TipoPermiso.EsCompensable = esComp ?? false;
            
            if (document.TryGetValue<bool?>("tipoPermisoRequiereDocumento", out var reqDoc))
                entity.TipoPermiso.RequiereDocumento = reqDoc ?? false;
        }
        
        // Usuario solicitante
        if (document.TryGetValue<int>("solicitadoPorId", out var solicitadoPorId))
        {
            entity.SolicitadoPorId = solicitadoPorId;
            if (document.TryGetValue<string>("solicitadoPorNombre", out var solNombre))
            {
                entity.SolicitadoPor = new Usuario
                {
                    Id = solicitadoPorId,
                    NombreCompleto = solNombre ?? string.Empty
                };
            }
        }
        
        // Usuario aprobador
        if (document.TryGetValue<int?>("aprobadoPorId", out var aprobadoPorId) && aprobadoPorId.HasValue)
        {
            entity.AprobadoPorId = aprobadoPorId;
            if (document.TryGetValue<string>("aprobadoPorNombre", out var aprobNombre))
            {
                entity.AprobadoPor = new Usuario
                {
                    Id = aprobadoPorId.Value,
                    NombreCompleto = aprobNombre ?? string.Empty
                };
            }
        }
        
        return entity;
    }
    
    /// <summary>
    /// Convierte un DocumentSnapshot a Permiso.
    /// Método público para uso con FirestoreListenerService.
    /// </summary>
    public Permiso ConvertFromSnapshot(DocumentSnapshot document) => DocumentToEntity(document);

    #endregion

    #region IPermisoRepository Implementation
    
    /// <summary>
    /// Obtiene todos los permisos pendientes de aprobación
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetPendientesAsync()
    {
        try
        {
            var query = Collection
                .WhereEqualTo("estado", EstadoPermiso.Pendiente.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(p => p.FechaSolicitud)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener permisos pendientes");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene los permisos de un empleado específico
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener permisos del empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene los permisos en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            // Nota: Firestore no permite múltiples range filters en diferentes campos
            // Filtramos por fechaInicio y luego en memoria por fechaFin
            var query = Collection
                .WhereGreaterThanOrEqualTo("fechaInicio", Timestamp.FromDateTime(fechaInicio.ToUniversalTime()))
                .WhereLessThanOrEqualTo("fechaInicio", Timestamp.FromDateTime(fechaFin.ToUniversalTime()))
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener permisos en rango de fechas");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene los permisos por estado
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetByEstadoAsync(EstadoPermiso estado)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("estado", estado.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener permisos con estado {Estado}", estado);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene el próximo número de acta disponible
    /// </summary>
    public async Task<string> GetProximoNumeroActaAsync()
    {
        try
        {
            var year = DateTime.Now.Year;
            var prefix = $"{ACTA_PREFIX}-{year}-";
            
            // Buscar el último número de acta del año actual
            var query = Collection
                .OrderByDescending("numeroActa")
                .Limit(20); // Traer varios para filtrar por año
            var snapshot = await query.GetSnapshotAsync();
            
            int maxNumber = 0;
            foreach (var doc in snapshot.Documents)
            {
                if (doc.TryGetValue<string>("numeroActa", out var acta) &&
                    acta != null && acta.StartsWith(prefix))
                {
                    var numStr = acta.Replace(prefix, "");
                    if (int.TryParse(numStr, out int num) && num > maxNumber)
                    {
                        maxNumber = num;
                    }
                }
            }
            
            return $"{prefix}{(maxNumber + 1):D4}";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener próximo número de acta");
            return $"{ACTA_PREFIX}-{DateTime.Now.Year}-0001";
        }
    }
    
    /// <summary>
    /// Verifica si existe solapamiento de fechas para un empleado
    /// </summary>
    public async Task<bool> ExisteSolapamientoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? excludePermisoId = null)
    {
        try
        {
            // Traer todos los permisos aprobados del empleado
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("estado", EstadoPermiso.Aprobado.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            foreach (var doc in snapshot.Documents)
            {
                if (excludePermisoId.HasValue)
                {
                    if (doc.TryGetValue<int>("id", out var id) && id == excludePermisoId.Value)
                        continue;
                }
                
                if (doc.TryGetValue<Timestamp>("fechaInicio", out var fInicio) &&
                    doc.TryGetValue<Timestamp>("fechaFin", out var fFin))
                {
                    var permisoInicio = fInicio.ToDateTime().ToLocalTime();
                    var permisoFin = fFin.ToDateTime().ToLocalTime();
                    
                    // Verificar solapamiento: (A.inicio <= B.fin) && (A.fin >= B.inicio)
                    if (fechaInicio <= permisoFin && fechaFin >= permisoInicio)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar solapamiento de fechas para empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los permisos con filtros opcionales
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetAllWithFiltersAsync(
        int? empleadoId = null,
        EstadoPermiso? estado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null)
    {
        try
        {
            Query query = Collection.WhereEqualTo("activo", true);
            
            if (empleadoId.HasValue)
                query = query.WhereEqualTo("empleadoId", empleadoId.Value);
            
            if (estado.HasValue)
                query = query.WhereEqualTo("estado", estado.Value.ToString());
            
            var snapshot = await query.GetSnapshotAsync();
            var permisos = snapshot.Documents.Select(DocumentToEntity).ToList();
            
            // Filtrar por fechas en memoria (Firestore tiene limitaciones con múltiples range)
            if (fechaDesde.HasValue)
                permisos = permisos.Where(p => p.FechaSolicitud >= fechaDesde.Value).ToList();
            
            if (fechaHasta.HasValue)
                permisos = permisos.Where(p => p.FechaSolicitud <= fechaHasta.Value).ToList();
            
            return permisos.OrderByDescending(p => p.FechaSolicitud).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener permisos con filtros");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un permiso por su número de acta
    /// </summary>
    public async Task<Permiso?> GetByNumeroActaAsync(string numeroActa)
    {
        try
        {
            var query = Collection.WhereEqualTo("numeroActa", numeroActa).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            return doc == null ? null : DocumentToEntity(doc);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener permiso por número de acta: {NumeroActa}", numeroActa);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los permisos activos ordenados
    /// </summary>
    public override async Task<IEnumerable<Permiso>> GetAllActiveAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener permisos activos");
            throw;
        }
    }
    
    #endregion
    
    #region Helper Methods - Actualizar datos desnormalizados
    
    /// <summary>
    /// Actualiza el nombre del empleado en todos sus permisos.
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
            _logger?.LogInformation("Actualizado empleadoNombre en {Count} permisos", snapshot.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar empleadoNombre en permisos para {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza el nombre del tipo de permiso en todos los permisos que lo usan.
    /// </summary>
    public async Task ActualizarTipoPermisoNombreAsync(int tipoPermisoId, string nuevoNombre)
    {
        try
        {
            var query = Collection.WhereEqualTo("tipoPermisoId", tipoPermisoId);
            var snapshot = await query.GetSnapshotAsync();
            
            var batch = CreateBatch();
            foreach (var doc in snapshot.Documents)
            {
                batch.Update(doc.Reference, new Dictionary<string, object>
                {
                    ["tipoPermisoNombre"] = nuevoNombre,
                    ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                });
            }
            
            await batch.CommitAsync();
            _logger?.LogInformation("Actualizado tipoPermisoNombre en {Count} permisos", snapshot.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar tipoPermisoNombre para {TipoPermisoId}", tipoPermisoId);
            throw;
        }
    }
    
    #endregion
}
