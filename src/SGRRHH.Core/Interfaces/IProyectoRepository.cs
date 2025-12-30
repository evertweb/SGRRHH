using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de proyectos
/// </summary>
public interface IProyectoRepository : IRepository<Proyecto>
{
    /// <summary>
    /// Obtiene todos los proyectos activos
    /// </summary>
    new Task<IEnumerable<Proyecto>> GetAllActiveAsync();

    /// <summary>
    /// Obtiene proyectos por estado
    /// </summary>
    Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado);

    /// <summary>
    /// Busca proyectos por término (nombre, código, cliente, ubicación)
    /// </summary>
    Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm);

    /// <summary>
    /// Busca proyectos con filtros combinados
    /// </summary>
    Task<IEnumerable<Proyecto>> SearchAsync(string? searchTerm, EstadoProyecto? estado);

    /// <summary>
    /// Verifica si existe un proyecto con el código especificado
    /// </summary>
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);

    /// <summary>
    /// Obtiene el siguiente código disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();

    /// <summary>
    /// Obtiene proyectos próximos a vencer (menos de X días para la fecha fin)
    /// </summary>
    Task<IEnumerable<Proyecto>> GetProximosAVencerAsync(int diasAnticipacion = 7);

    /// <summary>
    /// Obtiene proyectos vencidos (fecha fin pasada y estado activo)
    /// </summary>
    Task<IEnumerable<Proyecto>> GetVencidosAsync();

    /// <summary>
    /// Obtiene proyectos por responsable
    /// </summary>
    Task<IEnumerable<Proyecto>> GetByResponsableAsync(int empleadoId);

    /// <summary>
    /// Obtiene estadísticas de proyectos
    /// </summary>
    Task<ProyectoEstadisticas> GetEstadisticasAsync();
}

/// <summary>
/// Estadísticas de proyectos
/// </summary>
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
