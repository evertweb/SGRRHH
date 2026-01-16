using Microsoft.AspNetCore.Components;
using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Server.Components.ControlDiario;

public partial class RegistroAsistenciaModal : ComponentBase
{
    [Parameter] public RegistroDiario? RegistroSeleccionado { get; set; }

    [Parameter] public bool MostrarDetalle { get; set; }

    [Parameter] public bool EditandoObservaciones { get; set; }

    [Parameter] public EventCallback OnCerrar { get; set; }

    [Parameter] public EventCallback OnGuardarObservaciones { get; set; }

    [Parameter] public EventCallback OnEditarObservaciones { get; set; }

    [Parameter] public EventCallback OnCancelarObservaciones { get; set; }

    [Parameter] public EventCallback OnAgregarActividad { get; set; }

    [Parameter] public EventCallback<DetalleActividad> OnEditarActividad { get; set; }

    [Parameter] public EventCallback<DetalleActividad> OnEliminarActividad { get; set; }

    private List<DetalleActividad> DetallesOrdenados =>
        RegistroSeleccionado?.DetallesActividades?.OrderBy(d => d.Orden).ToList()
        ?? new List<DetalleActividad>();

    private string GetClasificacionCss(string clasificacion)
    {
        return clasificacion switch
        {
            "EXCELENTE" => "productividad-excelente",
            "ÓPTIMO" => "productividad-optimo",
            "ACEPTABLE" => "productividad-aceptable",
            "BAJO" => "productividad-bajo",
            "CRÍTICO" => "productividad-critico",
            _ => ""
        };
    }

    private string GetClasificacionTexto(string clasificacion)
    {
        return clasificacion switch
        {
            "EXCELENTE" => "Supera el rendimiento esperado",
            "ÓPTIMO" => "Alcanza el rendimiento esperado",
            "ACEPTABLE" => "Rendimiento aceptable",
            "BAJO" => "Rendimiento por debajo de lo esperado",
            "CRÍTICO" => "Rendimiento muy bajo",
            _ => "Sin datos de rendimiento"
        };
    }
}
