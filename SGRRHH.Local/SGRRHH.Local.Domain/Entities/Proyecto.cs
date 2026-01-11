using System.Collections.Generic;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class Proyecto : EntidadBase
{
    // ===== INFORMACIÓN GENERAL =====

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string? Cliente { get; set; }

    /// <summary>
    /// Ubicación general (mantener para compatibilidad)
    /// </summary>
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

    // ===== INFORMACIÓN FORESTAL (Tipo de Proyecto) =====

    /// <summary>
    /// Tipo de proyecto forestal (Plantación Nueva, Mantenimiento, Raleo, etc.)
    /// </summary>
    public TipoProyectoForestal TipoProyecto { get; set; } = TipoProyectoForestal.PlantacionNueva;

    // ===== INFORMACIÓN GEOGRÁFICA Y CATASTRAL =====

    /// <summary>
    /// Nombre del predio o finca donde se ubica el proyecto
    /// </summary>
    public string? Predio { get; set; }

    /// <summary>
    /// Identificación del lote o rodal dentro del predio
    /// </summary>
    public string? Lote { get; set; }

    /// <summary>
    /// Departamento de Colombia donde se ubica
    /// </summary>
    public string? Departamento { get; set; }

    /// <summary>
    /// Municipio donde se ubica el proyecto
    /// </summary>
    public string? Municipio { get; set; }

    /// <summary>
    /// Vereda o zona rural específica
    /// </summary>
    public string? Vereda { get; set; }

    /// <summary>
    /// Latitud del centroide del lote (coordenadas decimales)
    /// </summary>
    public decimal? Latitud { get; set; }

    /// <summary>
    /// Longitud del centroide del lote (coordenadas decimales)
    /// </summary>
    public decimal? Longitud { get; set; }

    /// <summary>
    /// Altitud en metros sobre el nivel del mar
    /// </summary>
    public int? AltitudMsnm { get; set; }

    // ===== INFORMACIÓN SILVICULTURAL =====

    /// <summary>
    /// Id de la especie forestal principal (Pino, Eucalipto, Teca, etc.)
    /// </summary>
    public int? EspecieId { get; set; }

    /// <summary>
    /// Especie forestal relacionada
    /// </summary>
    public EspecieForestal? Especie { get; set; }

    /// <summary>
    /// Área efectiva del proyecto en hectáreas
    /// </summary>
    public decimal? AreaHectareas { get; set; }

    /// <summary>
    /// Fecha de siembra/establecimiento de la plantación
    /// </summary>
    public DateTime? FechaSiembra { get; set; }

    /// <summary>
    /// Densidad inicial de siembra (árboles por hectárea)
    /// </summary>
    public int? DensidadInicial { get; set; }

    /// <summary>
    /// Densidad actual estimada (después de mortalidad y raleos)
    /// </summary>
    public int? DensidadActual { get; set; }

    /// <summary>
    /// Turno de cosecha esperado en años
    /// </summary>
    public int? TurnoCosechaAnios { get; set; }

    /// <summary>
    /// Tipo de tenencia del terreno (Propio, Arrendado, Comodato, etc.)
    /// </summary>
    public TipoTenencia TipoTenencia { get; set; } = TipoTenencia.Propio;

    /// <summary>
    /// Certificación forestal si aplica (FSC, PEFC, etc.)
    /// </summary>
    public string? Certificacion { get; set; }

    // ===== SEGUIMIENTO Y MÉTRICAS =====

    /// <summary>
    /// Total de horas trabajadas en el proyecto (calculado de actividades)
    /// </summary>
    public decimal TotalHorasTrabajadas { get; set; }

    /// <summary>
    /// Costo acumulado de mano de obra
    /// </summary>
    public decimal CostoManoObraAcumulado { get; set; }

    /// <summary>
    /// Número de jornales invertidos
    /// </summary>
    public int TotalJornales { get; set; }

    /// <summary>
    /// Última fecha de actualización de métricas
    /// </summary>
    public DateTime? FechaUltimaActualizacionMetricas { get; set; }

    // ===== PROPIEDADES CALCULADAS =====

    /// <summary>
    /// Edad actual del cultivo en años (calculado desde FechaSiembra)
    /// </summary>
    public int EdadCultivoAnios => FechaSiembra.HasValue 
        ? (int)((DateTime.Today - FechaSiembra.Value).TotalDays / 365.25) 
        : 0;

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

    /// <summary>
    /// Ubicación completa formateada (Vereda, Municipio, Departamento)
    /// </summary>
    public string UbicacionCompleta
    {
        get
        {
            var partes = new List<string>();
            if (!string.IsNullOrEmpty(Vereda)) partes.Add(Vereda);
            if (!string.IsNullOrEmpty(Municipio)) partes.Add(Municipio);
            if (!string.IsNullOrEmpty(Departamento)) partes.Add(Departamento);
            return partes.Count > 0 ? string.Join(", ", partes) : (Ubicacion ?? "-");
        }
    }

    /// <summary>
    /// Descripción breve del predio y lote
    /// </summary>
    public string PredioLote
    {
        get
        {
            if (!string.IsNullOrEmpty(Predio) && !string.IsNullOrEmpty(Lote))
                return $"{Predio} - Lote {Lote}";
            if (!string.IsNullOrEmpty(Predio))
                return Predio;
            if (!string.IsNullOrEmpty(Lote))
                return $"Lote {Lote}";
            return "-";
        }
    }
}

public enum EstadoProyecto
{
    Activo = 0,
    Suspendido = 1,
    Finalizado = 2,
    Cancelado = 3
}


