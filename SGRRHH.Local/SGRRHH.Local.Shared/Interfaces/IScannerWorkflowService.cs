using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IScannerWorkflowService
{
    Task<Result<List<ScannerDeviceDto>>> RefreshScannersAsync();
    Task<Result<List<ScanProfileDto>>> RefreshProfilesAsync();
    Task<Result<ScannedPageDto>> ScanSinglePageAsync(string deviceId, ScanOptionsDto options, ImageCorrectionDto? corrections = null);
    Task<Result<byte[]>> GeneratePdfAsync(List<ScannedPageDto> pages);
    Task<Result<byte[]>> GenerateOcrPdfAsync(List<ScannedPageDto> pages, string language);
    Task<Result<byte[]>> ApplyCorrectionsAsync(byte[] imageBytes, ImageCorrectionDto corrections);
    Task SaveProfileAsync(string name, string? description, bool isDefault, ScanOptionsDto options, ImageCorrectionDto corrections);
    Task DeleteProfileAsync(int profileId);
}
