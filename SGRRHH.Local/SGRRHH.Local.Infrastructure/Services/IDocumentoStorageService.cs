using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio centralizado para operaciones de almacenamiento y gestión de documentos de empleados
/// </summary>
public interface IDocumentoStorageService
{
    /// <summary>
    /// Guarda un documento escaneado o subido y lo registra en la base de datos
    /// </summary>
    /// <param name="empleadoId">ID del empleado</param>
    /// <param name="contenido">Contenido del archivo en bytes</param>
    /// <param name="nombreArchivo">Nombre del archivo original</param>
    /// <param name="tipo">Tipo de documento</param>
    /// <param name="descripcion">Descripción opcional del documento</param>
    /// <param name="fechaEmision">Fecha de emisión opcional</param>
    /// <param name="fechaVencimiento">Fecha de vencimiento opcional</param>
    /// <returns>Documento creado o null si hubo error</returns>
    Task<DocumentoEmpleado?> GuardarDocumentoAsync(
        int empleadoId,
        byte[] contenido,
        string nombreArchivo,
        TipoDocumentoEmpleado tipo,
        string? descripcion = null,
        DateTime? fechaEmision = null,
        DateTime? fechaVencimiento = null);
    
    /// <summary>
    /// Vincula un documento existente a una cuenta bancaria como certificado
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <param name="cuentaBancariaId">ID de la cuenta bancaria</param>
    /// <returns>True si se vinculó correctamente</returns>
    Task<bool> VincularCertificadoBancarioAsync(int documentoId, int cuentaBancariaId);
    
    /// <summary>
    /// Vincula un documento existente a una entrega de dotación como acta
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <param name="entregaDotacionId">ID de la entrega de dotación</param>
    /// <returns>True si se vinculó correctamente</returns>
    Task<bool> VincularActaEntregaAsync(int documentoId, int entregaDotacionId);
    
    /// <summary>
    /// Elimina un documento (soft delete)
    /// </summary>
    /// <param name="documentoId">ID del documento a eliminar</param>
    /// <returns>True si se eliminó correctamente</returns>
    Task<bool> EliminarDocumentoAsync(int documentoId);
}
