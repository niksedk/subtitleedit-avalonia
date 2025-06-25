using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class TesseractOcr
{
    public string Error { get; set; }

    public TesseractOcr()
    {
        Error = string.Empty;
    }

    private string _executablePath = string.Empty;  

    public async Task<string> Ocr(SKBitmap bitmap, string language, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_executablePath))
        {
            _executablePath = "tesseract";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _executablePath = Path.Combine(Se.TesseractFolder, "tesseract.exe");
                if (!File.Exists(_executablePath))
                {
                    Error = "Tesseract executable not found: " + _executablePath;
                    return string.Empty;
                }
            }
        }

        var tempImage = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
        await File.WriteAllBytesAsync(tempImage, bitmap.ToPngArray(), cancellationToken);

        var tempTextFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _executablePath,
                Arguments = $"\"{tempImage}\" \"{tempTextFileName}\" -l {language} --psm 6 hocr --tessdata-dir \"{Se.TesseractModelFolder}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Se.TesseractFolder
            },
        };

#pragma warning disable CA1416 // Validate platform compatibility
        process.Start();
#pragma warning restore CA1416 // Validate platform compatibility;
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            Error = await process.StandardError.ReadToEndAsync(cancellationToken);
            return string.Empty;
        }

        File.Delete(tempImage);

        var outputFileName = tempTextFileName + ".html";
        if (!File.Exists(outputFileName))
        {
            outputFileName = tempTextFileName + ".hocr";
        }

        var result = string.Empty;
        try
        {
            if (File.Exists(outputFileName))
            {
                result = await File.ReadAllTextAsync(outputFileName, Encoding.UTF8, cancellationToken);
                result = ParseHOcr(result);
                File.Delete(outputFileName);
            }
            File.Delete(tempTextFileName);
        }
        catch
        {
            // ignored
        }

        return result;
    }

    private static string ParseHOcr(string html)
    {
        var sb = new StringBuilder();
        var lineStart = html.IndexOf("<span class='ocr_line'", StringComparison.InvariantCulture);
        var alternateLineStart = html.IndexOf("<span class='ocr_header'", StringComparison.InvariantCulture);
        if (alternateLineStart > 0)
        {
            lineStart = Math.Min(lineStart, alternateLineStart);
        }

        while (lineStart > 0)
        {
            var wordStart = html.IndexOf("<span class='ocrx_word'", lineStart, StringComparison.InvariantCulture);
            var wordMax = html.IndexOf("<span class='ocr_line'", lineStart + 1, StringComparison.InvariantCulture);
            if (wordMax <= 0)
            {
                wordMax = html.Length;
            }

            while (wordStart > 0 && wordStart <= wordMax)
            {
                var startText = html.IndexOf('>', wordStart + 1);
                if (startText > 0)
                {
                    startText++;
                    var endText = html.IndexOf("</span>", startText, StringComparison.InvariantCulture);
                    if (endText > 0)
                    {
                        var text = html.Substring(startText, endText - startText);
                        sb.Append(text.Trim()).Append(' ');
                    }
                }
                wordStart = html.IndexOf("<span class='ocrx_word'", wordStart + 1, StringComparison.InvariantCulture);
            }
            sb.AppendLine();
            lineStart = html.IndexOf("<span class='ocr_line'", lineStart + 1, StringComparison.InvariantCulture);
        }
        sb.Replace("<em>", "<i>")
          .Replace("</em>", "</i>")
          .Replace("<strong>", string.Empty)
          .Replace("</strong>", string.Empty)
          .Replace("</i> <i>", " ")
          .Replace("</i><i>", string.Empty);

        // html escape decoding
        sb.Replace("&amp;", "&")
          .Replace("&lt;", "<")
          .Replace("&gt;", ">")
          .Replace("&quot;", "\"")
          .Replace("&#39;", "'")
          .Replace("&apos;", "'");

        sb.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine)
          .Replace(" " + Environment.NewLine, Environment.NewLine);

        return sb.ToString().Trim();
    }
}
