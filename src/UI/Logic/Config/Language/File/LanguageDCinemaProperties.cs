namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageDCinemaProperties
{
    public string Title { get; set; }
    public string TitleSmpte { get; set; }
    public string SubtitleId { get; set; }
    public string GenerateId { get; set; }
    public string MovieTitle { get; set; }
    public string ReelNumber { get; set; }
    public string Language { get; set; }
    public string IssueDate { get; set; }
    public string EditRate { get; set; }
    public string TimeCodeRate { get; set; }
    public string StartTime { get; set; }
    public string Font { get; set; }
    public string FontId { get; set; }
    public string FontUri { get; set; }
    public string FontColor { get; set; }
    public string FontEffect { get; set; }
    public string FontEffectColor { get; set; }
    public string FontSize { get; set; }
    public string TopBottomMargin { get; set; }
    public string FadeUpTime { get; set; }
    public string FadeDownTime { get; set; }
    public string ZPosition { get; set; }
    public string ZPositionHelp { get; set; }
    public string ChooseColor { get; set; }
    public string Generate { get; set; }
    public string GenerateNewIdOnSave { get; set; }

    public LanguageDCinemaProperties()
    { 
        Title = "D-Cinema properties";
        TitleSmpte = "SMPTE D-Cinema properties";
        SubtitleId = "Subtitle ID";
        GenerateId = "Generate new ID";
        MovieTitle = "Movie title";
        ReelNumber = "Reel number";
        Language = "Language";
        IssueDate = "Issue date";
        EditRate = "Edit rate";
        TimeCodeRate = "Time code rate";
        StartTime = "Start time";
        Font = "Font";
        FontId = "Font ID";
        FontUri = "Font URI";
        FontColor = "Font color";
        FontEffect = "Font effect";
        FontEffectColor = "Font effect color";
        FontSize = "Font size (in pixels)";
        TopBottomMargin = "Top/bottom margin (in pixels)";
        FadeUpTime = "Fade up time (in seconds)";
        FadeDownTime = "Fade down time (in seconds)";
        ZPosition = "Z position (0-15)";
        ZPositionHelp = "(0 is closest to the viewer, 15 is furthest away)";
        ChooseColor = "Choose color...";
        Generate = "Generate new ID on save?";
        GenerateNewIdOnSave = "Generate new ID on save?";
    }
}
