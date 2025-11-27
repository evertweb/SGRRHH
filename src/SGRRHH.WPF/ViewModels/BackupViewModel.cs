using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la gestión de backups
/// </summary>
public partial class BackupViewModel : ObservableObject
{
    private readonly IBackupService _backupService;
    
    [ObservableProperty]
    private ObservableCollection<BackupInfo> _backups = new();
    
    [ObservableProperty]
    private BackupInfo? _selectedBackup;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _mensaje;
    
    [ObservableProperty]
    private string _rutaBackups = string.Empty;
    
    public BackupViewModel(IBackupService backupService)
    {
        _backupService = backupService;
        RutaBackups = _backupService.GetRutaBackups();
    }
    
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        Mensaje = null;
        
        try
        {
            var backups = await _backupService.ListarBackupsAsync();
            
            Backups.Clear();
            foreach (var backup in backups)
            {
                Backups.Add(backup);
            }
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al cargar backups: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task CrearBackupAsync()
    {
        IsLoading = true;
        Mensaje = null;
        
        try
        {
            var result = await _backupService.CrearBackupAsync();
            
            if (result.Success)
            {
                MessageBox.Show($"Backup creado exitosamente:\n{result.Data}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al crear backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task CrearBackupEnAsync()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Guardar Backup",
            Filter = "Base de datos SQLite|*.db|Todos los archivos|*.*",
            FileName = $"sgrrhh_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
            DefaultExt = ".db"
        };
        
        if (dialog.ShowDialog() == true)
        {
            IsLoading = true;
            
            try
            {
                var result = await _backupService.CrearBackupAsync(dialog.FileName);
                
                if (result.Success)
                {
                    MessageBox.Show("Backup creado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    
    [RelayCommand]
    private async Task RestaurarBackupAsync()
    {
        if (SelectedBackup == null)
        {
            MessageBox.Show("Seleccione un backup para restaurar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirmacion = MessageBox.Show(
            $"¿Está seguro de restaurar la base de datos desde el backup:\n\n{SelectedBackup.NombreArchivo}\n\nFecha: {SelectedBackup.FechaCreacion:dd/MM/yyyy HH:mm}\n\nSe creará un backup de seguridad antes de restaurar.",
            "Confirmar Restauración",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        
        if (confirmacion != MessageBoxResult.Yes)
            return;
        
        IsLoading = true;
        
        try
        {
            var result = await _backupService.RestaurarBackupAsync(SelectedBackup.RutaCompleta);
            
            if (result.Success)
            {
                MessageBox.Show(
                    "Base de datos restaurada exitosamente.\n\nSe recomienda reiniciar la aplicación para aplicar los cambios.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                    
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al restaurar backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task RestaurarDesdeArchivoAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Seleccionar Backup",
            Filter = "Base de datos SQLite|*.db|Todos los archivos|*.*",
            CheckFileExists = true
        };
        
        if (dialog.ShowDialog() == true)
        {
            // Validar primero
            var validacion = await _backupService.ValidarBackupAsync(dialog.FileName);
            
            if (!validacion.Success)
            {
                MessageBox.Show($"El archivo seleccionado no es válido:\n\n{validacion.Message}", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            var confirmacion = MessageBox.Show(
                $"¿Está seguro de restaurar la base de datos desde:\n\n{Path.GetFileName(dialog.FileName)}\n\nSe creará un backup de seguridad antes de restaurar.",
                "Confirmar Restauración",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (confirmacion != MessageBoxResult.Yes)
                return;
            
            IsLoading = true;
            
            try
            {
                var result = await _backupService.RestaurarBackupAsync(dialog.FileName);
                
                if (result.Success)
                {
                    MessageBox.Show(
                        "Base de datos restaurada exitosamente.\n\nSe recomienda reiniciar la aplicación para aplicar los cambios.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                        
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restaurar backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    
    [RelayCommand]
    private async Task EliminarBackupAsync()
    {
        if (SelectedBackup == null)
        {
            MessageBox.Show("Seleccione un backup para eliminar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirmacion = MessageBox.Show(
            $"¿Está seguro de eliminar el backup:\n\n{SelectedBackup.NombreArchivo}?",
            "Confirmar Eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        
        if (confirmacion != MessageBoxResult.Yes)
            return;
        
        IsLoading = true;
        
        try
        {
            var result = await _backupService.EliminarBackupAsync(SelectedBackup.RutaCompleta);
            
            if (result.Success)
            {
                MessageBox.Show("Backup eliminado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void AbrirCarpetaBackups()
    {
        try
        {
            if (Directory.Exists(RutaBackups))
            {
                Process.Start("explorer.exe", RutaBackups);
            }
            else
            {
                MessageBox.Show("La carpeta de backups no existe", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al abrir carpeta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
