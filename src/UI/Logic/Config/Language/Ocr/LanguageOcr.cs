using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Translate;

public class LanguageOcr
{

    public string LinesToDraw { get; set; }
    public string CurrentImage { get; set; }
    public string AutoDrawAgain { get; set; }
    public string StartOcr { get; set; }
    public string PauseOcr { get; set; }
    public string InspectLine { get; set; }
    public string OcrEngine { get; set; }
    public string Database { get; set; }
    public string MaxWrongPixels { get; set; }
    public string NumberOfPixelsIsSpace { get; set; }
    public string InspectImageMatches { get; set; }
    public string ResolutionXYAndTopmarginZ { get; set; }
    public string RunningOcrDotDotDotXY { get; set; }
    public string RunningOcrDotDotDot { get; set; }
    public string AutoSubmitFirstCharacter { get; set; }

    public LanguageOcr()
    {
        LinesToDraw = "Lines to draw";
        CurrentImage = "Current image";
        AutoDrawAgain = "Auto draw again";
        StartOcr = "Start OCR";
        PauseOcr = "Pause OCR";
        InspectLine = "Inspect line...";
        OcrEngine = "OCR Engine";
        Database = "Database";
        MaxWrongPixels = "Max wrong pixels";
        NumberOfPixelsIsSpace = "Number of pixels is space";
        InspectImageMatches = "Inspect image matches";
        ResolutionXYAndTopmarginZ = "Resolution {0}x{1}, top margin {2}";
        RunningOcrDotDotDotXY = "Running OCR... {0}/{1}";
        RunningOcrDotDotDot = "Running OCR...";
        AutoSubmitFirstCharacter = "Auto submit first character";
    }
}