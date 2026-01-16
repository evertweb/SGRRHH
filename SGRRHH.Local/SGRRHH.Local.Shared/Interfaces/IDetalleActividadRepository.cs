using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IDetalleActividadRepository : IRepository<DetalleActividad>
{
    Task<IEnumerable<DetalleActividad>> GetByRegistroAsync(int registroDiarioId);

    Task<IEnumerable<DetalleActividad>> GetByRegistroIdsAsync(IEnumerable<int> registroIds);

    Task<IEnumerable<DetalleActividad>> GetByProyectoAsync(int proyectoId);
}
