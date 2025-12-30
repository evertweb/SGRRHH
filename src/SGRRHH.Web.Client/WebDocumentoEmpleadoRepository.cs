using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

/// <summary>
/// Repositorio Web para documentos de empleados usando Firestore JS
/// </summary>
public class WebDocumentoEmpleadoRepository : WebFirestoreRepository<DocumentoEmpleado>, IDocumentoEmpleadoRepository
{
    public WebDocumentoEmpleadoRepository(FirebaseJsInterop firebase) 
        : base(firebase, "documentosEmpleado")
    {
    }

    public async Task<IEnumerable<DocumentoEmpleado>> GetByEmpleadoIdAsync(int empleadoId)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "empleadoId", "==", empleadoId);
        return results.Select(MapToEntity).ToList();
    }

    public async Task<IEnumerable<DocumentoEmpleado>> GetByEmpleadoIdAndTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo)
    {
        // Firestore no soporta múltiples filtros en Web sin índice, obtenemos por empleado y filtramos
        var byEmpleado = await GetByEmpleadoIdAsync(empleadoId);
        return byEmpleado.Where(d => d.TipoDocumento == tipo).ToList();
    }

    public async Task<IEnumerable<DocumentoEmpleado>> GetDocumentosProximosAVencerAsync(int diasAnticipacion)
    {
        var all = await GetAllAsync();
        var fechaLimite = DateTime.Today.AddDays(diasAnticipacion);
        return all.Where(d => d.FechaVencimiento.HasValue && 
                              d.FechaVencimiento.Value <= fechaLimite && 
                              d.FechaVencimiento.Value >= DateTime.Today)
                  .ToList();
    }

    public async Task<bool> EmpleadoTieneDocumentoTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo)
    {
        var docs = await GetByEmpleadoIdAndTipoAsync(empleadoId, tipo);
        return docs.Any();
    }

    public async Task<int> GetConteoDocumentosByEmpleadoAsync(int empleadoId)
    {
        var docs = await GetByEmpleadoIdAsync(empleadoId);
        return docs.Count();
    }
}
