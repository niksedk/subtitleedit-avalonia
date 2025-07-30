using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public partial class PaddleOcr
{
    public string Error { get; set; }
    private List<PaddleOcrResultParser.TextDetectionResult> _textDetectionResults = new();
    private IProgress<PaddleOcrBatchProgress>? _batchProgress;
    private string _batchFileName = string.Empty;
    private List<PaddleOcrBatchInput> _batchFileNames = new List<PaddleOcrBatchInput>();
    private string _paddingOcrPath;
    private string _clsPath;
    private string _detPath;
    private string _recPath;
    private CancellationToken _cancellationToken;

    private const string TextlineOrientationModelName = "PP-LCNet_x1_0_textline_ori";

    private readonly List<string> LatinLanguageCodes = new List<string>
    {
        "af", "az", "bs", "cs", "cy", "da", "de", "es", "et", "fr", "ga",
        "hr", "hu", "id", "is", "it", "ku", "la", "lt", "lv", "mi", "ms",
        "mt", "nl", "no", "oc", "pi", "pl", "pt", "ro", "rs_latin", "sk",
        "sl", "sq", "sv", "sw", "tl", "tr", "uz", "vi", "french", "german"
    };

    private readonly List<string> ArabicLanguageCodes = new List<string>
    {
        "ar", "fa", "ug", "ur"
    };

    private readonly List<string> EslavLanguageCodes = new List<string>
    {
        "ru", "be", "uk"
    };

    private readonly List<string> CyrillicLanguageCodes = new List<string>
    {
        "rs_cyrillic", "bg", "mn", "abq", "ady", "kbd", "ava", "dar",
        "inh", "che", "lbe", "lez", "tab"
    };

    private readonly List<string> DevanagariLanguageCodes = new List<string>
    {
        "hi", "mr", "ne", "bh", "mai", "ang", "bho", "mah",
        "sck", "new", "gom", "bgc", "sa"
    };

    public PaddleOcr()
    {
        Error = string.Empty;
        _paddingOcrPath = Se.PaddleOcrFolder;
        _clsPath = Path.Combine(_paddingOcrPath, "cls");
        _detPath = Path.Combine(_paddingOcrPath, "det");
        _recPath = Path.Combine(_paddingOcrPath, "rec");
    }

    public async Task<string> Ocr(SKBitmap bitmap, string language, bool useGpu, string mode, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        string detFilePrefix = MakeDetPrefix(language);
        string recFilePrefix = MakeRecPrefix(language);

        var detName = GetDetectionName(language, mode);
        var recName = GetRecName(language, mode);

        var blackBlackground = MakeTransparentBlack(bitmap.Copy(bitmap.ColorType));
        var borderedBitmapTemp = AddBorder(blackBlackground, 10, SKColors.Transparent);
        blackBlackground.Dispose();
        var borderedBitmap = AddBorder(borderedBitmapTemp, 10, SKColors.Black);
        borderedBitmapTemp.Dispose();
        var tempImage = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
        await File.WriteAllBytesAsync(tempImage, borderedBitmap.ToPngArray(), cancellationToken);
        borderedBitmap.Dispose();

        var parameters = $"ocr -i \"{tempImage}\" " +
                 "--use_textline_orientation true " +
                 "--use_doc_orientation_classify false " +
                 "--use_doc_unwarping false " +
                 $"--device {(useGpu ? "gpu" : "cpu")} " +
                 $"--lang {language} " +
                 $"--text_detection_model_dir \"{_detPath + Path.DirectorySeparatorChar + detName}\" " +
                 $"--text_detection_model_name \"{detName}\" " +
                 $"--text_recognition_model_dir \"{_recPath + Path.DirectorySeparatorChar + recName}\" " +
                 $"--text_recognition_model_name \"{recName}\" " +
                 $"--textline_orientation_model_dir \"{_clsPath + Path.DirectorySeparatorChar + TextlineOrientationModelName}\" " +
                 $"--textline_orientation_model_name \"{TextlineOrientationModelName}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "paddleocr",
                Arguments = parameters,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };

        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
        process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
        process.OutputDataReceived += OutputHandler;
        _textDetectionResults.Clear();

#pragma warning disable CA1416 // Validate platform compatibility
        process.Start();
#pragma warning restore CA1416 // Validate platform compatibility;

        process.BeginOutputReadLine();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            Error = await process.StandardError.ReadToEndAsync(cancellationToken);
            return string.Empty;
        }

        File.Delete(tempImage);

        var result = MakeResult(_textDetectionResults);
        return result;
    }

    private string GetRecName(string language, string mode)
    {
        string recName;
        if (language == "ch" ||
            language == "chinese_cht" ||
            language == "en" ||
            language == "japan")
        {
            recName = $"PP-OCRv5_{mode}_rec";
        }
        else if (ArabicLanguageCodes.Contains(language))
        {
            recName = "arabic_PP-OCRv3_mobile_rec";
        }
        else if (EslavLanguageCodes.Contains(language))
        {
            recName = "eslav_PP-OCRv5_mobile_rec";
        }
        else if (CyrillicLanguageCodes.Contains(language))
        {
            recName = "cyrillic_PP-OCRv3_mobile_rec";
        }
        else if (DevanagariLanguageCodes.Contains(language))
        {
            recName = "devanagari_PP-OCRv3_mobile_rec";
        }
        else if (language == "korean")
        {
            recName = "korean_PP-OCRv5_mobile_rec";
        }
        else
        {
            recName = "latin_PP-OCRv5_mobile_rec";
        }

        return recName;
    }

    private string GetDetectionName(string language, string mode)
    {
        if (language == "ch" ||
            language == "chinese_cht" ||
            language == "en" ||
            language == "japan" ||
            language == "korean" ||
            LatinLanguageCodes.Contains(language) ||
            EslavLanguageCodes.Contains(language))
        {
            return $"PP-OCRv5_{mode}_det";
        }
        else
        {
            return "PP-OCRv3_mobile_det";
        }
    }

    private static SKBitmap MakeTransparentBlack(SKBitmap bitmap)
    {
        if (bitmap == null)
        {
            throw new ArgumentNullException(nameof(bitmap));
        }

        // Ensure the bitmap is mutable
        if (!bitmap.IsImmutable)
        {
            // Lock the pixels for modification
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);

                    // Check if the pixel is black
                    if (color.Alpha < 100)
                    {
                        // Set the pixel to be transparent
                        bitmap.SetPixel(x, y, new SKColor(0, 0, 0, 255));
                    }
                }
            }
        }

        return bitmap;
    }


    public async Task OcrBatch(List<PaddleOcrBatchInput> bitmaps, string language, bool useGpu, string mode, IProgress<PaddleOcrBatchProgress> progress, CancellationToken cancellationToken)
    {
        string detFilePrefix = MakeDetPrefix(language);
        string recFilePrefix = MakeRecPrefix(language);
        var detName = GetDetectionName(language, mode);
        var recName = GetRecName(language, mode);
        _batchProgress = progress;
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);
        _batchFileNames = new List<PaddleOcrBatchInput>();

        for (var i = 0; i < bitmaps.Count; i++)
        {
            var input = bitmaps[i];
            var bitmap = input.Bitmap == null ? new SKBitmap(1, 1, true) : input.Bitmap.Copy(input.Bitmap.ColorType);
            bitmap = MakeTransparentBlack(bitmap);
            var borderedBitmapTemp = AddBorder(bitmap, 10, SKColors.Black);
            bitmap.Dispose();
            var borderedBitmap = AddBorder(borderedBitmapTemp, 10, new SKColor(0, 0, 0, 0));
            borderedBitmapTemp.Dispose();
            var tempImage = Path.Combine(folder, input.Index.ToString(CultureInfo.InvariantCulture) + ".png");
            input.FileName = tempImage;
            _batchFileNames.Add(input);
            await File.WriteAllBytesAsync(tempImage, borderedBitmap.ToPngArray(), cancellationToken);
            borderedBitmap.Dispose();
        }

        var parameters = $"ocr -i \"{folder}\" " +
                    "--use_textline_orientation true " +
                    "--use_doc_orientation_classify false " +
                    "--use_doc_unwarping false " +
                    $"--device {(useGpu ? "gpu" : "cpu")} " +
                    $"--lang {language} " +
                    $"--text_detection_model_dir \"{_detPath + Path.DirectorySeparatorChar + detName}\" " +
                    $"--text_detection_model_name \"{detName}\" " +
                    $"--text_recognition_model_dir \"{_recPath + Path.DirectorySeparatorChar + recName}\" " +
                    $"--text_recognition_model_name \"{recName}\" " +
                    $"--textline_orientation_model_dir \"{_clsPath + Path.DirectorySeparatorChar + TextlineOrientationModelName}\" " +
                    $"--textline_orientation_model_name \"{TextlineOrientationModelName}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "paddleocr",
                Arguments = parameters,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };

        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
        process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
        process.OutputDataReceived += OutputHandlerBatch;
        _textDetectionResults.Clear();

#pragma warning disable CA1416 // Validate platform compatibility
        process.Start();
#pragma warning restore CA1416 // Validate platform compatibility;

        process.BeginOutputReadLine();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            Error = await process.StandardError.ReadToEndAsync(cancellationToken);
            return;
        }

        if (_textDetectionResults.Count > 0)
        {
            var input = _batchFileNames.First(p => p.FileName == _batchFileName);
            var p = new PaddleOcrBatchProgress
            {
                Index = input.Index,
                Text = MakeResult(_textDetectionResults),
                Item = input.Item,
            };
            _batchProgress?.Report(p);
            _textDetectionResults.Clear();
        }

        try
        {
            Directory.Delete(folder, true);
        }
        catch
        {
            // ignore
        }
    }

    private string MakeRecPrefix(string language)
    {
        var recFilePrefix = language;
        if (LatinLanguageCodes.Contains(language))
        {
            recFilePrefix = $"latin{Path.DirectorySeparatorChar}latin_PP-OCRv3_rec_infer";
        }
        else if (ArabicLanguageCodes.Contains(language))
        {
            recFilePrefix = $"arabic{Path.DirectorySeparatorChar}arabic_PP-OCRv4_rec_infer";
        }
        else if (CyrillicLanguageCodes.Contains(language))
        {
            recFilePrefix = $"cyrillic{Path.DirectorySeparatorChar}cyrillic_PP-OCRv3_rec_infer";
        }
        else if (DevanagariLanguageCodes.Contains(language))
        {
            recFilePrefix = $"devanagari{Path.DirectorySeparatorChar}devanagari_PP-OCRv4_rec_infer";
        }
        else if (language == "chinese_cht")
        {
            recFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv3_rec_infer";
        }
        else
        {
            recFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv4_rec_infer";
        }

        return recFilePrefix;
    }

    private static string MakeDetPrefix(string language)
    {
        var detFilePrefix = language;
        if (language != "en" && language != "ch")
        {
            detFilePrefix = $"ml{Path.DirectorySeparatorChar}Multilingual_PP-OCRv3_det_infer";
        }
        else if (language == "ch")
        {
            detFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv4_det_infer";
        }
        else
        {
            detFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv3_det_infer";
        }

        return detFilePrefix;
    }

    public static SKBitmap AddBorder(SKBitmap originalBitmap, int borderWidth, SKColor color)
    {
        // Calculate new dimensions
        int newWidth = originalBitmap.Width + 2 * borderWidth;
        int newHeight = originalBitmap.Height + 2 * borderWidth;

        // Create a new bitmap with the new dimensions
        SKBitmap borderedBitmap = new(newWidth, newHeight);

        // Create a canvas to draw on the new bitmap
        using (var canvas = new SKCanvas(borderedBitmap))
        {
            // Fill the canvas with a border color (optional)
            var borderColor = color;
            canvas.Clear(borderColor);

            // Draw the original bitmap onto the canvas, offset by the border width
            canvas.DrawBitmap(originalBitmap, borderWidth, borderWidth);
        }

        return borderedBitmap;
    }

    private string MakeResult(List<PaddleOcrResultParser.TextDetectionResult> textDetectionResults)
    {
        var sb = new StringBuilder();
        var lines = MakeLines(textDetectionResults);
        foreach (var line in lines)
        {
            var text = string.Join(' ', line.Select(p => p.Text));
            sb.AppendLine(text);
        }

        return sb.ToString().Trim().Replace(" " + Environment.NewLine, Environment.NewLine);
    }

    private List<List<PaddleOcrResultParser.TextDetectionResult>> MakeLines(List<PaddleOcrResultParser.TextDetectionResult> input)
    {
        var result = new List<List<PaddleOcrResultParser.TextDetectionResult>>();
        var heightAverage = input.Average(p => p.BoundingBox.Height);
        var sorted = input.OrderBy(p => p.BoundingBox.Center.Y);
        var line = new List<PaddleOcrResultParser.TextDetectionResult>();
        PaddleOcrResultParser.TextDetectionResult? last = null;
        foreach (var element in sorted)
        {
            if (last == null)
            {
                line.Add(element);
            }
            else
            {
                if (element.BoundingBox.Center.Y > last.BoundingBox.TopLeft.Y + heightAverage)
                {

                    result.Add(line.OrderBy(p => p.BoundingBox.TopLeft.X).ToList());
                    line = new List<PaddleOcrResultParser.TextDetectionResult>();
                }

                line.Add(element);
            }

            last = element;
        }

        if (line.Count > 0)
        {
            result.Add(line.OrderBy(p => p.BoundingBox.TopLeft.X).ToList());
        }

        return result;
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data) || _cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!outLine.Data.Contains("ppocr INFO:"))
        {
            return;
        }

        var arr = outLine.Data.Split("ppocr INFO: ");
        if (arr.Length < 2)
        {
            return;
        }

        var data = arr[1];

        string pattern = @"\[\[\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+]],\s*\(['""].*['""],\s*\d+\.\d+\)\]";
        var match = Regex.Match(data, pattern);
        if (match.Success)
        {
            var parser = new PaddleOcrResultParser();
            var x = parser.Parse(data);
            _textDetectionResults.Add(x);
        }

        // Example: [[[92.0, 56.0], [735.0, 60.0], [734.0, 118.0], [91.0, 113.0]], ('My mommy always said', 0.9907816052436829)]
    }

    private Lock _lock = new Lock();

    private void OutputHandlerBatch(object sendingProcess, DataReceivedEventArgs outLine)
    {

        if (string.IsNullOrWhiteSpace(outLine.Data) || _cancellationToken.IsCancellationRequested)
        {
            return;
        }

        Se.WriteWhisperLog(outLine.Data);

        if (!outLine.Data.Contains("ppocr INFO:"))
        {
            return;
        }

        lock (_lock)
        {
            foreach (var fileName in _batchFileNames)
            {
                if (outLine.Data.Contains(fileName.FileName))
                {
                    if (_textDetectionResults.Count > 0)
                    {
                        var old = _batchFileNames.First(p => p.FileName == _batchFileName);
                        var progress = new PaddleOcrBatchProgress
                        {
                            Index = old.Index,
                            Item = old.Item,
                            Text = MakeResult(_textDetectionResults),
                        };
                        _textDetectionResults.Clear();
                        _batchProgress?.Report(progress);
                    }
                    _batchFileName = fileName.FileName;
                    return;
                }
            }

            var arr = outLine.Data.Split("ppocr INFO: ");
            if (arr.Length < 2)
            {
                return;
            }

            var data = arr[1];

            string pattern = @"\[\[\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+]],\s*\(['""].*['""],\s*\d+\.\d+\)\]";
            var match = Regex.Match(data, pattern);
            if (match.Success)
            {
                var parser = new PaddleOcrResultParser();
                var x = parser.Parse(data);
                _textDetectionResults.Add(x);
            }
        }

        // Example: [[[92.0, 56.0], [735.0, 60.0], [734.0, 118.0], [91.0, 113.0]], ('My mommy always said', 0.9907816052436829)]
    }


    public static List<OcrLanguage2> GetLanguages()
    {
        return new List<OcrLanguage2>
        {
            new("abq", "Abkhazian"),
            new("ady", "Adyghe"),
            new("af", "Afrikaans"),
            new("sq", "Albanian"),
            new("ang", "Angika"),
            new("ar", "Arabic"),
            new("ava", "Avar"),
            new("az", "Azerbaijani"),
            new("be", "Belarusian"),
            new("bho", "Bhojpuri"),
            new("bh", "Bihari"),
            new("bs", "Bosnian"),
            new("bg", "Bulgarian"),
            new("ch", "Chinese and English"),
            new("chinese_cht", "Chinese traditional"),
            new("hr", "Croatian"),
            new("cs", "Czech"),
            new("da", "Danish"),
            new("dar", "Dargwa"),
            new("nl", "Dutch"),
            new("en", "English"),
            new("et", "Estonian"),
            new("fr", "French"),
            new("german", "German"),
            new("japan", "Japanese"),
            new("kbd", "Kabardian"),
            new("korean", "Korean"),
            new("ku", "Kurdish"),
            new("lbe", "Lak"),
            new("lv", "Latvian"),
            new("lez", "Lezghian"),
            new("lt", "Lithuanian"),
            new("mah", "Magahi"),
            new("mai", "Maithili"),
            new("ms", "Malay"),
            new("mt", "Maltese"),
            new("mi", "Maori"),
            new("mr", "Marathi"),
            new("mn", "Mongolian"),
            new("sck", "Nagpur"),
            new("ne", "Nepali"),
            new("new", "Newari"),
            new("no", "Norwegian"),
            new("oc", "Occitan"),
            new("fa", "Persian"),
            new("pl", "Polish"),
            new("pt", "Portuguese"),
            new("ro", "Romanian"),
            new("ru", "Russian"),
            new("sa", "Sanskrit"),
            new("rs_cyrillic", "Serbian (cyrillic)"),
            new("rs_latin", "Serbian (latin)"),
            new("sk", "Slovak"),
            new("sl", "Slovenian"),
            new("es", "Spanish"),
            new("sw", "Swahili"),
            new("sv", "Swedish"),
            new("tab", "Tabassaran"),
            new("tl", "Tagalog"),
            new("ta", "Tamil"),
            new("te", "Telugu"),
            new("tr", "Turkish"),
            new("uk", "Ukrainian"),
            new("ur", "Urdu"),
            new("ug", "Uyghur"),
            new("uz", "Uzbek"),
            new("vi", "Vietnamese"),
            new("cy", "Welsh"),
        }.OrderBy(p => p.Name).ToList();
    }
}
