using Avalonia.Media;
using Avalonia.Skia;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

public interface IColorService
{
    void RemoveColorTags(List<SubtitleLineViewModel> subtitles);
    void SetColor(List<SubtitleLineViewModel> subtitles, Color color, Subtitle subtitle, SubtitleFormat subtitleFormat);
    string SetColorTag(string input, Color color, bool isAssa, bool isWebVtt, Subtitle subtitle);
}

public class ColorService : IColorService
{
    public void RemoveColorTags(List<SubtitleLineViewModel> subtitles)
    {
        foreach (var p in subtitles)
        {
            RemoveColortags(p);
        }
    }

    private static void RemoveColortags(SubtitleLineViewModel p)
    {
        if (!p.Text.Contains("<font", StringComparison.OrdinalIgnoreCase))
        {
            if (p.Text.Contains("\\c") || p.Text.Contains("\\1c"))
            {
                p.Text = HtmlUtil.RemoveAssaColor(p.Text);
            }
        }

        p.Text = HtmlUtil.RemoveColorTags(p.Text);
    }

    public void SetColor(List<SubtitleLineViewModel> subtitles, Color color, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        var isAssa = subtitleFormat is AdvancedSubStationAlpha;
        var isWebVtt = subtitleFormat is WebVTT;

        foreach (var p in subtitles)
        {
            RemoveColortags(p);
            p.Text = SetColorTag(p.Text, color, isAssa, isWebVtt, subtitle);
        }
    }

    public string SetColorTag(string input, Color color, bool isAssa, bool isWebVtt, Subtitle subtitle)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var text = input;
        if (isAssa)
        {
            try
            {
                text = HtmlUtil.RemoveAssaColor(text);
                text = "{\\" + AdvancedSubStationAlpha.GetSsaColorStringForEvent(color.ToSKColor()) + "&}" + text;
            }
            catch
            {
                // ignore
            }

            return text;
        }

        if (isWebVtt)
        {
            try
            {
                var existingStyle = WebVttHelper.GetOnlyColorStyle(color.ToSKColor(), subtitle.Header);
                if (existingStyle != null)
                {
                    text = WebVttHelper.AddStyleToText(text, existingStyle, WebVttHelper.GetStyles(subtitle.Header));
                    text = WebVttHelper.RemoveUnusedColorStylesFromText(text, subtitle.Header);
                }
                else
                {
                    var styleWithColor = WebVttHelper.AddStyleFromColor(color.ToSKColor());
                    subtitle.Header = WebVttHelper.AddStyleToHeader(subtitle.Header, styleWithColor);
                    text = WebVttHelper.AddStyleToText(text, styleWithColor, WebVttHelper.GetStyles(subtitle.Header));
                    text = WebVttHelper.RemoveUnusedColorStylesFromText(text, subtitle.Header);
                }
            }
            catch
            {
                // ignore
            }

            return text;
        }

        string pre = string.Empty;
        if (text.StartsWith("{\\", StringComparison.Ordinal) && text.IndexOf('}') >= 0)
        {
            int endIndex = text.IndexOf('}') + 1;
            pre = text.Substring(0, endIndex);
            text = text.Remove(0, endIndex);
        }

        string s = text;
        if (s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
        {
            int end = s.IndexOf('>');
            if (end > 0)    
            {
                string f = s.Substring(0, end);

                if (f.Contains(" face=", StringComparison.OrdinalIgnoreCase) && !f.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                {
                    var start = s.IndexOf(" face=", StringComparison.OrdinalIgnoreCase);
                    s = s.Insert(start, string.Format(" color=\"{0}\"", color));
                    text = pre + s;
                    return text;
                }

                var colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                if (colorStart >= 0)
                {
                    if (s.IndexOf('"', colorStart + 8) > 0)
                    {
                        end = s.IndexOf('"', colorStart + 8);
                    }

                    s = s.Substring(0, colorStart) + string.Format(" color=\"{0}", color) + s.Substring(end);
                    text = pre + s;
                    return text;
                }
            }
        }

        return $"{pre}<font color=\"{color}\">{text}</font>";
    }
}
