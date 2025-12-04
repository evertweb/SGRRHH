using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;

namespace SGRRHH.WPF.ViewModels;

public class DocumentTemplateDefinition
{
    public required string Nombre { get; init; }
    public required string Descripcion { get; init; }
    public required TipoDocumentoPdf Tipo { get; init; }
    public bool RequiresEmpleado { get; init; }
    public bool RequiresPermiso { get; init; }
}

public partial class DocumentsViewModel : ViewModelBase
{
    private readonly IEmpleadoService _empleadoService;
    private readonly IPermisoService _permisoService;
    private readonly IDocumentService _documentService;

    [ObservableProperty]
    private ObservableCollection<DocumentTemplateDefinition> _templates = new();

    [ObservableProperty]
    private DocumentTemplateDefinition? _selectedTemplate;

    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();

    [ObservableProperty]
    private ObservableCollection<Permiso> _permisos = new();

    [ObservableProperty]
    private Empleado? _selectedEmpleado;

    [ObservableProperty]
    private Permiso? _selectedPermiso;

    [ObservableProperty]
    private CertificadoLaboralOptions _certificadoOptions = new();

    [ObservableProperty]
    private ConstanciaTrabajoOptions _constanciaOptions = new();

    [ObservableProperty]
    private CompanyInfo _companyInfo = new();

    [ObservableProperty]
    private string? _previewFilePath;

    public DocumentsViewModel(
        IEmpleadoService empleadoService,
        IPermisoService permisoService,
        IDocumentService documentService)
    {
        _empleadoService = empleadoService;
        _permisoService = permisoService;
        _documentService = documentService;

        _companyInfo = _documentService.GetCompanyInfo();
        _certificadoOptions = CreateCertificadoDefaults();
        _constanciaOptions = CreateConstanciaDefaults();

        InitializeTemplates();
    }

    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Cargando catálogos...";

            var empleados = await _empleadoService.GetAllAsync();
            Empleados = new ObservableCollection<Empleado>(empleados.OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres));

            var permisosResult = await _permisoService.GetAllAsync();
            if (permisosResult.Success && permisosResult.Data != null)
            {
                Permisos = new ObservableCollection<Permiso>(permisosResult.Data);
            }
            else
            {
                StatusMessage = permisosResult.Errors.Any()
                    ? string.Join("\n", permisosResult.Errors)
                    : permisosResult.Message ?? "No fue posible cargar los permisos";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar datos: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GenerateDocumentAsync()
    {
        if (SelectedTemplate == null)
        {
            StatusMessage = "Seleccione una plantilla";
            return;
        }

        if (SelectedTemplate.RequiresPermiso && SelectedPermiso == null)
        {
            StatusMessage = "Seleccione un permiso";
            return;
        }

        if (SelectedTemplate.RequiresEmpleado && SelectedEmpleado == null)
        {
            StatusMessage = "Seleccione un empleado";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Generando documento...";
            PreviewFilePath = null;

            ServiceResult<byte[]> result;
            string suggestedName;

            switch (SelectedTemplate.Tipo)
            {
                case TipoDocumentoPdf.ActaPermiso:
                    result = await _documentService.GenerateActaPermisoPdfAsync(SelectedPermiso!.Id);
                    suggestedName = $"ACTA_{SelectedPermiso.NumeroActa}";
                    break;
                case TipoDocumentoPdf.CertificadoLaboral:
                    result = await _documentService.GenerateCertificadoLaboralPdfAsync(SelectedEmpleado!.Id, CertificadoOptions);
                    suggestedName = $"CERTIFICADO_{SelectedEmpleado.Apellidos}_{DateTime.Now:yyyyMMdd}";
                    break;
                case TipoDocumentoPdf.ConstanciaTrabajo:
                    result = await _documentService.GenerateConstanciaTrabajoPdfAsync(SelectedEmpleado!.Id, ConstanciaOptions);
                    suggestedName = $"CONSTANCIA_{SelectedEmpleado.Apellidos}_{DateTime.Now:yyyyMMdd}";
                    break;
                default:
                    StatusMessage = "Plantilla no soportada";
                    return;
            }

            if (!result.Success || result.Data == null)
            {
                StatusMessage = result.Message ?? "No fue posible generar el PDF";
                return;
            }

            var saveResult = await _documentService.SaveDocumentAsync(result.Data, suggestedName);
            if (!saveResult.Success || string.IsNullOrWhiteSpace(saveResult.Data))
            {
                StatusMessage = saveResult.Message ?? "No se pudo guardar el PDF";
                return;
            }

            PreviewFilePath = saveResult.Data;
            StatusMessage = $"Documento generado en: {saveResult.Data}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error generando documento: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenPreviewExternally()
    {
        if (string.IsNullOrWhiteSpace(PreviewFilePath) || !File.Exists(PreviewFilePath))
        {
            StatusMessage = "No hay un documento para abrir";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = PreviewFilePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo abrir el archivo: {ex.Message}";
        }
    }

    [RelayCommand]
    private void PrintDocument()
    {
        if (string.IsNullOrWhiteSpace(PreviewFilePath) || !File.Exists(PreviewFilePath))
        {
            StatusMessage = "Genere un documento antes de imprimir";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = PreviewFilePath,
                Verb = "print",
                UseShellExecute = true
            });
            StatusMessage = "Se envió el documento a impresión";
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo imprimir: {ex.Message}";
        }
    }

    [RelayCommand]
    private void DownloadDocument()
    {
        if (string.IsNullOrWhiteSpace(PreviewFilePath) || !File.Exists(PreviewFilePath))
        {
            StatusMessage = "No hay documento para descargar";
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "PDF (*.pdf)|*.pdf",
            FileName = Path.GetFileName(PreviewFilePath)
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                File.Copy(PreviewFilePath, dialog.FileName, overwrite: true);
                StatusMessage = $"Documento guardado en {dialog.FileName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al copiar archivo: {ex.Message}";
            }
        }
    }

    partial void OnSelectedTemplateChanged(DocumentTemplateDefinition? value)
    {
        if (value == null)
        {
            return;
        }

        if (!value.RequiresEmpleado)
        {
            SelectedEmpleado = null;
        }

        if (!value.RequiresPermiso)
        {
            SelectedPermiso = null;
        }
    }

    partial void OnSelectedPermisoChanged(Permiso? value)
    {
        if (value?.Empleado != null)
        {
            SelectedEmpleado = value.Empleado;
        }
    }

    private void InitializeTemplates()
    {
        Templates = new ObservableCollection<DocumentTemplateDefinition>
        {
            new()
            {
                Nombre = "Acta de Permiso",
                Descripcion = "Genera el acta formal para una solicitud de permiso",
                Tipo = TipoDocumentoPdf.ActaPermiso,
                RequiresPermiso = true,
                RequiresEmpleado = false
            },
            new()
            {
                Nombre = "Certificado Laboral",
                Descripcion = "Certifica vínculo, cargo y antigüedad",
                Tipo = TipoDocumentoPdf.CertificadoLaboral,
                RequiresEmpleado = true,
                RequiresPermiso = false
            },
            new()
            {
                Nombre = "Constancia de Trabajo",
                Descripcion = "Constancia simple para trámites generales",
                Tipo = TipoDocumentoPdf.ConstanciaTrabajo,
                RequiresEmpleado = true,
                RequiresPermiso = false
            }
        };

        SelectedTemplate = Templates.FirstOrDefault();
    }

    private CertificadoLaboralOptions CreateCertificadoDefaults()
    {
        return new CertificadoLaboralOptions
        {
            CiudadExpedicion = CompanyInfo.Ciudad,
            ResponsableNombre = CompanyInfo.RepresentanteNombre,
            ResponsableCargo = CompanyInfo.RepresentanteCargo,
            ParrafoAdicional = "El trabajador cumple a satisfacción con sus responsabilidades."
        };
    }

    private ConstanciaTrabajoOptions CreateConstanciaDefaults()
    {
        return new ConstanciaTrabajoOptions
        {
            CiudadExpedicion = CompanyInfo.Ciudad,
            ResponsableNombre = CompanyInfo.RepresentanteNombre,
            ResponsableCargo = CompanyInfo.RepresentanteCargo,
            Motivo = "Trámite personal"
        };
    }
}
