using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// DTO para crear una nueva incapacidad
/// </summary>
public class CrearIncapacidadDto
{
    public int EmpleadoId { get; set; }
    public int? PermisoOrigenId { get; set; }
    public int? IncapacidadAnteriorId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaExpedicion { get; set; }
    public string? DiagnosticoCIE10 { get; set; }
    public string DiagnosticoDescripcion { get; set; } = string.Empty;
    public TipoIncapacidad TipoIncapacidad { get; set; }
    public string EntidadEmisora { get; set; } = string.Empty;
    public string? EntidadPagadora { get; set; }
    public string? Observaciones { get; set; }
    public string? DocumentoPath { get; set; }
}

/// <summary>
/// DTO para registrar la transcripción de una incapacidad
/// </summary>
public class RegistrarTranscripcionDto
{
    public int IncapacidadId { get; set; }
    public DateTime FechaTranscripcion { get; set; }
    public string? NumeroRadicado { get; set; }
    public string? DocumentoPath { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para registrar el cobro de una incapacidad
/// </summary>
public class RegistrarCobroDto
{
    public int IncapacidadId { get; set; }
    public DateTime FechaCobro { get; set; }
    public decimal ValorCobrado { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para crear una prórroga de incapacidad
/// </summary>
public class CrearProrrogaDto
{
    public int IncapacidadAnteriorId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaExpedicion { get; set; }
    public string? DiagnosticoCIE10 { get; set; }
    public string DiagnosticoDescripcion { get; set; } = string.Empty;
    public string EntidadEmisora { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? DocumentoPath { get; set; }
}

/// <summary>
/// DTO con información completa de una incapacidad para vista detallada
/// </summary>
public class IncapacidadDetalleDto
{
    // Datos básicos
    public int Id { get; set; }
    public string NumeroIncapacidad { get; set; } = string.Empty;
    
    // Empleado
    public int EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string EmpleadoCedula { get; set; } = string.Empty;
    public string EmpleadoCargo { get; set; } = string.Empty;
    public string EmpleadoDepartamento { get; set; } = string.Empty;
    public decimal? EmpleadoSalario { get; set; }
    
    // Origen
    public int? PermisoOrigenId { get; set; }
    public string? PermisoOrigenNumero { get; set; }
    public int? IncapacidadAnteriorId { get; set; }
    public string? IncapacidadAnteriorNumero { get; set; }
    public bool EsProrroga { get; set; }
    
    // Fechas
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalDias { get; set; }
    public DateTime FechaExpedicion { get; set; }
    public int DiasRestantes { get; set; }
    
    // Diagnóstico
    public string? DiagnosticoCIE10 { get; set; }
    public string DiagnosticoDescripcion { get; set; } = string.Empty;
    
    // Tipo y Entidad
    public TipoIncapacidad TipoIncapacidad { get; set; }
    public string TipoNombre { get; set; } = string.Empty;
    public string EntidadEmisora { get; set; } = string.Empty;
    public string? EntidadPagadora { get; set; }
    
    // Cálculos
    public int DiasEmpresa { get; set; }
    public int DiasEpsArl { get; set; }
    public decimal PorcentajePago { get; set; }
    public decimal? ValorDiaBase { get; set; }
    public decimal? ValorTotalCobrar { get; set; }
    
    // Estado
    public EstadoIncapacidad Estado { get; set; }
    public string EstadoNombre { get; set; } = string.Empty;
    public bool Transcrita { get; set; }
    public DateTime? FechaTranscripcion { get; set; }
    public string? NumeroRadicadoEps { get; set; }
    public bool Cobrada { get; set; }
    public DateTime? FechaCobro { get; set; }
    public decimal? ValorCobrado { get; set; }
    
    // Documentos
    public string? DocumentoIncapacidadPath { get; set; }
    public string? DocumentoTranscripcionPath { get; set; }
    
    // Otros
    public string? Observaciones { get; set; }
    public int RegistradoPorId { get; set; }
    public string RegistradoPorNombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    
    // Prórrogas
    public List<IncapacidadResumen> Prorrogas { get; set; } = new();
    public int TotalDiasAcumulados { get; set; }
    
    // Seguimiento
    public List<SeguimientoIncapacidadDto> Seguimientos { get; set; } = new();
}

/// <summary>
/// DTO para items de seguimiento de incapacidad
/// </summary>
public class SeguimientoIncapacidadDto
{
    public int Id { get; set; }
    public DateTime FechaAccion { get; set; }
    public TipoAccionSeguimientoIncapacidad TipoAccion { get; set; }
    public string TipoAccionNombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string RealizadoPorNombre { get; set; } = string.Empty;
    public string? DatosAdicionales { get; set; }
}

/// <summary>
/// DTO para el reporte de cobro a EPS/ARL
/// </summary>
public class ReporteCobroEpsDto
{
    public int Año { get; set; }
    public int Mes { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public decimal TotalPorCobrar { get; set; }
    public int TotalIncapacidades { get; set; }
    public int TotalDias { get; set; }
    public List<ItemReporteCobroDto> Items { get; set; } = new();
    public Dictionary<string, decimal> TotalesPorEntidad { get; set; } = new();
}

/// <summary>
/// Item individual del reporte de cobro
/// </summary>
public class ItemReporteCobroDto
{
    public string NumeroIncapacidad { get; set; } = string.Empty;
    public string EmpleadoCedula { get; set; } = string.Empty;
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalDias { get; set; }
    public int DiasEpsArl { get; set; }
    public string TipoIncapacidad { get; set; } = string.Empty;
    public string DiagnosticoDescripcion { get; set; } = string.Empty;
    public string EntidadPagadora { get; set; } = string.Empty;
    public decimal ValorDiaBase { get; set; }
    public decimal PorcentajePago { get; set; }
    public decimal ValorCobrar { get; set; }
    public bool Transcrita { get; set; }
    public string? NumeroRadicado { get; set; }
}
