using System.Collections.Generic;

namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa un proyecto o labor de la empresa
/// </summary>
public class Proyecto : EntidadBase
{
    /// <summary>
    /// Código único del proyecto
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del proyecto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del proyecto
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Cliente o entidad para la cual se realiza el proyecto
    /// </summary>
    public string? Cliente { get; set; }

    /// <summary>
    /// Ubicación o lugar donde se realiza el proyecto
    /// </summary>
    public string? Ubicacion { get; set; }

    /// <summary>
    /// Presupuesto estimado del proyecto
    /// </summary>
    public decimal? Presupuesto { get; set; }

    /// <summary>
    /// Porcentaje de progreso del proyecto (0-100)
    /// </summary>
    public int Progreso { get; set; } = 0;

    /// <summary>
    /// Fecha de inicio del proyecto
    /// </summary>
    public DateTime? FechaInicio { get; set; }

    /// <summary>
    /// Fecha estimada o real de finalización
    /// </summary>
    public DateTime? FechaFin { get; set; }

    /// <summary>
    /// Estado actual del proyecto
    /// </summary>
    public EstadoProyecto Estado { get; set; } = EstadoProyecto.Activo;

    /// <summary>
    /// ID del empleado responsable del proyecto
    /// </summary>
    public int? ResponsableId { get; set; }

    /// <summary>
    /// Empleado responsable del proyecto
    /// </summary>
    public Empleado? Responsable { get; set; }

    /// <summary>
    /// Registros diarios asociados al proyecto
    /// </summary>
    public ICollection<DetalleActividad>? DetallesActividades { get; set; }

    /// <summary>
    /// Empleados asignados al proyecto
    /// </summary>
    public ICollection<ProyectoEmpleado>? ProyectoEmpleados { get; set; }

    /// <summary>
    /// Indica si el proyecto está próximo a vencer (menos de 7 días para la fecha fin)
    /// </summary>
    public bool EstaProximoAVencer =>
        FechaFin.HasValue &&
        Estado == EstadoProyecto.Activo &&
        FechaFin.Value > DateTime.Today &&
        (FechaFin.Value - DateTime.Today).TotalDays <= 7;

    /// <summary>
    /// Indica si el proyecto está vencido (pasó la fecha fin y aún está activo)
    /// </summary>
    public bool EstaVencido =>
        FechaFin.HasValue &&
        Estado == EstadoProyecto.Activo &&
        FechaFin.Value < DateTime.Today;

    /// <summary>
    /// Cantidad de empleados asignados al proyecto
    /// </summary>
    public int CantidadEmpleados => ProyectoEmpleados?.Count ?? 0;
}

/// <summary>
/// Estados posibles de un proyecto
/// </summary>
public enum EstadoProyecto
{
    Activo = 0,
    Suspendido = 1,
    Finalizado = 2,
    Cancelado = 3
}
