namespace SGRRHH.Core.Models;

/// <summary>
/// Parámetros opcionales para la generación del certificado laboral.
/// </summary>
public class CertificadoLaboralOptions
{
    public DateTime FechaEmision { get; set; } = DateTime.Today;
    public string CiudadExpedicion { get; set; } = "Bogotá D.C.";
    public string ResponsableNombre { get; set; } = "María Pérez";
    public string ResponsableCargo { get; set; } = "Gerente General";
    public string? ParrafoAdicional { get; set; }
        = "El presente certificado se expide a solicitud del interesado.";
}
