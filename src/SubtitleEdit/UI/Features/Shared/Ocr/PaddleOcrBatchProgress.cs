namespace Nikse.SubtitleEdit.Features.Common.Ocr;

public class PaddleOcrBatchProgress
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public OcrSubtitleItem? Item { get; set; }
}
