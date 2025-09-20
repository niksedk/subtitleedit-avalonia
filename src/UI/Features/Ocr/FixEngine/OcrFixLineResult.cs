using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.FixEngine;

public class OcrFixLineResult
{
    public int LineIndex { get; set; }
    public List<OcrFixLinePartResult> Words { get; set; } = new();
    public SubtitleLineViewModel Paragraph { get; set; } = new();
}