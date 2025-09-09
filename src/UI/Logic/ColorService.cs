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
            SetColorTag(p, color, isAssa, isWebVtt, subtitle);
        }
    }

    private void SetColorTag(SubtitleLineViewModel p, Color color, bool isAssa, bool isWebVtt, Subtitle subtitle)
    {
        if (string.IsNullOrWhiteSpace(p.Text))
        {
            return;
        }

        if (isAssa)
        {
            try
            {
                p.Text = HtmlUtil.RemoveAssaColor(p.Text);
                p.Text = "{\\" + AdvancedSubStationAlpha.GetSsaColorStringForEvent(color.ToSKColor()) + "&}" + p.Text;
            }
            catch
            {
                // ignore
            }

            return;
        }

        if (isWebVtt)
        {
            try
            {
                var existingStyle = WebVttHelper.GetOnlyColorStyle(color.ToSKColor(), subtitle.Header);
                if (existingStyle != null)
                {
                    p.Text = WebVttHelper.AddStyleToText(p.Text, existingStyle, WebVttHelper.GetStyles(subtitle.Header));
                    p.Text = WebVttHelper.RemoveUnusedColorStylesFromText(p.Text, subtitle.Header);
                }
                else
                {
                    var styleWithColor = WebVttHelper.AddStyleFromColor(color.ToSKColor());
                    subtitle.Header = WebVttHelper.AddStyleToHeader(subtitle.Header, styleWithColor);
                    p.Text = WebVttHelper.AddStyleToText(p.Text, styleWithColor, WebVttHelper.GetStyles(subtitle.Header));
                    p.Text = WebVttHelper.RemoveUnusedColorStylesFromText(p.Text, subtitle.Header);
                }
            }
            catch
            {
                // ignore
            }

            return;
        }

        string pre = string.Empty;
        if (p.Text.StartsWith("{\\", StringComparison.Ordinal) && p.Text.IndexOf('}') >= 0)
        {
            int endIndex = p.Text.IndexOf('}') + 1;
            pre = p.Text.Substring(0, endIndex);
            p.Text = p.Text.Remove(0, endIndex);
        }

        string s = p.Text;
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
                    p.Text = pre + s;
                    return;
                }

                var colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                if (colorStart >= 0)
                {
                    if (s.IndexOf('"', colorStart + 8) > 0)
                    {
                        end = s.IndexOf('"', colorStart + 8);
                    }

                    s = s.Substring(0, colorStart) + string.Format(" color=\"{0}", color) + s.Substring(end);
                    p.Text = pre + s;
                    return;
                }
            }
        }

        p.Text = $"{pre}<font color=\"{color}\">{p.Text}</font>";
    }
}
