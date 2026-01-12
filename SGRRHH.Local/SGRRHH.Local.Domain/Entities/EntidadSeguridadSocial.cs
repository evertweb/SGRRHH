namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Clase base para entidades de seguridad social colombiana
/// </summary>
public abstract class EntidadSeguridadSocial : EntidadBase
{
    /// <summary>Código oficial ante la Superintendencia</summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>Nombre oficial de la entidad</summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>Observaciones adicionales</summary>
    public string? Observaciones { get; set; }
}

/// <summary>Entidad Promotora de Salud</summary>
public class Eps : EntidadSeguridadSocial 
{
    /// <summary>Estado de la EPS (Activa, Intervenida, etc.)</summary>
    public string? Estado { get; set; }
    
    /// <summary>Cobertura geográfica (Nacional, Regional)</summary>
    public string? Cobertura { get; set; }
    
    /// <summary>Régimen (Contributivo, Subsidiado)</summary>
    public string? Regimen { get; set; }
}

/// <summary>Administradora de Fondo de Pensiones</summary>
public class Afp : EntidadSeguridadSocial 
{
    /// <summary>Tipo de AFP (Público, Privado)</summary>
    public string? Tipo { get; set; }
}

/// <summary>Administradora de Riesgos Laborales</summary>
public class Arl : EntidadSeguridadSocial { }

/// <summary>Caja de Compensación Familiar</summary>
public class CajaCompensacion : EntidadSeguridadSocial 
{
    /// <summary>Región donde opera principalmente</summary>
    public string? Region { get; set; }
}
