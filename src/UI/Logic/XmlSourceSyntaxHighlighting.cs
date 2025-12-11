using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Syntax highlighting for XML-based subtitle formats (TTML, DFXP, etc.)
/// </summary>
public partial class XmlSourceSyntaxHighlighting : DocumentColorizingTransformer
{
    // Color scheme
    private static readonly IBrush XmlTagBrush = new SolidColorBrush(Color.Parse("#569CD6")); // Blue for XML tags
    private static readonly IBrush XmlAttributeBrush = new SolidColorBrush(Color.Parse("#9CDCFE")); // Light blue for attributes
    private static readonly IBrush XmlValueBrush = new SolidColorBrush(Color.Parse("#CE9178")); // Orange for attribute values
    private static readonly IBrush CommentBrush = new SolidColorBrush(Color.Parse("#6A9955")); // Green for XML comments

    // XML tags (opening, closing, self-closing)
    [GeneratedRegex(@"</?[a-zA-Z_][\w:.-]*(?:\s+[^>]*)?/?>", RegexOptions.IgnoreCase)]
    private static partial Regex XmlTagRegex();

    // XML attributes
    [GeneratedRegex(@"([a-zA-Z_][\w:.-]*)\s*=\s*([""'])([^""']*)\2", RegexOptions.IgnoreCase)]
    private static partial Regex XmlAttributeRegex();

    // XML comments
    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex XmlCommentRegex();

    protected override void ColorizeLine(DocumentLine line)
    {
        var lineText = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        ColorizeXmlComments(line, lineText);
        ColorizeXmlTags(line, lineText);
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
