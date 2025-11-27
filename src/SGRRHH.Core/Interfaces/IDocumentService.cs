using SGRRHH.Core.Common;
using SGRRHH.Core.Models;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio responsable de generar documentos PDF del sistema.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Obtiene la información actual de la empresa para mostrarla en la UI.
    /// </summary>
    CompanyInfo GetCompanyInfo();

    /// <summary>
    /// Genera el PDF del acta de permiso asociado al registro indicado.
    /// </summary>
    Task<ServiceResult<byte[]>> GenerateActaPermisoPdfAsync(int permisoId);

    /// <summary>
    /// Genera un certificado laboral del empleado.
    /// </summary>
    Task<ServiceResult<byte[]>> GenerateCertificadoLaboralPdfAsync(int empleadoId, CertificadoLaboralOptions? options = null);

    /// <summary>
    /// Genera una constancia de trabajo del empleado.
    /// </summary>
    Task<ServiceResult<byte[]>> GenerateConstanciaTrabajoPdfAsync(int empleadoId, ConstanciaTrabajoOptions? options = null);

    /// <summary>
    /// Guarda el PDF en disco en la carpeta estándar y retorna la ruta final.
    /// </summary>
    Task<ServiceResult<string>> SaveDocumentAsync(byte[] pdfBytes, string suggestedFileName);
}
