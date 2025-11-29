using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el diálogo de actualización
/// Flujo simplificado: siempre en primer plano, sin opción de instalar al cerrar
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

    [ObservableProperty]
    private bool _canClose = true;
    
    /// <summary>
    /// Resultado del diálogo: true = actualizar ahora, false = más tarde, null = omitir versión
    /// </summary>
    public bool? DialogResult { get; private set; }

    /// <summary>
    /// Indica si se debe instalar al cerrar la aplicación (siempre false en flujo simplificado)
    /// </summary>
    public bool InstallOnExit { get; private set; } = false;
    
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
            DownloadSize = "~12 MB";
        }
    }
    
    /// <summary>
    /// Actualizar ahora: descarga, muestra progreso, y cuando termina cierra la app para instalar
    /// </summary>
    [RelayCommand]
    private async Task UpdateNowAsync()
    {
        try
        {
            IsDownloading = true;
            CanClose = false;
            HasError = false;
            StatusMessage = "Conectando al servidor...";
            
            var progress = new Progress<UpdateProgress>(p =>
            {
                DownloadProgress = p.Percentage;
                
                // Mensajes más descriptivos según la fase
                StatusMessage = p.Phase switch
                {
                    UpdatePhase.Downloading => $"Descargando actualización... {p.Percentage}%",
                    UpdatePhase.Verifying => "Verificando integridad del archivo...",
                    UpdatePhase.Completed => "¡Descarga completada!",
                    UpdatePhase.Error => p.Message,
                    _ => p.Message ?? "Procesando..."
                };
                
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
                StatusMessage = "✅ Actualización descargada correctamente.\n\nAl hacer clic en 'Instalar y reiniciar', la aplicación se cerrará y se instalará la actualización automáticamente.";
                CanClose = true;
            }
            else
            {
                HasError = true;
                CanClose = true;
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    ErrorMessage = "Error durante la descarga. Verifique su conexión a internet e intente de nuevo.";
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            CanClose = true;
            ErrorMessage = $"Error inesperado: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
        }
    }
    
    /// <summary>
    /// Recordar después: cierra el diálogo sin hacer nada
    /// </summary>
    [RelayCommand]
    private void RemindLater()
    {
        if (!CanClose) return;
        DialogResult = false;
        CloseDialog?.Invoke();
    }
    
    /// <summary>
    /// Omitir esta versión: cierra el diálogo sin hacer nada
    /// </summary>
    [RelayCommand]
    private void SkipVersion()
    {
        if (!CanClose) return;
        DialogResult = null;
        CloseDialog?.Invoke();
    }
    
    /// <summary>
    /// Instalar y reiniciar: lanza el updater y cierra la aplicación
    /// </summary>
    [RelayCommand]
    private async Task InstallAndRestartAsync()
    {
        if (!DownloadCompleted) return;
        
        try
        {
            StatusMessage = "Preparando instalación...";
            CanClose = false;
            
            // Lanzar el proceso de actualización (updater.exe)
            var applyResult = await _updateService.ApplyUpdateAsync();
            
            if (applyResult)
            {
                DialogResult = true;
                CloseDialog?.Invoke();
                // La aplicación se cerrará desde App.xaml.cs
            }
            else
            {
                HasError = true;
                ErrorMessage = "No se pudo iniciar el instalador. Por favor, cierre la aplicación manualmente e intente de nuevo.";
                CanClose = true;
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error al preparar instalación: {ex.Message}";
            CanClose = true;
        }
    }
}
