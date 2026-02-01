using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeFormatting;

public static class FormattingReplacer
{
    public static string Replace(string text, ChangeFormattingType from, ChangeFormattingType to, Color color, SubtitleFormat format)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var isAssa = format is AdvancedSubStationAlpha;
        var result = text;

        // Step 1: Remove the "from" formatting
        result = RemoveFormatting(result, from, isAssa, color);

        // Step 2: Apply the "to" formatting
        result = ApplyFormatting(result, to, isAssa, color);

        return result;
    }

    private static string RemoveFormatting(string text, ChangeFormattingType formattingType, bool isAssa, Color color)
    {
        return formattingType switch
        {
            ChangeFormattingType.Italic => HtmlUtil.TagOff(text, HtmlUtil.TagItalic, true, isAssa),
            ChangeFormattingType.Bold => HtmlUtil.TagOff(text, HtmlUtil.TagBold, true, isAssa),
            ChangeFormattingType.Underline => HtmlUtil.TagOff(text, HtmlUtil.TagUnderline, true, isAssa),
            ChangeFormattingType.Color => RemoveColor(text, isAssa),
            _ => text
        };
    }

    private static string ApplyFormatting(string text, ChangeFormattingType formattingType, bool isAssa, Color color)
    {
        return formattingType switch
        {
            ChangeFormattingType.Italic => HtmlUtil.TagOn(text, HtmlUtil.TagItalic, true, isAssa),
            ChangeFormattingType.Bold => HtmlUtil.TagOn(text, HtmlUtil.TagBold, true, isAssa),
            ChangeFormattingType.Underline => HtmlUtil.TagOn(text, HtmlUtil.TagUnderline, true, isAssa),
            ChangeFormattingType.Color => ApplyColor(text, isAssa, color),
            _ => text
        };
    }

    private static string RemoveColor(string text, bool isAssa)
    {
        if (isAssa)
        {
            return HtmlUtil.RemoveAssaColor(text);
        }

        return HtmlUtil.RemoveColorTags(text);
    }

    private static string ApplyColor(string text, bool isAssa, Color color)
    {
        if (isAssa)
        {
            try
            {
                text = HtmlUtil.RemoveAssaColor(text);
                var skColor = new SkiaSharp.SKColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
                text = "{\\" + AdvancedSubStationAlpha.GetSsaColorStringForEvent(skColor) + "&}" + text;
            }
            catch
            {
                // ignore
            }

            return text;
        }

        // For HTML-based formats
        var pre = string.Empty;
        if (text.StartsWith("{\\", StringComparison.Ordinal) && text.IndexOf('}') >= 0)
        {
            int endIndex = text.IndexOf('}') + 1;
            pre = text.Substring(0, endIndex);
            text = text.Remove(0, endIndex);
        }

        return $"{pre}<font color=\"{ToHex(color)}\">{text}</font>";
    }

    private static string ToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
