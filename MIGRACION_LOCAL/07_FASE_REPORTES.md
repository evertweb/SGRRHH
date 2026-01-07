# 📊 FASE 6: Reportes con QuestPDF

## 📋 Contexto

Fases anteriores completadas:
- ✅ Estructura base del proyecto
- ✅ Infraestructura SQLite con Dapper
- ✅ Sistema de archivos local
- ✅ Autenticación local con BCrypt
- ✅ UI Premium estilo ForestechOil
- ✅ Todas las páginas migradas

**Objetivo:** Implementar generación de documentos PDF profesionales usando QuestPDF.

---

## 🎯 Objetivo de esta Fase

Crear plantillas de documentos PDF para actas de permiso, certificados laborales, reportes de empleados, y más.

---

## 📝 PROMPT PARA CLAUDE

```
Necesito que implementes el sistema de generación de PDFs con QuestPDF para SGRRHH.Local.

**PROYECTO:** SGRRHH.Local.Infrastructure/Services/

**DOCUMENTOS A GENERAR:**

1. Acta de Permiso
2. Certificado Laboral
3. Listado de Empleados
4. Reporte de Permisos por Período
5. Reporte de Vacaciones por Período
6. Reporte de Asistencia

---

## ARCHIVOS A CREAR:

### 1. Interfaz IReportService.cs (en Shared/Interfaces/)

```csharp
public interface IReportService
{
    // Documentos individuales
    Task<Result<byte[]>> GenerarActaPermisoAsync(int permisoId);
    Task<Result<byte[]>> GenerarCertificadoLaboralAsync(int empleadoId, string? tipoClasificado = null);
    
    // Listados
    Task<Result<byte[]>> GenerarListadoEmpleadosAsync(ListadoEmpleadosOptions? options = null);
    Task<Result<byte[]>> GenerarReportePermisosAsync(ReportePermisosOptions options);
    Task<Result<byte[]>> GenerarReporteVacacionesAsync(ReporteVacacionesOptions options);
    Task<Result<byte[]>> GenerarReporteAsistenciaAsync(ReporteAsistenciaOptions options);
    
    // Utilidades
    byte[] GenerarPdfDesdeHtml(string html);
}

public class ListadoEmpleadosOptions
{
    public int? DepartamentoId { get; set; }
    public EstadoEmpleado? Estado { get; set; }
    public bool IncluirFotos { get; set; } = false;
}

public class ReportePermisosOptions
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int? EmpleadoId { get; set; }
    public int? TipoPermisoId { get; set; }
    public EstadoPermiso? Estado { get; set; }
}

public class ReporteVacacionesOptions
{
    public int Año { get; set; }
    public int? EmpleadoId { get; set; }
    public int? DepartamentoId { get; set; }
}

public class ReporteAsistenciaOptions
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int? EmpleadoId { get; set; }
    public int? DepartamentoId { get; set; }
}
```

---

### 2. ReportService.cs (en Infrastructure/Services/)

```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ReportService : IReportService
{
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IVacacionRepository _vacacionRepository;
    private readonly IRegistroDiarioRepository _registroDiarioRepository;
    private readonly ILocalStorageService _storageService;
    private readonly IConfiguracionRepository _configRepository;
    private readonly ILogger<ReportService> _logger;
    
    // Configuración de la empresa (cargada desde BD)
    private string? _nombreEmpresa;
    private string? _nitEmpresa;
    private string? _direccionEmpresa;
    private string? _telefonoEmpresa;
    private byte[]? _logoEmpresa;
    
    public ReportService(
        IEmpleadoRepository empleadoRepository,
        IPermisoRepository permisoRepository,
        IVacacionRepository vacacionRepository,
        IRegistroDiarioRepository registroDiarioRepository,
        ILocalStorageService storageService,
        IConfiguracionRepository configRepository,
        ILogger<ReportService> logger)
    {
        _empleadoRepository = empleadoRepository;
        _permisoRepository = permisoRepository;
        _vacacionRepository = vacacionRepository;
        _registroDiarioRepository = registroDiarioRepository;
        _storageService = storageService;
        _configRepository = configRepository;
        _logger = logger;
        
        // Configurar licencia de QuestPDF (Community es gratis)
        QuestPDF.Settings.License = LicenseType.Community;
    }
    
    private async Task CargarConfiguracionEmpresa()
    {
        _nombreEmpresa = (await _configRepository.GetByClaveAsync("empresa.nombre"))?.Valor ?? "EMPRESA S.A.S.";
        _nitEmpresa = (await _configRepository.GetByClaveAsync("empresa.nit"))?.Valor ?? "900.000.000-0";
        _direccionEmpresa = (await _configRepository.GetByClaveAsync("empresa.direccion"))?.Valor ?? "";
        _telefonoEmpresa = (await _configRepository.GetByClaveAsync("empresa.telefono"))?.Valor ?? "";
        
        var logoResult = await _storageService.GetLogoEmpresaAsync();
        if (logoResult.Success)
        {
            _logoEmpresa = logoResult.Data;
        }
    }
    
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
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));
                    
                    // Header con logo y datos de empresa
                    page.Header().Element(ComposeHeader);
                    
                    // Contenido del acta
                    page.Content().Element(c => ComposeActaPermiso(c, permiso));
                    
                    // Footer con número de página
                    page.Footer().Element(ComposeFooter);
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
    
    private void ComposeHeader(IContainer container)
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
        
        container.PaddingBottom(20);
    }
    
    private void ComposeActaPermiso(IContainer container, Permiso permiso)
    {
        container.Column(column =>
        {
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
    
    private void ComposeFooter(IContainer container)
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
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));
                    
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeCertificadoLaboral(c, empleado, tipoClasificado));
                    page.Footer().Element(ComposeFooter);
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
    
    private void ComposeCertificadoLaboral(IContainer container, Empleado empleado, string? destinatario)
    {
        container.Column(column =>
        {
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
                    text.Span(empleado.FechaIngreso.ToString("dd 'de' MMMM 'de' yyyy", 
                        new System.Globalization.CultureInfo("es-CO"))).Bold();
                    text.Span(" hasta la fecha");
                }
                else
                {
                    text.Span(", estuvo vinculado(a) laboralmente con nuestra empresa desde el ");
                    text.Span(empleado.FechaIngreso.ToString("dd 'de' MMMM 'de' yyyy",
                        new System.Globalization.CultureInfo("es-CO"))).Bold();
                    text.Span(" hasta el ");
                    text.Span((empleado.FechaRetiro ?? DateTime.Now).ToString("dd 'de' MMMM 'de' yyyy",
                        new System.Globalization.CultureInfo("es-CO"))).Bold();
                }
                text.Span(", desempeñando el cargo de ");
                text.Span(empleado.Cargo?.Nombre ?? "N/A").Bold();
                text.Span(".");
            });
            
            column.Item().PaddingVertical(15);
            
            // Tipo de contrato
            column.Item().Text(text =>
            {
                text.Span("Tipo de contrato: ");
                text.Span(empleado.TipoContrato.ToString()).Bold();
            });
            
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
                text.Span(_direccionEmpresa ?? "la ciudad");
                text.Span(", a los ");
                text.Span(DateTime.Now.ToString("dd 'días del mes de' MMMM 'de' yyyy",
                    new System.Globalization.CultureInfo("es-CO")));
                text.Span(".");
            });
            
            // Firma
            column.Item().PaddingTop(50);
            column.Item().AlignCenter().Text("_______________________");
            column.Item().AlignCenter().Text("Representante Legal").FontSize(10);
            column.Item().AlignCenter().Text(_nombreEmpresa ?? "").FontSize(9);
        });
    }
    
    // ===== LISTADO DE EMPLEADOS =====
    public async Task<Result<byte[]>> GenerarListadoEmpleadosAsync(ListadoEmpleadosOptions? options = null)
    {
        // Similar pattern: obtener datos, crear documento, generar PDF
        // Incluir tabla con columnas: Código, Cédula, Nombre, Cargo, Departamento, Estado
    }
    
    // ===== REPORTE DE PERMISOS =====
    public async Task<Result<byte[]>> GenerarReportePermisosAsync(ReportePermisosOptions options)
    {
        // Generar reporte con tabla de permisos filtrados
        // Incluir resumen: total por tipo, total por estado, etc.
    }
    
    // Implementar resto de métodos...
}
```

---

## PLANTILLAS DE DOCUMENTOS:

### Acta de Permiso:
- Logo + Datos empresa
- Número de acta
- Datos del empleado
- Detalles del permiso (fechas, tipo, motivo)
- Estado (Aprobado/Rechazado)
- Espacio para firmas
- Fecha de generación

### Certificado Laboral:
- Membrete de empresa
- Título "CERTIFICADO LABORAL"
- "A QUIEN PUEDA INTERESAR"
- Datos del empleado (nombre, cédula, cargo)
- Fechas de vinculación
- Tipo de contrato
- Salario (opcional, según parámetro)
- Fecha y firma del representante legal

### Listado de Empleados:
- Título con filtros aplicados
- Tabla con empleados
- Resumen al final (total activos, inactivos)
- Fecha de generación

---

**IMPORTANTE:**
- Usar QuestPDF.Settings.License = LicenseType.Community
- Configurar fuentes legibles (Arial, Helvetica)
- Incluir logo si está configurado
- Paginar documentos largos
- Numerar páginas
```

---

## ✅ Checklist de Entregables

- [ ] Shared/Interfaces/IReportService.cs
- [ ] Infrastructure/Services/ReportService.cs
- [ ] Método: GenerarActaPermisoAsync
- [ ] Método: GenerarCertificadoLaboralAsync
- [ ] Método: GenerarListadoEmpleadosAsync
- [ ] Método: GenerarReportePermisosAsync
- [ ] Método: GenerarReporteVacacionesAsync
- [ ] Método: GenerarReporteAsistenciaAsync
- [ ] Registrar servicio en Program.cs

---

## 🔗 Dependencias NuGet

```xml
<PackageReference Include="QuestPDF" Version="2024.3.0" />
```

---

## 📄 Uso en Páginas

```csharp
// En Permisos.razor
@inject IReportService ReportService
@inject IJSRuntime JSRuntime

private async Task DescargarActa(int permisoId)
{
    var result = await ReportService.GenerarActaPermisoAsync(permisoId);
    
    if (result.Success)
    {
        // Descargar el PDF
        await JSRuntime.InvokeVoidAsync("downloadFile", 
            $"Acta-Permiso-{permisoId}.pdf", 
            "application/pdf", 
            Convert.ToBase64String(result.Data));
    }
    else
    {
        errorMessage = result.Message;
    }
}
```

```javascript
// En wwwroot/js/app.js
window.downloadFile = function(fileName, mimeType, base64Content) {
    const link = document.createElement('a');
    link.href = `data:${mimeType};base64,${base64Content}`;
    link.download = fileName;
    link.click();
};
```
