using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;
using System.IO;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la configuración de empresa
/// </summary>
public partial class ConfiguracionEmpresaViewModel : ObservableObject
{
    private readonly IConfiguracionService _configuracionService;
    
    [ObservableProperty]
    private string _nombre = string.Empty;
    
    [ObservableProperty]
    private string _nit = string.Empty;
    
    [ObservableProperty]
    private string _direccion = string.Empty;
    
    [ObservableProperty]
    private string _ciudad = string.Empty;
    
    [ObservableProperty]
    private string _telefono = string.Empty;
    
    [ObservableProperty]
    private string _correo = string.Empty;
    
    [ObservableProperty]
    private string _representanteNombre = string.Empty;
    
    [ObservableProperty]
    private string _representanteCargo = string.Empty;
    
    [ObservableProperty]
    private string? _logoPath;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _mensaje;
    
    [ObservableProperty]
    private bool _hayLogo;
    
    public ConfiguracionEmpresaViewModel(IConfiguracionService configuracionService)
    {
        _configuracionService = configuracionService;
    }
    
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        Mensaje = null;
        
        try
        {
            var companyInfo = await _configuracionService.GetCompanyInfoAsync();
            
            Nombre = companyInfo.Nombre;
            Nit = companyInfo.Nit;
            Direccion = companyInfo.Direccion;
            Ciudad = companyInfo.Ciudad;
            Telefono = companyInfo.Telefono;
            Correo = companyInfo.Correo;
            RepresentanteNombre = companyInfo.RepresentanteNombre;
            RepresentanteCargo = companyInfo.RepresentanteCargo;
            LogoPath = companyInfo.LogoPath;
            
            HayLogo = !string.IsNullOrEmpty(LogoPath) && File.Exists(LogoPath);
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al cargar configuración: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task GuardarAsync()
    {
        IsLoading = true;
        Mensaje = null;
        
        try
        {
            var companyInfo = new CompanyInfo
            {
                Nombre = Nombre,
                Nit = Nit,
                Direccion = Direccion,
                Ciudad = Ciudad,
                Telefono = Telefono,
                Correo = Correo,
                RepresentanteNombre = RepresentanteNombre,
                RepresentanteCargo = RepresentanteCargo,
                LogoPath = LogoPath
            };
            
            var result = await _configuracionService.SaveCompanyInfoAsync(companyInfo);
            
            if (result.Success)
            {
                MessageBox.Show("Configuración guardada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Mensaje = result.Message;
            }
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al guardar: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task SeleccionarLogoAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Seleccionar Logo",
            Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp|Todos los archivos|*.*",
            CheckFileExists = true
        };
        
        if (dialog.ShowDialog() == true)
        {
            IsLoading = true;
            
            try
            {
                var result = await _configuracionService.CopiarLogoAsync(dialog.FileName);
                
                if (result.Success)
                {
                    LogoPath = result.Data;
                    HayLogo = true;
                    MessageBox.Show("Logo actualizado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al copiar logo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    
    [RelayCommand]
    private void EliminarLogo()
    {
        var result = MessageBox.Show("¿Está seguro de eliminar el logo?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            LogoPath = null;
            HayLogo = false;
        }
    }
}
