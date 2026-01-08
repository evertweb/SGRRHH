using System.Collections.Generic;

namespace SGRRHH.Local.Domain.Entities;

public class Proyecto : EntidadBase
{
    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string? Cliente { get; set; }

    public string? Ubicacion { get; set; }

    public decimal? Presupuesto { get; set; }

    public int Progreso { get; set; } = 0;

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public EstadoProyecto Estado { get; set; } = EstadoProyecto.Activo;

    public int? ResponsableId { get; set; }

    public Empleado? Responsable { get; set; }

    public ICollection<DetalleActividad>? DetallesActividades { get; set; }

    public ICollection<ProyectoEmpleado>? ProyectoEmpleados { get; set; }

    public bool EstaProximoAVencer =>
        FechaFin.HasValue &&
        Estado == EstadoProyecto.Activo &&
        FechaFin.Value > DateTime.Today &&
        (FechaFin.Value - DateTime.Today).TotalDays <= 7;

    public bool EstaVencido =>
        FechaFin.HasValue &&
        Estado == EstadoProyecto.Activo &&
        FechaFin.Value < DateTime.Today;

    public int CantidadEmpleados => ProyectoEmpleados?.Count ?? 0;
}

public enum EstadoProyecto
{
    Activo = 0,
    Suspendido = 1,
    Finalizado = 2,
    Cancelado = 3
}


