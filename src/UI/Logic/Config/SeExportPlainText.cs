using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeExportPlainText
{
    public string TextProcessing { get; set; }
    public bool TextRemoveStyling { get; set; }
    public bool ShowLineNumbers { get; set; }
    public bool AddNewLineAfterLineNumber { get; set; }
    public bool ShowTimeCodes { get; set; }
    public bool AddNewLineAfterTimeCode { get; set; }
    public string SelectedTimeCodeFormats { get; set; }
    public string SelectedTimeCodeSeparator { get; set; }
    public bool AddLineAfterText { get; set; }
    public bool AddLineBetweenSubtitles { get; set; }

    public SeExportPlainText()
    {
        TextProcessing = "None";
        TextRemoveStyling = true;
        ShowLineNumbers = false;
        AddNewLineAfterLineNumber = true;
        ShowTimeCodes = false;
        AddNewLineAfterTimeCode = true;
        SelectedTimeCodeFormats = "hh:mm:ss,ms";
        SelectedTimeCodeSeparator = " --> ";
        AddLineAfterText = true;
        AddLineBetweenSubtitles = true;
    }
}