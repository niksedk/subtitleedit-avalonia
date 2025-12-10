using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

public partial class SubtitleSyntaxHighlighting : DocumentColorizingTransformer
{
    private static readonly IBrush HtmlTagBrush = new SolidColorBrush(Color.Parse("#57A64A"));
    private static readonly Typeface BoldTypeface = new(FontFamily.Default, weight: FontWeight.Bold);

    [GeneratedRegex(@"</?(?:font[^>]*|[ibus]|sup|sub|ruby|rt)>", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"<font[^>]*color\s*=\s*[""']?([^""'>\s]+)[""']?[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex FontColorRegex();

    protected override void ColorizeLine(DocumentLine line)
    {
        var lineText = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        // Colorize HTML tags
        ColorizeHtmlTags(line, lineText);

        // Apply actual colors from font color attributes
        ApplyFontColors(line, lineText);
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

    private void ApplyFontColors(DocumentLine line, string lineText)
    {
        var fontColorMatches = FontColorRegex().Matches(lineText);
        if (fontColorMatches.Count == 0)
        {
            return;
        }

        var colorStack = new System.Collections.Generic.Stack<(int endPos, IBrush brush)>();

        // Build a list of all tags (opening font with color, closing font)
        var tags = new System.Collections.Generic.List<(int pos, bool isOpening, string? color)>();

        foreach (Match match in fontColorMatches)
        {
            var colorValue = match.Groups[1].Value;
            tags.Add((match.Index + match.Length, true, colorValue));
        }

        // Find all closing font tags
        foreach (Match match in Regex.Matches(lineText, @"</font>", RegexOptions.IgnoreCase))
        {
            tags.Add((match.Index, false, null));
        }

        // Sort tags by position
        tags.Sort((a, b) => a.pos.CompareTo(b.pos));

        foreach (var tag in tags)
        {
            if (tag.isOpening && tag.color != null)
            {
                // Find the matching closing tag
                var closingPos = FindMatchingClosingTag(lineText, tag.pos);
                if (closingPos > tag.pos)
                {
                    var brush = ParseColorToBrush(tag.color);
                    if (brush != null)
                    {
                        ChangeLinePart(
                            line.Offset + tag.pos,
                            line.Offset + closingPos,
                            element => element.TextRunProperties.SetForegroundBrush(brush));
                    }
                }
            }
        }
    }

    private static int FindMatchingClosingTag(string text, int startPos)
    {
        var closingTagMatch = Regex.Match(text[startPos..], @"</font>", RegexOptions.IgnoreCase);
        return closingTagMatch.Success ? startPos + closingTagMatch.Index : text.Length;
    }

    private static IBrush? ParseColorToBrush(string colorValue)
    {
        try
        {
            // Handle #RRGGBB format
            if (colorValue.StartsWith('#'))
            {
                return new SolidColorBrush(Color.Parse(colorValue));
            }

            // Handle named colors (e.g., "red", "blue")
            if (Color.TryParse(colorValue, out var namedColor))
            {
                return new SolidColorBrush(namedColor);
            }

            // Handle rgb(r,g,b) format
            var rgbMatch = Regex.Match(colorValue, @"rgb\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)", RegexOptions.IgnoreCase);
            if (rgbMatch.Success)
            {
                var r = byte.Parse(rgbMatch.Groups[1].Value);
                var g = byte.Parse(rgbMatch.Groups[2].Value);
                var b = byte.Parse(rgbMatch.Groups[3].Value);
                return new SolidColorBrush(Color.FromRgb(r, g, b));
            }

            // Handle legacy hex format without #
            if (colorValue.Length == 6 && int.TryParse(colorValue, NumberStyles.HexNumber, null, out _))
            {
                return new SolidColorBrush(Color.Parse("#" + colorValue));
            }
        }
        catch
        {
            // Invalid color format, ignore
        }

        return null;
    }
}
