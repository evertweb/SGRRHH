using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IDetalleEntregaDotacionRepository : IRepository<DetalleEntregaDotacion>
{
    Task<IEnumerable<DetalleEntregaDotacion>> GetByEntregaIdAsync(int entregaId);
    Task DeleteByEntregaIdAsync(int entregaId);
}
