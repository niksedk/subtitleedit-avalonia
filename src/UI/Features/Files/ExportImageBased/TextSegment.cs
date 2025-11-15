using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public record TextSegment(string Text, bool IsItalic, bool IsBold, SKColor Color, bool IsRightToLeft);