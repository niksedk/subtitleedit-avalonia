namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMainMenu
{
    public string File { get; set; }
    public string New { get; set; }
    public string Open { get; set; }
    public string Reopen { get; set; }
    public string ClearRecentFiles { get; set; }
    public string RestoreAutoBackup { get; set; }
    public string Save { get; set; }
    public string SaveAs { get; set; }
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

    public string Synchronization { get; set; }
    public string AdjustAllTimes { get; set; }
    public string ChangeFrameRate { get; set; }
    public string ChangeSpeed { get; set; }


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
        File = "File";
        New = "New";
        Open = "Open...";
        Reopen = "Reopen...";
        ClearRecentFiles = "Clear recent files";
        RestoreAutoBackup = "Restore auto-backup...";
        Save = "Save";
        SaveAs = "Save as...";
        Export = "Export";
        Exit = "Exit";

        Edit = "Edit";
        Undo = "Undo";
        Redo = "Redo";
        ShowHistory = "Show history...";
        Find = "Find...";
        FindNext = "Find next";
        Replace = "Replace...";
        MultipleReplace = "Multiple replace...";
        GoToLineNumber = "Go to line number...";

        Tools = "Tools";
        AdjustDurations = "Adjust durations...";
        FixCommonErrors = "Fix common errors...";
        RemoveTextForHearingImpaired = "Remove text for hearing impaired...";
        ChangeCasing = "Change casing...";
        BatchConvert = "Batch convert...";

        SpellCheckTitle = "Spell check";
        SpellCheck = "Spell check...";
        GetDictionaries = "Get dictionaries...";

        Video = "Video";
        OpenVideo = "Open video...";
        OpenVideoFromUrl = "Open video from URL...";
        CloseVideoFile = "Close video file";
        SpeechToText = "Speech to text...";
        TextToSpeech = "Text to speech...";
        GenerateBurnIn = "Generate video with burned-in subtitles...";
        GenerateTransparent = "Generate transparent video with subtitles...";

        Synchronization = "Synchronization";
        AdjustAllTimes = "Adjust all times...";
        ChangeFrameRate = "Change frame rate...";
        ChangeSpeed = "Change speed...";

        Options = "Options";
        Settings = "Settings...";
        Shortcuts = "Shortcuts...";
        ChooseLanguage = "Choose language...";
        WordLists = "Word lists...";

        Translate = "Translate";
        AutoTranslate = "Auto-translate...";
        
        HelpTitle = "Help";
        Help = "Help...";
        About = "About...";
    }
}