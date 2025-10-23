using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic;

public static class ShortcutsMain
{
    public static List<ShortCut> GetUsedShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<ShortCut>();
        var keys = Se.Settings.Shortcuts.ToDictionary(p => p.ActionName, p => p);
        foreach (var shortcut in GetAllAvailableShortcuts(vm))
        {
            if (keys.TryGetValue(shortcut.Name, out var match))
            {
                shortcuts.Add(new ShortCut(shortcut, match));
            }
        }

        return shortcuts;
    }

    public static List<ShortCut> GetAllShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<ShortCut>();
        var keys = Se.Settings.Shortcuts.ToDictionary(p => p.ActionName, p => p);
        foreach (var shortcut in GetAllAvailableShortcuts(vm))
        {
            shortcuts.Add(keys.TryGetValue(shortcut.Name, out var match)
                ? new ShortCut(shortcut, match)
                : new ShortCut(shortcut));
        }

        return shortcuts;
    }

    private static void AddShortcut<T>(IList<AvailableShortcut> list, T command, string name, ShortcutCategory category)
    where T : IRelayCommand
    {
        list.Add(new AvailableShortcut(command, name, category));
    }

    public static readonly Dictionary<string, string> CommandTranslationLookup = new Dictionary<string, string>
    {
        { nameof(MainViewModel.DeleteSelectedLinesCommand), Se.Language.Options.Shortcuts.ListDeleteSelection },
        { nameof(MainViewModel.DuplicateSelectedLinesCommand), Se.Language.Options.Shortcuts.DuplicateSelectedLines},
        { nameof(MainViewModel.ShowAlignmentPickerCommand), Se.Language.Options.Shortcuts.ShowAlignmentPicker},
        { nameof(MainViewModel.AddOrEditBookmarkCommand), Se.Language.Options.Shortcuts.AddOrEditBookmark},
        { nameof(MainViewModel.ToggleBookmarkSelectedLinesNoTextCommand), Se.Language.Options.Shortcuts.ToggleBookmark},
        { nameof(MainViewModel.CopyTextFromOriginalToTranslationCommand), Se.Language.Options.Shortcuts.CopyTextFromOriginalSelectedLines},
        { nameof(MainViewModel.SwitchOriginalAndTranslationTextSelectedLinesCommand), Se.Language.Options.Shortcuts.SwitchOriginalAndTranslationSelectedLines},
        { nameof(MainViewModel.MergeOriginalIntoTranslationSelectedLinesCommand), Se.Language.Options.Shortcuts.MergeOriginalIntoTranslationSelectedLines},
        { nameof(MainViewModel.ListBookmarksCommand), Se.Language.Options.Shortcuts.ListBookmarks},
        { nameof(MainViewModel.GoToNextBookmarkCommand), Se.Language.Options.Shortcuts.GoToNextBookmark},
        { nameof(MainViewModel.OpenDataFolderCommand), Se.Language.Options.Shortcuts.OpenSeDataFolder },
        { nameof(MainViewModel.ToggleIsWaveformToolbarVisibleCommand), Se.Language.Options.Shortcuts.ToggleWaveformToolbar },

        // File
        { nameof(MainViewModel.CommandFileOpenCommand), Se.Language.Options.Shortcuts.FileOpen },
        { nameof(MainViewModel.CommandFileOpenKeepVideoCommand), Se.Language.Options.Shortcuts.FileOpenKeepVideo },
        { nameof(MainViewModel.FileOpenOriginalCommand), Se.Language.Options.Shortcuts.FileOpenOriginal },
        { nameof(MainViewModel.FileCloseOriginalCommand), Se.Language.Options.Shortcuts.FileCloseOriginal },
        { nameof(MainViewModel.CommandExitCommand), Se.Language.Options.Shortcuts.FileExit },
        { nameof(MainViewModel.CommandFileNewCommand), Se.Language.Options.Shortcuts.FileNew },
        { nameof(MainViewModel.CommandFileNewKeepVideoCommand), Se.Language.Options.Shortcuts.CommandFileNewKeepVideo },
        { nameof(MainViewModel.CommandFileSaveCommand), Se.Language.Options.Shortcuts.FileSave },
        { nameof(MainViewModel.CommandFileSaveAsCommand), Se.Language.Options.Shortcuts.FileSaveAs },
        { nameof(MainViewModel.ShowStatisticsCommand), Se.Language.Options.Shortcuts.FileStatistics },
        { nameof(MainViewModel.ShowCompareCommand), Se.Language.Options.Shortcuts.FileCompare },
        { nameof(MainViewModel.ShowRestoreAutoBackupCommand), Se.Language.Options.Shortcuts.RestoreAutoBackup },
        { nameof(MainViewModel.OpenContainingFolderCommand), Se.Language.Options.Shortcuts.OpenContainingFolder },
        { nameof(MainViewModel.ImportTimeCodesCommand), Se.Language.Options.Shortcuts.ImportTimeCodes },
        { nameof(MainViewModel.ShowImportSubtitleWithManuallyChosenEncodingCommand), Se.Language.Options.Shortcuts.ImportSubtitleWithManuallyChosenEncoding },
        { nameof(MainViewModel.ExportBluRaySupCommand), Se.Language.Options.Shortcuts.ExportBluRaySup },
        { nameof(MainViewModel.ShowExportCustomTextFormatCommand), Se.Language.Options.Shortcuts.ExportCustomTextFormat },
        { nameof(MainViewModel.ShowExportPlainTextCommand), Se.Language.Options.Shortcuts.ExportPlainText },

        // Edit
        { nameof(MainViewModel.UndoCommand), Se.Language.General.Undo },
        { nameof(MainViewModel.RedoCommand), Se.Language.General.Redo },
        { nameof(MainViewModel.ShowHistoryCommand), Se.Language.Options.Shortcuts.ShowHistory },
        { nameof(MainViewModel.ShowFindCommand), Se.Language.Options.Shortcuts.EditFind },
        { nameof(MainViewModel.FindNextCommand), Se.Language.Options.Shortcuts.EditFindNext },
        { nameof(MainViewModel.FindPreviousCommand), Se.Language.Options.Shortcuts.EditFindPrevious },
        { nameof(MainViewModel.ShowReplaceCommand), Se.Language.Options.Shortcuts.EditReplace },
        { nameof(MainViewModel.ShowMultipleReplaceCommand), Se.Language.Options.Shortcuts.EditMultipleReplace },
        { nameof(MainViewModel.ShowGoToLineCommand), Se.Language.Options.Shortcuts.GeneralGoToLineNumber },
        { nameof(MainViewModel.RightToLeftToggleCommand), Se.Language.Options.Shortcuts.ToggleRightToLeft },
        { nameof(MainViewModel.ShowModifySelectionCommand), Se.Language.Options.Shortcuts.ModifySelection },
        { nameof(MainViewModel.SelectAllLinesCommand), Se.Language.Options.Shortcuts.ListSelectAll },
        { nameof(MainViewModel.InverseSelectionCommand), Se.Language.Options.Shortcuts.ListInverseSelection },

        // Tools
        { nameof(MainViewModel.ShowToolsAdjustDurationsCommand), Se.Language.Options.Shortcuts.AdjustDurations },
        { nameof(MainViewModel.ShowApplyDurationLimitsCommand), Se.Language.Options.Shortcuts.ApplyDurationLimits },
        { nameof(MainViewModel.ShowToolsBatchConvertCommand), Se.Language.Options.Shortcuts.BatchConvert },
        { nameof(MainViewModel.ShowBridgeGapsCommand), Se.Language.Options.Shortcuts.BridgeGaps },
        { nameof(MainViewModel.ShowApplyMinGapCommand), Se.Language.Options.Shortcuts.ApplyMinGap },
        { nameof(MainViewModel.ShowToolsChangeCasingCommand), Se.Language.Options.Shortcuts.ChangeCasing },
        { nameof(MainViewModel.ShowToolsFixCommonErrorsCommand), Se.Language.Options.Shortcuts.FixCommonErrors },
        { nameof(MainViewModel.ToolsMakeEmptyTranslationFromCurrentSubtitleCommand), Se.Language.Options.Shortcuts.MakeEmptyTranslationFromCurrentSubtitle },
        { nameof(MainViewModel.ShowToolsMergeLinesWithSameTextCommand), Se.Language.Options.Shortcuts.MergeLinesWithSameText },
        { nameof(MainViewModel.ShowToolsMergeLinesWithSameTimeCodesCommand), Se.Language.Options.Shortcuts.MergeLinesWithSameTimeCodes },
        { nameof(MainViewModel.ShowToolsSplitBreakLongLinesCommand), Se.Language.Options.Shortcuts.SplitBreakLongLines },
        { nameof(MainViewModel.ShowToolsMergeShortLinesCommand), Se.Language.Options.Shortcuts.MergeShortLines },
        { nameof(MainViewModel.ShowToolsRemoveTextForHearingImpairedCommand), Se.Language.Options.Shortcuts.RemoveTextForHearingImpaired },
        { nameof(MainViewModel.ShowToolsJoinCommand), Se.Language.Options.Shortcuts.JoinSubtitles },
        { nameof(MainViewModel.ShowToolsSplitCommand), Se.Language.Options.Shortcuts.SplitSubtitle },

        // Spell check
        { nameof(MainViewModel.ShowSpellCheckCommand), Se.Language.Options.Shortcuts.SpellCheck },
        { nameof(MainViewModel.ShowSpellCheckDictionariesCommand), Se.Language.Options.Shortcuts.SpellCheckGetDictionary },

        // Video
        { nameof(MainViewModel.CommandVideoOpenCommand), Se.Language.Options.Shortcuts.OpenVideo },
        { nameof(MainViewModel.ShowVideoOpenFromUrlCommand), Se.Language.Options.Shortcuts.OpenVideoFromUrl },
        { nameof(MainViewModel.CommandVideoCloseCommand), Se.Language.Options.Shortcuts.CloseVideo },
        { nameof(MainViewModel.ShowVideoAudioToTextWhisperCommand), Se.Language.Options.Shortcuts.AudioToText },
        { nameof(MainViewModel.ShowVideoTextToSpeechCommand), Se.Language.Options.Shortcuts.TextToSpeech },
        { nameof(MainViewModel.ShowVideoBurnInCommand), Se.Language.Options.Shortcuts.BurnIn },
        { nameof(MainViewModel.ShowVideoTransparentSubtitlesCommand), Se.Language.Options.Shortcuts.GenerateTransparent },
        { nameof(MainViewModel.ShowShotChangesSubtitlesCommand),  Se.Language.General.GenerateImportShotChanges },
        { nameof(MainViewModel.VideoFullScreenCommand),  Se.Language.Options.Shortcuts.VideoFullScreen },

        // Sync
        { nameof(MainViewModel.ShowSyncAdjustAllTimesCommand),  Se.Language.Options.Shortcuts.AdjustAllTimes },
        { nameof(MainViewModel.ShowVisualSyncCommand),  Se.Language.Options.Shortcuts.VisualSync },
        { nameof(MainViewModel.ShowSyncChangeFrameRateCommand),  Se.Language.Options.Shortcuts.ChangeFrameRate },
        { nameof(MainViewModel.ShowSyncChangeSpeedCommand),  Se.Language.Options.Shortcuts.ChangeSpeed },

        // Translate
        { nameof(MainViewModel.ShowAutoTranslateCommand), Se.Language.Options.Shortcuts.AutoTranslate },
        { nameof(MainViewModel.ShowTranslateViaCopyPasteCommand), Se.Language.Options.Shortcuts.TranslateViaCopyPaste },

        // Options
        { nameof(MainViewModel.CommandShowSettingsCommand), Se.Language.Options.Shortcuts.Settings },
        { nameof(MainViewModel.CommandShowSettingsShortcutsCommand), Se.Language.Options.Shortcuts.Shortcuts },
        { nameof(MainViewModel.ShowWordListsCommand), Se.Language.Options.Shortcuts.WordLists },
        { nameof(MainViewModel.CommandShowSettingsLanguageCommand), Se.Language.Options.Shortcuts.ChooseLanguage },


        { nameof(MainViewModel.ShowGoToVideoPositionCommand), Se.Language.Options.Shortcuts.GeneralGoToVideoPosition },
        { nameof(MainViewModel.ToggleLinesItalicCommand), Se.Language.Options.Shortcuts.GeneralToggleItalic },
        { nameof(MainViewModel.ToggleLinesBoldCommand), Se.Language.Options.Shortcuts.GeneralToggleBold },

        { nameof(MainViewModel.TogglePlayPauseCommand), Se.Language.Options.Shortcuts.TogglePlayPause },
        { nameof(MainViewModel.TogglePlayPause2Command), Se.Language.Options.Shortcuts.TogglePlayPause },

        { nameof(MainViewModel.CommandShowLayoutCommand), Se.Language.Options.Shortcuts.GeneralChooseLayout },

        { nameof(MainViewModel.GoToNextLineCommand), Se.Language.Options.Shortcuts.GeneralGoToNextSubtitle },
        { nameof(MainViewModel.GoToPreviousLineCommand), Se.Language.Options.Shortcuts.GeneralGoToPrevSubtitle },
        { nameof(MainViewModel.GoToNextLineAndSetVideoPositionCommand), Se.Language.Options.Shortcuts.GoToNextLineAndSetVideoPosition },
        { nameof(MainViewModel.GoToPreviousLineAndSetVideoPositionCommand), Se.Language.Options.Shortcuts.GoToPreviousLineAndSetVideoPosition },
        { nameof(MainViewModel.SaveLanguageFileCommand), Se.Language.Main.SaveLanguageFile },

        { nameof(MainViewModel.UnbreakCommand), Se.Language.General.Unbreak },
        { nameof(MainViewModel.AutoBreakCommand), Se.Language.General.AutoBreak },
        { nameof(MainViewModel.SplitCommand), Se.Language.General.SplitLine },
        { nameof(MainViewModel.SplitAtVideoPositionCommand), Se.Language.General.SplitLineAtVideoPosition },
        { nameof(MainViewModel.SplitAtTextBoxCursorPositionCommand), Se.Language.General.SplitAtTextBoxCursorPosition },
        { nameof(MainViewModel.SplitAtVideoPositionAndTextBoxCursorPositionCommand), Se.Language.General.SplitLineAtVideoAndTextBoxPosition },
        { nameof(MainViewModel.TextBoxDeleteSelectionCommand), Se.Language.Options.Shortcuts.TextBoxDeleteSelectionNoClipboard },
        { nameof(MainViewModel.TextBoxCutCommand), Se.Language.Options.Shortcuts.TextBoxCut },
        { nameof(MainViewModel.TextBoxCut2Command), Se.Language.Options.Shortcuts.TextBoxCut2 },
        { nameof(MainViewModel.TextBoxPasteCommand), Se.Language.Options.Shortcuts.TextBoxPaste },
        { nameof(MainViewModel.TextBoxCopyCommand), Se.Language.Options.Shortcuts.TextBoxCopy },
        { nameof(MainViewModel.TextBoxSelectAllCommand), Se.Language.Options.Shortcuts.TextBoxSelectAll },
        { nameof(MainViewModel.TextBoxRemoveAllFormattingCommand), Se.Language.Options.Shortcuts.TextBoxRemoveAllFormatting },
        { nameof(MainViewModel.TextBoxItalicCommand), Se.Language.Options.Shortcuts.TextBoxItalic },

        { nameof(MainViewModel.VideoOneFrameBackCommand), Se.Language.General.VideoOneFrameBack },
        { nameof(MainViewModel.VideoOneFrameForwardCommand),  Se.Language.General.VideoOneFrameForward },
        { nameof(MainViewModel.Video100MsBackCommand), Se.Language.General.Video100MsBack },
        { nameof(MainViewModel.Video100MsForwardCommand),  Se.Language.General.Video100MsForward },
        { nameof(MainViewModel.Video500MsBackCommand), Se.Language.General.Video500MsBack },
        { nameof(MainViewModel.Video500MsForwardCommand),  Se.Language.General.Video500MsForward },
        { nameof(MainViewModel.VideoOneSecondBackCommand), Se.Language.General.VideoOneSecondBack },
        { nameof(MainViewModel.VideoOneSecondForwardCommand),  Se.Language.General.VideoOneSecondForward },

        { nameof(MainViewModel.WaveformSetStartAndOffsetTheRestCommand),  Se.Language.General.SetStartAndOffsetTheRest },
        { nameof(MainViewModel.WaveformSetStartCommand),  Se.Language.General.SetStart },
        { nameof(MainViewModel.WaveformSetEndCommand),  Se.Language.General.SetEnd },
        { nameof(MainViewModel.WaveformSetEndAndGoToNextCommand),  Se.Language.General.SetEndAndGoToNext },
        { nameof(MainViewModel.ToggleShotChangesAtVideoPositionCommand),  Se.Language.General.ToggleShotChangesAtVideoPosition },
        { nameof(MainViewModel.ShowWaveformSeekSilenceCommand),  Se.Language.Options.Shortcuts.SeekSilence },
        { nameof(MainViewModel.ShowShotChangesListCommand),  Se.Language.General.ShowShotChangesList },
        { nameof(MainViewModel.VideoUndockControlsCommand),  Se.Language.Options.Shortcuts.UndockVideoControls },
        { nameof(MainViewModel.VideoRedockControlsCommand),  Se.Language.Options.Shortcuts.RedockVideoControls },
        { nameof(MainViewModel.VideoGenerateBlankCommand),  Se.Language.Options.Shortcuts.GenerateBlankVideo },
        { nameof(MainViewModel.VideoReEncodeCommand),  Se.Language.Options.Shortcuts.ReencodeVideo},
        { nameof(MainViewModel.VideoCutCommand),  Se.Language.Options.Shortcuts.CutVideo},
        { nameof(MainViewModel.ResetWaveformZoomAndSpeedCommand),  Se.Language.Waveform.ResetWaveformZoomAndSpeed },
        { nameof(MainViewModel.ExtendSelectedToPreviousCommand),  Se.Language.General.ExtendSelectedToPrevious },
        { nameof(MainViewModel.ExtendSelectedToNextCommand),  Se.Language.General.ExtendSelectedToNext },
        { nameof(MainViewModel.ToggleLockTimeCodesCommand), Se.Language.Options.Shortcuts.ToggleLockTimeCodes },
        { nameof(MainViewModel.ShowHelpCommand), Se.Language.Options.Shortcuts.Help },
        { nameof(MainViewModel.ShowSourceViewCommand), Se.Language.Options.Shortcuts.SourceView },
        { nameof(MainViewModel.MergeWithLineBeforeCommand), Se.Language.General.MergeWithLineBeforeAndAutoBreak },
        { nameof(MainViewModel.MergeWithLineAfterCommand), Se.Language.General.MergeWithLineAfterAndAutoBreak },
        { nameof(MainViewModel.MergeWithLineBeforeKeepBreaksCommand), Se.Language.General.MergeWithLineBeforeKeepBreaks },
        { nameof(MainViewModel.MergeWithLineAfterKeepBreaksCommand), Se.Language.General.MergeWithLineAfterKeepBreaks },
        { nameof(MainViewModel.MergeSelectedLinesCommand), Se.Language.General.MergeSelectedLines },
        { nameof(MainViewModel.MergeSelectedLinesDialogCommand), Se.Language.General.MergeSelectedLinesDialog },
        { nameof(MainViewModel.ShowColorPickerCommand), Se.Language.General.ChooseColorDotDotDot },
        { nameof(MainViewModel.WaveformSetEndAndStartOfNextAfterGapCommand), Se.Language.Options.Shortcuts.WaveformSetEndAndStartOfNextAfterGap },
        { nameof(MainViewModel.WaveformSetEndAndStartOfNextAfterGapAndGoToNextCommand), Se.Language.Options.Shortcuts.WaveformSetEndAndStartOfNextAfterGapAndGoToNext },
        { nameof(MainViewModel.WaveformSetStartAndSetEndOfPreviousMinusGapCommand), Se.Language.Options.Shortcuts.WaveformSetStartAndSetEndOfPreviousMinusGap },
        { nameof(MainViewModel.FetchFirstWordFromNextSubtitleCommand), Se.Language.Options.Shortcuts.FetchFirstWordFromNextSubtitle },
        { nameof(MainViewModel.MoveLastWordToNextSubtitleCommand), Se.Language.Options.Shortcuts.MoveLastWordToNextSubtitle },
        { nameof(MainViewModel.MoveLastWordFromFirstLineDownCurrentSubtitleCommand), Se.Language.Options.Shortcuts.MoveLastWordFromFirstLineDownCurrentSubtitle },
        { nameof(MainViewModel.MoveFirstWordFromNextLineUpCurrentSubtitleCommand), Se.Language.Options.Shortcuts.MoveFirstWordFromNextLineUpCurrentSubtitle },
        { nameof(MainViewModel.ToggleFocusGridAndWaveformCommand), Se.Language.Options.Shortcuts.ToggleFocusGridAndWaveform },
        { nameof(MainViewModel.ToggleFocusTextBoxAndWaveformCommand), Se.Language.Options.Shortcuts.ToggleFocusTextBoxAndWaveform },
        { nameof(MainViewModel.ToggleFocusTextBoxAndSubtitleGridCommand), Se.Language.Options.Shortcuts.ToggleFocusTextBoxAndGrid },
        { nameof(MainViewModel.SubtitleGridCutCommand), Se.Language.Options.Shortcuts.SubtitleGridCut },
        { nameof(MainViewModel.SubtitleGridCopyCommand), Se.Language.Options.Shortcuts.SubtitleGridCopy },
        { nameof(MainViewModel.SubtitleGridPasteCommand), Se.Language.Options.Shortcuts.SubtitleGridPaste },
        { nameof(MainViewModel.ShowChooseProfileCommand), Se.Language.Options.Shortcuts.ChooseRuleProfile },
        { nameof(MainViewModel.TogglePlaybackSpeedCommand), Se.Language.Options.Shortcuts.TogglePlaybackSpeed },
        { nameof(MainViewModel.VideoSetPositionCurrentSubtitleStartCommand), Se.Language.Options.Shortcuts.SetVideoPositionCurrentSubtitleStart },
        { nameof(MainViewModel.VideoSetPositionCurrentSubtitleEndCommand), Se.Language.Options.Shortcuts.SetVideoPositionCurrentSubtitleEnd },
        { nameof(MainViewModel.ToggleAudioTracksCommand), Se.Language.Options.Shortcuts.ToggleAudioTracks },
        { nameof(MainViewModel.ListErrorsCommand), Se.Language.Options.Shortcuts.ListErrors },
        { nameof(MainViewModel.GoToPreviousErrorCommand), Se.Language.Options.Shortcuts.GoToPreviousError },
        { nameof(MainViewModel.GoToNextErrorCommand), Se.Language.Options.Shortcuts.GoToNextError },
        { nameof(MainViewModel.ShowAddToNameListCommand), Se.Language.Options.Shortcuts.AddNameToNameList },
        { nameof(MainViewModel.ShowFindDoubleWordsCommand), Se.Language.Options.Shortcuts.FindDoubleWords },
    };

    private static List<AvailableShortcut> GetAllAvailableShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<AvailableShortcut>();

        AddShortcut(shortcuts, vm.SelectAllLinesCommand, nameof(vm.SelectAllLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.InverseSelectionCommand, nameof(vm.InverseSelectionCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DeleteSelectedLinesCommand, nameof(vm.DeleteSelectedLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DuplicateSelectedLinesCommand, nameof(vm.DuplicateSelectedLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.ShowAlignmentPickerCommand, nameof(vm.ShowAlignmentPickerCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.AddOrEditBookmarkCommand, nameof(vm.AddOrEditBookmarkCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.ToggleBookmarkSelectedLinesNoTextCommand, nameof(vm.ToggleBookmarkSelectedLinesNoTextCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.CopyTextFromOriginalToTranslationCommand, nameof(vm.CopyTextFromOriginalToTranslationCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.SwitchOriginalAndTranslationTextSelectedLinesCommand, nameof(vm.SwitchOriginalAndTranslationTextSelectedLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.MergeOriginalIntoTranslationSelectedLinesCommand, nameof(vm.MergeOriginalIntoTranslationSelectedLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.ListBookmarksCommand, nameof(vm.ListBookmarksCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextBookmarkCommand, nameof(vm.GoToNextBookmarkCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.OpenDataFolderCommand, nameof(vm.OpenDataFolderCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleIsWaveformToolbarVisibleCommand, nameof(vm.ToggleIsWaveformToolbarVisibleCommand), ShortcutCategory.General);

        // File
        AddShortcut(shortcuts, vm.CommandFileOpenCommand, nameof(vm.CommandFileOpenCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandExitCommand, nameof(vm.CommandExitCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileNewCommand, nameof(vm.CommandFileNewCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileNewKeepVideoCommand, nameof(vm.CommandFileNewKeepVideoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileSaveCommand, nameof(vm.CommandFileSaveCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileSaveAsCommand, nameof(vm.CommandFileSaveAsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowStatisticsCommand, nameof(vm.ShowStatisticsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowCompareCommand, nameof(vm.ShowCompareCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowRestoreAutoBackupCommand, nameof(vm.ShowRestoreAutoBackupCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.OpenContainingFolderCommand, nameof(vm.OpenContainingFolderCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ImportTimeCodesCommand, nameof(vm.ImportTimeCodesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowImportSubtitleWithManuallyChosenEncodingCommand, nameof(vm.ShowImportSubtitleWithManuallyChosenEncodingCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileOpenKeepVideoCommand, nameof(vm.CommandFileOpenKeepVideoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FileOpenOriginalCommand, nameof(vm.FileOpenOriginalCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FileCloseOriginalCommand, nameof(vm.FileCloseOriginalCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ExportBluRaySupCommand, nameof(vm.ExportBluRaySupCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowExportCustomTextFormatCommand, nameof(vm.ShowExportCustomTextFormatCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowExportPlainTextCommand, nameof(vm.ShowExportPlainTextCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.UndoCommand, nameof(vm.UndoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.RedoCommand, nameof(vm.RedoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowFindCommand, nameof(vm.ShowFindCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FindNextCommand, nameof(vm.FindNextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FindPreviousCommand, nameof(vm.FindPreviousCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowReplaceCommand, nameof(vm.ShowReplaceCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowMultipleReplaceCommand, nameof(vm.ShowMultipleReplaceCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowGoToLineCommand, nameof(vm.ShowGoToLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.RightToLeftToggleCommand, nameof(vm.RightToLeftToggleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowModifySelectionCommand, nameof(vm.ShowModifySelectionCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.ShowGoToVideoPositionCommand, nameof(vm.ShowGoToVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleLinesItalicCommand, nameof(vm.ToggleLinesItalicCommand), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.ToggleLinesBoldCommand, nameof(vm.ToggleLinesBoldCommand), ShortcutCategory.SubtitleGridAndTextBox);

        AddShortcut(shortcuts, vm.TogglePlayPauseCommand, nameof(vm.TogglePlayPauseCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.TogglePlayPause2Command, nameof(vm.TogglePlayPause2Command), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.CommandShowLayoutCommand, nameof(vm.CommandShowLayoutCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAutoTranslateCommand, nameof(vm.ShowAutoTranslateCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowTranslateViaCopyPasteCommand, nameof(vm.ShowTranslateViaCopyPasteCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowSettingsCommand, nameof(vm.CommandShowSettingsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowSettingsShortcutsCommand, nameof(vm.CommandShowSettingsShortcutsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowWordListsCommand, nameof(vm.ShowWordListsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowSettingsLanguageCommand, nameof(vm.CommandShowSettingsLanguageCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.GoToPreviousLineCommand, nameof(vm.GoToPreviousLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextLineCommand, nameof(vm.GoToNextLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToPreviousLineAndSetVideoPositionCommand, nameof(vm.GoToPreviousLineAndSetVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextLineAndSetVideoPositionCommand, nameof(vm.GoToNextLineAndSetVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SaveLanguageFileCommand, nameof(vm.SaveLanguageFileCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.UnbreakCommand, nameof(vm.UnbreakCommand), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.AutoBreakCommand, nameof(vm.AutoBreakCommand), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SplitCommand, nameof(vm.SplitCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SplitAtVideoPositionCommand, nameof(vm.SplitAtVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SplitAtTextBoxCursorPositionCommand, nameof(vm.SplitAtTextBoxCursorPositionCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.SplitAtVideoPositionAndTextBoxCursorPositionCommand, nameof(vm.SplitAtVideoPositionAndTextBoxCursorPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.TextBoxDeleteSelectionCommand, nameof(vm.TextBoxDeleteSelectionCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxCutCommand, nameof(vm.TextBoxCutCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxCut2Command, nameof(vm.TextBoxCut2Command), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxPasteCommand, nameof(vm.TextBoxPasteCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxCopyCommand, nameof(vm.TextBoxCopyCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxSelectAllCommand, nameof(vm.TextBoxSelectAllCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxRemoveAllFormattingCommand, nameof(vm.TextBoxRemoveAllFormattingCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxItalicCommand, nameof(vm.TextBoxItalicCommand), ShortcutCategory.TextBox);

        // Tools
        AddShortcut(shortcuts, vm.ShowBridgeGapsCommand, nameof(vm.ShowBridgeGapsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsAdjustDurationsCommand, nameof(vm.ShowToolsAdjustDurationsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowApplyDurationLimitsCommand, nameof(vm.ShowApplyDurationLimitsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsBatchConvertCommand, nameof(vm.ShowToolsBatchConvertCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowApplyMinGapCommand, nameof(vm.ShowApplyMinGapCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsChangeCasingCommand, nameof(vm.ShowToolsChangeCasingCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsFixCommonErrorsCommand, nameof(vm.ShowToolsFixCommonErrorsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToolsMakeEmptyTranslationFromCurrentSubtitleCommand, nameof(vm.ToolsMakeEmptyTranslationFromCurrentSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsMergeLinesWithSameTextCommand, nameof(vm.ShowToolsMergeLinesWithSameTextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsMergeLinesWithSameTimeCodesCommand, nameof(vm.ShowToolsMergeLinesWithSameTimeCodesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsSplitBreakLongLinesCommand, nameof(vm.ShowToolsSplitBreakLongLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsMergeShortLinesCommand, nameof(vm.ShowToolsMergeShortLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsRemoveTextForHearingImpairedCommand, nameof(vm.ShowToolsRemoveTextForHearingImpairedCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsJoinCommand, nameof(vm.ShowToolsJoinCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsSplitCommand, nameof(vm.ShowToolsSplitCommand), ShortcutCategory.General);

        // Spell check
        AddShortcut(shortcuts, vm.ShowSpellCheckCommand, nameof(vm.ShowSpellCheckCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowSpellCheckDictionariesCommand, nameof(vm.ShowSpellCheckDictionariesCommand), ShortcutCategory.General);

        // Video
        AddShortcut(shortcuts, vm.CommandVideoOpenCommand, nameof(vm.CommandVideoOpenCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowVideoOpenFromUrlCommand, nameof(vm.ShowVideoOpenFromUrlCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandVideoCloseCommand, nameof(vm.CommandVideoCloseCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowVideoAudioToTextWhisperCommand, nameof(vm.ShowVideoAudioToTextWhisperCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowVideoTextToSpeechCommand, nameof(vm.ShowVideoTextToSpeechCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowVideoBurnInCommand, nameof(vm.ShowVideoBurnInCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowVideoTransparentSubtitlesCommand, nameof(vm.ShowVideoTransparentSubtitlesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowShotChangesSubtitlesCommand, nameof(vm.ShowShotChangesSubtitlesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowShotChangesListCommand, nameof(vm.ShowShotChangesListCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoUndockControlsCommand, nameof(vm.VideoUndockControlsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoRedockControlsCommand, nameof(vm.VideoRedockControlsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoGenerateBlankCommand, nameof(vm.VideoGenerateBlankCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoReEncodeCommand, nameof(vm.VideoReEncodeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoCutCommand, nameof(vm.VideoCutCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoFullScreenCommand, nameof(vm.VideoFullScreenCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.VideoOneFrameBackCommand, nameof(vm.VideoOneFrameBackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoOneFrameForwardCommand, nameof(vm.VideoOneFrameForwardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.Video100MsBackCommand, nameof(vm.Video100MsBackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.Video100MsForwardCommand, nameof(vm.Video100MsForwardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.Video500MsBackCommand, nameof(vm.Video500MsBackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.Video500MsForwardCommand, nameof(vm.Video500MsForwardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoOneSecondBackCommand, nameof(vm.VideoOneSecondBackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoOneSecondForwardCommand, nameof(vm.VideoOneSecondForwardCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.WaveformSetStartAndOffsetTheRestCommand, nameof(vm.WaveformSetStartAndOffsetTheRestCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformSetStartCommand, nameof(vm.WaveformSetStartCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformSetEndCommand, nameof(vm.WaveformSetEndCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformSetEndAndGoToNextCommand, nameof(vm.WaveformSetEndAndGoToNextCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformSetEndAndStartOfNextAfterGapCommand, nameof(vm.WaveformSetEndAndStartOfNextAfterGapCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformSetEndAndStartOfNextAfterGapAndGoToNextCommand, nameof(vm.WaveformSetEndAndStartOfNextAfterGapAndGoToNextCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformSetStartAndSetEndOfPreviousMinusGapCommand, nameof(vm.WaveformSetStartAndSetEndOfPreviousMinusGapCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.ToggleShotChangesAtVideoPositionCommand, nameof(vm.ToggleShotChangesAtVideoPositionCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.ShowWaveformSeekSilenceCommand, nameof(vm.ShowWaveformSeekSilenceCommand), ShortcutCategory.Waveform);

        AddShortcut(shortcuts, vm.ResetWaveformZoomAndSpeedCommand, nameof(vm.ResetWaveformZoomAndSpeedCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ExtendSelectedToPreviousCommand, nameof(vm.ExtendSelectedToPreviousCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ExtendSelectedToNextCommand, nameof(vm.ExtendSelectedToNextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleLockTimeCodesCommand, nameof(vm.ToggleLockTimeCodesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowHelpCommand, nameof(vm.ShowHelpCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowSourceViewCommand, nameof(vm.ShowSourceViewCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MergeWithLineBeforeCommand, nameof(vm.MergeWithLineBeforeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MergeWithLineAfterCommand, nameof(vm.MergeWithLineAfterCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MergeSelectedLinesCommand, nameof(vm.MergeSelectedLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MergeWithLineBeforeKeepBreaksCommand, nameof(vm.MergeWithLineBeforeKeepBreaksCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MergeWithLineAfterKeepBreaksCommand, nameof(vm.MergeWithLineAfterKeepBreaksCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.MergeSelectedLinesDialogCommand, nameof(vm.MergeSelectedLinesDialogCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowColorPickerCommand, nameof(vm.ShowColorPickerCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FetchFirstWordFromNextSubtitleCommand, nameof(vm.FetchFirstWordFromNextSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MoveLastWordToNextSubtitleCommand, nameof(vm.MoveLastWordToNextSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand, nameof(vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MoveFirstWordFromNextLineUpCurrentSubtitleCommand, nameof(vm.MoveFirstWordFromNextLineUpCurrentSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleFocusGridAndWaveformCommand, nameof(vm.ToggleFocusGridAndWaveformCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleFocusTextBoxAndWaveformCommand, nameof(vm.ToggleFocusTextBoxAndWaveformCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleFocusTextBoxAndSubtitleGridCommand, nameof(vm.ToggleFocusTextBoxAndSubtitleGridCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SubtitleGridCutCommand, nameof(vm.SubtitleGridCutCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.SubtitleGridCopyCommand, nameof(vm.SubtitleGridCopyCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.SubtitleGridPasteCommand, nameof(vm.SubtitleGridPasteCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.ShowChooseProfileCommand, nameof(vm.ShowChooseProfileCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.TogglePlaybackSpeedCommand, nameof(vm.TogglePlaybackSpeedCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoSetPositionCurrentSubtitleStartCommand, nameof(vm.VideoSetPositionCurrentSubtitleStartCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoSetPositionCurrentSubtitleEndCommand, nameof(vm.VideoSetPositionCurrentSubtitleEndCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleAudioTracksCommand, nameof(vm.ToggleAudioTracksCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ListErrorsCommand, nameof(vm.ListErrorsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToPreviousErrorCommand, nameof(vm.GoToPreviousErrorCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextErrorCommand, nameof(vm.GoToNextErrorCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAddToNameListCommand, nameof(vm.ShowAddToNameListCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowFindDoubleWordsCommand, nameof(vm.ShowFindDoubleWordsCommand), ShortcutCategory.General);

        return shortcuts;
    }

    public static List<SeShortCut> GetDefaultShortcuts(MainViewModel vm)
    {
        var commandOrWin = GetCommandOrWin();

        return
        [
            new(nameof(vm.UndoCommand), [commandOrWin, "Z"]),
            new(nameof(vm.RedoCommand), [commandOrWin, "Y"]),
            new(nameof(vm.ShowGoToLineCommand), [commandOrWin, "G"]),
            new(nameof(vm.AddOrEditBookmarkCommand), [commandOrWin + "Shift", "B"]),
            new(nameof(vm.GoToPreviousLineCommand), ["Alt", "Up"]),
            new(nameof(vm.GoToNextLineCommand), ["Alt", "Down"]),
            new(nameof(vm.SelectAllLinesCommand), [commandOrWin, "A"], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.InverseSelectionCommand), [commandOrWin, "Shift", "I"], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ToggleLinesItalicCommand), [commandOrWin, "I"], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.DeleteSelectedLinesCommand), ["Delete"], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ShowFindCommand), [commandOrWin, "F"], ShortcutCategory.General),
            new(nameof(vm.FindNextCommand), [nameof(Avalonia.Input.Key.F3)], ShortcutCategory.General),
            new(nameof(vm.FindPreviousCommand), ["Shift", nameof(Avalonia.Input.Key.F3)], ShortcutCategory.General),
            new(nameof(vm.ShowReplaceCommand), [commandOrWin, "H"], ShortcutCategory.General),
            new(nameof(vm.OpenDataFolderCommand), [commandOrWin, "Alt", "Shift", "D"], ShortcutCategory.General),
            new(nameof(vm.CommandFileNewCommand), [commandOrWin, "N"], ShortcutCategory.General),
            new(nameof(vm.CommandFileOpenCommand), [commandOrWin, "O"], ShortcutCategory.General),
            new(nameof(vm.CommandFileSaveCommand), [commandOrWin, "S"], ShortcutCategory.General),
            new(nameof(vm.TogglePlayPauseCommand), [nameof(Avalonia.Input.Key.Space)], ShortcutCategory.General),
            new(nameof(vm.TogglePlayPause2Command), [commandOrWin, nameof(Avalonia.Input.Key.Space)], ShortcutCategory.General),
            new(nameof(vm.VideoOneSecondBackCommand), [nameof(Avalonia.Input.Key.Left)], ShortcutCategory.General),
            new(nameof(vm.VideoOneSecondForwardCommand), [nameof(Avalonia.Input.Key.Right)], ShortcutCategory.General),
            new(nameof(vm.ShowHelpCommand), [nameof(Avalonia.Input.Key.F1)], ShortcutCategory.General),
            new(nameof(vm.ShowSourceViewCommand), [nameof(Avalonia.Input.Key.F2)], ShortcutCategory.General),
            new(nameof(vm.TextBoxDeleteSelectionCommand), ["Shift", nameof(Avalonia.Input.Key.Back)], ShortcutCategory.TextBox),
            new(nameof(vm.TextBoxCut2Command), ["Shift", nameof(Avalonia.Input.Key.Delete) ], ShortcutCategory.TextBox),
            new(nameof(vm.TextBoxCutCommand), [GetCommandOrWin(), nameof(Avalonia.Input.Key.X)], ShortcutCategory.TextBox),
            new(nameof(vm.TextBoxPasteCommand), [GetCommandOrWin(), nameof(Avalonia.Input.Key.V)], ShortcutCategory.TextBox),
            new(nameof(vm.TextBoxCopyCommand), [GetCommandOrWin(), nameof(Avalonia.Input.Key.C)], ShortcutCategory.TextBox),
            new(nameof(vm.TextBoxSelectAllCommand), [GetCommandOrWin(), nameof(Avalonia.Input.Key.A)], ShortcutCategory.TextBox),
            new(nameof(vm.SubtitleGridCutCommand), [GetCommandOrWin(), nameof(Avalonia.Input.Key.X)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.SubtitleGridPasteCommand), [GetCommandOrWin(), nameof(Avalonia.Input.Key.V)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.SubtitleGridCopyCommand), [GetCommandOrWin(), nameof(Avalonia.Input.Key.C)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ListErrorsCommand), [GetCommandOrWin(), nameof(Avalonia.Input.Key.F8)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.GoToPreviousErrorCommand), ["Shift", nameof(Avalonia.Input.Key.F8)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.GoToNextErrorCommand), [nameof(Avalonia.Input.Key.F8)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ShowSpellCheckCommand), ["Alt", nameof(Avalonia.Input.Key.F7)], ShortcutCategory.SubtitleGrid),
        ];
    }

    private static string GetCommandOrWin()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Win";
        }

        return "Ctrl";
    }

    public class AvailableShortcut
    {
        public string Name { get; set; }
        public ShortcutCategory Category { get; set; }

        public AvailableShortcut(IRelayCommand relayCommand, string shortcutName)
        {
            Name = shortcutName;
            RelayCommand = relayCommand;
            Category = ShortcutCategory.General;
        }
        public AvailableShortcut(IRelayCommand relayCommand, string shortcutName, ShortcutCategory category)
        {
            Name = shortcutName;
            RelayCommand = relayCommand;
            Category = category;
        }

        public IRelayCommand RelayCommand { get; set; }
    }
}