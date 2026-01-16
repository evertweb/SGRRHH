using SGRRHH.Local.Domain.DTOs;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IScannerModalStateService
{
    // Estado de páginas escaneadas
    List<ScannedPageDto> ScannedPages { get; }
    int PreviewIndex { get; set; }
    int CurrentPage { get; set; }

    // Estado de dispositivos
    List<ScannerDeviceDto> Scanners { get; }
    string? SelectedDeviceId { get; set; }

    // Estado de perfiles
    List<ScanProfileDto> Profiles { get; }
    int? SelectedProfileId { get; set; }

    // Estado de opciones de escaneo
    ScanOptionsDto ScanOptions { get; }
    string OutputFormat { get; set; }
    string OcrLanguage { get; set; }

    // Estado de corrección de imagen
    ImageCorrectionDto ImageCorrection { get; }

    // Flags de estado
    bool IsScanning { get; set; }
    bool IsGeneratingPdf { get; set; }
    bool IsLoadingScanners { get; set; }

    // Mensajes
    string? ErrorMessage { get; set; }
    string? SuccessMessage { get; set; }

    // Eventos
    event EventHandler? StateChanged;

    // Métodos
    void AddPage(ScannedPageDto page);
    void RemovePage(int index);
    void MovePage(int fromIndex, int toIndex);
    void ClearPages();
    void ApplyCorrectionToPage(int index, byte[] correctedBytes);
    void Reset();
}
