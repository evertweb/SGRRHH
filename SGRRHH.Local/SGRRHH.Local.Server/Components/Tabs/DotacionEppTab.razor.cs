using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Domain.Services;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Server.Components.Tabs;

public class DotacionEppTabBase : ComponentBase
{
    // ===== PARAMETERS =====
    [Parameter] public int EmpleadoId { get; set; }
    [Parameter] public Empleado? Empleado { get; set; }
    [Parameter] public List<DocumentoEmpleado> Documentos { get; set; } = new();
    [Parameter] public EventCallback OnDocumentosChanged { get; set; }
    
    // EventCallbacks para acciones del componente padre
    [Parameter] public EventCallback<DocumentoEmpleado> OnPrevisualizarDocumento { get; set; }
    [Parameter] public EventCallback<DocumentoEmpleado> OnDescargarDocumento { get; set; }
    [Parameter] public EventCallback<int> OnAbrirScannerActa { get; set; }
    
    // ===== INJECTIONS =====
    [Inject] protected ITallasEmpleadoRepository TallasRepo { get; set; } = default!;
    [Inject] protected IEntregaDotacionRepository EntregaDotacionRepo { get; set; } = default!;
    [Inject] protected IDocumentoEmpleadoRepository DocumentoRepo { get; set; } = default!;
    [Inject] protected ILocalStorageService StorageService { get; set; } = default!;
    [Inject] protected IAuthService AuthService { get; set; } = default!;
    [Inject] protected ILogger<DotacionEppTabBase> Logger { get; set; } = default!;
    
    // ===== STATE =====
    [Parameter] public dynamic? messageToast { get; set; }
    
    protected TallasEmpleado? TallasEmpleado { get; set; }
    protected List<EntregaDotacion> EntregasDotacion { get; set; } = new();
    
    protected int? entregaEditandoId = null;
    protected EntregaDotacion? entregaEnEdicion = null;
    protected List<DetalleEntregaDotacion> detallesEnEdicion = new();
    protected bool isSavingEntrega = false;
    protected bool showTallasForm = false;
    
    protected EntregaDotacion? entregaAEliminar = null;
    protected bool showConfirmDeleteEntrega = false;
    
    // ===== LIFECYCLE =====
    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }
    
    protected override async Task OnParametersSetAsync()
    {
        if (EmpleadoId > 0)
        {
            await CargarDatos();
        }
    }
    
    private async Task CargarDatos()
    {
        try
        {
            // Cargar tallas
            TallasEmpleado = await TallasRepo.GetByEmpleadoIdAsync(EmpleadoId);
            
            // Cargar entregas de dotación
            EntregasDotacion = (await EntregaDotacionRepo.GetByEmpleadoIdWithDetallesAsync(EmpleadoId))
                .OrderByDescending(e => e.FechaEntrega)
                .ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cargando datos de dotación para empleado {EmpleadoId}", EmpleadoId);
            messageToast?.ShowError("Error al cargar datos de dotación");
        }
    }
    
    // ===== MÉTODOS DE TALLAS =====
    
    protected async Task GuardarTallas()
    {
        if (TallasEmpleado == null)
        {
            TallasEmpleado = new TallasEmpleado { EmpleadoId = EmpleadoId };
        }

        isSavingEntrega = true;
        StateHasChanged();

        try
        {
            if (TallasEmpleado.Id == 0)
            {
                await TallasRepo.AddAsync(TallasEmpleado);
            }
            else
            {
                await TallasRepo.UpdateAsync(TallasEmpleado);
            }

            showTallasForm = false;
            messageToast?.ShowSuccess("✓ Tallas guardadas exitosamente");
            Logger.LogInformation("Tallas guardadas para empleado {Id}", EmpleadoId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando tallas");
            messageToast?.ShowError("Error al guardar tallas: " + ex.Message);
        }
        finally
        {
            isSavingEntrega = false;
            StateHasChanged();
        }
    }

    // ===== MÉTODOS DE ENTREGAS =====

    protected void AgregarNuevaEntrega()
    {
        if (TallasEmpleado == null)
        {
            messageToast?.ShowWarning("Primero debe registrar las tallas del empleado");
            showTallasForm = true;
            return;
        }

        entregaEditandoId = -1;
        entregaEnEdicion = new EntregaDotacion
        {
            EmpleadoId = EmpleadoId,
            FechaEntrega = DateTime.Today.AddMonths(1),
            Periodo = $"{DateTime.Today.Year}-{((DateTime.Today.Month - 1) / 4) + 1}",
            TipoEntrega = TipoEntregaDotacion.DotacionLegal,
            Estado = EstadoEntregaDotacion.Programada
        };
        detallesEnEdicion = new List<DetalleEntregaDotacion>();
    }

    protected void EditarEntrega(EntregaDotacion entrega)
    {
        entregaEditandoId = entrega.Id;
        entregaEnEdicion = new EntregaDotacion
        {
            Id = entrega.Id,
            EmpleadoId = entrega.EmpleadoId,
            FechaEntrega = entrega.FechaEntrega,
            Periodo = entrega.Periodo,
            TipoEntrega = entrega.TipoEntrega,
            NumeroEntregaAnual = entrega.NumeroEntregaAnual,
            Estado = entrega.Estado,
            Observaciones = entrega.Observaciones
        };
        detallesEnEdicion = entrega.Detalles.Select(d => new DetalleEntregaDotacion
        {
            Id = d.Id,
            EntregaId = d.EntregaId,
            CategoriaElemento = d.CategoriaElemento,
            NombreElemento = d.NombreElemento,
            Cantidad = d.Cantidad,
            Talla = d.Talla,
            EsDotacionLegal = d.EsDotacionLegal,
            EsEPP = d.EsEPP
        }).ToList();
    }

    protected void CancelarEdicionEntrega()
    {
        entregaEditandoId = null;
        entregaEnEdicion = null;
        detallesEnEdicion.Clear();
    }

    protected async Task GuardarEntregaEditada()
    {
        if (entregaEnEdicion == null) return;

        isSavingEntrega = true;
        StateHasChanged();

        try
        {
            if (entregaEnEdicion.Id == 0)
            {
                var nuevaEntrega = await EntregaDotacionRepo.AddAsync(entregaEnEdicion);
                EntregasDotacion.Insert(0, nuevaEntrega);
            }
            else
            {
                await EntregaDotacionRepo.UpdateAsync(entregaEnEdicion);
                var index = EntregasDotacion.FindIndex(e => e.Id == entregaEnEdicion.Id);
                if (index >= 0)
                {
                    EntregasDotacion[index] = entregaEnEdicion;
                }
            }

            messageToast?.ShowSuccess("✓ Entrega guardada exitosamente");
            CancelarEdicionEntrega();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando entrega");
            messageToast?.ShowError("Error al guardar entrega: " + ex.Message);
        }
        finally
        {
            isSavingEntrega = false;
            StateHasChanged();
        }
    }

    protected async Task MarcarComoEntregada(int entregaId)
    {
        try
        {
            var entrega = EntregasDotacion.FirstOrDefault(e => e.Id == entregaId);
            if (entrega == null) return;

            entrega.Estado = EstadoEntregaDotacion.Entregada;
            entrega.FechaEntregaReal = DateTime.Today;
            entrega.EntregadoPorNombre = AuthService.CurrentUser?.NombreCompleto;
            entrega.EntregadoPorUsuarioId = AuthService.CurrentUser?.Id;

            await EntregaDotacionRepo.UpdateAsync(entrega);
            messageToast?.ShowSuccess("✓ Entrega marcada como completada");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error marcando entrega como entregada");
            messageToast?.ShowError("Error: " + ex.Message);
        }
    }

    protected void MostrarDetalles(EntregaDotacion entrega)
    {
        // Implementar modal o expansión de detalles si es necesario
        messageToast?.ShowInfo($"Entrega {entrega.Periodo}: {entrega.Detalles.Count} elementos");
    }

    protected void ConfirmarEliminarEntrega(EntregaDotacion entrega)
    {
        entregaAEliminar = entrega;
        showConfirmDeleteEntrega = true;
    }

    protected async Task EliminarEntrega()
    {
        if (entregaAEliminar == null) return;

        try
        {
            await EntregaDotacionRepo.DeleteAsync(entregaAEliminar.Id);
            EntregasDotacion.RemoveAll(e => e.Id == entregaAEliminar.Id);
            messageToast?.ShowSuccess("✓ Entrega eliminada");
            Logger.LogInformation("Entrega eliminada: {Id}", entregaAEliminar.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error eliminando entrega {Id}", entregaAEliminar.Id);
            messageToast?.ShowError("Error al eliminar entrega: " + ex.Message);
        }
        finally
        {
            showConfirmDeleteEntrega = false;
            entregaAEliminar = null;
            StateHasChanged();
        }
    }
    public async Task AsociarActaAEntrega(int entregaId, DocumentoEmpleado documento)
    {
        var entrega = EntregasDotacion.FirstOrDefault(e => e.Id == entregaId);
        if (entrega == null) return;

        entrega.DocumentoActaId = documento.Id;
        await EntregaDotacionRepo.UpdateAsync(entrega);
        StateHasChanged();
        Logger.LogInformation("Acta de entrega vinculada a entrega {EntregaId}", entrega.Id);
        messageToast?.ShowSuccess("✓ Acta de entrega vinculada");
    }
}
