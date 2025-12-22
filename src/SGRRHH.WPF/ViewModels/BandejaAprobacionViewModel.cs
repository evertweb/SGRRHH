using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Cloud.Firestore;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Firebase.Repositories;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la bandeja de aprobación de permisos (para aprobadores/admin)
/// Incluye sincronización en tiempo real mediante FirestoreListenerService.
/// </summary>
public partial class BandejaAprobacionViewModel : ViewModelBase, IDisposable
{
    private readonly IPermisoService _permisoService;
    private readonly IDialogService _dialogService;
    private readonly IFirestoreListenerService? _listenerService;
    private readonly PermisoFirestoreRepository? _permisoRepository;
    private string? _subscriptionId;
    private bool _disposed;

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

    [ObservableProperty]
    private bool _isRealTimeEnabled;

    /// <summary>
    /// Evento cuando se aprueba o rechaza un permiso
    /// </summary>
    public event EventHandler? PermisoProcessed;

    public BandejaAprobacionViewModel(
        IPermisoService permisoService,
        IDialogService dialogService,
        IFirestoreListenerService? listenerService = null,
        PermisoFirestoreRepository? permisoRepository = null)
    {
        _permisoService = permisoService;
        _dialogService = dialogService;
        _listenerService = listenerService;
        _permisoRepository = permisoRepository;

        // Verificar si el modo tiempo real está disponible
        IsRealTimeEnabled = _listenerService != null && _permisoRepository != null;
    }
    
    public async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            // Si el listener está disponible, suscribirse para tiempo real
            if (IsRealTimeEnabled && _subscriptionId == null)
            {
                StartRealTimeListening();
            }
            else
            {
                // Fallback: carga tradicional
                await LoadDataTraditionalAsync();
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error al cargar permisos pendientes: {ex.Message}", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Inicia la escucha en tiempo real de permisos pendientes.
    /// </summary>
    private void StartRealTimeListening()
    {
        if (_listenerService == null || _permisoRepository == null) return;

        // Cancelar suscripción anterior si existe
        if (_subscriptionId != null)
        {
            _listenerService.Unsubscribe(_subscriptionId);
        }

        _subscriptionId = _listenerService.Subscribe<Permiso>(
            collectionName: "permisos",
            onSnapshot: OnPermisosUpdated,
            documentToEntity: _permisoRepository.ConvertFromSnapshot,
            onError: ex =>
            {
                _dialogService.ShowError($"Error en sincronización tiempo real: {ex.Message}", "Error");
            },
            queryBuilder: query => query
                .WhereEqualTo("estado", EstadoPermiso.Pendiente.ToString())
                .WhereEqualTo("activo", true)
        );
    }

    /// <summary>
    /// Callback cuando se reciben actualizaciones en tiempo real.
    /// </summary>
    private void OnPermisosUpdated(IEnumerable<Permiso> permisos)
    {
        // Esto ya se ejecuta en el UI thread gracias a FirestoreListenerService
        PermisosPendientes.Clear();
        foreach (var permiso in permisos.OrderBy(p => p.FechaSolicitud))
        {
            PermisosPendientes.Add(permiso);
        }

        TotalPendientes = PermisosPendientes.Count;
        IsLoading = false;
    }

    /// <summary>
    /// Carga tradicional (sin tiempo real) para fallback.
    /// </summary>
    private async Task LoadDataTraditionalAsync()
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
        
        if (_dialogService.Confirm(
            $"¿Está seguro de aprobar el permiso {SelectedPermiso.NumeroActa}?\n\n" +
            $"Empleado: {SelectedPermiso.Empleado.NombreCompleto}\n" +
            $"Tipo: {SelectedPermiso.TipoPermiso.Nombre}\n" +
            $"Fechas: {SelectedPermiso.FechaInicio:dd/MM/yyyy} - {SelectedPermiso.FechaFin:dd/MM/yyyy}",
            "Confirmar Aprobación"))
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
                    _dialogService.ShowSuccess(
                        $"Permiso {SelectedPermiso.NumeroActa} aprobado exitosamente",
                        "Éxito");
                        
                    CancelAction();
                    await LoadDataAsync();
                    PermisoProcessed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _dialogService.ShowError($"Error: {string.Join(", ", approveResult.Errors)}", "Error");
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
        if (SelectedPermiso == null) return;
        
        if (string.IsNullOrWhiteSpace(MotivoRechazo))
        {
            _dialogService.ShowWarning("Debe ingresar el motivo del rechazo", "Validación");
            return;
        }
        
        if (_dialogService.ConfirmWarning(
            $"¿Está seguro de rechazar el permiso {SelectedPermiso.NumeroActa}?\n\n" +
            $"Empleado: {SelectedPermiso.Empleado.NombreCompleto}\n" +
            $"Motivo del rechazo: {MotivoRechazo}",
            "Confirmar Rechazo"))
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
                    _dialogService.ShowInfo(
                        $"Permiso {SelectedPermiso.NumeroActa} rechazado",
                        "Información");
                        
                    CancelAction();
                    await LoadDataAsync();
                    PermisoProcessed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _dialogService.ShowError($"Error: {string.Join(", ", rejectResult.Errors)}", "Error");
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
        if (!PermisosPendientes.Any())
        {
            _dialogService.ShowInfo("No hay permisos pendientes", "Información");
            return;
        }

        if (_dialogService.Confirm(
            $"¿Está seguro de aprobar todos los {PermisosPendientes.Count} permisos pendientes?",
            "Confirmar Aprobación Masiva"))
        {
            IsLoading = true;

            try
            {
                var userId = App.CurrentUser!.Id;
                var permisos = PermisosPendientes.ToList();

                // Ejecutar todas las aprobaciones en paralelo
                var tasks = permisos.Select(async permiso =>
                {
                    try
                    {
                        var result = await _permisoService.AprobarPermisoAsync(permiso.Id, userId);
                        return (permiso.Id, Success: result.Success, Error: result.Success ? null : string.Join(", ", result.Errors));
                    }
                    catch (Exception ex)
                    {
                        return (permiso.Id, Success: false, Error: ex.Message);
                    }
                });

                var results = await Task.WhenAll(tasks);

                int aprobados = results.Count(r => r.Success);
                int errores = results.Count(r => !r.Success);

                if (errores > 0)
                    _dialogService.ShowWarning(
                        $"Proceso completado:\n\n" +
                        $"Aprobados: {aprobados}\n" +
                        $"Errores: {errores}",
                        "Resultado");
                else
                    _dialogService.ShowSuccess(
                        $"Proceso completado:\n\n" +
                        $"Aprobados: {aprobados}\n" +
                        $"Errores: {errores}",
                        "Resultado");

                await LoadDataAsync();
                PermisoProcessed?.Invoke(this, EventArgs.Empty);
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
    
    partial void OnSelectedPermisoChanged(Permiso? value)
    {
        if (value == null)
        {
            ShowApprovalPanel = false;
        }
    }

    /// <summary>
    /// Limpia los recursos y cancela la suscripción al listener.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_subscriptionId != null && _listenerService != null)
        {
            _listenerService.Unsubscribe(_subscriptionId);
            _subscriptionId = null;
        }

        GC.SuppressFinalize(this);
    }
}
