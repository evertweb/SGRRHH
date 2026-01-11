using SGRRHH.Local.Domain.DTOs;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio para procesamiento y corrección de imágenes escaneadas
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Aplica correcciones de imagen (brillo, contraste, gamma, nitidez)
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen original</param>
    /// <param name="corrections">Configuración de correcciones a aplicar</param>
    /// <returns>Bytes de la imagen corregida</returns>
    byte[] ApplyCorrections(byte[] imageBytes, ImageCorrectionDto corrections);
    
    /// <summary>
    /// Rota la imagen 90° en sentido horario
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen original</param>
    /// <returns>Bytes de la imagen rotada</returns>
    byte[] RotateClockwise(byte[] imageBytes);
    
    /// <summary>
    /// Rota la imagen 90° en sentido antihorario
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen original</param>
    /// <returns>Bytes de la imagen rotada</returns>
    byte[] RotateCounterClockwise(byte[] imageBytes);
    
    /// <summary>
    /// Rota la imagen 180°
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen original</param>
    /// <returns>Bytes de la imagen rotada</returns>
    byte[] Rotate180(byte[] imageBytes);
    
    /// <summary>
    /// Voltea la imagen horizontalmente
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen original</param>
    /// <returns>Bytes de la imagen volteada</returns>
    byte[] FlipHorizontal(byte[] imageBytes);
    
    /// <summary>
    /// Voltea la imagen verticalmente
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen original</param>
    /// <returns>Bytes de la imagen volteada</returns>
    byte[] FlipVertical(byte[] imageBytes);
    
    /// <summary>
    /// Recorta automáticamente los bordes blancos/negros de la imagen
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen original</param>
    /// <param name="threshold">Umbral de detección de contenido (0-255)</param>
    /// <returns>Bytes de la imagen recortada</returns>
    byte[] AutoCrop(byte[] imageBytes, int threshold = 10);
    
    /// <summary>
    /// Recorta la imagen según un área especificada en porcentajes
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen original</param>
    /// <param name="area">Área de recorte en porcentajes (0-100)</param>
    /// <returns>Bytes de la imagen recortada</returns>
    byte[] CropToArea(byte[] imageBytes, ScanAreaDto area);
}
