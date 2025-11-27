using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la bandeja de aprobación de permisos (para aprobadores/admin)
/// </summary>
public partial class BandejaAprobacionViewModel : ObservableObject
{
    private readonly IPermisoService _permisoService;
    
    [ObservableProperty]
    private ObservableCollection<Permiso> _permisosPendientes = new();
    
    [ObservableProperty]
    private Permiso? _selectedPermiso;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private int _totalPendientes;
    
    [ObservableProperty]
    private string _observacionesAprobacion = string.Empty;
    
    [ObservableProperty]
    private string _motivoRechazo = string.Empty;
    
    [ObservableProperty]
    private bool _showApprovalPanel;
    
    [ObservableProperty]
    private bool _isApproving;
    
    [ObservableProperty]
    private bool _isRejecting;
    
    /// <summary>
    /// Evento cuando se aprueba o rechaza un permiso
    /// </summary>
    public event EventHandler? PermisoProcessed;
    
    public BandejaAprobacionViewModel(IPermisoService permisoService)
    {
        _permisoService = permisoService;
    }
    
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        
        try
        {
            var result = await _permisoService.GetPendientesAsync();
            
            if (result.Success && result.Data != null)
            {
                PermisosPendientes.Clear();
                foreach (var permiso in result.Data.OrderBy(p => p.FechaSolicitud))
                {
                    PermisosPendientes.Add(permiso);
                }
                
                TotalPendientes = PermisosPendientes.Count;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar permisos pendientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void SelectForApproval(Permiso? permiso)
    {
        if (permiso == null) return;
        
        SelectedPermiso = permiso;
        ShowApprovalPanel = true;
        IsApproving = true;
        IsRejecting = false;
        ObservacionesAprobacion = string.Empty;
        MotivoRechazo = string.Empty;
    }
    
    [RelayCommand]
    private void SelectForRejection(Permiso? permiso)
    {
        if (permiso == null) return;
        
        SelectedPermiso = permiso;
        ShowApprovalPanel = true;
        IsApproving = false;
        IsRejecting = true;
        ObservacionesAprobacion = string.Empty;
        MotivoRechazo = string.Empty;
    }
    
    [RelayCommand]
    private void CancelAction()
    {
        ShowApprovalPanel = false;
        SelectedPermiso = null;
        IsApproving = false;
        IsRejecting = false;
        ObservacionesAprobacion = string.Empty;
        MotivoRechazo = string.Empty;
    }
    
    [RelayCommand]
    private async Task ConfirmApproval()
    {
        if (SelectedPermiso == null) return;
        
        var result = MessageBox.Show(
            $"¿Está seguro de aprobar el permiso {SelectedPermiso.NumeroActa}?\n\n" +
            $"Empleado: {SelectedPermiso.Empleado.NombreCompleto}\n" +
            $"Tipo: {SelectedPermiso.TipoPermiso.Nombre}\n" +
            $"Fechas: {SelectedPermiso.FechaInicio:dd/MM/yyyy} - {SelectedPermiso.FechaFin:dd/MM/yyyy}",
            "Confirmar Aprobación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            IsLoading = true;
            
            try
            {
                var approveResult = await _permisoService.AprobarPermisoAsync(
                    SelectedPermiso.Id, 
                    App.CurrentUser!.Id,
                    string.IsNullOrWhiteSpace(ObservacionesAprobacion) ? null : ObservacionesAprobacion);
                    
                if (approveResult.Success)
                {
                    MessageBox.Show(
                        $"Permiso {SelectedPermiso.NumeroActa} aprobado exitosamente",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                        
                    CancelAction();
                    await LoadDataAsync();
                    PermisoProcessed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show($"Error: {string.Join(", ", approveResult.Errors)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
    
    [RelayCommand]
    private async Task ConfirmRejection()
    {
        if (SelectedPermiso == null) return;
        
        if (string.IsNullOrWhiteSpace(MotivoRechazo))
        {
            MessageBox.Show("Debe ingresar el motivo del rechazo", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var result = MessageBox.Show(
            $"¿Está seguro de rechazar el permiso {SelectedPermiso.NumeroActa}?\n\n" +
            $"Empleado: {SelectedPermiso.Empleado.NombreCompleto}\n" +
            $"Motivo del rechazo: {MotivoRechazo}",
            "Confirmar Rechazo",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
            
        if (result == MessageBoxResult.Yes)
        {
            IsLoading = true;
            
            try
            {
                var rejectResult = await _permisoService.RechazarPermisoAsync(
                    SelectedPermiso.Id, 
                    App.CurrentUser!.Id,
                    MotivoRechazo);
                    
                if (rejectResult.Success)
                {
                    MessageBox.Show(
                        $"Permiso {SelectedPermiso.NumeroActa} rechazado",
                        "Información",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                        
                    CancelAction();
                    await LoadDataAsync();
                    PermisoProcessed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show($"Error: {string.Join(", ", rejectResult.Errors)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
    
    [RelayCommand]
    private async Task ApproveAll()
    {
        if (!PermisosPendientes.Any())
        {
            MessageBox.Show("No hay permisos pendientes", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"¿Está seguro de aprobar todos los {PermisosPendientes.Count} permisos pendientes?",
            "Confirmar Aprobación Masiva",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            IsLoading = true;
            int aprobados = 0;
            int errores = 0;
            
            try
            {
                foreach (var permiso in PermisosPendientes.ToList())
                {
                    var approveResult = await _permisoService.AprobarPermisoAsync(permiso.Id, App.CurrentUser!.Id);
                    
                    if (approveResult.Success)
                    {
                        aprobados++;
                    }
                    else
                    {
                        errores++;
                    }
                }
                
                MessageBox.Show(
                    $"Proceso completado:\n\n" +
                    $"Aprobados: {aprobados}\n" +
                    $"Errores: {errores}",
                    "Resultado",
                    MessageBoxButton.OK,
                    errores > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
                    
                await LoadDataAsync();
                PermisoProcessed?.Invoke(this, EventArgs.Empty);
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
    
    partial void OnSelectedPermisoChanged(Permiso? value)
    {
        if (value == null)
        {
            ShowApprovalPanel = false;
        }
    }
}
