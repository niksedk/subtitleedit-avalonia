namespace Nikse.SubtitleEdit.Logic.Config.Language.Sync;

public class LanguageSync
{
    public string VisualSync { get; set; }
    public string Sync { get; set; }
    public string StartScene { get; set; }
    public string EndScene { get; set; }
    public string PlayTwoSecondsAndBack { get; set; }
    public string FindText { get; set; }

    public LanguageSync()
    {
        VisualSync = "Visual Sync";
        Sync = "Sync";
        StartScene = "Start scene";
        EndScene = "End scene";
        PlayTwoSecondsAndBack = "Play 2 secs & back";
        FindText = "Find text";
    }
}