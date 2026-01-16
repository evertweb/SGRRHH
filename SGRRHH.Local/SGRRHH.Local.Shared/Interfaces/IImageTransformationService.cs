using SGRRHH.Local.Domain.DTOs;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio para transformar imágenes (rotación, recorte, correcciones, base64).
/// </summary>
public interface IImageTransformationService
{
    Task<byte[]> RotarAsync(byte[] imageBytes, int grados);
    Task<byte[]> VoltearHorizontalAsync(byte[] imageBytes);
    Task<byte[]> VoltearVerticalAsync(byte[] imageBytes);
    Task<byte[]> AutoRecortarAsync(byte[] imageBytes, int umbral = 10);
    Task<byte[]> RecortarAreaAsync(byte[] imageBytes, ScanAreaDto area);
    Task<byte[]> AplicarCorreccionesAsync(byte[] imageBytes, ImageCorrectionDto correcciones);
    Task<byte[]> AjustarBrilloAsync(byte[] imageBytes, int brillo);
    Task<byte[]> AjustarContrasteAsync(byte[] imageBytes, int contraste);
    string ABase64(byte[] imageBytes, string mimeType = "image/jpeg");
    byte[] DesdeBase64(string base64);
    (int Ancho, int Alto) ObtenerDimensiones(byte[] imageBytes);
}
