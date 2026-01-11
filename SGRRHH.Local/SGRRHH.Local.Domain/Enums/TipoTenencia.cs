namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Tipo de tenencia del terreno donde se desarrolla el proyecto forestal
/// </summary>
public enum TipoTenencia
{
    /// <summary>
    /// Terreno propio de la empresa
    /// </summary>
    Propio = 1,

    /// <summary>
    /// Terreno arrendado o alquilado
    /// </summary>
    Arrendado = 2,

    /// <summary>
    /// Préstamo de uso gratuito (comodato)
    /// </summary>
    Comodato = 3,

    /// <summary>
    /// Convenio asociativo con propietarios
    /// </summary>
    ConvenioAsociativo = 4,

    /// <summary>
    /// Concesión de terrenos del Estado
    /// </summary>
    ConcesionEstatal = 5,

    /// <summary>
    /// Otro tipo de tenencia
    /// </summary>
    Otro = 99
}
