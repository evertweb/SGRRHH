using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class Empleado : EntidadBase
{
    public string Codigo { get; set; } = string.Empty;
    
    public string Cedula { get; set; } = string.Empty;
    
    public string Nombres { get; set; } = string.Empty;
    
    public string Apellidos { get; set; } = string.Empty;
    
    public string NombreCompleto => $"{Nombres ?? "Sin Nombre"} {Apellidos ?? ""}".Trim();
    
    public DateTime? FechaNacimiento { get; set; }
    
    public Genero? Genero { get; set; }
    
    public EstadoCivil? EstadoCivil { get; set; }
    
    public string? Direccion { get; set; }
    
    public string? Telefono { get; set; }
    
    public string? TelefonoEmergencia { get; set; }
    
    public string? ContactoEmergencia { get; set; }
    
    public string? Email { get; set; }
    
    public string? FotoPath { get; set; }
    
    public DateTime FechaIngreso { get; set; }
    
    public DateTime? FechaRetiro { get; set; }
    
    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;
    
    public int? CargoId { get; set; }
    
    public Cargo? Cargo { get; set; }
    
    public int? DepartamentoId { get; set; }
    
    public Departamento? Departamento { get; set; }
    
    public int? SupervisorId { get; set; }
    
    public Empleado? Supervisor { get; set; }
    
    public string? Observaciones { get; set; }
    
    public string? NumeroCuenta { get; set; }
    
    public string? Banco { get; set; }
    
    public NivelEducacion? NivelEducacion { get; set; }
    
    // ========== SEGURIDAD SOCIAL COMPLETA (Colombia) ==========
    
    public string? EPS { get; set; }
    
    public string? CodigoEPS { get; set; }
    
    public string? ARL { get; set; }
    
    public string? CodigoARL { get; set; }
    
    public int ClaseRiesgoARL { get; set; } = 1; // Clase I-V
    
    public string? AFP { get; set; }
    
    public string? CodigoAFP { get; set; }
    
    public string? CajaCompensacion { get; set; }
    
    public string? CodigoCajaCompensacion { get; set; }
    
    public decimal? SalarioBase { get; set; }
    
    public int? CreadoPorId { get; set; }
    
    public Usuario? CreadoPor { get; set; }
    
    public DateTime? FechaSolicitud { get; set; }
    
    public int? AprobadoPorId { get; set; }
    
    public Usuario? AprobadoPor { get; set; }
    
    public DateTime? FechaAprobacion { get; set; }
    
    public string? MotivoRechazo { get; set; }
    
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


