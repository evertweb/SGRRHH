using System.Globalization;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="IDocumentService"/> usando QuestPDF.
/// </summary>
public class DocumentService : IDocumentService
{
    private const string ConfigFolderName = "config";
    private const string DocumentsFolderName = "documentos";
    private const string CompanyFileName = "company.json";
    private static readonly CultureInfo Culture = new("es-CO");

    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly string _documentsPath;
    private readonly CompanyInfo _companyInfo;
    private readonly byte[]? _logoBytes;

    static DocumentService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public DocumentService(IEmpleadoRepository empleadoRepository, IPermisoRepository permisoRepository)
    {
        _empleadoRepository = empleadoRepository;
        _permisoRepository = permisoRepository;

        var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        Directory.CreateDirectory(dataDirectory);

        var configDirectory = Path.Combine(dataDirectory, ConfigFolderName);
        Directory.CreateDirectory(configDirectory);

        _documentsPath = Path.Combine(dataDirectory, DocumentsFolderName);
        Directory.CreateDirectory(_documentsPath);

        _companyInfo = LoadCompanyInfo(Path.Combine(configDirectory, CompanyFileName));
        _logoBytes = TryLoadLogo(_companyInfo.LogoPath);
    }

    public CompanyInfo GetCompanyInfo() => _companyInfo;

    public async Task<ServiceResult<byte[]>> GenerateActaPermisoPdfAsync(int permisoId)
    {
        var permiso = await _permisoRepository.GetByIdAsync(permisoId);
        if (permiso == null)
        {
            return ServiceResult<byte[]>.Fail("El permiso seleccionado no existe");
        }

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(TextStyle.Default.FontSize(11));

                page.Header().Element(header => BuildHeader(header));

                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text("ACTA DE PERMISO LABORAL")
                        .FontSize(16)
                        .Bold()
                        .AlignCenter();

                    column.Item().Text(text =>
                    {
                        text.Span($"Acta No.: {permiso.NumeroActa}").SemiBold();
                        text.Line($"Fecha de emisión: {FormatLongDate(DateTime.Now)}");
                    });

                    column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(info =>
                    {
                        info.Item().Text($"Empleado: {permiso.Empleado?.NombreCompleto ?? "N/A"}");
                        info.Item().Text($"Documento: {permiso.Empleado?.Cedula ?? "N/A"}");
                        info.Item().Text($"Cargo: {permiso.Empleado?.Cargo?.Nombre ?? "N/A"}");
                        info.Item().Text($"Departamento: {permiso.Empleado?.Departamento?.Nombre ?? "N/A"}");
                    });

                    column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(info =>
                    {
                        info.Item().Text($"Tipo de permiso: {permiso.TipoPermiso?.Nombre ?? "N/A"}");
                        info.Item().Text($"Estado: {permiso.Estado}");
                        info.Item().Text($"Periodo: {permiso.FechaInicio:dd/MM/yyyy} al {permiso.FechaFin:dd/MM/yyyy}");
                        info.Item().Text($"Total de días: {permiso.TotalDias}");
                    });

                    column.Item().Text("Motivo del permiso").Bold();
                    column.Item().PaddingLeft(10).Text(permiso.Motivo).FontSize(11);

                    if (!string.IsNullOrWhiteSpace(permiso.Observaciones))
                    {
                        column.Item().Text("Observaciones").Bold();
                        column.Item().PaddingLeft(10).Text(permiso.Observaciones).FontSize(11);
                    }

                    if (permiso.DiasPendientesCompensacion.HasValue)
                    {
                        column.Item().Text("Compensación").Bold();
                        column.Item().PaddingLeft(10).Text(text =>
                        {
                            text.Span($"Días pendientes: {permiso.DiasPendientesCompensacion.Value}");
                            if (permiso.FechaCompensacion.HasValue)
                            {
                                text.Line($" - Fecha programada: {permiso.FechaCompensacion:dd/MM/yyyy}");
                            }
                        });
                    }

                    column.Item().PaddingTop(20).Row(row =>
                    {
                        row.ConstantItem(250).Column(col => BuildSignature(col, _companyInfo.RepresentanteNombre, _companyInfo.RepresentanteCargo));
                        row.RelativeItem().Column(col =>
                        {
                            if (permiso.AprobadoPor != null)
                            {
                                BuildSignature(col, permiso.AprobadoPor.NombreCompleto, permiso.AprobadoPor.Rol.ToString());
                            }
                            else
                            {
                                BuildSignature(col, "Responsable", "Firma y sello");
                            }
                        });
                    });
                });

                page.Footer().AlignCenter().Text($"Generado automáticamente por SGRRHH el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
            });
        });

        var bytes = documento.GeneratePdf();
        return ServiceResult<byte[]>.Ok(bytes, "Acta generada correctamente");
    }

    public async Task<ServiceResult<byte[]>> GenerateCertificadoLaboralPdfAsync(int empleadoId, CertificadoLaboralOptions? options)
    {
        options ??= new CertificadoLaboralOptions();
        var empleado = await _empleadoRepository.GetByIdWithRelationsAsync(empleadoId);
        if (empleado == null)
        {
            return ServiceResult<byte[]>.Fail("El empleado no existe");
        }

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(TextStyle.Default.FontSize(12));
                page.Header().Element(header => BuildHeader(header));
                page.Content().Column(column =>
                {
                    column.Spacing(20);
                    column.Item().Text("CERTIFICADO LABORAL").FontSize(18).Bold().AlignCenter();

                    column.Item().Text($"{options.CiudadExpedicion}, {FormatLongDate(options.FechaEmision)}").AlignRight();

                    column.Item().Text(text =>
                    {
                        text.Line($"Quien suscribe, {_companyInfo.RepresentanteNombre}, {_companyInfo.RepresentanteCargo} de {_companyInfo.Nombre}, certifica que:");
                    });

                    column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(info =>
                    {
                        info.Item().Text($"{empleado.NombreCompleto} identificado con cédula {empleado.Cedula}.");
                        info.Item().Text($"Ingreso el {empleado.FechaIngreso:dd/MM/yyyy} desempeñándose actualmente como {empleado.Cargo?.Nombre ?? "[Cargo]"} en el departamento {empleado.Departamento?.Nombre ?? "[Departamento]"}.");
                        info.Item().Text($"Tipo de contrato: {empleado.TipoContrato}.");
                    });

                    column.Item().Text("El colaborador ha demostrado responsabilidad, puntualidad y compromiso en las funciones asignadas.");

                    if (!string.IsNullOrWhiteSpace(options.ParrafoAdicional))
                    {
                        column.Item().Text(options.ParrafoAdicional);
                    }

                    column.Item().Text("Se expide el presente documento a solicitud del interesado.");

                    column.Item().PaddingTop(40).Column(col => BuildSignature(col, options.ResponsableNombre, options.ResponsableCargo));
                });
                page.Footer().AlignCenter().Text($"{_companyInfo.Nombre} - {_companyInfo.Direccion} - {_companyInfo.Telefono}").FontSize(9);
            });
        });

        var bytes = documento.GeneratePdf();
        return ServiceResult<byte[]>.Ok(bytes, "Certificado generado correctamente");
    }

    public async Task<ServiceResult<byte[]>> GenerateConstanciaTrabajoPdfAsync(int empleadoId, ConstanciaTrabajoOptions? options)
    {
        options ??= new ConstanciaTrabajoOptions();
        var empleado = await _empleadoRepository.GetByIdWithRelationsAsync(empleadoId);
        if (empleado == null)
        {
            return ServiceResult<byte[]>.Fail("El empleado no existe");
        }

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(TextStyle.Default.FontSize(12));
                page.Header().Element(header => BuildHeader(header));
                page.Content().Column(column =>
                {
                    column.Spacing(20);
                    column.Item().Text("CONSTANCIA DE TRABAJO").FontSize(18).Bold().AlignCenter();

                    column.Item().Text($"{options.CiudadExpedicion}, {FormatLongDate(options.FechaEmision)}").AlignRight();

                    column.Item().Text(text =>
                    {
                        text.Line($"Se deja constancia de que {empleado.NombreCompleto}, identificado(a) con cédula {empleado.Cedula}, labora en {_companyInfo.Nombre} desde el {empleado.FechaIngreso:dd/MM/yyyy}.");
                        if (empleado.Cargo != null)
                        {
                            text.Line($"Actualmente ocupa el cargo de {empleado.Cargo.Nombre} en el departamento {empleado.Departamento?.Nombre ?? "[Departamento]"}.");
                        }
                        text.Line($"Motivo de la constancia: {options.Motivo}.");
                    });

                    if (!string.IsNullOrWhiteSpace(options.NotasAdicionales))
                    {
                        column.Item().Text(options.NotasAdicionales);
                    }

                    column.Item().Text("Se firma en constancia.");

                    column.Item().PaddingTop(40).Column(col => BuildSignature(col, options.ResponsableNombre, options.ResponsableCargo));
                });
                page.Footer().AlignCenter().Text($"{_companyInfo.Correo} | {_companyInfo.Telefono}").FontSize(9);
            });
        });

        var bytes = documento.GeneratePdf();
        return ServiceResult<byte[]>.Ok(bytes, "Constancia generada correctamente");
    }

    public Task<ServiceResult<string>> SaveDocumentAsync(byte[] pdfBytes, string suggestedFileName)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            return Task.FromResult(ServiceResult<string>.Fail("El archivo generado está vacío"));
        }

        var sanitizedName = string.Join("_", suggestedFileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var fileName = sanitizedName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? sanitizedName
            : $"{sanitizedName}.pdf";
        var filePath = Path.Combine(_documentsPath, fileName);

        File.WriteAllBytes(filePath, pdfBytes);
        return Task.FromResult(ServiceResult<string>.Ok(filePath, "Documento guardado"));
    }

    private void BuildHeader(IContainer container)
    {
        container.Row(row =>
        {
            if (_logoBytes != null)
            {
                row.ConstantItem(80).Height(80).AlignLeft().AlignMiddle().Image(_logoBytes);
            }
            else
            {
                row.ConstantItem(80).Height(80).Background(Colors.Grey.Lighten4).AlignCenter().AlignMiddle()
                    .Text(_companyInfo.Nombre.Substring(0, Math.Min(_companyInfo.Nombre.Length, 3)).ToUpper())
                    .FontSize(18).Bold();
            }

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().AlignRight().Text(_companyInfo.Nombre).FontSize(16).Bold();
                col.Item().AlignRight().Text($"NIT: {_companyInfo.Nit}");
                col.Item().AlignRight().Text(_companyInfo.Direccion);
                col.Item().AlignRight().Text($"{_companyInfo.Ciudad} - {_companyInfo.Telefono}");
                col.Item().AlignRight().Text(_companyInfo.Correo);
            });
        });
    }

    private static void BuildSignature(ColumnDescriptor column, string nombre, string cargo)
    {
        column.Spacing(3);
        column.Item().AlignLeft().Text("________________________");
        column.Item().Text(nombre).Bold();
        column.Item().Text(cargo);
    }

    private static string FormatLongDate(DateTime date)
        => date.ToString("dd 'de' MMMM 'de' yyyy", Culture);

    private static byte[]? TryLoadLogo(string? path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
        }
        catch
        {
            // Ignorar errores de lectura de logo.
        }

        return null;
    }

    private static CompanyInfo LoadCompanyInfo(string configPath)
    {
        try
        {
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var info = JsonSerializer.Deserialize<CompanyInfo>(json);
                if (info != null)
                {
                    return new CompanyInfo().Merge(info);
                }
            }
            else
            {
                var defaults = new CompanyInfo();
                var json = JsonSerializer.Serialize(defaults, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
                return defaults;
            }
        }
        catch
        {
            // Si el archivo está corrupto, regresar valores por defecto.
        }

        return new CompanyInfo();
    }
}
