using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Main;

public class LanguageMain
{
    public LanguageMainMenu Menu { get; set; } = new();

    public string CharactersPerSecond { get; set; }
    public string SingleLineLength { get; set; }
    public string TotalCharacters { get; set; }
    public string ExtractingWaveInfo { get; set; }
    public string LoadingWaveInfoFromCache { get; set; }
    public string FailedToExtractWaveInfo { get; set; }
    public string ParsingMatroskaFile { get; set; }
    public string SubtitleImportedFromMatroskaFile { get; set; }
    public string LineXTextAndTimingChanged { get; set; }
    public string LineXTextChangedFromYToZ { get; set; }
    public string LineXTimingChanged { get; set; }

    public LanguageMain()
    {
        CharactersPerSecond = "Chars/second: {0}";
        SingleLineLength = "Line length: ";
        TotalCharacters = "Total chars: {0}";
        ExtractingWaveInfo = "Extracting wave info...";
        LoadingWaveInfoFromCache = "Loading wave info from cache...";
        FailedToExtractWaveInfo = "Failed to extract wave info.";
        ParsingMatroskaFile = "Parsing Matroska file...";
        SubtitleImportedFromMatroskaFile = "Subtitle imported from Matroska file";
        LineXTextAndTimingChanged = "Line {0}: Text and timing changed";
        LineXTextChangedFromYToZ = "Line {0}: Text changed from '{1}' to '{2}'";
        LineXTimingChanged = "Line {0}: Timing changed";
    }
}