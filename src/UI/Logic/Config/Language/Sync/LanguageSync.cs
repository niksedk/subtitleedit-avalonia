namespace Nikse.SubtitleEdit.Logic.Config.Language.Sync;

public class LanguageSync
{
    public string VisualSync { get; set; }
    public string Sync { get; set; }
    public string StartScene { get; set; }
    public string EndScene { get; set; }
    public string PlayTwoSecondsAndBack { get; set; }
    public string FindText { get; set; }
    public string ResolutionXDurationYFrameRateZ { get; set; }
    public string StartSceneMustComeBeforeEndScene { get; set; }

    public LanguageSync()
    {
        VisualSync = "Visual Sync";
        Sync = "Sync";
        StartScene = "Start scene";
        EndScene = "End scene";
        PlayTwoSecondsAndBack = "Play 2 secs & back";
        FindText = "Find text";
        ResolutionXDurationYFrameRateZ = "Resolution: {0}, duration: {1}, frame rate: {2}";
        StartSceneMustComeBeforeEndScene = "Start scene must come before end scene";
    }
}