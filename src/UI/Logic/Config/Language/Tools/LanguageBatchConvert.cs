using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageBatchConvert
{
    public string Title { get; set; }
    public string OneActionsSelected { get; set; }
    public string XActionsSelected { get; set; }
    public string OutputFolderSource { get; set; }
    public string OutputFolderX { get; set; }
    public string EncodingXOverwriteY { get; set; }
    public string TargetFormatSettings { get; set; }

    public LanguageBatchConvert()
    {
        Title = "Batch convert";
        OneActionsSelected = "One action selected";
        XActionsSelected = "{0} actions selected";
        OutputFolderSource = " Output folder: Source folder";
        OutputFolderX = " Output folder: {0}";
        EncodingXOverwriteY = "Encoding: {0}, overwrite existing files: {1}";
        TargetFormatSettings = "Target format settings";
    }
}