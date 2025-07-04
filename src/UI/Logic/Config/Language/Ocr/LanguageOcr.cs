namespace Nikse.SubtitleEdit.Logic.Config.Language.Translate;

public class LanguageOcr
{
    public string LinesToDraw { get; set; }
    public string CurrentImage { get; set; }
    public string AutoDrawAgain { get; internal set; }

    public LanguageOcr()
    {
        LinesToDraw = "Lines to draw";
        CurrentImage = "Current image";
        AutoDrawAgain = "Auto draw again";
    }
}