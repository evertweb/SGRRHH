using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la bandeja de aprobación de vacaciones (para aprobadores/admin)
/// </summary>
public partial class BandejaVacacionesViewModel : ViewModelBase
{
    private readonly IVacacionService _vacacionService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Vacacion> _vacacionesPendientes = new();

    [ObservableProperty]
    private Vacacion? _selectedVacacion;

    [ObservableProperty]
    private int _totalPendientes;

    [ObservableProperty]
    private string _motivoRechazo = string.Empty;

    [ObservableProperty]
    private bool _showApprovalPanel;

    [ObservableProperty]
    private bool _isApproving;

    [ObservableProperty]
    private bool _isRejecting;

    /// <summary>
    /// Evento cuando se aprueba o rechaza una vacación
    /// </summary>
    public event EventHandler? VacacionProcessed;

    public BandejaVacacionesViewModel(
        IVacacionService vacacionService,
        IDialogService dialogService)
    {
        _vacacionService = vacacionService;
        _dialogService = dialogService;
    }
    
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Cargando vacaciones pendientes...";

        try
        {
            var result = await _vacacionService.GetVacacionesPendientesAsync();

            if (result.Success && result.Data != null)
            {
                VacacionesPendientes.Clear();
                foreach (var vacacion in result.Data)
                {
                    VacacionesPendientes.Add(vacacion);
                }

                TotalPendientes = VacacionesPendientes.Count;
                StatusMessage = $"{TotalPendientes} solicitudes pendientes";
            }
            else
            {
                _dialogService.ShowError($"Error al cargar vacaciones: {result.Message}", "Error");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error al cargar vacaciones pendientes: {ex.Message}", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void SelectForApproval(Vacacion? vacacion)
    {
        if (vacacion == null) return;
        
        SelectedVacacion = vacacion;
        ShowApprovalPanel = true;
        IsApproving = true;
        IsRejecting = false;
        MotivoRechazo = string.Empty;
    }
    
    [RelayCommand]
    private void SelectForRejection(Vacacion? vacacion)
    {
        if (vacacion == null) return;
        
        SelectedVacacion = vacacion;
        ShowApprovalPanel = true;
        IsApproving = false;
        IsRejecting = true;
        MotivoRechazo = string.Empty;
    }
    
    [RelayCommand]
    private void CancelAction()
    {
        ShowApprovalPanel = false;
        SelectedVacacion = null;
        IsApproving = false;
        IsRejecting = false;
        MotivoRechazo = string.Empty;
    }
    
    [RelayCommand]
    private async Task ConfirmApproval()
    {
        if (SelectedVacacion == null) return;
        
        var empleadoNombre = SelectedVacacion.Empleado?.NombreCompleto ?? "Empleado";
        
        if (_dialogService.Confirm(
            $"¿Está seguro de aprobar la solicitud de vacaciones?\n\n" +
            $"Empleado: {empleadoNombre}\n" +
            $"Fechas: {SelectedVacacion.FechaInicio:dd/MM/yyyy} - {SelectedVacacion.FechaFin:dd/MM/yyyy}\n" +
            $"Días: {SelectedVacacion.DiasTomados}",
            "Confirmar Aprobación"))
        {
            IsLoading = true;
            
            try
            {
                var approveResult = await _vacacionService.AprobarVacacionAsync(
                    SelectedVacacion.Id, 
                    App.CurrentUser!.Id);
                    
                if (approveResult.Success)
                {
                    _dialogService.ShowSuccess(
                        $"Vacaciones de {empleadoNombre} aprobadas exitosamente",
                        "Éxito");
                        
                    CancelAction();
                    await LoadDataAsync();
                    VacacionProcessed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _dialogService.ShowError($"Error: {approveResult.Message}", "Error");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error: {ex.Message}", "Error");
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
        if (SelectedVacacion == null) return;
        
        if (string.IsNullOrWhiteSpace(MotivoRechazo))
        {
            _dialogService.ShowWarning("Debe ingresar el motivo del rechazo", "Validación");
            return;
        }
        
        var empleadoNombre = SelectedVacacion.Empleado?.NombreCompleto ?? "Empleado";
        
        if (_dialogService.ConfirmWarning(
            $"¿Está seguro de rechazar la solicitud de vacaciones?\n\n" +
            $"Empleado: {empleadoNombre}\n" +
            $"Motivo del rechazo: {MotivoRechazo}",
            "Confirmar Rechazo"))
        {
            IsLoading = true;
            
            try
            {
                var rejectResult = await _vacacionService.RechazarVacacionAsync(
                    SelectedVacacion.Id, 
                    App.CurrentUser!.Id,
                    MotivoRechazo);
                    
                if (rejectResult.Success)
                {
                    _dialogService.ShowInfo(
                        $"Vacaciones de {empleadoNombre} rechazadas",
                        "Información");
                        
                    CancelAction();
                    await LoadDataAsync();
                    VacacionProcessed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _dialogService.ShowError($"Error: {rejectResult.Message}", "Error");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error: {ex.Message}", "Error");
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
        if (!VacacionesPendientes.Any())
        {
            _dialogService.ShowInfo("No hay vacaciones pendientes", "Información");
            return;
        }

        if (_dialogService.Confirm(
            $"¿Está seguro de aprobar todas las {VacacionesPendientes.Count} solicitudes pendientes?",
            "Confirmar Aprobación Masiva"))
        {
            IsLoading = true;

            try
            {
                var userId = App.CurrentUser!.Id;
                var vacaciones = VacacionesPendientes.ToList();

                int aprobados = 0;
                int errores = 0;

                foreach (var vacacion in vacaciones)
                {
                    try
                    {
                        var result = await _vacacionService.AprobarVacacionAsync(vacacion.Id, userId);
                        if (result.Success)
                            aprobados++;
                        else
                            errores++;
                    }
                    catch
                    {
                        errores++;
                    }
                }

                if (errores > 0)
                    _dialogService.ShowWarning(
                        $"Proceso completado:\n\n" +
                        $"Aprobados: {aprobados}\n" +
                        $"Errores: {errores}",
                        "Resultado");
                else
                    _dialogService.ShowSuccess(
                        $"Todas las {aprobados} solicitudes fueron aprobadas",
                        "Resultado");

                await LoadDataAsync();
                VacacionProcessed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error: {ex.Message}", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    
    partial void OnSelectedVacacionChanged(Vacacion? value)
    {
        if (value == null)
        {
            ShowApprovalPanel = false;
        }
    }
}
