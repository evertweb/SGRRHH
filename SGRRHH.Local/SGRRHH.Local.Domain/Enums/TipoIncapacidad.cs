namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Tipos de incapacidad según normativa colombiana
/// </summary>
public enum TipoIncapacidad
{
    /// <summary>Enfermedad de origen común</summary>
    EnfermedadGeneral = 1,
    
    /// <summary>Accidente de trabajo (ARL)</summary>
    AccidenteTrabajo = 2,
    
    /// <summary>Enfermedad de origen laboral (ARL)</summary>
    EnfermedadLaboral = 3,
    
    /// <summary>Licencia de maternidad</summary>
    LicenciaMaternidad = 4,
    
    /// <summary>Licencia de paternidad</summary>
    LicenciaPaternidad = 5
}
