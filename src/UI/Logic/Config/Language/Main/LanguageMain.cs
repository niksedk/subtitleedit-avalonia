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
    public string ParsingMatroskaFile { get; internal set; }
    public string SubtitleImportedFromMatroskaFile { get; internal set; }

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
    }
}