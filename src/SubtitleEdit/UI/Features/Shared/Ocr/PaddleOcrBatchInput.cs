using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class PaddleOcrBatchInput
{
    public int Index { get; set; }
    public SKBitmap? Bitmap { get; set; }
    public string Text { get; set; } = string.Empty;
    public string FileName { get;  set; } = string.Empty;
    public Shared.Ocr.OcrSubtitleItem? Item { get; set; }
}
