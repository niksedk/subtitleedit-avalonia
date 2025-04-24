namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageGeneral
{
    public string Title { get; set; }
    public string Version { get; set; }
    public string TranslatedBy { get; set; }
    public string CultureName { get; set; }
    public string Ok { get; set; }
    public string Cancel { get; set; }
    public string Yes { get; set; }
    public string No { get; set; }
    public string Close { get; set; }
    public string Apply { get; set; }
    public string ApplyTo { get; set; }
    public string None { get; set; }
    public string All { get; set; }
    public string Preview { get; set; }
    public string ShowPreview { get; set; }
    public string HidePreview { get; set; }
    public string SubtitleFile { get; set; }
    public string SubtitleFiles { get; set; }
    public string AllFiles { get; set; }
    public string VideoFiles { get; set; }
    public string Images { get; set; }
    public string Fonts { get; set; }
    public string AudioFiles { get; set; }
    public string OpenSubtitle { get; set; }
    public string OpenVideoFile { get; set; }
    public string OpenVideoFileTitle { get; set; }
    public string NoVideoLoaded { get; set; }
    public string OnlineVideoFeatureNotAvailable { get; set; }
    public string VideoInformation { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Duration { get; set; }
    public string CharsPerSec { get; set; }
    public string WordsPerMin { get; set; }
    public string Actor { get; set; }
    public string Gap { get; set; }
    public string Region { get; set; }
    public string Layer { get; set; }
    public string NumberSymbol { get; set; }
    public string Number { get; set; }
    public string Text { get; set; }
    public string HourMinutesSecondsDecimalSeparatorMilliseconds { get; set; }
    public string HourMinutesSecondsFrames { get; set; }
    public string XSeconds { get; set; }
    public string Bold { get; set; }
    public string Italic { get; set; }
    public string Underline { get; set; }
    public string Strikeout { get; set; }
    public string Visible { get; set; }
    public string FrameRate { get; set; }
    public string Name { get; set; }
    public string FileNameXAndSize { get; set; }
    public string ResolutionX { get; set; }
    public string FrameRateX { get; set; }
    public string TotalFramesX { get; set; }
    public string VideoEncodingX { get; set; }
    public string SingleLineLengths { get; set; }
    public string TotalLengthX { get; set; }
    public string TotalLengthXSplitLine { get; set; }
    public string SplitLine { get; set; }
    public string NotAvailable { get; set; }
    public string Overlap { get; set; }
    public string OverlapPreviousLineX { get; set; }
    public string OverlapX { get; set; }
    public string OverlapNextX { get; set; }
    public string OverlapStartAndEnd { get; set; }
    public string Negative { get; set; }
    public string RegularExpressionIsNotValid { get; set; }
    public string CurrentSubtitle { get; set; }
    public string OriginalText { get; set; }
    public string OpenOriginalSubtitleFile { get; set; }
    public string PleaseWait { get; set; }
    public string SessionKey { get; set; }
    public string SessionKeyGenerate { get; set; }
    public string UserName { get; set; }
    public string UserNameAlreadyInUse { get; set; }
    public string WebServiceUrl { get; set; }
    public string IP { get; set; }
    public string Advanced { get; set; }
    public string Style { get; set; }
    public string StyleLanguage { get; set; }
    public string Character { get; set; }
    public string Class { get; set; }
    public string GeneralText { get; set; }
    public string LineNumber { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
    public string Size { get; set; }
    public string Search { get; set; }
    public string DeleteCurrentLine { get; set; }
    public string Width { get; set; }
    public string Height { get; set; }
    public string Collapse { get; set; }
    public string ShortcutX { get; set; }
    public string ExampleX { get; set; }
    public string ViewX { get; set; }
    public string Reset { get; set; }
    public string Error { get; set; }
    public string Warning { get; set; }
    public string UseLargerFontForThisWindow { get; set; }
    public string ChangeLanguageFilter { get; set; }
    public string MoreInfo { get; set; }

    public LanguageGeneral()
    {
        Title = "Subtitle Alchemist";
        Version = "0.0.8.0 Alpha";
        TranslatedBy = " ";
        CultureName = "en-US";
        Ok = "&OK";
        Cancel = "C&ancel";
        Yes = "Yes";
        No = "No";
        Close = "Close";
        Apply = "Apply";
        ApplyTo = "Apply to";
        None = "None";
        All = "All";
        Preview = "Preview";
        ShowPreview = "Show preview";
        HidePreview = "Hide preview";
        SubtitleFile = "Subtitle file";
        SubtitleFiles = "Subtitle files";
        AllFiles = "All files";
        VideoFiles = "Video files";
        Images = "Images";
        Fonts = "Fonts";
        AudioFiles = "Audio files";
        OpenSubtitle = "Open subtitle...";
        OpenVideoFile = "Open video file...";
        OpenVideoFileTitle = "Open video file...";
        NoVideoLoaded = "No video loaded";
        OnlineVideoFeatureNotAvailable = "Feature not available for online video";
        VideoInformation = "Video info";
        StartTime = "Start time";
        EndTime = "End time";
        Duration = "Duration";
        CharsPerSec = "Chars/sec";
        WordsPerMin = "Words/min";
        Actor = "Actor";
        Gap = "Gap";
        Region = "Region";
        Layer = "Layer";
        NumberSymbol = "#";
        Number = "Number";
        Text = "Text";
        HourMinutesSecondsDecimalSeparatorMilliseconds = "Hour:min:sec{0}ms";
        HourMinutesSecondsFrames = "Hour:min:sec:frames";
        XSeconds = "{0:0.0##} seconds";
        Bold = "Bold";
        Italic = "Italic";
        Underline = "Underline";
        Strikeout = "Strikeout";
        Visible = "Visible";
        FrameRate = "Frame rate";
        Name = "Name";
        SingleLineLengths = "Single line length:";
        TotalLengthX = "Total length: {0}";
        TotalLengthXSplitLine = "Total length: {0} (split line!)";
        SplitLine = "Split line!";
        NotAvailable = "N/A";
        FileNameXAndSize = "File name: {0} ({1})";
        ResolutionX = "Resolution: {0}";
        FrameRateX = "Frame rate: {0:0.0###}";
        TotalFramesX = "Total frames: {0:#;##0.##}";
        VideoEncodingX = "Video encoding: {0}";
        Overlap = "Overlap";
        OverlapPreviousLineX = "Overlap prev line ({0:#;##0.###})";
        OverlapX = "Overlap ({0:#;##0.###})";
        OverlapNextX = "Overlap next ({0:#;##0.###})";
        OverlapStartAndEnd = "Overlap start and end";
        Negative = "Negative";
        RegularExpressionIsNotValid = "Regular expression is not valid!";
        CurrentSubtitle = "Current subtitle";
        OriginalText = "Original text";
        OpenOriginalSubtitleFile = "Open original subtitle file...";
        PleaseWait = "Please wait...";
        SessionKey = "Session key";
        SessionKeyGenerate = "Generate new key";
        UserName = "Username";
        UserNameAlreadyInUse = "Username already in use";
        WebServiceUrl = "Webservice URL";
        IP = "IP";
        Advanced = "Advanced";
        Style = "Style";
        StyleLanguage = "Style / Language";
        Character = "Character";
        Class = "Class";
        GeneralText = "General";
        LineNumber = "Line#";
        Before = "Before";
        After = "After";
        Size = "Size";
        Search = "Search";
        DeleteCurrentLine = "Delete current line";
        Width = "Width";
        Height = "Height";
        Collapse = "Collapse";
        ShortcutX = "Shortcut: {0}";
        ExampleX = "Example: {0}";
        ViewX = "View {0}";
        Reset = "Reset";
        Error = "Error";
        Warning = "Warning";
        UseLargerFontForThisWindow = "Use larger font for this window";
        ChangeLanguageFilter = "Change language filter...";
        MoreInfo = "More info";
    }
}