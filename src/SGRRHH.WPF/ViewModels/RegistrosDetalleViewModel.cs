using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la ventana de previsualización detallada de uno o múltiples registros diarios
/// </summary>
public partial class RegistrosDetalleViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    [ObservableProperty]
    private ObservableCollection<RegistroDiario> _registros = new();

    [ObservableProperty]
    private RegistroDiario? _selectedRegistro;

    [ObservableProperty]
    private decimal _totalHoras;

    [ObservableProperty]
    private int _totalActividades;

    [ObservableProperty]
    private string _titulo = "Detalles de Registros";

    public RegistrosDetalleViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    /// <summary>
    /// Inicializa el ViewModel con una lista de registros
    /// </summary>
    public void Initialize(IEnumerable<RegistroDiario> registros)
    {
        Registros.Clear();
        foreach (var registro in registros.OrderBy(r => r.Fecha))
        {
            Registros.Add(registro);
        }

        // Seleccionar el primer registro por defecto
        SelectedRegistro = Registros.FirstOrDefault();

        // Calcular totales
        TotalHoras = Registros.Sum(r => r.TotalHoras);
        TotalActividades = Registros.Sum(r => r.DetallesActividades?.Count ?? 0);

        // Actualizar título
        if (Registros.Count == 1)
        {
            Titulo = $"Detalle del Registro - {Registros[0].Fecha:dd/MM/yyyy}";
        }
        else
        {
            Titulo = $"Detalle de {Registros.Count} Registros";
        }
    }

    /// <summary>
    /// Imprime el/los registros (placeholder)
    /// </summary>
    [RelayCommand]
    private void Print()
    {
        _dialogService.ShowInfo("Función de impresión en desarrollo", "Información");
    }

    /// <summary>
    /// Exporta el/los registros a PDF (placeholder)
    /// </summary>
    [RelayCommand]
    private void ExportToPdf()
    {
        _dialogService.ShowInfo("Función de exportación a PDF en desarrollo", "Información");
    }

    /// <summary>
    /// Cierra la ventana
    /// </summary>
    [RelayCommand]
    private void Close(Window window)
    {
        window?.Close();
    }
}
