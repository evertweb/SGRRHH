using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

public interface IContratoRepository : IRepository<Contrato>
{
    Task<IEnumerable<Contrato>> GetByEmpleadoIdAsync(int empleadoId);
    Task<Contrato?> GetContratoActivoByEmpleadoIdAsync(int empleadoId);
    Task<IEnumerable<Contrato>> GetContratosProximosAVencerAsync(int diasAnticipacion);
    Task<IEnumerable<Contrato>> GetContratosPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
}
