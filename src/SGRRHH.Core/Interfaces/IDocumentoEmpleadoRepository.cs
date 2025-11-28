using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz del repositorio para documentos de empleados
/// </summary>
public interface IDocumentoEmpleadoRepository : IRepository<DocumentoEmpleado>
{
    /// <summary>
    /// Obtiene todos los documentos de un empleado
    /// </summary>
    Task<IEnumerable<DocumentoEmpleado>> GetByEmpleadoIdAsync(int empleadoId);
    
    /// <summary>
    /// Obtiene documentos de un empleado por tipo
    /// </summary>
    Task<IEnumerable<DocumentoEmpleado>> GetByEmpleadoIdAndTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo);
    
    /// <summary>
    /// Obtiene documentos próximos a vencer
    /// </summary>
    Task<IEnumerable<DocumentoEmpleado>> GetDocumentosProximosAVencerAsync(int diasAnticipacion);
    
    /// <summary>
    /// Verifica si un empleado tiene un tipo específico de documento
    /// </summary>
    Task<bool> EmpleadoTieneDocumentoTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo);
    
    /// <summary>
    /// Obtiene el conteo de documentos por empleado
    /// </summary>
    Task<int> GetConteoDocumentosByEmpleadoAsync(int empleadoId);
}
