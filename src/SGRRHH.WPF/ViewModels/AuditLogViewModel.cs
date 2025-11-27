using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el log de auditoría
/// </summary>
public partial class AuditLogViewModel : ObservableObject
{
    private readonly IAuditService _auditService;
    
    [ObservableProperty]
    private ObservableCollection<AuditLog> _registros = new();
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _mensaje;
    
    [ObservableProperty]
    private DateTime? _fechaDesde;
    
    [ObservableProperty]
    private DateTime? _fechaHasta;
    
    [ObservableProperty]
    private string? _filtroEntidad;
    
    [ObservableProperty]
    private int _maxRegistros = 100;
    
    // Lista de entidades para filtro
    public List<string> Entidades { get; } = new()
    {
        "",
        "Usuario",
        "Empleado",
        "Permiso",
        "Vacacion",
        "Contrato",
        "RegistroDiario",
        "Backup"
    };
    
    public AuditLogViewModel(IAuditService auditService)
    {
        _auditService = auditService;
        FechaDesde = DateTime.Today.AddDays(-30);
        FechaHasta = DateTime.Today.AddDays(1);
    }
    
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        Mensaje = null;
        
        try
        {
            var registros = await _auditService.ObtenerRegistrosAsync(
                FechaDesde,
                FechaHasta,
                string.IsNullOrWhiteSpace(FiltroEntidad) ? null : FiltroEntidad,
                null,
                MaxRegistros);
            
            Registros.Clear();
            foreach (var registro in registros)
            {
                Registros.Add(registro);
            }
            
            if (registros.Count == 0)
            {
                Mensaje = "No se encontraron registros con los filtros seleccionados";
            }
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al cargar registros: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task BuscarAsync()
    {
        await LoadDataAsync();
    }
    
    [RelayCommand]
    private async Task LimpiarFiltrosAsync()
    {
        FechaDesde = DateTime.Today.AddDays(-30);
        FechaHasta = DateTime.Today.AddDays(1);
        FiltroEntidad = null;
        await LoadDataAsync();
    }
    
    [RelayCommand]
    private async Task LimpiarRegistrosAntiguosAsync()
    {
        var confirmacion = MessageBox.Show(
            "¿Está seguro de eliminar los registros de auditoría anteriores a 90 días?\n\nEsta acción no se puede deshacer.",
            "Confirmar Limpieza",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        
        if (confirmacion != MessageBoxResult.Yes)
            return;
        
        IsLoading = true;
        
        try
        {
            var eliminados = await _auditService.LimpiarRegistrosAntiguosAsync(90);
            MessageBox.Show($"Se eliminaron {eliminados} registros antiguos", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
