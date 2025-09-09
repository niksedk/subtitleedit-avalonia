using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

public interface IFontNameService
{
    void RemoveFontNames(SubtitleLineViewModel p, bool isAssa);
    void RemoveFontNames(List<SubtitleLineViewModel> subtitles, SubtitleFormat subtitleFormat);
    void SetFontName(List<SubtitleLineViewModel> subtitles, string fontName, SubtitleFormat subtitleFormat);
    void SetFontName(SubtitleLineViewModel p, string fontName, bool isAssa);
}

public class FontNameService : IFontNameService
{
    public void RemoveFontNames(List<SubtitleLineViewModel> subtitles, SubtitleFormat subtitleFormat)
    {
        var isAssa = subtitleFormat is AdvancedSubStationAlpha;

        foreach (var p in subtitles)
        {
            RemoveFontNames(p, isAssa);
        }
    }

    public void SetFontName(List<SubtitleLineViewModel> subtitles, string fontName, SubtitleFormat subtitleFormat)
    {
        var isAssa = subtitleFormat is AdvancedSubStationAlpha;

        foreach (var p in subtitles)
        {
            SetFontName(p, fontName, isAssa);
        }
    }

    public void SetFontName(SubtitleLineViewModel p, string fontName, bool isAssa)
    {
        if (string.IsNullOrWhiteSpace(p.Text))
        {
            return;
        }

        if (isAssa)
        {
            p.Text = Regex.Replace(p.Text, "{\\\\fn[^\\\\]+}", string.Empty);
            p.Text = Regex.Replace(p.Text, "\\\\fn[a-zA-Z \\d]+\\\\", string.Empty);
            p.Text = "{\\fn" + fontName + "}" + p.Text;
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
            var end = s.IndexOf('>');
            if (end > 0)
            {
                var f = s.Substring(0, end);

                if (f.Contains(" color=", StringComparison.OrdinalIgnoreCase) && !f.Contains(" face=", StringComparison.OrdinalIgnoreCase))
                {
                    var start = s.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                    p.Text = pre + s.Insert(start, string.Format(" face=\"{0}\"", fontName));
                    return;
                }

                var faceStart = f.IndexOf(" face=", StringComparison.OrdinalIgnoreCase);
                if (f.Contains(" face=", StringComparison.OrdinalIgnoreCase))
                {
                    if (s.IndexOf('"', faceStart + 7) > 0)
                    {
                        end = s.IndexOf('"', faceStart + 7);
                    }

                    p.Text = pre + s.Substring(0, faceStart) + string.Format(" face=\"{0}", fontName) + s.Substring(end);
                    return;
                }
            }
        }

        p.Text = $"{pre}<font face=\"{fontName}\">{s}</font>";
    }

    public void RemoveFontNames(SubtitleLineViewModel p, bool isAssa)
    {

    }
}
