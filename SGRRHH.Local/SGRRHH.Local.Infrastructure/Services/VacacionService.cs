using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services
{
    public class VacacionService : IVacacionService
    {
        private readonly IVacacionRepository _vacacionRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly ILogger<VacacionService> _logger;

        public VacacionService(
            IVacacionRepository vacacionRepository,
            IEmpleadoRepository empleadoRepository,
            ILogger<VacacionService> logger)
        {
            _vacacionRepository = vacacionRepository;
            _empleadoRepository = empleadoRepository;
            _logger = logger;
        }

        public async Task<int> CalcularDiasDisponiblesAsync(int empleadoId, int periodo)
        {
            try
            {
                // Obtener empleado
                var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
                if (empleado == null) return 0;

                // Calcular años de antigüedad
                var antiguedad = DateTime.Now.Year - empleado.FechaIngreso.Year;

                // Días base por año (típicamente 15 días, puede variar según legislación)
                int diasBase = 15;

                // Días adicionales por antigüedad (1 día adicional cada 5 años, hasta un máximo)
                int diasAdicionales = Math.Min((antiguedad / 5), 5); // Máximo 5 días adicionales (20 días total)

                int diasTotales = diasBase + diasAdicionales;

                // Obtener vacaciones ya tomadas o pendientes para el período
                var vacacionesPeriodo = await _vacacionRepository.GetByEmpleadoYPeriodoAsync(empleadoId, periodo);

                int diasUsados = vacacionesPeriodo
                    .Where(v => v.Estado != EstadoVacacion.Rechazada &&
                               v.Estado != EstadoVacacion.Cancelada)
                    .Sum(v => v.DiasTomados);

                int diasDisponibles = diasTotales - diasUsados;

                _logger.LogInformation($"Empleado {empleadoId}: {diasTotales} días totales, {diasUsados} días usados, {diasDisponibles} días disponibles para período {periodo}");

                return diasDisponibles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al calcular días disponibles para empleado {empleadoId}");
                throw;
            }
        }

        public int CalcularDiasHabiles(DateTime inicio, DateTime fin)
        {
            if (fin < inicio) return 0;

            int dias = 0;
            var fecha = inicio;

            while (fecha <= fin)
            {
                // Solo contar días de lunes a viernes
                if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
                {
                    dias++;
                }
                fecha = fecha.AddDays(1);
            }

            return dias;
        }

        public async Task<bool> ExisteTraslapeAsync(int empleadoId, DateTime inicio, DateTime fin, int? vacacionIdExcluir = null)
        {
            return await _vacacionRepository.ExisteTraslapeAsync(empleadoId, inicio, fin, vacacionIdExcluir);
        }

        public async Task<List<Vacacion>> GetHistorialAsync(int empleadoId)
        {
            try
            {
                var historial = await _vacacionRepository.GetByEmpleadoIdAsync(empleadoId);
                return historial
                    .OrderByDescending(v => v.FechaInicio)
                    .Take(10) // Últimas 10 vacaciones
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar historial de vacaciones para empleado {empleadoId}");
                throw;
            }
        }

        public async Task<ResumenVacaciones> GetResumenAsync(int empleadoId)
        {
            try
            {
                var resumen = await _vacacionRepository.GetResumenVacacionesAsync(empleadoId);
                return resumen ?? new ResumenVacaciones();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar resumen de vacaciones para empleado {empleadoId}");
                throw;
            }
        }
    }
}
