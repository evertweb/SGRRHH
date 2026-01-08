using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IRegistroDiarioRepository : IRepository<RegistroDiario>
{
    Task<RegistroDiario?> GetByFechaEmpleadoAsync(DateTime fecha, int empleadoId);
    
    Task<RegistroDiario?> GetByIdWithDetallesAsync(int id);
    
    Task<IEnumerable<RegistroDiario>> GetByEmpleadoRangoFechasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    
    Task<IEnumerable<RegistroDiario>> GetByFechaAsync(DateTime fecha);
    
    Task<IEnumerable<RegistroDiario>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
    
    Task<IEnumerable<RegistroDiario>> GetByEmpleadoWithDetallesAsync(int empleadoId, int? cantidad = null);
    
    Task<IEnumerable<RegistroDiario>> GetByEmpleadoMesActualAsync(int empleadoId);
    
    Task<bool> ExistsByFechaEmpleadoAsync(DateTime fecha, int empleadoId);
    
    Task<decimal> GetTotalHorasByEmpleadoRangoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    
    Task<DetalleActividad> AddDetalleAsync(int registroId, DetalleActividad detalle);
    
    Task UpdateDetalleAsync(int registroId, DetalleActividad detalle);
    
    Task UpdateDetalleAsync(DetalleActividad detalle);
    
    Task DeleteDetalleAsync(int registroId, int detalleId);
    
    Task<DetalleActividad?> GetDetalleByIdAsync(int registroId, int detalleId);
    
    Task<DetalleActividad?> GetDetalleByIdAsync(int detalleId);

    Task<IEnumerable<DetalleActividad>> GetDetallesByProyectoAsync(int proyectoId);

    Task<IEnumerable<DetalleActividad>> GetDetallesByProyectoRangoFechasAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin);

    Task<decimal> GetTotalHorasByProyectoAsync(int proyectoId);

    Task<IEnumerable<ProyectoHorasEmpleado>> GetHorasPorEmpleadoProyectoAsync(int proyectoId);
}

public class ProyectoHorasEmpleado
{
    public int EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public decimal TotalHoras { get; set; }
    public int CantidadActividades { get; set; }
    public DateTime? UltimaActividad { get; set; }
}


