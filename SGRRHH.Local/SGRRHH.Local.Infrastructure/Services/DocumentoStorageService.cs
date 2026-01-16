using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Helpers;
using SGRRHH.Local.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio centralizado para operaciones de almacenamiento y gestión de documentos de empleados
/// </summary>
public class DocumentoStorageService : IDocumentoStorageService
{
    private readonly IDocumentoEmpleadoRepository _documentoRepo;
    private readonly ICuentaBancariaRepository _cuentaRepo;
    private readonly IEntregaDotacionRepository _entregaRepo;
    private readonly ILocalStorageService _storageService;
    private readonly IAuthService _authService;
    private readonly ILogger<DocumentoStorageService> _logger;
    
    public DocumentoStorageService(
        IDocumentoEmpleadoRepository documentoRepo,
        ICuentaBancariaRepository cuentaRepo,
        IEntregaDotacionRepository entregaRepo,
        ILocalStorageService storageService,
        IAuthService authService,
        ILogger<DocumentoStorageService> logger)
    {
        _documentoRepo = documentoRepo;
        _cuentaRepo = cuentaRepo;
        _entregaRepo = entregaRepo;
        _storageService = storageService;
        _authService = authService;
        _logger = logger;
    }
    
    public async Task<DocumentoEmpleado?> GuardarDocumentoAsync(
        int empleadoId,
        byte[] contenido,
        string nombreArchivo,
        TipoDocumentoEmpleado tipo,
        string? descripcion = null,
        DateTime? fechaEmision = null,
        DateTime? fechaVencimiento = null)
    {
        // Validaciones
        if (empleadoId <= 0)
        {
            _logger.LogWarning("Intento de guardar documento con empleadoId inválido: {EmpleadoId}", empleadoId);
            return null;
        }
        
        if (contenido == null || contenido.Length == 0)
        {
            _logger.LogWarning("Intento de guardar documento sin contenido para empleado {EmpleadoId}", empleadoId);
            return null;
        }
        
        if (string.IsNullOrWhiteSpace(nombreArchivo))
        {
            _logger.LogWarning("Intento de guardar documento sin nombre de archivo para empleado {EmpleadoId}", empleadoId);
            return null;
        }
        
        try
        {
            // Obtener nombre legible del tipo de documento
            var tipoNombre = DocumentHelper.GetTipoDocumentoNombre(tipo);
            
            // Guardar archivo físico
            var storageResult = await _storageService.SaveDocumentoEmpleadoAsync(
                empleadoId,
                tipoNombre,
                contenido,
                nombreArchivo);
            
            if (!storageResult.IsSuccess)
            {
                _logger.LogError("Error guardando archivo físico para empleado {EmpleadoId}: {Error}", 
                    empleadoId, storageResult.Error);
                return null;
            }
            
            // Determinar tipo MIME basado en extensión del archivo
            var tipoMime = ObtenerTipoMime(nombreArchivo);
            
            // Crear registro en base de datos
            var documento = new DocumentoEmpleado
            {
                EmpleadoId = empleadoId,
                TipoDocumento = tipo,
                Nombre = tipoNombre,
                Descripcion = descripcion,
                NombreArchivoOriginal = nombreArchivo,
                TipoMime = tipoMime,
                TamanoArchivo = contenido.Length,
                FechaEmision = fechaEmision,
                FechaVencimiento = fechaVencimiento,
                SubidoPorNombre = _authService.CurrentUser?.NombreCompleto,
                SubidoPorUsuarioId = _authService.CurrentUser?.Id,
                ArchivoPath = storageResult.Value ?? string.Empty,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            
            var documentoGuardado = await _documentoRepo.AddAsync(documento);
            
            _logger.LogInformation(
                "Documento guardado exitosamente: {Tipo} (ID: {DocumentoId}) para empleado {EmpleadoId}",
                tipoNombre, documentoGuardado.Id, empleadoId);
            
            return documentoGuardado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error guardando documento {Tipo} para empleado {EmpleadoId}", 
                tipo, empleadoId);
            return null;
        }
    }
    
    public async Task<bool> VincularCertificadoBancarioAsync(int documentoId, int cuentaBancariaId)
    {
        try
        {
            // Validar que el documento existe
            var documento = await _documentoRepo.GetByIdAsync(documentoId);
            if (documento == null || !documento.Activo)
            {
                _logger.LogWarning(
                    "Intento de vincular certificado bancario inexistente o inactivo: DocumentoId={DocumentoId}, CuentaId={CuentaId}",
                    documentoId, cuentaBancariaId);
                return false;
            }
            
            // Validar que el tipo de documento es correcto
            if (documento.TipoDocumento != TipoDocumentoEmpleado.CertificadoBancario)
            {
                _logger.LogWarning(
                    "Intento de vincular documento que no es certificado bancario: DocumentoId={DocumentoId}, Tipo={Tipo}",
                    documentoId, documento.TipoDocumento);
                return false;
            }
            
            // Obtener cuenta bancaria
            var cuenta = await _cuentaRepo.GetByIdAsync(cuentaBancariaId);
            if (cuenta == null || !cuenta.Activo)
            {
                _logger.LogWarning(
                    "Intento de vincular certificado a cuenta bancaria inexistente o inactiva: CuentaId={CuentaId}",
                    cuentaBancariaId);
                return false;
            }
            
            // Vincular documento a cuenta
            cuenta.DocumentoCertificacionId = documentoId;
            await _cuentaRepo.UpdateAsync(cuenta);
            
            _logger.LogInformation(
                "Certificado bancario {DocumentoId} vinculado exitosamente a cuenta {CuentaId}",
                documentoId, cuentaBancariaId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error vinculando certificado bancario {DocumentoId} a cuenta {CuentaId}",
                documentoId, cuentaBancariaId);
            return false;
        }
    }
    
    public async Task<bool> VincularActaEntregaAsync(int documentoId, int entregaDotacionId)
    {
        try
        {
            // Validar que el documento existe
            var documento = await _documentoRepo.GetByIdAsync(documentoId);
            if (documento == null || !documento.Activo)
            {
                _logger.LogWarning(
                    "Intento de vincular acta de entrega inexistente o inactiva: DocumentoId={DocumentoId}, EntregaId={EntregaId}",
                    documentoId, entregaDotacionId);
                return false;
            }
            
            // Validar que el tipo de documento es correcto
            if (documento.TipoDocumento != TipoDocumentoEmpleado.ActaEntregaDotacion)
            {
                _logger.LogWarning(
                    "Intento de vincular documento que no es acta de entrega: DocumentoId={DocumentoId}, Tipo={Tipo}",
                    documentoId, documento.TipoDocumento);
                return false;
            }
            
            // Obtener entrega de dotación
            var entrega = await _entregaRepo.GetByIdAsync(entregaDotacionId);
            if (entrega == null || !entrega.Activo)
            {
                _logger.LogWarning(
                    "Intento de vincular acta a entrega de dotación inexistente o inactiva: EntregaId={EntregaId}",
                    entregaDotacionId);
                return false;
            }
            
            // Vincular documento a entrega
            entrega.DocumentoActaId = documentoId;
            await _entregaRepo.UpdateAsync(entrega);
            
            _logger.LogInformation(
                "Acta de entrega {DocumentoId} vinculada exitosamente a entrega {EntregaId}",
                documentoId, entregaDotacionId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error vinculando acta de entrega {DocumentoId} a entrega {EntregaId}",
                documentoId, entregaDotacionId);
            return false;
        }
    }
    
    public async Task<bool> EliminarDocumentoAsync(int documentoId)
    {
        try
        {
            // Obtener documento
            var documento = await _documentoRepo.GetByIdAsync(documentoId);
            if (documento == null)
            {
                _logger.LogWarning("Intento de eliminar documento inexistente: {DocumentoId}", documentoId);
                return false;
            }
            
            // Eliminar archivo físico si existe
            if (!string.IsNullOrEmpty(documento.ArchivoPath))
            {
                var deleteResult = await _storageService.DeleteDocumentoEmpleadoAsync(documentoId);
                if (!deleteResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Error eliminando archivo físico del documento {DocumentoId}: {Error}",
                        documentoId, deleteResult.Error);
                    // Continuar con la eliminación en BD aunque falle el archivo físico
                }
            }
            
            // Soft delete en base de datos
            await _documentoRepo.DeleteAsync(documentoId);
            
            _logger.LogInformation(
                "Documento eliminado exitosamente: {DocumentoId} ({Tipo})",
                documentoId, documento.TipoDocumento);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando documento {DocumentoId}", documentoId);
            return false;
        }
    }
    
    /// <summary>
    /// Determina el tipo MIME basado en la extensión del archivo
    /// </summary>
    private static string ObtenerTipoMime(string nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}
