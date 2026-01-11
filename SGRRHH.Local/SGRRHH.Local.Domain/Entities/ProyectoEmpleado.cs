using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Asignación de un empleado a un proyecto (miembro de cuadrilla)
/// </summary>
public class ProyectoEmpleado : EntidadBase
{
    public int ProyectoId { get; set; }
    public Proyecto? Proyecto { get; set; }
    
    public int EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }
    
    // ===== ASIGNACIÓN =====
    
    /// <summary>
    /// Fecha en que se asignó al proyecto
    /// </summary>
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Fecha en que se retiró del proyecto (null = activo)
    /// </summary>
    public DateTime? FechaDesasignacion { get; set; }
    
    /// <summary>
    /// Motivo de desasignación (si aplica)
    /// </summary>
    public string? MotivoDesasignacion { get; set; }
    
    // ===== ROL EN EL PROYECTO =====
    
    /// <summary>
    /// Rol del empleado en este proyecto (enum)
    /// </summary>
    public RolProyectoForestal? RolEnum { get; set; }
    
    /// <summary>
    /// Rol como texto (legacy o personalizado)
    /// </summary>
    public string? Rol { get; set; }
    
    /// <summary>
    /// Si es el líder/responsable de la cuadrilla
    /// </summary>
    public bool EsLiderCuadrilla { get; set; }
    
    // ===== DEDICACIÓN =====
    
    /// <summary>
    /// Porcentaje de dedicación al proyecto (0-100)
    /// 100 = tiempo completo, 50 = medio tiempo
    /// </summary>
    public int PorcentajeDedicacion { get; set; } = 100;
    
    /// <summary>
    /// Tipo de vinculación al proyecto
    /// </summary>
    public string? TipoVinculacion { get; set; } // Permanente, Temporal, Por Tarea
    
    // ===== MÉTRICAS (actualizadas periódicamente) =====
    
    /// <summary>
    /// Total de horas trabajadas en este proyecto
    /// </summary>
    public decimal TotalHorasTrabajadas { get; set; }
    
    /// <summary>
    /// Total de jornales en este proyecto
    /// </summary>
    public int TotalJornales { get; set; }
    
    /// <summary>
    /// Última fecha de trabajo registrada
    /// </summary>
    public DateTime? UltimaFechaTrabajo { get; set; }
    
    /// <summary>
    /// Número de días trabajados
    /// </summary>
    public int DiasTrabajados { get; set; }
    
    // ===== OTROS =====
    
    public string? Observaciones { get; set; }
    
    // ===== PROPIEDADES CALCULADAS =====
    
    /// <summary>
    /// Indica si la asignación está activa
    /// </summary>
    public bool EstaActivo => !FechaDesasignacion.HasValue;
    
    /// <summary>
    /// Días de permanencia en el proyecto
    /// </summary>
    public int DiasEnProyecto
    {
        get
        {
            var fechaFin = FechaDesasignacion ?? DateTime.Today;
            return (int)(fechaFin - FechaAsignacion).TotalDays;
        }
    }
    
    /// <summary>
    /// Promedio de horas por día trabajado
    /// </summary>
    public decimal PromedioHorasDia => DiasTrabajados > 0 
        ? Math.Round(TotalHorasTrabajadas / DiasTrabajados, 1) 
        : 0;
    
    /// <summary>
    /// Nombre del rol para mostrar
    /// </summary>
    public string NombreRol => RolEnum.HasValue 
        ? RolEnum.Value.ToString() 
        : Rol ?? "Sin rol";
}


