namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Tipos de proyectos forestales/silviculturales
/// </summary>
public enum TipoProyectoForestal
{
    /// <summary>
    /// Establecimiento de nueva plantación forestal
    /// </summary>
    PlantacionNueva = 1,

    /// <summary>
    /// Mantenimiento general de plantación existente
    /// </summary>
    Mantenimiento = 2,

    /// <summary>
    /// Raleo o entresaca para mejorar crecimiento
    /// </summary>
    Raleo = 3,

    /// <summary>
    /// Poda de formación del árbol
    /// </summary>
    PodaFormacion = 4,

    /// <summary>
    /// Poda sanitaria por enfermedad o daño
    /// </summary>
    PodaSanitaria = 5,

    /// <summary>
    /// Cosecha o aprovechamiento final
    /// </summary>
    Cosecha = 6,

    /// <summary>
    /// Reforestación de áreas intervenidas
    /// </summary>
    Reforestacion = 7,

    /// <summary>
    /// Actividades de vivero forestal
    /// </summary>
    Vivero = 8,

    /// <summary>
    /// Investigación y ensayos forestales
    /// </summary>
    InvestigacionEnsayo = 9,

    /// <summary>
    /// Construcción y mantenimiento de vías internas
    /// </summary>
    InfraestructuraVial = 10,

    /// <summary>
    /// Control de plagas y enfermedades
    /// </summary>
    ControlFitosanitario = 11,

    /// <summary>
    /// Aplicación de fertilizantes
    /// </summary>
    Fertilizacion = 12,

    /// <summary>
    /// Inventario forestal y mediciones
    /// </summary>
    Inventario = 13,

    /// <summary>
    /// Control de malezas
    /// </summary>
    ControlMalezas = 14,

    /// <summary>
    /// Otro tipo de proyecto no clasificado
    /// </summary>
    Otro = 99
}
