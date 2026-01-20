using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;


namespace SGRRHH.Local.Shared.Interfaces
{
    public interface IVacacionService
    {
        Task<int> CalcularDiasDisponiblesAsync(int empleadoId, int periodo);
        int CalcularDiasHabiles(DateTime inicio, DateTime fin);
        Task<bool> ExisteTraslapeAsync(int empleadoId, DateTime inicio, DateTime fin, int? vacacionIdExcluir = null);
        Task<List<SGRRHH.Local.Domain.Entities.Vacacion>> GetHistorialAsync(int empleadoId);
        Task<ResumenVacaciones> GetResumenAsync(int empleadoId);
    }
}
