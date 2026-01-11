using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IProyectoEmpleadoRepository : IRepository<ProyectoEmpleado>
{
    // ===== Consultas por Proyecto =====
    
    Task<IEnumerable<ProyectoEmpleado>> GetByProyectoAsync(int proyectoId);
    Task<IEnumerable<ProyectoEmpleado>> GetActiveByProyectoAsync(int proyectoId);
    Task<int> GetCountByProyectoAsync(int proyectoId);
    Task<int> GetActiveCountByProyectoAsync(int proyectoId);
    
    // ===== Consultas por Empleado =====
    
    Task<IEnumerable<ProyectoEmpleado>> GetByEmpleadoAsync(int empleadoId);
    Task<IEnumerable<ProyectoEmpleado>> GetActiveByEmpleadoAsync(int empleadoId);
    Task<IEnumerable<Proyecto>> GetProyectosActivosByEmpleadoAsync(int empleadoId);
    
    // ===== Gestión de Asignaciones =====
    
    Task<bool> ExistsAsignacionAsync(int proyectoId, int empleadoId);
    Task<bool> ExistsAsignacionActivaAsync(int proyectoId, int empleadoId);
    Task<ProyectoEmpleado?> GetAsignacionAsync(int proyectoId, int empleadoId);
    Task<ProyectoEmpleado?> GetAsignacionActivaAsync(int proyectoId, int empleadoId);
    Task DesasignarAsync(int proyectoId, int empleadoId, string? motivo = null);
    
    // ===== Asignación Masiva =====
    
    Task AsignarMultiplesAsync(int proyectoId, IEnumerable<int> empleadoIds, RolProyectoForestal? rol = null);
    Task DesasignarMultiplesAsync(int proyectoId, IEnumerable<int> empleadoIds, string? motivo = null);
    
    // ===== Consultas por Rol =====
    
    Task<IEnumerable<ProyectoEmpleado>> GetByRolAsync(int proyectoId, RolProyectoForestal rol);
    Task<ProyectoEmpleado?> GetLiderCuadrillaAsync(int proyectoId);
    
    // ===== Métricas =====
    
    Task ActualizarMetricasAsync(int proyectoEmpleadoId);
    Task ActualizarMetricasProyectoAsync(int proyectoId);
    
    // ===== Reportes =====
    
    Task<IEnumerable<EmpleadoProyectoResumen>> GetResumenPorEmpleadoAsync(int proyectoId);
    Task<IEnumerable<ProyectoEmpleadoHistorial>> GetHistorialAsync(int proyectoId);
    
    // ===== Consultas con Include =====
    
    Task<IEnumerable<ProyectoEmpleado>> GetActiveByProyectoWithEmpleadoAsync(int proyectoId);
}

/// <summary>
/// Resumen de participación de un empleado en un proyecto
/// </summary>
public class EmpleadoProyectoResumen
{
    public int EmpleadoId { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public decimal TotalHoras { get; set; }
    public int TotalJornales { get; set; }
    public int DiasTrabajados { get; set; }
    public DateTime? UltimaActividad { get; set; }
    public bool Activo { get; set; }
}

/// <summary>
/// Registro de historial de asignaciones
/// </summary>
public class ProyectoEmpleadoHistorial
{
    public int EmpleadoId { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }
    public DateTime? FechaDesasignacion { get; set; }
    public string? MotivoDesasignacion { get; set; }
    public int DiasEnProyecto { get; set; }
    public decimal HorasTrabajadas { get; set; }
}


