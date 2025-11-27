namespace SGRRHH.Core.Models;

/// <summary>
/// Parámetros usados para la constancia de trabajo.
/// </summary>
public class ConstanciaTrabajoOptions
{
    public DateTime FechaEmision { get; set; } = DateTime.Today;
    public string CiudadExpedicion { get; set; } = "Bogotá D.C.";
    public string Motivo { get; set; } = "Trámite personal";
    public string ResponsableNombre { get; set; } = "María Pérez";
    public string ResponsableCargo { get; set; } = "Gerente General";
    public string? NotasAdicionales { get; set; }
        = "Se expide en constancia de que el colaborador cumple con sus funciones a satisfacción.";
}
