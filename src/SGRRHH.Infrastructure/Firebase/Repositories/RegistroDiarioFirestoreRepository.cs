using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Registros Diarios para Firestore.
/// Colección: "registros-diarios"
/// Subcolección: "registros-diarios/{id}/detalles"
/// 
/// Campos desnormalizados:
/// - empleadoNombre, empleadoCodigo, empleadoDepartamento
/// </summary>
public class RegistroDiarioFirestoreRepository : FirestoreRepository<RegistroDiario>, IRegistroDiarioRepository
{
    private const string COLLECTION_NAME = "registros-diarios";
    private const string DETALLES_SUBCOLLECTION = "detalles";
    
    public RegistroDiarioFirestoreRepository(FirebaseInitializer firebase, ILogger<RegistroDiarioFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(RegistroDiario entity)
    {
        var doc = base.EntityToDocument(entity);
        
        // Fecha del registro
        doc["fecha"] = Timestamp.FromDateTime(entity.Fecha.Date.ToUniversalTime());
        
        // Horas de entrada/salida (almacenadas como strings "hh:mm")
        doc["horaEntrada"] = entity.HoraEntrada?.ToString(@"hh\:mm");
        doc["horaSalida"] = entity.HoraSalida?.ToString(@"hh\:mm");
        
        // Estado y observaciones
        doc["estado"] = entity.Estado.ToString();
        doc["observaciones"] = entity.Observaciones;
        
        // Empleado - con datos desnormalizados
        doc["empleadoId"] = entity.EmpleadoId;
        doc["empleadoNombre"] = entity.Empleado?.NombreCompleto;
        doc["empleadoCodigo"] = entity.Empleado?.Codigo;
        doc["empleadoDepartamento"] = entity.Empleado?.Departamento?.Nombre;
        
        // Total de horas (calculado de los detalles, pero lo guardamos para queries)
        doc["totalHoras"] = (double)entity.TotalHoras;
        
        return doc;
    }
    
    protected override RegistroDiario DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        // Fecha
        if (document.TryGetValue<Timestamp>("fecha", out var fecha))
            entity.Fecha = fecha.ToDateTime().ToLocalTime();
        
        // Horas de entrada/salida
        if (document.TryGetValue<string>("horaEntrada", out var horaEntradaStr) && !string.IsNullOrEmpty(horaEntradaStr))
            if (TimeSpan.TryParse(horaEntradaStr, out var horaEntrada))
                entity.HoraEntrada = horaEntrada;
        
        if (document.TryGetValue<string>("horaSalida", out var horaSalidaStr) && !string.IsNullOrEmpty(horaSalidaStr))
            if (TimeSpan.TryParse(horaSalidaStr, out var horaSalida))
                entity.HoraSalida = horaSalida;
        
        // Estado
        if (document.TryGetValue<string>("estado", out var estadoStr) && !string.IsNullOrEmpty(estadoStr))
            if (Enum.TryParse<EstadoRegistroDiario>(estadoStr, out var estado))
                entity.Estado = estado;
        
        // Observaciones
        if (document.TryGetValue<string>("observaciones", out var obs))
            entity.Observaciones = obs;
        
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
        
        // Inicializar lista de detalles vacía (se cargan por separado con GetByIdWithDetallesAsync)
        entity.DetallesActividades = new List<DetalleActividad>();
        
        return entity;
    }
    
    #endregion
    
    #region Detalle Actividad Mapping
    
    /// <summary>
    /// Convierte un DetalleActividad a diccionario para guardar en subcolección
    /// </summary>
    private Dictionary<string, object?> DetalleToDocument(DetalleActividad detalle)
    {
        return new Dictionary<string, object?>
        {
            ["id"] = detalle.Id,
            ["activo"] = detalle.Activo,
            ["fechaCreacion"] = Timestamp.FromDateTime(detalle.FechaCreacion.ToUniversalTime()),
            ["fechaModificacion"] = detalle.FechaModificacion.HasValue 
                ? Timestamp.FromDateTime(detalle.FechaModificacion.Value.ToUniversalTime()) 
                : null,
            
            // Actividad - desnormalizado
            ["actividadId"] = detalle.ActividadId,
            ["actividadNombre"] = detalle.Actividad?.Nombre,
            
            // Proyecto - desnormalizado (opcional)
            ["proyectoId"] = detalle.ProyectoId,
            ["proyectoNombre"] = detalle.Proyecto?.Nombre,
            
            // Datos del detalle
            ["horas"] = (double)detalle.Horas,
            ["descripcion"] = detalle.Descripcion,
            ["horaInicio"] = detalle.HoraInicio?.ToString(@"hh\:mm"),
            ["horaFin"] = detalle.HoraFin?.ToString(@"hh\:mm"),
            ["orden"] = detalle.Orden
        };
    }
    
    /// <summary>
    /// Convierte un documento de subcolección a DetalleActividad
    /// </summary>
    private DetalleActividad DocumentToDetalle(DocumentSnapshot document, int registroDiarioId)
    {
        var detalle = new DetalleActividad
        {
            RegistroDiarioId = registroDiarioId
        };
        
        if (document.TryGetValue<int>("id", out var id))
            detalle.Id = id;
        
        if (document.TryGetValue<bool>("activo", out var activo))
            detalle.Activo = activo;
        
        if (document.TryGetValue<Timestamp>("fechaCreacion", out var fc))
            detalle.FechaCreacion = fc.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp?>("fechaModificacion", out var fm) && fm.HasValue)
            detalle.FechaModificacion = fm.Value.ToDateTime().ToLocalTime();
        
        // Actividad
        if (document.TryGetValue<int>("actividadId", out var actividadId))
        {
            detalle.ActividadId = actividadId;
            detalle.Actividad = new Actividad { Id = actividadId };
            
            if (document.TryGetValue<string>("actividadNombre", out var actNombre))
                detalle.Actividad.Nombre = actNombre ?? string.Empty;
        }
        
        // Proyecto (opcional)
        if (document.TryGetValue<int?>("proyectoId", out var proyectoId) && proyectoId.HasValue)
        {
            detalle.ProyectoId = proyectoId;
            detalle.Proyecto = new Proyecto { Id = proyectoId.Value };
            
            if (document.TryGetValue<string>("proyectoNombre", out var proyNombre))
                detalle.Proyecto.Nombre = proyNombre ?? string.Empty;
        }
        
        // Datos del detalle
        if (document.TryGetValue<double>("horas", out var horas))
            detalle.Horas = (decimal)horas;
        
        if (document.TryGetValue<string>("descripcion", out var desc))
            detalle.Descripcion = desc;
        
        if (document.TryGetValue<string>("horaInicio", out var hiStr) && !string.IsNullOrEmpty(hiStr))
            if (TimeSpan.TryParse(hiStr, out var hi))
                detalle.HoraInicio = hi;
        
        if (document.TryGetValue<string>("horaFin", out var hfStr) && !string.IsNullOrEmpty(hfStr))
            if (TimeSpan.TryParse(hfStr, out var hf))
                detalle.HoraFin = hf;
        
        if (document.TryGetValue<int>("orden", out var orden))
            detalle.Orden = orden;
        
        // Guardar Document ID
        detalle.SetFirestoreDocumentId(document.Id);
        
        return detalle;
    }
    
    #endregion
    
    #region Override AddAsync to handle detalles
    
    /// <summary>
    /// Agrega un registro diario con sus detalles de actividades
    /// </summary>
    public override async Task<RegistroDiario> AddAsync(RegistroDiario entity)
    {
        try
        {
            var registro = await base.AddAsync(entity);
            var documentId = registro.GetFirestoreDocumentId();
            
            if (!string.IsNullOrEmpty(documentId) && entity.DetallesActividades?.Any() == true)
            {
                await AddDetallesAsync(documentId, entity.DetallesActividades.ToList());
            }
            
            return registro;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al agregar registro diario con detalles");
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza un registro diario y sus detalles
    /// </summary>
    public override async Task UpdateAsync(RegistroDiario entity)
    {
        try
        {
            var documentId = entity.GetFirestoreDocumentId();
            
            if (string.IsNullOrEmpty(documentId))
            {
                // Buscar por Id
                var query = Collection.WhereEqualTo("id", entity.Id).Limit(1);
                var snapshot = await query.GetSnapshotAsync();
                var doc = snapshot.Documents.FirstOrDefault();
                
                if (doc == null)
                    throw new InvalidOperationException($"No se encontró el registro diario con Id {entity.Id}");
                
                documentId = doc.Id;
            }
            
            // Actualizar documento principal
            entity.FechaModificacion = DateTime.Now;
            
            // Actualizar totalHoras
            var data = EntityToDocument(entity);
            data["totalHoras"] = (double)(entity.DetallesActividades?.Sum(d => d.Horas) ?? 0);
            
            var docRef = Collection.Document(documentId);
            await docRef.SetAsync(data, SetOptions.MergeAll);
            
            // Sincronizar detalles
            if (entity.DetallesActividades != null)
            {
                await SyncDetallesAsync(documentId, entity.DetallesActividades.ToList());
            }
            
            entity.SetFirestoreDocumentId(documentId);
            
            _logger?.LogInformation("Registro diario actualizado: {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar registro diario con Id {Id}", entity.Id);
            throw;
        }
    }
    
    #endregion
    
    #region Detalles Subcollection Methods
    
    /// <summary>
    /// Agrega detalles de actividades a un registro diario
    /// </summary>
    private async Task AddDetallesAsync(string registroDocumentId, List<DetalleActividad> detalles)
    {
        var detallesCollection = Collection.Document(registroDocumentId).Collection(DETALLES_SUBCOLLECTION);
        var batch = CreateBatch();
        
        int nextId = 1;
        
        foreach (var detalle in detalles)
        {
            if (detalle.Id == 0)
                detalle.Id = nextId++;
            
            detalle.FechaCreacion = DateTime.Now;
            detalle.Activo = true;
            
            var detalleDoc = DetalleToDocument(detalle);
            var docRef = detallesCollection.Document();
            batch.Set(docRef, detalleDoc);
            
            detalle.SetFirestoreDocumentId(docRef.Id);
        }
        
        await batch.CommitAsync();
        _logger?.LogInformation("Agregados {Count} detalles al registro {DocumentId}", detalles.Count, registroDocumentId);
    }
    
    /// <summary>
    /// Sincroniza los detalles de actividades (elimina los que no están, agrega nuevos, actualiza existentes)
    /// </summary>
    private async Task SyncDetallesAsync(string registroDocumentId, List<DetalleActividad> detalles)
    {
        var detallesCollection = Collection.Document(registroDocumentId).Collection(DETALLES_SUBCOLLECTION);
        
        // Obtener detalles actuales
        var currentSnapshot = await detallesCollection.GetSnapshotAsync();
        var currentDocIds = currentSnapshot.Documents.Select(d => d.Id).ToHashSet();
        
        var batch = CreateBatch();
        var newDocIds = new HashSet<string>();
        
        int nextId = detalles.Where(d => d.Id > 0).Select(d => d.Id).DefaultIfEmpty(0).Max() + 1;
        
        foreach (var detalle in detalles)
        {
            var existingDocId = detalle.GetFirestoreDocumentId();
            
            if (!string.IsNullOrEmpty(existingDocId) && currentDocIds.Contains(existingDocId))
            {
                // Actualizar existente
                detalle.FechaModificacion = DateTime.Now;
                var detalleDoc = DetalleToDocument(detalle);
                var docRef = detallesCollection.Document(existingDocId);
                batch.Set(docRef, detalleDoc, SetOptions.MergeAll);
                newDocIds.Add(existingDocId);
            }
            else
            {
                // Agregar nuevo
                if (detalle.Id == 0)
                    detalle.Id = nextId++;
                
                detalle.FechaCreacion = DateTime.Now;
                detalle.Activo = true;
                
                var detalleDoc = DetalleToDocument(detalle);
                var docRef = detallesCollection.Document();
                batch.Set(docRef, detalleDoc);
                newDocIds.Add(docRef.Id);
                
                detalle.SetFirestoreDocumentId(docRef.Id);
            }
        }
        
        // Eliminar detalles que ya no están
        foreach (var docId in currentDocIds.Except(newDocIds))
        {
            batch.Delete(detallesCollection.Document(docId));
        }
        
        await batch.CommitAsync();
    }
    
    /// <summary>
    /// Carga los detalles de actividades de un registro
    /// </summary>
    private async Task<List<DetalleActividad>> GetDetallesAsync(string registroDocumentId, int registroDiarioId)
    {
        var detallesCollection = Collection.Document(registroDocumentId).Collection(DETALLES_SUBCOLLECTION);
        var snapshot = await detallesCollection.WhereEqualTo("activo", true).GetSnapshotAsync();
        
        return snapshot.Documents
            .Select(d => DocumentToDetalle(d, registroDiarioId))
            .OrderBy(d => d.Orden)
            .ToList();
    }
    
    #endregion
    
    #region IRegistroDiarioRepository Implementation
    
    /// <summary>
    /// Obtiene un registro por fecha y empleado
    /// </summary>
    public async Task<RegistroDiario?> GetByFechaEmpleadoAsync(DateTime fecha, int empleadoId)
    {
        try
        {
            var fechaInicio = fecha.Date.ToUniversalTime();
            var fechaFin = fecha.Date.AddDays(1).ToUniversalTime();
            
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereGreaterThanOrEqualTo("fecha", Timestamp.FromDateTime(fechaInicio))
                .WhereLessThan("fecha", Timestamp.FromDateTime(fechaFin))
                .WhereEqualTo("activo", true)
                .Limit(1);
            
            var snapshot = await query.GetSnapshotAsync();
            var doc = snapshot.Documents.FirstOrDefault();
            
            if (doc == null)
                return null;
            
            var registro = DocumentToEntity(doc);
            
            // Cargar detalles
            var documentId = registro.GetFirestoreDocumentId();
            if (!string.IsNullOrEmpty(documentId))
            {
                registro.DetallesActividades = await GetDetallesAsync(documentId, registro.Id);
            }
            
            return registro;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener registro por fecha {Fecha} y empleado {EmpleadoId}", fecha, empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un registro con sus detalles por ID
    /// </summary>
    public async Task<RegistroDiario?> GetByIdWithDetallesAsync(int id)
    {
        try
        {
            var query = Collection.WhereEqualTo("id", id).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            var doc = snapshot.Documents.FirstOrDefault();
            
            if (doc == null)
                return null;
            
            var registro = DocumentToEntity(doc);
            
            // Cargar detalles
            var documentId = registro.GetFirestoreDocumentId();
            if (!string.IsNullOrEmpty(documentId))
            {
                registro.DetallesActividades = await GetDetallesAsync(documentId, registro.Id);
            }
            
            return registro;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener registro {Id} con detalles", id);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene registros de un empleado en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoRangoFechasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereGreaterThanOrEqualTo("fecha", Timestamp.FromDateTime(fechaInicio.Date.ToUniversalTime()))
                .WhereLessThanOrEqualTo("fecha", Timestamp.FromDateTime(fechaFin.Date.ToUniversalTime()))
                .WhereEqualTo("activo", true);
            
            var snapshot = await query.GetSnapshotAsync();
            
            var registros = new List<RegistroDiario>();
            foreach (var doc in snapshot.Documents)
            {
                var registro = DocumentToEntity(doc);
                var documentId = registro.GetFirestoreDocumentId();
                if (!string.IsNullOrEmpty(documentId))
                {
                    registro.DetallesActividades = await GetDetallesAsync(documentId, registro.Id);
                }
                registros.Add(registro);
            }
            
            return registros.OrderByDescending(r => r.Fecha).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener registros de empleado {EmpleadoId} en rango de fechas", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene registros de una fecha específica (todos los empleados)
    /// </summary>
    public async Task<IEnumerable<RegistroDiario>> GetByFechaAsync(DateTime fecha)
    {
        try
        {
            var fechaInicio = fecha.Date.ToUniversalTime();
            var fechaFin = fecha.Date.AddDays(1).ToUniversalTime();
            
            var query = Collection
                .WhereGreaterThanOrEqualTo("fecha", Timestamp.FromDateTime(fechaInicio))
                .WhereLessThan("fecha", Timestamp.FromDateTime(fechaFin))
                .WhereEqualTo("activo", true);
            
            var snapshot = await query.GetSnapshotAsync();
            
            var registros = new List<RegistroDiario>();
            foreach (var doc in snapshot.Documents)
            {
                var registro = DocumentToEntity(doc);
                var documentId = registro.GetFirestoreDocumentId();
                if (!string.IsNullOrEmpty(documentId))
                {
                    registro.DetallesActividades = await GetDetallesAsync(documentId, registro.Id);
                }
                registros.Add(registro);
            }
            
            return registros
                .OrderBy(r => r.Empleado?.Apellidos ?? string.Empty)
                .ThenBy(r => r.Empleado?.Nombres ?? string.Empty)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener registros de fecha {Fecha}", fecha);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene registros de todos los empleados en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<RegistroDiario>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var query = Collection
                .WhereGreaterThanOrEqualTo("fecha", Timestamp.FromDateTime(fechaInicio.Date.ToUniversalTime()))
                .WhereLessThanOrEqualTo("fecha", Timestamp.FromDateTime(fechaFin.Date.ToUniversalTime()))
                .WhereEqualTo("activo", true);
            
            var snapshot = await query.GetSnapshotAsync();
            
            var registros = new List<RegistroDiario>();
            foreach (var doc in snapshot.Documents)
            {
                var registro = DocumentToEntity(doc);
                var documentId = registro.GetFirestoreDocumentId();
                if (!string.IsNullOrEmpty(documentId))
                {
                    registro.DetallesActividades = await GetDetallesAsync(documentId, registro.Id);
                }
                registros.Add(registro);
            }
            
            return registros.OrderByDescending(r => r.Fecha).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener registros en rango de fechas {FechaInicio} - {FechaFin}", fechaInicio, fechaFin);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene los registros de un empleado con sus detalles
    /// </summary>
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoWithDetallesAsync(int empleadoId, int? cantidad = null)
    {
        try
        {
            Query query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true)
                .OrderByDescending("fecha");
            
            if (cantidad.HasValue)
                query = query.Limit(cantidad.Value);
            
            var snapshot = await query.GetSnapshotAsync();
            
            var registros = new List<RegistroDiario>();
            foreach (var doc in snapshot.Documents)
            {
                var registro = DocumentToEntity(doc);
                var documentId = registro.GetFirestoreDocumentId();
                if (!string.IsNullOrEmpty(documentId))
                {
                    registro.DetallesActividades = await GetDetallesAsync(documentId, registro.Id);
                }
                registros.Add(registro);
            }
            
            return registros;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener registros con detalles de empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene registros del mes actual de un empleado
    /// </summary>
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoMesActualAsync(int empleadoId)
    {
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        
        return await GetByEmpleadoRangoFechasAsync(empleadoId, inicioMes, finMes);
    }
    
    /// <summary>
    /// Verifica si existe un registro para la fecha y empleado
    /// </summary>
    public async Task<bool> ExistsByFechaEmpleadoAsync(DateTime fecha, int empleadoId)
    {
        try
        {
            var fechaInicio = fecha.Date.ToUniversalTime();
            var fechaFin = fecha.Date.AddDays(1).ToUniversalTime();
            
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereGreaterThanOrEqualTo("fecha", Timestamp.FromDateTime(fechaInicio))
                .WhereLessThan("fecha", Timestamp.FromDateTime(fechaFin))
                .WhereEqualTo("activo", true)
                .Limit(1);
            
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Any();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar existencia de registro para fecha {Fecha} y empleado {EmpleadoId}", fecha, empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Calcula el total de horas de un empleado en un rango de fechas
    /// </summary>
    public async Task<decimal> GetTotalHorasByEmpleadoRangoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var registros = await GetByEmpleadoRangoFechasAsync(empleadoId, fechaInicio, fechaFin);
            return registros.Sum(r => r.TotalHoras);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al calcular total de horas de empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    #endregion
    
    #region Helper Methods - Actualizar datos desnormalizados
    
    /// <summary>
    /// Actualiza el nombre del empleado en todos sus registros diarios.
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
            _logger?.LogInformation("Actualizado empleadoNombre en {Count} registros diarios", snapshot.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar empleadoNombre en registros diarios para {EmpleadoId}", empleadoId);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza el nombre de una actividad en todos los detalles que la usan.
    /// </summary>
    public async Task ActualizarActividadNombreAsync(int actividadId, string nuevoNombre)
    {
        try
        {
            // Obtener todos los registros
            var registrosSnapshot = await Collection.GetSnapshotAsync();
            
            foreach (var registroDoc in registrosSnapshot.Documents)
            {
                var detallesCollection = registroDoc.Reference.Collection(DETALLES_SUBCOLLECTION);
                var detallesQuery = detallesCollection.WhereEqualTo("actividadId", actividadId);
                var detallesSnapshot = await detallesQuery.GetSnapshotAsync();
                
                if (detallesSnapshot.Count > 0)
                {
                    var batch = CreateBatch();
                    foreach (var detalleDoc in detallesSnapshot.Documents)
                    {
                        batch.Update(detalleDoc.Reference, new Dictionary<string, object>
                        {
                            ["actividadNombre"] = nuevoNombre,
                            ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                        });
                    }
                    await batch.CommitAsync();
                }
            }
            
            _logger?.LogInformation("Actualizado actividadNombre para actividad {ActividadId}", actividadId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar actividadNombre para {ActividadId}", actividadId);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza el nombre de un proyecto en todos los detalles que lo usan.
    /// </summary>
    public async Task ActualizarProyectoNombreAsync(int proyectoId, string nuevoNombre)
    {
        try
        {
            // Obtener todos los registros
            var registrosSnapshot = await Collection.GetSnapshotAsync();
            
            foreach (var registroDoc in registrosSnapshot.Documents)
            {
                var detallesCollection = registroDoc.Reference.Collection(DETALLES_SUBCOLLECTION);
                var detallesQuery = detallesCollection.WhereEqualTo("proyectoId", proyectoId);
                var detallesSnapshot = await detallesQuery.GetSnapshotAsync();
                
                if (detallesSnapshot.Count > 0)
                {
                    var batch = CreateBatch();
                    foreach (var detalleDoc in detallesSnapshot.Documents)
                    {
                        batch.Update(detalleDoc.Reference, new Dictionary<string, object>
                        {
                            ["proyectoNombre"] = nuevoNombre,
                            ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                        });
                    }
                    await batch.CommitAsync();
                }
            }
            
            _logger?.LogInformation("Actualizado proyectoNombre para proyecto {ProyectoId}", proyectoId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar proyectoNombre para {ProyectoId}", proyectoId);
            throw;
        }
    }
    
    #endregion
    
    #region IRegistroDiarioRepository - Detalle Methods
    
    /// <summary>
    /// Agrega un detalle de actividad a un registro diario
    /// </summary>
    public async Task<DetalleActividad> AddDetalleAsync(int registroId, DetalleActividad detalle)
    {
        try
        {
            // Buscar el documento del registro por Id
            var query = Collection.WhereEqualTo("id", registroId).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            var registroDoc = snapshot.Documents.FirstOrDefault();
            
            if (registroDoc == null)
                throw new InvalidOperationException($"No se encontró el registro diario con Id {registroId}");
            
            var detallesCollection = registroDoc.Reference.Collection(DETALLES_SUBCOLLECTION);
            
            // Obtener el máximo Id de los detalles existentes
            var existingDetalles = await detallesCollection.GetSnapshotAsync();
            var maxId = existingDetalles.Documents
                .Select(d => d.TryGetValue<int>("id", out var id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();
            
            detalle.Id = maxId + 1;
            detalle.RegistroDiarioId = registroId;
            detalle.FechaCreacion = DateTime.Now;
            detalle.Activo = true;
            
            // Establecer orden
            if (detalle.Orden == 0)
                detalle.Orden = existingDetalles.Count + 1;
            
            var detalleDoc = DetalleToDocument(detalle);
            var docRef = await detallesCollection.AddAsync(detalleDoc);
            
            detalle.SetFirestoreDocumentId(docRef.Id);
            
            _logger?.LogInformation("Detalle agregado al registro {RegistroId}: {DetalleId}", registroId, detalle.Id);
            
            return detalle;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al agregar detalle al registro {RegistroId}", registroId);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza un detalle de actividad
    /// </summary>
    public async Task UpdateDetalleAsync(int registroId, DetalleActividad detalle)
    {
        try
        {
            // Buscar el documento del registro por Id
            var registroQuery = Collection.WhereEqualTo("id", registroId).Limit(1);
            var registroSnapshot = await registroQuery.GetSnapshotAsync();
            var registroDoc = registroSnapshot.Documents.FirstOrDefault();
            
            if (registroDoc == null)
                throw new InvalidOperationException($"No se encontró el registro diario con Id {registroId}");
            
            var detallesCollection = registroDoc.Reference.Collection(DETALLES_SUBCOLLECTION);
            
            // Buscar el detalle por Id
            var detalleQuery = detallesCollection.WhereEqualTo("id", detalle.Id).Limit(1);
            var detalleSnapshot = await detalleQuery.GetSnapshotAsync();
            var detalleDoc = detalleSnapshot.Documents.FirstOrDefault();
            
            if (detalleDoc == null)
                throw new InvalidOperationException($"No se encontró el detalle con Id {detalle.Id}");
            
            detalle.FechaModificacion = DateTime.Now;
            var detalleData = DetalleToDocument(detalle);
            
            await detalleDoc.Reference.SetAsync(detalleData, SetOptions.MergeAll);
            
            _logger?.LogInformation("Detalle actualizado en registro {RegistroId}: {DetalleId}", registroId, detalle.Id);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar detalle {DetalleId} del registro {RegistroId}", detalle.Id, registroId);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza un detalle de actividad usando el RegistroDiarioId del detalle
    /// </summary>
    public async Task UpdateDetalleAsync(DetalleActividad detalle)
    {
        await UpdateDetalleAsync(detalle.RegistroDiarioId, detalle);
    }
    
    /// <summary>
    /// Elimina un detalle de actividad
    /// </summary>
    public async Task DeleteDetalleAsync(int registroId, int detalleId)
    {
        try
        {
            // Buscar el documento del registro por Id
            var registroQuery = Collection.WhereEqualTo("id", registroId).Limit(1);
            var registroSnapshot = await registroQuery.GetSnapshotAsync();
            var registroDoc = registroSnapshot.Documents.FirstOrDefault();
            
            if (registroDoc == null)
                throw new InvalidOperationException($"No se encontró el registro diario con Id {registroId}");
            
            var detallesCollection = registroDoc.Reference.Collection(DETALLES_SUBCOLLECTION);
            
            // Buscar el detalle por Id
            var detalleQuery = detallesCollection.WhereEqualTo("id", detalleId).Limit(1);
            var detalleSnapshot = await detalleQuery.GetSnapshotAsync();
            var detalleDoc = detalleSnapshot.Documents.FirstOrDefault();
            
            if (detalleDoc == null)
                throw new InvalidOperationException($"No se encontró el detalle con Id {detalleId}");
            
            await detalleDoc.Reference.DeleteAsync();
            
            _logger?.LogInformation("Detalle eliminado del registro {RegistroId}: {DetalleId}", registroId, detalleId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar detalle {DetalleId} del registro {RegistroId}", detalleId, registroId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un detalle de actividad con su registro padre
    /// </summary>
    public async Task<DetalleActividad?> GetDetalleByIdAsync(int registroId, int detalleId)
    {
        try
        {
            // Buscar el documento del registro por Id
            var registroQuery = Collection.WhereEqualTo("id", registroId).Limit(1);
            var registroSnapshot = await registroQuery.GetSnapshotAsync();
            var registroDoc = registroSnapshot.Documents.FirstOrDefault();
            
            if (registroDoc == null)
                return null;
            
            var detallesCollection = registroDoc.Reference.Collection(DETALLES_SUBCOLLECTION);
            
            // Buscar el detalle por Id
            var detalleQuery = detallesCollection.WhereEqualTo("id", detalleId).Limit(1);
            var detalleSnapshot = await detalleQuery.GetSnapshotAsync();
            var detalleDoc = detalleSnapshot.Documents.FirstOrDefault();
            
            if (detalleDoc == null)
                return null;
            
            var detalle = DocumentToDetalle(detalleDoc, registroId);
            
            // Cargar registro padre
            var registro = DocumentToEntity(registroDoc);
            detalle.RegistroDiario = registro;
            
            return detalle;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener detalle {DetalleId} del registro {RegistroId}", detalleId, registroId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un detalle de actividad solo por su ID (busca en todos los registros)
    /// </summary>
    public async Task<DetalleActividad?> GetDetalleByIdAsync(int detalleId)
    {
        try
        {
            // Obtener todos los registros
            var registrosSnapshot = await Collection.GetSnapshotAsync();

            foreach (var registroDoc in registrosSnapshot.Documents)
            {
                var detallesCollection = registroDoc.Reference.Collection(DETALLES_SUBCOLLECTION);
                var detalleQuery = detallesCollection.WhereEqualTo("id", detalleId).Limit(1);
                var detalleSnapshot = await detalleQuery.GetSnapshotAsync();
                var detalleDoc = detalleSnapshot.Documents.FirstOrDefault();

                if (detalleDoc != null)
                {
                    var registro = DocumentToEntity(registroDoc);
                    var detalle = DocumentToDetalle(detalleDoc, registro.Id);
                    detalle.RegistroDiario = registro;
                    return detalle;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al buscar detalle {DetalleId} en todos los registros", detalleId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene las actividades (detalles) asociadas a un proyecto
    /// </summary>
    public async Task<IEnumerable<DetalleActividad>> GetDetallesByProyectoAsync(int proyectoId)
    {
        try
        {
            var detalles = new List<DetalleActividad>();

            // Obtener todos los registros activos
            var registrosQuery = Collection.WhereEqualTo("activo", true);
            var registrosSnapshot = await registrosQuery.GetSnapshotAsync();

            foreach (var registroDoc in registrosSnapshot.Documents)
            {
                var registro = DocumentToEntity(registroDoc);
                var detallesCollection = registroDoc.Reference.Collection(DETALLES_SUBCOLLECTION);

                // Buscar detalles con el proyectoId especificado
                var detalleQuery = detallesCollection
                    .WhereEqualTo("proyectoId", proyectoId)
                    .WhereEqualTo("activo", true);
                var detalleSnapshot = await detalleQuery.GetSnapshotAsync();

                foreach (var detalleDoc in detalleSnapshot.Documents)
                {
                    var detalle = DocumentToDetalle(detalleDoc, registro.Id);
                    detalle.RegistroDiario = registro;
                    detalles.Add(detalle);
                }
            }

            return detalles.OrderByDescending(d => d.RegistroDiario?.Fecha).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener detalles del proyecto {ProyectoId}", proyectoId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene las actividades de un proyecto en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<DetalleActividad>> GetDetallesByProyectoRangoFechasAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var detalles = new List<DetalleActividad>();

            // Obtener registros en el rango de fechas
            var registrosQuery = Collection
                .WhereGreaterThanOrEqualTo("fecha", Timestamp.FromDateTime(fechaInicio.Date.ToUniversalTime()))
                .WhereLessThanOrEqualTo("fecha", Timestamp.FromDateTime(fechaFin.Date.ToUniversalTime()))
                .WhereEqualTo("activo", true);
            var registrosSnapshot = await registrosQuery.GetSnapshotAsync();

            foreach (var registroDoc in registrosSnapshot.Documents)
            {
                var registro = DocumentToEntity(registroDoc);
                var detallesCollection = registroDoc.Reference.Collection(DETALLES_SUBCOLLECTION);

                var detalleQuery = detallesCollection
                    .WhereEqualTo("proyectoId", proyectoId)
                    .WhereEqualTo("activo", true);
                var detalleSnapshot = await detalleQuery.GetSnapshotAsync();

                foreach (var detalleDoc in detalleSnapshot.Documents)
                {
                    var detalle = DocumentToDetalle(detalleDoc, registro.Id);
                    detalle.RegistroDiario = registro;
                    detalles.Add(detalle);
                }
            }

            return detalles.OrderByDescending(d => d.RegistroDiario?.Fecha).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener detalles del proyecto {ProyectoId} en rango de fechas", proyectoId);
            throw;
        }
    }

    /// <summary>
    /// Calcula el total de horas trabajadas en un proyecto
    /// </summary>
    public async Task<decimal> GetTotalHorasByProyectoAsync(int proyectoId)
    {
        try
        {
            var detalles = await GetDetallesByProyectoAsync(proyectoId);
            return detalles.Sum(d => d.Horas);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al calcular total de horas del proyecto {ProyectoId}", proyectoId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene un resumen de horas por empleado en un proyecto
    /// </summary>
    public async Task<IEnumerable<ProyectoHorasEmpleado>> GetHorasPorEmpleadoProyectoAsync(int proyectoId)
    {
        try
        {
            var detalles = await GetDetallesByProyectoAsync(proyectoId);

            var resumen = detalles
                .Where(d => d.RegistroDiario != null)
                .GroupBy(d => d.RegistroDiario!.EmpleadoId)
                .Select(g => new ProyectoHorasEmpleado
                {
                    EmpleadoId = g.Key,
                    EmpleadoNombre = g.First().RegistroDiario?.Empleado?.NombreCompleto ?? $"Empleado {g.Key}",
                    TotalHoras = g.Sum(d => d.Horas),
                    CantidadActividades = g.Count(),
                    UltimaActividad = g.Max(d => d.RegistroDiario?.Fecha)
                })
                .OrderByDescending(x => x.TotalHoras)
                .ToList();

            return resumen;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener horas por empleado del proyecto {ProyectoId}", proyectoId);
            throw;
        }
    }

    #endregion
}
