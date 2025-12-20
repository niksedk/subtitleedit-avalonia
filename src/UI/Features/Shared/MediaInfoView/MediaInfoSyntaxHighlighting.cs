using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace Nikse.SubtitleEdit.Features.Shared.MediaInfoView;

/// <summary>
/// Syntax highlighting for media file information output
/// </summary>
public partial class MediaInfoSyntaxHighlighting : DocumentColorizingTransformer
{
    // Color scheme for media info
    private static readonly IBrush HeaderBrush = new SolidColorBrush(Color.Parse("#569CD6")); // Blue for headers (File name, Duration, etc.)
    private static readonly IBrush ValueBrush = new SolidColorBrush(Color.Parse("#CE9178")); // Orange for values
    private static readonly IBrush TrackNumberBrush = new SolidColorBrush(Color.Parse("#B5CEA8")); // Light green for track numbers
    private static readonly IBrush TrackTypeBrush = new SolidColorBrush(Color.Parse("#4EC9B0")); // Cyan for track types (Video, Audio, Other)
    private static readonly IBrush CodecBrush = new SolidColorBrush(Color.Parse("#DCDCAA")); // Yellow for codec names
    private static readonly IBrush TechnicalBrush = new SolidColorBrush(Color.Parse("#9CDCFE")); // Light blue for technical specs
    private static readonly IBrush SeparatorBrush = new SolidColorBrush(Color.Parse("#808080")); // Gray for separators and delimiters
    private static readonly Typeface BoldTypeface = new(FontFamily.Default, weight: FontWeight.Bold);

    // Pattern for field headers (e.g., "File name:", "Duration:")
    [GeneratedRegex(@"^(File name|File size|Duration|Resolution|Framerate|Container|Tracks):", RegexOptions.Multiline)]
    private static partial Regex FieldHeaderRegex();

    // Pattern for track headers (e.g., "#1 - Video")
    [GeneratedRegex(@"^#(\d+)\s*-\s*(Video|Audio|Other)", RegexOptions.Multiline)]
    private static partial Regex TrackHeaderRegex();

    // Pattern for numbers (file size, duration, resolution, framerate, bitrate, etc.)
    [GeneratedRegex(@"\b\d+[.,]?\d*\b")]
    private static partial Regex NumberRegex();

    // Pattern for codec names in parentheses (e.g., "(High)", "(LC)")
    [GeneratedRegex(@"\([^)]+\)")]
    private static partial Regex ParenthesesRegex();

    // Pattern for technical terms (kb/s, fps, Hz, tbr, tbn, etc.)
    [GeneratedRegex(@"\b(kb/s|mb|fps|tbr|tbn|Hz|SAR|DAR|progressive|stereo|fltp|start|default)\b", RegexOptions.IgnoreCase)]
    private static partial Regex TechnicalTermRegex();

    protected override void ColorizeLine(DocumentLine line)
    {
        var lineText = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        // Check for field headers (File name:, Duration:, etc.)
        var fieldHeaderMatch = FieldHeaderRegex().Match(lineText);
        if (fieldHeaderMatch.Success && fieldHeaderMatch.Index == 0)
        {
            // Colorize the header part
            ChangeLinePart(
                line.Offset,
                line.Offset + fieldHeaderMatch.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(HeaderBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });

            // Colorize the value part (everything after the colon)
            if (fieldHeaderMatch.Length < lineText.Length)
            {
                ChangeLinePart(
                    line.Offset + fieldHeaderMatch.Length,
                    line.Offset + line.Length,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(ValueBrush);
                    });
            }
            return;
        }

        // Check for track headers (#1 - Video, #2 - Audio, etc.)
        var trackHeaderMatch = TrackHeaderRegex().Match(lineText);
        if (trackHeaderMatch.Success && trackHeaderMatch.Index == 0)
        {
            // Colorize track number (#1, #2, etc.)
            var numberGroup = trackHeaderMatch.Groups[1];
            ChangeLinePart(
                line.Offset,
                line.Offset + numberGroup.Index + numberGroup.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(TrackNumberBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });

            // Colorize separator " - "
            var separatorStart = numberGroup.Index + numberGroup.Length;
            var typeGroup = trackHeaderMatch.Groups[2];
            ChangeLinePart(
                line.Offset + separatorStart,
                line.Offset + typeGroup.Index,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(SeparatorBrush);
                });

            // Colorize track type (Video, Audio, Other)
            ChangeLinePart(
                line.Offset + typeGroup.Index,
                line.Offset + typeGroup.Index + typeGroup.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(TrackTypeBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });

            // Colorize the rest of the line (track details)
            ColorizeTrackDetails(line, lineText, trackHeaderMatch.Length);
            return;
        }

        // If line starts with whitespace, it's likely a continuation of track info
        if (lineText.Length > 0 && char.IsWhiteSpace(lineText[0]))
        {
            ColorizeTrackDetails(line, lineText, 0);
        }
    }

    private void ColorizeTrackDetails(DocumentLine line, string lineText, int startOffset)
    {
        var lineStartOffset = line.Offset + startOffset;
        var remainingText = lineText.Substring(startOffset);

        // First, colorize codec names (first word on the line, typically)
        if (startOffset < lineText.Length)
        {
            var firstWordStart = startOffset;
            while (firstWordStart < lineText.Length && char.IsWhiteSpace(lineText[firstWordStart]))
                firstWordStart++;

            if (firstWordStart < lineText.Length)
            {
                var firstWordEnd = firstWordStart;
                while (firstWordEnd < lineText.Length && !char.IsWhiteSpace(lineText[firstWordEnd]) && lineText[firstWordEnd] != '(')
                    firstWordEnd++;

                if (firstWordEnd > firstWordStart)
                {
                    ChangeLinePart(
                        line.Offset + firstWordStart,
                        line.Offset + firstWordEnd,
                        element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(CodecBrush);
                            element.TextRunProperties.SetTypeface(BoldTypeface);
                        });
                }
            }
        }

        // Colorize parentheses content (codec details)
        foreach (Match match in ParenthesesRegex().Matches(remainingText))
        {
            ChangeLinePart(
                lineStartOffset + match.Index,
                lineStartOffset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(TechnicalBrush);
                });
        }

        // Colorize technical terms
        foreach (Match match in TechnicalTermRegex().Matches(remainingText))
        {
            ChangeLinePart(
                lineStartOffset + match.Index,
                lineStartOffset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(TechnicalBrush);
                });
        }

        // Colorize square brackets content (SAR, DAR)
        for (int i = 0; i < remainingText.Length; i++)
        {
            if (remainingText[i] == '[')
            {
                var endBracket = remainingText.IndexOf(']', i);
                if (endBracket != -1)
                {
                    ChangeLinePart(
                        lineStartOffset + i,
                        lineStartOffset + endBracket + 1,
                        element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(TechnicalBrush);
                        });
                    i = endBracket;
                }
            }
        }

        // Colorize commas as separators
        for (int i = 0; i < remainingText.Length; i++)
        {
            if (remainingText[i] == ',')
            {
                ChangeLinePart(
                    lineStartOffset + i,
                    lineStartOffset + i + 1,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(SeparatorBrush);
                    });
            }
        }
    }
}