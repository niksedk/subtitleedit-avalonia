namespace Nikse.SubtitleEdit.Logic.Config.Language.Options;

public class LanguageSettingsShortcuts
{
    public string Title { get; set; }
    public string SearchShortcuts { get; set; }
    public string Filter { get; set; }

    public string CategoryGeneral { get; set; }
    public string CategorySubtitleGridAndTextBox { get; set; }
    public string CategorySubtitleGrid { get; set; }
    public string CategoryWaveform { get; set; }

    public string GeneralMergeSelectedLines { get; set; }
    public string GeneralMergeWithPrevious { get; set; }
    public string GeneralMergeWithNext { get; set; }
    public string GeneralMergeWithPreviousAndUnbreak { get; set; }
    public string GeneralMergeWithNextAndUnbreak { get; set; }
    public string GeneralMergeWithPreviousAndAutoBreak { get; set; }
    public string GeneralMergeWithNextAndAutoBreak { get; set; }
    public string GeneralMergeSelectedLinesAndAutoBreak { get; set; }
    public string GeneralMergeSelectedLinesAndUnbreak { get; set; }
    public string GeneralMergeSelectedLinesAndUnbreakCjk { get; set; }
    public string GeneralMergeSelectedLinesOnlyFirstText { get; set; }
    public string GeneralMergeSelectedLinesBilingual { get; set; }
    public string GeneralMergeWithPreviousBilingual { get; set; }
    public string GeneralMergeWithNextBilingual { get; set; }
    public string GeneralMergeOriginalAndTranslation { get; set; }
    public string GeneralToggleTranslationMode { get; set; }
    public string GeneralSwitchOriginalAndTranslation { get; set; }
    public string GeneralSwitchOriginalAndTranslationTextBoxes { get; set; }
    public string GeneralChooseLayout { get; set; }
    public string GeneralLayoutChooseX { get; set; }
    public string GeneralPlayFirstSelected { get; set; }
    public string GeneralGoToFirstSelectedLine { get; set; }
    public string GeneralGoToNextEmptyLine { get; set; }
    public string GeneralGoToNextSubtitle { get; set; }
    public string GeneralGoToNextSubtitlePlayTranslate { get; set; }
    public string GeneralGoToNextSubtitleCursorAtEnd { get; set; }
    public string GeneralGoToPrevSubtitle { get; set; }
    public string GeneralGoToPrevSubtitlePlayTranslate { get; set; }
    public string GeneralGoToStartOfCurrentSubtitle { get; set; }
    public string GeneralGoToEndOfCurrentSubtitle { get; set; }
    public string GeneralGoToPreviousSubtitleAndFocusVideo { get; set; }
    public string GeneralGoToNextSubtitleAndFocusVideo { get; set; }
    public string GeneralGoToPrevSubtitleAndPlay { get; set; }
    public string GeneralGoToNextSubtitleAndPlay { get; set; }
    public string GeneralGoToPreviousSubtitleAndFocusWaveform { get; set; }
    public string GeneralGoToNextSubtitleAndFocusWaveform { get; set; }
    public string GeneralGoToLineNumber { get; set; }
    public string GeneralToggleItalic { get; set; }
    public string GeneralToggleBold { get; set; }
    public string GeneralToggleBookmarks { get; set; }
    public string GeneralFocusTextBox { get; set; }
    public string GeneralToggleBookmarksWithText { get; set; }
    public string GeneralEditBookmarks { get; set; }



    public string FileNew { get; set; }
    public string FileOpen { get; set; }
    public string FileOpenKeepVideo { get; set; }
    public string FileSave { get; set; }
    public string FileSaveAs { get; set; }
    public string FileSaveAll { get; set; }
    public string FileSaveOriginal { get; set; }
    public string FileSaveOriginalAs { get; set; }
    public string FileOpenOriginalSubtitle { get; set; }
    public string FileCloseOriginalSubtitle { get; set; }
    public string FileTranslatedSubtitle { get; set; }
    public string FileCompare { get; set; }
    public string FileImportPlainText { get; set; }
    public string FileImportBluRaySupForOcr { get; set; }
    public string FileImportBluRaySupForEdit { get; set; }
    public string FileImportTimeCodes { get; set; }
    public string FileExportEbuStl { get; set; }
    public string FileExportPac { get; set; }
    public string FileExportEdlClipName { get; set; }
    public string FileExportPlainText { get; set; }
    public string FileExportCustomTextFormat1 { get; set; }
    public string FileExportCustomTextFormat2 { get; set; }
    public string FileExportCustomTextFormat3 { get; set; }
    public string FileExit { get; set; }
    public string OpenSeDataFolder { get; set; }

    public string EditFind { get; set; }
    public string EditFindNext { get; set; }
    public string EditReplace { get; set; }
    public string EditMultipleReplace { get; set; }
    public string EditModifySelection { get; set; }

    public string ListSelectAll { get; set; }
    public string ListSelectFirst { get; set; }
    public string ListSelectLast { get; set; }
    public string ListInverseSelection { get; set; }
    public string ListDeleteSelection { get; set; }

    public string AutoTranslate { get; set; }

    public string Settings { get; set; }
    public string Assigned { get; internal set; }
    public string Unassigned { get; internal set; }
    public string PressedKeyX { get; set; }
    public string PressAKey { get; set; }
    public string DetectKey { get; set; }
    public string Control { get; set; }
    public string Alt { get; set; }
    public string Win { get; set; }
    public string Shift { get; set; }
    public string ResetShortcuts { get; set; }
    public string ResetShortcutsDetail { get; set; }

    public LanguageSettingsShortcuts()
    {
        Title = "Shortcuts";
        SearchShortcuts = "Search shortcuts...";
        Filter = "Filter";

        CategoryGeneral = "General";
        CategorySubtitleGridAndTextBox = "Subtitle list view & text box";
        CategorySubtitleGrid = "Subtitle list view";
        CategoryWaveform = "Waveform";

        GeneralMergeSelectedLines = "Merge selected lines";
        GeneralMergeWithPrevious = "Merge with previous";
        GeneralMergeWithNext = "Merge with next";
        GeneralMergeWithPreviousAndUnbreak = "Merge with previous and unbreak";
        GeneralMergeWithNextAndUnbreak = "Merge with next and unbreak";
        GeneralMergeWithPreviousAndAutoBreak = "Merge with previous and auto-break";
        GeneralMergeWithNextAndAutoBreak = "Merge with next and auto-break";
        GeneralMergeSelectedLinesAndAutoBreak = "Merge selected lines and auto-break";
        GeneralMergeSelectedLinesAndUnbreak = "Merge selected lines and unbreak";
        GeneralMergeSelectedLinesAndUnbreakCjk = "Merge selected lines and unbreak CJK";
        GeneralMergeSelectedLinesOnlyFirstText = "Merge selected lines only first text";
        GeneralMergeSelectedLinesBilingual = "Merge selected lines bilingual";
        GeneralMergeWithPreviousBilingual = "Merge with previous bilingual";
        GeneralMergeWithNextBilingual = "Merge with next bilingual";
        GeneralMergeOriginalAndTranslation = "Merge original and translation";
        GeneralToggleTranslationMode = "Toggle translation mode";
        GeneralSwitchOriginalAndTranslation = "Switch original and translation";
        GeneralSwitchOriginalAndTranslationTextBoxes = "Switch original and translation text boxes";
        GeneralChooseLayout = "Choose layout";
        GeneralLayoutChooseX = "Layout {0}";
        GeneralPlayFirstSelected = "Play first selected";
        GeneralGoToFirstSelectedLine = "Go to first selected line";
        GeneralGoToNextEmptyLine = "Go to next empty line";
        GeneralGoToNextSubtitle = "Go to next subtitle";
        GeneralGoToNextSubtitlePlayTranslate = "Go to next subtitle (play translate)";
        GeneralGoToNextSubtitleCursorAtEnd = "Go to next subtitle (cursor at end)";
        GeneralGoToPrevSubtitle = "Go to prev subtitle";
        GeneralGoToPrevSubtitlePlayTranslate = "Go to prev subtitle (play translate)";
        GeneralGoToStartOfCurrentSubtitle = "Go to start of current subtitle";
        GeneralGoToEndOfCurrentSubtitle = "Go to end of current subtitle";
        GeneralGoToPreviousSubtitleAndFocusVideo = "Go to previous subtitle and focus video";
        GeneralGoToNextSubtitleAndFocusVideo = "Go to next subtitle and focus video";
        GeneralGoToPrevSubtitleAndPlay = "Go to prev subtitle and play";
        GeneralGoToNextSubtitleAndPlay = "Go to next subtitle and play";
        GeneralGoToPreviousSubtitleAndFocusWaveform = "Go to previous subtitle and focus waveform";
        GeneralGoToNextSubtitleAndFocusWaveform = "Go to next subtitle and focus waveform";
        GeneralGoToLineNumber = "Go to line number";
        GeneralToggleItalic = "Toggle italic";
        GeneralToggleBold = "Toggle bold";
        GeneralToggleBookmarks = "Toggle bookmarks";
        GeneralFocusTextBox = "Focus text box";
        GeneralToggleBookmarksWithText = "Toggle bookmarks with text";
        GeneralEditBookmarks = "Edit bookmarks";

        FileNew = "New";
        FileOpen = "Open";
        FileOpenKeepVideo = "Open (keep video)";
        FileSave = "Save";
        FileSaveAs = "Save as";
        FileSaveAll = "Save all";
        FileSaveOriginal = "Save original";
        FileSaveOriginalAs = "Save original as";
        FileOpenOriginalSubtitle = "Open original subtitle";
        FileCloseOriginalSubtitle = "Close original subtitle";
        FileTranslatedSubtitle = "Translated subtitle";
        FileCompare = "Compare";
        FileImportPlainText = "Import plain text";
        FileImportBluRaySupForOcr = "Import Blu-ray SUP for OCR";
        FileImportBluRaySupForEdit = "Import Blu-ray SUP for edit";
        FileImportTimeCodes = "Import time codes";
        FileExportEbuStl = "Export EBU STL";
        FileExportPac = "Export PAC";
        FileExportEdlClipName = "Export EDL clip name";
        FileExportPlainText = "Export plain text";
        FileExportCustomTextFormat1 = "Export custom text format 1";
        FileExportCustomTextFormat2 = "Export custom text format 2";
        FileExportCustomTextFormat3 = "Export custom text format 3";
        FileExit = "Exit";
        OpenSeDataFolder = "Open Subtitle Edit folder";

        EditFind = "Find";
        EditFindNext = "Find next";
        EditReplace = "Replace";
        EditMultipleReplace = "Multiple replace";
        EditModifySelection = "Modify selection";

        ListSelectAll = "Select all";
        ListSelectFirst = "Select first";
        ListSelectLast = "Select last";
        ListInverseSelection = "Inverse selection";
        ListDeleteSelection = "Delete selection";

        AutoTranslate = "Auto-translate";

        Settings = "Settings";

        Assigned = "Assigned";
        Unassigned = "Unassigned";
        
        PressedKeyX = "Pressed key: {0}";
        PressAKey = "Press a key";
        
        DetectKey = "Detect key";
        
        Control = "Control";
        Alt = "Alt";
        Win = "Win";
        Shift = "Shift";

        ResetShortcuts = "Reset shortcuts";
        ResetShortcutsDetail = "Do you want to reset all shortcuts to default values?";
    }
}