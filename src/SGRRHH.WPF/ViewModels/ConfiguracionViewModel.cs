using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel principal de Configuración que agrupa todas las secciones
/// </summary>
public partial class ConfiguracionViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    
    [ObservableProperty]
    private string _currentSection = "Empresa";
    
    [ObservableProperty]
    private object? _currentSectionView;
    
    [ObservableProperty]
    private ConfiguracionEmpresaViewModel? _empresaViewModel;
    
    [ObservableProperty]
    private BackupViewModel? _backupViewModel;
    
    [ObservableProperty]
    private AuditLogViewModel? _auditLogViewModel;

    [ObservableProperty]
    private SeguridadViewModel? _seguridadViewModel;

    public ConfiguracionViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task LoadDataAsync()
    {
        // Cargar la sección por defecto (Empresa)
        await NavigateToSectionAsync("Empresa");
    }
    
    [RelayCommand]
    private async Task NavigateToSectionAsync(string section)
    {
        CurrentSection = section;
        
        using var scope = _serviceProvider.CreateScope();
        
        switch (section)
        {
            case "Empresa":
                EmpresaViewModel = scope.ServiceProvider.GetRequiredService<ConfiguracionEmpresaViewModel>();
                await EmpresaViewModel.LoadDataAsync();
                CurrentSectionView = EmpresaViewModel;
                break;
                
            case "Backup":
                BackupViewModel = scope.ServiceProvider.GetRequiredService<BackupViewModel>();
                await BackupViewModel.LoadDataAsync();
                CurrentSectionView = BackupViewModel;
                break;
                
            case "Auditoria":
                AuditLogViewModel = scope.ServiceProvider.GetRequiredService<AuditLogViewModel>();
                await AuditLogViewModel.LoadDataAsync();
                CurrentSectionView = AuditLogViewModel;
                break;

            case "Seguridad":
                SeguridadViewModel = scope.ServiceProvider.GetRequiredService<SeguridadViewModel>();
                await SeguridadViewModel.LoadDataAsync();
                CurrentSectionView = SeguridadViewModel;
                break;

            default:
                CurrentSectionView = null;
                break;
        }
    }
}
