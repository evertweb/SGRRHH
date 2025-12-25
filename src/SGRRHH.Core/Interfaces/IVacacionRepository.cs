using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Interfaces;

public interface IVacacionRepository : IRepository<Vacacion>
{
    Task<IEnumerable<Vacacion>> GetByEmpleadoIdAsync(int empleadoId);
    Task<IEnumerable<Vacacion>> GetByEmpleadoYPeriodoAsync(int empleadoId, int periodo);
    Task<IEnumerable<Vacacion>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<bool> ExisteTraslapeAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? vacacionIdExcluir = null);
    Task<IEnumerable<Vacacion>> GetByEstadoAsync(EstadoVacacion estado);
}
