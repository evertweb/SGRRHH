using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el diálogo de actualización
/// </summary>
public partial class UpdateDialogViewModel : ObservableObject
{
    private readonly IUpdateService _updateService;
    
    [ObservableProperty]
    private string _currentVersion = "1.0.0";
    
    [ObservableProperty]
    private string _newVersion = "";
    
    [ObservableProperty]
    private string _releaseNotes = "";
    
    [ObservableProperty]
    private DateTime _releaseDate;
    
    [ObservableProperty]
    private string _downloadSize = "";
    
    [ObservableProperty]
    private bool _isMandatory;
    
    [ObservableProperty]
    private bool _isDownloading;
    
    [ObservableProperty]
    private int _downloadProgress;
    
    [ObservableProperty]
    private string _statusMessage = "";
    
    [ObservableProperty]
    private bool _downloadCompleted;
    
    [ObservableProperty]
    private bool _hasError;
    
    [ObservableProperty]
    private string _errorMessage = "";
    
    /// <summary>
    /// Resultado del diálogo: true = actualizar ahora, false = más tarde, null = omitir versión
    /// </summary>
    public bool? DialogResult { get; private set; }

    /// <summary>
    /// Indica si se debe instalar al cerrar la aplicación
    /// </summary>
    public bool InstallOnExit { get; private set; }
    
    /// <summary>
    /// Acción para cerrar el diálogo
    /// </summary>
    public Action? CloseDialog { get; set; }
    
    public UpdateDialogViewModel(IUpdateService updateService, VersionInfo newVersionInfo)
    {
        _updateService = updateService;
        
        CurrentVersion = updateService.GetCurrentVersion();
        NewVersion = newVersionInfo.Version;
        ReleaseNotes = newVersionInfo.ReleaseNotes ?? "Sin notas de versión disponibles.";
        ReleaseDate = newVersionInfo.ReleaseDate;
        IsMandatory = newVersionInfo.Mandatory;
        
        // Formatear tamaño de descarga
        if (newVersionInfo.DownloadSize > 0)
        {
            var sizeMb = newVersionInfo.DownloadSize / 1024.0 / 1024.0;
            DownloadSize = sizeMb >= 1 
                ? $"{sizeMb:F1} MB" 
                : $"{newVersionInfo.DownloadSize / 1024.0:F0} KB";
        }
        else
        {
            DownloadSize = "Desconocido";
        }
    }
    
    [RelayCommand]
    private async Task UpdateNowAsync()
    {
        try
        {
            IsDownloading = true;
            HasError = false;
            StatusMessage = "Preparando descarga...";
            
            var progress = new Progress<UpdateProgress>(p =>
            {
                DownloadProgress = p.Percentage;
                StatusMessage = p.Message;
                
                if (p.Phase == UpdatePhase.Error)
                {
                    HasError = true;
                    ErrorMessage = p.Message;
                }
            });
            
            var success = await _updateService.DownloadUpdateAsync(progress);
            
            if (success)
            {
                StatusMessage = "Preparando instalación...";
                var applyResult = await _updateService.ApplyUpdateAsync();
                
                if (applyResult)
                {
                    DownloadCompleted = true;
                    StatusMessage = "Actualización lista. La aplicación se reiniciará para completar la instalación.";
                    DialogResult = true;
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "No se pudo preparar la instalación.";
                }
            }
            else
            {
                HasError = true;
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    ErrorMessage = "Error durante la descarga.";
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
        }
    }
    
    [RelayCommand]
    private void RemindLater()
    {
        DialogResult = false;
        CloseDialog?.Invoke();
    }
    
    [RelayCommand]
    private void SkipVersion()
    {
        DialogResult = null;
        CloseDialog?.Invoke();
    }
    
    [RelayCommand]
    private void RestartNow()
    {
        DialogResult = true;
        InstallOnExit = false;
        CloseDialog?.Invoke();
    }

    [RelayCommand]
    private async Task InstallOnExitAsync()
    {
        try
        {
            IsDownloading = true;
            HasError = false;
            StatusMessage = "Preparando descarga...";

            var progress = new Progress<UpdateProgress>(p =>
            {
                DownloadProgress = p.Percentage;
                StatusMessage = p.Message;

                if (p.Phase == UpdatePhase.Error)
                {
                    HasError = true;
                    ErrorMessage = p.Message;
                }
            });

            var success = await _updateService.DownloadUpdateAsync(progress);

            if (success)
            {
                DownloadCompleted = true;
                InstallOnExit = true;
                StatusMessage = "La actualización se instalará cuando cierre la aplicación.";
                DialogResult = false; // No reiniciar ahora

                // Cerrar el diálogo después de 2 segundos
                await Task.Delay(2000);
                CloseDialog?.Invoke();
            }
            else
            {
                HasError = true;
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    ErrorMessage = "Error durante la descarga.";
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
        }
    }
}
