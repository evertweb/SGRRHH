using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de ProyectoEmpleado para Firestore.
/// Colección: "proyecto_empleados"
/// </summary>
public class ProyectoEmpleadoFirestoreRepository : FirestoreRepository<ProyectoEmpleado>, IProyectoEmpleadoRepository
{
    private const string COLLECTION_NAME = "proyecto_empleados";

    public ProyectoEmpleadoFirestoreRepository(
        FirebaseInitializer firebase,
        ILogger<ProyectoEmpleadoFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
    }

    #region Entity <-> Document Mapping

    protected override Dictionary<string, object?> EntityToDocument(ProyectoEmpleado entity)
    {
        var doc = base.EntityToDocument(entity);
        doc["proyectoId"] = entity.ProyectoId;
        doc["empleadoId"] = entity.EmpleadoId;
        doc["fechaAsignacion"] = Timestamp.FromDateTime(entity.FechaAsignacion.ToUniversalTime());
        doc["fechaDesasignacion"] = entity.FechaDesasignacion.HasValue
            ? Timestamp.FromDateTime(entity.FechaDesasignacion.Value.ToUniversalTime())
            : null;
        doc["rol"] = entity.Rol;
        doc["observaciones"] = entity.Observaciones;
        return doc;
    }

    protected override ProyectoEmpleado DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);

        if (document.TryGetValue<int>("proyectoId", out var proyectoId))
            entity.ProyectoId = proyectoId;

        if (document.TryGetValue<int>("empleadoId", out var empleadoId))
            entity.EmpleadoId = empleadoId;

        if (document.TryGetValue<Timestamp>("fechaAsignacion", out var fechaAsignacion))
            entity.FechaAsignacion = fechaAsignacion.ToDateTime().ToLocalTime();

        if (document.TryGetValue<Timestamp?>("fechaDesasignacion", out var fechaDesasignacion) && fechaDesasignacion.HasValue)
            entity.FechaDesasignacion = fechaDesasignacion.Value.ToDateTime().ToLocalTime();

        if (document.TryGetValue<string>("rol", out var rol))
            entity.Rol = rol;

        if (document.TryGetValue<string>("observaciones", out var observaciones))
            entity.Observaciones = observaciones;

        return entity;
    }

    #endregion

    #region IProyectoEmpleadoRepository Implementation

    /// <summary>
    /// Obtiene todas las asignaciones de un proyecto
    /// </summary>
    public async Task<IEnumerable<ProyectoEmpleado>> GetByProyectoAsync(int proyectoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("proyectoId", proyectoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(pe => pe.FechaAsignacion)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener asignaciones del proyecto: {ProyectoId}", proyectoId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene todas las asignaciones activas de un proyecto
    /// </summary>
    public async Task<IEnumerable<ProyectoEmpleado>> GetActiveByProyectoAsync(int proyectoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("proyectoId", proyectoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(pe => !pe.FechaDesasignacion.HasValue)
                .OrderBy(pe => pe.FechaAsignacion)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener asignaciones activas del proyecto: {ProyectoId}", proyectoId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene todos los proyectos de un empleado
    /// </summary>
    public async Task<IEnumerable<ProyectoEmpleado>> GetByEmpleadoAsync(int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(pe => pe.FechaAsignacion)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener proyectos del empleado: {EmpleadoId}", empleadoId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene todos los proyectos activos de un empleado
    /// </summary>
    public async Task<IEnumerable<ProyectoEmpleado>> GetActiveByEmpleadoAsync(int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(DocumentToEntity)
                .Where(pe => !pe.FechaDesasignacion.HasValue)
                .OrderByDescending(pe => pe.FechaAsignacion)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener proyectos activos del empleado: {EmpleadoId}", empleadoId);
            throw;
        }
    }

    /// <summary>
    /// Verifica si un empleado está asignado a un proyecto
    /// </summary>
    public async Task<bool> ExistsAsignacionAsync(int proyectoId, int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("proyectoId", proyectoId)
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(DocumentToEntity)
                .Any(pe => !pe.FechaDesasignacion.HasValue);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar asignación: Proyecto={ProyectoId}, Empleado={EmpleadoId}",
                proyectoId, empleadoId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene la asignación de un empleado a un proyecto
    /// </summary>
    public async Task<ProyectoEmpleado?> GetAsignacionAsync(int proyectoId, int empleadoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("proyectoId", proyectoId)
                .WhereEqualTo("empleadoId", empleadoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(DocumentToEntity)
                .FirstOrDefault(pe => !pe.FechaDesasignacion.HasValue);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener asignación: Proyecto={ProyectoId}, Empleado={EmpleadoId}",
                proyectoId, empleadoId);
            throw;
        }
    }

    /// <summary>
    /// Desasigna un empleado de un proyecto (soft delete)
    /// </summary>
    public async Task DesasignarAsync(int proyectoId, int empleadoId)
    {
        try
        {
            var asignacion = await GetAsignacionAsync(proyectoId, empleadoId);
            if (asignacion == null)
            {
                _logger?.LogWarning("No se encontró asignación activa: Proyecto={ProyectoId}, Empleado={EmpleadoId}",
                    proyectoId, empleadoId);
                return;
            }

            asignacion.FechaDesasignacion = DateTime.Now;
            await UpdateAsync(asignacion);

            _logger?.LogInformation("Empleado {EmpleadoId} desasignado del proyecto {ProyectoId}",
                empleadoId, proyectoId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al desasignar: Proyecto={ProyectoId}, Empleado={EmpleadoId}",
                proyectoId, empleadoId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene la cantidad de empleados asignados a un proyecto
    /// </summary>
    public async Task<int> GetCountByProyectoAsync(int proyectoId)
    {
        try
        {
            var asignaciones = await GetActiveByProyectoAsync(proyectoId);
            return asignaciones.Count();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al contar empleados del proyecto: {ProyectoId}", proyectoId);
            throw;
        }
    }

    #endregion
}
