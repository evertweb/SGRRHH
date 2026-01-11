using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// Estadísticas del dashboard de seguimiento de permisos
/// </summary>
public class EstadisticasSeguimiento
{
    /// <summary>Total de permisos en estado Pendiente</summary>
    public int TotalPermisosPendientes { get; set; }
    
    /// <summary>Total de permisos aprobados pendientes de documento</summary>
    public int TotalPendientesDocumento { get; set; }
    
    /// <summary>Total de permisos en proceso de compensación</summary>
    public int TotalEnCompensacion { get; set; }
    
    /// <summary>Total de permisos marcados para descuento</summary>
    public int TotalParaDescuento { get; set; }
    
    /// <summary>Total de documentos vencidos sin entregar</summary>
    public int TotalDocumentosVencidos { get; set; }
    
    /// <summary>Total de compensaciones vencidas sin completar</summary>
    public int TotalCompensacionesVencidas { get; set; }
    
    /// <summary>Monto total a descontar en el período actual</summary>
    public decimal MontoTotalDescuentos { get; set; }
    
    /// <summary>Lista de permisos que requieren atención urgente</summary>
    public List<PermisoConSeguimiento> PermisosUrgentes { get; set; } = new();
}

/// <summary>
/// DTO para mostrar permisos con información de seguimiento
/// </summary>
public class PermisoConSeguimiento
{
    public int Id { get; set; }
    
    public string NumeroActa { get; set; } = string.Empty;
    
    public string EmpleadoNombre { get; set; } = string.Empty;
    
    public int EmpleadoId { get; set; }
    
    public string TipoPermiso { get; set; } = string.Empty;
    
    public int TipoPermisoId { get; set; }
    
    public DateTime FechaInicio { get; set; }
    
    public DateTime FechaFin { get; set; }
    
    public int TotalDias { get; set; }
    
    public EstadoPermiso Estado { get; set; }
    
    public TipoResolucionPermiso TipoResolucion { get; set; }
    
    public DateTime? FechaLimite { get; set; }
    
    public int? DiasRestantes { get; set; }
    
    public string MotivoUrgencia { get; set; } = string.Empty;
    
    /// <summary>Para compensaciones: horas totales a compensar</summary>
    public int? HorasCompensar { get; set; }
    
    /// <summary>Para compensaciones: horas ya compensadas</summary>
    public int HorasCompensadas { get; set; }
    
    /// <summary>Monto a descontar (si aplica)</summary>
    public decimal? MontoDescuento { get; set; }
    
    /// <summary>Período de nómina para descuento</summary>
    public string? PeriodoDescuento { get; set; }
    
    /// <summary>Indica si está vencido</summary>
    public bool EstaVencido { get; set; }
    
    /// <summary>Indica si vence pronto (próximos 3 días)</summary>
    public bool VencePronto { get; set; }
}

/// <summary>
/// DTO para alertas de permisos
/// </summary>
public class AlertaPermiso
{
    public int PermisoId { get; set; }
    
    public string NumeroActa { get; set; } = string.Empty;
    
    public string EmpleadoNombre { get; set; } = string.Empty;
    
    public TipoAlertaPermiso TipoAlerta { get; set; }
    
    public string Mensaje { get; set; } = string.Empty;
    
    public DateTime FechaAlerta { get; set; }
    
    public DateTime? FechaLimite { get; set; }
    
    public int? DiasRestantes { get; set; }
    
    public bool EsUrgente { get; set; }
}

/// <summary>
/// Tipos de alertas para permisos
/// </summary>
public enum TipoAlertaPermiso
{
    /// <summary>Documento próximo a vencer (3 días)</summary>
    DocumentoPorVencer = 1,
    
    /// <summary>Documento ya vencido</summary>
    DocumentoVencido = 2,
    
    /// <summary>Compensación próxima a vencer (3 días)</summary>
    CompensacionPorVencer = 3,
    
    /// <summary>Compensación ya vencida</summary>
    CompensacionVencida = 4,
    
    /// <summary>Permiso pendiente de aprobación por más de 3 días</summary>
    AprobacionPendiente = 5
}
