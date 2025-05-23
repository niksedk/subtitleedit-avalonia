using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config.Language;

namespace Nikse.SubtitleEdit.Logic.Config;

public class Se
{
    public SeGeneral General { get; set; } = new();
    public List<SeShortCut> Shortcuts { get; set; }
    public SeFile File { get; set; } = new();
    public SeTools Tools { get; set; } = new();
    public SeAutoTranslate AutoTranslate { get; set; } = new();
    public SeSync Synchronization { get; set; } = new();
    public SeSpellCheck SpellCheck { get; set; } = new();
    public SeAppearance Appearance { get; set; } = new();
    public SeVideo Video { get; set; } = new();
    public SeOcr Ocr { get; set; } = new();
    public static SeLanguage Language { get; set; } = new();
    public static Se Settings { get; set; } = new();

    private static string _baseFolder = string.Empty;

    public static string BaseFolder
    {
        get
        {
            if (string.IsNullOrEmpty(_baseFolder))
            {
                _baseFolder = AppContext.BaseDirectory;
            }

            return _baseFolder;
        }
    }

    public static string DictionariesFolder => Path.Combine(BaseFolder, "Dictionaries");
    public static string AutoBackupFolder => Path.Combine(BaseFolder, "AutoBackup");
    public static string TtsFolder => Path.Combine(BaseFolder, "TTS");
    public static string OcrFolder => Path.Combine(BaseFolder, "OCR");
    public static string PaddleOcrFolder => Path.Combine(BaseFolder, "PaddleOCR");
    public static string TesseractFolder => Path.Combine(BaseFolder, "Tesseract550");
    public static string TesseractModelFolder => Path.Combine(TesseractFolder, "tessdata");
    public static string FfmpegFolder => Path.Combine(BaseFolder, "ffmpeg");
    public static string WhisperFolder => Path.Combine(BaseFolder, "Whisper");

    public Se()
    {
        Shortcuts = new List<SeShortCut>();
    }

    public void InitializeMainShortcuts(MainViewModel vm)
    {
        if (Shortcuts.Count > 0)
        {
            return;
        }

        Shortcuts =
        [
            new(nameof(vm.UndoCommand), new List<string> { "Control", "Z" }),
            new(nameof(vm.RedoCommand), new List<string> { "Control", "Y" }),
            new(nameof(vm.ShowGoToLineCommand), new List<string> { "Control", "G" }),
            new(nameof(vm.GoToPreviousLineCommand), new List<string> { "Alt", "Up" }),
            new(nameof(vm.GoToNextLineCommand), new List<string> { "Alt", "Down" }),
            new(nameof(vm.SelectAllLinesCommand), new List<string> { "Control", "A" }, ShortcutCategory.SubtitleGrid),
            new(nameof(vm.InverseSelectionCommand), new List<string> { "Control", "Shift", "I" }, ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ToggleLinesItalicCommand), new List<string> { "Control", "I" }, ShortcutCategory.SubtitleGrid),
            new(nameof(vm.DeleteSelectedItems), new List<string> { "Del" }, ShortcutCategory.SubtitleGrid),
        ];
    }

    public static void SaveSettings()
    {
        var settingsFileName = Path.Combine(BaseFolder, "Settings.json");
        SaveSettings(settingsFileName);
    }

    public static void SaveSettings(string settingsFileName)
    {
        var settings = Settings;
        var json = JsonSerializer.Serialize(settings);
        System.IO.File.WriteAllText(settingsFileName, json);

        UpdateLibSeSettings();
    }

    public static void LoadSettings()
    {
        var settingsFileName = Path.Combine(BaseFolder, "Settings.json");
        LoadSettings(settingsFileName);
    }

    public static void LoadSettings(string settingsFileName)
    {
        if (System.IO.File.Exists(settingsFileName))
        {
            var json = System.IO.File.ReadAllText(settingsFileName);

            try
            {
                Settings = JsonSerializer.Deserialize<Se>(json)!;
            }
            catch
            {
                Settings = new Se();
            }

            SetDefaultValues();

            UpdateLibSeSettings();
        }
    }

    private static void SetDefaultValues()
    {
        if (Settings.Tools == null)
        {
            Settings.Tools = new();
        }

        if (Settings.AutoTranslate == null)
        {
            Settings.AutoTranslate = new();
        }

        if (Settings.File == null)
        {
            Settings.File = new();
        }

        if (Settings.General == null)
        {
            Settings.General = new();
        }

        if (Settings.Synchronization == null)
        {
            Settings.Synchronization = new();
        }

        if (Settings.SpellCheck == null)
        {
            Settings.SpellCheck = new();
        }

        if (Settings.Appearance == null)
        {
            Settings.Appearance = new();
        }

        if (Settings.Video == null)
        {
            Settings.Video = new();
        }

        if (Settings.Ocr == null)
        {
            Settings.Ocr = new();
        }

        if (Settings.Tools.FixCommonErrors.Profiles.Count == 0)
        {
            Settings.Tools.FixCommonErrors.Profiles.Add(new SeFixCommonErrorsProfile
            {
                ProfileName = "Default",
                SelectedRules = new()
                {
                    nameof(FixEmptyLines),
                    nameof(FixOverlappingDisplayTimes),
                    nameof(FixLongDisplayTimes),
                    nameof(FixShortDisplayTimes),
                    nameof(FixShortGaps),
                    nameof(FixInvalidItalicTags),
                    nameof(FixUnneededSpaces),
                    nameof(FixMissingSpaces),
                    nameof(FixUnneededPeriods),
                },
            });
            Settings.Tools.FixCommonErrors.LastProfileName = "Default";
        }
    }

    public static void WriteWhisperLog(string log)
    {
        try
        {
            var filePath = GetWhisperLogFilePath();
            using var writer = new StreamWriter(filePath, true, Encoding.UTF8);
            writer.WriteLine("-----------------------------------------------------------------------------");
            writer.WriteLine($"Date: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
            writer.WriteLine($"SE: {GetSeInfo()}");
            writer.WriteLine(log);
            writer.WriteLine();
        }
        catch
        {
            // ignore
        }
    }

    private static string GetSeInfo()
    {
        try
        {
            return
                $"{System.Reflection.Assembly.GetEntryAssembly()!.GetName().Version} - {Environment.OSVersion} - {IntPtr.Size * 8}-bit";
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string GetWhisperLogFilePath()
    {
        return Path.Combine(BaseFolder, "whisper_log.txt");
    }

    private static void UpdateLibSeSettings()
    {
        Configuration.Settings.General.FFmpegLocation = Settings.General.FfmpegPath;
        Configuration.Settings.General.UseDarkTheme = Settings.Appearance.Theme == "Dark";
        Configuration.Settings.General.UseTimeFormatHHMMSSFF = Settings.General.UseTimeFormatHhMmSsFf;

        var tts = Settings.Tools.AudioToText;
        Configuration.Settings.Tools.WhisperChoice = tts.WhisperChoice;
        Configuration.Settings.Tools.WhisperIgnoreVersion = tts.WhisperIgnoreVersion;
        Configuration.Settings.Tools.WhisperDeleteTempFiles = tts.WhisperDeleteTempFiles;
        Configuration.Settings.Tools.WhisperModel = tts.WhisperModel;
        Configuration.Settings.Tools.WhisperLanguageCode = tts.WhisperLanguageCode;
        Configuration.Settings.Tools.WhisperLocation = tts.WhisperLocation;
        Configuration.Settings.Tools.WhisperCtranslate2Location = tts.WhisperCtranslate2Location;
        Configuration.Settings.Tools.WhisperPurfviewFasterWhisperLocation = tts.WhisperPurfviewFasterWhisperLocation;
        Configuration.Settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd =
            tts.WhisperPurfviewFasterWhisperDefaultCmd;
        Configuration.Settings.Tools.WhisperXLocation = tts.WhisperXLocation;
        Configuration.Settings.Tools.WhisperStableTsLocation = tts.WhisperStableTsLocation;
        Configuration.Settings.Tools.WhisperCppModelLocation = tts.WhisperCppModelLocation;
        Configuration.Settings.Tools.WhisperExtraSettings = tts.WhisperCustomCommandLineArguments;
        Configuration.Settings.Tools.WhisperExtraSettingsHistory = tts.WhisperExtraSettingsHistory;
        Configuration.Settings.Tools.WhisperAutoAdjustTimings = tts.WhisperAutoAdjustTimings;
        Configuration.Settings.Tools.WhisperUseLineMaxChars = tts.WhisperUseLineMaxChars;
        Configuration.Settings.Tools.WhisperPostProcessingAddPeriods = tts.WhisperPostProcessingAddPeriods;
        Configuration.Settings.Tools.WhisperPostProcessingMergeLines = tts.WhisperPostProcessingMergeLines;
        Configuration.Settings.Tools.WhisperPostProcessingSplitLines = tts.WhisperPostProcessingSplitLines;
        Configuration.Settings.Tools.WhisperPostProcessingFixCasing = tts.WhisperPostProcessingFixCasing;
        Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration = tts.WhisperPostProcessingFixShortDuration;
        Configuration.Settings.Tools.VoskPostProcessing = tts.PostProcessing;

        Configuration.Settings.Tools.AutoTranslateLastName = Settings.AutoTranslate.AutoTranslateLastName;
    }
}