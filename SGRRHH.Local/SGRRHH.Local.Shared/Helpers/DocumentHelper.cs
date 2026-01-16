using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Helpers;

/// <summary>
/// Helper para operaciones relacionadas con documentos de empleados
/// </summary>
public static class DocumentHelper
{
    /// <summary>
    /// Obtiene el nombre legible de un tipo de documento
    /// Extraído de EmpleadoExpediente.razor líneas 891-919
    /// </summary>
    public static string GetTipoDocumentoNombre(TipoDocumentoEmpleado tipo)
    {
        return tipo switch
        {
            TipoDocumentoEmpleado.Cedula => "Cédula",
            TipoDocumentoEmpleado.HojaVida => "Hoja de Vida",
            TipoDocumentoEmpleado.CertificadoEstudios => "Certificado Estudios",
            TipoDocumentoEmpleado.CertificadoLaboral => "Certificado Laboral",
            TipoDocumentoEmpleado.ExamenMedicoIngreso => "Examen Médico Ingreso",
            TipoDocumentoEmpleado.ExamenMedicoPeriodico => "Examen Médico Periódico",
            TipoDocumentoEmpleado.ExamenMedicoEgreso => "Examen Médico Egreso",
            TipoDocumentoEmpleado.AfiliacionEPS => "Afiliación EPS",
            TipoDocumentoEmpleado.AfiliacionAFP => "Afiliación AFP",
            TipoDocumentoEmpleado.AfiliacionARL => "Afiliación ARL",
            TipoDocumentoEmpleado.AfiliacionCajaCompensacion => "Caja Compensación",
            TipoDocumentoEmpleado.ReferenciasPersonales => "Referencias Personales",
            TipoDocumentoEmpleado.ReferenciasLaborales => "Referencias Laborales",
            TipoDocumentoEmpleado.Antecedentes => "Antecedentes",
            TipoDocumentoEmpleado.LicenciaConduccion => "Licencia Conducción",
            TipoDocumentoEmpleado.LibretaMilitar => "Libreta Militar",
            TipoDocumentoEmpleado.RUT => "RUT",
            TipoDocumentoEmpleado.CertificadoBancario => "Certificado Bancario",
            TipoDocumentoEmpleado.ActaEntregaDotacion => "Entrega Dotación",
            TipoDocumentoEmpleado.Capacitacion => "Capacitación",
            TipoDocumentoEmpleado.ContratoFirmado => "Contrato Firmado",
            TipoDocumentoEmpleado.Foto => "Foto",
            _ => "Otro"
        };
    }
    
    /// <summary>
    /// Obtiene el estado de un documento (vigente, próximo a vencer, vencido)
    /// Extraído de EmpleadoExpediente.razor líneas 921-928
    /// </summary>
    public static string GetDocumentoStatus(DocumentoEmpleado doc)
    {
        if (!doc.FechaVencimiento.HasValue) return "vigente";
        var dias = (doc.FechaVencimiento.Value - DateTime.Today).Days;
        if (dias < 0) return "vencido";
        if (dias <= 30) return "proximo";
        return "vigente";
    }
    
    /// <summary>
    /// Verifica si un MIME type corresponde a imagen
    /// Extraído de EmpleadoExpediente.razor líneas 737-740
    /// </summary>
    public static bool IsImageMime(string? mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType)) return false;
        return mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }
}
