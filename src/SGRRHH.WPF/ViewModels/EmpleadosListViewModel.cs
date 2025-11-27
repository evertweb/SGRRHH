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
/// ViewModel para la lista de empleados
/// </summary>
public partial class EmpleadosListViewModel : ObservableObject
{
    private readonly IEmpleadoService _empleadoService;
    private readonly IDepartamentoService _departamentoService;
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _empleadosPendientes = new();
    
    [ObservableProperty]
    private ObservableCollection<Departamento> _departamentos = new();
    
    [ObservableProperty]
    private Empleado? _selectedEmpleado;
    
    [ObservableProperty]
    private Empleado? _selectedPendiente;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private Departamento? _selectedDepartamento;
    
    [ObservableProperty]
    private EstadoEmpleado? _selectedEstado;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private int _totalEmpleados;
    
    [ObservableProperty]
    private int _totalPendientes;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private bool _canApprove;
    
    [ObservableProperty]
    private bool _showPendientes;
    
    /// <summary>
    /// Lista de estados para el filtro
    /// </summary>
    public ObservableCollection<EstadoEmpleadoItem> Estados { get; } = new()
    {
        new EstadoEmpleadoItem { Nombre = "Todos", Valor = null },
        new EstadoEmpleadoItem { Nombre = "Pendiente Aprobación", Valor = EstadoEmpleado.PendienteAprobacion },
        new EstadoEmpleadoItem { Nombre = "Activo", Valor = EstadoEmpleado.Activo },
        new EstadoEmpleadoItem { Nombre = "Vacaciones", Valor = EstadoEmpleado.EnVacaciones },
        new EstadoEmpleadoItem { Nombre = "Licencia", Valor = EstadoEmpleado.EnLicencia },
        new EstadoEmpleadoItem { Nombre = "Suspendido", Valor = EstadoEmpleado.Suspendido },
        new EstadoEmpleadoItem { Nombre = "Retirado", Valor = EstadoEmpleado.Retirado },
        new EstadoEmpleadoItem { Nombre = "Rechazado", Valor = EstadoEmpleado.Rechazado }
    };
    
    /// <summary>
    /// Evento para solicitar apertura de formulario de nuevo empleado
    /// </summary>
    public event EventHandler? CreateEmpleadoRequested;
    
    /// <summary>
    /// Evento para solicitar apertura de formulario de edición
    /// </summary>
    public event EventHandler<Empleado>? EditEmpleadoRequested;
    
    /// <summary>
    /// Evento para solicitar ver detalle de empleado
    /// </summary>
    public event EventHandler<Empleado>? ViewEmpleadoRequested;
    
    public EmpleadosListViewModel(IEmpleadoService empleadoService, IDepartamentoService departamentoService)
    {
        _empleadoService = empleadoService;
        _departamentoService = departamentoService;
        
        // Verificar si el usuario puede aprobar (Admin o Aprobador)
        var currentUser = App.CurrentUser;
        CanApprove = currentUser != null && 
            (currentUser.Rol == RolUsuario.Administrador || currentUser.Rol == RolUsuario.Aprobador);
        ShowPendientes = CanApprove;
    }
    
    /// <summary>
    /// Carga inicial de datos
    /// </summary>
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Cargando datos...";
        
        try
        {
            // Cargar departamentos para el filtro
            var departamentos = await _departamentoService.GetAllAsync();
            Departamentos.Clear();
            Departamentos.Add(new Departamento { Id = 0, Nombre = "Todos los departamentos" });
            foreach (var dep in departamentos)
            {
                Departamentos.Add(dep);
            }
            
            // Cargar empleados pendientes si puede aprobar
            if (CanApprove)
            {
                await LoadPendientesAsync();
            }
            
            // Cargar empleados
            await SearchEmpleadosAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar datos: {ex.Message}";
            MessageBox.Show($"Error al cargar los datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Carga empleados pendientes de aprobación
    /// </summary>
    private async Task LoadPendientesAsync()
    {
        var pendientes = await _empleadoService.GetPendientesAprobacionAsync();
        EmpleadosPendientes.Clear();
        foreach (var emp in pendientes)
        {
            EmpleadosPendientes.Add(emp);
        }
        TotalPendientes = EmpleadosPendientes.Count;
    }
    
    /// <summary>
    /// Busca empleados según los filtros actuales
    /// </summary>
    [RelayCommand]
    private async Task SearchEmpleadosAsync()
    {
        IsLoading = true;
        StatusMessage = "Buscando...";
        
        try
        {
            IEnumerable<Empleado> resultado;
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                resultado = await _empleadoService.SearchAsync(SearchText);
            }
            else if (SelectedDepartamento != null && SelectedDepartamento.Id > 0)
            {
                resultado = await _empleadoService.GetByDepartamentoAsync(SelectedDepartamento.Id);
            }
            else if (SelectedEstado.HasValue)
            {
                resultado = await _empleadoService.GetByEstadoAsync(SelectedEstado.Value);
            }
            else
            {
                resultado = await _empleadoService.GetAllAsync();
            }
            
            // Aplicar filtros adicionales si es necesario
            if (SelectedDepartamento != null && SelectedDepartamento.Id > 0 && !string.IsNullOrWhiteSpace(SearchText))
            {
                resultado = resultado.Where(e => e.DepartamentoId == SelectedDepartamento.Id);
            }
            
            if (SelectedEstado.HasValue && (SelectedDepartamento?.Id > 0 || !string.IsNullOrWhiteSpace(SearchText)))
            {
                resultado = resultado.Where(e => e.Estado == SelectedEstado.Value);
            }
            
            Empleados.Clear();
            foreach (var empleado in resultado)
            {
                Empleados.Add(empleado);
            }
            
            TotalEmpleados = Empleados.Count;
            StatusMessage = $"{TotalEmpleados} empleado(s) encontrado(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error en la búsqueda: {ex.Message}";
            MessageBox.Show($"Error en la búsqueda: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Limpia los filtros y recarga todos los empleados
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedDepartamento = Departamentos.FirstOrDefault();
        SelectedEstado = null;
        await SearchEmpleadosAsync();
    }
    
    /// <summary>
    /// Abre el formulario para crear un nuevo empleado
    /// </summary>
    [RelayCommand]
    private void CreateEmpleado()
    {
        CreateEmpleadoRequested?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Abre el formulario para editar el empleado seleccionado
    /// </summary>
    [RelayCommand]
    private void EditEmpleado()
    {
        if (SelectedEmpleado == null)
        {
            MessageBox.Show("Seleccione un empleado para editar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        EditEmpleadoRequested?.Invoke(this, SelectedEmpleado);
    }
    
    /// <summary>
    /// Muestra el detalle del empleado seleccionado
    /// </summary>
    [RelayCommand]
    private void ViewEmpleado()
    {
        if (SelectedEmpleado == null)
        {
            MessageBox.Show("Seleccione un empleado para ver su detalle", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        ViewEmpleadoRequested?.Invoke(this, SelectedEmpleado);
    }
    
    /// <summary>
    /// Desactiva el empleado seleccionado
    /// </summary>
    [RelayCommand]
    private async Task DeactivateEmpleadoAsync()
    {
        if (SelectedEmpleado == null)
        {
            MessageBox.Show("Seleccione un empleado para desactivar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"¿Está seguro de desactivar al empleado {SelectedEmpleado.NombreCompleto}?",
            "Confirmar desactivación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var serviceResult = await _empleadoService.DeactivateAsync(SelectedEmpleado.Id);
                
                if (serviceResult.Success)
                {
                    await SearchEmpleadosAsync();
                    MessageBox.Show(serviceResult.Message, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(serviceResult.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desactivar el empleado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    /// <summary>
    /// Elimina permanentemente el empleado seleccionado
    /// </summary>
    [RelayCommand]
    private async Task DeleteEmpleadoAsync()
    {
        if (SelectedEmpleado == null)
        {
            MessageBox.Show("Seleccione un empleado para eliminar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"⚠️ ADVERTENCIA: Esta acción eliminará PERMANENTEMENTE al empleado {SelectedEmpleado.NombreCompleto} y todos sus datos asociados.\n\n¿Está completamente seguro?",
            "Confirmar eliminación permanente",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
            
        if (result == MessageBoxResult.Yes)
        {
            // Segunda confirmación
            var confirmResult = MessageBox.Show(
                "Esta acción NO se puede deshacer.\n\n¿Desea continuar con la eliminación permanente?",
                "Última confirmación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Stop);
                
            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    var serviceResult = await _empleadoService.DeletePermanentlyAsync(SelectedEmpleado.Id);
                    
                    if (serviceResult.Success)
                    {
                        await SearchEmpleadosAsync();
                        MessageBox.Show(serviceResult.Message, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(serviceResult.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el empleado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
    
    /// <summary>
    /// Aprueba el empleado pendiente seleccionado
    /// </summary>
    [RelayCommand]
    private async Task AprobarEmpleadoAsync()
    {
        if (SelectedPendiente == null)
        {
            MessageBox.Show("Seleccione un empleado pendiente para aprobar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"¿Está seguro de aprobar al empleado {SelectedPendiente.NombreCompleto}?\n\nEl empleado quedará activo en el sistema.",
            "Confirmar aprobación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var currentUser = App.CurrentUser;
                if (currentUser == null)
                {
                    MessageBox.Show("Error: Usuario no identificado", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                var serviceResult = await _empleadoService.AprobarAsync(SelectedPendiente.Id, currentUser.Id);
                
                if (serviceResult.Success)
                {
                    await LoadPendientesAsync();
                    await SearchEmpleadosAsync();
                    MessageBox.Show($"✅ {SelectedPendiente.NombreCompleto} ha sido aprobado exitosamente.", "Aprobado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(serviceResult.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aprobar el empleado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    /// <summary>
    /// Rechaza el empleado pendiente seleccionado
    /// </summary>
    [RelayCommand]
    private async Task RechazarEmpleadoAsync()
    {
        if (SelectedPendiente == null)
        {
            MessageBox.Show("Seleccione un empleado pendiente para rechazar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        // Crear diálogo simple para pedir motivo
        var dialog = new Window
        {
            Title = "Motivo de Rechazo",
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };
        
        var panel = new System.Windows.Controls.StackPanel { Margin = new Thickness(15) };
        panel.Children.Add(new System.Windows.Controls.TextBlock 
        { 
            Text = $"Ingrese el motivo del rechazo para:\n{SelectedPendiente.NombreCompleto}",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        });
        
        var textBox = new System.Windows.Controls.TextBox 
        { 
            Height = 60,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto
        };
        panel.Children.Add(textBox);
        
        var buttonPanel = new System.Windows.Controls.StackPanel 
        { 
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 15, 0, 0)
        };
        
        var okButton = new System.Windows.Controls.Button 
        { 
            Content = "Rechazar", 
            Width = 80, 
            Margin = new Thickness(0, 0, 10, 0),
            IsDefault = true
        };
        okButton.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
        
        var cancelButton = new System.Windows.Controls.Button 
        { 
            Content = "Cancelar", 
            Width = 80,
            IsCancel = true
        };
        cancelButton.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };
        
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        panel.Children.Add(buttonPanel);
        
        dialog.Content = panel;
        
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(textBox.Text))
        {
            return;
        }
        
        var motivo = textBox.Text.Trim();
        
        var result = MessageBox.Show(
            $"¿Está seguro de rechazar al empleado {SelectedPendiente.NombreCompleto}?\n\nMotivo: {motivo}",
            "Confirmar rechazo",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
            
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var currentUser = App.CurrentUser;
                if (currentUser == null)
                {
                    MessageBox.Show("Error: Usuario no identificado", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                var serviceResult = await _empleadoService.RechazarAsync(SelectedPendiente.Id, currentUser.Id, motivo);
                
                if (serviceResult.Success)
                {
                    await LoadPendientesAsync();
                    await SearchEmpleadosAsync();
                    MessageBox.Show($"❌ {SelectedPendiente.NombreCompleto} ha sido rechazado.", "Rechazado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(serviceResult.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al rechazar el empleado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    /// <summary>
    /// Se ejecuta cuando cambia el texto de búsqueda
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        // Búsqueda automática con debounce se implementaría aquí
        // Por simplicidad, el usuario debe presionar el botón de buscar
    }
    
    /// <summary>
    /// Se ejecuta cuando cambia el departamento seleccionado
    /// </summary>
    partial void OnSelectedDepartamentoChanged(Departamento? value)
    {
        if (value != null)
        {
            _ = SearchEmpleadosAsync();
        }
    }
}

/// <summary>
/// Item para el combo de estados
/// </summary>
public class EstadoEmpleadoItem
{
    public string Nombre { get; set; } = string.Empty;
    public EstadoEmpleado? Valor { get; set; }
}
