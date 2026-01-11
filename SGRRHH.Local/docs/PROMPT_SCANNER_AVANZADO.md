# PROMPT: Potenciación del Módulo de Scanner

## ✅ Estado de Implementación

| Fase | Descripción | Estado | Fecha |
|------|-------------|--------|-------|
| 1 | Corrección de imagen (brillo, contraste, gamma, nitidez) | ✅ Completada | 2026-01-11 |
| 2 | Transformaciones (rotación, voltear, auto-crop) | ✅ Completada | 2026-01-11 |
| 3 | Vista previa real (pre-escaneo, selección de área) | ✅ Completada | 2026-01-11 |
| 4 | Perfiles de escaneo (guardar/cargar configuraciones) | ✅ Completada | 2026-01-11 |
| 5 | OCR integrado (PDF buscable con Tesseract) | ✅ Completada | 2026-01-11 |

**\* Nota FASE 5:** OCR habilitado con paquete `Tesseract 5.2.0`. Los archivos de idioma (.traineddata) están instalados en `C:\SGRRHH\tessdata` (spa.traineddata, eng.traineddata).

### Archivos creados/modificados (Fases 1-5):
- `SGRRHH.Local.Shared/Interfaces/IImageProcessingService.cs` - ✅ Modificado (CropToArea)
- `SGRRHH.Local.Infrastructure/Services/ImageProcessingService.cs` - ✅ Modificado (CropToArea)
- `SGRRHH.Local.Domain/DTOs/ScannerDtos.cs` - ✅ Modificado (ScanAreaDto, ScanProfileDto)
- `SGRRHH.Local.Server/Components/Shared/ScannerModal.razor` - ✅ Modificado (Perfiles, Preview, OCR)
- `SGRRHH.Local.Server/Components/Shared/ScannerModal.razor.css` - ✅ Modificado (estilos nuevas funciones)
- `SGRRHH.Local.Server/Program.cs` - ✅ Modificado (registro servicios)
- `SGRRHH.Local.Shared/Interfaces/IScannerService.cs` - ✅ Modificado (PreviewScanAsync)
- `SGRRHH.Local.Infrastructure/Services/ScannerService.cs` - ✅ Modificado (PreviewScanAsync)
- `SGRRHH.Local.Shared/Interfaces/IScanProfileRepository.cs` - ✅ Nuevo
- `SGRRHH.Local.Infrastructure/Repositories/ScanProfileRepository.cs` - ✅ Nuevo
- `SGRRHH.Local.Shared/Interfaces/IOcrService.cs` - ✅ Nuevo
- `SGRRHH.Local.Infrastructure/Services/OcrService.cs` - ✅ Nuevo
- `scripts/migration_scan_profiles_v1.sql` - ✅ Nuevo (ejecutar para crear tabla)

---

## 📋 Contexto

El sistema SGRRHH.Local tiene un módulo de escaneo basado en **NAPS2.Sdk** que funciona correctamente pero carece de características avanzadas presentes en herramientas como IJ Scan Utility (Canon) y Epson Scan Utility.

### Archivos existentes:
- `SGRRHH.Local.Shared/Interfaces/IScannerService.cs` - Interfaz del servicio
- `SGRRHH.Local.Infrastructure/Services/ScannerService.cs` - Implementación
- `SGRRHH.Local.Domain/DTOs/ScannerDtos.cs` - DTOs y enums
- `SGRRHH.Local.Server/Components/Shared/ScannerModal.razor` - UI actual
- `SGRRHH.Local.Server/Components/Shared/ScannerModal.razor.css` - Estilos

### Tecnología base:
- NAPS2.Sdk v1.2.1 (WIA, TWAIN, ESCL)
- NAPS2.Images.Gdi para procesamiento
- QuestPDF para generación de PDFs
- Blazor Server para UI

---

## 🎯 Objetivos

Implementar las siguientes características en **5 fases independientes**:

1. **Corrección de imagen** (brillo, contraste, gamma, nitidez, umbral B&N)
2. **Transformaciones** (rotación 90°, voltear, deskew automático, auto-crop)
3. **Vista previa real** (pre-escaneo, selección de área)
4. **Perfiles de escaneo** (guardar/cargar configuraciones)
5. **OCR integrado** (PDF buscable con Tesseract)

---

## FASE 1: Corrección de Imagen

### 1.1 Nuevos DTOs

Agregar a `ScannerDtos.cs`:

```csharp
/// <summary>
/// Opciones de corrección de imagen post-escaneo
/// </summary>
public class ImageCorrectionDto
{
    /// <summary>
    /// Brillo: -100 a +100 (0 = sin cambio)
    /// </summary>
    public int Brightness { get; set; } = 0;
    
    /// <summary>
    /// Contraste: -100 a +100 (0 = sin cambio)
    /// </summary>
    public int Contrast { get; set; } = 0;
    
    /// <summary>
    /// Gamma: 0.1 a 3.0 (1.0 = sin cambio)
    /// </summary>
    public float Gamma { get; set; } = 1.0f;
    
    /// <summary>
    /// Nitidez/Sharpness: 0 a 100 (0 = sin cambio)
    /// </summary>
    public int Sharpness { get; set; } = 0;
    
    /// <summary>
    /// Umbral para blanco y negro: 0-255 (128 = default)
    /// Solo aplica cuando ColorMode = BlackWhite
    /// </summary>
    public int BlackWhiteThreshold { get; set; } = 128;
    
    /// <summary>
    /// Indica si hay correcciones activas
    /// </summary>
    public bool HasCorrections => 
        Brightness != 0 || Contrast != 0 || 
        Math.Abs(Gamma - 1.0f) > 0.01f || Sharpness > 0;
}
```

Modificar `ScanOptionsDto`:

```csharp
public class ScanOptionsDto
{
    // ... propiedades existentes ...
    
    /// <summary>
    /// Correcciones de imagen a aplicar después del escaneo
    /// </summary>
    public ImageCorrectionDto? ImageCorrection { get; set; }
}
```

### 1.2 Servicio de Procesamiento de Imagen

Crear `SGRRHH.Local.Infrastructure/Services/ImageProcessingService.cs`:

```csharp
using System.Drawing;
using System.Drawing.Imaging;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio para procesamiento y corrección de imágenes escaneadas
/// </summary>
public class ImageProcessingService : IImageProcessingService
{
    /// <summary>
    /// Aplica correcciones de brillo, contraste y gamma a una imagen
    /// </summary>
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
}
```

### 1.3 Interfaz del servicio

Crear `SGRRHH.Local.Shared/Interfaces/IImageProcessingService.cs`:

```csharp
using SGRRHH.Local.Domain.DTOs;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IImageProcessingService
{
    /// <summary>
    /// Aplica correcciones de imagen (brillo, contraste, gamma, nitidez)
    /// </summary>
    byte[] ApplyCorrections(byte[] imageBytes, ImageCorrectionDto corrections);
}
```

### 1.4 UI - Controles de corrección

Agregar al `ScannerModal.razor` después de las opciones existentes:

```razor
@* Panel de Corrección de Imagen (colapsable) *@
<div class="scanner-correction-panel">
    <div class="scanner-correction-header" @onclick="ToggleCorrectionPanel">
        <span>CORRECCION DE IMAGEN</span>
        <span class="scanner-correction-toggle">@(showCorrectionPanel ? "▼" : "▶")</span>
    </div>
    
    @if (showCorrectionPanel)
    {
        <div class="scanner-correction-body">
            <div class="scanner-slider-row">
                <label>BRILLO:</label>
                <input type="range" min="-100" max="100" @bind="imageCorrection.Brightness" 
                       @bind:event="oninput" disabled="@isScanning" />
                <span class="scanner-slider-value">@imageCorrection.Brightness</span>
            </div>
            
            <div class="scanner-slider-row">
                <label>CONTRASTE:</label>
                <input type="range" min="-100" max="100" @bind="imageCorrection.Contrast" 
                       @bind:event="oninput" disabled="@isScanning" />
                <span class="scanner-slider-value">@imageCorrection.Contrast</span>
            </div>
            
            <div class="scanner-slider-row">
                <label>GAMMA:</label>
                <input type="range" min="10" max="300" @bind="gammaSliderValue" 
                       @bind:event="oninput" disabled="@isScanning" />
                <span class="scanner-slider-value">@imageCorrection.Gamma.ToString("F1")</span>
            </div>
            
            <div class="scanner-slider-row">
                <label>NITIDEZ:</label>
                <input type="range" min="0" max="100" @bind="imageCorrection.Sharpness" 
                       @bind:event="oninput" disabled="@isScanning" />
                <span class="scanner-slider-value">@imageCorrection.Sharpness</span>
            </div>
            
            @if (scanOptions.ColorMode == ScanColorMode.BlackWhite)
            {
                <div class="scanner-slider-row">
                    <label>UMBRAL B/N:</label>
                    <input type="range" min="0" max="255" @bind="imageCorrection.BlackWhiteThreshold" 
                           @bind:event="oninput" disabled="@isScanning" />
                    <span class="scanner-slider-value">@imageCorrection.BlackWhiteThreshold</span>
                </div>
            }
            
            <button class="scanner-btn-small" @onclick="ResetCorrections" disabled="@isScanning">
                RESTABLECER
            </button>
        </div>
    }
</div>
```

Variables adicionales en @code:

```csharp
private ImageCorrectionDto imageCorrection = new();
private bool showCorrectionPanel = false;

// Gamma usa slider entero (10-300) mapeado a float (0.1-3.0)
private int gammaSliderValue
{
    get => (int)(imageCorrection.Gamma * 100);
    set => imageCorrection.Gamma = value / 100f;
}

private void ToggleCorrectionPanel() => showCorrectionPanel = !showCorrectionPanel;

private void ResetCorrections()
{
    imageCorrection = new ImageCorrectionDto();
}
```

### 1.5 CSS adicional

Agregar a `ScannerModal.razor.css`:

```css
/* ========================================
   CORRECTION PANEL
   ======================================== */
.scanner-correction-panel {
    border: 1px solid #ccc;
    margin-top: 8px;
}

.scanner-correction-header {
    background-color: #e0e0e0;
    padding: 8px 12px;
    font-size: 11px;
    font-weight: bold;
    cursor: pointer;
    display: flex;
    justify-content: space-between;
    align-items: center;
    user-select: none;
}

.scanner-correction-header:hover {
    background-color: #d0d0d0;
}

.scanner-correction-toggle {
    font-size: 10px;
}

.scanner-correction-body {
    padding: 12px;
    display: flex;
    flex-direction: column;
    gap: 10px;
    background-color: #f9f9f9;
}

.scanner-slider-row {
    display: flex;
    align-items: center;
    gap: 8px;
}

.scanner-slider-row label {
    font-size: 10px;
    font-weight: bold;
    min-width: 70px;
}

.scanner-slider-row input[type="range"] {
    flex: 1;
    height: 6px;
    cursor: pointer;
}

.scanner-slider-value {
    font-size: 11px;
    font-family: 'Courier New', monospace;
    min-width: 40px;
    text-align: right;
}
```

---

## FASE 2: Transformaciones de Imagen

### 2.1 Nuevos métodos en IImageProcessingService

```csharp
public interface IImageProcessingService
{
    // ... existentes ...
    
    /// <summary>
    /// Rota la imagen 90° en sentido horario
    /// </summary>
    byte[] RotateClockwise(byte[] imageBytes);
    
    /// <summary>
    /// Rota la imagen 90° en sentido antihorario
    /// </summary>
    byte[] RotateCounterClockwise(byte[] imageBytes);
    
    /// <summary>
    /// Rota la imagen 180°
    /// </summary>
    byte[] Rotate180(byte[] imageBytes);
    
    /// <summary>
    /// Voltea la imagen horizontalmente
    /// </summary>
    byte[] FlipHorizontal(byte[] imageBytes);
    
    /// <summary>
    /// Voltea la imagen verticalmente
    /// </summary>
    byte[] FlipVertical(byte[] imageBytes);
    
    /// <summary>
    /// Aplica corrección automática de inclinación (deskew)
    /// </summary>
    byte[] AutoDeskew(byte[] imageBytes);
    
    /// <summary>
    /// Recorta automáticamente los bordes blancos/negros
    /// </summary>
    byte[] AutoCrop(byte[] imageBytes, int threshold = 10);
}
```

### 2.2 Implementación de transformaciones

```csharp
public byte[] RotateClockwise(byte[] imageBytes)
{
    using var ms = new MemoryStream(imageBytes);
    using var image = Image.FromStream(ms);
    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
    
    using var output = new MemoryStream();
    image.Save(output, ImageFormat.Jpeg);
    return output.ToArray();
}

public byte[] RotateCounterClockwise(byte[] imageBytes)
{
    using var ms = new MemoryStream(imageBytes);
    using var image = Image.FromStream(ms);
    image.RotateFlip(RotateFlipType.Rotate270FlipNone);
    
    using var output = new MemoryStream();
    image.Save(output, ImageFormat.Jpeg);
    return output.ToArray();
}

public byte[] Rotate180(byte[] imageBytes)
{
    using var ms = new MemoryStream(imageBytes);
    using var image = Image.FromStream(ms);
    image.RotateFlip(RotateFlipType.Rotate180FlipNone);
    
    using var output = new MemoryStream();
    image.Save(output, ImageFormat.Jpeg);
    return output.ToArray();
}

public byte[] FlipHorizontal(byte[] imageBytes)
{
    using var ms = new MemoryStream(imageBytes);
    using var image = Image.FromStream(ms);
    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
    
    using var output = new MemoryStream();
    image.Save(output, ImageFormat.Jpeg);
    return output.ToArray();
}

public byte[] FlipVertical(byte[] imageBytes)
{
    using var ms = new MemoryStream(imageBytes);
    using var image = Image.FromStream(ms);
    image.RotateFlip(RotateFlipType.RotateNoneFlipY);
    
    using var output = new MemoryStream();
    image.Save(output, ImageFormat.Jpeg);
    return output.ToArray();
}

public byte[] AutoDeskew(byte[] imageBytes)
{
    // NAPS2 tiene soporte nativo para deskew
    // Alternativa: usar algoritmo de detección de líneas Hough
    using var ms = new MemoryStream(imageBytes);
    using var bitmap = new Bitmap(ms);
    
    float angle = DetectSkewAngle(bitmap);
    if (Math.Abs(angle) < 0.5f) // Menos de 0.5° no amerita corrección
        return imageBytes;
    
    return RotateImage(bitmap, -angle);
}

private float DetectSkewAngle(Bitmap bitmap)
{
    // Algoritmo  usar OpenCV o similar
    // ...
    return 0f; // Placeholder
}

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
    
    if (width < 50 || height < 50) // Imagen muy pequeña, no recortar
        return imageBytes;
    
    var rect = new Rectangle(left, top, width, height);
    using var cropped = bitmap.Clone(rect, bitmap.PixelFormat);
    
    using var output = new MemoryStream();
    cropped.Save(output, ImageFormat.Jpeg);
    return output.ToArray();
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
```

### 2.3 UI - Barra de herramientas de transformación

Agregar después del preview de imagen:

```razor
@if (scannedPages.Count > 0 && !isScanning)
{
    <div class="scanner-toolbar">
        <button class="scanner-tool-btn" @onclick="() => RotatePage(-90)" title="Rotar izquierda">
            ↺
        </button>
        <button class="scanner-tool-btn" @onclick="() => RotatePage(90)" title="Rotar derecha">
            ↻
        </button>
        <button class="scanner-tool-btn" @onclick="FlipCurrentPage" title="Voltear horizontal">
            ⇆
        </button>
        <div class="scanner-tool-separator"></div>
        <button class="scanner-tool-btn" @onclick="AutoDeskewPage" title="Auto-enderezar">
            ⊿
        </button>
        <button class="scanner-tool-btn" @onclick="AutoCropPage" title="Auto-recortar">
            ⬚
        </button>
    </div>
}
```

Métodos en @code:

```csharp
@inject IImageProcessingService ImageProcessor

private async Task RotatePage(int degrees)
{
    if (previewIndex < 0 || previewIndex >= scannedPages.Count) return;
    
    var page = scannedPages[previewIndex];
    byte[] rotated = degrees switch
    {
        90 => ImageProcessor.RotateClockwise(page.ImageBytes),
        -90 => ImageProcessor.RotateCounterClockwise(page.ImageBytes),
        180 => ImageProcessor.Rotate180(page.ImageBytes),
        _ => page.ImageBytes
    };
    
    // Actualizar dimensiones
    using var ms = new MemoryStream(rotated);
    using var img = Image.FromStream(ms);
    
    page.ImageBytes = rotated;
    page.Width = img.Width;
    page.Height = img.Height;
    
    StateHasChanged();
}

private void FlipCurrentPage()
{
    if (previewIndex < 0 || previewIndex >= scannedPages.Count) return;
    
    var page = scannedPages[previewIndex];
    page.ImageBytes = ImageProcessor.FlipHorizontal(page.ImageBytes);
    StateHasChanged();
}

private void AutoDeskewPage()
{
    if (previewIndex < 0 || previewIndex >= scannedPages.Count) return;
    
    var page = scannedPages[previewIndex];
    page.ImageBytes = ImageProcessor.AutoDeskew(page.ImageBytes);
    successMessage = "Imagen enderezada automáticamente";
    StateHasChanged();
}

private void AutoCropPage()
{
    if (previewIndex < 0 || previewIndex >= scannedPages.Count) return;
    
    var page = scannedPages[previewIndex];
    var cropped = ImageProcessor.AutoCrop(page.ImageBytes);
    
    using var ms = new MemoryStream(cropped);
    using var img = Image.FromStream(ms);
    
    page.ImageBytes = cropped;
    page.Width = img.Width;
    page.Height = img.Height;
    
    successMessage = "Imagen recortada automáticamente";
    StateHasChanged();
}
```

### 2.4 CSS para toolbar

```css
/* ========================================
   IMAGE TOOLBAR
   ======================================== */
.scanner-toolbar {
    display: flex;
    gap: 4px;
    padding: 8px;
    background-color: #f0f0f0;
    border-bottom: 1px solid #ccc;
    justify-content: center;
}

.scanner-tool-btn {
    width: 32px;
    height: 32px;
    border: 1px solid #000;
    background-color: #fff;
    cursor: pointer;
    font-size: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-family: 'Courier New', monospace;
}

.scanner-tool-btn:hover {
    background-color: #000;
    color: #fff;
}

.scanner-tool-separator {
    width: 1px;
    background-color: #ccc;
    margin: 0 8px;
}
```

---

## FASE 3: Vista Previa Real (Pre-Scan)

### 3.1 Nuevo método en IScannerService

```csharp
/// <summary>
/// Realiza un pre-escaneo en baja resolución para vista previa
/// </summary>
/// <param name="options">Opciones (se forzará DPI bajo)</param>
/// <returns>Imagen de preview</returns>
Task<Result<ScannedDocumentDto>> PreviewScanAsync(ScanOptionsDto? options = null);
```

### 3.2 Implementación

```csharp
public async Task<Result<ScannedDocumentDto>> PreviewScanAsync(ScanOptionsDto? options = null)
{
    options ??= new ScanOptionsDto();
    
    // Forzar configuración para preview rápido
    var previewOptions = new ScanOptionsDto
    {
        DeviceId = options.DeviceId,
        Source = options.Source,
        Dpi = 75, // Baja resolución para velocidad
        ColorMode = options.ColorMode,
        PageSize = options.PageSize
    };
    
    return await ScanSinglePageAsync(previewOptions);
}
```

### 3.3 Modelo para área de selección

```csharp
/// <summary>
/// Área rectangular de selección para recorte
/// Valores en porcentaje (0-100) del área total
/// </summary>
public class ScanAreaDto
{
    public double Left { get; set; } = 0;
    public double Top { get; set; } = 0;
    public double Width { get; set; } = 100;
    public double Height { get; set; } = 100;
    
    public bool IsFullArea => 
        Left == 0 && Top == 0 && Width == 100 && Height == 100;
}
```

### 3.4 UI - Modo Preview con selección

```razor
@if (isPreviewMode && previewImage != null)
{
    <div class="scanner-preview-area">
        <div class="scanner-preview-canvas" 
             @onmousedown="StartSelection"
             @onmousemove="UpdateSelection"
             @onmouseup="EndSelection">
            <img src="data:@previewImage.MimeType;base64,@Convert.ToBase64String(previewImage.ImageBytes)" 
                 @ref="previewImageRef" />
            
            @if (isSelecting || scanArea != null)
            {
                <div class="scanner-selection-overlay"
                     style="left: @(scanArea?.Left ?? selectionStart.X)%; 
                            top: @(scanArea?.Top ?? selectionStart.Y)%;
                            width: @(scanArea?.Width ?? (selectionEnd.X - selectionStart.X))%;
                            height: @(scanArea?.Height ?? (selectionEnd.Y - selectionStart.Y))%;">
                </div>
            }
        </div>
        
        <div class="scanner-preview-actions">
            <button class="scanner-btn" @onclick="ClearSelection">AREA COMPLETA</button>
            <button class="scanner-btn scanner-btn-primary" @onclick="ScanWithArea">
                ESCANEAR AREA SELECCIONADA
            </button>
        </div>
    </div>
}
```

---

## FASE 4: Perfiles de Escaneo

### 4.1 Modelo de Perfil

```csharp
/// <summary>
/// Perfil de escaneo guardado por el usuario
/// </summary>
public class ScanProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    
    // Configuración de escaneo
    public int Dpi { get; set; } = 200;
    public ScanColorMode ColorMode { get; set; } = ScanColorMode.Color;
    public ScanSource Source { get; set; } = ScanSource.Flatbed;
    public ScanPageSize PageSize { get; set; } = ScanPageSize.Letter;
    
    // Correcciones de imagen
    public ImageCorrectionDto? ImageCorrection { get; set; }
    
    // Transformaciones automáticas
    public bool AutoDeskew { get; set; }
    public bool AutoCrop { get; set; }
    
    // Metadatos
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastUsedAt { get; set; }
}
```

### 4.2 Tabla SQL

```sql
-- Migration: Perfiles de escaneo
CREATE TABLE IF NOT EXISTS ScanProfiles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    IsDefault INTEGER NOT NULL DEFAULT 0,
    Dpi INTEGER NOT NULL DEFAULT 200,
    ColorMode TEXT NOT NULL DEFAULT 'Color',
    Source TEXT NOT NULL DEFAULT 'Flatbed',
    PageSize TEXT NOT NULL DEFAULT 'Letter',
    Brightness INTEGER DEFAULT 0,
    Contrast INTEGER DEFAULT 0,
    Gamma REAL DEFAULT 1.0,
    Sharpness INTEGER DEFAULT 0,
    BlackWhiteThreshold INTEGER DEFAULT 128,
    AutoDeskew INTEGER DEFAULT 0,
    AutoCrop INTEGER DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastUsedAt TEXT
);

-- Perfiles predefinidos
INSERT INTO ScanProfiles (Name, Description, IsDefault, Dpi, ColorMode, Source)
VALUES 
    ('Documento Rápido', 'Escaneo rápido de documentos en escala de grises', 0, 150, 'Grayscale', 'Flatbed'),
    ('Documento Alta Calidad', 'Escaneo de alta calidad para archivo', 0, 300, 'Color', 'Flatbed'),
    ('Cédula/ID', 'Optimizado para documentos de identidad', 0, 300, 'Color', 'Flatbed'),
    ('Foto', 'Escaneo de fotografías en alta resolución', 0, 600, 'Color', 'Flatbed');
```

### 4.3 Repositorio

```csharp
public interface IScanProfileRepository
{
    Task<List<ScanProfileDto>> GetAllAsync();
    Task<ScanProfileDto?> GetByIdAsync(int id);
    Task<ScanProfileDto?> GetDefaultAsync();
    Task<ScanProfileDto> SaveAsync(ScanProfileDto profile);
    Task DeleteAsync(int id);
    Task SetDefaultAsync(int id);
}
```

### 4.4 UI - Selector de perfiles

```razor
<div class="scanner-profiles">
    <select @bind="selectedProfileId" class="scanner-select" disabled="@isScanning"
            @bind:after="LoadSelectedProfile">
        <option value="0">-- CONFIGURACION MANUAL --</option>
        @foreach (var profile in profiles)
        {
            <option value="@profile.Id">
                @profile.Name @(profile.IsDefault ? "(★)" : "")
            </option>
        }
    </select>
    <button class="scanner-btn-small" @onclick="SaveCurrentAsProfile" title="Guardar perfil">
        💾
    </button>
</div>
```

---

## FASE 5: OCR Integrado

### 5.1 Dependencias NuGet

```xml
<PackageReference Include="NAPS2.Ocr" Version="1.2.1" />
```

El paquete NAPS2.Ocr usa Tesseract internamente.

### 5.2 Interfaz OCR

```csharp
public interface IOcrService
{
    /// <summary>
    /// Extrae texto de una imagen escaneada
    /// </summary>
    Task<Result<string>> ExtractTextAsync(byte[] imageBytes, string language = "spa");
    
    /// <summary>
    /// Genera un PDF buscable (PDF/A) a partir de imágenes
    /// </summary>
    Task<Result<byte[]>> GenerateSearchablePdfAsync(
        List<ScannedPageDto> pages, 
        string language = "spa");
    
    /// <summary>
    /// Obtiene los idiomas OCR disponibles
    /// </summary>
    Task<List<string>> GetAvailableLanguagesAsync();
}
```

### 5.3 Implementación

```csharp
using NAPS2.Ocr;
using NAPS2.Pdf;

public class OcrService : IOcrService
{
    private readonly ILogger<OcrService> _logger;
    
    public OcrService(ILogger<OcrService> logger)
    {
        _logger = logger;
    }
    
    public async Task<Result<byte[]>> GenerateSearchablePdfAsync(
        List<ScannedPageDto> pages, 
        string language = "spa")
    {
        try
        {
            using var scanningContext = new ScanningContext(new GdiImageContext());
            
            // Configurar OCR
            var ocrEngine = TesseractOcrEngine.BundledWithMirrors(language);
            scanningContext.OcrEngine = ocrEngine;
            
            // Convertir páginas a ProcessedImage
            var images = new List<ProcessedImage>();
            foreach (var page in pages)
            {
                using var ms = new MemoryStream(page.ImageBytes);
                using var bitmap = new Bitmap(ms);
                var image = scanningContext.CreateProcessedImage(bitmap);
                images.Add(image);
            }
            
            // Exportar a PDF con OCR
            var pdfExporter = new PdfExporter(scanningContext);
            using var output = new MemoryStream();
            
            await pdfExporter.Export(output, images, new PdfExportParams
            {
                Compat = PdfCompat.PdfA2B // PDF/A para archivo
            });
            
            // Limpiar
            foreach (var img in images)
                img.Dispose();
            
            return Result<byte[]>.Ok(output.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando PDF con OCR");
            return Result<byte[]>.Fail($"Error OCR: {ex.Message}");
        }
    }
    
    public async Task<Result<string>> ExtractTextAsync(byte[] imageBytes, string language = "spa")
    {
        try
        {
            using var scanningContext = new ScanningContext(new GdiImageContext());
            var ocrEngine = TesseractOcrEngine.BundledWithMirrors(language);
            
            using var ms = new MemoryStream(imageBytes);
            using var bitmap = new Bitmap(ms);
            using var image = scanningContext.CreateProcessedImage(bitmap);
            
            var result = await ocrEngine.ProcessImage(
                scanningContext, 
                image, 
                new OcrParams(language, OcrMode.Fast));
            
            return Result<string>.Ok(result.RightToLeft 
                ? string.Join("\n", result.Elements.Select(e => e.Text).Reverse())
                : string.Join("\n", result.Elements.Select(e => e.Text)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extrayendo texto OCR");
            return Result<string>.Fail($"Error OCR: {ex.Message}");
        }
    }
    
    public Task<List<string>> GetAvailableLanguagesAsync()
    {
        // Idiomas comunes soportados por Tesseract
        return Task.FromResult(new List<string>
        {
            "spa", // Español
            "eng", // Inglés
            "por", // Portugués
            "fra", // Francés
            "deu"  // Alemán
        });
    }
}
```

### 5.4 UI - Opción OCR

```razor
@if (AllowMultiplePages)
{
    <div class="scanner-option-row">
        <label class="scanner-label">SALIDA:</label>
        <select @bind="outputFormat" class="scanner-select" disabled="@isScanning">
            <option value="images">Imágenes separadas</option>
            <option value="pdf">PDF simple</option>
            <option value="pdf-ocr">PDF buscable (OCR)</option>
        </select>
    </div>
    
    @if (outputFormat == "pdf-ocr")
    {
        <div class="scanner-option-row">
            <label class="scanner-label">IDIOMA OCR:</label>
            <select @bind="ocrLanguage" class="scanner-select" disabled="@isScanning">
                <option value="spa">Español</option>
                <option value="eng">Inglés</option>
            </select>
        </div>
    }
}
```

---

## 📦 Registro de Servicios

Agregar a `Program.cs`:

```csharp
// Servicios de Scanner avanzado
builder.Services.AddScoped<IScannerService, ScannerService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddScoped<IScanProfileRepository, ScanProfileRepository>();
```

---

## ✅ Orden de Implementación Recomendado

1. **FASE 1**: Corrección de imagen (independiente, alto impacto)
2. **FASE 2**: Transformaciones (depende de ImageProcessingService de F1)
3. **FASE 4**: Perfiles (independiente, mejora UX)
4. **FASE 3**: Vista previa real (más complejo, requiere JS interop)
5. **FASE 5**: OCR (requiere descarga de datos Tesseract ~50MB)

---

## 🔧 Notas de Implementación

1. **Seguridad de memoria**: Usar `using` para todos los Bitmap/Image
2. **Rendimiento**: Procesar imágenes en Task.Run para no bloquear UI
3. **Estilos**: Mantener estética "hospitalaria" (Courier New, bordes sólidos)
4. **Idioma**: Todo en español
5. **Compatibilidad**: Probar con escáneres reales antes de entregar

---

*Documento creado: Enero 2026*
*Para implementar: Iniciar nueva sesión con contexto limpio*
