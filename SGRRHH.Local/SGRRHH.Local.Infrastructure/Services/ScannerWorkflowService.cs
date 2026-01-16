using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class ScannerWorkflowService : IScannerWorkflowService
{
    private readonly IScannerService scannerService;
    private readonly IImageTransformationService imageTransform;
    private readonly IScanProfileRepository profileRepository;
    private readonly IOcrService ocrService;

    public ScannerWorkflowService(
        IScannerService scannerService,
        IImageTransformationService imageTransform,
        IScanProfileRepository profileRepository,
        IOcrService ocrService)
    {
        this.scannerService = scannerService;
        this.imageTransform = imageTransform;
        this.profileRepository = profileRepository;
        this.ocrService = ocrService;
    }

    public Task<Result<List<ScannerDeviceDto>>> RefreshScannersAsync()
    {
        return scannerService.GetAvailableScannersAsync();
    }

    public async Task<Result<List<ScanProfileDto>>> RefreshProfilesAsync()
    {
        try
        {
            await profileRepository.InitializeDefaultProfilesAsync();
            var profiles = await profileRepository.GetAllAsync();
            return Result<List<ScanProfileDto>>.Ok(profiles);
        }
        catch (Exception ex)
        {
            return Result<List<ScanProfileDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result<ScannedPageDto>> ScanSinglePageAsync(
        string deviceId,
        ScanOptionsDto options,
        ImageCorrectionDto? corrections = null)
    {
        options.DeviceId = deviceId;

        var result = await scannerService.ScanSinglePageAsync(options);
        if (!result.IsSuccess || result.Value == null)
        {
            return Result<ScannedPageDto>.Fail(result.Error ?? "Error al escanear");
        }

        var doc = result.Value;
        var finalBytes = doc.ImageBytes;
        if (corrections != null && corrections.HasCorrections)
        {
            finalBytes = await imageTransform.AplicarCorreccionesAsync(doc.ImageBytes, corrections);
        }

        var page = new ScannedPageDto
        {
            ImageBytes = finalBytes,
            MimeType = doc.MimeType,
            Width = doc.Width,
            Height = doc.Height,
            Dpi = doc.Dpi,
            ScannedAt = doc.ScannedAt
        };

        return Result<ScannedPageDto>.Ok(page);
    }

    public async Task<Result<byte[]>> GeneratePdfAsync(List<ScannedPageDto> pages)
    {
        try
        {
            var bytes = await Task.Run(() =>
            {
                using var ms = new MemoryStream();

                Document.Create(container =>
                {
                    foreach (var page in pages)
                    {
                        container.Page(pageDescriptor =>
                        {
                            pageDescriptor.Size(PageSizes.Letter);
                            pageDescriptor.Margin(0);
                            pageDescriptor.Content().Image(page.ImageBytes).FitArea();
                        });
                    }
                }).GeneratePdf(ms);

                return ms.ToArray();
            });

            return Result<byte[]>.Ok(bytes);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Fail(ex.Message);
        }
    }

    public Task<Result<byte[]>> GenerateOcrPdfAsync(List<ScannedPageDto> pages, string language)
    {
        return ocrService.GenerateSearchablePdfAsync(pages, language);
    }

    public async Task<Result<byte[]>> ApplyCorrectionsAsync(byte[] imageBytes, ImageCorrectionDto corrections)
    {
        try
        {
            var result = await imageTransform.AplicarCorreccionesAsync(imageBytes, corrections);
            return Result<byte[]>.Ok(result);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Fail(ex.Message);
        }
    }

    public async Task SaveProfileAsync(
        string name,
        string? description,
        bool isDefault,
        ScanOptionsDto options,
        ImageCorrectionDto corrections)
    {
        var profile = new ScanProfileDto
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsDefault = isDefault,
            Dpi = options.Dpi,
            ColorMode = options.ColorMode,
            Source = options.Source,
            PageSize = options.PageSize,
            Brightness = corrections.Brightness,
            Contrast = corrections.Contrast,
            Gamma = corrections.Gamma,
            Sharpness = corrections.Sharpness,
            BlackWhiteThreshold = corrections.BlackWhiteThreshold
        };

        if (isDefault)
        {
            await profileRepository.SetDefaultAsync(0);
        }

        await profileRepository.SaveAsync(profile);
    }

    public Task DeleteProfileAsync(int profileId)
    {
        return profileRepository.DeleteAsync(profileId);
    }
}
