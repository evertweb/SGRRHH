using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface ITallasEmpleadoRepository : IRepository<TallasEmpleado>
{
    Task<TallasEmpleado?> GetByEmpleadoIdAsync(int empleadoId);
    Task<bool> EmpleadoTieneTallasRegistradasAsync(int empleadoId);
}
