using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

public class WebContratoRepository : WebFirestoreRepository<Contrato>, IContratoRepository
{
    public WebContratoRepository(FirebaseJsInterop firebase) 
        : base(firebase, "contratos")
    {
    }
    
    public async Task<IEnumerable<Contrato>> GetByEmpleadoIdAsync(int empleadoId)
    {
        var all = await GetAllAsync();
        return all.Where(c => c.EmpleadoId == empleadoId).OrderByDescending(c => c.FechaInicio);
    }
    
    public async Task<Contrato?> GetContratoActivoByEmpleadoIdAsync(int empleadoId)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(c => c.EmpleadoId == empleadoId && c.Activo);
    }
    
    public async Task<IEnumerable<Contrato>> GetContratosProximosAVencerAsync(int diasAnticipacion)
    {
        var all = await GetAllAsync();
        var fechaLimite = DateTime.Today.AddDays(diasAnticipacion);
        return all.Where(c => c.Activo && c.FechaFin.HasValue && c.FechaFin.Value <= fechaLimite && c.FechaFin.Value >= DateTime.Today)
                  .OrderBy(c => c.FechaFin);
    }
    
    public async Task<IEnumerable<Contrato>> GetContratosPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var all = await GetAllAsync();
        return all.Where(c => c.FechaInicio >= fechaInicio && c.FechaInicio <= fechaFin)
                  .OrderBy(c => c.FechaInicio);
    }
}
