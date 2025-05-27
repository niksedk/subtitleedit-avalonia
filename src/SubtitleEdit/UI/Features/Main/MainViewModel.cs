using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.Validators;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Common;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Edit.GoToLineNumber;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Features.Edit.ShowHistory;
using Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Options.Language;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.UndoRedo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    private List<SubtitleLineViewModel>? _selectedSubtitles;
    [ObservableProperty] private int? _selectedSubtitleIndex;
    [ObservableProperty] private bool _showColumnOriginalText;

    [ObservableProperty] private string _editText;
    [ObservableProperty] private string _editTextCharactersPerSecond;
    [ObservableProperty] private string _editTextTotalLength;
    [ObservableProperty] private string _editTextLineLengths;

    [ObservableProperty] private ObservableCollection<SubtitleFormat> _subtitleFormats;
    [ObservableProperty] private SubtitleFormat _selectedSubtitleFormat;

    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    public TextEncoding SelectedEncoding { get; set; }

    [ObservableProperty] private string _statusTextLeft;
    [ObservableProperty] private string _statusTextRight;

    [ObservableProperty] private bool _isWaveformToolbarVisible;

    public DataGrid SubtitleGrid { get; set; }
    public TextBox EditTextBox { get; set; }
    public Window Window { get; set; }
    public Grid ContentGrid { get; set; }
    public MainView MainView { get; set; }

    public TextBlock StatusTextLeftLabel { get; set; }

    //public Grid Waveform { get; internal set; }
    public MenuItem MenuReopen { get; set; }
    public AudioVisualizer? AudioVisualizer { get; set; }

    private bool _updateAudioVisualizer = false;
    private string? _subtitleFileName;
    private Subtitle _subtitle;
    private SubtitleFormat? _lastOpenSaveFormat;
    private string? _videoFileName;
    private CancellationTokenSource? _statusFadeCts;
    private int _changeSubtitleHash = -1;
    private bool _subtitleGridSelectionChangedSkip;

    private readonly IFileHelper _fileHelper;
    private readonly IShortcutManager _shortcutManager;
    private readonly IWindowService _windowService;
    private readonly IInsertService _insertService;
    private readonly IMergeManager _mergeManager;
    private readonly IAutoBackupService _autoBackupService;
    private readonly IUndoRedoManager _undoRedoManager;

    private bool IsEmpty => Subtitles.Count == 0 || string.IsNullOrEmpty(Subtitles[0].Text);

    public VideoPlayerControl? VideoPlayerControl { get; internal set; }

    public MainViewModel(
        IFileHelper fileHelper,
        IShortcutManager shortcutManager,
        IWindowService windowService,
        IInsertService insertService,
        IMergeManager mergeManager,
        IAutoBackupService autoBackupService,
        IUndoRedoManager undoRedoManager)
    {
        _fileHelper = fileHelper;
        _shortcutManager = shortcutManager;
        _windowService = windowService;
        _insertService = insertService;
        _mergeManager = mergeManager;
        _autoBackupService = autoBackupService;
        _undoRedoManager = undoRedoManager;

        EditText = string.Empty;
        EditTextCharactersPerSecond = string.Empty;
        EditTextTotalLength = string.Empty;
        EditTextLineLengths = string.Empty;
        StatusTextLeftLabel = new TextBlock();
        SubtitleGrid = new DataGrid();
        EditTextBox = new TextBox();
        ContentGrid = new Grid();
        MenuReopen = new MenuItem();
        _subtitle = new Subtitle();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        SubtitleFormats = [.. SubtitleFormat.AllSubtitleFormats];
        SelectedSubtitleFormat = SubtitleFormats[0];
        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        SelectedEncoding = Encodings[0];
        StatusTextLeft = string.Empty;
        StatusTextRight = string.Empty;

        LoadShortcuts();
        _isWaveformToolbarVisible = Se.Settings.Waveform.ShowToolbar;
        StartTitleTimer();
        _autoBackupService.StartAutoBackup(this);
    }

    private void LoadShortcuts()
    {
        Se.Settings.InitializeMainShortcuts(this);
        foreach (var shortCut in ShortcutsMain.GetUsedShortcuts(this))
        {
            _shortcutManager.RegisterShortcut(shortCut);
        }
    }

    [RelayCommand]
    private async Task CommandExit()
    {
        if (HasChanges())
        {
            var result = await MessageBox.Show(
                Window,
                "Save Changes",
                "Do you want to save before exiting?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == MessageBoxResult.Cancel)
            {
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                await SaveSubtitle();
            }
        }

        Environment.Exit(0);
    }

    [RelayCommand]
    private async Task CommandShowLayout()
    {
        // Open a dialog with a specific ViewModel and get the result
        var vm = await _windowService.ShowDialogAsync<LayoutWindow, LayoutViewModel>(Window,
            viewModel => { viewModel.SelectedLayout = Se.Settings.General.LayoutNumber; });

        if (vm.OkPressed && vm.SelectedLayout != null && vm.SelectedLayout != Se.Settings.General.LayoutNumber)
        {
            Se.Settings.General.LayoutNumber = InitLayout.MakeLayout(MainView, this, vm.SelectedLayout.Value);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowHelp()
    {
        await Window!.Launcher.LaunchUriAsync(new Uri("https://www.nikse.dk/subtitleedit/help"));
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowAbout()
    {
        var newWindow = new AboutWindow();
        await newWindow.ShowDialog(Window);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileNew()
    {
        if (HasChanges())
        {
            var result = await MessageBox.Show(
                Window,
                "Save Changes?",
                "Do you want to save changes?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == MessageBoxResult.Cancel)
            {
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                await SaveSubtitle();
            }
        }

        Subtitles.Clear();
        _subtitleFileName = string.Empty;
        _subtitle = new Subtitle();
        _changeSubtitleHash = GetFastSubtitleHash();
        if (AudioVisualizer?.WavePeaks != null)
        {
            AudioVisualizer.WavePeaks = null;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileOpen()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, "Open subtitle file");
        if (!string.IsNullOrEmpty(fileName))
        {
            await SubtitleOpen(fileName);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileReopen(RecentFile recentFile)
    {
        await SubtitleOpen(recentFile.SubtitleFileName, recentFile.VideoFileName, recentFile.SelectedLine);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void CommandFileClearRecentFiles(RecentFile recentFile)
    {
        Se.Settings.File.RecentFiles.Clear();
        InitMenu.UpdateRecentFiles(this);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileSave()
    {
        await SaveSubtitle();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileSaveAs()
    {
        await SaveSubtitleAs();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportBluRaySup()
    {
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportCapMakerPlus()
    {
        var format = new CapMakerPlus();
        using var ms = new MemoryStream();
        format.Save(_subtitleFileName, ms, GetUpdateSubtitle(), false);

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window,
            format,
            "newFileName",
            $"Save {format.Name} file as");

        if (!string.IsNullOrEmpty(fileName))
        {
            File.WriteAllBytes(fileName, ms.ToArray());
            ShowStatus($"File exported in format {format.Name} to {fileName}");
        }
    }

    [RelayCommand]
    private async Task ExportCavena890()
    {
        //var result = await _popupService
        //    .ShowPopupAsync<ExportCavena890PopupModel>(onPresenting: viewModel
        //        => viewModel.SetValues(UpdatedSubtitle), CancellationToken.None);

        //if (result is not (string and "OK"))
        //{
        //    return;
        //}

        //var cavena = new Cavena890();
        //using var ms = new MemoryStream();
        //cavena.Save(_subtitleFileName, ms, UpdatedSubtitle, false);

        //var fileHelper = new FileHelper();
        //var subtitleFileName = await fileHelper.SaveStreamAs(ms, $"Save {CurrentSubtitleFormat.Name} file as", _videoFileName, cavena);
        //if (!string.IsNullOrEmpty(subtitleFileName))
        //{
        //    ShowStatus($"File exported in format {cavena.Name} to {subtitleFileName}");
        //}
        //_shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportPac()
    {
        //var result = await _popupService.ShowPopupAsync<ExportPacPopupModel>(CancellationToken.None);
        //if (result is not int codePage || codePage < 0)
        //{
        //    return;
        //}

        //var pac = new Pac { CodePage = codePage };
        //using var ms = new MemoryStream();
        //pac.Save(_subtitleFileName, ms, GetUpdateSubtitle(), false);

        //var fileHelper = new FileHelper();
        //var subtitleFileName = await fileHelper.SaveStreamAs(ms, $"Save {CurrentSubtitleFormat.Name} file as", _videoFileName, pac);
        //if (!string.IsNullOrEmpty(subtitleFileName))
        //{
        //    ShowStatus($"File exported in format {pac.Name} to {subtitleFileName}");
        //}
        //_shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportPacUnicode()
    {
        var format = new PacUnicode();
        using var ms = new MemoryStream();
        format.Save(_subtitleFileName, ms, GetUpdateSubtitle());

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window,
            format,
            "newFileName",
            $"Save {format.Name} file as");

        if (!string.IsNullOrEmpty(fileName))
        {
            File.WriteAllBytes(fileName, ms.ToArray());
            ShowStatus($"File exported in format {format.Name} to {fileName}");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportEbuStl()
    {
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task Undo()
    {
        PerformUndo();
    }

    [RelayCommand]
    private async Task Redo()
    {
        PerformRedo();
    }

    [RelayCommand]
    private async Task ShowToolsAdjustDurations()
    {
        await _windowService.ShowDialogAsync<AdjustDurationWindow, AdjustDurationViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsBatchConvert()
    {
        await _windowService.ShowDialogAsync<BatchConvertWindow, BatchConvertViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsChangeCasing()
    {
        await _windowService.ShowDialogAsync<ChangeCasingWindow, ChangeCasingViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsFixCommonErrors()
    {
        var viewModel =
            await _windowService.ShowDialogAsync<FixCommonErrorsWindow, FixCommonErrorsViewModel>(Window,
                vm => { vm.Initialize(GetUpdateSubtitle()); });

        if (viewModel.OkPressed)
        {
            SetSubtitles(viewModel.FixedSubtitle);
            SelectAndScrollToRow(0);
            ShowStatus($"Fixed {viewModel.FixedSubtitle.Paragraphs.Count} lines");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsRemoveTextForHearingImpaired()
    {
        await _windowService.ShowDialogAsync<RemoveTextForHearingImpairedWindow, RemoveTextForHearingImpairedViewModel>(
            Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandVideoOpen()
    {
        var fileName = await _fileHelper.PickOpenVideoFile(Window, "Open video file");
        if (!string.IsNullOrEmpty(fileName))
        {
            await VideoOpenFile(fileName);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void CommandVideoClose()
    {
        VideoCloseFile();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSpellCheck()
    {
        await _windowService.ShowDialogAsync<SpellCheckWindow, SpellCheckViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowSpellCheckDictionaries()
    {
        await _windowService.ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoAudioToTextWhisper()
    {
        if (string.IsNullOrEmpty(_videoFileName))
        {
            await CommandVideoOpen();
            if (string.IsNullOrEmpty(_videoFileName))
            {
                return;
            }
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        var vm = await _windowService.ShowDialogAsync<AudioToTextWhisperWindow, AudioToTextWhisperViewModel>(Window,
            viewModel => { viewModel.Initialize(_videoFileName); });

        if (vm.OkPressed)
        {
            _subtitle = vm.TranscribedSubtitle;
            SetSubtitles(_subtitle);
            SelectAndScrollToRow(0);
            ShowStatus($"Transcription completed with {vm.TranscribedSubtitle.Paragraphs.Count} lines");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoBurnIn()
    {
        await _windowService.ShowDialogAsync<BurnInWindow, BurnInViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoOpenFromUrl()
    {
        await _windowService.ShowDialogAsync<OpenFromUrlWindow, OpenFromUrlViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoTextToSpeech()
    {
        await _windowService.ShowDialogAsync<TextToSpeechWindow, TextToSpeechViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoTransparentSubtitles()
    {
        await _windowService.ShowDialogAsync<TransparentSubtitlesWindow, TransparentSubtitlesViewModel>(Window,
            vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSyncAdjustAllTimes()
    {
        await _windowService.ShowDialogAsync<AdjustAllTimesWindow, AdjustAllTimesViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSyncChangeFrameRate()
    {
        await _windowService.ShowDialogAsync<ChangeFrameRateWindow, ChangeFrameRateViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSyncChangeSpeed()
    {
        await _windowService.ShowDialogAsync<ChangeSpeedWindow, ChangeSpeedViewModel>(Window, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandShowAutoTranslate()
    {
        var viewModel = await _windowService.ShowDialogAsync<AutoTranslateWindow, AutoTranslateViewModel>(Window,
            viewModel => { viewModel.Initialize(GetUpdateSubtitle()); });

        if (viewModel.OkPressed)
        {
            Console.WriteLine("User confirmed the action");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandShowSettings()
    {
        var oldTheme = Se.Settings.Appearance.Theme;

        var viewModel = await _windowService.ShowDialogAsync<SettingsWindow, SettingsViewModel>(Window);
        _shortcutManager.ClearKeys();
        if (!viewModel.OkPressed)
        {
            return;
        }

        if (oldTheme != viewModel.SelectedTheme)
        {
            if (viewModel.SelectedTheme == "Dark")
            {
                Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
            }
            else if (viewModel.SelectedTheme == "Light")
            {
                Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            }
            else
            {
                Application.Current!.RequestedThemeVariant = ThemeVariant.Default;
            }
        }

        if (AudioVisualizer != null)
        {
            AudioVisualizer.DrawGridLines = Se.Settings.Waveform.DrawGridLines;
            IsWaveformToolbarVisible = Se.Settings.Waveform.ShowToolbar;
            AudioVisualizer.WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor();
            AudioVisualizer.WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor();
            AudioVisualizer.InvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel;
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task CommandShowSettingsShortcuts()
    {
        await _windowService.ShowDialogAsync<ShortcutsWindow, ShortcutsViewModel>(Window,
            vm => { vm.LoadShortCuts(this); });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandShowSettingsLanguage()
    {
        var viewModel = await _windowService.ShowDialogAsync<LanguageWindow, LanguageViewModel>(Window);
        if (viewModel.OkPressed)
        {
            // todo
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task DeleteSelectedLines()
    {
        await DeleteSelectedItems();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void InsertLineBefore()
    {
        InsertBeforeSelectedItem();
    }

    [RelayCommand]
    private void InsertLineAfter()
    {
        InsertAfterSelectedItem();
    }

    [RelayCommand]
    private void MergeWithLineBefore()
    {
        MergeLineBefore();
    }

    [RelayCommand]
    private void MergeWithLineAfter()
    {
        MergeLineAfter();
    }

    [RelayCommand]
    private void MergeSelectedLines()
    {
        MergeLinesSelected();
    }

    [RelayCommand]
    private void MergeSelectedLinesDialog()
    {
        MergeLinesSelectedAsDialog();
    }

    [RelayCommand]
    private void ToggleLinesItalic()
    {
        ToggleItalic();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void ToggleLinesBold()
    {
        ToggleBold();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowRestoreAutoBackup()
    {
        var viewModel = await _windowService
            .ShowDialogAsync<RestoreAutoBackupWindow, RestoreAutoBackupViewModel>(Window);

        if (viewModel.OkPressed && !string.IsNullOrEmpty(viewModel.RestoreFileName))
        {
            await SubtitleOpen(viewModel.RestoreFileName);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowHistory()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel =
            await _windowService.ShowDialogAsync<ShowHistoryWindow, ShowHistoryViewModel>(Window, vm => { });

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowFind()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<FindWindow, FindViewModel>(Window, vm => { });

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task FindNext()
    {
        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowReplace()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<ReplaceWindow, ReplaceViewModel>(Window, vm => { });

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowMultipleReplace()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel =
            await _windowService.ShowDialogAsync<MultipleReplaceWindow, MultipleReplaceViewModel>(Window, vm => { });

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowGoToLine()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<GoToLineNumberWindow, GoToLineNumberViewModel>(Window,
            vm => { vm.MaxLineNumber = Subtitles.Count; });
        if (viewModel is { OkPressed: true, LineNumber: >= 0 } && viewModel.LineNumber < Subtitles.Count)
        {
            SelectAndScrollToRow(viewModel.LineNumber - 1);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void SelectAllLines()
    {
        SelectAllRows();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void InverseSelection()
    {
        InverseRowSelection();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void GoToNextLine()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (Subtitles.Count == 0 || idx < 0 || idx >= Subtitles.Count)
        {
            return;
        }

        idx++;
        if (idx >= Subtitles.Count)
        {
            return;
        }

        SelectAndScrollToRow(idx);
    }

    [RelayCommand]
    private void GoToPreviousLine()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (Subtitles.Count == 0 || idx < 0 || idx >= Subtitles.Count)
        {
            return;
        }

        idx--;
        if (idx < 0)
        {
            return;
        }

        SelectAndScrollToRow(idx);
    }

    private Control? _fullscreenBeforeParent;

    [RelayCommand]
    private void VideoFullScreen()
    {
        var control = VideoPlayerControl;
        if (control == null || control.IsFullScreen)
        {
            return;
        }

        var parent = (Control)control.Parent!;
        _fullscreenBeforeParent = parent;
        control.RemoveControlFromParent();
        control.IsFullScreen = true;
        var fullscreenWindow = new FullScreenVideoWindow(control, () =>
        {
            if (_fullscreenBeforeParent != null)
            {
                control.RemoveControlFromParent().AddControlToParent(_fullscreenBeforeParent);
            }

            control.IsFullScreen = false;
        });
        fullscreenWindow.Show(Window);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void Unbreak()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        s.Text = Utilities.UnbreakLine(s.Text);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void AutoBreak()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        s.Text = Utilities.AutoBreakLine(s.Text);
        _shortcutManager.ClearKeys();
    }

    private async Task<bool> RequireFfmpegOk()
    {
        if (FfmpegHelper.IsFfmpegInstalled())
        {
            return true;
        }

        if (File.Exists(DownloadFfmpegViewModel.GetFfmpegFileName()))
        {
            Se.Settings.General.FfmpegPath = DownloadFfmpegViewModel.GetFfmpegFileName();
            return true;
        }

        if (!Configuration.IsRunningOnWindows && File.Exists("/usr/local/bin/ffmpeg"))
        {
            Se.Settings.General.FfmpegPath = "/usr/local/bin/ffmpeg";
            return true;
        }

        if (Configuration.IsRunningOnWindows || Configuration.IsRunningOnMac)
        {
            var answer = await MessageBox.Show(
                Window!,
                "Download ffmpeg?",
                $"{Environment.NewLine}\"Audio to text\" requires ffmpeg.{Environment.NewLine}{Environment.NewLine}Download and use ffmpeg?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            var result = await _windowService.ShowDialogAsync<DownloadFfmpegWindow, DownloadFfmpegViewModel>(Window);
            if (!string.IsNullOrEmpty(result.FfmpegFileName))
            {
                Se.Settings.General.FfmpegPath = result.FfmpegFileName;
                ShowStatus($"ffmpeg downloaded and installed to {result.FfmpegFileName}");
                return true;
            }
        }

        return false;
    }

    private void PerformUndo()
    {
        if (!_undoRedoManager.CanUndo)
        {
            return;
        }

        var undoRedoObject = _undoRedoManager.Undo()!;
        RestoreUndoRedoState(undoRedoObject);
        ShowStatus("Undo performed");
    }

    private void PerformRedo()
    {
        if (!_undoRedoManager.CanRedo)
        {
            return;
        }

        var undoRedoObject = _undoRedoManager.Redo()!;
        RestoreUndoRedoState(undoRedoObject);
        ShowStatus("Redo performed");
    }

    private void MakeHistoryForUndo(string description)
    {
        if (Subtitles.Count == 0 || Subtitles.Count == 1 && string.IsNullOrWhiteSpace(SelectedSubtitle?.Text))
        {
            return;
        }

        var hash = GetFastSubtitleHash();
        if (hash == _changeSubtitleHash && _undoRedoManager.CanUndo)
        {
            return; // no changes
        }

        var undoRedoObject = MakeUndoRedoObject(description);
        _undoRedoManager.Do(undoRedoObject);
        _changeSubtitleHash = GetFastSubtitleHash();
    }

    private UndoRedoItem MakeUndoRedoObject(string description)
    {
        return new UndoRedoItem(
            description,
            Subtitles.ToArray(),
            _subtitleFileName,
            new int[] { 0 },
            1,
            1);
    }

    private void RestoreUndoRedoState(UndoRedoItem undoRedoObject)
    {
        Subtitles = new ObservableCollection<SubtitleLineViewModel>(undoRedoObject.Subtitles);
        _subtitleFileName = undoRedoObject.SubtitleFileName;
        SelectAndScrollToRow(undoRedoObject.SelectedLines.First());
    }

    private void SelectAllRows()
    {
        SubtitleGrid.SelectAll();
    }

    private void InverseRowSelection()
    {
        if (SubtitleGrid.SelectedItems == null || Subtitles.Count == 0)
        {
            return;
        }

        // Store currently selected items
        var selectedItems =
            new HashSet<SubtitleLineViewModel>(SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>());

        _subtitleGridSelectionChangedSkip = true;
        SubtitleGrid.SelectedItems.Clear();
        foreach (var item in Subtitles)
        {
            if (!selectedItems.Contains(item))
            {
                SubtitleGrid.SelectedItems.Add(item);
            }
        }

        _subtitleGridSelectionChangedSkip = false;
        SubtitleGridSelectionChanged();
    }

    private void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Subtitles.Count)
        {
            return;
        }

        // First, set the selected index (this selects the row)
        SubtitleGrid.SelectedIndex = index;

        // Then, scroll to the selected item to make it visible
        SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);
    }

    public void SelectAndScrollToSubtitle(SubtitleLineViewModel subtitle)
    {
        if (subtitle == null || !Subtitles.Contains(subtitle))
        {
            return;
        }

        // First, set the selected item (this selects the row)
        SubtitleGrid.SelectedItem = subtitle;

        // Then, scroll to the selected item to make it visible
        SubtitleGrid.ScrollIntoView(subtitle, null);
    }

    private async Task SubtitleOpen(string fileName, string? videoFileName = null, int? selectedSubtitleIndex = null)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var ext = Path.GetExtension(fileName);
        var fileSize = (long)0;
        try
        {
            var fi = new FileInfo(fileName);
            fileSize = fi.Length;
        }
        catch
        {
            // ignore
        }

        if (fileSize < 10)
        {
            var message = fileSize == 0 ? "File size is zero!" : $"File size too small - only {fileSize} bytes";
            await MessageBox.Show(Window, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            foreach (var f in SubtitleFormat.GetBinaryFormats(false))
            {
                if (f.IsMine(null, fileName))
                {
                    subtitle = new Subtitle();
                    f.LoadSubtitle(subtitle, null, fileName);
                    subtitle.OriginalFormat = f;
                    break; // format found, exit the loop
                }
            }

            if (subtitle == null)
            {
                var message = "Unknown format?";
                await MessageBox.Show(Window, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        _subtitleFileName = fileName;
        _subtitle = subtitle;
        _lastOpenSaveFormat = subtitle.OriginalFormat;
        SetSubtitles(_subtitle);
        ShowStatus($"Subtitle loaded: {fileName}");

        if (selectedSubtitleIndex != null)
        {
            SelectAndScrollToRow(selectedSubtitleIndex.Value);
        }

        if (Se.Settings.Video.AutoOpen)
        {
            if (!string.IsNullOrEmpty(videoFileName) && File.Exists(videoFileName))
            {
                await VideoOpenFile(videoFileName);
            }
            else if (FindVideoFileName.TryFindVideoFileName(fileName, out videoFileName))
            {
                await VideoOpenFile(videoFileName);
            }
        }

        AddToRecentFiles(true);
        _changeSubtitleHash = GetFastSubtitleHash();
    }

    private void SetSubtitles(Subtitle subtitle)
    {
        Subtitles.Clear();
        foreach (var p in subtitle.Paragraphs)
        {
            Subtitles.Add(new SubtitleLineViewModel(p));
        }

        Renumber();
    }

    public bool HasChanges()
    {
        return !IsEmpty && _changeSubtitleHash != GetFastSubtitleHash();
    }

    private async Task SaveSubtitle()
    {
        if (Subtitles == null || !Subtitles.Any())
        {
            ShowStatus("Nothing to save");
            return;
        }

        if (string.IsNullOrEmpty(_subtitleFileName))
        {
            await SaveSubtitleAs();
            return;
        }

        if (_lastOpenSaveFormat == null || _lastOpenSaveFormat.Name != SelectedSubtitleFormat.Name)
        {
            await SaveSubtitleAs();
            return;
        }

        var text = GetUpdateSubtitle().ToText(SelectedSubtitleFormat);
        await File.WriteAllTextAsync(_subtitleFileName, text);
        _changeSubtitleHash = GetFastSubtitleHash();
        _lastOpenSaveFormat = SelectedSubtitleFormat;
    }

    public Subtitle GetUpdateSubtitle()
    {
        _subtitle.Paragraphs.Clear();
        foreach (var line in Subtitles)
        {
            var p = new Paragraph()
            {
                Number = line.Number,
                StartTime = new TimeCode(line.StartTime),
                EndTime = new TimeCode(line.EndTime),
                Text = line.Text,
                Actor = line.Actor,
                Style = line.Style,
                Language = line.Language,
            };

            _subtitle.Paragraphs.Add(p);
        }

        return _subtitle;
    }

    private async Task SaveSubtitleAs()
    {
        var newFileName = "New" + SelectedSubtitleFormat.Extension;
        if (!string.IsNullOrEmpty(_subtitleFileName))
        {
            newFileName = Path.GetFileNameWithoutExtension(_subtitleFileName) + SelectedSubtitleFormat.Extension;
        }
        else if (!string.IsNullOrEmpty(_videoFileName))
        {
            newFileName = Path.GetFileNameWithoutExtension(_videoFileName) + SelectedSubtitleFormat.Extension;
        }

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window,
            SelectedSubtitleFormat,
            newFileName,
            "Save subtitle file");
        if (!string.IsNullOrEmpty(fileName))
        {
            _subtitleFileName = fileName;
            _subtitle.FileName = fileName;
            _lastOpenSaveFormat = SelectedSubtitleFormat;
            await SaveSubtitle();
            AddToRecentFiles(true);
        }
    }

    private void AddToRecentFiles(bool updateMenu)
    {
        if (string.IsNullOrEmpty(_subtitleFileName))
        {
            return;
        }

        Se.Settings.File.AddToRecentFiles(_subtitleFileName, _videoFileName ?? string.Empty, SelectedSubtitleIndex ?? 0,
            SelectedEncoding.DisplayName);
        Se.SaveSettings();

        if (updateMenu)
        {
            InitMenu.UpdateRecentFiles(this);
        }
    }

    private void ShowStatus(string message, int delayMs = 3000)
    {
        Task.Run(() => ShowStatusWithWaitAsync(message, delayMs));
    }

    private async Task ShowStatusWithWaitAsync(string message, int delayMs = 3000)
    {
        // Cancel any previous animation
        _statusFadeCts?.Cancel();
        _statusFadeCts = new CancellationTokenSource();
        var token = _statusFadeCts.Token;

        Dispatcher.UIThread.Post(() =>
        {
            StatusTextLeft = message;
            StatusTextLeftLabel.Opacity = 1;
            StatusTextLeftLabel.IsVisible = true;
        }, DispatcherPriority.Background);

        try
        {
            await Task.Delay(delayMs, token); // Wait 3 seconds, cancellable
            Dispatcher.UIThread.Post(() => { StatusTextLeft = string.Empty; }, DispatcherPriority.Background);
        }
        catch (TaskCanceledException)
        {
            // Ignore - a new message came in and interrupted
        }
    }

    internal void OnClosing()
    {
        //MediaPlayerVlc?.Dispose();
        //libVLC?.Dispose();

        AddToRecentFiles(false);
        Se.SaveSettings();
    }

    internal void OnLoaded()
    {
        if (Se.Settings.File.ShowRecentFiles)
        {
            var first = Se.Settings.File.RecentFiles.FirstOrDefault();
            if (first != null && File.Exists(first.SubtitleFileName))
            {
                SubtitleOpen(first.SubtitleFileName, first.VideoFileName, first.SelectedLine).ConfigureAwait(false);
            }
        }
    }

    private async Task VideoOpenFile(string videoFileName)
    {
        if (VideoPlayerControl == null)
        {
            return;
        }

        await VideoPlayerControl.Open(videoFileName);

        var peakWaveFileName = WavePeakGenerator.GetPeakWaveFileName(videoFileName);
        if (!File.Exists(peakWaveFileName))
        {
            if (FfmpegHelper.IsFfmpegInstalled())
            {
                var tempWaveFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
                var process = WaveFileExtractor.GetCommandLineProcess(videoFileName, -1, tempWaveFileName,
                    Configuration.Settings.General.VlcWaveTranscodeSettings, out _);
                ShowStatus("Extracting wave info...");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () =>
                {
                    await ExtractWaveformAndSpectrogram(process, tempWaveFileName, peakWaveFileName);
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
        else
        {
            ShowStatus("Loading wave info from cache...");
            var wavePeaks = WavePeakData.FromDisk(peakWaveFileName);
            if (AudioVisualizer != null)
            {
                AudioVisualizer.WavePeaks = wavePeaks;
            }
        }

        _videoFileName = videoFileName;

        //if (!_stopping)
        //{
        //    _timer.Start();
        //}
    }

    private async Task ExtractWaveformAndSpectrogram(Process process, string tempWaveFileName, string peakWaveFileName)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        process.Start();
#pragma warning restore CA1416 // Validate platform compatibility

        var token = new CancellationTokenSource().Token;
        while (!process.HasExited)
        {
            await Task.Delay(100, token);
        }

        if (process.ExitCode != 0)
        {
            ShowStatus("Failed to extract wave info.");
            return;
        }

        if (File.Exists(tempWaveFileName))
        {
            using var waveFile = new WavePeakGenerator(tempWaveFileName);
            waveFile.GeneratePeaks(0, peakWaveFileName);

            var wavePeaks = WavePeakData.FromDisk(peakWaveFileName);

            Dispatcher.UIThread.Post(() =>
            {
                if (AudioVisualizer != null)
                {
                    AudioVisualizer.WavePeaks = wavePeaks;
                }

                //if (!_stopping)
                //{
                //    _timer.Start();
                //    _audioVisualizer.InvalidateSurface();
                //    ShowStatus("Wave info loaded.");
                //}
                _updateAudioVisualizer = true;
            }, DispatcherPriority.Background);
        }
    }

    private void VideoCloseFile()
    {
        VideoPlayerControl?.Close();
    }

    private int GetFastSubtitleHash()
    {
        var pre = _subtitleFileName + SelectedEncoding.DisplayName;
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;
            hash = hash * 23 + pre.GetHashCode();

            if (_subtitle.Header != null)
            {
                hash = hash * 23 + _subtitle.Header.Trim().GetHashCode();
            }

            if (_subtitle.Footer != null)
            {
                hash = hash * 23 + _subtitle.Footer.Trim().GetHashCode();
            }

            var max = Subtitles.Count;
            for (var i = 0; i < max; i++)
            {
                var p = Subtitles[i];
                hash = hash * 23 + p.Number.GetHashCode();
                hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();

                foreach (var line in p.Text.SplitToLines())
                {
                    hash = hash * 23 + line.GetHashCode();
                }

                if (p.Style != null)
                {
                    hash = hash * 23 + p.Style.GetHashCode();
                }

                if (p.Extra != null)
                {
                    hash = hash * 23 + p.Extra.GetHashCode();
                }

                if (p.Actor != null)
                {
                    hash = hash * 23 + p.Actor.GetHashCode();
                }

                hash = hash * 23 + p.Layer.GetHashCode();
            }

            return hash;
        }
    }

    public async Task DeleteSelectedItems()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (selectedItems == null || !selectedItems.Any())
        {
            return;
        }

        if (Se.Settings.General.PromptDeleteLines)
        {
            var answer = await MessageBox.Show(
                Window!,
                "Delete lines?",
                $"Do you want to delete {selectedItems.Count} lines?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            _shortcutManager.ClearKeys();
        }

        foreach (var item in selectedItems)
        {
            Subtitles.Remove(item);
        }

        Renumber();
    }

    public void InsertBeforeSelectedItem()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            _insertService.InsertBefore(SelectedSubtitleFormat, _subtitle, Subtitles, index, string.Empty);
            Renumber();
            SelectAndScrollToRow(index);
        }
    }

    public void InsertAfterSelectedItem()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            _insertService.InsertAfter(SelectedSubtitleFormat, _subtitle, Subtitles, index, string.Empty);
            Renumber();
            SelectAndScrollToRow(index + 1);
        }
    }

    private void Renumber()
    {
        for (var index = 0; index < Subtitles.Count; index++)
        {
            Subtitles[index].Number = index + 1;
        }
    }

    public void MergeLineBefore()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            //            _mergeManager.MergeSelectedLines();
            Renumber();
            SelectAndScrollToRow(index - 1);
        }
    }

    public void MergeLineAfter()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            _mergeManager.MergeSelectedLines(Subtitles, SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList());
            Renumber();
            SelectAndScrollToRow(index - 1);
        }
    }

    public void MergeLinesSelected()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);

            _mergeManager.MergeSelectedLines(Subtitles, SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList());

            SelectAndScrollToRow(index - 1);
            Renumber();
        }
    }

    public void MergeLinesSelectedAsDialog()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            //            _mergeManager.MergeSelectedLines();
            SelectAndScrollToRow(index - 1);
            Renumber();
        }
    }

    private void ToggleItalic()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (selectedItems.Any())
        {
            foreach (var item in selectedItems)
            {
                item.Text = item.Text.Contains("<i>")
                    ? item.Text.Replace("<i>", "").Replace("</i>", "")
                    : $"<i>{item.Text}</i>";
            }
        }
    }

    private void ToggleBold()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (selectedItems.Any())
        {
            foreach (var item in selectedItems)
            {
                item.Text = item.Text.Contains("<b>")
                    ? item.Text.Replace("<b>", "").Replace("</b>", "")
                    : $"<b>{item.Text}</b>";
            }
        }
    }

    public void SubtitleContextOpening(object? sender, EventArgs e)
    {
    }

    public void KeyDown(KeyEventArgs keyEventArgs)
    {
        _shortcutManager.OnKeyPressed(this, keyEventArgs);

        var relayCommand = _shortcutManager.CheckShortcuts(ShortcutCategory.SubtitleGrid.ToStringInvariant());
        if (relayCommand != null)
        {
            keyEventArgs.Handled = true;
            relayCommand.Execute(null);
            return;
        }

        relayCommand = _shortcutManager.CheckShortcuts(ShortcutCategory.General.ToStringInvariant());
        if (relayCommand != null)
        {
            keyEventArgs.Handled = true;
            relayCommand.Execute(null);
            return;
        }
    }

    public void OnKeyUpHandler(object? sender, KeyEventArgs e)
    {
        _shortcutManager.OnKeyReleased(this, e);
    }

    private bool _subtitleGridIsRightClick = false;
    private bool _subtitleGridIsLeftClick = false;
    private bool _subtitleGridIsControlPressed = false;

    public void SubtitleGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _subtitleGridIsControlPressed = false;
        _subtitleGridIsLeftClick = false;
        _subtitleGridIsRightClick = false;

        if (sender is Control { ContextFlyout: not null } control)
        {
            var props = e.GetCurrentPoint(control).Properties;
            _subtitleGridIsLeftClick = props.IsLeftButtonPressed;
            _subtitleGridIsRightClick = props.IsRightButtonPressed;
            _subtitleGridIsControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        }
    }

    public void SubtitleGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Control { ContextFlyout: MenuFlyout menuFlyout } control)
        {
            if (_subtitleGridIsRightClick)
            {
                menuFlyout.ShowAt(control, true);
            }

            if (OperatingSystem.IsMacOS())
            {
                if (_subtitleGridIsLeftClick && _subtitleGridIsControlPressed)
                {
                    menuFlyout.ShowAt(control, true);
                    e.Handled = true;
                }
            }
        }
    }

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_subtitleGridSelectionChangedSkip)
        {
            return;
        }

        SubtitleGridSelectionChanged();
    }

    private void SubtitleGridSelectionChanged()
    {
        var selectedItems = SubtitleGrid.SelectedItems;

        if (selectedItems == null)
        {
            SelectedSubtitle = null;
            SelectedSubtitleIndex = null;
            _selectedSubtitles = null;
            StatusTextRight = string.Empty;
            EditTextCharactersPerSecond = string.Empty;
            EditTextTotalLength = string.Empty;
            EditTextLineLengths = string.Empty;
            return;
        }

        _selectedSubtitles = selectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count > 1)
        {
            StatusTextRight = $"{selectedItems.Count} lines selected of {Subtitles.Count}";
            EditTextCharactersPerSecond = string.Empty;
            EditTextTotalLength = string.Empty;
            EditTextLineLengths = string.Empty;
            return;
        }

        var item = _selectedSubtitles.FirstOrDefault();
        if (item == null)
        {
            SelectedSubtitle = null;
            SelectedSubtitleIndex = null;
            StatusTextRight = string.Empty;
            return;
        }

        SelectedSubtitle = item;
        SelectedSubtitleIndex = Subtitles.IndexOf(item);
        StatusTextRight = $"{Subtitles.IndexOf(item) + 1}/{Subtitles.Count}";

        MakeSubtitleTextInfo(item.Text, item);
        _updateAudioVisualizer = true;
    }

    private void MakeSubtitleTextInfo(string text, SubtitleLineViewModel item)
    {
        text = HtmlUtil.RemoveHtmlTags(text, true);
        var totalLength = text.CountCharacters(false);
        var cps = new Paragraph(text, item.StartTime.TotalMilliseconds, item.EndTime.TotalMilliseconds)
            .GetCharactersPerSecond();

        var lines = text.SplitToLines();
        var lineLenghts = new List<string>(lines);
        for (var i = 0; i < lines.Count; i++)
        {
            lineLenghts[i] = $"{lines[i].Length}";
        }

        EditTextCharactersPerSecond = $"Chars/sec: {cps:0.0}";
        EditTextTotalLength = $"Total length: {totalLength}";
        EditTextLineLengths = $"Line length: {string.Join('/', lineLenghts)}";
    }

    private DispatcherTimer _positionTimer = new DispatcherTimer();

    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _positionTimer.Tick += (s, e) =>
        {
            var text = "Untitled";
            if (!string.IsNullOrEmpty(_subtitleFileName))
            {
                text = Configuration.Settings.General.TitleBarFullFileName
                    ? _subtitleFileName
                    : Path.GetFileName(_subtitleFileName);
            }

            text = text + " - " + "Subtitle Edit 5.0 Alpha";
            if (_changeSubtitleHash != GetFastSubtitleHash())
            {
                text = "*" + text;
            }

            Window.Title = text;

            // update audio visualizer position if available
            var av = AudioVisualizer;
            var vp = VideoPlayerControl;
            if (av != null && vp != null)
            {
                var subtitle = new ObservableCollection<SubtitleLineViewModel>();
                var orderedList = Subtitles.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
                var firstSelectedIndex = -1;
                for (var i = 0; i < orderedList.Count; i++)
                {
                    var dp = orderedList[i];
                    subtitle.Add(dp);
                }

                var mediaPlayerSeconds = vp.Position;
                var startPos = mediaPlayerSeconds - 0.01;
                if (startPos < 0)
                {
                    startPos = 0;
                }

                av.CurrentVideoPositionSeconds = vp.Position;
                var isPlaying = vp.IsPlaying;

                if (Se.Settings.Waveform.CenterVideoPosition)
                {
                    // calculate the center position based on the waveform width
                    var waveformHalfSeconds = (av.EndPositionSeconds - av.StartPositionSeconds) / 2.0;
                    av.SetPosition(Math.Max(0, mediaPlayerSeconds - waveformHalfSeconds), subtitle, mediaPlayerSeconds,
                        firstSelectedIndex, _selectedSubtitles ?? new List<SubtitleLineViewModel>());
                }
                else if ((isPlaying || !av.IsScrolling) && (mediaPlayerSeconds > av.EndPositionSeconds ||
                                                            mediaPlayerSeconds < av.StartPositionSeconds))
                {
                    av.SetPosition(startPos, subtitle, mediaPlayerSeconds, 0,
                        _selectedSubtitles ?? new List<SubtitleLineViewModel>());
                }
                else
                {
                    av.SetPosition(av.StartPositionSeconds, subtitle, mediaPlayerSeconds, firstSelectedIndex,
                        _selectedSubtitles ?? new List<SubtitleLineViewModel>());
                }

                if (_updateAudioVisualizer)
                {
                    av.InvalidateVisual();
                    _updateAudioVisualizer = false;
                }
            }
        };
        _positionTimer.Start();
    }


    public void SubtitleTextChanged(object? sender, TextChangedEventArgs e)
    {
        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        MakeSubtitleTextInfo(selectedSubtitle.Text, selectedSubtitle);
        _updateAudioVisualizer = true;
    }

    public void AudioVisualizerOnNewSelectionInsert(object sender, ParagraphEventArgs e)
    {
        var index = _insertService.InsertInCorrectPosition(Subtitles, e.Paragraph);
        SelectAndScrollToRow(index);

        if (Se.Settings.Waveform.FocusTextBoxAfterInsertNew)
        {
            Dispatcher.UIThread.Post(() => { EditTextBox.Focus(); }, DispatcherPriority.Background);
        }
    }

    internal void AudioVisualizerOnVideoPositionChanged(object sender, AudioVisualizer.PositionEventArgs e)
    {
        var vp = VideoPlayerControl;
        if (vp == null)
        {
            return;
        }

        var newPosition = e.PositionInSeconds;
        newPosition = Math.Max(0, newPosition);
        newPosition = Math.Min(vp.Duration, newPosition);

        VideoPlayerControl?.SetPosition(newPosition);
        _updateAudioVisualizer = true; // Update the audio visualizer position
    }

    internal void AudioVisualizerOnStatus(object sender, ParagraphEventArgs e)
    {
        ShowStatus(e.Paragraph.Text, 3000);
    }

    internal void OnSubtitleGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem != null)
        {
            if (grid.SelectedItem is SubtitleLineViewModel selectedItem)
            {
                if (!string.IsNullOrEmpty(_videoFileName) && VideoPlayerControl != null)
                {
                    VideoPlayerControl.Position = selectedItem.StartTime.TotalSeconds;
                }
            }
        }
    }

    internal void OnWaveformDoubleTapped(object sender, ParagraphEventArgs e)
    {
        if (!string.IsNullOrEmpty(_videoFileName) && VideoPlayerControl != null)
        {
            VideoPlayerControl.Position = e.Seconds;
            var p = Subtitles.FirstOrDefault(p =>
                Math.Abs(p.StartTime.TotalMilliseconds - e.Paragraph.StartTime.TotalMilliseconds) < 0.01);
            if (p != null)
            {
                SelectAndScrollToSubtitle(p);
            }
        }
    }

    public void AudioVisualizerOnAddToSelection(object sender, ParagraphEventArgs e)
    {
        if (!string.IsNullOrEmpty(_videoFileName) && VideoPlayerControl != null)
        {
            var p = Subtitles.FirstOrDefault(p =>
                Math.Abs(p.StartTime.TotalMilliseconds - e.Paragraph.StartTime.TotalMilliseconds) < 0.01);
            if (p != null)
            {
                SubtitleGrid.SelectedItems.Add(p);
            }
        }
    }
}