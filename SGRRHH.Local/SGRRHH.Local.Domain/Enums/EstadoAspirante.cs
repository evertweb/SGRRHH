namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Estados posibles de un aspirante en el proceso de selección.
/// </summary>
public enum EstadoAspirante
{
    /// <summary>
    /// Aspirante recién registrado en el sistema.
    /// </summary>
    Registrado = 0,
    
    /// <summary>
    /// Aspirante en revisión por RRHH.
    /// </summary>
    EnRevision = 1,
    
    /// <summary>
    /// Aspirante preseleccionado para entrevista.
    /// </summary>
    Preseleccionado = 2,
    
    /// <summary>
    /// Aspirante que ya pasó por entrevista.
    /// </summary>
    Entrevistado = 3,
    
    /// <summary>
    /// Aspirante contratado exitosamente (migrado a empleado).
    /// </summary>
    Contratado = 4,
    
    /// <summary>
    /// Aspirante descartado del proceso.
    /// </summary>
    Descartado = 5,
    
    /// <summary>
    /// Aspirante previamente descartado que vuelve a aplicar.
    /// </summary>
    Reactivado = 6
}
