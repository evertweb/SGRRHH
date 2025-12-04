namespace SGRRHH.Core.Enums;

/// <summary>
/// Tipos de documentos que se pueden adjuntar a un empleado
/// Basado en requisitos laborales de Colombia
/// </summary>
public enum TipoDocumentoEmpleado
{
    /// <summary>
    /// Cédula de ciudadanía escaneada
    /// </summary>
    Cedula = 1,
    
    /// <summary>
    /// Hoja de vida / Currículo
    /// </summary>
    HojaVida = 2,
    
    /// <summary>
    /// Certificados de estudios (diplomas, títulos)
    /// </summary>
    CertificadoEstudios = 3,
    
    /// <summary>
    /// Certificados laborales de empleos anteriores
    /// </summary>
    CertificadoLaboral = 4,
    
    /// <summary>
    /// Examen médico de ingreso (ocupacional)
    /// </summary>
    ExamenMedicoIngreso = 5,
    
    /// <summary>
    /// Examen médico periódico
    /// </summary>
    ExamenMedicoPeriodico = 6,
    
    /// <summary>
    /// Examen médico de egreso
    /// </summary>
    ExamenMedicoEgreso = 7,
    
    /// <summary>
    /// Afiliación a EPS (Salud)
    /// </summary>
    AfiliacionEPS = 8,
    
    /// <summary>
    /// Afiliación a AFP (Pensión)
    /// </summary>
    AfiliacionAFP = 9,
    
    /// <summary>
    /// Afiliación a ARL (Riesgos Laborales)
    /// </summary>
    AfiliacionARL = 10,
    
    /// <summary>
    /// Afiliación a Caja de Compensación Familiar
    /// </summary>
    AfiliacionCajaCompensacion = 11,
    
    /// <summary>
    /// Referencias personales
    /// </summary>
    ReferenciasPersonales = 12,
    
    /// <summary>
    /// Referencias laborales
    /// </summary>
    ReferenciasLaborales = 13,
    
    /// <summary>
    /// Antecedentes penales / Policivos
    /// </summary>
    Antecedentes = 14,
    
    /// <summary>
    /// Licencia de conducción
    /// </summary>
    LicenciaConduccion = 15,
    
    /// <summary>
    /// Libreta militar
    /// </summary>
    LibretaMilitar = 16,
    
    /// <summary>
    /// RUT (Registro Único Tributario)
    /// </summary>
    RUT = 17,
    
    /// <summary>
    /// Certificado de cuenta bancaria
    /// </summary>
    CertificadoBancario = 18,
    
    /// <summary>
    /// Acta de entrega de dotación
    /// </summary>
    ActaEntregaDotacion = 19,
    
    /// <summary>
    /// Capacitación / Certificación
    /// </summary>
    Capacitacion = 20,
    
    /// <summary>
    /// Contrato de trabajo firmado
    /// </summary>
    ContratoFirmado = 21,
    
    /// <summary>
    /// Foto del empleado
    /// </summary>
    Foto = 22,
    
    /// <summary>
    /// Otro documento
    /// </summary>
    Otro = 99
}
