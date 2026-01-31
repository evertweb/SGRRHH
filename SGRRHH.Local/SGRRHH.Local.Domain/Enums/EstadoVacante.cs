namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Estados posibles de una vacante en el sistema.
/// </summary>
public enum EstadoVacante
{
    /// <summary>
    /// Vacante en preparación, no visible para aspirantes.
    /// </summary>
    Borrador = 0,
    
    /// <summary>
    /// Vacante publicada y aceptando aspirantes.
    /// </summary>
    Abierta = 1,
    
    /// <summary>
    /// Vacante con proceso de selección en curso.
    /// </summary>
    EnProceso = 2,
    
    /// <summary>
    /// Vacante cubierta o finalizada exitosamente.
    /// </summary>
    Cerrada = 3,
    
    /// <summary>
    /// Vacante cancelada sin cubrir.
    /// </summary>
    Cancelada = 4
}
