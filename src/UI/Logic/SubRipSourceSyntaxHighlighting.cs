using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Syntax highlighting for SubRip (.srt) and WebVTT (.vtt) subtitle formats
/// </summary>
public partial class SubRipSourceSyntaxHighlighting : DocumentColorizingTransformer
{
    // Color scheme
    private static readonly IBrush NumberBrush = new SolidColorBrush(Color.Parse("#B5CEA8")); // Light green for numbers
    private static readonly IBrush TimeBrush = new SolidColorBrush(Color.Parse("#4EC9B0")); // Cyan for timecodes
    private static readonly IBrush TimeSeparatorBrush = new SolidColorBrush(Color.Parse("#C586C0")); // Purple/magenta for "-->" separator
    private static readonly IBrush HtmlTagBrush = new SolidColorBrush(Color.Parse("#57A64A")); // Green for HTML tags
    private static readonly Typeface BoldTypeface = new(FontFamily.Default, weight: FontWeight.Bold);

    // SubRip number pattern (e.g., "1", "2", "123")
    [GeneratedRegex(@"^\d+$", RegexOptions.Multiline)]
    private static partial Regex SubRipNumberRegex();

    // SubRip timecode pattern (e.g., "00:00:00,000 --> 00:00:01,670")
    [GeneratedRegex(@"\d{2}:\d{2}:\d{2}[,\.]\d{3}\s*-->\s*\d{2}:\d{2}:\d{2}[,\.]\d{3}")]
    private static partial Regex SubRipTimecodeRegex();

    // HTML tags in subtitle text
    [GeneratedRegex(@"</?(?:font[^>]*|[ibus]|sup|sub|ruby|rt)>", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlTagRegex();

    protected override void ColorizeLine(DocumentLine line)
    {
        var lineText = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        ColorizeSubRipFormat(line, lineText);
        ColorizeHtmlTags(line, lineText);
    }

    private void ColorizeSubRipFormat(DocumentLine line, string lineText)
    {
        // Colorize SubRip sequence numbers
        var numberMatch = SubRipNumberRegex().Match(lineText);
        if (numberMatch.Success && numberMatch.Value == lineText.Trim())
        {
            ChangeLinePart(
                line.Offset,
                line.Offset + line.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(NumberBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });
            return;
        }

        // Colorize SubRip timecodes with special handling for the separator
        foreach (Match match in SubRipTimecodeRegex().Matches(lineText))
        {
            var fullTimecode = match.Value;
            var separatorIndex = fullTimecode.IndexOf("-->", System.StringComparison.Ordinal);
            
            if (separatorIndex >= 0)
            {
                // Colorize the start timecode (before "-->")
                var startTimecodeLength = separatorIndex;
                ChangeLinePart(
                    line.Offset + match.Index,
                    line.Offset + match.Index + startTimecodeLength,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(TimeBrush);
                        element.TextRunProperties.SetTypeface(BoldTypeface);
                    });

                // Colorize the separator "-->" with a different color
                var separatorStart = match.Index + separatorIndex;
                var separatorEnd = separatorStart + 3; // Length of "-->"
                
                // Skip any whitespace before the separator
                while (separatorStart > match.Index && char.IsWhiteSpace(lineText[separatorStart - 1]))
                {
                    separatorStart--;
                }
                
                // Include whitespace after the separator
                while (separatorEnd < match.Index + match.Length && char.IsWhiteSpace(lineText[separatorEnd]))
                {
                    separatorEnd++;
                }
                
                ChangeLinePart(
                    line.Offset + separatorStart,
                    line.Offset + separatorEnd,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(TimeSeparatorBrush);
                        element.TextRunProperties.SetTypeface(BoldTypeface);
                    });

                // Colorize the end timecode (after "-->")
                var endTimecodeStart = separatorEnd;
                var endTimecodeLength = match.Index + match.Length - endTimecodeStart;
                
                if (endTimecodeLength > 0)
                {
                    ChangeLinePart(
                        line.Offset + endTimecodeStart,
                        line.Offset + match.Index + match.Length,
                        element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(TimeBrush);
                            element.TextRunProperties.SetTypeface(BoldTypeface);
                        });
                }
            }
            else
            {
                // Fallback: colorize the entire match as timecode
                ChangeLinePart(
                    line.Offset + match.Index,
                    line.Offset + match.Index + match.Length,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(TimeBrush);
                        element.TextRunProperties.SetTypeface(BoldTypeface);
                    });
            }
        }
    }

    private void ColorizeHtmlTags(DocumentLine line, string lineText)
    {
        foreach (Match match in HtmlTagRegex().Matches(lineText))
        {
            ChangeLinePart(
                line.Offset + match.Index,
                line.Offset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(HtmlTagBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });
        }
    }
}
