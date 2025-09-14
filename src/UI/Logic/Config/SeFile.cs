using Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFile
{
    public bool ShowRecentFiles { get; set; } = true;
    public int RecentFilesMaximum { get; set; } = 25;
    public List<RecentFile> RecentFiles { get; set; } = new();
    public List<SeExportCustomFormatItem> ExportCustomFormats { get; set; } = new();
    public SeExportImages ExportImages { get; set; } = new();

    public SeFile()
    {
        ExportCustomFormats.Add(new SeExportCustomFormatItem
        {
            Name = "SubRip",
            Extension = "srt",
            FormatHeader = string.Empty,
            FormatText = "{number}" + Environment.NewLine + "{start} --> {end}" + Environment.NewLine + "{text}" + Environment.NewLine,
            FormatFooter = string.Empty,
            FormatTimeCode = "hh:mm:ss,zzz",
            FormatNewLine = string.Empty,
        });
        ExportCustomFormats.Add(new SeExportCustomFormatItem
        {
            Name = "MicroDVD",
            Extension = "sub",
            FormatHeader = string.Empty,
            FormatText = "{{start}}{{end}}{text}",
            FormatFooter = string.Empty,
            FormatTimeCode = "ff",
            FormatNewLine = "||",
        });
    }

    public void AddToRecentFiles(string subtitleFileName, string subtitleFileNameOriginal, string videoFileName, int selectedLine, string encoding)
    {
        RecentFiles.RemoveAll(rf => rf.SubtitleFileName == subtitleFileName && rf.SubtitleFileNameOriginal == subtitleFileNameOriginal);

        RecentFiles.Insert(0, new RecentFile
        {
            SubtitleFileName = subtitleFileName,
            SubtitleFileNameOriginal = subtitleFileNameOriginal,
            VideoFileName = videoFileName,
            SelectedLine = selectedLine,
            Encoding = encoding,
        });

        if (RecentFiles.Count > RecentFilesMaximum)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }
}