namespace SGRRHH.Local.Domain.Enums;

public enum EstadoNomina
{
    /// <summary>
    /// Nómina en proceso de cálculo o ajuste
    /// </summary>
    Borrador = 1,
    
    /// <summary>
    /// Nómina calculada y lista para aprobación
    /// </summary>
    Calculada = 2,
    
    /// <summary>
    /// Nómina aprobada por el responsable
    /// </summary>
    Aprobada = 3,
    
    /// <summary>
    /// Nómina pagada a los empleados
    /// </summary>
    Pagada = 4,
    
    /// <summary>
    /// Nómina contabilizada
    /// </summary>
    Contabilizada = 5,
    
    /// <summary>
    /// Nómina anulada o reversada
    /// </summary>
    Anulada = 6
}
