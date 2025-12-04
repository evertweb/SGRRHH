using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SGRRHH.WPF.Messages;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la vista de Catálogos
/// </summary>
public partial class CatalogosViewModel : ViewModelBase
{
    [ObservableProperty]
    private DateTime fechaActual = DateTime.Now;
    
    public CatalogosViewModel()
    {
    }
    
    [RelayCommand]
    private void NavigateToDepartamentos()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Departamentos"));
    }
    
    [RelayCommand]
    private void NavigateToCargos()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Cargos"));
    }
    
    [RelayCommand]
    private void NavigateToProyectos()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Proyectos"));
    }
    
    [RelayCommand]
    private void NavigateToActividades()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Actividades"));
    }
    
    [RelayCommand]
    private void NavigateToTiposPermiso()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("TiposPermiso"));
    }
}
