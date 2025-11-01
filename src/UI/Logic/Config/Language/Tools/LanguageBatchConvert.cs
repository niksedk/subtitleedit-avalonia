using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageBatchConvert
{
    public string Title { get; set; }
    public string OneActionsSelected { get; set; }
    public string XActionsSelected { get; set; }

    public LanguageBatchConvert()
    {
        Title = "Batch convert";
        OneActionsSelected = "One action selected";
        XActionsSelected = "{0} actions selected";
    }
}