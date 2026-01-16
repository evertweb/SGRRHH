using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class ScannerModalStateService : IScannerModalStateService
{
    private readonly List<ScannedPageDto> scannedPages = new();
    private readonly List<ScannerDeviceDto> scanners = new();
    private readonly List<ScanProfileDto> profiles = new();
    private readonly ScanOptionsDto scanOptions;
    private readonly ImageCorrectionDto imageCorrection;

    private int previewIndex;
    private int currentPage;
    private string? selectedDeviceId;
    private int? selectedProfileId;
    private string outputFormat = "images";
    private string ocrLanguage = "spa";
    private bool isScanning;
    private bool isGeneratingPdf;
    private bool isLoadingScanners;
    private string? errorMessage;
    private string? successMessage;

    public ScannerModalStateService()
    {
        scanOptions = new ScanOptionsDto { Dpi = 300 };
        imageCorrection = new ImageCorrectionDto();
    }

    public List<ScannedPageDto> ScannedPages => scannedPages;
    public int PreviewIndex
    {
        get => previewIndex;
        set => SetAndNotify(ref previewIndex, value);
    }

    public int CurrentPage
    {
        get => currentPage;
        set => SetAndNotify(ref currentPage, value);
    }

    public List<ScannerDeviceDto> Scanners => scanners;
    public string? SelectedDeviceId
    {
        get => selectedDeviceId;
        set => SetAndNotify(ref selectedDeviceId, value);
    }

    public List<ScanProfileDto> Profiles => profiles;
    public int? SelectedProfileId
    {
        get => selectedProfileId;
        set => SetAndNotify(ref selectedProfileId, value);
    }

    public ScanOptionsDto ScanOptions => scanOptions;
    public string OutputFormat
    {
        get => outputFormat;
        set => SetAndNotify(ref outputFormat, value);
    }

    public string OcrLanguage
    {
        get => ocrLanguage;
        set => SetAndNotify(ref ocrLanguage, value);
    }

    public ImageCorrectionDto ImageCorrection => imageCorrection;

    public bool IsScanning
    {
        get => isScanning;
        set => SetAndNotify(ref isScanning, value);
    }

    public bool IsGeneratingPdf
    {
        get => isGeneratingPdf;
        set => SetAndNotify(ref isGeneratingPdf, value);
    }

    public bool IsLoadingScanners
    {
        get => isLoadingScanners;
        set => SetAndNotify(ref isLoadingScanners, value);
    }

    public string? ErrorMessage
    {
        get => errorMessage;
        set => SetAndNotify(ref errorMessage, value);
    }

    public string? SuccessMessage
    {
        get => successMessage;
        set => SetAndNotify(ref successMessage, value);
    }

    public event EventHandler? StateChanged;

    public void AddPage(ScannedPageDto page)
    {
        scannedPages.Add(page);
        NotifyStateChanged();
    }

    public void RemovePage(int index)
    {
        if (index < 0 || index >= scannedPages.Count)
        {
            return;
        }

        scannedPages.RemoveAt(index);
        RenumberPages();

        if (previewIndex >= scannedPages.Count)
        {
            previewIndex = Math.Max(0, scannedPages.Count - 1);
        }

        NotifyStateChanged();
    }

    public void MovePage(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= scannedPages.Count)
        {
            return;
        }

        if (toIndex < 0 || toIndex >= scannedPages.Count)
        {
            return;
        }

        var item = scannedPages[fromIndex];
        scannedPages.RemoveAt(fromIndex);
        scannedPages.Insert(toIndex, item);
        RenumberPages();
        NotifyStateChanged();
    }

    public void ClearPages()
    {
        scannedPages.Clear();
        previewIndex = 0;
        NotifyStateChanged();
    }

    public void ApplyCorrectionToPage(int index, byte[] correctedBytes)
    {
        if (index < 0 || index >= scannedPages.Count)
        {
            return;
        }

        scannedPages[index].ImageBytes = correctedBytes;
        NotifyStateChanged();
    }

    public void Reset()
    {
        scannedPages.Clear();
        scanners.Clear();
        profiles.Clear();
        previewIndex = 0;
        currentPage = 0;
        selectedDeviceId = null;
        selectedProfileId = 0;
        outputFormat = "images";
        ocrLanguage = "spa";
        scanOptions.Dpi = 300;
        scanOptions.ColorMode = default;
        scanOptions.Source = default;
        scanOptions.PageSize = default;
        imageCorrection.Brightness = 0;
        imageCorrection.Contrast = 0;
        imageCorrection.Gamma = 1.0f;
        imageCorrection.Sharpness = 0;
        imageCorrection.BlackWhiteThreshold = 128;
        isScanning = false;
        isGeneratingPdf = false;
        isLoadingScanners = false;
        errorMessage = null;
        successMessage = null;
        NotifyStateChanged();
    }

    private void RenumberPages()
    {
        for (int i = 0; i < scannedPages.Count; i++)
        {
            scannedPages[i].PageNumber = i + 1;
        }
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetAndNotify<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        NotifyStateChanged();
    }
}
