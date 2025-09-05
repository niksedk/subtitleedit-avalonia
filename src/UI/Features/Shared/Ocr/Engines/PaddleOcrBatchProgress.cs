namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class PaddleOcrBatchProgress
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public Shared.Ocr.OcrSubtitleItem? Item { get; set; }
}
