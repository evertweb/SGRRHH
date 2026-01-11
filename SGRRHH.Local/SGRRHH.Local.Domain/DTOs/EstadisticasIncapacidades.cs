using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// Estadísticas generales de incapacidades para el dashboard
/// </summary>
public class EstadisticasIncapacidades
{
    public int TotalActivas { get; set; }
    public int TotalPendientesTranscribir { get; set; }
    public int TotalPendientesCobro { get; set; }
    public int TotalFinalizadasMes { get; set; }
    public decimal TotalPorCobrar { get; set; }
    public decimal TotalCobradoMes { get; set; }
    public int TotalDiasIncapacidadMes { get; set; }
    public List<IncapacidadResumen> IncapacidadesActivas { get; set; } = new();
    public List<IncapacidadResumen> ProximasVencer { get; set; } = new();
}

/// <summary>
/// Resumen de una incapacidad para listados
/// </summary>
public class IncapacidadResumen
{
    public int Id { get; set; }
    public string NumeroIncapacidad { get; set; } = string.Empty;
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string EmpleadoCedula { get; set; } = string.Empty;
    public int EmpleadoId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalDias { get; set; }
    public int DiasRestantes { get; set; }
    public TipoIncapacidad Tipo { get; set; }
    public EstadoIncapacidad Estado { get; set; }
    public bool Transcrita { get; set; }
    public bool Cobrada { get; set; }
    public decimal? ValorPorCobrar { get; set; }
    public string? DiagnosticoDescripcion { get; set; }
    public string? EntidadEmisora { get; set; }
    public string? EntidadPagadora { get; set; }
    public bool EsProrroga { get; set; }
    
    /// <summary>Nombre descriptivo del tipo de incapacidad</summary>
    public string TipoNombre => Tipo switch
    {
        TipoIncapacidad.EnfermedadGeneral => "Enfermedad General",
        TipoIncapacidad.AccidenteTrabajo => "Accidente de Trabajo",
        TipoIncapacidad.EnfermedadLaboral => "Enfermedad Laboral",
        TipoIncapacidad.LicenciaMaternidad => "Licencia Maternidad",
        TipoIncapacidad.LicenciaPaternidad => "Licencia Paternidad",
        _ => "Desconocido"
    };
    
    /// <summary>Nombre descriptivo del estado</summary>
    public string EstadoNombre => Estado switch
    {
        EstadoIncapacidad.Activa => "Activa",
        EstadoIncapacidad.Finalizada => "Finalizada",
        EstadoIncapacidad.Transcrita => "Transcrita",
        EstadoIncapacidad.Cobrada => "Cobrada",
        EstadoIncapacidad.Cancelada => "Cancelada",
        _ => "Desconocido"
    };
    
    /// <summary>Indica si la incapacidad está vencida</summary>
    public bool EstaVencida => DateTime.Today > FechaFin.Date;
    
    /// <summary>Indica si vence pronto (en los próximos 3 días)</summary>
    public bool VencePronto => !EstaVencida && DiasRestantes <= 3;
}
