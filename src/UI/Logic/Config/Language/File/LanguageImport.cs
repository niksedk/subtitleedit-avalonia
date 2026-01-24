namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageImport
{
    public string ImportTimeCodes { get; set; }
    public string PlainTextDotDotDot { get; set; }
    public string TitleImportPlainText { get; set; }
    public string ImagesForOcrDotDotDot { get; set; }
    public string TimeCodesDotDotDot { get; set; }
    public string SubtitleWithManuallyChosenEncodingDotDotDot { get; set; }
    public string TitleImportImages { get; set; }
    public string ImportFileLabel { get; set; }
    public string ImportFilesInfo { get; set; }
    public string FormattingDotDotDot { get; set; }
    public string ImagedBasedSubtitleForEditDotDotDot { get;  set; }
    public string ImagedBasedSubtitleForOcrDotDotDot { get;  set; }
    public string SplitTextAt { get; set; }
    public string BlankLines { get; set; }
    public string OneLineIsOneSubtitle { get; set; }
    public string TwoLinesAreOneSubtitle { get; set; }
    public string ImportFilesDotDotDot { get; set; }
    public string MultipleFiles { get; set; }

    public LanguageImport()
    {
        PlainTextDotDotDot = "Plain text...";
        TitleImportPlainText = "Import plain text";
        ImportTimeCodes = "Import time codes...";
        ImagesForOcrDotDotDot = "Images for OCR...";
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
        SplitTextAt = "Split text at";
        BlankLines = "Blank lines";
        OneLineIsOneSubtitle = "One line is one subtitle";
        TwoLinesAreOneSubtitle = "Two lines are one subtitle";
        ImportFilesDotDotDot = "Import files...";
        MultipleFiles = "Import from multiple text files (one file is one subtitle)";
    }
}