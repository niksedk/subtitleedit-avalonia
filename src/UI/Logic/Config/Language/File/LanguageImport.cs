namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageImport
{
    public string ImportTimeCodes { get; set; }
    public string PlainTextDotDotDot { get; set; }
    public string TitleImportPlainText { get; set; }
    public string ImagesDotDotDot { get; set; }
    public string TimeCodesDotDotDot { get; set; }
    public string SubtitleWithManuallyChosenEncodingDotDotDot { get; set; }
    public string TitleImportImages { get; set; }
    public string ImportFileLabel { get; set; }
    public string ImportFilesInfo { get; set; }
    public string FormattingDotDotDot { get; set; }
    public string ImagedBasedSubtitleForEditDotDotDot { get;  set; }
    public string ImagedBasedSubtitleForOcrDotDotDot { get;  set; }


    public LanguageImport()
    {
        PlainTextDotDotDot = "Plain text...";
        TitleImportPlainText = "Import plain text";
        ImportTimeCodes = "Import time codes...";
        ImagesDotDotDot = "Images...";
        TitleImportImages = "Import images";
        TimeCodesDotDotDot = "Time codes...";
        FormattingDotDotDot = "Formatting...";
        SubtitleWithManuallyChosenEncodingDotDotDot = "_Subtitle with manually chosen encoding...";
        ImportFileLabel = "Choose images to import (time codes in file names supported)";
        ImportFilesInfo = @"Use time-coded filenames:
start_HH_MM_SS_MMM__end_HH_MM_SS_MMM[_index].ext

Examples:
0_00_01_042__0_00_03_919_0001.png
0_00_01_042__0_00_03_919.png

Rules:
• HH_MM_SS_MMM for start and end times
• Double underscore separates start/end
• Optional index after end time";
        ImagedBasedSubtitleForEditDotDotDot = "Imaged-based subtitle for edit...";
        ImagedBasedSubtitleForOcrDotDotDot = "Imaged-based subtitle for OCR...";
    }
}