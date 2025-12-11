using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

public partial class SubtitleSourceSyntaxHighlighting : DocumentColorizingTransformer
{
    // Color scheme
    private static readonly IBrush NumberBrush = new SolidColorBrush(Color.Parse("#B5CEA8")); // Light green for numbers
    private static readonly IBrush TimeBrush = new SolidColorBrush(Color.Parse("#4EC9B0")); // Cyan for timecodes
    private static readonly IBrush XmlTagBrush = new SolidColorBrush(Color.Parse("#569CD6")); // Blue for XML tags
    private static readonly IBrush XmlAttributeBrush = new SolidColorBrush(Color.Parse("#9CDCFE")); // Light blue for attributes
    private static readonly IBrush XmlValueBrush = new SolidColorBrush(Color.Parse("#CE9178")); // Orange for attribute values
    private static readonly IBrush HtmlTagBrush = new SolidColorBrush(Color.Parse("#57A64A")); // Green for HTML tags
    private static readonly IBrush CommentBrush = new SolidColorBrush(Color.Parse("#6A9955")); // Green for XML comments
    private static readonly Typeface BoldTypeface = new(FontFamily.Default, weight: FontWeight.Bold);

    // SubRip number pattern (e.g., "1", "2", "123")
    [GeneratedRegex(@"^\d+$", RegexOptions.Multiline)]
    private static partial Regex SubRipNumberRegex();

    // SubRip timecode pattern (e.g., "00:00:00,000 --> 00:00:01,670")
    [GeneratedRegex(@"\d{2}:\d{2}:\d{2},\d{3}\s*-->\s*\d{2}:\d{2}:\d{2},\d{3}")]
    private static partial Regex SubRipTimecodeRegex();

    // XML tags (opening, closing, self-closing)
    [GeneratedRegex(@"</?[a-zA-Z_][\w:.-]*(?:\s+[^>]*)?/?>", RegexOptions.IgnoreCase)]
    private static partial Regex XmlTagRegex();

    // XML attributes
    [GeneratedRegex(@"([a-zA-Z_][\w:.-]*)\s*=\s*([""'])([^""']*)\2", RegexOptions.IgnoreCase)]
    private static partial Regex XmlAttributeRegex();

    // XML comments
    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex XmlCommentRegex();

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

        // Try to detect format and colorize accordingly
        ColorizeXmlComments(line, lineText);
        ColorizeXmlTags(line, lineText);
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

        // Colorize SubRip timecodes
        foreach (Match match in SubRipTimecodeRegex().Matches(lineText))
        {
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

    private void ColorizeXmlComments(DocumentLine line, string lineText)
    {
        foreach (Match match in XmlCommentRegex().Matches(lineText))
        {
            ChangeLinePart(
                line.Offset + match.Index,
                line.Offset + match.Index + match.Length,
                element => element.TextRunProperties.SetForegroundBrush(CommentBrush));
        }
    }

    private void ColorizeXmlTags(DocumentLine line, string lineText)
    {
        foreach (Match match in XmlTagRegex().Matches(lineText))
        {
            // Check if this is within a comment
            if (IsWithinComment(lineText, match.Index))
            {
                continue;
            }

            var tagText = match.Value;

            // Colorize the tag itself (brackets and tag name)
            ChangeLinePart(
                line.Offset + match.Index,
                line.Offset + match.Index + match.Length,
                element => element.TextRunProperties.SetForegroundBrush(XmlTagBrush));

            // Colorize attributes within the tag
            ColorizeXmlAttributes(line, lineText, match.Index, match.Length);
        }
    }

    private void ColorizeXmlAttributes(DocumentLine line, string lineText, int tagStart, int tagLength)
    {
        var tagText = lineText.Substring(tagStart, tagLength);
        
        foreach (Match attrMatch in XmlAttributeRegex().Matches(tagText))
        {
            // Colorize attribute name
            var attrNameStart = tagStart + attrMatch.Groups[1].Index;
            ChangeLinePart(
                line.Offset + attrNameStart,
                line.Offset + attrNameStart + attrMatch.Groups[1].Length,
                element => element.TextRunProperties.SetForegroundBrush(XmlAttributeBrush));

            // Colorize attribute value (including quotes)
            var attrValueStart = tagStart + attrMatch.Groups[2].Index;
            var attrValueLength = attrMatch.Groups[2].Length + attrMatch.Groups[3].Length + 1; // Include both quotes
            ChangeLinePart(
                line.Offset + attrValueStart,
                line.Offset + attrValueStart + attrValueLength,
                element => element.TextRunProperties.SetForegroundBrush(XmlValueBrush));
        }
    }

    private void ColorizeHtmlTags(DocumentLine line, string lineText)
    {
        // Don't colorize HTML tags if we're in XML mode (if line contains XML declaration or root elements)
        if (lineText.TrimStart().StartsWith("<?xml") || lineText.TrimStart().StartsWith("<!DOCTYPE"))
        {
            return;
        }

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

    private static bool IsWithinComment(string lineText, int position)
    {
        foreach (Match commentMatch in XmlCommentRegex().Matches(lineText))
        {
            if (position >= commentMatch.Index && position < commentMatch.Index + commentMatch.Length)
            {
                return true;
            }
        }
        return false;
    }
}
