using System.Drawing;
using System.Drawing.Imaging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio para procesamiento y corrección de imágenes escaneadas
/// </summary>
public class ImageProcessingService : IImageProcessingService
{
    /// <inheritdoc />
    public byte[] ApplyCorrections(byte[] imageBytes, ImageCorrectionDto corrections)
    {
        if (!corrections.HasCorrections)
            return imageBytes;
        
        using var ms = new MemoryStream(imageBytes);
        using var original = Image.FromStream(ms);
        using var bitmap = new Bitmap(original);
        
        // Aplicar brillo y contraste
        if (corrections.Brightness != 0 || corrections.Contrast != 0)
        {
            ApplyBrightnessContrast(bitmap, corrections.Brightness, corrections.Contrast);
        }
        
        // Aplicar gamma
        if (Math.Abs(corrections.Gamma - 1.0f) > 0.01f)
        {
            ApplyGamma(bitmap, corrections.Gamma);
        }
        
        // Aplicar nitidez
        if (corrections.Sharpness > 0)
        {
            ApplySharpen(bitmap, corrections.Sharpness);
        }
        
        using var output = new MemoryStream();
        bitmap.Save(output, ImageFormat.Jpeg);
        return output.ToArray();
    }
    
    /// <inheritdoc />
    public byte[] RotateClockwise(byte[] imageBytes)
    {
        using var ms = new MemoryStream(imageBytes);
        using var image = Image.FromStream(ms);
        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
        
        using var output = new MemoryStream();
        image.Save(output, ImageFormat.Jpeg);
        return output.ToArray();
    }
    
    /// <inheritdoc />
    public byte[] RotateCounterClockwise(byte[] imageBytes)
    {
        using var ms = new MemoryStream(imageBytes);
        using var image = Image.FromStream(ms);
        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
        
        using var output = new MemoryStream();
        image.Save(output, ImageFormat.Jpeg);
        return output.ToArray();
    }
    
    /// <inheritdoc />
    public byte[] Rotate180(byte[] imageBytes)
    {
        using var ms = new MemoryStream(imageBytes);
        using var image = Image.FromStream(ms);
        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
        
        using var output = new MemoryStream();
        image.Save(output, ImageFormat.Jpeg);
        return output.ToArray();
    }
    
    /// <inheritdoc />
    public byte[] FlipHorizontal(byte[] imageBytes)
    {
        using var ms = new MemoryStream(imageBytes);
        using var image = Image.FromStream(ms);
        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
        
        using var output = new MemoryStream();
        image.Save(output, ImageFormat.Jpeg);
        return output.ToArray();
    }
    
    /// <inheritdoc />
    public byte[] FlipVertical(byte[] imageBytes)
    {
        using var ms = new MemoryStream(imageBytes);
        using var image = Image.FromStream(ms);
        image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        
        using var output = new MemoryStream();
        image.Save(output, ImageFormat.Jpeg);
        return output.ToArray();
    }
    
    /// <inheritdoc />
    public byte[] AutoCrop(byte[] imageBytes, int threshold = 10)
    {
        using var ms = new MemoryStream(imageBytes);
        using var bitmap = new Bitmap(ms);
        
        // Detectar bordes
        int left = 0, right = bitmap.Width - 1;
        int top = 0, bottom = bitmap.Height - 1;
        
        // Encontrar borde izquierdo
        for (int x = 0; x < bitmap.Width; x++)
        {
            if (HasContentInColumn(bitmap, x, threshold))
            {
                left = x;
                break;
            }
        }
        
        // Encontrar borde derecho
        for (int x = bitmap.Width - 1; x >= 0; x--)
        {
            if (HasContentInColumn(bitmap, x, threshold))
            {
                right = x;
                break;
            }
        }
        
        // Encontrar borde superior
        for (int y = 0; y < bitmap.Height; y++)
        {
            if (HasContentInRow(bitmap, y, threshold))
            {
                top = y;
                break;
            }
        }
        
        // Encontrar borde inferior
        for (int y = bitmap.Height - 1; y >= 0; y--)
        {
            if (HasContentInRow(bitmap, y, threshold))
            {
                bottom = y;
                break;
            }
        }
        
        // Agregar margen de 5px
        left = Math.Max(0, left - 5);
        top = Math.Max(0, top - 5);
        right = Math.Min(bitmap.Width - 1, right + 5);
        bottom = Math.Min(bitmap.Height - 1, bottom + 5);
        
        int width = right - left + 1;
        int height = bottom - top + 1;
        
        // Imagen muy pequeña, no recortar
        if (width < 50 || height < 50)
            return imageBytes;
        
        var rect = new Rectangle(left, top, width, height);
        using var cropped = bitmap.Clone(rect, bitmap.PixelFormat);
        
        using var output = new MemoryStream();
        cropped.Save(output, ImageFormat.Jpeg);
        return output.ToArray();
    }
    
    /// <inheritdoc />
    public byte[] CropToArea(byte[] imageBytes, ScanAreaDto area)
    {
        // Si es área completa, no recortar
        if (area.IsFullArea)
            return imageBytes;
        
        using var ms = new MemoryStream(imageBytes);
        using var bitmap = new Bitmap(ms);
        
        // Convertir porcentajes a píxeles
        int left = (int)(bitmap.Width * area.Left / 100.0);
        int top = (int)(bitmap.Height * area.Top / 100.0);
        int width = (int)(bitmap.Width * area.Width / 100.0);
        int height = (int)(bitmap.Height * area.Height / 100.0);
        
        // Validar dimensiones
        left = Math.Max(0, Math.Min(bitmap.Width - 1, left));
        top = Math.Max(0, Math.Min(bitmap.Height - 1, top));
        width = Math.Min(width, bitmap.Width - left);
        height = Math.Min(height, bitmap.Height - top);
        
        // Mínimo 50x50 px
        if (width < 50 || height < 50)
            return imageBytes;
        
        var rect = new Rectangle(left, top, width, height);
        using var cropped = bitmap.Clone(rect, bitmap.PixelFormat);
        
        using var output = new MemoryStream();
        cropped.Save(output, ImageFormat.Jpeg);
        return output.ToArray();
    }
    
    #region Métodos privados de procesamiento
    
    private void ApplyBrightnessContrast(Bitmap bitmap, int brightness, int contrast)
    {
        // Normalizar valores
        float brightnessFactor = brightness / 100f;
        float contrastFactor = (100f + contrast) / 100f;
        contrastFactor *= contrastFactor; // Cuadrático para mejor respuesta
        
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        
        unsafe
        {
            byte* ptr = (byte*)data.Scan0;
            int bytes = Math.Abs(data.Stride) * bitmap.Height;
            
            for (int i = 0; i < bytes; i++)
            {
                float pixel = ptr[i] / 255f;
                
                // Aplicar contraste (centrado en 0.5)
                pixel = (pixel - 0.5f) * contrastFactor + 0.5f;
                
                // Aplicar brillo
                pixel += brightnessFactor;
                
                // Clamp
                pixel = Math.Max(0, Math.Min(1, pixel));
                
                ptr[i] = (byte)(pixel * 255);
            }
        }
        
        bitmap.UnlockBits(data);
    }
    
    private void ApplyGamma(Bitmap bitmap, float gamma)
    {
        // Crear tabla de lookup para gamma
        byte[] gammaTable = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            gammaTable[i] = (byte)Math.Min(255, 
                (int)(255 * Math.Pow(i / 255.0, 1.0 / gamma)));
        }
        
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        
        unsafe
        {
            byte* ptr = (byte*)data.Scan0;
            int bytes = Math.Abs(data.Stride) * bitmap.Height;
            
            for (int i = 0; i < bytes; i++)
            {
                ptr[i] = gammaTable[ptr[i]];
            }
        }
        
        bitmap.UnlockBits(data);
    }
    
    private void ApplySharpen(Bitmap bitmap, int amount)
    {
        // Kernel de nitidez 3x3
        float factor = amount / 100f;
        float[,] kernel = {
            { 0, -factor, 0 },
            { -factor, 1 + 4*factor, -factor },
            { 0, -factor, 0 }
        };
        
        ApplyConvolution(bitmap, kernel);
    }
    
    private void ApplyConvolution(Bitmap bitmap, float[,] kernel)
    {
        using var clone = (Bitmap)bitmap.Clone();
        
        int width = bitmap.Width;
        int height = bitmap.Height;
        
        var srcData = clone.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);
            
        var dstData = bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format24bppRgb);
        
        int stride = srcData.Stride;
        
        unsafe
        {
            byte* srcPtr = (byte*)srcData.Scan0;
            byte* dstPtr = (byte*)dstData.Scan0;
            
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        float sum = 0;
                        for (int ky = -1; ky <= 1; ky++)
                        {
                            for (int kx = -1; kx <= 1; kx++)
                            {
                                int pos = (y + ky) * stride + (x + kx) * 3 + c;
                                sum += srcPtr[pos] * kernel[ky + 1, kx + 1];
                            }
                        }
                        int dstPos = y * stride + x * 3 + c;
                        dstPtr[dstPos] = (byte)Math.Max(0, Math.Min(255, sum));
                    }
                }
            }
        }
        
        clone.UnlockBits(srcData);
        bitmap.UnlockBits(dstData);
    }
    
    private bool HasContentInColumn(Bitmap bitmap, int x, int threshold)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            var pixel = bitmap.GetPixel(x, y);
            int brightness = (pixel.R + pixel.G + pixel.B) / 3;
            if (brightness < 255 - threshold)
                return true;
        }
        return false;
    }
    
    private bool HasContentInRow(Bitmap bitmap, int y, int threshold)
    {
        for (int x = 0; x < bitmap.Width; x++)
        {
            var pixel = bitmap.GetPixel(x, y);
            int brightness = (pixel.R + pixel.G + pixel.B) / 3;
            if (brightness < 255 - threshold)
                return true;
        }
        return false;
    }
    
    #endregion
}
