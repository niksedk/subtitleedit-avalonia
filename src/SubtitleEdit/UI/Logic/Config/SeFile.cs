using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFile
{
    public bool ShowRecentFiles { get; set; } = true;
    public int RecentFilesMaximum { get; set; } = 25;
    public List<RecentFile> RecentFiles { get; set; } = new();
    public ExportImages ExportImages { get; set; } = new();

    public void AddToRecentFiles(string subtitleFileName, string videoFileName, int selectedLine, string encoding)
    {
        RecentFiles.RemoveAll(rf => rf.SubtitleFileName == subtitleFileName);
        RecentFiles.Insert(0, new RecentFile { SubtitleFileName = subtitleFileName, VideoFileName = videoFileName, SelectedLine = selectedLine, Encoding = encoding });
        if (RecentFiles.Count > RecentFilesMaximum)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }
}