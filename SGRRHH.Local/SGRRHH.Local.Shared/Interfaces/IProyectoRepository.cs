using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IProyectoRepository : IRepository<Proyecto>
{
    new Task<IEnumerable<Proyecto>> GetAllActiveAsync();

    Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado);

    Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm);

    Task<IEnumerable<Proyecto>> SearchAsync(string? searchTerm, EstadoProyecto? estado);

    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);

    Task<string> GetNextCodigoAsync();

    Task<IEnumerable<Proyecto>> GetProximosAVencerAsync(int diasAnticipacion = 7);

    Task<IEnumerable<Proyecto>> GetVencidosAsync();

    Task<IEnumerable<Proyecto>> GetByResponsableAsync(int empleadoId);

    Task<ProyectoEstadisticas> GetEstadisticasAsync();
}

public class ProyectoEstadisticas
{
    public int TotalProyectos { get; set; }
    public int Activos { get; set; }
    public int Suspendidos { get; set; }
    public int Finalizados { get; set; }
    public int Cancelados { get; set; }
    public int ProximosAVencer { get; set; }
    public int Vencidos { get; set; }
    public decimal PresupuestoTotal { get; set; }
}


