using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IDocumentoEmpleadoRepository : IRepository<DocumentoEmpleado>
{
    Task<IEnumerable<DocumentoEmpleado>> GetByEmpleadoIdAsync(int empleadoId);
    
    Task<IEnumerable<DocumentoEmpleado>> GetByEmpleadoIdAndTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo);
    
    Task<IEnumerable<DocumentoEmpleado>> GetDocumentosProximosAVencerAsync(int diasAnticipacion);
    
    Task<bool> EmpleadoTieneDocumentoTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo);
    
    Task<int> GetConteoDocumentosByEmpleadoAsync(int empleadoId);
}


