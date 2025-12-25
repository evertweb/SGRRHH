using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa un empleado de la empresa
/// </summary>
public class Empleado : EntidadBase
{
    /// <summary>
    /// Código único del empleado
    /// </summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>
    /// Número de cédula o documento de identidad
    /// </summary>
    public string Cedula { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombres del empleado
    /// </summary>
    public string Nombres { get; set; } = string.Empty;
    
    /// <summary>
    /// Apellidos del empleado
    /// </summary>
    public string Apellidos { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre completo del empleado
    /// </summary>
    public string NombreCompleto => $"{Nombres ?? "Sin Nombre"} {Apellidos ?? ""}".Trim();
    
    /// <summary>
    /// Fecha de nacimiento
    /// </summary>
    public DateTime? FechaNacimiento { get; set; }
    
    /// <summary>
    /// Género del empleado
    /// </summary>
    public Genero? Genero { get; set; }
    
    /// <summary>
    /// Estado civil del empleado
    /// </summary>
    public EstadoCivil? EstadoCivil { get; set; }
    
    /// <summary>
    /// Dirección de residencia
    /// </summary>
    public string? Direccion { get; set; }
    
    /// <summary>
    /// Teléfono principal
    /// </summary>
    public string? Telefono { get; set; }
    
    /// <summary>
    /// Teléfono de emergencia
    /// </summary>
    public string? TelefonoEmergencia { get; set; }
    
    /// <summary>
    /// Nombre del contacto de emergencia
    /// </summary>
    public string? ContactoEmergencia { get; set; }
    
    /// <summary>
    /// Correo electrónico personal
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Ruta de la foto del empleado
    /// </summary>
    public string? FotoPath { get; set; }
    
    /// <summary>
    /// Fecha de ingreso a la empresa
    /// </summary>
    public DateTime FechaIngreso { get; set; }
    
    /// <summary>
    /// Fecha de retiro (si aplica)
    /// </summary>
    public DateTime? FechaRetiro { get; set; }
    
    /// <summary>
    /// Estado actual del empleado
    /// </summary>
    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;
    
    /// <summary>
    /// Tipo de contrato actual
    /// </summary>
    public TipoContrato TipoContrato { get; set; }
    
    /// <summary>
    /// ID del cargo asignado
    /// </summary>
    public int? CargoId { get; set; }
    
    /// <summary>
    /// Cargo asignado al empleado
    /// </summary>
    public Cargo? Cargo { get; set; }
    
    /// <summary>
    /// ID del departamento asignado
    /// </summary>
    public int? DepartamentoId { get; set; }
    
    /// <summary>
    /// Departamento asignado al empleado
    /// </summary>
    public Departamento? Departamento { get; set; }
    
    /// <summary>
    /// ID del supervisor directo
    /// </summary>
    public int? SupervisorId { get; set; }
    
    /// <summary>
    /// Supervisor directo del empleado
    /// </summary>
    public Empleado? Supervisor { get; set; }
    
    /// <summary>
    /// Observaciones generales
    /// </summary>
    public string? Observaciones { get; set; }
    
    /// <summary>
    /// ID del usuario que creó/solicitó el empleado
    /// </summary>
    public int? CreadoPorId { get; set; }
    
    /// <summary>
    /// Usuario que creó la solicitud
    /// </summary>
    public Usuario? CreadoPor { get; set; }
    
    /// <summary>
    /// Fecha en que se creó la solicitud
    /// </summary>
    public DateTime? FechaSolicitud { get; set; }
    
    /// <summary>
    /// ID del usuario que aprobó/rechazó el empleado
    /// </summary>
    public int? AprobadoPorId { get; set; }
    
    /// <summary>
    /// Usuario que aprobó la solicitud
    /// </summary>
    public Usuario? AprobadoPor { get; set; }
    
    /// <summary>
    /// Fecha de aprobación o rechazo
    /// </summary>
    public DateTime? FechaAprobacion { get; set; }
    
    /// <summary>
    /// Motivo del rechazo (si aplica)
    /// </summary>
    public string? MotivoRechazo { get; set; }
    
    /// <summary>
    /// Calcula los años de antigüedad del empleado
    /// </summary>
    public int Antiguedad
    {
        get
        {
            var fechaFin = FechaRetiro ?? DateTime.Today;
            var antiguedad = fechaFin.Year - FechaIngreso.Year;
            if (fechaFin.DayOfYear < FechaIngreso.DayOfYear)
                antiguedad--;
            return Math.Max(0, antiguedad);
        }
    }
}
