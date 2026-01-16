using System;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Server.Components.Shared;

public partial class ScannerModal : ComponentBase, IDisposable
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    /// <summary>
    /// Se dispara cuando el usuario confirma las páginas escaneadas (modo imágenes).
    /// Retorna la lista de páginas escaneadas.
    /// </summary>
    [Parameter] public EventCallback<List<ScannedPageDto>> OnScanComplete { get; set; }

    /// <summary>
    /// Se dispara cuando el usuario genera un PDF combinado.
    /// Retorna los bytes del PDF.
    /// </summary>
    [Parameter] public EventCallback<byte[]> OnPdfGenerated { get; set; }

    /// <summary>
    /// Se dispara cuando se cancela el modal sin confirmar.
    /// </summary>
    [Parameter] public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Permite escaneo de múltiples páginas (batch).
    /// Si es false, solo escanea una página y cierra automáticamente.
    /// </summary>
    [Parameter] public bool AllowMultiplePages { get; set; } = false;

    /// <summary>
    /// Texto personalizado para el título del modal.
    /// </summary>
    [Parameter] public string? Titulo { get; set; }

    [Inject] private IScannerService ScannerService { get; set; } = default!;
    [Inject] private IImageTransformationService ImageTransform { get; set; } = default!;
    [Inject] private IOcrService ocrService { get; set; } = default!;
    [Inject] private IScannerModalStateService State { get; set; } = default!;
    [Inject] private IScannerWorkflowService Workflow { get; set; } = default!;
    [Inject] private IScanProfileRepository ProfileRepository { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    // Estado base desde servicio
    private List<ScannerDeviceDto> scanners => State.Scanners;
    private List<ScannedPageDto> scannedPages => State.ScannedPages;
    private ScanOptionsDto scanOptions => State.ScanOptions;
    private string? selectedDeviceId
    {
        get => State.SelectedDeviceId;
        set => State.SelectedDeviceId = value;
    }

    private string outputFormat
    {
        get => State.OutputFormat;
        set => State.OutputFormat = value;
    }

    private string ocrLanguage
    {
        get => State.OcrLanguage;
        set => State.OcrLanguage = value;
    }

    private List<ScanProfileDto> profiles => State.Profiles;
    private int selectedProfileId
    {
        get => State.SelectedProfileId ?? 0;
        set => State.SelectedProfileId = value;
    }

    private ImageCorrectionDto imageCorrection => State.ImageCorrection;

    private bool isScanning
    {
        get => State.IsScanning;
        set => State.IsScanning = value;
    }

    private bool isGeneratingPdf
    {
        get => State.IsGeneratingPdf;
        set => State.IsGeneratingPdf = value;
    }

    private bool isLoadingScanners
    {
        get => State.IsLoadingScanners;
        set => State.IsLoadingScanners = value;
    }

    private int previewIndex
    {
        get => State.PreviewIndex;
        set => State.PreviewIndex = value;
    }

    private int currentPage
    {
        get => State.CurrentPage;
        set => State.CurrentPage = value;
    }

    private string? errorMessage
    {
        get => State.ErrorMessage;
        set => State.ErrorMessage = value;
    }

    private string? successMessage
    {
        get => State.SuccessMessage;
        set => State.SuccessMessage = value;
    }

    // Propiedades de binding para los selectores (sincronizadas con scanOptions)
    private int selectedDpi
    {
        get => scanOptions.Dpi;
        set
        {
            scanOptions.Dpi = value;
            Console.WriteLine($"[UI] DPI cambiado a: {value}");
        }
    }

    private string selectedColorMode
    {
        get => scanOptions.ColorMode.ToString();
        set
        {
            if (Enum.TryParse<ScanColorMode>(value, out var mode))
            {
                scanOptions.ColorMode = mode;
                Console.WriteLine($"[UI] ColorMode cambiado a: {mode}");
            }
        }
    }

    private string selectedSource
    {
        get => scanOptions.Source.ToString();
        set
        {
            if (Enum.TryParse<ScanSource>(value, out var source))
            {
                scanOptions.Source = source;
                Console.WriteLine($"[UI] Source cambiado a: {source}");
            }
        }
    }

    // Perfiles de escaneo
    private bool showSaveProfileDialog = false;
    private string newProfileName = string.Empty;
    private string newProfileDescription = string.Empty;
    private bool newProfileIsDefault = false;

    // Vista previa y selección de área
    private bool isPreviewMode = false;
    private ScannedDocumentDto? previewImage = null;
    private ScanAreaDto? selectedArea = null;
    private bool isSelectingArea = false;
    private (double X, double Y) selectionStart = (0, 0);
    private (double X, double Y) selectionCurrent = (0, 0);

    // Corrección de imagen
    private bool showCorrectionPanel = false;
    private bool isPreviewingCorrections = false;
    private byte[]? correctionPreviewBytes = null;

    // Gamma usa slider entero (10-300) mapeado a float (0.1-3.0)
    private int gammaSliderValue
    {
        get => (int)(imageCorrection.Gamma * 100);
        set => imageCorrection.Gamma = value / 100f;
    }

    // Nitidez como selector (0=off, 1=on)
    private int sharpnessMode
    {
        get => imageCorrection.Sharpness > 0 ? 1 : 0;
        set => imageCorrection.Sharpness = value == 1 ? 50 : 0;
    }

    // Control de zoom de vista previa (0 = ajustar, 50 = 50%, 100 = 100%)
    private int previewZoom = 0;

    // Progreso de escaneo
    private double progressPercent = 0;
    private string progressStatus = string.Empty;

    // Popup de pantalla completa
    private bool showFullscreenPreview = false;
    private string? fullscreenImageUrl = null;
    private string? fullscreenImageInfo = null;
    private string? fullscreenPageInfo = null;
    private int fullscreenCurrentIndex = 0;
    private int fullscreenTotalPages = 1;

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && scanners.Count == 0 && !isLoadingScanners)
        {
            await RefreshScanners();
            await RefreshProfiles();
        }
    }

    protected override void OnInitialized()
    {
        ScannerService.ScanProgress += OnScanProgress;
        State.StateChanged += OnStateChanged;
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnScanProgress(object? sender, ScanProgressEventArgs e)
    {
        InvokeAsync(() =>
        {
            currentPage = e.CurrentPage;
            progressPercent = e.Progress;
            progressStatus = e.Status;

            if (e.HasError)
            {
                errorMessage = e.ErrorMessage;
            }

            StateHasChanged();
        });
    }

    private async Task RefreshScanners()
    {
        isLoadingScanners = true;
        errorMessage = null;

        try
        {
            var result = await Workflow.RefreshScannersAsync();
            if (result.IsSuccess && result.Value != null)
            {
                scanners.Clear();
                scanners.AddRange(result.Value);

                if (scanners.Count > 0 && string.IsNullOrEmpty(selectedDeviceId))
                {
                    selectedDeviceId = scanners[0].Id;
                }

                if (scanners.Count == 0)
                {
                    errorMessage = "No se encontraron escáneres. Verifique que el escáner esté conectado y encendido.";
                }
            }
            else
            {
                errorMessage = result.Error ?? "Error al buscar escáneres";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isLoadingScanners = false;
            StateHasChanged();
        }
    }

    private async Task StartScan()
    {
        if (isScanning)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedDeviceId))
        {
            errorMessage = "Seleccione un escáner antes de continuar.";
            return;
        }

        isScanning = true;
        errorMessage = null;
        successMessage = null;

        try
        {
            scanOptions.DeviceId = selectedDeviceId;
            Console.WriteLine("[SCAN-UI] ========== INICIANDO ESCANEO ==========");
            Console.WriteLine($"[SCAN-UI] DeviceId: {scanOptions.DeviceId}");
            Console.WriteLine($"[SCAN-UI] DPI: {scanOptions.Dpi}");
            Console.WriteLine($"[SCAN-UI] ColorMode: {scanOptions.ColorMode}");
            Console.WriteLine($"[SCAN-UI] Source: {scanOptions.Source}");
            Console.WriteLine($"[SCAN-UI] PageSize: {scanOptions.PageSize}");
            Console.WriteLine("[SCAN-UI] =========================================");

            var result = await Workflow.ScanSinglePageAsync(selectedDeviceId, scanOptions, imageCorrection);
            if (result.IsSuccess && result.Value != null)
            {
                var page = result.Value;
                page.PageNumber = scannedPages.Count + 1;
                State.AddPage(page);
                previewIndex = scannedPages.Count - 1;

                if (AllowMultiplePages)
                {
                    successMessage = $"Página {scannedPages.Count} escaneada. Puede continuar escaneando más páginas.";
                }
                else
                {
                    successMessage = "Página escaneada correctamente";
                }
            }
            else
            {
                errorMessage = result.Error ?? "Error al escanear";
            }
        }
        catch (OperationCanceledException)
        {
            successMessage = "Escaneo cancelado";
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
            Console.WriteLine($"[SCAN-UI] ERROR: {ex}");
        }
        finally
        {
            isScanning = false;
            StateHasChanged();
        }
    }

    private void CancelScan()
    {
        ScannerService.CancelCurrentScan();
    }

    private async Task ConfirmScan()
    {
        if (scannedPages.Count == 0)
        {
            return;
        }

        isGeneratingPdf = true;
        errorMessage = null;
        progressStatus = "Generando PDF...";
        StateHasChanged();

        try
        {
            RenumberPages();
            var pdfResult = await Workflow.GeneratePdfAsync(scannedPages);

            if (pdfResult.IsSuccess && pdfResult.Value != null && pdfResult.Value.Length > 0)
            {
                Console.WriteLine($"[SCAN] PDF generado exitosamente: {pdfResult.Value.Length} bytes ({scannedPages.Count} página(s))");
                await OnPdfGenerated.InvokeAsync(pdfResult.Value);
                await Cerrar();
            }
            else
            {
                errorMessage = pdfResult.Error ?? "Error al generar el PDF";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error generando PDF: {ex.Message}";
            Console.WriteLine($"[SCAN] ERROR generando PDF: {ex}");
        }
        finally
        {
            isGeneratingPdf = false;
            StateHasChanged();
        }
    }

    private async Task ConfirmAsPdf()
    {
        if (scannedPages.Count == 0)
        {
            return;
        }

        isGeneratingPdf = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            var pdfResult = await Workflow.GeneratePdfAsync(scannedPages);
            if (pdfResult.IsSuccess && pdfResult.Value != null && pdfResult.Value.Length > 0)
            {
                await OnPdfGenerated.InvokeAsync(pdfResult.Value);
                await Cerrar();
            }
            else
            {
                errorMessage = pdfResult.Error ?? "Error al generar el PDF";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error generando PDF: {ex.Message}";
        }
        finally
        {
            isGeneratingPdf = false;
            StateHasChanged();
        }
    }

    private async Task ConfirmAsOcrPdf()
    {
        if (scannedPages.Count == 0)
        {
            return;
        }

        isGeneratingPdf = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            var result = await Workflow.GenerateOcrPdfAsync(scannedPages, ocrLanguage);
            if (result.IsSuccess && result.Value != null)
            {
                if (!ocrService.IsAvailable)
                {
                    successMessage = "PDF generado (sin OCR - función no habilitada)";
                }

                await OnPdfGenerated.InvokeAsync(result.Value);
                await Cerrar();
            }
            else
            {
                errorMessage = result.Error ?? "Error al generar el PDF buscable";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error generando PDF con OCR: {ex.Message}";
        }
        finally
        {
            isGeneratingPdf = false;
            StateHasChanged();
        }
    }

    private void SelectPage(int index)
    {
        if (index >= 0 && index < scannedPages.Count)
        {
            previewIndex = index;
        }
    }

    private void DeletePage(int index)
    {
        if (index < 0 || index >= scannedPages.Count)
        {
            return;
        }

        State.RemovePage(index);
        successMessage = $"Página eliminada. Quedan {scannedPages.Count} página(s).";
    }

    private void ClearPages()
    {
        State.ClearPages();
        previewIndex = 0;
        successMessage = null;
        errorMessage = null;
    }

    private void PreviousPage()
    {
        if (previewIndex > 0)
        {
            previewIndex--;
        }
    }

    private void NextPage()
    {
        if (previewIndex < scannedPages.Count - 1)
        {
            previewIndex++;
        }
    }

    private async Task OnBackdropClick()
    {
        if (!isScanning && !isGeneratingPdf)
        {
            await Cerrar();
        }
    }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (isScanning || isGeneratingPdf)
        {
            return;
        }

        switch (e.Key)
        {
            case "Escape":
                await Cerrar();
                break;
            case "F5":
                await StartScan();
                break;
            case "ArrowLeft":
                PreviousPage();
                break;
            case "ArrowRight":
                NextPage();
                break;
            case "Delete":
                if (scannedPages.Count > 0)
                {
                    DeletePage(previewIndex);
                }

                break;
        }
    }

    private async Task Cerrar()
    {
        if (isScanning || isGeneratingPdf)
        {
            return;
        }

        errorMessage = null;
        successMessage = null;
        ExitPreviewMode();
        showSaveProfileDialog = false;

        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
        await OnCancel.InvokeAsync();
    }

    private ScannedPageDto? GetCurrentPage()
    {
        if (previewIndex < 0 || previewIndex >= scannedPages.Count)
        {
            return null;
        }

        return scannedPages[previewIndex];
    }

    private string? GetCurrentPageImageUrl()
    {
        var page = GetCurrentPage();
        if (page == null)
        {
            return null;
        }

        return ImageTransform.ABase64(page.ImageBytes, page.MimeType);
    }

    private string? GetPreviewImageUrl()
    {
        if (previewImage == null)
        {
            return null;
        }

        return ImageTransform.ABase64(previewImage.ImageBytes, previewImage.MimeType);
    }

    private string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }

    #region Corrección y Transformación de Imagen

    private void ToggleCorrectionPanel() => showCorrectionPanel = !showCorrectionPanel;

    private void ResetCorrections()
    {
        imageCorrection.Brightness = 0;
        imageCorrection.Contrast = 0;
        imageCorrection.Gamma = 1.0f;
        imageCorrection.Sharpness = 0;
        imageCorrection.BlackWhiteThreshold = 128;
        correctionPreviewBytes = null;
        isPreviewingCorrections = false;
        StateHasChanged();
    }

    /// <summary>
    /// Genera una vista previa de las correcciones sin aplicarlas permanentemente
    /// </summary>
    private async Task PreviewCorrections()
    {
        if (previewIndex < 0 || previewIndex >= scannedPages.Count)
        {
            return;
        }

        if (!imageCorrection.HasCorrections)
        {
            return;
        }

        isPreviewingCorrections = true;
        StateHasChanged();

        try
        {
            var page = scannedPages[previewIndex];
            correctionPreviewBytes = await ImageTransform.AplicarCorreccionesAsync(page.ImageBytes, imageCorrection);

            var originalBytes = page.ImageBytes;
            page.ImageBytes = correctionPreviewBytes;
            StateHasChanged();

            await Task.Delay(2000);
            page.ImageBytes = originalBytes;
            correctionPreviewBytes = null;

            successMessage = "Vista previa terminada. Presione APLICAR para confirmar.";
        }
        catch (Exception ex)
        {
            errorMessage = $"Error generando vista previa: {ex.Message}";
        }
        finally
        {
            isPreviewingCorrections = false;
            StateHasChanged();
        }
    }

    private async Task ApplyCorrectionsToCurrentPage()
    {
        if (previewIndex < 0 || previewIndex >= scannedPages.Count)
        {
            return;
        }

        if (!imageCorrection.HasCorrections)
        {
            return;
        }

        var page = scannedPages[previewIndex];
        var result = await Workflow.ApplyCorrectionsAsync(page.ImageBytes, imageCorrection);
        if (result.IsSuccess && result.Value != null)
        {
            page.ImageBytes = result.Value;
            UpdatePageDimensions(page);
            successMessage = "Correcciones aplicadas a la página";
            ResetCorrections();
        }
        else
        {
            errorMessage = result.Error ?? "Error aplicando correcciones";
        }

        StateHasChanged();
    }

    private async Task RotatePage(int degrees)
    {
        if (previewIndex < 0 || previewIndex >= scannedPages.Count)
        {
            return;
        }

        var page = scannedPages[previewIndex];
        page.ImageBytes = await ImageTransform.RotarAsync(page.ImageBytes, degrees);
        UpdatePageDimensions(page);

        successMessage = $"Imagen rotada {Math.Abs(degrees)}°";
        await InvokeAsync(StateHasChanged);
    }

    private async Task FlipHorizontalPage()
    {
        if (previewIndex < 0 || previewIndex >= scannedPages.Count)
        {
            return;
        }

        var page = scannedPages[previewIndex];
        page.ImageBytes = await ImageTransform.VoltearHorizontalAsync(page.ImageBytes);

        successMessage = "Imagen volteada horizontalmente";
        await InvokeAsync(StateHasChanged);
    }

    private async Task FlipVerticalPage()
    {
        if (previewIndex < 0 || previewIndex >= scannedPages.Count)
        {
            return;
        }

        var page = scannedPages[previewIndex];
        page.ImageBytes = await ImageTransform.VoltearVerticalAsync(page.ImageBytes);

        successMessage = "Imagen volteada verticalmente";
        await InvokeAsync(StateHasChanged);
    }

    private async Task AutoCropPage()
    {
        if (previewIndex < 0 || previewIndex >= scannedPages.Count)
        {
            return;
        }

        var page = scannedPages[previewIndex];
        page.ImageBytes = await ImageTransform.AutoRecortarAsync(page.ImageBytes);
        UpdatePageDimensions(page);

        successMessage = "Imagen recortada automáticamente";
        await InvokeAsync(StateHasChanged);
    }

    private void UpdatePageDimensions(ScannedPageDto page)
    {
        try
        {
            var dimensiones = ImageTransform.ObtenerDimensiones(page.ImageBytes);
            page.Width = dimensiones.Ancho;
            page.Height = dimensiones.Alto;
        }
        catch
        {
            // Si falla, mantener dimensiones anteriores
        }
    }

    /// <summary>
    /// Establece el nivel de zoom de la vista previa
    /// </summary>
    private void SetZoom(int zoom)
    {
        previewZoom = zoom;
        StateHasChanged();
    }

    /// <summary>
    /// Aumenta el zoom de la vista previa
    /// </summary>
    private void ZoomIn()
    {
        if (previewZoom == 0) previewZoom = 50;
        else if (previewZoom < 200) previewZoom += 25;
        StateHasChanged();
    }

    /// <summary>
    /// Reduce el zoom de la vista previa
    /// </summary>
    private void ZoomOut()
    {
        if (previewZoom > 25) previewZoom -= 25;
        else previewZoom = 0;
        StateHasChanged();
    }

    /// <summary>
    /// Genera el estilo CSS para aplicar zoom a la imagen
    /// </summary>
    private string GetImageZoomStyle()
    {
        if (previewZoom == 0)
        {
            return "max-width: 100%; max-height: 100%; width: auto; height: auto;";
        }

        return $"max-width: none; max-height: none; width: {previewZoom}%; height: auto;";
    }

    #endregion

    #region Popup Pantalla Completa (Overlay)

    /// <summary>
    /// Abre el overlay de vista previa a pantalla completa
    /// </summary>
    private void OpenFullscreenOverlay()
    {
        if (scannedPages.Count == 0 && previewImage == null)
        {
            Console.WriteLine("[FULLSCREEN] No hay imágenes para mostrar");
            return;
        }

        Console.WriteLine("[FULLSCREEN] Abriendo overlay de vista previa...");

        if (scannedPages.Count > 0)
        {
            fullscreenCurrentIndex = previewIndex;
            fullscreenTotalPages = scannedPages.Count;
            UpdateFullscreenImage();
        }
        else if (previewImage != null)
        {
            fullscreenCurrentIndex = 0;
            fullscreenTotalPages = 1;
            fullscreenImageUrl = ImageTransform.ABase64(previewImage.ImageBytes, previewImage.MimeType);
            fullscreenImageInfo = $"{previewImage.Width}x{previewImage.Height} px";
            fullscreenPageInfo = "Vista previa";
        }

        showFullscreenPreview = true;
    }

    private void UpdateFullscreenImage()
    {
        var page = GetCurrentPage();
        if (page == null)
        {
            return;
        }

        fullscreenImageUrl = ImageTransform.ABase64(page.ImageBytes, page.MimeType);
        fullscreenImageInfo = $"{page.Width}x{page.Height} px | {page.Dpi} dpi";
        fullscreenPageInfo = $"Página {fullscreenCurrentIndex + 1} de {fullscreenTotalPages}";
    }

    /// <summary>
    /// Navega a una página específica en el popup
    /// </summary>
    private void NavigateFullscreenPage(int newIndex)
    {
        if (newIndex >= 0 && newIndex < fullscreenTotalPages)
        {
            fullscreenCurrentIndex = newIndex;
            previewIndex = newIndex;
            UpdateFullscreenImage();
            StateHasChanged();
        }
    }

    #endregion

    #region Vista Previa y Selección de Área

    private async Task StartPreviewScan()
    {
        if (isScanning)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedDeviceId))
        {
            errorMessage = "Seleccione un escáner antes de continuar.";
            return;
        }

        isScanning = true;
        errorMessage = null;
        successMessage = null;

        try
        {
            scanOptions.DeviceId = selectedDeviceId;

            var result = await ScannerService.PreviewScanAsync(scanOptions);
            if (result.IsSuccess && result.Value != null)
            {
                previewImage = result.Value;
                isPreviewMode = true;
                selectedArea = null;
                successMessage = "Vista previa obtenida. Seleccione el área a escanear o presione ESCANEAR para área completa.";
            }
            else
            {
                errorMessage = result.Error ?? "Error al obtener vista previa";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isScanning = false;
            StateHasChanged();
        }
    }

    private void ExitPreviewMode()
    {
        isPreviewMode = false;
        previewImage = null;
        selectedArea = null;
        isSelectingArea = false;
    }

    private async Task ScanWithSelectedArea()
    {
        if (isScanning)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedDeviceId))
        {
            errorMessage = "Seleccione un escáner antes de continuar.";
            return;
        }

        isScanning = true;
        isPreviewMode = false;
        errorMessage = null;

        try
        {
            scanOptions.DeviceId = selectedDeviceId;

            var result = await ScannerService.ScanSinglePageAsync(scanOptions);
            if (result.IsSuccess && result.Value != null)
            {
                var doc = result.Value;
                byte[] finalBytes = doc.ImageBytes;
                int width = doc.Width;
                int height = doc.Height;

                if (selectedArea != null && !selectedArea.IsFullArea)
                {
                    finalBytes = await ImageTransform.RecortarAreaAsync(finalBytes, selectedArea);

                    var dimensiones = ImageTransform.ObtenerDimensiones(finalBytes);
                    width = dimensiones.Ancho;
                    height = dimensiones.Alto;
                }

                var page = new ScannedPageDto
                {
                    ImageBytes = finalBytes,
                    MimeType = doc.MimeType,
                    Width = width,
                    Height = height,
                    Dpi = doc.Dpi,
                    ScannedAt = doc.ScannedAt,
                    PageNumber = scannedPages.Count + 1
                };

                State.AddPage(page);
                previewIndex = scannedPages.Count - 1;

                successMessage = selectedArea?.IsFullArea == false
                    ? "Área seleccionada escaneada y recortada correctamente"
                    : "Página escaneada correctamente";
            }
            else
            {
                errorMessage = result.Error ?? "Error al escanear";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isScanning = false;
            previewImage = null;
            selectedArea = null;
            StateHasChanged();
        }
    }

    private async Task StartAreaSelectionAsync(MouseEventArgs e)
    {
        try
        {
            var pos = await JS.InvokeAsync<PositionResult?>("getMousePercentInContainer", e);
            if (pos != null)
            {
                isSelectingArea = true;
                selectionStart = (pos.x, pos.y);
                selectionCurrent = selectionStart;
                StateHasChanged();
            }
        }
        catch
        {
            isSelectingArea = true;
            selectionStart = (e.OffsetX / 4, e.OffsetY / 3);
            selectionCurrent = selectionStart;
        }
    }

    private async Task UpdateAreaSelectionAsync(MouseEventArgs e)
    {
        if (!isSelectingArea)
        {
            return;
        }

        try
        {
            var pos = await JS.InvokeAsync<PositionResult?>("getMousePercentInContainer", e);
            if (pos != null)
            {
                selectionCurrent = (pos.x, pos.y);
                StateHasChanged();
            }
        }
        catch
        {
            selectionCurrent = (e.OffsetX / 4, e.OffsetY / 3);
            StateHasChanged();
        }
    }

    private async Task EndAreaSelectionAsync(MouseEventArgs e)
    {
        if (!isSelectingArea)
        {
            return;
        }

        isSelectingArea = false;

        try
        {
            var pos = await JS.InvokeAsync<PositionResult?>("getMousePercentInContainer", e);
            if (pos != null)
            {
                selectionCurrent = (pos.x, pos.y);
            }
        }
        catch
        {
            // Ignorar
        }

        double left = Math.Min(selectionStart.X, selectionCurrent.X);
        double top = Math.Min(selectionStart.Y, selectionCurrent.Y);
        double width = Math.Abs(selectionCurrent.X - selectionStart.X);
        double height = Math.Abs(selectionCurrent.Y - selectionStart.Y);

        if (width > 5 && height > 5)
        {
            selectedArea = new ScanAreaDto
            {
                Left = Math.Max(0, Math.Min(100, left)),
                Top = Math.Max(0, Math.Min(100, top)),
                Width = Math.Min(100 - left, width),
                Height = Math.Min(100 - top, height)
            };
        }

        StateHasChanged();
    }

    // Clase para deserializar resultado de JS (propiedades en minúsculas para match con JSON)
    private class PositionResult
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    private void ClearSelectedArea()
    {
        selectedArea = null;
        StateHasChanged();
    }

    #endregion

    #region Perfiles de Escaneo

    private async Task RefreshProfiles()
    {
        try
        {
            Console.WriteLine("[PROFILES] Cargando perfiles de escaneo...");
            var result = await Workflow.RefreshProfilesAsync();
            if (result.IsSuccess && result.Value != null)
            {
                profiles.Clear();
                profiles.AddRange(result.Value);
            }
            else
            {
                profiles.Clear();
            }

            Console.WriteLine($"[PROFILES] Se encontraron {profiles.Count} perfiles:");
            foreach (var p in profiles)
            {
                Console.WriteLine($"[PROFILES]   - ID:{p.Id} '{p.Name}' ColorMode:{p.ColorMode} Dpi:{p.Dpi} Default:{p.IsDefault}");
            }

            var defaultProfile = profiles.FirstOrDefault(p => p.IsDefault);
            if (defaultProfile != null && selectedProfileId == 0)
            {
                Console.WriteLine($"[PROFILES] Aplicando perfil predeterminado: {defaultProfile.Name}");
                selectedProfileId = defaultProfile.Id;
                await LoadSelectedProfile();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PROFILES] ERROR al cargar perfiles: {ex.Message}");
            profiles.Clear();
        }
        finally
        {
            StateHasChanged();
        }
    }

    private async Task LoadSelectedProfile()
    {
        Console.WriteLine($"[PROFILE] LoadSelectedProfile llamado con selectedProfileId={selectedProfileId}");

        if (selectedProfileId == 0)
        {
            Console.WriteLine("[PROFILE] selectedProfileId es 0, no se carga perfil");
            return;
        }

        var profile = profiles.FirstOrDefault(p => p.Id == selectedProfileId);
        if (profile != null)
        {
            Console.WriteLine($"[PROFILE] Cargando perfil: {profile.Name}");
            Console.WriteLine($"[PROFILE]   - Dpi: {profile.Dpi}");
            Console.WriteLine($"[PROFILE]   - ColorMode: {profile.ColorMode}");
            Console.WriteLine($"[PROFILE]   - Source: {profile.Source}");
            Console.WriteLine($"[PROFILE]   - PageSize: {profile.PageSize}");

            scanOptions.Dpi = profile.Dpi;
            scanOptions.ColorMode = profile.ColorMode;
            scanOptions.Source = profile.Source;
            scanOptions.PageSize = profile.PageSize;

            Console.WriteLine($"[PROFILE] Después de aplicar: scanOptions.ColorMode = {scanOptions.ColorMode}");

            ApplyImageCorrection(profile.ToImageCorrection());

            await ProfileRepository.UpdateLastUsedAsync(profile.Id);
            successMessage = $"Perfil \"{profile.Name}\" cargado";
            StateHasChanged();
        }
    }

    private void OpenSaveProfileDialog()
    {
        newProfileName = string.Empty;
        newProfileDescription = string.Empty;
        newProfileIsDefault = false;
        showSaveProfileDialog = true;
    }

    private void CloseSaveProfileDialog()
    {
        showSaveProfileDialog = false;
    }

    private async Task SaveCurrentProfile()
    {
        if (string.IsNullOrWhiteSpace(newProfileName))
        {
            return;
        }

        try
        {
            await Workflow.SaveProfileAsync(
                newProfileName,
                newProfileDescription,
                newProfileIsDefault,
                scanOptions,
                imageCorrection);

            await RefreshProfiles();

            var savedProfile = profiles
                .Where(p => p.Name.Equals(newProfileName.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.Id)
                .FirstOrDefault();

            if (savedProfile != null)
            {
                selectedProfileId = savedProfile.Id;
            }

            successMessage = $"Perfil \"{newProfileName.Trim()}\" guardado correctamente";
            CloseSaveProfileDialog();
        }
        catch (Exception ex)
        {
            errorMessage = $"Error guardando perfil: {ex.Message}";
        }
    }

    #endregion

    private void ApplyImageCorrection(ImageCorrectionDto source)
    {
        imageCorrection.Brightness = source.Brightness;
        imageCorrection.Contrast = source.Contrast;
        imageCorrection.Gamma = source.Gamma;
        imageCorrection.Sharpness = source.Sharpness;
        imageCorrection.BlackWhiteThreshold = source.BlackWhiteThreshold;
    }

    private void RenumberPages()
    {
        for (int i = 0; i < scannedPages.Count; i++)
        {
            scannedPages[i].PageNumber = i + 1;
        }
    }

    public void Dispose()
    {
        ScannerService.ScanProgress -= OnScanProgress;
        State.StateChanged -= OnStateChanged;
    }
}
