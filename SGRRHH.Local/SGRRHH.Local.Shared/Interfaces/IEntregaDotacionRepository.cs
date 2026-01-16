using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IEntregaDotacionRepository : IRepository<EntregaDotacion>
{
    Task<IEnumerable<EntregaDotacion>> GetByEmpleadoIdAsync(int empleadoId);
    Task<IEnumerable<EntregaDotacion>> GetByEmpleadoIdWithDetallesAsync(int empleadoId);
    Task<EntregaDotacion?> GetByIdWithDetallesAsync(int id);
    Task<IEnumerable<EntregaDotacion>> GetProximasEntregasAsync(int diasAnticipacion = 30);
    Task<IEnumerable<EntregaDotacion>> GetEntregasPendientesAsync();
    Task<bool> EmpleadoTieneEntregaProgramadaAsync(int empleadoId, string periodo);
}
