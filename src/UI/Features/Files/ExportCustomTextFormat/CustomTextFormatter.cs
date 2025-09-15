using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public static class CustomTextFormatter
{
    public const string EnglishDoNotModify = "[Do not modify]";
    private static readonly Regex CurlyCodePattern = new Regex("{\\d+[,:]*[A-Z\\d-]*}", RegexOptions.Compiled);

    public static string GenerateCustomText(CustomFormatItem customFormat, List<SubtitleLineViewModel> subtitles, string title, string videoFileName)
    {
        var formatNewLine = customFormat.FormatNewLine ?? Environment.NewLine;

        var sb = new StringBuilder();
        sb.Append(GetHeaderOrFooter(title, videoFileName, subtitles, customFormat.FormatHeader));
        var template = GetParagraphTemplate(customFormat.FormatParagraph);
        var isXml = customFormat.FormatHeader.Contains("<?xml version=", StringComparison.OrdinalIgnoreCase);
        for (var i = 0; i < subtitles.Count; i++)
        {
            var p = subtitles[i];
            var start = GetTimeCode(new TimeCode(p.StartTime), customFormat.FormatTimeCode);
            var end = GetTimeCode(new TimeCode(p.EndTime), customFormat.FormatTimeCode);

            var gap = string.Empty;
            var next = subtitles.GetOrNull(i + 1);
            if (next != null)
            {
                gap = GetTimeCode(new TimeCode(next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds), customFormat.FormatTimeCode);
            }

            var text = p.Text;
            if (isXml)
            {
                text = text.Replace("<", "&lt;")
                           .Replace(">", "&gt;")
                           .Replace("&", "&amp;");
            }
            text = GetText(text, formatNewLine);

            var originalText = p.OriginalText;
            var paragraph = GetParagraph(template, start, end, text, originalText, i, p.Actor, new TimeCode(p.Duration), gap, customFormat.FormatTimeCode, p, videoFileName);
            sb.Append(paragraph);
        }
        sb.Append(GetHeaderOrFooter(title, videoFileName, subtitles, customFormat.FormatFooter));
        return sb.ToString();
    }

    public static string GetHeaderOrFooter(string title, string videoFileName, List<SubtitleLineViewModel> subtitles, string template)
    {
        template = template.Replace("{title}", title);
        template = template.Replace("{media-file-name-full}", videoFileName);
        template = template.Replace("{media-file-name}", string.IsNullOrEmpty(videoFileName) ? videoFileName : System.IO.Path.GetFileNameWithoutExtension(videoFileName));
        template = template.Replace("{media-file-name-with-ext}", string.IsNullOrEmpty(videoFileName) ? videoFileName : System.IO.Path.GetFileName(videoFileName));
        template = template.Replace("{#lines}", subtitles.Count.ToString(CultureInfo.InvariantCulture));
        if (template.Contains("{#total-words}"))
        {
            template = template.Replace("{#total-words}", CalculateTotalWords(subtitles).ToString(CultureInfo.InvariantCulture));
        }
        if (template.Contains("{#total-characters}"))
        {
            template = template.Replace("{#total-characters}", CalculateTotalCharacters(subtitles).ToString(CultureInfo.InvariantCulture));
        }

        template = template.Replace("{tab}", "\t");
        return template;
    }

    public static string GetParagraphTemplate(string template)
    {
        var s = template.Replace("{start}", "{0}");
        s = s.Replace("{end}", "{1}");
        s = s.Replace("{text}", "{2}");
        s = s.Replace("{original-text}", "{3}");
        s = s.Replace("{number}", "{4}");
        s = s.Replace("{number:", "{4:");
        s = s.Replace("{number,", "{4,");
        s = s.Replace("{number-1}", "{5}");
        s = s.Replace("{number-1:", "{5:");
        s = s.Replace("{duration}", "{6}");
        s = s.Replace("{actor}", "{7}");
        s = s.Replace("{actor-colon-space}", "{21}");
        s = s.Replace("{actor-upper-brackets-space}", "{22}");
        s = s.Replace("{text-line-1}", "{8}");
        s = s.Replace("{text-line-2}", "{9}");
        s = s.Replace("{cps-comma}", "{10}");
        s = s.Replace("{cps-period}", "{11}");
        s = s.Replace("{text-length}", "{12}");
        s = s.Replace("{text-length-br0}", "{13}");
        s = s.Replace("{text-length-br1}", "{14}");
        s = s.Replace("{text-length-br2}", "{15}");
        s = s.Replace("{gap}", "{16}");
        s = s.Replace("{bookmark}", "{17}");
        s = s.Replace("{media-file-name}", "{18}");
        s = s.Replace("{media-file-name-full}", "{19}");
        s = s.Replace("{media-file-name-with-ext}", "{20}");
        s = s.Replace("{tab}", "\t");
        return s;
    }

    public static string GetText(string text, string newLine)
    {
        if (!string.IsNullOrEmpty(newLine) && newLine != EnglishDoNotModify)
        {
            newLine = newLine.Replace("{newline}", Environment.NewLine);
            newLine = newLine.Replace("{tab}", "\t");
            newLine = newLine.Replace("{lf}", "\n");
            newLine = newLine.Replace("{cr}", "\r");
            return text.Replace(Environment.NewLine, newLine);
        }
        return text;
    }

    public static string GetTimeCode(TimeCode timeCode, string template)
    {
        var t = template;
        var templateTrimmed = t.Trim();
        if (templateTrimmed == "ss")
        {
            t = t.Replace("ss", $"{timeCode.TotalSeconds:00}");
        }

        if (templateTrimmed == "s")
        {
            t = t.Replace("s", $"{(long)Math.Round(timeCode.TotalSeconds, MidpointRounding.AwayFromZero)}");
        }

        if (templateTrimmed == "zzz")
        {
            t = t.Replace("zzz", $"{timeCode.TotalMilliseconds:000}");
        }

        if (templateTrimmed == "z")
        {
            t = t.Replace("z", $"{(long)Math.Round(timeCode.TotalMilliseconds, MidpointRounding.AwayFromZero)}");
        }

        if (templateTrimmed == "ff")
        {
            t = t.Replace("ff", $"{SubtitleFormat.MillisecondsToFrames(timeCode.TotalMilliseconds)}");
        }

        var totalSeconds = (int)Math.Round(timeCode.TotalSeconds, MidpointRounding.AwayFromZero);
        if (t.StartsWith("ssssssss", StringComparison.Ordinal))
        {
            t = t.Replace("ssssssss", $"{totalSeconds:00000000}");
        }

        if (t.StartsWith("sssssss", StringComparison.Ordinal))
        {
            t = t.Replace("sssssss", $"{totalSeconds:0000000}");
        }

        if (t.StartsWith("ssssss", StringComparison.Ordinal))
        {
            t = t.Replace("ssssss", $"{totalSeconds:000000}");
        }

        if (t.StartsWith("sssss", StringComparison.Ordinal))
        {
            t = t.Replace("sssss", $"{totalSeconds:00000}");
        }

        if (t.StartsWith("ssss", StringComparison.Ordinal))
        {
            t = t.Replace("ssss", $"{totalSeconds:0000}");
        }

        if (t.StartsWith("sss", StringComparison.Ordinal))
        {
            t = t.Replace("sss", $"{totalSeconds:000}");
        }

        if (t.StartsWith("ss", StringComparison.Ordinal))
        {
            t = t.Replace("ss", $"{totalSeconds:00}");
        }

        var totalMilliseconds = (long)Math.Round(timeCode.TotalMilliseconds, MidpointRounding.AwayFromZero);
        if (t.StartsWith("zzzzzzzz", StringComparison.Ordinal))
        {
            t = t.Replace("zzzzzzzz", $"{totalMilliseconds:00000000}");
        }

        if (t.StartsWith("zzzzzzz", StringComparison.Ordinal))
        {
            t = t.Replace("zzzzzzz", $"{totalMilliseconds:0000000}");
        }

        if (t.StartsWith("zzzzzz", StringComparison.Ordinal))
        {
            t = t.Replace("zzzzzz", $"{totalMilliseconds:000000}");
        }

        if (t.StartsWith("zzzzz", StringComparison.Ordinal))
        {
            t = t.Replace("zzzzz", $"{totalMilliseconds:00000}");
        }

        if (t.StartsWith("zzzz", StringComparison.Ordinal))
        {
            t = t.Replace("zzzz", $"{totalMilliseconds:0000}");
        }

        if (t.StartsWith("zzz", StringComparison.Ordinal))
        {
            t = t.Replace("zzz", $"{totalMilliseconds:000}");
        }

        t = t.Replace("hh", $"{timeCode.Hours:00}");
        t = t.Replace("h", $"{timeCode.Hours}");
        t = t.Replace("mm", $"{timeCode.Minutes:00}");
        t = t.Replace("m", $"{timeCode.Minutes}");
        t = t.Replace("ss", $"{timeCode.Seconds:00}");
        t = t.Replace("s", $"{timeCode.Seconds}");
        t = t.Replace("zzz", $"{timeCode.Milliseconds:000}");
        t = t.Replace("zz", $"{Math.Round(timeCode.Milliseconds / 10.0):00}");
        t = t.Replace("z", $"{Math.Round(timeCode.Milliseconds / 100.0):0}");
        t = t.Replace("ff", $"{SubtitleFormat.MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}");
        t = t.Replace("f", $"{SubtitleFormat.MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds)}");

        if (timeCode.TotalMilliseconds < 0)
        {
            return "-" + t.RemoveChar('-');
        }

        return t;
    }

    internal static string GetParagraph(string template, string start, string end, string text, string originalText, int number, string actor, TimeCode duration, string gap, string timeCodeTemplate, SubtitleLineViewModel p, string videoFileName)
    {
        var cps = p.GetCharactersPerSecond();
        var d = duration.ToString();
        if (timeCodeTemplate == "ff" || timeCodeTemplate == "f")
        {
            d = SubtitleFormat.MillisecondsToFrames(duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
        }

        if (timeCodeTemplate == "zzz" || timeCodeTemplate == "zz" || timeCodeTemplate == "z")
        {
            d = ((long)Math.Round(duration.TotalMilliseconds, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
        }

        if (timeCodeTemplate == "sss" || timeCodeTemplate == "ss" || timeCodeTemplate == "s")
        {
            d = duration.Seconds.ToString(CultureInfo.InvariantCulture);
        }
        else if (timeCodeTemplate.EndsWith("ss.ff", StringComparison.Ordinal))
        {
            if (duration.Minutes > 0 && timeCodeTemplate.EndsWith("mm:ss.ff"))
            {
                d = $"{duration.Minutes:00}:{duration.Seconds:00}.{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            }
            else
            {
                d = $"{duration.Seconds:00}.{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            }
        }
        else if (timeCodeTemplate.EndsWith("ss:ff", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00}:{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
        }
        else if (timeCodeTemplate.EndsWith("ss,ff", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00},{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
        }
        else if (timeCodeTemplate.EndsWith("ss;ff", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00};{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
        }
        else if (timeCodeTemplate.EndsWith("ss.zzz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00}.{duration.Milliseconds:000}";
        }
        else if (timeCodeTemplate.EndsWith("ss:zzz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00}:{duration.Milliseconds:000}";
        }
        else if (timeCodeTemplate.EndsWith("ss,zzz", StringComparison.Ordinal))
        {
            if (duration.Minutes > 0 && timeCodeTemplate.EndsWith("mm:ss,zzz"))
            {
                d = $"{duration.Minutes:00}:{duration.Seconds:00},{duration.Milliseconds:000}";
            }
            else
            {
                d = $"{duration.Seconds:00},{duration.Milliseconds:000}";
            }
        }
        else if (timeCodeTemplate.EndsWith("ss;zzz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00};{duration.Milliseconds:000}";
        }
        else if (timeCodeTemplate.EndsWith("ss.zz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00}.{Math.Round(duration.Milliseconds / 10.0):00}";
        }
        else if (timeCodeTemplate.EndsWith("ss:zz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00}:{Math.Round(duration.Milliseconds / 10.0):00}";
        }
        else if (timeCodeTemplate.EndsWith("ss,zz", StringComparison.Ordinal))
        {
            if (duration.Minutes > 0 && timeCodeTemplate.EndsWith("mm:ss,zz"))
            {
                d = $"{duration.Minutes:00}:{duration.Seconds:00},{Math.Round(duration.Milliseconds / 10.0):00}";
            }
            else
            {
                d = $"{duration.Seconds:00},{Math.Round(duration.Milliseconds / 10.0):00}";
            }
        }
        else if (timeCodeTemplate.EndsWith("ss;zz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00};{Math.Round(duration.Milliseconds / 10.0):00}";
        }

        var lines = text.SplitToLines();
        var line1 = string.Empty;
        var line2 = string.Empty;
        if (lines.Count > 0)
        {
            line1 = lines[0];
        }

        if (lines.Count > 1)
        {
            line2 = lines[1];
        }

        var s = template;
        var replaceStart = GetReplaceChar(s);
        var replaceEnd = GetReplaceChar(s + replaceStart);
        var actorColonSpace = string.IsNullOrEmpty(actor) ? string.Empty : $"{actor}: ";
        var actorUppercaseBracketsSpace = string.IsNullOrEmpty(actor) ? string.Empty : $"[{actor.ToUpperInvariant()}] ";
        s = PreBeginCurly(s, replaceStart);
        s = PreEndCurly(s, replaceEnd);
        s = string.Format(s, start, end, text, originalText, number + 1, number, d, actor, line1, line2,
                          cps.ToString(CultureInfo.InvariantCulture).Replace(".", ","),
                          cps.ToString(CultureInfo.InvariantCulture),
                          text.Length,
                          p.Text.RemoveChar('\r', '\n').Length,
                          p.Text.RemoveChar('\r', '\n').Length + lines.Count - 1,
                          p.Text.RemoveChar('\r', '\n').Length + (lines.Count - 1) * 2,
                          gap,
                          p.Bookmark == string.Empty ? "*" : p.Bookmark,
                          string.IsNullOrEmpty(videoFileName) ? string.Empty : System.IO.Path.GetFileNameWithoutExtension(videoFileName),
                          videoFileName,
                          string.IsNullOrEmpty(videoFileName) ? string.Empty : System.IO.Path.GetFileName(videoFileName),
                          actorColonSpace,
                          actorUppercaseBracketsSpace
                          );
        s = PostCurly(s, replaceStart, replaceEnd);
        return s;
    }

    private static string GetReplaceChar(string s)
    {
        var chars = new List<char> { '@', '¤', '%', '=', '+', 'æ', 'Æ', '`', '*', ';' };

        foreach (var c in chars)
        {
            if (!s.Contains(c))
            {
                return c.ToString();
            }
        }

        return string.Empty;
    }

    private static string PreBeginCurly(string s, string replaceStart)
    {
        if (string.IsNullOrEmpty(replaceStart))
        {
            return s;
        }

        var indices = GetCurlyBeginIndexesReversed(s);
        for (var i = 0; i < indices.Count; i++)
        {
            var idx = indices[i];
            s = s.Remove(idx, 1);
            s = s.Insert(idx, replaceStart);
        }

        return s;
    }

    private static string PreEndCurly(string s, string replaceEnd)
    {
        if (string.IsNullOrEmpty(replaceEnd))
        {
            return s;
        }

        var indices = GetCurlyEndIndexesReversed(s);
        for (var i = 0; i < indices.Count; i++)
        {
            var idx = indices[i];
            s = s.Remove(idx, 1);
            s = s.Insert(idx, replaceEnd);
        }

        return s;
    }

    private static string PostCurly(string s, string replaceStart, string replaceEnd)
    {
        if (!string.IsNullOrEmpty(replaceStart))
        {
            s = s.Replace(replaceStart, "{");
        }

        if (!string.IsNullOrEmpty(replaceEnd))
        {
            s = s.Replace(replaceEnd, "}");
        }

        return s;
    }

    private static List<int> GetCurlyBeginIndexesReversed(string s)
    {
        var matchIndices = CurlyCodePattern.Matches(s)
            .Cast<Match>()
            .Select(m => m.Index)
            .ToList();
        var list = new List<int>();
        for (var i = s.Length - 1; i >= 0; i--)
        {
            var c = s[i];
            if (c == '{' && !matchIndices.Contains(i))
            {
                list.Add(i);
            }
        }

        return list;
    }

    private static List<int> GetCurlyEndIndexesReversed(string s)
    {
        var matchIndices = CurlyCodePattern.Matches(s)
            .Cast<Match>()
            .Select(m => m.Index + m.Length - 1)
            .ToList();
        var list = new List<int>();
        for (var i = s.Length - 1; i >= 0; i--)
        {
            var c = s[i];
            if (c == '}' && !matchIndices.Contains(i))
            {
                list.Add(i);
            }
        }

        return list;
    }

    private static int CalculateTotalWords(List<SubtitleLineViewModel> subtitles)
    {
        var wordCount = 0;
        foreach (var p in subtitles)
        {
            wordCount += p.Text.CountWords();
        }

        return wordCount;
    }

    private static int CalculateTotalCharacters(List<SubtitleLineViewModel> subtitles)
    {
        decimal characterCount = 0;
        foreach (var p in subtitles)
        {
            characterCount += p.Text.CountCharacters(false);
        }

        return (int)characterCount;
    }
}
