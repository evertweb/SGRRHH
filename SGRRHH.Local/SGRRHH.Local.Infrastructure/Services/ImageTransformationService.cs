using System.Drawing;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio para consolidar transformaciones y utilidades de imágenes.
/// </summary>
public class ImageTransformationService : IImageTransformationService
{
    private readonly IImageProcessingService imageProcessing;

    public ImageTransformationService(IImageProcessingService imageProcessing)
    {
        this.imageProcessing = imageProcessing;
    }

    public Task<byte[]> RotarAsync(byte[] imageBytes, int grados)
    {
        var normalizado = ((grados % 360) + 360) % 360;
        byte[] resultado = normalizado switch
        {
            90 => imageProcessing.RotateClockwise(imageBytes),
            180 => imageProcessing.Rotate180(imageBytes),
            270 => imageProcessing.RotateCounterClockwise(imageBytes),
            _ => imageBytes
        };

        return Task.FromResult(resultado);
    }

    public Task<byte[]> VoltearHorizontalAsync(byte[] imageBytes)
    {
        return Task.FromResult(imageProcessing.FlipHorizontal(imageBytes));
    }

    public Task<byte[]> VoltearVerticalAsync(byte[] imageBytes)
    {
        return Task.FromResult(imageProcessing.FlipVertical(imageBytes));
    }

    public Task<byte[]> AutoRecortarAsync(byte[] imageBytes, int umbral = 10)
    {
        return Task.FromResult(imageProcessing.AutoCrop(imageBytes, umbral));
    }

    public Task<byte[]> RecortarAreaAsync(byte[] imageBytes, ScanAreaDto area)
    {
        return Task.FromResult(imageProcessing.CropToArea(imageBytes, area));
    }

    public Task<byte[]> AplicarCorreccionesAsync(byte[] imageBytes, ImageCorrectionDto correcciones)
    {
        return Task.FromResult(imageProcessing.ApplyCorrections(imageBytes, correcciones));
    }

    public Task<byte[]> AjustarBrilloAsync(byte[] imageBytes, int brillo)
    {
        var correcciones = new ImageCorrectionDto { Brightness = brillo };
        return Task.FromResult(imageProcessing.ApplyCorrections(imageBytes, correcciones));
    }

    public Task<byte[]> AjustarContrasteAsync(byte[] imageBytes, int contraste)
    {
        var correcciones = new ImageCorrectionDto { Contrast = contraste };
        return Task.FromResult(imageProcessing.ApplyCorrections(imageBytes, correcciones));
    }

    public string ABase64(byte[] imageBytes, string mimeType = "image/jpeg")
    {
        return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
    }

    public byte[] DesdeBase64(string base64)
    {
        var data = base64;
        var commaIndex = data.IndexOf(',');
        if (commaIndex >= 0)
        {
            data = data[(commaIndex + 1)..];
        }

        return Convert.FromBase64String(data);
    }

    public (int Ancho, int Alto) ObtenerDimensiones(byte[] imageBytes)
    {
        using var ms = new MemoryStream(imageBytes);
        using var img = Image.FromStream(ms);
        return (img.Width, img.Height);
    }
}
