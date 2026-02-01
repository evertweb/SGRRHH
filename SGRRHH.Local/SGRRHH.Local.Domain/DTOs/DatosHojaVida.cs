namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// DTO que contiene los datos extraídos de un PDF de hoja de vida.
/// </summary>
public class DatosHojaVida
{
    // ========== DATOS PERSONALES ==========
    
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string? Cedula { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Genero { get; set; }
    public string? EstadoCivil { get; set; }
    
    // ========== UBICACIÓN ==========
    
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Departamento { get; set; }
    
    // ========== CONTACTO ==========
    
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    
    // ========== TALLAS EPP ==========
    
    public string? TallaCasco { get; set; }
    public string? TallaBotas { get; set; }
    
    // ========== DATOS RELACIONADOS ==========
    
    public List<DatosFormacion> Formaciones { get; set; } = new();
    public List<DatosExperiencia> Experiencias { get; set; } = new();
    public List<DatosReferencia> Referencias { get; set; } = new();
    
    // ========== METADATOS ==========
    
    public DateTime? FechaFirma { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para datos de formación académica extraídos del PDF.
/// </summary>
public class DatosFormacion
{
    public string? Nivel { get; set; }
    public string? Titulo { get; set; }
    public string? Institucion { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool EnCurso { get; set; }
}

/// <summary>
/// DTO para datos de experiencia laboral extraídos del PDF.
/// </summary>
public class DatosExperiencia
{
    public string? Empresa { get; set; }
    public string? Cargo { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool TrabajoActual { get; set; }
    public string? Funciones { get; set; }
    public string? MotivoRetiro { get; set; }
}

/// <summary>
/// DTO para datos de referencia extraídos del PDF.
/// </summary>
public class DatosReferencia
{
    public string? Tipo { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Telefono { get; set; }
    public string? Relacion { get; set; }
    public string? Empresa { get; set; }
    public string? Cargo { get; set; }
}
