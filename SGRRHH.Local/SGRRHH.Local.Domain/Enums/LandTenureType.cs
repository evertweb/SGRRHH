namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Tipo de tenencia del terreno donde se desarrolla el proyecto forestal
/// </summary>
public enum LandTenureType
{
    /// <summary>
    /// Terreno propio de la empresa
    /// </summary>
    Owned = 1,

    /// <summary>
    /// Terreno arrendado o alquilado
    /// </summary>
    Leased = 2,

    /// <summary>
    /// Préstamo de uso gratuito (comodato)
    /// </summary>
    Loan = 3,

    /// <summary>
    /// Convenio asociativo con propietarios
    /// </summary>
    AssociativeAgreement = 4,

    /// <summary>
    /// Concesión de terrenos del Estado
    /// </summary>
    StateConcession = 5,

    /// <summary>
    /// Otro tipo de tenencia
    /// </summary>
    Other = 99
}
