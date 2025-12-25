namespace SGRRHH.Core.Enums;

/// <summary>
/// Estados posibles de un contrato laboral
/// </summary>
public enum EstadoContrato
{
    /// <summary>
    /// Contrato vigente
    /// </summary>
    Activo,
    
    /// <summary>
    /// Contrato finalizó normalmente (cumplió su duración)
    /// </summary>
    Finalizado,
    
    /// <summary>
    /// Contrato fue renovado con un nuevo contrato
    /// </summary>
    Renovado,
    
    /// <summary>
    /// Contrato cancelado por ruptura anticipada (despido, renuncia, etc.)
    /// </summary>
    Cancelado
}
