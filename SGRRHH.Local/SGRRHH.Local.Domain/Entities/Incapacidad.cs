using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Representa una incapacidad médica de un empleado
/// </summary>
public class Incapacidad : EntidadBase
{
    // ===== IDENTIFICACIÓN =====
    
    /// <summary>Número único de la incapacidad (ej: INC-2026-0001)</summary>
    public string NumeroIncapacidad { get; set; } = string.Empty;
    
    /// <summary>ID del empleado incapacitado</summary>
    public int EmpleadoId { get; set; }
    
    /// <summary>Empleado relacionado</summary>
    public Empleado Empleado { get; set; } = null!;
    
    // ===== ORIGEN Y PRÓRROGAS =====
    
    /// <summary>ID del permiso que originó esta incapacidad (si aplica)</summary>
    public int? PermisoOrigenId { get; set; }
    
    /// <summary>Permiso de origen</summary>
    public Permiso? PermisoOrigen { get; set; }
    
    /// <summary>ID de la incapacidad anterior (si es prórroga)</summary>
    public int? IncapacidadAnteriorId { get; set; }
    
    /// <summary>Incapacidad anterior (para prórrogas)</summary>
    public Incapacidad? IncapacidadAnterior { get; set; }
    
    /// <summary>Indica si es una prórroga de otra incapacidad</summary>
    public bool EsProrroga { get; set; }
    
    // ===== FECHAS =====
    
    /// <summary>Primer día de incapacidad</summary>
    public DateTime FechaInicio { get; set; }
    
    /// <summary>Último día de incapacidad</summary>
    public DateTime FechaFin { get; set; }
    
    /// <summary>Total de días de incapacidad</summary>
    public int TotalDias { get; set; }
    
    /// <summary>Fecha en que se expidió el documento</summary>
    public DateTime FechaExpedicion { get; set; }
    
    // ===== DIAGNÓSTICO =====
    
    /// <summary>Código CIE-10 del diagnóstico (opcional)</summary>
    public string? DiagnosticoCIE10 { get; set; }
    
    /// <summary>Descripción del diagnóstico</summary>
    public string DiagnosticoDescripcion { get; set; } = string.Empty;
    
    // ===== TIPO Y ENTIDAD =====
    
    /// <summary>Tipo de incapacidad</summary>
    public TipoIncapacidad TipoIncapacidad { get; set; } = TipoIncapacidad.EnfermedadGeneral;
    
    /// <summary>Entidad que expidió la incapacidad (médico/IPS)</summary>
    public string EntidadEmisora { get; set; } = string.Empty;
    
    /// <summary>Entidad que debe pagar (EPS, ARL, nombre)</summary>
    public string? EntidadPagadora { get; set; }
    
    // ===== CÁLCULO DE DÍAS Y PAGOS =====
    
    /// <summary>Días que paga la empresa (normalmente 1-2)</summary>
    public int DiasEmpresa { get; set; }
    
    /// <summary>Días que paga EPS/ARL</summary>
    public int DiasEpsArl { get; set; }
    
    /// <summary>Porcentaje que paga la entidad (66.67%, 100%, etc.)</summary>
    public decimal PorcentajePago { get; set; } = 66.67m;
    
    /// <summary>Salario diario base para cálculo</summary>
    public decimal? ValorDiaBase { get; set; }
    
    /// <summary>Valor total a cobrar a EPS/ARL</summary>
    public decimal? ValorTotalCobrar { get; set; }
    
    // ===== ESTADO DE GESTIÓN =====
    
    /// <summary>Estado actual de la incapacidad</summary>
    public EstadoIncapacidad Estado { get; set; } = EstadoIncapacidad.Activa;
    
    /// <summary>¿Ya se transcribió ante EPS?</summary>
    public bool Transcrita { get; set; }
    
    /// <summary>Fecha de transcripción</summary>
    public DateTime? FechaTranscripcion { get; set; }
    
    /// <summary>Número de radicado ante EPS/ARL</summary>
    public string? NumeroRadicadoEps { get; set; }
    
    /// <summary>¿Ya se cobró a EPS/ARL?</summary>
    public bool Cobrada { get; set; }
    
    /// <summary>Fecha en que se realizó el cobro</summary>
    public DateTime? FechaCobro { get; set; }
    
    /// <summary>Valor efectivamente cobrado</summary>
    public decimal? ValorCobrado { get; set; }
    
    // ===== DOCUMENTOS =====
    
    /// <summary>Ruta al documento de incapacidad escaneado</summary>
    public string? DocumentoIncapacidadPath { get; set; }
    
    /// <summary>Ruta al documento de transcripción</summary>
    public string? DocumentoTranscripcionPath { get; set; }
    
    // ===== OTROS =====
    
    /// <summary>Observaciones adicionales</summary>
    public string? Observaciones { get; set; }
    
    /// <summary>ID del usuario que registró la incapacidad</summary>
    public int RegistradoPorId { get; set; }
    
    /// <summary>Usuario que registró</summary>
    public Usuario RegistradoPor { get; set; } = null!;
    
    // ===== RELACIONES =====
    
    /// <summary>Prórrogas de esta incapacidad</summary>
    public ICollection<Incapacidad> Prorrogas { get; set; } = new List<Incapacidad>();
    
    /// <summary>Historial de seguimiento</summary>
    public ICollection<SeguimientoIncapacidad> Seguimientos { get; set; } = new List<SeguimientoIncapacidad>();
    
    // ===== PROPIEDADES CALCULADAS =====
    
    /// <summary>Días restantes de incapacidad</summary>
    public int DiasRestantes => Math.Max(0, (FechaFin.Date - DateTime.Today).Days);
    
    /// <summary>Indica si la incapacidad está vencida</summary>
    public bool EstaVencida => DateTime.Today > FechaFin.Date;
    
    /// <summary>Nombre descriptivo del tipo de incapacidad</summary>
    public string TipoNombre => TipoIncapacidad switch
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
    
    // ===== MÉTODOS DE CÁLCULO =====
    
    /// <summary>
    /// Calcula los días que paga empresa vs EPS/ARL según tipo
    /// </summary>
    public void CalcularDistribucionDias()
    {
        switch (TipoIncapacidad)
        {
            case TipoIncapacidad.EnfermedadGeneral:
                // Empresa paga días 1-2, EPS paga día 3 en adelante
                DiasEmpresa = Math.Min(2, TotalDias);
                DiasEpsArl = Math.Max(0, TotalDias - 2);
                PorcentajePago = TotalDias <= 90 ? 66.67m : 50m;
                break;
                
            case TipoIncapacidad.AccidenteTrabajo:
            case TipoIncapacidad.EnfermedadLaboral:
                // ARL paga desde el día 1 al 100%
                DiasEmpresa = 0;
                DiasEpsArl = TotalDias;
                PorcentajePago = 100m;
                break;
                
            case TipoIncapacidad.LicenciaMaternidad:
            case TipoIncapacidad.LicenciaPaternidad:
                // EPS paga al 100%
                DiasEmpresa = 0;
                DiasEpsArl = TotalDias;
                PorcentajePago = 100m;
                break;
        }
    }
    
    /// <summary>
    /// Calcula el valor a cobrar a EPS/ARL
    /// </summary>
    /// <param name="salarioMensual">Salario mensual del empleado</param>
    public void CalcularValorCobro(decimal salarioMensual)
    {
        ValorDiaBase = salarioMensual / 30;
        ValorTotalCobrar = DiasEpsArl * ValorDiaBase.Value * (PorcentajePago / 100);
    }
    
    /// <summary>
    /// Calcula el total de días incluyendo prórrogas
    /// </summary>
    /// <returns>Total de días acumulados</returns>
    public int CalcularDiasAcumulados()
    {
        int total = TotalDias;
        foreach (var prorroga in Prorrogas)
        {
            total += prorroga.TotalDias;
        }
        return total;
    }
}
