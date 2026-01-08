using System.Drawing;
using System.Drawing.Imaging;

namespace SGRRHH.Local.Infrastructure.Services;

public static class ImageHelper
{
    public static byte[] ResizeImage(byte[] imageBytes, int maxWidth, int maxHeight)
    {
        using var input = new MemoryStream(imageBytes);
        using var image = Image.FromStream(input);

        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(1, Math.Min(ratioX, ratioY));

        if (ratio >= 1)
        {
            return imageBytes; // No upscale
        }

        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        using var resized = new Bitmap(newWidth, newHeight);
        using var graphics = Graphics.FromImage(resized);
        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        graphics.DrawImage(image, 0, 0, newWidth, newHeight);

        using var output = new MemoryStream();
        resized.Save(output, image.RawFormat);
        return output.ToArray();
    }

    public static byte[] OptimizeForStorage(byte[] imageBytes, int quality = 85)
    {
        using var input = new MemoryStream(imageBytes);
        using var image = Image.FromStream(input);
        using var output = new MemoryStream();

        var encoder = ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
        if (encoder == null)
        {
            image.Save(output, ImageFormat.Jpeg);
            return output.ToArray();
        }

        using var encoderParameters = new EncoderParameters(1);
        encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
        image.Save(output, encoder, encoderParameters);
        return output.ToArray();
    }

    public static bool IsValidImage(byte[] fileBytes)
    {
        return GetImageExtension(fileBytes) != null;
    }

    public static string? GetImageExtension(byte[] fileBytes)
    {
        if (fileBytes.Length < 4) return null;

        if (fileBytes[0] == 0xFF && fileBytes[1] == 0xD8)
            return ".jpg";

        if (fileBytes[0] == 0x89 && fileBytes[1] == 0x50 && fileBytes[2] == 0x4E && fileBytes[3] == 0x47)
            return ".png";

        if (fileBytes[0] == 0x47 && fileBytes[1] == 0x49 && fileBytes[2] == 0x46)
            return ".gif";

        if (fileBytes[0] == 0x42 && fileBytes[1] == 0x4D)
            return ".bmp";

        return null;
    }
}