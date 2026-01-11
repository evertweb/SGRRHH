using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace SGRRHH.Local.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IVacacionRepository _vacacionRepository;
    private readonly IRegistroDiarioRepository _registroDiarioRepository;
    private readonly IIncapacidadRepository _incapacidadRepository;
    private readonly ILocalStorageService _storageService;
    private readonly IConfiguracionRepository _configRepository;
    private readonly ILogger<ReportService> _logger;
    
    // Configuración de la empresa (cargada desde BD)
    private string _nombreEmpresa = "EMPRESA S.A.S.";
    private string _nitEmpresa = "900.000.000-0";
    private string _direccionEmpresa = "";
    private string _telefonoEmpresa = "";
    private byte[]? _logoEmpresa;
    
    public ReportService(
        IEmpleadoRepository empleadoRepository,
        IPermisoRepository permisoRepository,
        IVacacionRepository vacacionRepository,
        IRegistroDiarioRepository registroDiarioRepository,
        IIncapacidadRepository incapacidadRepository,
        ILocalStorageService storageService,
        IConfiguracionRepository configRepository,
        ILogger<ReportService> logger)
    {
        _empleadoRepository = empleadoRepository;
        _permisoRepository = permisoRepository;
        _vacacionRepository = vacacionRepository;
        _registroDiarioRepository = registroDiarioRepository;
        _incapacidadRepository = incapacidadRepository;
        _storageService = storageService;
        _configRepository = configRepository;
        _logger = logger;
        
        // Configurar licencia de QuestPDF (Community es gratis)
        QuestPDF.Settings.License = LicenseType.Community;
    }
    
    private async Task CargarConfiguracionEmpresa()
    {
        var nombreConfig = await _configRepository.GetByClaveAsync("empresa.nombre");
        if (nombreConfig != null) _nombreEmpresa = nombreConfig.Valor;
        
        var nitConfig = await _configRepository.GetByClaveAsync("empresa.nit");
        if (nitConfig != null) _nitEmpresa = nitConfig.Valor;
        
        var direccionConfig = await _configRepository.GetByClaveAsync("empresa.direccion");
        if (direccionConfig != null) _direccionEmpresa = direccionConfig.Valor;
        
        var telefonoConfig = await _configRepository.GetByClaveAsync("empresa.telefono");
        if (telefonoConfig != null) _telefonoEmpresa = telefonoConfig.Valor;
        
        var logoResult = await _storageService.GetLogoEmpresaAsync();
        if (logoResult.IsSuccess && logoResult.Value != null)
        {
            _logoEmpresa = logoResult.Value;
        }
    }

    #region Listado de Empleados

    public byte[] GenerarListadoEmpleadosPdf(List<Empleado> empleados)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeListadoEmpleados(content, empleados));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeListadoEmpleados(IContainer container, List<Empleado> empleados)
    {
        container.Column(column =>
        {
            column.Spacing(5);

            column.Item().Text($"Total de empleados: {empleados.Count}")
                .FontSize(11).Bold();

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);  // Código
                    columns.ConstantColumn(80);  // Cédula
                    columns.RelativeColumn(2);   // Nombre
                    columns.RelativeColumn(1.5f); // Cargo
                    columns.RelativeColumn(1.5f); // Departamento
                    columns.ConstantColumn(70);  // Fecha Ingreso
                    columns.ConstantColumn(60);  // Estado
                });

                // Encabezado
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("CÓDIGO").Bold();
                    header.Cell().Element(CellStyle).Text("CÉDULA").Bold();
                    header.Cell().Element(CellStyle).Text("NOMBRE COMPLETO").Bold();
                    header.Cell().Element(CellStyle).Text("CARGO").Bold();
                    header.Cell().Element(CellStyle).Text("DEPARTAMENTO").Bold();
                    header.Cell().Element(CellStyle).Text("F. INGRESO").Bold();
                    header.Cell().Element(CellStyle).Text("ESTADO").Bold();
                });

                // Filas
                foreach (var emp in empleados)
                {
                    table.Cell().Element(CellStyle).Text(emp.Codigo ?? "-");
                    table.Cell().Element(CellStyle).Text(emp.Cedula ?? "-");
                    table.Cell().Element(CellStyle).Text(emp.NombreCompleto);
                    table.Cell().Element(CellStyle).Text(emp.Cargo?.Nombre ?? "-");
                    table.Cell().Element(CellStyle).Text(emp.Departamento?.Nombre ?? "-");
                    table.Cell().Element(CellStyle).Text(emp.FechaIngreso.ToString("dd/MM/yyyy"));
                    table.Cell().Element(CellStyle).Text(emp.Estado.ToString());
                }
            });
        });
    }

    public byte[] GenerarListadoEmpleadosExcel(List<Empleado> empleados)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Empleados");

        // Encabezados
        worksheet.Cell(1, 1).Value = "CÓDIGO";
        worksheet.Cell(1, 2).Value = "CÉDULA";
        worksheet.Cell(1, 3).Value = "NOMBRES";
        worksheet.Cell(1, 4).Value = "APELLIDOS";
        worksheet.Cell(1, 5).Value = "CARGO";
        worksheet.Cell(1, 6).Value = "DEPARTAMENTO";
        worksheet.Cell(1, 7).Value = "FECHA INGRESO";
        worksheet.Cell(1, 8).Value = "TELÉFONO";
        worksheet.Cell(1, 9).Value = "EMAIL";
        worksheet.Cell(1, 10).Value = "ESTADO";

        // Estilo del encabezado
        var headerRange = worksheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Datos
        int row = 2;
        foreach (var emp in empleados)
        {
            worksheet.Cell(row, 1).Value = emp.Codigo ?? "";
            worksheet.Cell(row, 2).Value = emp.Cedula ?? "";
            worksheet.Cell(row, 3).Value = emp.Nombres;
            worksheet.Cell(row, 4).Value = emp.Apellidos;
            worksheet.Cell(row, 5).Value = emp.Cargo?.Nombre ?? "";
            worksheet.Cell(row, 6).Value = emp.Departamento?.Nombre ?? "";
            worksheet.Cell(row, 7).Value = emp.FechaIngreso.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 8).Value = emp.Telefono ?? "";
            worksheet.Cell(row, 9).Value = emp.Email ?? "";
            worksheet.Cell(row, 10).Value = emp.Estado.ToString();
            row++;
        }

        // Ajustar columnas
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Reporte de Permisos

    public byte[] GenerarReportePermisosPdf(List<Permiso> permisos, DateTime fechaInicio, DateTime fechaFin)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Element(content => ComposeHeaderPermisos(content, fechaInicio, fechaFin));
                page.Content().Element(content => ComposeReportePermisos(content, permisos));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeaderPermisos(IContainer container, DateTime fechaInicio, DateTime fechaFin)
    {
        container.Column(column =>
        {
            column.Item().Text("REPORTE DE PERMISOS").FontSize(16).Bold().AlignCenter();
            column.Item().Text($"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}")
                .FontSize(11).AlignCenter();
            column.Item().PaddingBottom(10);
        });
    }

    private void ComposeReportePermisos(IContainer container, List<Permiso> permisos)
    {
        container.Column(column =>
        {
            column.Spacing(5);

            // Estadísticas
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Total: {permisos.Count}").Bold();
                row.RelativeItem().Text($"Días totales: {permisos.Sum(p => p.TotalDias)}").Bold();
                row.RelativeItem().Text($"Aprobados: {permisos.Count(p => p.Estado == EstadoPermiso.Aprobado)}").Bold();
                row.RelativeItem().Text($"Rechazados: {permisos.Count(p => p.Estado == EstadoPermiso.Rechazado)}").Bold();
            });

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70);  // N° Acta
                    columns.ConstantColumn(70);  // Fecha
                    columns.RelativeColumn(2);   // Empleado
                    columns.RelativeColumn(1.5f); // Tipo
                    columns.ConstantColumn(70);  // F. Inicio
                    columns.ConstantColumn(70);  // F. Fin
                    columns.ConstantColumn(40);  // Días
                    columns.ConstantColumn(70);  // Estado
                });

                // Encabezado
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("N° ACTA").Bold();
                    header.Cell().Element(CellStyle).Text("FECHA").Bold();
                    header.Cell().Element(CellStyle).Text("EMPLEADO").Bold();
                    header.Cell().Element(CellStyle).Text("TIPO").Bold();
                    header.Cell().Element(CellStyle).Text("INICIO").Bold();
                    header.Cell().Element(CellStyle).Text("FIN").Bold();
                    header.Cell().Element(CellStyle).Text("DÍAS").Bold();
                    header.Cell().Element(CellStyle).Text("ESTADO").Bold();
                });

                // Filas
                foreach (var permiso in permisos)
                {
                    table.Cell().Element(CellStyle).Text(permiso.NumeroActa ?? "-");
                    table.Cell().Element(CellStyle).Text(permiso.FechaSolicitud.ToString("dd/MM/yyyy"));
                    table.Cell().Element(CellStyle).Text(permiso.Empleado?.NombreCompleto ?? "-");
                    table.Cell().Element(CellStyle).Text(permiso.TipoPermiso?.Nombre ?? "-");
                    table.Cell().Element(CellStyle).Text(permiso.FechaInicio.ToString("dd/MM/yyyy"));
                    table.Cell().Element(CellStyle).Text(permiso.FechaFin.ToString("dd/MM/yyyy"));
                    table.Cell().Element(CellStyle).Text(permiso.TotalDias.ToString());
                    table.Cell().Element(CellStyle).Text(permiso.Estado.ToString());
                }
            });
        });
    }

    public byte[] GenerarReportePermisosExcel(List<Permiso> permisos)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Permisos");

        // Encabezados
        worksheet.Cell(1, 1).Value = "N° ACTA";
        worksheet.Cell(1, 2).Value = "FECHA SOLICITUD";
        worksheet.Cell(1, 3).Value = "EMPLEADO";
        worksheet.Cell(1, 4).Value = "CÉDULA";
        worksheet.Cell(1, 5).Value = "TIPO PERMISO";
        worksheet.Cell(1, 6).Value = "FECHA INICIO";
        worksheet.Cell(1, 7).Value = "FECHA FIN";
        worksheet.Cell(1, 8).Value = "DÍAS";
        worksheet.Cell(1, 9).Value = "MOTIVO";
        worksheet.Cell(1, 10).Value = "ESTADO";

        // Estilo del encabezado
        var headerRange = worksheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Datos
        int row = 2;
        foreach (var permiso in permisos)
        {
            worksheet.Cell(row, 1).Value = permiso.NumeroActa ?? "";
            worksheet.Cell(row, 2).Value = permiso.FechaSolicitud.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 3).Value = permiso.Empleado?.NombreCompleto ?? "";
            worksheet.Cell(row, 4).Value = permiso.Empleado?.Cedula ?? "";
            worksheet.Cell(row, 5).Value = permiso.TipoPermiso?.Nombre ?? "";
            worksheet.Cell(row, 6).Value = permiso.FechaInicio.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 7).Value = permiso.FechaFin.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 8).Value = permiso.TotalDias;
            worksheet.Cell(row, 9).Value = permiso.Motivo ?? "";
            worksheet.Cell(row, 10).Value = permiso.Estado.ToString();
            row++;
        }

        // Totales
        worksheet.Cell(row + 1, 7).Value = "TOTAL DÍAS:";
        worksheet.Cell(row + 1, 7).Style.Font.Bold = true;
        worksheet.Cell(row + 1, 8).Value = permisos.Sum(p => p.TotalDias);
        worksheet.Cell(row + 1, 8).Style.Font.Bold = true;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Reporte de Vacaciones

    public byte[] GenerarReporteVacacionesPdf(List<Vacacion> vacaciones, int periodo)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Element(content => ComposeHeaderVacaciones(content, periodo));
                page.Content().Element(content => ComposeReporteVacaciones(content, vacaciones));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeaderVacaciones(IContainer container, int periodo)
    {
        container.Column(column =>
        {
            column.Item().Text("REPORTE DE VACACIONES").FontSize(16).Bold().AlignCenter();
            column.Item().Text($"Período: {periodo}").FontSize(11).AlignCenter();
            column.Item().PaddingBottom(10);
        });
    }

    private void ComposeReporteVacaciones(IContainer container, List<Vacacion> vacaciones)
    {
        container.Column(column =>
        {
            column.Spacing(5);

            column.Item().Text($"Total de solicitudes: {vacaciones.Count}").Bold();

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);    // Empleado
                    columns.RelativeColumn(1.5f); // Departamento
                    columns.ConstantColumn(70);   // F. Inicio
                    columns.ConstantColumn(70);   // F. Fin
                    columns.ConstantColumn(50);   // Días tomados
                    columns.ConstantColumn(50);   // Días disponibles
                    columns.ConstantColumn(70);   // Estado
                });

                // Encabezado
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("EMPLEADO").Bold();
                    header.Cell().Element(CellStyle).Text("DEPARTAMENTO").Bold();
                    header.Cell().Element(CellStyle).Text("INICIO").Bold();
                    header.Cell().Element(CellStyle).Text("FIN").Bold();
                    header.Cell().Element(CellStyle).Text("DÍAS").Bold();
                    header.Cell().Element(CellStyle).Text("DISPONIBLE").Bold();
                    header.Cell().Element(CellStyle).Text("ESTADO").Bold();
                });

                // Filas
                foreach (var vacacion in vacaciones)
                {
                    table.Cell().Element(CellStyle).Text(vacacion.Empleado?.NombreCompleto ?? "-");
                    table.Cell().Element(CellStyle).Text(vacacion.Empleado?.Departamento?.Nombre ?? "-");
                    table.Cell().Element(CellStyle).Text(vacacion.FechaInicio.ToString("dd/MM/yyyy"));
                    table.Cell().Element(CellStyle).Text(vacacion.FechaFin.ToString("dd/MM/yyyy"));
                    table.Cell().Element(CellStyle).Text(vacacion.DiasTomados.ToString());
                    table.Cell().Element(CellStyle).Text("-"); // DiasDisponibles no existe, se calcula aparte
                    table.Cell().Element(CellStyle).Text(vacacion.Estado.ToString());
                }
            });
        });
    }

    public byte[] GenerarReporteVacacionesExcel(List<Vacacion> vacaciones)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Vacaciones");

        // Encabezados
        worksheet.Cell(1, 1).Value = "EMPLEADO";
        worksheet.Cell(1, 2).Value = "CÉDULA";
        worksheet.Cell(1, 3).Value = "DEPARTAMENTO";
        worksheet.Cell(1, 4).Value = "PERÍODO";
        worksheet.Cell(1, 5).Value = "FECHA INICIO";
        worksheet.Cell(1, 6).Value = "FECHA FIN";
        worksheet.Cell(1, 7).Value = "DÍAS TOMADOS";
        worksheet.Cell(1, 8).Value = "DÍAS DISPONIBLES";
        worksheet.Cell(1, 9).Value = "ESTADO";

        var headerRange = worksheet.Range(1, 1, 1, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int row = 2;
        foreach (var vacacion in vacaciones)
        {
            worksheet.Cell(row, 1).Value = vacacion.Empleado?.NombreCompleto ?? "";
            worksheet.Cell(row, 2).Value = vacacion.Empleado?.Cedula ?? "";
            worksheet.Cell(row, 3).Value = vacacion.Empleado?.Departamento?.Nombre ?? "";
            worksheet.Cell(row, 4).Value = vacacion.PeriodoCorrespondiente;
            worksheet.Cell(row, 5).Value = vacacion.FechaInicio.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 6).Value = vacacion.FechaFin.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 7).Value = vacacion.DiasTomados;
            worksheet.Cell(row, 8).Value = "-"; // DiasDisponibles se calcula aparte
            worksheet.Cell(row, 9).Value = vacacion.Estado.ToString();
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Certificado Laboral

    public byte[] GenerarCertificadoLaboralPdf(Empleado empleado, string proposito, bool incluirSalario)
    {
        var fechaActual = DateTime.Now;
        var tiempoServicio = fechaActual - empleado.FechaIngreso;
        var años = (int)(tiempoServicio.TotalDays / 365.25);
        var meses = (int)((tiempoServicio.TotalDays % 365.25) / 30.44);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(3, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial").LineHeight(1.5f));

                page.Content().Column(column =>
                {
                    column.Spacing(15);

                    // Título
                    column.Item().AlignCenter().Text("CERTIFICADO LABORAL")
                        .FontSize(18).Bold().Underline();

                    column.Item().PaddingTop(30);

                    // Contenido
                    column.Item().Text(text =>
                    {
                        text.Span("La empresa certifica que ").FontSize(12);
                        text.Span($"{empleado.NombreCompleto}").Bold();
                        text.Span($", identificado(a) con cédula de identidad N° ");
                        text.Span($"{empleado.Cedula}").Bold();
                        text.Span($", labora en esta institución desde el ");
                        text.Span($"{empleado.FechaIngreso:dd 'de' MMMM 'de' yyyy}").Bold();
                        text.Span($", desempeñando el cargo de ");
                        text.Span($"{empleado.Cargo?.Nombre ?? "N/A"}").Bold();
                        text.Span($" en el departamento de ");
                        text.Span($"{empleado.Departamento?.Nombre ?? "N/A"}").Bold();
                        text.Span(".");
                    });

                    column.Item().Text($"Tiempo de servicio: {años} año(s) y {meses} mes(es).")
                        .Bold();

                    column.Item().PaddingTop(20).Text(text =>
                    {
                        text.Span("El presente certificado se expide a solicitud del interesado para ");
                        text.Span(proposito.ToLower()).Bold();
                        text.Span(".");
                    });

                    column.Item().PaddingTop(40).AlignRight().Text(text =>
                    {
                        text.Span("Fecha de emisión: ");
                        text.Span($"{fechaActual:dd 'de' MMMM 'de' yyyy}").Bold();
                    });

                    column.Item().PaddingTop(60).AlignCenter().Column(signColumn =>
                    {
                        signColumn.Item().LineHorizontal(1).LineColor(Colors.Black);
                        signColumn.Item().PaddingTop(5).Text("Firma Autorizada").FontSize(10);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    #endregion

    #region Utilidades

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("LISTADO DE EMPLEADOS").FontSize(16).Bold().AlignCenter();
            column.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).AlignCenter();
            column.Item().PaddingBottom(10);
        });
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .Border(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5);
    }

    #endregion

    #region New Report Methods - Phase 6

    // ===== ACTA DE PERMISO =====
    public async Task<Result<byte[]>> GenerarActaPermisoAsync(int permisoId)
    {
        try
        {
            await CargarConfiguracionEmpresa();
            
            var permiso = await _permisoRepository.GetByIdAsync(permisoId);
            if (permiso == null)
                return Result<byte[]>.Fail("Permiso no encontrado");
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(11));
                    
                    // Header con logo y datos de empresa
                    page.Header().Element(ComposeHeaderWithLogo);
                    
                    // Contenido del acta
                    page.Content().Element(c => ComposeActaPermiso(c, permiso));
                    
                    // Footer con número de página
                    page.Footer().Element(ComposeSimpleFooter);
                });
            });
            
            var pdfBytes = document.GeneratePdf();
            
            _logger.LogInformation("Acta de permiso generada: {NumeroActa}", permiso.NumeroActa);
            return Result<byte[]>.Ok(pdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando acta de permiso {PermisoId}", permisoId);
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    private void ComposeHeaderWithLogo(IContainer container)
    {
        container.Row(row =>
        {
            // Logo (si existe)
            if (_logoEmpresa != null)
            {
                row.ConstantItem(80).Image(_logoEmpresa);
            }
            
            // Datos de la empresa
            row.RelativeItem().Column(column =>
            {
                column.Item().AlignCenter().Text(_nombreEmpresa)
                    .Bold().FontSize(14);
                column.Item().AlignCenter().Text($"NIT: {_nitEmpresa}")
                    .FontSize(10);
                if (!string.IsNullOrEmpty(_direccionEmpresa))
                {
                    column.Item().AlignCenter().Text(_direccionEmpresa)
                        .FontSize(9);
                }
                if (!string.IsNullOrEmpty(_telefonoEmpresa))
                {
                    column.Item().AlignCenter().Text($"Tel: {_telefonoEmpresa}")
                        .FontSize(9);
                }
            });
            
            // Espacio para balancear
            row.ConstantItem(80);
        });
    }
    
    private void ComposeActaPermiso(IContainer container, Permiso permiso)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(20);
            
            // Título
            column.Item().AlignCenter().Text("ACTA DE PERMISO")
                .Bold().FontSize(16);
            column.Item().AlignCenter().Text(permiso.NumeroActa)
                .FontSize(12);
            column.Item().PaddingVertical(20);
            
            // Información del empleado
            column.Item().Text(text =>
            {
                text.Span("El(la) empleado(a) ");
                text.Span(permiso.Empleado?.NombreCompleto ?? "N/A").Bold();
                text.Span(", identificado(a) con cédula de ciudadanía No. ");
                text.Span(permiso.Empleado?.Cedula ?? "N/A").Bold();
                text.Span(", quien desempeña el cargo de ");
                text.Span(permiso.Empleado?.Cargo?.Nombre ?? "N/A").Bold();
                text.Span(" en el departamento de ");
                text.Span(permiso.Empleado?.Departamento?.Nombre ?? "N/A").Bold();
                text.Span(", solicita permiso de ");
                text.Span(permiso.TipoPermiso?.Nombre ?? "N/A").Bold();
                text.Span(".");
            });
            
            column.Item().PaddingVertical(15);
            
            // Detalles del permiso
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });
                
                table.Cell().Border(1).Padding(5).Text("Fecha de Solicitud:").Bold();
                table.Cell().Border(1).Padding(5).Text(permiso.FechaSolicitud.ToString("dd/MM/yyyy"));
                
                table.Cell().Border(1).Padding(5).Text("Fecha Inicio:").Bold();
                table.Cell().Border(1).Padding(5).Text(permiso.FechaInicio.ToString("dd/MM/yyyy"));
                
                table.Cell().Border(1).Padding(5).Text("Fecha Fin:").Bold();
                table.Cell().Border(1).Padding(5).Text(permiso.FechaFin.ToString("dd/MM/yyyy"));
                
                table.Cell().Border(1).Padding(5).Text("Total Días:").Bold();
                table.Cell().Border(1).Padding(5).Text(permiso.TotalDias.ToString());
                
                table.Cell().Border(1).Padding(5).Text("Motivo:").Bold();
                table.Cell().Border(1).Padding(5).Text(permiso.Motivo);
                
                table.Cell().Border(1).Padding(5).Text("Estado:").Bold();
                table.Cell().Border(1).Padding(5).Text(permiso.Estado.ToString()).Bold();
            });
            
            column.Item().PaddingVertical(15);
            
            // Observaciones (si hay)
            if (!string.IsNullOrEmpty(permiso.Observaciones))
            {
                column.Item().Text("Observaciones:").Bold();
                column.Item().Text(permiso.Observaciones);
                column.Item().PaddingVertical(10);
            }
            
            // Firmas
            column.Item().PaddingTop(40);
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text("_______________________");
                    c.Item().AlignCenter().Text("Solicitante").FontSize(10);
                    c.Item().AlignCenter().Text(permiso.Empleado?.NombreCompleto ?? "").FontSize(9);
                });
                
                row.ConstantItem(50);
                
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text("_______________________");
                    c.Item().AlignCenter().Text("Aprobador").FontSize(10);
                    c.Item().AlignCenter().Text(permiso.AprobadoPor?.NombreCompleto ?? "").FontSize(9);
                });
            });
        });
    }
    
    private void ComposeSimpleFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Generado el: ").FontSize(8);
                text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
            });
            
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Página ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
                text.Span(" de ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        });
    }
    
    // ===== CERTIFICADO LABORAL =====
    public async Task<Result<byte[]>> GenerarCertificadoLaboralAsync(int empleadoId, string? tipoClasificado = null)
    {
        try
        {
            await CargarConfiguracionEmpresa();
            
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return Result<byte[]>.Fail("Empleado no encontrado");
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    
                    page.Header().Element(ComposeHeaderWithLogo);
                    page.Content().Element(c => ComposeCertificadoLaboral(c, empleado, tipoClasificado));
                    page.Footer().Element(ComposeSimpleFooter);
                });
            });
            
            var pdfBytes = document.GeneratePdf();
            
            _logger.LogInformation("Certificado laboral generado para: {EmpleadoId}", empleadoId);
            return Result<byte[]>.Ok(pdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando certificado laboral");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    private void ComposeCertificadoLaboral(IContainer container, Empleado empleado, string? tipoClasificado)
    {
        var cultura = new CultureInfo("es-CO");
        
        container.Column(column =>
        {
            column.Item().PaddingVertical(20);
            
            // Título
            column.Item().AlignCenter().Text("CERTIFICADO LABORAL")
                .Bold().FontSize(16);
            column.Item().PaddingVertical(30);
            
            // Dirigido a
            column.Item().Text("A QUIEN PUEDA INTERESAR:");
            column.Item().PaddingVertical(20);
            
            // Cuerpo del certificado
            column.Item().Text(text =>
            {
                text.Span("El suscrito Representante Legal de ");
                text.Span(_nombreEmpresa).Bold();
                text.Span(", identificada con NIT ");
                text.Span(_nitEmpresa).Bold();
                text.Span(", certifica que:");
            });
            
            column.Item().PaddingVertical(15);
            
            column.Item().Text(text =>
            {
                text.Span("El(la) señor(a) ");
                text.Span(empleado.NombreCompleto).Bold();
                text.Span(", identificado(a) con cédula de ciudadanía No. ");
                text.Span(empleado.Cedula).Bold();
                
                if (empleado.Estado == EstadoEmpleado.Activo)
                {
                    text.Span(", se encuentra vinculado(a) laboralmente con nuestra empresa desde el ");
                    text.Span(empleado.FechaIngreso.ToString("dd 'de' MMMM 'de' yyyy", cultura)).Bold();
                    text.Span(" hasta la fecha");
                }
                else
                {
                    text.Span(", estuvo vinculado(a) laboralmente con nuestra empresa desde el ");
                    text.Span(empleado.FechaIngreso.ToString("dd 'de' MMMM 'de' yyyy", cultura)).Bold();
                    text.Span(" hasta el ");
                    text.Span((empleado.FechaRetiro ?? DateTime.Now).ToString("dd 'de' MMMM 'de' yyyy", cultura)).Bold();
                }
                text.Span(", desempeñando el cargo de ");
                text.Span(empleado.Cargo?.Nombre ?? "N/A").Bold();
                text.Span(".");
            });
            
            column.Item().PaddingVertical(15);
            
            // Tipo de contrato - Comentado: ahora se obtiene de la tabla Contrato
            // TODO: Cargar el contrato activo del empleado para mostrar el tipo
            /* column.Item().Text(text =>
            {
                text.Span("Tipo de contrato: ");
                text.Span(empleado.TipoContrato.ToString()).Bold();
            }); */
            
            // Salario (si no es clasificado)
            if (tipoClasificado != "clasificado" && empleado.SalarioBase.HasValue)
            {
                column.Item().PaddingTop(10);
                column.Item().Text(text =>
                {
                    text.Span("Salario mensual: ");
                    text.Span($"${empleado.SalarioBase:N0} COP").Bold();
                });
            }
            
            column.Item().PaddingVertical(20);
            
            // Cierre
            column.Item().Text("El presente certificado se expide a solicitud del interesado para los fines que estime conveniente.");
            
            column.Item().PaddingVertical(10);
            
            column.Item().Text(text =>
            {
                text.Span("Dado en ");
                text.Span(!string.IsNullOrEmpty(_direccionEmpresa) ? _direccionEmpresa : "la ciudad");
                text.Span(", a los ");
                text.Span(DateTime.Now.ToString("dd 'días del mes de' MMMM 'de' yyyy", cultura));
                text.Span(".");
            });
            
            // Firma
            column.Item().PaddingTop(50);
            column.Item().AlignCenter().Text("_______________________");
            column.Item().AlignCenter().Text("Representante Legal").FontSize(10);
            column.Item().AlignCenter().Text(_nombreEmpresa).FontSize(9);
        });
    }
    
    // ===== LISTADO DE EMPLEADOS =====
    public async Task<Result<byte[]>> GenerarListadoEmpleadosAsync(ListadoEmpleadosOptions? options = null)
    {
        try
        {
            await CargarConfiguracionEmpresa();
            
            var empleados = await _empleadoRepository.GetAllAsync();
            
            // Aplicar filtros
            if (options != null)
            {
                if (options.DepartamentoId.HasValue)
                {
                    empleados = empleados.Where(e => e.DepartamentoId == options.DepartamentoId.Value).ToList();
                }
                
                if (options.Estado.HasValue)
                {
                    empleados = empleados.Where(e => e.Estado == options.Estado.Value).ToList();
                }
            }
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter.Landscape());
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    
                    page.Header().Element(c => ComposeListadoEmpleadosHeader(c, options));
                    page.Content().Element(c => ComposeListadoEmpleadosContent(c, empleados));
                    page.Footer().Element(ComposeSimpleFooter);
                });
            });
            
            var pdfBytes = document.GeneratePdf();
            
            _logger.LogInformation("Listado de empleados generado: {Count} registros", empleados.Count());
            return Result<byte[]>.Ok(pdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando listado de empleados");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    private void ComposeListadoEmpleadosHeader(IContainer container, ListadoEmpleadosOptions? options)
    {
        container.Column(column =>
        {
            // Header empresarial
            column.Item().Row(row =>
            {
                if (_logoEmpresa != null)
                {
                    row.ConstantItem(60).Image(_logoEmpresa);
                }
                
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(_nombreEmpresa).Bold().FontSize(12);
                    c.Item().AlignCenter().Text($"NIT: {_nitEmpresa}").FontSize(9);
                });
                
                row.ConstantItem(60);
            });
            
            column.Item().PaddingVertical(10);
            
            // Título del reporte
            column.Item().AlignCenter().Text("LISTADO DE EMPLEADOS")
                .Bold().FontSize(14);
            
            // Filtros aplicados
            if (options != null)
            {
                column.Item().PaddingTop(5);
                var filtros = new List<string>();
                
                if (options.DepartamentoId.HasValue)
                    filtros.Add($"Departamento: {options.DepartamentoId}");
                    
                if (options.Estado.HasValue)
                    filtros.Add($"Estado: {options.Estado}");
                
                if (filtros.Any())
                {
                    column.Item().AlignCenter().Text($"Filtros: {string.Join(" | ", filtros)}")
                        .FontSize(9).Italic();
                }
            }
            
            column.Item().PaddingVertical(5);
        });
    }
    
    private void ComposeListadoEmpleadosContent(IContainer container, IEnumerable<Empleado> empleados)
    {
        container.Column(column =>
        {
            // Tabla de empleados
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);  // Código
                    columns.ConstantColumn(80);  // Cédula
                    columns.RelativeColumn(2);   // Nombre
                    columns.RelativeColumn(1.5f); // Cargo
                    columns.RelativeColumn(1.5f); // Departamento
                    columns.ConstantColumn(80);  // Fecha Ingreso
                    columns.ConstantColumn(70);  // Estado
                });
                
                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Código").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Cédula").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Nombre Completo").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Cargo").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Departamento").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("F. Ingreso").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Estado").Bold();
                });
                
                // Filas
                foreach (var empleado in empleados)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(empleado.Codigo).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(empleado.Cedula).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(empleado.NombreCompleto).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(empleado.Cargo?.Nombre ?? "N/A").FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(empleado.Departamento?.Nombre ?? "N/A").FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(empleado.FechaIngreso.ToString("dd/MM/yyyy")).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(empleado.Estado.ToString()).FontSize(8);
                }
            });
            
            // Resumen
            column.Item().PaddingTop(15);
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Total de empleados: ").Bold();
                    text.Span(empleados.Count().ToString());
                });
                
                row.RelativeItem().Text(text =>
                {
                    text.Span("Activos: ").Bold();
                    text.Span(empleados.Count(e => e.Estado == EstadoEmpleado.Activo).ToString());
                });
                
                row.RelativeItem().Text(text =>
                {
                    text.Span("Inactivos: ").Bold();
                    text.Span(empleados.Count(e => e.Estado != EstadoEmpleado.Activo).ToString());
                });
            });
        });
    }
    
    // ===== REPORTE DE PERMISOS =====
    public async Task<Result<byte[]>> GenerarReportePermisosAsync(ReportePermisosOptions options)
    {
        try
        {
            await CargarConfiguracionEmpresa();
            
            var permisos = await _permisoRepository.GetAllAsync();
            
            // Aplicar filtros
            permisos = permisos.Where(p => 
                p.FechaInicio >= options.FechaInicio && 
                p.FechaInicio <= options.FechaFin).ToList();
            
            if (options.EmpleadoId.HasValue)
            {
                permisos = permisos.Where(p => p.EmpleadoId == options.EmpleadoId.Value).ToList();
            }
            
            if (options.TipoPermisoId.HasValue)
            {
                permisos = permisos.Where(p => p.TipoPermisoId == options.TipoPermisoId.Value).ToList();
            }
            
            if (options.Estado.HasValue)
            {
                permisos = permisos.Where(p => p.Estado == options.Estado.Value).ToList();
            }
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter.Landscape());
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(9));
                    
                    page.Header().Element(c => ComposeReportePermisosHeaderPhase6(c, options));
                    page.Content().Element(c => ComposeReportePermisosContent(c, permisos));
                    page.Footer().Element(ComposeSimpleFooter);
                });
            });
            
            var pdfBytes = document.GeneratePdf();
            
            _logger.LogInformation("Reporte de permisos generado: {Count} registros", permisos.Count());
            return Result<byte[]>.Ok(pdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte de permisos");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    private void ComposeReportePermisosHeaderPhase6(IContainer container, ReportePermisosOptions options)
    {
        container.Column(column =>
        {
            // Header empresarial simplificado
            column.Item().Row(row =>
            {
                if (_logoEmpresa != null)
                {
                    row.ConstantItem(50).Image(_logoEmpresa);
                }
                
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(_nombreEmpresa).Bold().FontSize(11);
                    c.Item().AlignCenter().Text("REPORTE DE PERMISOS").Bold().FontSize(12);
                });
                
                row.ConstantItem(50);
            });
            
            column.Item().PaddingVertical(5);
            
            // Período
            column.Item().AlignCenter().Text($"Período: {options.FechaInicio:dd/MM/yyyy} - {options.FechaFin:dd/MM/yyyy}")
                .FontSize(9);
        });
    }
    
    private void ComposeReportePermisosContent(IContainer container, IEnumerable<Permiso> permisos)
    {
        container.Column(column =>
        {
            // Tabla
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);  // Nº Acta
                    columns.RelativeColumn(2);   // Empleado
                    columns.RelativeColumn(1.5f); // Tipo
                    columns.ConstantColumn(70);  // F. Inicio
                    columns.ConstantColumn(70);  // F. Fin
                    columns.ConstantColumn(40);  // Días
                    columns.ConstantColumn(70);  // Estado
                });
                
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Nº Acta").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Empleado").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Tipo Permiso").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("F. Inicio").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("F. Fin").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Días").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Estado").Bold().FontSize(8);
                });
                
                foreach (var permiso in permisos)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(permiso.NumeroActa).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(permiso.Empleado?.NombreCompleto ?? "N/A").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(permiso.TipoPermiso?.Nombre ?? "N/A").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(permiso.FechaInicio.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(permiso.FechaFin.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .AlignCenter().Text(permiso.TotalDias.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(permiso.Estado.ToString()).FontSize(7);
                }
            });
            
            // Resumen
            column.Item().PaddingTop(10);
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Total permisos: ").Bold().FontSize(9);
                    text.Span(permisos.Count().ToString()).FontSize(9);
                });
                
                row.RelativeItem().Text(text =>
                {
                    text.Span("Total días: ").Bold().FontSize(9);
                    text.Span(permisos.Sum(p => p.TotalDias).ToString()).FontSize(9);
                });
                
                row.RelativeItem().Text(text =>
                {
                    text.Span("Aprobados: ").Bold().FontSize(9);
                    text.Span(permisos.Count(p => p.Estado == EstadoPermiso.Aprobado).ToString()).FontSize(9);
                });
            });
        });
    }
    
    // ===== REPORTE DE VACACIONES =====
    public async Task<Result<byte[]>> GenerarReporteVacacionesAsync(ReporteVacacionesOptions options)
    {
        try
        {
            await CargarConfiguracionEmpresa();
            
            var vacaciones = await _vacacionRepository.GetAllAsync();
            
            // Filtrar por año
            vacaciones = vacaciones.Where(v => v.FechaInicio.Year == options.Año).ToList();
            
            if (options.EmpleadoId.HasValue)
            {
                vacaciones = vacaciones.Where(v => v.EmpleadoId == options.EmpleadoId.Value).ToList();
            }
            
            if (options.DepartamentoId.HasValue)
            {
                vacaciones = vacaciones.Where(v => 
                    v.Empleado?.DepartamentoId == options.DepartamentoId.Value).ToList();
            }
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter.Landscape());
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(9));
                    
                    page.Header().Element(c => ComposeReporteVacacionesHeaderPhase6(c, options));
                    page.Content().Element(c => ComposeReporteVacacionesContent(c, vacaciones));
                    page.Footer().Element(ComposeSimpleFooter);
                });
            });
            
            var pdfBytes = document.GeneratePdf();
            
            _logger.LogInformation("Reporte de vacaciones generado: {Count} registros", vacaciones.Count());
            return Result<byte[]>.Ok(pdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte de vacaciones");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    private void ComposeReporteVacacionesHeaderPhase6(IContainer container, ReporteVacacionesOptions options)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                if (_logoEmpresa != null)
                {
                    row.ConstantItem(50).Image(_logoEmpresa);
                }
                
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(_nombreEmpresa).Bold().FontSize(11);
                    c.Item().AlignCenter().Text("REPORTE DE VACACIONES").Bold().FontSize(12);
                });
                
                row.ConstantItem(50);
            });
            
            column.Item().PaddingVertical(5);
            column.Item().AlignCenter().Text($"Año: {options.Año}").FontSize(9);
        });
    }
    
    private void ComposeReporteVacacionesContent(IContainer container, IEnumerable<Vacacion> vacaciones)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);   // Empleado
                    columns.RelativeColumn(1.5f); // Departamento
                    columns.ConstantColumn(70);  // F. Inicio
                    columns.ConstantColumn(70);  // F. Fin
                    columns.ConstantColumn(40);  // Días
                    columns.ConstantColumn(60);  // Período
                    columns.ConstantColumn(70);  // Estado
                });
                
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Empleado").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Departamento").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("F. Inicio").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("F. Fin").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Días").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Período").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Estado").Bold().FontSize(8);
                });
                
                foreach (var vacacion in vacaciones)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(vacacion.Empleado?.NombreCompleto ?? "N/A").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(vacacion.Empleado?.Departamento?.Nombre ?? "N/A").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(vacacion.FechaInicio.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(vacacion.FechaFin.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .AlignCenter().Text(vacacion.DiasTomados.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .AlignCenter().Text(vacacion.PeriodoCorrespondiente.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(vacacion.Estado.ToString()).FontSize(7);
                }
            });
            
            // Resumen
            column.Item().PaddingTop(10);
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Total registros: ").Bold().FontSize(9);
                    text.Span(vacaciones.Count().ToString()).FontSize(9);
                });
                
                row.RelativeItem().Text(text =>
                {
                    text.Span("Total días tomados: ").Bold().FontSize(9);
                    text.Span(vacaciones.Sum(v => v.DiasTomados).ToString()).FontSize(9);
                });
            });
        });
    }
    
    // ===== REPORTE DE ASISTENCIA =====
    public async Task<Result<byte[]>> GenerarReporteAsistenciaAsync(ReporteAsistenciaOptions options)
    {
        try
        {
            await CargarConfiguracionEmpresa();
            
            var registros = await _registroDiarioRepository.GetAllAsync();
            
            // Aplicar filtros
            registros = registros.Where(r => 
                r.Fecha >= options.FechaInicio && 
                r.Fecha <= options.FechaFin).ToList();
            
            if (options.EmpleadoId.HasValue)
            {
                registros = registros.Where(r => r.EmpleadoId == options.EmpleadoId.Value).ToList();
            }
            
            if (options.DepartamentoId.HasValue)
            {
                registros = registros.Where(r => 
                    r.Empleado?.DepartamentoId == options.DepartamentoId.Value).ToList();
            }
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter.Landscape());
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(9));
                    
                    page.Header().Element(c => ComposeReporteAsistenciaHeaderPhase6(c, options));
                    page.Content().Element(c => ComposeReporteAsistenciaContent(c, registros));
                    page.Footer().Element(ComposeSimpleFooter);
                });
            });
            
            var pdfBytes = document.GeneratePdf();
            
            _logger.LogInformation("Reporte de asistencia generado: {Count} registros", registros.Count());
            return Result<byte[]>.Ok(pdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte de asistencia");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    private void ComposeReporteAsistenciaHeaderPhase6(IContainer container, ReporteAsistenciaOptions options)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                if (_logoEmpresa != null)
                {
                    row.ConstantItem(50).Image(_logoEmpresa);
                }
                
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(_nombreEmpresa).Bold().FontSize(11);
                    c.Item().AlignCenter().Text("REPORTE DE ASISTENCIA").Bold().FontSize(12);
                });
                
                row.ConstantItem(50);
            });
            
            column.Item().PaddingVertical(5);
            column.Item().AlignCenter().Text($"Período: {options.FechaInicio:dd/MM/yyyy} - {options.FechaFin:dd/MM/yyyy}")
                .FontSize(9);
        });
    }
    
    private void ComposeReporteAsistenciaContent(IContainer container, IEnumerable<RegistroDiario> registros)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70);  // Fecha
                    columns.RelativeColumn(2);   // Empleado
                    columns.RelativeColumn(1.5f); // Departamento
                    columns.ConstantColumn(60);  // Entrada
                    columns.ConstantColumn(60);  // Salida
                    columns.ConstantColumn(60);  // H. Trabajadas
                    columns.RelativeColumn(1);   // Observaciones
                });
                
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Fecha").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Empleado").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Departamento").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Entrada").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Salida").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("H. Trab.").Bold().FontSize(8);
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Observaciones").Bold().FontSize(8);
                });
                
                foreach (var registro in registros)
                {
                    var horasTrabajadas = registro.HoraEntrada.HasValue && registro.HoraSalida.HasValue
                        ? (registro.HoraSalida.Value - registro.HoraEntrada.Value).ToString(@"hh\:mm")
                        : "-";
                    
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(registro.Fecha.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(registro.Empleado?.NombreCompleto ?? "N/A").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(registro.Empleado?.Departamento?.Nombre ?? "N/A").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(registro.HoraEntrada?.ToString(@"hh\:mm") ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(registro.HoraSalida?.ToString(@"hh\:mm") ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(horasTrabajadas).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(registro.Observaciones ?? "").FontSize(7);
                }
            });
            
            // Resumen
            column.Item().PaddingTop(10);
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Total registros: ").Bold().FontSize(9);
                    text.Span(registros.Count().ToString()).FontSize(9);
                });
                
                var totalHoras = registros
                    .Where(r => r.HoraEntrada.HasValue && r.HoraSalida.HasValue)
                    .Sum(r => (r.HoraSalida!.Value - r.HoraEntrada!.Value).TotalHours);
                
                row.RelativeItem().Text(text =>
                {
                    text.Span("Total horas: ").Bold().FontSize(9);
                    text.Span($"{totalHoras:F1}h").FontSize(9);
                });
            });
        });
    }
    
    // ===== UTILIDAD: PDF DESDE HTML =====
    public byte[] GenerarPdfDesdeHtml(string html)
    {
        // Implementación básica - QuestPDF no soporta HTML directamente
        // Esta es una funcionalidad placeholder que podría usar otra librería
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(11));
                
                page.Content().Text("Generación desde HTML no implementada. Use QuestPDF directamente.");
            });
        });
        
        return document.GeneratePdf();
    }

    // ===== MÉTODOS REQUERIDOS POR INTERFAZ =====
    public async Task<Result<byte[]>> GenerarCertificadoLaboralPdf(int empleadoId)
    {
        return await GenerarCertificadoLaboralAsync(empleadoId);
    }
    
    public async Task<Result<byte[]>> GenerarListadoEmpleadosPdf(ListadoEmpleadosOptions? options = null)
    {
        return await GenerarListadoEmpleadosAsync(options);
    }
    
    public async Task<Result<byte[]>> GenerarListadoEmpleadosExcel(ListadoEmpleadosOptions? options = null)
    {
        try
        {
            var empleados = await _empleadoRepository.GetAllAsync();
            
            if (options != null)
            {
                if (options.DepartamentoId.HasValue)
                    empleados = empleados.Where(e => e.DepartamentoId == options.DepartamentoId.Value).ToList();
                if (options.Estado.HasValue)
                    empleados = empleados.Where(e => e.Estado == options.Estado.Value).ToList();
            }
            
            var bytes = GenerarListadoEmpleadosExcel(empleados.ToList());
            return Result<byte[]>.Ok(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando listado de empleados Excel");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Result<byte[]>> GenerarReportePermisosPdf(ReportePermisosOptions options)
    {
        return await GenerarReportePermisosAsync(options);
    }
    
    public async Task<Result<byte[]>> GenerarReportePermisosExcel(ReportePermisosOptions options)
    {
        try
        {
            var permisos = await _permisoRepository.GetAllAsync();
            
            permisos = permisos.Where(p => 
                p.FechaInicio >= options.FechaInicio && 
                p.FechaInicio <= options.FechaFin).ToList();
            
            if (options.EmpleadoId.HasValue)
                permisos = permisos.Where(p => p.EmpleadoId == options.EmpleadoId.Value).ToList();
            if (options.TipoPermisoId.HasValue)
                permisos = permisos.Where(p => p.TipoPermisoId == options.TipoPermisoId.Value).ToList();
            if (options.Estado.HasValue)
                permisos = permisos.Where(p => p.Estado == options.Estado.Value).ToList();
            
            var bytes = GenerarReportePermisosExcel(permisos.ToList());
            return Result<byte[]>.Ok(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte de permisos Excel");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Result<byte[]>> GenerarReporteVacacionesPdf(ReporteVacacionesOptions options)
    {
        return await GenerarReporteVacacionesAsync(options);
    }
    
    public async Task<Result<byte[]>> GenerarReporteVacacionesExcel(ReporteVacacionesOptions options)
    {
        try
        {
            var vacaciones = await _vacacionRepository.GetAllAsync();
            
            vacaciones = vacaciones.Where(v => v.FechaInicio.Year == options.Año).ToList();
            
            if (options.EmpleadoId.HasValue)
                vacaciones = vacaciones.Where(v => v.EmpleadoId == options.EmpleadoId.Value).ToList();
            if (options.DepartamentoId.HasValue)
                vacaciones = vacaciones.Where(v => 
                    v.Empleado?.DepartamentoId == options.DepartamentoId.Value).ToList();
            
            var bytes = GenerarReporteVacacionesExcel(vacaciones.ToList());
            return Result<byte[]>.Ok(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte de vacaciones Excel");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }

    #endregion
    
    #region Reporte de Incapacidades
    
    public async Task<Result<byte[]>> GenerarReporteCobroIncapacidadesExcel(int año, int mes)
    {
        try
        {
            await CargarConfiguracionEmpresa();
            var reporte = await _incapacidadRepository.GetReporteCobroAsync(año, mes);
            
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Cobro Incapacidades");
            
            // === ENCABEZADO ===
            worksheet.Cell(1, 1).Value = _nombreEmpresa;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Range(1, 1, 1, 12).Merge();
            
            worksheet.Cell(2, 1).Value = $"NIT: {_nitEmpresa}";
            worksheet.Range(2, 1, 2, 12).Merge();
            
            worksheet.Cell(3, 1).Value = $"REPORTE DE COBRO DE INCAPACIDADES - {reporte.Periodo}";
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            worksheet.Range(3, 1, 3, 12).Merge();
            
            worksheet.Cell(4, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Range(4, 1, 4, 12).Merge();
            
            // === RESUMEN ===
            var rowResumen = 6;
            worksheet.Cell(rowResumen, 1).Value = "RESUMEN";
            worksheet.Cell(rowResumen, 1).Style.Font.Bold = true;
            worksheet.Range(rowResumen, 1, rowResumen, 4).Merge();
            worksheet.Range(rowResumen, 1, rowResumen, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
            
            worksheet.Cell(rowResumen + 1, 1).Value = "Total Incapacidades:";
            worksheet.Cell(rowResumen + 1, 2).Value = reporte.TotalIncapacidades;
            
            worksheet.Cell(rowResumen + 2, 1).Value = "Total Días:";
            worksheet.Cell(rowResumen + 2, 2).Value = reporte.TotalDias;
            
            worksheet.Cell(rowResumen + 3, 1).Value = "Total Por Cobrar:";
            worksheet.Cell(rowResumen + 3, 2).Value = reporte.TotalPorCobrar;
            worksheet.Cell(rowResumen + 3, 2).Style.NumberFormat.Format = "$#,##0";
            worksheet.Cell(rowResumen + 3, 2).Style.Font.Bold = true;
            
            // Totales por entidad
            var rowEntidad = rowResumen + 5;
            worksheet.Cell(rowEntidad, 1).Value = "TOTALES POR ENTIDAD";
            worksheet.Cell(rowEntidad, 1).Style.Font.Bold = true;
            worksheet.Range(rowEntidad, 1, rowEntidad, 4).Merge();
            worksheet.Range(rowEntidad, 1, rowEntidad, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
            
            var entidadRow = rowEntidad + 1;
            foreach (var entidad in reporte.TotalesPorEntidad)
            {
                worksheet.Cell(entidadRow, 1).Value = entidad.Key;
                worksheet.Cell(entidadRow, 2).Value = entidad.Value;
                worksheet.Cell(entidadRow, 2).Style.NumberFormat.Format = "$#,##0";
                entidadRow++;
            }
            
            // === DETALLE ===
            var rowHeader = entidadRow + 2;
            worksheet.Cell(rowHeader, 1).Value = "DETALLE DE INCAPACIDADES";
            worksheet.Cell(rowHeader, 1).Style.Font.Bold = true;
            worksheet.Range(rowHeader, 1, rowHeader, 12).Merge();
            worksheet.Range(rowHeader, 1, rowHeader, 12).Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Encabezados de tabla
            var headerRow = rowHeader + 1;
            var headers = new[] { "No.", "Cédula", "Empleado", "Cargo", "Tipo", "Fecha Inicio", "Fecha Fin", 
                                  "Días EPS/ARL", "Entidad", "Valor Día", "% Pago", "Valor Cobrar" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(headerRow, i + 1).Value = headers[i];
                worksheet.Cell(headerRow, i + 1).Style.Font.Bold = true;
                worksheet.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                worksheet.Cell(headerRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            
            // Datos
            var dataRow = headerRow + 1;
            foreach (var item in reporte.Items)
            {
                worksheet.Cell(dataRow, 1).Value = item.NumeroIncapacidad;
                worksheet.Cell(dataRow, 2).Value = item.EmpleadoCedula;
                worksheet.Cell(dataRow, 3).Value = item.EmpleadoNombre;
                worksheet.Cell(dataRow, 4).Value = item.Cargo;
                worksheet.Cell(dataRow, 5).Value = item.TipoIncapacidad;
                worksheet.Cell(dataRow, 6).Value = item.FechaInicio.ToString("dd/MM/yyyy");
                worksheet.Cell(dataRow, 7).Value = item.FechaFin.ToString("dd/MM/yyyy");
                worksheet.Cell(dataRow, 8).Value = item.DiasEpsArl;
                worksheet.Cell(dataRow, 9).Value = item.EntidadPagadora;
                worksheet.Cell(dataRow, 10).Value = item.ValorDiaBase;
                worksheet.Cell(dataRow, 10).Style.NumberFormat.Format = "$#,##0";
                worksheet.Cell(dataRow, 11).Value = $"{item.PorcentajePago:F2}%";
                worksheet.Cell(dataRow, 12).Value = item.ValorCobrar;
                worksheet.Cell(dataRow, 12).Style.NumberFormat.Format = "$#,##0";
                
                // Bordes
                for (int i = 1; i <= 12; i++)
                {
                    worksheet.Cell(dataRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                
                dataRow++;
            }
            
            // Total final
            worksheet.Cell(dataRow, 11).Value = "TOTAL:";
            worksheet.Cell(dataRow, 11).Style.Font.Bold = true;
            worksheet.Cell(dataRow, 12).Value = reporte.TotalPorCobrar;
            worksheet.Cell(dataRow, 12).Style.NumberFormat.Format = "$#,##0";
            worksheet.Cell(dataRow, 12).Style.Font.Bold = true;
            
            // Ajustar anchos
            worksheet.Columns().AdjustToContents();
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            
            _logger.LogInformation("Reporte de cobro de incapacidades generado: {Periodo}", reporte.Periodo);
            return Result<byte[]>.Ok(stream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte de cobro de incapacidades");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    #endregion
}
