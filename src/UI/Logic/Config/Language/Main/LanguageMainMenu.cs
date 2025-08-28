namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMainMenu
{
    public string File { get; set; }
    public string New { get; set; }
    public string Open { get; set; }
    public string OpenOriginal { get; set; }
    public string CloseOriginal { get; set; }
    public string Reopen { get; set; }
    public string ClearRecentFiles { get; set; }
    public string RestoreAutoBackup { get; set; }
    public string Save { get; set; }
    public string SaveAs { get; set; }
    public string Compare { get; set; }
    public string Statistics { get; set; }
    public string Export { get; set; }
    public string Exit { get; set; }

    public string Edit { get; set; }
    public string Undo { get; set; }
    public string Redo { get; set; }
    public string ShowHistory { get; set; }
    public string Find { get; set; }
    public string FindNext { get; set; }
    public string Replace { get; set; }
    public string MultipleReplace { get; set; }
    public string GoToLineNumber { get; set; }


    public string Tools { get; set; }
    public string AdjustDurations { get; set; }
    public string FixCommonErrors { get; set; }
    public string RemoveTextForHearingImpaired { get; set; }
    public string ChangeCasing { get; set; }
    public string BridgeGaps { get; set; }
    public string BatchConvert { get; set; }

    public string SpellCheckTitle { get; set; }
    public string SpellCheck { get; set; }
    public string GetDictionaries { get; set; }


    public string Video { get; set; }
    public string OpenVideo { get; set; }
    public string OpenVideoFromUrl { get; set; }
    public string CloseVideoFile { get; set; }
    public string SpeechToText { get; set; }
    public string TextToSpeech { get; set; }
    public string GenerateBurnIn { get; set; }
    public string GenerateTransparent { get; set; }
    public object UndockVideoControls { get; set; }
    public object DockVideoControls { get; set; }

    public string Synchronization { get; set; }
    public string AdjustAllTimes { get; set; }
    public string ChangeFrameRate { get; set; }
    public string ChangeSpeed { get; set; }
    public string VisualSync { get; set; }


    public string Options { get; set; }
    public string Settings { get; set; }
    public string Shortcuts { get; set; }
    public string ChooseLanguage { get; set; }
    public string WordLists { get; set; }

    public string Translate { get; set; }
    public string AutoTranslate { get; set; }

    public string HelpTitle { get; set; }
    public string Help { get; set; }
    public string About { get; set; }

    public LanguageMainMenu()
    {
        File = "_File";
        New = "_New";
        Open = "_Open...";
        OpenOriginal = "Open ori_ginal...";
        CloseOriginal = "_Close original";
        Reopen = "_Reopen...";
        ClearRecentFiles = "_Clear recent files";
        RestoreAutoBackup = "Restore auto-_backup...";
        Save = "_Save";
        SaveAs = "Save _as...";
        Compare = "Com_pare...";
        Statistics = "Stat_istics...";
        Export = "_Export";
        Exit = "E_xit";

        Edit = "_Edit";
        Undo = "_Undo";
        Redo = "Re_do";
        ShowHistory = "_Show history for undo...";
        Find = "_Find...";
        FindNext = "Find _next";
        Replace = "_Replace...";
        MultipleReplace = "_Multiple replace...";
        GoToLineNumber = "_Go to line number...";

        Tools = "_Tools";
        AdjustDurations = "_Adjust durations...";
        FixCommonErrors = "_Fix common errors...";
        RemoveTextForHearingImpaired = "_Remove text for hearing impaired...";
        ChangeCasing = "_Change casing...";
        BridgeGaps = "Bridge _gaps...";
        BatchConvert = "_Batch convert...";

        SpellCheckTitle = "_Spell check";
        SpellCheck = "_Spell check...";
        GetDictionaries = "_Get dictionaries...";

        Video = "_Video";
        OpenVideo = "_Open video...";
        OpenVideoFromUrl = "Open video from _URL...";
        CloseVideoFile = "_Close video file";
        SpeechToText = "_Speech to text...";
        TextToSpeech = "_Text to speech...";
        UndockVideoControls = "_Undock video controls";
        DockVideoControls = "_Dock video controls";
        GenerateBurnIn = "Generate video with _burned-in subtitles...";
        GenerateTransparent = "_Generate transparent video with subtitles...";

        Synchronization = "S_ynchronization";
        AdjustAllTimes = "_Adjust all times...";
        VisualSync = "_Visual sync...";
        ChangeFrameRate = "Change _frame rate...";
        ChangeSpeed = "Change _speed...";

        Options = "_Options";
        Settings = "_Settings...";
        Shortcuts = "S_hortcuts...";
        ChooseLanguage = "_Choose language...";
        WordLists = "_Word lists...";

        Translate = "Tr_anslate";
        AutoTranslate = "_Auto-translate...";
        
        HelpTitle = "_Help";
        Help = "_Help...";
        About = "_About...";
    }
}