using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Proyectos para Firestore.
/// Colección: "proyectos"
/// Incluye cache en memoria para reducir round-trips.
/// </summary>
public class ProyectoFirestoreRepository : FirestoreRepository<Proyecto>, IProyectoRepository
{
    private const string COLLECTION_NAME = "proyectos";
    private const string CODE_PREFIX = "PRY-";
    private const string CACHE_KEY_ALL_ACTIVE = "proyectos_all_active";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    private readonly ICacheService? _cache;

    public ProyectoFirestoreRepository(
        FirebaseInitializer firebase,
        ICacheService? cache = null,
        ILogger<ProyectoFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
        _cache = cache;
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(Proyecto entity)
    {
        var doc = base.EntityToDocument(entity);
        doc["codigo"] = entity.Codigo;
        doc["nombre"] = entity.Nombre;
        doc["descripcion"] = entity.Descripcion;
        doc["cliente"] = entity.Cliente;
        doc["ubicacion"] = entity.Ubicacion;
        doc["presupuesto"] = entity.Presupuesto;
        doc["progreso"] = entity.Progreso;
        doc["responsableId"] = entity.ResponsableId;
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
        doc["ubicacionLower"] = entity.Ubicacion?.ToLowerInvariant();
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

        if (document.TryGetValue<string>("ubicacion", out var ubicacion))
            entity.Ubicacion = ubicacion;

        if (document.TryGetValue<double>("presupuesto", out var presupuesto))
            entity.Presupuesto = (decimal)presupuesto;

        if (document.TryGetValue<int>("progreso", out var progreso))
            entity.Progreso = progreso;

        if (document.TryGetValue<int>("responsableId", out var responsableId))
            entity.ResponsableId = responsableId;

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
    /// Obtiene todos los proyectos activos ordenados por nombre.
    /// Usa cache en memoria con expiración de 10 minutos.
    /// </summary>
    public override async Task<IEnumerable<Proyecto>> GetAllActiveAsync()
    {
        try
        {
            if (_cache != null)
            {
                return await _cache.GetOrCreateAsync(
                    CACHE_KEY_ALL_ACTIVE,
                    FetchAllActiveFromFirestoreAsync,
                    CacheExpiration);
            }

            return await FetchAllActiveFromFirestoreAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener proyectos activos");
            throw;
        }
    }

    private async Task<IEnumerable<Proyecto>> FetchAllActiveFromFirestoreAsync()
    {
        var query = Collection.WhereEqualTo("activo", true);
        var snapshot = await query.GetSnapshotAsync();

        return snapshot.Documents
            .Select(DocumentToEntity)
            .OrderBy(p => p.Nombre)
            .ToList();
    }

    /// <summary>
    /// Invalida el cache de proyectos.
    /// </summary>
    public void InvalidateCache()
    {
        _cache?.InvalidateByPrefix("proyectos");
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
        return await SearchAsync(searchTerm, null);
    }

    /// <summary>
    /// Busca proyectos con filtros combinados
    /// </summary>
    public async Task<IEnumerable<Proyecto>> SearchAsync(string? searchTerm, EstadoProyecto? estado)
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();

            var proyectos = snapshot.Documents.Select(DocumentToEntity);

            // Filtrar por estado si se especifica
            if (estado.HasValue)
            {
                proyectos = proyectos.Where(p => p.Estado == estado.Value);
            }

            // Filtrar por término de búsqueda
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLowerInvariant().Trim();
                proyectos = proyectos.Where(p =>
                    (p.Codigo?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Nombre?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Cliente?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Ubicacion?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Descripcion?.ToLowerInvariant().Contains(term) ?? false));
            }

            return proyectos.OrderBy(p => p.Nombre).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al buscar proyectos con término: {SearchTerm}, estado: {Estado}", searchTerm, estado);
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
        return await GetNextCodigoAsync(CODE_PREFIX, 4);
    }

    /// <summary>
    /// Obtiene proyectos próximos a vencer
    /// </summary>
    public async Task<IEnumerable<Proyecto>> GetProximosAVencerAsync(int diasAnticipacion = 7)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("activo", true)
                .WhereEqualTo("estado", (int)EstadoProyecto.Activo);
            var snapshot = await query.GetSnapshotAsync();

            var hoy = DateTime.Today;
            var fechaLimite = hoy.AddDays(diasAnticipacion);

            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(p => p.FechaFin.HasValue &&
                           p.FechaFin.Value > hoy &&
                           p.FechaFin.Value <= fechaLimite)
                .OrderBy(p => p.FechaFin)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener proyectos próximos a vencer");
            throw;
        }
    }

    /// <summary>
    /// Obtiene proyectos vencidos
    /// </summary>
    public async Task<IEnumerable<Proyecto>> GetVencidosAsync()
    {
        try
        {
            var query = Collection
                .WhereEqualTo("activo", true)
                .WhereEqualTo("estado", (int)EstadoProyecto.Activo);
            var snapshot = await query.GetSnapshotAsync();

            var hoy = DateTime.Today;

            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(p => p.FechaFin.HasValue && p.FechaFin.Value < hoy)
                .OrderBy(p => p.FechaFin)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener proyectos vencidos");
            throw;
        }
    }

    /// <summary>
    /// Obtiene proyectos por responsable
    /// </summary>
    public async Task<IEnumerable<Proyecto>> GetByResponsableAsync(int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("activo", true)
                .WhereEqualTo("responsableId", empleadoId);
            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(p => p.Nombre)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener proyectos por responsable: {EmpleadoId}", empleadoId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene estadísticas de proyectos
    /// </summary>
    public async Task<ProyectoEstadisticas> GetEstadisticasAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();

            var proyectos = snapshot.Documents.Select(DocumentToEntity).ToList();
            var hoy = DateTime.Today;
            var fechaLimite = hoy.AddDays(7);

            return new ProyectoEstadisticas
            {
                TotalProyectos = proyectos.Count,
                Activos = proyectos.Count(p => p.Estado == EstadoProyecto.Activo),
                Suspendidos = proyectos.Count(p => p.Estado == EstadoProyecto.Suspendido),
                Finalizados = proyectos.Count(p => p.Estado == EstadoProyecto.Finalizado),
                Cancelados = proyectos.Count(p => p.Estado == EstadoProyecto.Cancelado),
                ProximosAVencer = proyectos.Count(p =>
                    p.Estado == EstadoProyecto.Activo &&
                    p.FechaFin.HasValue &&
                    p.FechaFin.Value > hoy &&
                    p.FechaFin.Value <= fechaLimite),
                Vencidos = proyectos.Count(p =>
                    p.Estado == EstadoProyecto.Activo &&
                    p.FechaFin.HasValue &&
                    p.FechaFin.Value < hoy),
                PresupuestoTotal = proyectos
                    .Where(p => p.Presupuesto.HasValue)
                    .Sum(p => p.Presupuesto!.Value)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener estadísticas de proyectos");
            throw;
        }
    }

    #endregion
}
