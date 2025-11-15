using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public static class RtlTextProcessor
{
    /// <summary>
    /// Processes TextSegments for RTL rendering, splitting segments that contain mixed LTR/RTL text
    /// and marking each segment with its directionality.
    /// </summary>
    public static List<TextSegment> ProcessRtlSegments(List<TextSegment> segments, bool isRightToLeft)
    {
        if (!isRightToLeft || segments.Count == 0)
        {
            return segments;
        }

        var result = new List<TextSegment>();
        foreach (var segment in segments)
        {
            result.AddRange(SplitSegmentByDirectionality(segment));
        }

        return result;
    }

    /// <summary>
    /// Splits a segment into multiple segments based on text directionality (LTR vs RTL).
    /// Each resulting segment is marked with IsRightToLeft and RTL segments have their text reversed.
    /// </summary>
    private static List<TextSegment> SplitSegmentByDirectionality(TextSegment segment)
    {
        if (string.IsNullOrEmpty(segment.Text))
        {
            return [segment];
        }

        var result = new List<TextSegment>();
        var currentText = new StringBuilder();
        bool? currentIsLtr = null;

        foreach (char c in segment.Text)
        {
            bool isLtr = IsLtrCharacter(c);

            if (currentIsLtr == null)
            {
                // First character
                currentIsLtr = isLtr;
                currentText.Append(c);
            }
            else if (currentIsLtr == isLtr)
            {
                // Same direction, continue building current segment
                currentText.Append(c);
            }
            else
            {
                // Direction changed, save current segment and start new one
                AddSegment(result, currentText.ToString(), currentIsLtr.Value, segment);
                currentText.Clear();
                currentText.Append(c);
                currentIsLtr = isLtr;
            }
        }

        // Add the last segment
        if (currentText.Length > 0 && currentIsLtr.HasValue)
        {
            AddSegment(result, currentText.ToString(), currentIsLtr.Value, segment);
        }

        return result;
    }

    /// <summary>
    /// Creates a new TextSegment with the given text and directionality, preserving styling from original.
    /// RTL text is reversed.
    /// </summary>
    private static void AddSegment(List<TextSegment> result, string text, bool isLtr, TextSegment originalSegment)
    {
        var finalText = isLtr ? text : ReverseText(text);
        var newSegment = new TextSegment(
            Text: finalText,
            IsItalic: originalSegment.IsItalic,
            IsBold: originalSegment.IsBold,
            Color: originalSegment.Color,
            IsRightToLeft: !isLtr
        );
        result.Add(newSegment);
    }

    /// <summary>
    /// Reverses the text character by character.
    /// </summary>
    private static string ReverseText(string text)
    {
        return string.Join(string.Empty, text.Reverse());
    }

    /// <summary>
    /// Determines if a character should be treated as LTR (Latin letters, numbers, punctuation)
    /// </summary>
    private static bool IsLtrCharacter(char c)
    {
        // Numbers (0-9)
        if (c >= '0' && c <= '9')
        {
            return true;
        }

        // Basic Latin letters (A-Z, a-z)
        if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
        {
            return true;
        }

        // Common punctuation and symbols
        if (c == '.' || c == ',' || c == ':' || c == ';' || c == '!' || c == '?' ||
            c == '(' || c == ')' || c == '[' || c == ']' || c == '{' || c == '}' ||
            c == '+' || c == '-' || c == '*' || c == '/' || c == '=' || c == '%' ||
            c == '@' || c == '#' || c == '$' || c == '&' || c == '_' || c == '|')
        {
            return true;
        }

        // Extended Latin characters (accented letters)
        if (c >= 0x00C0 && c <= 0x00FF)
        {
            return true;
        }

        // Space character - treat as LTR
        if (c == ' ')
        {
            return true;
        }

        return false;
    }
}