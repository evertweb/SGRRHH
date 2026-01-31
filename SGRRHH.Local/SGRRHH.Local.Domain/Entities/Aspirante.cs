using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Representa un aspirante a un puesto de trabajo.
/// </summary>
public class Aspirante : EntidadBase
{
    public int? VacanteId { get; set; }
    
    public Vacante? Vacante { get; set; }
    
    // ========== IDENTIFICACIÓN ==========
    
    public string Cedula { get; set; } = string.Empty;
    
    public string Nombres { get; set; } = string.Empty;
    
    public string Apellidos { get; set; } = string.Empty;
    
    public string NombreCompleto => $"{Nombres ?? "Sin Nombre"} {Apellidos ?? ""}".Trim();
    
    public DateTime FechaNacimiento { get; set; }
    
    public Genero Genero { get; set; }
    
    public EstadoCivil EstadoCivil { get; set; }
    
    // ========== UBICACIÓN ==========
    
    public string Direccion { get; set; } = string.Empty;
    
    public string Ciudad { get; set; } = string.Empty;
    
    public string Departamento { get; set; } = string.Empty;
    
    // ========== CONTACTO ==========
    
    public string Telefono { get; set; } = string.Empty;
    
    public string? Email { get; set; }
    
    // ========== EDUCACIÓN ==========
    
    public NivelEducacion NivelEducacion { get; set; }
    
    public string? TituloObtenido { get; set; }
    
    public string? InstitucionEducativa { get; set; }
    
    // ========== TALLAS EPP ==========
    
    public string? TallasCasco { get; set; }
    
    public string? TallasBotas { get; set; }
    
    // ========== PROCESO ==========
    
    public EstadoAspirante Estado { get; set; } = EstadoAspirante.Registrado;
    
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    
    public string? Notas { get; set; }
    
    public int? PuntajeEvaluacion { get; set; }
    
    /// <summary>
    /// Indica si el registro está activo (para soft delete en la tabla).
    /// </summary>
    public bool EsActivo { get; set; } = true;
    
    // ========== PROPIEDADES CALCULADAS ==========
    
    /// <summary>
    /// Edad del aspirante en años.
    /// </summary>
    public int Edad
    {
        get
        {
            var edad = DateTime.Today.Year - FechaNacimiento.Year;
            if (DateTime.Today.DayOfYear < FechaNacimiento.DayOfYear)
                edad--;
            return Math.Max(0, edad);
        }
    }
    
    /// <summary>
    /// Indica si el aspirante puede ser contratado.
    /// </summary>
    public bool PuedeSerContratado => Estado == EstadoAspirante.Entrevistado ||
                                       Estado == EstadoAspirante.Preseleccionado;
    
    // ========== NAVEGACIÓN ==========
    
    public ICollection<FormacionAspirante>? Formaciones { get; set; }
    
    public ICollection<ExperienciaAspirante>? Experiencias { get; set; }
    
    public ICollection<ReferenciaAspirante>? Referencias { get; set; }
    
    public ICollection<HojaVidaPdf>? HojasVida { get; set; }
}
