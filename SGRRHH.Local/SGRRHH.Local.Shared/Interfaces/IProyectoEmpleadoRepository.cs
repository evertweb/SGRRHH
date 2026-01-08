using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IProyectoEmpleadoRepository : IRepository<ProyectoEmpleado>
{
    Task<IEnumerable<ProyectoEmpleado>> GetByProyectoAsync(int proyectoId);

    Task<IEnumerable<ProyectoEmpleado>> GetActiveByProyectoAsync(int proyectoId);

    Task<IEnumerable<ProyectoEmpleado>> GetByEmpleadoAsync(int empleadoId);

    Task<IEnumerable<ProyectoEmpleado>> GetActiveByEmpleadoAsync(int empleadoId);

    Task<bool> ExistsAsignacionAsync(int proyectoId, int empleadoId);

    Task<ProyectoEmpleado?> GetAsignacionAsync(int proyectoId, int empleadoId);

    Task DesasignarAsync(int proyectoId, int empleadoId);

    Task<int> GetCountByProyectoAsync(int proyectoId);
}


