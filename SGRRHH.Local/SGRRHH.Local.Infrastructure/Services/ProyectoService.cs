using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services
{
    public interface IProyectoService
    {
        Task<Proyecto?> GetByIdAsync(int id);
        Task<IEnumerable<ProyectoEmpleado>> GetCuadrillaAsync(int proyectoId);
        Task<IEnumerable<ProyectoEmpleadoHistorial>> GetHistorialAsync(int proyectoId);
        Task AsignarEmpleadoAsync(ProyectoEmpleado asignacion);
        Task DesasignarEmpleadoAsync(int proyectoId, int empleadoId, string motivo);
        Task AsignarMultiplesAsync(int proyectoId, IEnumerable<int> empleadosIds, RolProyectoForestal? rol);
        Task DesasignarMultiplesAsync(int proyectoId, IEnumerable<int> empleadosIds, string motivo);
    }

    public class ProyectoService : IProyectoService
    {
        private readonly IProyectoRepository _proyectoRepository;
        private readonly IProyectoEmpleadoRepository _proyectoEmpleadoRepository;

        public ProyectoService(
            IProyectoRepository proyectoRepository,
            IProyectoEmpleadoRepository proyectoEmpleadoRepository)
        {
            _proyectoRepository = proyectoRepository;
            _proyectoEmpleadoRepository = proyectoEmpleadoRepository;
        }

        public async Task<Proyecto?> GetByIdAsync(int id)
        {
            return await _proyectoRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ProyectoEmpleado>> GetCuadrillaAsync(int proyectoId)
        {
            return await _proyectoEmpleadoRepository.GetActiveByProyectoWithEmpleadoAsync(proyectoId);
        }

        public async Task<IEnumerable<ProyectoEmpleadoHistorial>> GetHistorialAsync(int proyectoId)
        {
            return await _proyectoEmpleadoRepository.GetHistorialAsync(proyectoId);
        }

        public async Task AsignarEmpleadoAsync(ProyectoEmpleado asignacion)
        {
            if (await _proyectoEmpleadoRepository.ExistsAsignacionActivaAsync(asignacion.ProyectoId, asignacion.EmpleadoId))
            {
                throw new InvalidOperationException("El empleado ya está asignado a este proyecto.");
            }

            // Sincronizar Rol texto
            if (asignacion.RolEnum.HasValue)
            {
                asignacion.Rol = asignacion.RolEnum.Value.ToString();
            }

            await _proyectoEmpleadoRepository.AddAsync(asignacion);
        }

        public async Task DesasignarEmpleadoAsync(int proyectoId, int empleadoId, string motivo)
        {
            await _proyectoEmpleadoRepository.DesasignarAsync(proyectoId, empleadoId, motivo);
        }

        public async Task AsignarMultiplesAsync(int proyectoId, IEnumerable<int> empleadosIds, RolProyectoForestal? rol)
        {
             await _proyectoEmpleadoRepository.AsignarMultiplesAsync(proyectoId, empleadosIds, rol);
        }

        public async Task DesasignarMultiplesAsync(int proyectoId, IEnumerable<int> empleadosIds, string motivo)
        {
            await _proyectoEmpleadoRepository.DesasignarMultiplesAsync(proyectoId, empleadosIds, motivo);
        }
    }
}
