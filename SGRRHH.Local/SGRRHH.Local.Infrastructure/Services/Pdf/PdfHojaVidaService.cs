using System.Security.Cryptography;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Shared.Interfaces;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using System.Threading.Tasks;

namespace SGRRHH.Local.Infrastructure.Services.Pdf;

/// <summary>
/// Servicio para generación y procesamiento de PDFs de hoja de vida.
/// </summary>
public class PdfHojaVidaService : IPdfHojaVidaService
{
    private readonly ILogger<PdfHojaVidaService> _logger;
    private readonly string _webRootPath;
    private readonly IEmpleadoRepository _empleadoRepo;
    private readonly IAspiranteRepositorio _aspiranteRepo;
    private readonly IXmpMetadataHandler _xmpHandler;
    private readonly PdfFieldMapper _fieldMapper;
    
    // Configuración
    private const string NOMBRE_TEMPLATE = "hoja_vida_template.pdf";
    private const string CARPETA_TEMPLATES = "templates";
    
    // Departamentos de Colombia para los dropdowns
    private static readonly string[] DepartamentosColombia = new[]
    {
        "Amazonas", "Antioquia", "Arauca", "Atlántico", "Bogotá D.C.", "Bolívar",
        "Boyacá", "Caldas", "Caquetá", "Casanare", "Cauca", "Cesar", "Chocó",
        "Córdoba", "Cundinamarca", "Guainía", "Guaviare", "Huila", "La Guajira",
        "Magdalena", "Meta", "Nariño", "Norte de Santander", "Putumayo", "Quindío",
        "Risaralda", "San Andrés y Providencia", "Santander", "Sucre", "Tolima",
        "Valle del Cauca", "Vaupés", "Vichada"
    };
    
    private static readonly string[] NivelesEducacion = new[]
    {
        "Primaria", "Secundaria", "Técnico", "Tecnológico", "Profesional", "Especialización", "Maestría", "Doctorado"
    };
    
    private static readonly string[] EstadosCiviles = new[]
    {
        "Soltero(a)", "Casado(a)", "Unión Libre", "Divorciado(a)", "Viudo(a)"
    };
    
    private static readonly string[] TallasCasco = new[]
    {
        "XS", "S", "M", "L", "XL", "XXL"
    };
    
    public PdfHojaVidaService(
        ILogger<PdfHojaVidaService> logger,
        IConfiguration configuration,
        IEmpleadoRepository empleadoRepo,
        IAspiranteRepositorio aspiranteRepo,
        IXmpMetadataHandler xmpHandler,
        PdfFieldMapper fieldMapper)
    {
        _logger = logger;
        // Obtener WebRootPath de configuración o usar ruta por defecto
        _webRootPath = configuration["WebRootPath"] ?? 
                       System.IO.Path.Combine(AppContext.BaseDirectory, "wwwroot");
        _empleadoRepo = empleadoRepo;
        _aspiranteRepo = aspiranteRepo;
        _xmpHandler = xmpHandler;
        _fieldMapper = fieldMapper;
    }
    
    private string TemplatePath => System.IO.Path.Combine(_webRootPath, CARPETA_TEMPLATES, NOMBRE_TEMPLATE);
    
    /// <summary>
    /// Genera un PDF vacío con el formulario de hoja de vida.
    /// </summary>
    public async Task<byte[]> GenerarPdfVacioAsync()
    {
        _logger.LogInformation("Generando PDF vacío de hoja de vida");
        
        await AsegurarTemplateExisteAsync();
        
        using var memoryStream = new MemoryStream();
        
        // Copiar template y agregar metadatos
        using (var reader = new PdfReader(TemplatePath))
        using (var writer = new PdfWriter(memoryStream))
        {
            writer.SetCloseStream(false);
            using (var pdfDoc = new PdfDocument(reader, writer))
        {
            // Agregar metadatos Forestech
            _xmpHandler.EscribirMetadatos(pdfDoc, new Dictionary<string, string>
            {
                ["tipo"] = "vacio",
                ["proposito"] = "aspirante_nuevo"
            });
        }
        }
        
        _logger.LogInformation("PDF vacío generado exitosamente ({Size} bytes)", memoryStream.Length);
        return memoryStream.ToArray();
    }
    
    /// <summary>
    /// Genera un PDF prellenado con los datos del empleado.
    /// </summary>
    public async Task<byte[]> GenerarPdfEmpleadoAsync(int empleadoId)
    {
        _logger.LogInformation("Generando PDF para empleado {EmpleadoId}", empleadoId);
        
        var empleado = await _empleadoRepo.GetByIdAsync(empleadoId);
        if (empleado == null)
        {
            throw new InvalidOperationException($"Empleado con ID {empleadoId} no encontrado");
        }
        
        await AsegurarTemplateExisteAsync();
        
        using var memoryStream = new MemoryStream();
        
        using (var reader = new PdfReader(TemplatePath))
        using (var writer = new PdfWriter(memoryStream))
        {
            writer.SetCloseStream(false);
            using (var pdfDoc = new PdfDocument(reader, writer))
        {
            // Obtener formulario y rellenar
            var form = PdfAcroForm.GetAcroForm(pdfDoc, false);
            if (form != null)
            {
                _fieldMapper.RellenarFormulario(form, empleado);
            }
            
            // Agregar metadatos
            _xmpHandler.EscribirMetadatos(pdfDoc, new Dictionary<string, string>
            {
                ["tipo"] = "empleado",
                ["empleadoId"] = empleadoId.ToString(),
                ["cedula"] = empleado.Cedula
            });
        }
        }
        
        _logger.LogInformation("PDF generado para empleado {Cedula} ({Size} bytes)", 
            empleado.Cedula, memoryStream.Length);
        return memoryStream.ToArray();
    }
    
    /// <summary>
    /// Genera un PDF prellenado con los datos del aspirante.
    /// </summary>
    public async Task<byte[]> GenerarPdfAspiranteAsync(int aspiranteId)
    {
        _logger.LogInformation("Generando PDF para aspirante {AspiranteId}", aspiranteId);
        
        var aspirante = await _aspiranteRepo.ObtenerPorIdAsync(aspiranteId);
        if (aspirante == null)
        {
            throw new InvalidOperationException($"Aspirante con ID {aspiranteId} no encontrado");
        }
        
        await AsegurarTemplateExisteAsync();
        
        using var memoryStream = new MemoryStream();
        
        using (var reader = new PdfReader(TemplatePath))
        using (var writer = new PdfWriter(memoryStream))
        {
            writer.SetCloseStream(false);
            using (var pdfDoc = new PdfDocument(reader, writer))
        {
            // Obtener formulario y rellenar
            var form = PdfAcroForm.GetAcroForm(pdfDoc, false);
            if (form != null)
            {
                _fieldMapper.RellenarFormulario(form, aspirante);
            }
            
            // Agregar metadatos
            _xmpHandler.EscribirMetadatos(pdfDoc, new Dictionary<string, string>
            {
                ["tipo"] = "aspirante",
                ["aspiranteId"] = aspiranteId.ToString(),
                ["cedula"] = aspirante.Cedula
            });
        }
        }
        
        _logger.LogInformation("PDF generado para aspirante {Cedula} ({Size} bytes)", 
            aspirante.Cedula, memoryStream.Length);
        return memoryStream.ToArray();
    }
    
    /// <summary>
    /// Procesa un PDF subido y extrae los datos del formulario.
    /// </summary>
    public async Task<ResultadoParseo> ProcesarPdfAsync(Stream pdfStream, string nombreArchivo)
    {
        _logger.LogInformation("Procesando PDF: {NombreArchivo}", nombreArchivo);
        
        try
        {
            // Copiar stream a memoria para poder procesarlo
            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            // Calcular hash del contenido
            var hash = CalcularHash(memoryStream.ToArray());
            memoryStream.Position = 0;
            
            using var reader = new PdfReader(memoryStream);
            using var pdfDoc = new PdfDocument(reader);
            
            // Verificar si es formato Forestech
            var esForestec = _xmpHandler.EsFormatoForestec(pdfDoc);
            
            // Verificar firma digital
            var tieneFirma = VerificarFirma(pdfDoc);
            
            // Obtener formulario
            var form = PdfAcroForm.GetAcroForm(pdfDoc, false);
            
            if (form == null)
            {
                return ResultadoParseo.Error(
                    "El PDF no contiene un formulario AcroForm editable",
                    "El archivo PDF debe ser un formulario con campos editables");
            }
            
            // Extraer datos
            var datos = _fieldMapper.ExtraerDatos(form);
            
            // Validar campos requeridos
            var errores = ValidarDatosRequeridos(datos);
            
            if (errores.Any())
            {
                var resultado = ResultadoParseo.Error("Faltan campos requeridos en el formulario");
                resultado.Errores = errores;
                resultado.EsFormatoForestec = esForestec;
                resultado.Datos = datos; // Incluir datos parciales
                return resultado;
            }
            
            var resultadoExitoso = ResultadoParseo.Exito(datos, esForestec, tieneFirma);
            resultadoExitoso.HashContenido = hash;
            
            // Advertencia si no tiene firma
            if (!tieneFirma)
            {
                resultadoExitoso.Advertencias.Add("El documento no tiene firma digital");
            }
            
            _logger.LogInformation("PDF procesado exitosamente. Forestech: {EsForestec}, Firma: {TieneFirma}", 
                esForestec, tieneFirma);
            
            return resultadoExitoso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar PDF: {NombreArchivo}", nombreArchivo);
            return ResultadoParseo.Error("Error al procesar el archivo PDF", ex.Message);
        }
    }
    
    /// <summary>
    /// Verifica si un PDF es formato Forestech.
    /// </summary>
    public async Task<bool> EsFormatoForestechAsync(Stream pdfStream)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            using var reader = new PdfReader(memoryStream);
            using var pdfDoc = new PdfDocument(reader);
            
            return _xmpHandler.EsFormatoForestec(pdfDoc);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar formato Forestech del PDF");
            return false;
        }
    }
    
    // ========== MÉTODOS PRIVADOS ==========
    
    /// <summary>
    /// Asegura que el template PDF existe, generándolo si es necesario.
    /// </summary>
    private async Task AsegurarTemplateExisteAsync()
    {
        var templateDir = System.IO.Path.GetDirectoryName(TemplatePath)!;
        
        if (!Directory.Exists(templateDir))
        {
            Directory.CreateDirectory(templateDir);
        }
        
        if (!File.Exists(TemplatePath))
        {
            _logger.LogInformation("Template PDF no existe, generando...");
            await GenerarTemplatePdfAsync();
        }
    }
    
    /// <summary>
    /// Genera el template PDF con todos los campos AcroForm y etiquetas visuales.
    /// </summary>
    private async Task GenerarTemplatePdfAsync()
    {
        using var writer = new PdfWriter(TemplatePath);
        using var pdfDoc = new PdfDocument(writer);
        using var doc = new Document(pdfDoc, PageSize.LETTER);
        
        // Crear formulario AcroForm
        var form = PdfAcroForm.GetAcroForm(pdfDoc, true);
        
        // Estilos
        var fontTitulo = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var fontLabel = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        
        // Layout constants
        float margenIzq = 40;
        float margenDer = 40;
        float anchoPagina = PageSize.LETTER.GetWidth();
        float anchoUtil = anchoPagina - margenIzq - margenDer;
        
        // Filas: 2 columnas iguales
        float col2Ancho = (anchoUtil - 20) / 2;
        float col2X = margenIzq + col2Ancho + 20;
        
        // Filas: 3 columnas iguales
        float col3Ancho = (anchoUtil - 40) / 3;
        float col3X2 = margenIzq + col3Ancho + 20;
        float col3X3 = col3X2 + col3Ancho + 20;

        float y = 750; // Inicio Y
        
        // ========== PÁGINA 1: DATOS PERSONALES ==========
        var pagina1 = pdfDoc.AddNewPage(PageSize.LETTER);
        int pageNum1 = 1;
        
        // Título Principal
        doc.Add(new Paragraph("HOJA DE VIDA - FORESTECH")
            .SetFont(fontTitulo)
            .SetFontSize(16)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFixedPosition(pageNum1, margenIzq, y, anchoUtil));
            
        y -= 20;
        doc.Add(new Paragraph("Complete todos los campos requeridos (*)")
            .SetFontSize(9)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontColor(ColorConstants.DARK_GRAY)
            .SetFixedPosition(pageNum1, margenIzq, y, anchoUtil));
            
        y -= 40;
        
        // --- SECCIÓN: DATOS PERSONALES ---
        DibujarSeccion(doc, pageNum1, "DATOS PERSONALES", y, margenIzq, anchoUtil, fontTitulo);
        y -= 30; // Espacio para la cabecera
        
        // Fila 1: Nombres y Apellidos
        AgregarCampoTexto(form, doc, pageNum1, "Nombres", "Nombres (*)", margenIzq, y, col2Ancho, true);
        AgregarCampoTexto(form, doc, pageNum1, "Apellidos", "Apellidos (*)", col2X, y, col2Ancho, true);
        y -= 50;
        
        // Fila 2: Cédula, Fecha Nac, Género
        AgregarCampoTexto(form, doc, pageNum1, "Cedula", "Cédula (*)", margenIzq, y, col3Ancho, true);
        AgregarCampoTexto(form, doc, pageNum1, "FechaNacimiento", "Fecha de Nacimiento (DD/MM/AAAA) (*)", col3X2, y, col3Ancho, true);
        AgregarCampoCombo(form, doc, pageNum1, "Genero", "Género", col3X3, y, col3Ancho, new[] { "Masculino", "Femenino" });
        y -= 50;
        
        // Fila 3: Estado Civil, Teléfono, Email
        AgregarCampoCombo(form, doc, pageNum1, "EstadoCivil", "Estado Civil", margenIzq, y, col3Ancho, EstadosCiviles);
        AgregarCampoTexto(form, doc, pageNum1, "Telefono", "Teléfono (*)", col3X2, y, col3Ancho, true);
        AgregarCampoTexto(form, doc, pageNum1, "Email", "Correo Electrónico", col3X3, y, col3Ancho, false);
        y -= 50;

        // Fila 4: Dirección Completa (Ciudad y Depto)
        AgregarCampoTexto(form, doc, pageNum1, "Direccion", "Dirección de Residencia (*)", margenIzq, y, col2Ancho, true);
        float anchoCuarto = (col2Ancho - 10) / 2;
        AgregarCampoTexto(form, doc, pageNum1, "Ciudad", "Ciudad (*)", col2X, y, anchoCuarto, true);
        AgregarCampoCombo(form, doc, pageNum1, "Departamento", "Departamento", col2X + anchoCuarto + 10, y, anchoCuarto, DepartamentosColombia);
        y -= 60;

        // --- SECCIÓN: TALLAS EPP ---
        DibujarSeccion(doc, pageNum1, "DOTACIÓN Y EPP", y, margenIzq, anchoUtil, fontTitulo);
        y -= 30;
        
        AgregarCampoCombo(form, doc, pageNum1, "TallaCasco", "Talla de Casco", margenIzq, y, col3Ancho, TallasCasco);
        AgregarCampoTexto(form, doc, pageNum1, "TallaBotas", "Talla de Botas", col3X2, y, col3Ancho, false);
        
        // ========== PÁGINA 2: FORMACIÓN ACADÉMICA ==========
        var pagina2 = pdfDoc.AddNewPage(PageSize.LETTER);
        int pageNum2 = 2;
        y = 750;
        
        DibujarSeccion(doc, pageNum2, "FORMACIÓN ACADÉMICA", y, margenIzq, anchoUtil, fontTitulo);
        y -= 30;
        
        for (int i = 1; i <= 3; i++)
        {
            // Subtítulo numérico
            doc.Add(new Paragraph($"Formación {i}")
                .SetFont(fontTitulo).SetFontSize(10)
                .SetFixedPosition(pageNum2, margenIzq, y, anchoUtil));
            y -= 20;

            // Fila 1: Nivel y Título
            AgregarCampoCombo(form, doc, pageNum2, $"Form{i}_Nivel", "Nivel Educativo", margenIzq, y, col3Ancho, NivelesEducacion);
            AgregarCampoTexto(form, doc, pageNum2, $"Form{i}_Titulo", "Título Obtendo", col3X2, y, col3Ancho * 2 + 20, false);
            y -= 50;

            // Fila 2: Institución y Fechas
            AgregarCampoTexto(form, doc, pageNum2, $"Form{i}_Institucion", "Institución Educativa", margenIzq, y, col2Ancho, false);
            
            float smallW = 70;
            float datesX = col2X;
            AgregarCampoTexto(form, doc, pageNum2, $"Form{i}_FechaInicio", "Inicio", datesX, y, smallW, false);
            AgregarCampoTexto(form, doc, pageNum2, $"Form{i}_FechaFin", "Fin", datesX + smallW + 10, y, smallW, false);
            
            AgregarCampoCheckbox(form, doc, pageNum2, $"Form{i}_EnCurso", "En Curso", datesX + (smallW * 2) + 30, y);
            
            y -= 50;
            
            // Separador visual
            if (i < 3)
            {
                new Canvas(new PdfCanvas(pagina2), new Rectangle(margenIzq, y + 25, anchoUtil, 1))
                    .Add(new LineSeparator(new SolidLine(0.5f)).SetFontColor(ColorConstants.LIGHT_GRAY));
                y -= 20;
            }
        }
        
        // ========== PÁGINA 3: EXPERIENCIA LABORAL ==========
        var pagina3 = pdfDoc.AddNewPage(PageSize.LETTER);
        int pageNum3 = 3;
        y = 750;
        
        DibujarSeccion(doc, pageNum3, "EXPERIENCIA LABORAL", y, margenIzq, anchoUtil, fontTitulo);
        y -= 30;
        
        for (int i = 1; i <= 3; i++)
        {
            doc.Add(new Paragraph($"Experiencia {i}")
                .SetFont(fontTitulo).SetFontSize(10)
                .SetFixedPosition(pageNum3, margenIzq, y, anchoUtil));
            y -= 20;

            // Fila 1: Empresa y Cargo
            AgregarCampoTexto(form, doc, pageNum3, $"Exp{i}_Empresa", "Empresa / Empleador", margenIzq, y, col2Ancho, false);
            AgregarCampoTexto(form, doc, pageNum3, $"Exp{i}_Cargo", "Cargo Ocupado", col2X, y, col2Ancho, false);
            y -= 50;

            // Fila 2: Fechas y Checkbox
            float smallW = 80;
            AgregarCampoTexto(form, doc, pageNum3, $"Exp{i}_FechaInicio", "Fecha Inicio", margenIzq, y, smallW, false);
            AgregarCampoTexto(form, doc, pageNum3, $"Exp{i}_FechaFin", "Fecha Fin", margenIzq + smallW + 15, y, smallW, false);
            AgregarCampoCheckbox(form, doc, pageNum3, $"Exp{i}_TrabajoActual", "Trabajo Actual", margenIzq + (smallW * 2) + 40, y);
            
            // Motivo Retiro al lado
            AgregarCampoTexto(form, doc, pageNum3, $"Exp{i}_MotivoRetiro", "Motivo de Retiro", col2X, y, col2Ancho, false);
            y -= 50;

            // Fila 3: Funciones (TextArea)
            AgregarCampoTextArea(form, doc, pageNum3, $"Exp{i}_Funciones", "Funciones y Logros Principales", margenIzq, y, anchoUtil, 50);
            y -= 80;

             // Separador
            if (i < 3)
            {
                new Canvas(new PdfCanvas(pagina3), new Rectangle(margenIzq, y + 25, anchoUtil, 1))
                    .Add(new LineSeparator(new SolidLine(0.5f)).SetFontColor(ColorConstants.LIGHT_GRAY));
                y -= 20;
            }
        }

        // ========== PÁGINA 4: REFERENCIAS Y FIRMA ==========
        var pagina4 = pdfDoc.AddNewPage(PageSize.LETTER);
        int pageNum4 = 4;
        y = 750;
        
        DibujarSeccion(doc, pageNum4, "REFERENCIAS", y, margenIzq, anchoUtil, fontTitulo);
        y -= 30;
        
        // Grid 2x2 para referencias
        for (int i = 1; i <= 4; i++)
        {
            var tipo = i <= 2 ? "Familiar/Personal" : "Laboral";
            float xBase = (i % 2 != 0) ? margenIzq : col2X; // Izquierda o Derecha
            
            // Si es nueva fila (impar), bajar Y, salvo el primero
            if (i == 3) y -= 120;
            
            float currentY = (i % 2 != 0) ? y : y; // Mantener Y para pares

            doc.Add(new Paragraph($"Referencia {i} ({tipo})")
                .SetFont(fontTitulo).SetFontSize(9)
                .SetFixedPosition(pageNum4, xBase, currentY, col2Ancho));
                
            float innerY = currentY - 20;
            
            AgregarCampoTexto(form, doc, pageNum4, $"Ref{i}_Nombre", "Nombre Completo", xBase, innerY, col2Ancho, false);
            innerY -= 45;
            
            float halfCol = (col2Ancho - 10) / 2;
            AgregarCampoTexto(form, doc, pageNum4, $"Ref{i}_Telefono", "Teléfono", xBase, innerY, halfCol, false);
            AgregarCampoTexto(form, doc, pageNum4, $"Ref{i}_Relacion", "Relación/Cargo", xBase + halfCol + 10, innerY, halfCol, false);
            
            // Campo oculto para tipo
            var tipoField = new TextFormFieldBuilder(pdfDoc, $"Ref{i}_Tipo").CreateText();
            tipoField.SetValue(i <= 2 ? "Personal" : "Laboral");
            // No lo agregamos con widget visible, solo al form
            form.AddField(tipoField);
        }
        
        y -= 140; // Bajar después de referencias

        // --- FIRMA ---
        DibujarSeccion(doc, pageNum4, "DECLARACIÓN Y FIRMA", y, margenIzq, anchoUtil, fontTitulo);
        y -= 40;
        
        doc.Add(new Paragraph("Declaro bajo gravedad de juramento que la información aquí suministrada es veraz y verificable.")
            .SetFontSize(10)
            .SetFixedPosition(pageNum4, margenIzq, y, anchoUtil));
            
        y -= 50;
        
        // Caja de firma
        var sigRect = new Rectangle(margenIzq, y - 60, col2Ancho, 60);
        
        // Dibujar línea de firma visual
        new PdfCanvas(pagina4)
            .MoveTo(margenIzq, y - 60)
            .LineTo(margenIzq + col2Ancho, y - 60)
            .Stroke();
            
        doc.Add(new Paragraph("Firma del Aspirante/Empleado")
            .SetFontSize(8)
            .SetFixedPosition(pageNum4, margenIzq, y - 75, col2Ancho));
            
        var sigField = new TextFormFieldBuilder(pdfDoc, "Sig")
            .SetWidgetRectangle(sigRect)
            .CreateText();
        sigField.SetValue("");
        form.AddField(sigField);
        
        // Fecha al lado
        AgregarCampoTexto(form, doc, pageNum4, "FechaFirma", "Fecha de Diligenciamiento", col2X, y - 20, 150, false);
        
        // Metadatos
        _xmpHandler.EscribirMetadatos(pdfDoc, new Dictionary<string, string>
        {
            ["tipo"] = "template",
            ["version"] = "2.0"
        });
        
        _logger.LogInformation("Template PDF v2 generado en {Path}", TemplatePath);
        
        await Task.CompletedTask;
    }
    
    // ========== HELPERS DE DISEÑO ==========
    
    private void DibujarSeccion(Document doc, int pageNum, string titulo, float y, float x, float ancho, PdfFont font)
    {
        // Fondo gris suave para el encabezado
        // Nota: canvas directo es más fácil para rectángulos puros
        // Pero usamos Paragraph con background si es simple
        
        doc.Add(new Paragraph(titulo)
            .SetFont(font)
            .SetFontSize(11)
            .SetFontColor(ColorConstants.WHITE)
            .SetBackgroundColor(ColorConstants.DARK_GRAY)
            .SetPaddingLeft(5)
            .SetPaddingTop(2)
            .SetPaddingBottom(2)
            .SetFixedPosition(pageNum, x, y, ancho));
    }
    
    private void AgregarCampoTexto(PdfAcroForm form, Document doc, int pageNum, string nombre, string etiqueta, float x, float y, float ancho, bool requerido)
    {
        // 1. Etiqueta Visual
        doc.Add(new Paragraph(etiqueta)
            .SetFontSize(8)
            .SetFontColor(ColorConstants.DARK_GRAY)
            .SetFixedPosition(pageNum, x, y + 22, ancho));
            
        // 2. Campo de Formulario
        var rect = new Rectangle(x, y, ancho, 20);
        var field = new TextFormFieldBuilder(doc.GetPdfDocument(), nombre)
            .SetWidgetRectangle(rect)
            .CreateText();
            
        if (requerido) field.SetRequired(true);
        field.SetValue("");
        
        // Fondo azul claro para indicar campo editable (opcional, pero ayuda UX)
        // field.SetBackgroundColor(new DeviceRgb(240, 245, 255)); 
        
        form.AddField(field);
    }
    
    private void AgregarCampoTextArea(PdfAcroForm form, Document doc, int pageNum, string nombre, string etiqueta, float x, float y, float ancho, float alto)
    {
        // 1. Etiqueta Visual
        doc.Add(new Paragraph(etiqueta)
            .SetFontSize(8)
            .SetFontColor(ColorConstants.DARK_GRAY)
            .SetFixedPosition(pageNum, x, y + 2, ancho)); // Justo encima del rect, que está abajo
            
        // 2. Campo
        var rect = new Rectangle(x, y - alto, ancho, alto);
        var field = new TextFormFieldBuilder(doc.GetPdfDocument(), nombre)
            .SetWidgetRectangle(rect)
            .CreateMultilineText();
            
        field.SetValue("");
        form.AddField(field);
    }
    
    private void AgregarCampoCombo(PdfAcroForm form, Document doc, int pageNum, string nombre, string etiqueta, float x, float y, float ancho, string[] opciones)
    {
        doc.Add(new Paragraph(etiqueta)
            .SetFontSize(8)
            .SetFontColor(ColorConstants.DARK_GRAY)
            .SetFixedPosition(pageNum, x, y + 22, ancho));
            
        var rect = new Rectangle(x, y, ancho, 20);
        var field = new ChoiceFormFieldBuilder(doc.GetPdfDocument(), nombre)
            .SetWidgetRectangle(rect)
            .SetOptions(opciones)
            .CreateComboBox();
            
        field.SetValue("");
        form.AddField(field);
    }
    
    private void AgregarCampoCheckbox(PdfAcroForm form, Document doc, int pageNum, string nombre, string etiqueta, float x, float y)
    {
        var rect = new Rectangle(x, y + 3, 14, 14); // Ajuste fino para alinear con texto
        var field = new CheckBoxFormFieldBuilder(doc.GetPdfDocument(), nombre)
            .SetWidgetRectangle(rect)
            .CreateCheckBox();
            
        field.SetValue("Off");
        form.AddField(field);
        
        // Etiqueta al lado derecho
        doc.Add(new Paragraph(etiqueta)
            .SetFontSize(9)
            .SetFontColor(ColorConstants.BLACK)
            .SetFixedPosition(pageNum, x + 20, y + 4, 150));
    }
    
    private bool VerificarFirma(PdfDocument pdfDoc)
    {
        try
        {
            var form = PdfAcroForm.GetAcroForm(pdfDoc, false);
            if (form == null) return false;
            
            var sigField = form.GetField("Sig");
            if (sigField == null) return false;
            
            var valor = sigField.GetValueAsString();
            return !string.IsNullOrWhiteSpace(valor);
        }
        catch
        {
            return false;
        }
    }
    
    private List<string> ValidarDatosRequeridos(DatosHojaVida datos)
    {
        var errores = new List<string>();
        
        if (string.IsNullOrWhiteSpace(datos.Nombres))
            errores.Add("El campo 'Nombres' es requerido");
        
        if (string.IsNullOrWhiteSpace(datos.Apellidos))
            errores.Add("El campo 'Apellidos' es requerido");
        
        if (string.IsNullOrWhiteSpace(datos.Cedula))
            errores.Add("El campo 'Cédula' es requerido");
        
        if (!datos.FechaNacimiento.HasValue)
            errores.Add("El campo 'Fecha de Nacimiento' es requerido");
        
        if (string.IsNullOrWhiteSpace(datos.Telefono))
            errores.Add("El campo 'Teléfono' es requerido");
        
        if (string.IsNullOrWhiteSpace(datos.Direccion))
            errores.Add("El campo 'Dirección' es requerido");
        
        if (string.IsNullOrWhiteSpace(datos.Ciudad))
            errores.Add("El campo 'Ciudad' es requerido");
        
        return errores;
    }
    
    private string CalcularHash(byte[] contenido)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(contenido);
        return Convert.ToBase64String(hashBytes);
    }
}
