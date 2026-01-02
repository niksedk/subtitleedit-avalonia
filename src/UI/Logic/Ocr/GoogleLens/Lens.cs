using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public class Lens : LensCore
{
    public Lens(Dictionary<string, object>? config = null, Func<HttpRequestMessage, Task<HttpResponseMessage>>? fetch = null)
        : base(config, fetch)
    {
        if (config != null && config.GetType() != typeof(Dictionary<string, object>))
        {
            Console.WriteLine($"Lens constructor expects a dictionary, got {config.GetType()}");
            config = new Dictionary<string, object>();
        }
    }

    public async Task<LensResult> ScanByFile(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException($"scanByFile expects a string, got null or empty");
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        var fileBuffer = await File.ReadAllBytesAsync(path);
        return await ScanByBuffer(fileBuffer);
    }

    public async Task<LensResult> ScanByBuffer(byte[] buffer)
    {
        SKEncodedImageFormat? fileType = null;
        
        try
        {
            using var stream = new MemoryStream(buffer);
            using var codec = SKCodec.Create(stream);
            if (codec != null)
            {
                fileType = codec.EncodedFormat;
            }
        }
        catch
        {
            Console.WriteLine("Could not determine file type from buffer. Attempting to process anyway.");
        }

        int originalWidth, originalHeight;
        
        try
        {
            var dimensions = Helper.ImageDimensionsFromData(buffer);
            originalWidth = dimensions.Width;
            originalHeight = dimensions.Height;
        }
        catch (Exception e)
        {
            throw new Exception("Could not read image metadata using SkiaSharp.", e);
        }

        if (originalWidth == 0 || originalHeight == 0)
        {
            throw new Exception("Could not determine original image dimensions.");
        }

        var imageToProcessBuffer = buffer;
        var finalMime = fileType switch
        {
            SKEncodedImageFormat.Png => "image/png",
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            SKEncodedImageFormat.Webp => "image/webp",
            _ => "image/png"
        };

        const int MAX_DIMENSION = 1200;
        
        // Only process if absolutely necessary
        if (originalWidth > MAX_DIMENSION || originalHeight > MAX_DIMENSION)
        {
            using var inputStream = new MemoryStream(buffer);
            using var original = SKBitmap.Decode(inputStream);
            
            if (original == null)
                throw new Exception("Could not decode image");

            // Calculate new dimensions maintaining aspect ratio
            float ratio = Math.Min((float)MAX_DIMENSION / original.Width, (float)MAX_DIMENSION / original.Height);
            int newWidth = (int)(original.Width * ratio);
            int newHeight = (int)(original.Height * ratio);

            using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
            if (resized == null)
                throw new Exception("Could not resize image");
                
            using var image = SKImage.FromBitmap(resized);
            
            // Keep original format if possible
            SKEncodedImageFormat format = fileType == SKEncodedImageFormat.Png 
                ? SKEncodedImageFormat.Png 
                : SKEncodedImageFormat.Jpeg;
                
            if (format == SKEncodedImageFormat.Png)
            {
                finalMime = "image/png";
            }
            else
            {
                finalMime = "image/jpeg";
            }
            
            using var data = image.Encode(format, 90);
            imageToProcessBuffer = data.ToArray();
        }

        return await ScanByData(imageToProcessBuffer, finalMime, new[] { originalWidth, originalHeight });
    }
}
