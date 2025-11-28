using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz del servicio para gestionar documentos de empleados
/// Incluye control de permisos por rol
/// </summary>
public interface IDocumentoEmpleadoService
{
    /// <summary>
    /// Obtiene todos los documentos de un empleado
    /// </summary>
    Task<ServiceResult<IEnumerable<DocumentoEmpleado>>> GetByEmpleadoIdAsync(int empleadoId);
    
    /// <summary>
    /// Obtiene documentos de un empleado por tipo
    /// </summary>
    Task<ServiceResult<IEnumerable<DocumentoEmpleado>>> GetByEmpleadoIdAndTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo);
    
    /// <summary>
    /// Obtiene un documento por su ID
    /// </summary>
    Task<ServiceResult<DocumentoEmpleado>> GetByIdAsync(int id);
    
    /// <summary>
    /// Sube y registra un nuevo documento
    /// Requiere rol Administrador u Operador
    /// </summary>
    /// <param name="documento">Información del documento</param>
    /// <param name="archivoBytes">Contenido del archivo</param>
    /// <param name="usuarioId">ID del usuario que sube el documento</param>
    /// <param name="rolUsuario">Rol del usuario para validar permisos</param>
    Task<ServiceResult<DocumentoEmpleado>> SubirDocumentoAsync(
        DocumentoEmpleado documento, 
        byte[] archivoBytes,
        int usuarioId,
        RolUsuario rolUsuario);
    
    /// <summary>
    /// Actualiza la información de un documento (no el archivo)
    /// Requiere rol Administrador u Operador
    /// </summary>
    Task<ServiceResult<DocumentoEmpleado>> UpdateAsync(
        DocumentoEmpleado documento,
        RolUsuario rolUsuario);
    
    /// <summary>
    /// Elimina un documento
    /// Requiere rol Administrador
    /// </summary>
    Task<ServiceResult<bool>> DeleteAsync(int id, RolUsuario rolUsuario);
    
    /// <summary>
    /// Obtiene los bytes del archivo de un documento
    /// </summary>
    Task<ServiceResult<byte[]>> DescargarArchivoAsync(int documentoId);
    
    /// <summary>
    /// Obtiene documentos próximos a vencer (exámenes médicos, etc.)
    /// </summary>
    Task<ServiceResult<IEnumerable<DocumentoEmpleado>>> GetDocumentosProximosAVencerAsync(int diasAnticipacion = 30);
    
    /// <summary>
    /// Obtiene el checklist de documentos requeridos para un empleado
    /// </summary>
    Task<ServiceResult<IEnumerable<DocumentoChecklistItem>>> GetChecklistDocumentosAsync(int empleadoId);
    
    /// <summary>
    /// Verifica si el usuario tiene permisos para gestionar documentos
    /// </summary>
    bool PuedeGestionarDocumentos(RolUsuario rol);
    
    /// <summary>
    /// Verifica si el usuario puede eliminar documentos
    /// </summary>
    bool PuedeEliminarDocumentos(RolUsuario rol);
}

/// <summary>
/// Item del checklist de documentos
/// </summary>
public class DocumentoChecklistItem
{
    /// <summary>
    /// Tipo de documento
    /// </summary>
    public TipoDocumentoEmpleado Tipo { get; set; }
    
    /// <summary>
    /// Nombre legible del tipo
    /// </summary>
    public string NombreTipo { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si es un documento requerido/obligatorio
    /// </summary>
    public bool EsRequerido { get; set; }
    
    /// <summary>
    /// Indica si el empleado ya tiene este documento
    /// </summary>
    public bool TieneDocumento { get; set; }
    
    /// <summary>
    /// Documento existente (si aplica)
    /// </summary>
    public DocumentoEmpleado? Documento { get; set; }
    
    /// <summary>
    /// Indica si el documento está vencido
    /// </summary>
    public bool EstaVencido { get; set; }
}
