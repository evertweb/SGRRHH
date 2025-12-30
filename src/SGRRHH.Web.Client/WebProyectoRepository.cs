using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

public class WebProyectoRepository : WebFirestoreRepository<Proyecto>, IProyectoRepository
{
    public WebProyectoRepository(FirebaseJsInterop firebase)
        : base(firebase, "proyectos")
    {
    }

    public new async Task<IEnumerable<Proyecto>> GetAllActiveAsync()
    {
        var all = await GetAllAsync();
        return all.Where(p => p.Activo).OrderBy(p => p.Nombre);
    }

    public async Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado)
    {
        var all = await GetAllAsync();
        return all.Where(p => p.Activo && p.Estado == estado).OrderBy(p => p.Nombre);
    }

    public async Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm)
    {
        return await SearchAsync(searchTerm, null);
    }

    public async Task<IEnumerable<Proyecto>> SearchAsync(string? searchTerm, EstadoProyecto? estado)
    {
        var all = await GetAllAsync();
        var proyectos = all.Where(p => p.Activo);

        // Filtrar por estado si se especifica
        if (estado.HasValue)
        {
            proyectos = proyectos.Where(p => p.Estado == estado.Value);
        }

        // Filtrar por término de búsqueda
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            proyectos = proyectos.Where(p =>
                (p.Nombre?.ToLower().Contains(term) ?? false) ||
                (p.Codigo?.ToLower().Contains(term) ?? false) ||
                (p.Cliente?.ToLower().Contains(term) ?? false) ||
                (p.Ubicacion?.ToLower().Contains(term) ?? false) ||
                (p.Descripcion?.ToLower().Contains(term) ?? false));
        }

        return proyectos.OrderBy(p => p.Nombre);
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var all = await GetAllAsync();
        return all.Any(p => p.Codigo == codigo && p.Id != excludeId);
    }

    public async Task<string> GetNextCodigoAsync()
    {
        var all = await GetAllAsync();
        var maxCode = all.Where(p => !string.IsNullOrEmpty(p.Codigo) && p.Codigo.StartsWith("PRY"))
                         .Select(p => int.TryParse(p.Codigo.Substring(4), out var n) ? n : 0)
                         .DefaultIfEmpty(0)
                         .Max();
        return $"PRY-{(maxCode + 1):D4}";
    }

    public async Task<IEnumerable<Proyecto>> GetProximosAVencerAsync(int diasAnticipacion = 7)
    {
        var all = await GetAllAsync();
        var hoy = DateTime.Today;
        var fechaLimite = hoy.AddDays(diasAnticipacion);

        return all.Where(p =>
                p.Activo &&
                p.Estado == EstadoProyecto.Activo &&
                p.FechaFin.HasValue &&
                p.FechaFin.Value > hoy &&
                p.FechaFin.Value <= fechaLimite)
            .OrderBy(p => p.FechaFin);
    }

    public async Task<IEnumerable<Proyecto>> GetVencidosAsync()
    {
        var all = await GetAllAsync();
        var hoy = DateTime.Today;

        return all.Where(p =>
                p.Activo &&
                p.Estado == EstadoProyecto.Activo &&
                p.FechaFin.HasValue &&
                p.FechaFin.Value < hoy)
            .OrderBy(p => p.FechaFin);
    }

    public async Task<IEnumerable<Proyecto>> GetByResponsableAsync(int empleadoId)
    {
        var all = await GetAllAsync();
        return all.Where(p => p.Activo && p.ResponsableId == empleadoId)
                  .OrderBy(p => p.Nombre);
    }

    public async Task<ProyectoEstadisticas> GetEstadisticasAsync()
    {
        var all = await GetAllAsync();
        var proyectos = all.Where(p => p.Activo).ToList();
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
}
