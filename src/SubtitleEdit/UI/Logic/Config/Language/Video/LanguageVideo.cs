namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageVideo
{
    public LanguageBurnIn VideoBurnIn { get; set; } = new();
    public LanguageTransparentVideo VideoTransparent { get; set; } = new();


    public LanguageVideo()
    {

    }
}