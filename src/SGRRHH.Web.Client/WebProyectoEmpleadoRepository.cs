using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

/// <summary>
/// Implementación web del repositorio de ProyectoEmpleado para Blazor WASM.
/// </summary>
public class WebProyectoEmpleadoRepository : WebFirestoreRepository<ProyectoEmpleado>, IProyectoEmpleadoRepository
{
    public WebProyectoEmpleadoRepository(FirebaseJsInterop firebase)
        : base(firebase, "proyecto_empleados")
    {
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetByProyectoAsync(int proyectoId)
    {
        var all = await GetAllAsync();
        return all.Where(pe => pe.Activo && pe.ProyectoId == proyectoId)
                  .OrderBy(pe => pe.FechaAsignacion);
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetActiveByProyectoAsync(int proyectoId)
    {
        var all = await GetAllAsync();
        return all.Where(pe => pe.Activo && pe.ProyectoId == proyectoId && !pe.FechaDesasignacion.HasValue)
                  .OrderBy(pe => pe.FechaAsignacion);
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetByEmpleadoAsync(int empleadoId)
    {
        var all = await GetAllAsync();
        return all.Where(pe => pe.Activo && pe.EmpleadoId == empleadoId)
                  .OrderByDescending(pe => pe.FechaAsignacion);
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetActiveByEmpleadoAsync(int empleadoId)
    {
        var all = await GetAllAsync();
        return all.Where(pe => pe.Activo && pe.EmpleadoId == empleadoId && !pe.FechaDesasignacion.HasValue)
                  .OrderByDescending(pe => pe.FechaAsignacion);
    }

    public async Task<bool> ExistsAsignacionAsync(int proyectoId, int empleadoId)
    {
        var all = await GetAllAsync();
        return all.Any(pe => pe.Activo &&
                            pe.ProyectoId == proyectoId &&
                            pe.EmpleadoId == empleadoId &&
                            !pe.FechaDesasignacion.HasValue);
    }

    public async Task<ProyectoEmpleado?> GetAsignacionAsync(int proyectoId, int empleadoId)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(pe => pe.Activo &&
                                        pe.ProyectoId == proyectoId &&
                                        pe.EmpleadoId == empleadoId &&
                                        !pe.FechaDesasignacion.HasValue);
    }

    public async Task DesasignarAsync(int proyectoId, int empleadoId)
    {
        var asignacion = await GetAsignacionAsync(proyectoId, empleadoId);
        if (asignacion == null) return;

        asignacion.FechaDesasignacion = DateTime.Now;
        await UpdateAsync(asignacion);
    }

    public async Task<int> GetCountByProyectoAsync(int proyectoId)
    {
        var asignaciones = await GetActiveByProyectoAsync(proyectoId);
        return asignaciones.Count();
    }
}
