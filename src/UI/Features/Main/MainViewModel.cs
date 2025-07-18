using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.Validators;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
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
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.Ocr;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
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
using Nikse.SubtitleEdit.Logic.Config.Language;
using Nikse.SubtitleEdit.Logic.Initializers;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.UndoRedo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Features.Files.Export;
using Nikse.SubtitleEdit.Features.Files.Export.ExportEbuStl;
using Nikse.SubtitleEdit.Features.Files.ExportEbuStl;
using Nikse.SubtitleEdit.Features.Files.ExportCavena890;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Files.ExportPac;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel :
    ObservableObject,
    IAdjustCallback,
    IFocusSubtitleLine,
    IUndoRedoClient
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    private List<SubtitleLineViewModel>? _selectedSubtitles;
    [ObservableProperty] private int? _selectedSubtitleIndex;
    [ObservableProperty] private bool _showColumnOriginalText;

    [ObservableProperty] private string _editText;
    [ObservableProperty] private string _editTextCharactersPerSecond;
    [ObservableProperty] private IBrush _editTextCharactersPerSecondBackground;
    [ObservableProperty] private string _editTextTotalLength;
    [ObservableProperty] private IBrush _editTextTotalLengthBackground;
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
    public Window? Window { get; set; }
    public Grid ContentGrid { get; set; }
    public MainView? MainView { get; set; }
    public TextBlock StatusTextLeftLabel { get; set; }
    public MenuItem MenuReopen { get; set; }
    public AudioVisualizer? AudioVisualizer { get; set; }

    private static Color _errorColor = Se.Settings.General.ErrorColor.FromHexToColor();

    private bool _updateAudioVisualizer = false;
    private string? _subtitleFileName;
    private Subtitle _subtitle;
    private SubtitleFormat? _lastOpenSaveFormat;
    private string? _videoFileName;
    private CancellationTokenSource? _statusFadeCts;
    private int _changeSubtitleHash = -1;
    private bool _subtitleGridSelectionChangedSkip;
    private long _lastKeyPressedTicks;

    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IShortcutManager _shortcutManager;
    private readonly IWindowService _windowService;
    private readonly IInsertService _insertService;
    private readonly IMergeManager _mergeManager;
    private readonly IAutoBackupService _autoBackupService;
    private readonly IUndoRedoManager _undoRedoManager;
    private readonly IBluRayHelper _bluRayHelper;
    private readonly IMpvReloader _mpvReloader;
    private readonly IFindService _findService;

    private bool IsEmpty => Subtitles.Count == 0 || string.IsNullOrEmpty(Subtitles[0].Text);

    public VideoPlayerControl? VideoPlayerControl { get; internal set; }
    public Menu Menu { get; internal set; }
    public StackPanel PanelSingleLineLenghts { get; internal set; }
    public MenuItem MenuItemMergeAsDialog { get; internal set; }

    public MainViewModel(
        IFileHelper fileHelper,
        IFolderHelper folderHelper,
        IShortcutManager shortcutManager,
        IWindowService windowService,
        IInsertService insertService,
        IMergeManager mergeManager,
        IAutoBackupService autoBackupService,
        IUndoRedoManager undoRedoManager,
        IBluRayHelper bluRayHelper,
        IMpvReloader mpvReloader,
        IFindService findService,
        IDictionaryInitializer dictionaryInitializer,
        ILanguageInitializer languageInitializer,
        IOcrInitializer ocrInitializer,
        IThemeInitializer themeInitializer)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _shortcutManager = shortcutManager;
        _windowService = windowService;
        _insertService = insertService;
        _mergeManager = mergeManager;
        _autoBackupService = autoBackupService;
        _undoRedoManager = undoRedoManager;
        _bluRayHelper = bluRayHelper;
        _mpvReloader = mpvReloader;
        _findService = findService;

        EditText = string.Empty;
        EditTextCharactersPerSecond = string.Empty;
        EditTextCharactersPerSecondBackground = Brushes.Transparent;
        EditTextTotalLength = string.Empty;
        EditTextTotalLengthBackground = Brushes.Transparent;
        EditTextLineLengths = string.Empty;
        StatusTextLeftLabel = new TextBlock();
        SubtitleGrid = new DataGrid();
        EditTextBox = new TextBox();
        ContentGrid = new Grid();
        MenuReopen = new MenuItem();
        Menu = new Menu();
        PanelSingleLineLenghts = new StackPanel();
        MenuItemMergeAsDialog = new MenuItem();
        _subtitle = new Subtitle();
        _videoFileName = string.Empty;
        _subtitleFileName = string.Empty;
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        SubtitleFormats = [.. SubtitleFormat.AllSubtitleFormats];
        SelectedSubtitleFormat = SubtitleFormats[0];
        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        SelectedEncoding = Encodings[0];
        StatusTextLeft = string.Empty;
        StatusTextRight = string.Empty;

        themeInitializer.UpdateThemesIfNeeded().ConfigureAwait(true);
        Dispatcher.UIThread.Post(async () =>
        {
            await languageInitializer.UpdateLanguagesIfNeeded();
            await dictionaryInitializer.UpdateDictionariesIfNeeded();
            await ocrInitializer.UpdateOcrDictionariesIfNeeded();
        }, DispatcherPriority.Loaded);
        InitializeLibMpv();
        InitializeFfmpeg();
        LoadShortcuts();
        _isWaveformToolbarVisible = Se.Settings.Waveform.ShowToolbar;

        StartTitleTimer();
        _autoBackupService.StartAutoBackup(this);
        _undoRedoManager.SetupChangeDetection(this, TimeSpan.FromSeconds(1));
    }

    private void InitializeFfmpeg()
    {
        if (!string.IsNullOrEmpty(Se.Settings.General.FfmpegPath) &&
            File.Exists(Se.Settings.General.FfmpegPath))
        {
            return;
        }

        var ffmpegFileName = DownloadFfmpegViewModel.GetFfmpegFileName();
        if (!string.IsNullOrEmpty(ffmpegFileName) && File.Exists(ffmpegFileName))
        {
            Se.Settings.General.FfmpegPath = DownloadFfmpegViewModel.GetFfmpegFileName();
        }
    }

    private static void InitializeLibMpv()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var newFileName = DownloadLibMpvViewModel.GetFallbackLibMpvFileName(false);
            if (string.IsNullOrEmpty(Se.Settings.General.LibMpvPath) || File.Exists(newFileName))
            {
                var libMpvFileName = DownloadLibMpvViewModel.GetLibMpvFileName();
                if (File.Exists(newFileName))
                {
                    try
                    {
                        if (File.Exists(libMpvFileName))
                        {
                            File.Delete(libMpvFileName);
                        }
                        File.Move(newFileName, libMpvFileName);

                        Directory.Delete(Path.GetDirectoryName(newFileName)!);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                if (File.Exists(libMpvFileName))
                {
                    Se.Settings.General.LibMpvPath = libMpvFileName;
                }
            }
        }
    }

    private void LoadShortcuts()
    {
        Se.Settings.InitializeMainShortcuts(this);
        foreach (var shortCut in ShortcutsMain.GetUsedShortcuts(this))
        {
            _shortcutManager.RegisterShortcut(shortCut);
        }
    }

    private void ReloadShortcuts()
    {
        _shortcutManager.ClearShortcuts();
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
                Window!,
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
        var vm = await _windowService.ShowDialogAsync<LayoutWindow, LayoutViewModel>(Window!,
            viewModel => { viewModel.SelectedLayout = Se.Settings.General.LayoutNumber; });

        if (vm.OkPressed && vm.SelectedLayout != null && vm.SelectedLayout != Se.Settings.General.LayoutNumber)
        {
            Se.Settings.General.LayoutNumber = InitLayout.MakeLayout(MainView!, this, vm.SelectedLayout.Value);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void TogglePlayPause()
    {
        var control = VideoPlayerControl;
        if (control == null)
        {
            return;
        }

        control.TogglePlayPause();

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void TogglePlayPause2()
    {
        TogglePlayPause();
    }

    [RelayCommand]
    private async Task ShowHelp()
    {
        await Window!.Launcher.LaunchUriAsync(new Uri("https://www.nikse.dk/subtitleedit/help"));
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task SaveLanguageFile()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(Se.Language, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        });

        var currentDirectory = Directory.GetCurrentDirectory();
        var fileName = Path.Combine(currentDirectory, "English.json");
        await File.WriteAllTextAsync(fileName, json, Encoding.UTF8);
        ShowStatus($"Language file saved to {fileName}");
        await _folderHelper.OpenFolder(Window!, currentDirectory);

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowAbout()
    {
        var newWindow = new AboutWindow();
        await newWindow.ShowDialog(Window!);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileNew()
    {
        if (HasChanges())
        {
            var result = await MessageBox.Show(
                Window!,
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

        ResetSubtitle();
        VideoCloseFile();
    }

    private void ResetSubtitle()
    {
        Subtitles.Clear();
        _subtitleFileName = string.Empty;
        _subtitle = new Subtitle();
        _changeSubtitleHash = GetFastHash();
        if (AudioVisualizer?.WavePeaks != null)
        {
            AudioVisualizer.WavePeaks = null;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileOpen()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, "Open subtitle file");
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
        var result = await _windowService.ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(Window!, vm =>
        {
            vm.Initialize(ExportImageType.BluRaySup, Subtitles, _subtitleFileName, _videoFileName);
        });
        
        if (!result.OkPressed)
        {
            return;
        }
        
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportCapMakerPlus()
    {
        var format = new CapMakerPlus();
        using var ms = new MemoryStream();
        format.Save(_subtitleFileName, ms, GetUpdateSubtitle(), false);

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
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
        var result = await _windowService.ShowDialogAsync<ExportCavena890Window, ExportCavena890ViewModel>(Window!);
        if (!result.OkPressed)
        {
            return;
        }

        var cavena = new Cavena890();
        using var ms = new MemoryStream();
        cavena.Save(_subtitleFileName, ms, GetUpdateSubtitle(), false);

        var suggestedFileName = string.Empty;
        if (!string.IsNullOrEmpty(_subtitleFileName))
        {
            suggestedFileName = Path.GetFileNameWithoutExtension(_subtitleFileName);
        }

        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, cavena, suggestedFileName, string.Format(Se.Language.Main.SaveXFileAs, cavena.Name));
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        cavena.Save(fileName, ms, GetUpdateSubtitle(), false);
        ms.Position = 0;
        File.WriteAllBytes(fileName, ms.ToArray());

        ShowStatus($"File exported in format {cavena.Name} to {fileName}");

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportPac()
    {
        var result = await _windowService.ShowDialogAsync<ExportPacWindow, ExportPacViewModel>(Window!);
        if (!result.OkPressed || result.PacCodePage == null)
        {
            return;
        }

        var pac = new Pac { CodePage = result.PacCodePage!.Value };
        using var ms = new MemoryStream();
        var suggestedFileName = string.Empty;
        if (!string.IsNullOrEmpty(_subtitleFileName))
        {
            suggestedFileName = Path.GetFileNameWithoutExtension(_subtitleFileName);
        }

        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, pac, suggestedFileName, string.Format(Se.Language.Main.SaveXFileAs, pac.Name));
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        pac.Save(fileName, ms, GetUpdateSubtitle(), false);
        ms.Position = 0;
        await File.WriteAllBytesAsync(fileName, ms.ToArray());

        ShowStatus($"File exported in format {pac.Name} to {fileName}");

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportPacUnicode()
    {
        var format = new PacUnicode();
        using var ms = new MemoryStream();
        format.Save(_subtitleFileName, ms, GetUpdateSubtitle());

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            format,
            "newFileName",
            $"Save {format.Name} file as");

        if (!string.IsNullOrEmpty(fileName))
        {
            await File.WriteAllBytesAsync(fileName, ms.ToArray());
            ShowStatus($"File exported in format {format.Name} to {fileName}");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportEbuStl()
    {
        var result = await _windowService.ShowDialogAsync<ExportEbuStlWindow, ExportEbuStlViewModel>(Window!, vm =>
        {
            vm.Initialize(GetUpdateSubtitle());
        });

        if (!result.OkPressed)
        {
            return;
        }

        var format = new Ebu();
        using var ms = new MemoryStream();
        format.Save(_subtitleFileName, ms, GetUpdateSubtitle(), false);

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            format,
            "newFileName",
            $"Save {format.Name} file as");

        if (!string.IsNullOrEmpty(fileName))
        {
            await File.WriteAllBytesAsync(fileName, ms.ToArray());
            ShowStatus($"File exported in format {format.Name} to {fileName}");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void Undo()
    {
        PerformUndo();
    }

    [RelayCommand]
    private void Redo()
    {
        PerformRedo();
    }

    [RelayCommand]
    private async Task ShowToolsAdjustDurations()
    {
        var result = await _windowService.ShowDialogAsync<AdjustDurationWindow, AdjustDurationViewModel>(Window!, vm => { });

        if (result.OkPressed)
        {
            result.AdjustDuration(Subtitles);
            _updateAudioVisualizer = true;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsBatchConvert()
    {
        await _windowService.ShowDialogAsync<BatchConvertWindow, BatchConvertViewModel>(Window!, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsChangeCasing()
    {
        var result = await _windowService.ShowDialogAsync<ChangeCasingWindow, ChangeCasingViewModel>(Window!, vm =>
        {
            vm.Initialize(GetUpdateSubtitle());
        });

        if (result.OkPressed)
        {
            for (var i = 0; i < Subtitles.Count; i++)
            {
                if (result.Subtitle.Paragraphs.Count <= i)
                {
                    break;
                }
                Subtitles[i].Text = result.Subtitle.Paragraphs[i].Text;
            }
            ShowStatus(result.Info);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsFixCommonErrors()
    {
        var viewModel =
            await _windowService.ShowDialogAsync<FixCommonErrorsWindow, FixCommonErrorsViewModel>(Window!,
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
        var result = await _windowService
            .ShowDialogAsync<RemoveTextForHearingImpairedWindow, RemoveTextForHearingImpairedViewModel>(
                Window!, vm =>
                {
                    vm.Initialize(GetUpdateSubtitle());
                });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            Subtitles.AddRange(result.FixedSubtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p)));
            SelectAndScrollToRow(0);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandVideoOpen()
    {
        var fileName = await _fileHelper.PickOpenVideoFile(Window!, "Open video file");
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
        var result = await _windowService.ShowDialogAsync<SpellCheckWindow, SpellCheckViewModel>(Window!, vm =>
        {
            vm.Initialize(Subtitles, SelectedSubtitleIndex, this);
        });

        if (result.OkPressed && result.TotalChangedWords > 0)
        {
            ShowStatus(StatusTextRight = $"{result.TotalChangedWords} words corrected in spell check");
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowSpellCheckDictionaries()
    {
        await _windowService.ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>(Window!, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoAudioToTextWhisper()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AudioToTextWhisperWindow, AudioToTextWhisperViewModel>(Window!, vm =>
        {
            vm.Initialize(_videoFileName);
        });

        if (result.OkPressed && !result.IsBatchMode)
        {
            _subtitle = result.TranscribedSubtitle;
            SetSubtitles(_subtitle);
            SelectAndScrollToRow(0);
            ShowStatus($"Transcription completed with {result.TranscribedSubtitle.Paragraphs.Count} lines");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoBurnIn()
    {
        await _windowService.ShowDialogAsync<BurnInWindow, BurnInViewModel>(Window!, vm =>
        {
            vm.Initialize(_videoFileName ?? string.Empty, GetUpdateSubtitle(), SelectedSubtitleFormat);
        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoOpenFromUrl()
    {
        var result = await _windowService.ShowDialogAsync<OpenFromUrlWindow, OpenFromUrlViewModel>(Window!);

        if (result.OkPressed)
        {
            var videoFileName = result.Url.Trim();
            if (!string.IsNullOrEmpty(videoFileName))
            {
                await VideoOpenFile(videoFileName);
                _videoFileName = videoFileName;
            }
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoTextToSpeech()
    {
        await _windowService.ShowDialogAsync<TextToSpeechWindow, TextToSpeechViewModel>(Window!, vm =>
        {
            vm.Initialize(GetUpdateSubtitle(), _videoFileName ?? string.Empty, AudioVisualizer?.WavePeaks, Path.GetTempPath());
        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoTransparentSubtitles()
    {
        await _windowService.ShowDialogAsync<TransparentSubtitlesWindow, TransparentSubtitlesViewModel>(Window!, vm =>
        {
            vm.Initialize(_videoFileName ?? string.Empty, GetUpdateSubtitle(), SelectedSubtitleFormat);
        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSyncAdjustAllTimes()
    {
        var result = await _windowService.ShowDialogAsync<AdjustAllTimesWindow, AdjustAllTimesViewModel>(Window!, vm =>
        {
            vm.Initialize(this); // uses call from IAdjustCallback: Adjust
        });

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSyncChangeFrameRate()
    {
        await _windowService.ShowDialogAsync<ChangeFrameRateWindow, ChangeFrameRateViewModel>(Window!, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSyncChangeSpeed()
    {
        await _windowService.ShowDialogAsync<ChangeSpeedWindow, ChangeSpeedViewModel>(Window!, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandShowAutoTranslate()
    {
        var result = await _windowService.ShowDialogAsync<AutoTranslateWindow, AutoTranslateViewModel>(Window!,
            viewModel => { viewModel.Initialize(GetUpdateSubtitle()); });

        if (result.OkPressed)
        {
            for (var i = 0; i < Subtitles.Count; i++)
            {
                if (result.Rows.Count <= i)
                {
                    break;
                }

                Subtitles[i].Text = result.Rows[i].TranslatedText;
            }
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandShowSettings()
    {
        var oldTheme = Se.Settings.Appearance.Theme;

        var viewModel = await _windowService.ShowDialogAsync<SettingsWindow, SettingsViewModel>(Window!);

        if (!viewModel.OkPressed)
        {
            _shortcutManager.ClearKeys();
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

        _errorColor = Se.Settings.General.ErrorColor.FromHexToColor();
        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandShowSettingsShortcuts()
    {
        await _windowService.ShowDialogAsync<ShortcutsWindow, ShortcutsViewModel>(Window!, vm =>
        {
            vm.LoadShortCuts(this);
        });
        ReloadShortcuts();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandShowSettingsLanguage()
    {
        var viewModel = await _windowService.ShowDialogAsync<LanguageWindow, LanguageViewModel>(Window!);
        if (viewModel.OkPressed && viewModel.SelectedLanguage != null)
        {
            var jsonFileName = viewModel.SelectedLanguage.FileName;
            var json = await File.ReadAllTextAsync(jsonFileName, Encoding.UTF8);
            var language = System.Text.Json.JsonSerializer.Deserialize<SeLanguage>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            Se.Language = language ?? new SeLanguage();

            // reload current layout
            InitMenu.Make(this);
            InitLayout.MakeLayout(MainView!, this, Se.Settings.General.LayoutNumber);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task OpenDataFolder()
    {
        await _folderHelper.OpenFolder(Window!, Se.DataFolder);
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
        _undoRedoManager.StopChangeDetection();
        InsertBeforeSelectedItem();
        _undoRedoManager.StartChangeDetection();

    }

    [RelayCommand]
    private void InsertLineAfter()
    {
        _undoRedoManager.StopChangeDetection();
        InsertAfterSelectedItem();
        _undoRedoManager.StartChangeDetection();
    }

    [RelayCommand]
    private void MergeWithLineBefore()
    {
        _undoRedoManager.StopChangeDetection();
        MergeLineBefore();
        _undoRedoManager.StartChangeDetection();
    }

    [RelayCommand]
    private void MergeWithLineAfter()
    {
        _undoRedoManager.StopChangeDetection();
        MergeLineAfter();
        _undoRedoManager.StartChangeDetection();
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
            .ShowDialogAsync<RestoreAutoBackupWindow, RestoreAutoBackupViewModel>(Window!);

        if (viewModel.OkPressed && !string.IsNullOrEmpty(viewModel.RestoreFileName))
        {
            await SubtitleOpen(viewModel.RestoreFileName);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowHistory()
    {
        _undoRedoManager.CheckForChanges(null);
        _undoRedoManager.StopChangeDetection();

        var result = await _windowService.ShowDialogAsync<ShowHistoryWindow, ShowHistoryViewModel>(Window!, vm =>
        {
            vm.Initialize(_undoRedoManager);
        });

        if (result.OkPressed && result.SelectedHistoryItem != null)
        {
            for (int i = 0; i <= _undoRedoManager.UndoCount; i++)
            {
                var undoItem = _undoRedoManager.Undo();
                if (undoItem?.Hash == result.SelectedHistoryItem.Hash)
                {
                    RestoreUndoRedoState(undoItem);
                    ShowUndoStatus();
                    break;
                }
            }
        }

        _undoRedoManager.StartChangeDetection();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowFind()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<FindWindow, FindViewModel>(Window!, vm => { });

        if (result.OkPressed && !string.IsNullOrEmpty(result.SearchText))
        {
            _findService.Initialize(Subtitles.Select(p => p.Text).ToList(), SelectedSubtitleIndex ?? 0, Se.Settings.Edit.Find.FindWholeWords, FindService.FindMode.CaseSensitive);
            var idx = _findService.Find(result.SearchText);
            SelectAndScrollToRow(idx);
            ShowStatus($"'{_findService.SearchText}' found in line '{idx + 1}'");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void FindNext()
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

        var viewModel = await _windowService.ShowDialogAsync<ReplaceWindow, ReplaceViewModel>(Window!, vm => { });

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowMultipleReplace()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var result =
            await _windowService.ShowDialogAsync<MultipleReplaceWindow, MultipleReplaceViewModel>(Window!, vm =>
            {
                vm.Initialize(GetUpdateSubtitle());
            });

        if (result.OkPressed)
        {
            SetSubtitles(result.FixedSubtitle);
            SelectAndScrollToRow(0);
            ShowStatus($"Replaced {result.TotalReplaced} occurrences");
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowGoToLine()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<GoToLineNumberWindow, GoToLineNumberViewModel>(Window!,
            vm => { vm.MaxLineNumber = Subtitles.Count; });
        if (viewModel is { OkPressed: true, LineNumber: >= 0 } && viewModel.LineNumber <= Subtitles.Count)
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
        fullscreenWindow.Show(Window!);
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
                $"{Environment.NewLine}\"Speech to text\" requires ffmpeg.{Environment.NewLine}{Environment.NewLine}Download and use ffmpeg?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            var result = await _windowService.ShowDialogAsync<DownloadFfmpegWindow, DownloadFfmpegViewModel>(Window!);
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

        _undoRedoManager.CheckForChanges(null);
        _undoRedoManager.StopChangeDetection();
        var undoRedoObject = _undoRedoManager.Undo()!;
        RestoreUndoRedoState(undoRedoObject);
        ShowUndoStatus();
        _undoRedoManager.StartChangeDetection();
    }

    private void ShowUndoStatus()
    {
        if (_undoRedoManager.CanUndo)
        {
            ShowStatus(string.Format(Se.Language.Main.UndoPerformedXActionLeft, _undoRedoManager.UndoList.Count));
        }
        else
        {
            ShowStatus(Se.Language.Main.UndoPerformed);
        }
    }

    private void ShowRedoStatus()
    {
        if (_undoRedoManager.CanRedo)
        {
            ShowStatus(string.Format(Se.Language.Main.RedoPerformedXActionLeft, _undoRedoManager.RedoList.Count));
        }
        else
        {
            ShowStatus(Se.Language.Main.RedoPerformed);
        }
    }

    private void PerformRedo()
    {
        _undoRedoManager.CheckForChanges(null);
        if (!_undoRedoManager.CanRedo)
        {
            return;
        }

        _undoRedoManager.StopChangeDetection();
        var undoRedoObject = _undoRedoManager.Redo()!;
        RestoreUndoRedoState(undoRedoObject);
        ShowRedoStatus();
        _undoRedoManager.StartChangeDetection();
    }

    public UndoRedoItem MakeUndoRedoObject(string description)
    {
        return new UndoRedoItem(
            description,
            Subtitles.Select(p => new SubtitleLineViewModel(p)).ToArray(),
            GetFastHash(),
            _subtitleFileName,
            [SelectedSubtitleIndex ?? 0],
            1,
            1);
    }

    private void RestoreUndoRedoState(UndoRedoItem undoRedoObject)
    {
        Subtitles.Clear();
        foreach (var p in undoRedoObject.Subtitles)
        {
            Subtitles.Add(p);
        }

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

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleGrid.SelectedIndex = index;
            SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);
        }, DispatcherPriority.Background);
    }

    public void SelectAndScrollToSubtitle(SubtitleLineViewModel subtitle)
    {
        if (subtitle == null || !Subtitles.Contains(subtitle))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleGrid.SelectedItem = subtitle;
            SubtitleGrid.ScrollIntoView(subtitle, null);
        }, DispatcherPriority.Background);
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
            await MessageBox.Show(Window!, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {

            if (FileUtil.IsMatroskaFileFast(fileName) && FileUtil.IsMatroskaFile(fileName))
            {
                ImportSubtitleFromMatroskaFile(fileName, videoFileName);
                return;
            }

            if (ext == ".sup" && FileUtil.IsBluRaySup(fileName))
            {
                var log = new StringBuilder();
                var subtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
                if (subtitles.Count > 0)
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        var result = await _windowService.ShowDialogAsync<OcrWindow, OcrViewModel>(Window!, vm =>
                        {
                            vm.Initialize(subtitles, fileName);
                        });

                        if (result.OkPressed)
                        {
                            _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                            Subtitles.Clear();
                            Subtitles.AddRange(result.OcredSubtitle);
                        }
                    });
                    return;
                }
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
                    await MessageBox.Show(Window!, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            SelectedSubtitleFormat = SubtitleFormats.FirstOrDefault(p => p.Name == subtitle.OriginalFormat.Name) ?? SelectedSubtitleFormat; ;

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
            _changeSubtitleHash = GetFastHash();

        }
        finally
        {
            _undoRedoManager.Do(MakeUndoRedoObject("Open subtitle file " + _subtitleFileName));
            _undoRedoManager.StartChangeDetection();
        }
    }

    private void ImportSubtitleFromMatroskaFile(string fileName, string? videoFileName)
    {
        var matroska = new MatroskaFile(fileName);
        var subtitleList = matroska.GetTracks(true);
        if (subtitleList.Count == 0)
        {
            matroska.Dispose();
            Dispatcher.UIThread.Post(async () =>
            {
                var answer = await MessageBox.Show(
                    Window!,
                    "No subtitle found",
                    "The Matroska file does not seem to contain any subtitles.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            });
            return;
        }

        if (subtitleList.Count > 1)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var result = await _windowService.ShowDialogAsync<PickMatroskaTrackWindow, PickMatroskaTrackViewModel>(Window!, vm =>
                {
                    vm.Initialize(matroska, subtitleList, fileName);
                });
                if (result.OkPressed && result.SelectedMatroskaTrack != null)
                {
                    if (LoadMatroskaSubtitle(result.SelectedMatroskaTrack, matroska))
                    {
                        _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);

                        if (Se.Settings.General.AutoOpenVideo)
                        {
                            if (fileName.EndsWith("mkv", StringComparison.OrdinalIgnoreCase))
                            {
                                await VideoOpenFile(fileName);
                            }
                            else
                            {
                                // load video?
                            }
                        }
                    }
                }

                matroska.Dispose();
            });
        }
        else
        {
            var ext = Path.GetExtension(matroska.Path).ToLowerInvariant();
            if (LoadMatroskaSubtitle(subtitleList[0], matroska))
            {
                if (!Configuration.Settings.General.DisableVideoAutoLoading)
                {
                    if (ext == ".mkv")
                    {
                        Dispatcher.UIThread.Post(async () =>
                        {
                            await VideoOpenFile(matroska.Path);
                        });
                    }
                    else
                    {
                        if (FindVideoFileName.TryFindVideoFileName(matroska.Path, out videoFileName))
                        {
                            Dispatcher.UIThread.Post(async () =>
                            {
                                await VideoOpenFile(videoFileName);
                            });
                        }
                    }
                }
            }

            matroska.Dispose();
        }
    }

    private bool LoadMatroskaSubtitle(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska)
    {
        if (matroskaSubtitleInfo.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
        {
            var pgsSubtitle = _bluRayHelper.LoadBluRaySubFromMatroska(matroskaSubtitleInfo, matroska, out var errorMessage);
            return true;
        }

        if (matroskaSubtitleInfo.CodecId.Equals("S_HDMV/TEXTST", StringComparison.OrdinalIgnoreCase))
        {
            return LoadTextSTFromMatroska(matroskaSubtitleInfo, matroska);
        }

        if (matroskaSubtitleInfo.CodecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase))
        {
            return LoadDvbFromMatroska(matroskaSubtitleInfo, matroska);
        }

        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, null);
        var subtitle = new Subtitle();
        var format = Utilities.LoadMatroskaTextSubtitle(matroskaSubtitleInfo, matroska, sub, subtitle);

        ResetSubtitle();
        subtitle.Renumber();
        Subtitles.Clear();
        Subtitles.AddRange(subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p)));


        //        Paragraphs = new ObservableCollection<DisplayParagraph>(subtitle.Paragraphs.Select(p => new DisplayParagraph(p)));
        //SelectedSubtitleFormat = format.Name;

        //        ShowStatus(_language.SubtitleImportedFromMatroskaFile);
        //_subtitle.Renumber();
        //if (matroska.Path.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) || matroska.Path.EndsWith(".mks", StringComparison.OrdinalIgnoreCase))
        //{
        //    _fileName = matroska.Path.Remove(matroska.Path.Length - 4) + format.Extension;
        //}

        //SetTitle();
        //_fileDateTime = new DateTime();
        //_converted = true;

        //if (batchMode)
        //{
        //    return true;
        //}

        //_subtitleListViewIndex = -1;
        //UpdateSourceView();
        //SubtitleListview1.Fill(_subtitle, _subtitleOriginal);
        //SubtitleListview1.SelectIndexAndEnsureVisible(0, true);
        //RefreshSelectedParagraph();

        return true;
    }

    private bool LoadDvbFromMatroska(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska)
    {
        ShowStatus(Se.Language.Main.ParsingMatroskaFile);
        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, MatroskaProgress);
        ResetSubtitle();

        _subtitle.Paragraphs.Clear();
        var subtitleImages = new List<DvbSubPes>();
        var subtitle = new Subtitle();
        Utilities.LoadMatroskaTextSubtitle(matroskaSubtitleInfo, matroska, sub, _subtitle);
        for (var index = 0; index < sub.Count; index++)
        {
            try
            {
                var msub = sub[index];
                DvbSubPes? pes = null;
                var data = msub.GetData(matroskaSubtitleInfo);
                if (data != null && data.Length > 9 && data[0] == 15 && data[1] >= SubtitleSegment.PageCompositionSegment && data[1] <= SubtitleSegment.DisplayDefinitionSegment) // sync byte + segment id
                {
                    var buffer = new byte[data.Length + 3];
                    Buffer.BlockCopy(data, 0, buffer, 2, data.Length);
                    buffer[0] = 32;
                    buffer[1] = 0;
                    buffer[buffer.Length - 1] = 255;
                    pes = new DvbSubPes(0, buffer);
                }
                else if (VobSubParser.IsMpeg2PackHeader(data))
                {
                    pes = new DvbSubPes(data, Mpeg2Header.Length);
                }
                else if (VobSubParser.IsPrivateStream1(data, 0))
                {
                    pes = new DvbSubPes(data, 0);
                }
                else if (data!.Length > 9 && data[0] == 32 && data[1] == 0 && data[2] == 14 && data[3] == 16)
                {
                    pes = new DvbSubPes(0, data);
                }

                if (pes == null && subtitle.Paragraphs.Count > 0)
                {
                    var last = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                    if (last.DurationTotalMilliseconds < 100)
                    {
                        last.EndTime.TotalMilliseconds = msub.Start;
                        if (last.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + 3000;
                        }
                    }
                }

                if (pes != null && pes.PageCompositions != null && pes.PageCompositions.Any(p => p.Regions.Count > 0))
                {
                    subtitleImages.Add(pes);
                    subtitle.Paragraphs.Add(new Paragraph(string.Empty, msub.Start, msub.End));
                }
            }
            catch
            {
                // continue
            }
        }

        if (subtitleImages.Count == 0)
        {
            return false;
        }

        for (var index = 0; index < subtitle.Paragraphs.Count; index++)
        {
            var p = subtitle.Paragraphs[index];
            if (p.DurationTotalMilliseconds < 200)
            {
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 3000;
            }

            var next = subtitle.GetParagraphOrDefault(index + 1);
            if (next != null && next.StartTime.TotalMilliseconds < p.EndTime.TotalMilliseconds)
            {
                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Se.Settings.General.MinimumMillisecondsBetweenLines;
            }
        }

        //using (var formSubOcr = new VobSubOcr())
        //{
        //    formSubOcr.Initialize(subtitle, subtitleImages, Configuration.Settings.VobSubOcr); // TODO: language???
        //    if (_loading)
        //    {
        //        formSubOcr.Icon = (Icon)Icon.Clone();
        //        formSubOcr.ShowInTaskbar = true;
        //        formSubOcr.ShowIcon = true;
        //    }

        //    if (formSubOcr.ShowDialog(this) == DialogResult.OK)
        //    {
        //        ResetSubtitle();
        //        _subtitle.Paragraphs.Clear();
        //        foreach (var p in formSubOcr.SubtitleFromOcr.Paragraphs)
        //        {
        //            _subtitle.Paragraphs.Add(p);
        //        }

        //        UpdateSourceView();
        //        SubtitleListview1.Fill(_subtitle, _subtitleOriginal);
        //        _subtitleListViewIndex = -1;
        //        SubtitleListview1.FirstVisibleIndex = -1;
        //        SubtitleListview1.SelectIndexAndEnsureVisible(0, true);
        //        RefreshSelectedParagraph();
        //        _fileName = Utilities.GetPathAndFileNameWithoutExtension(matroska.Path) + GetCurrentSubtitleFormat().Extension;
        //        _converted = true;
        //        SetTitle();

        //        Configuration.Settings.Save();
        //        return true;
        //    }
        //}

        return false;
    }

    private void MatroskaProgress(long position, long total)
    {
        // UpdateProgress(position, total, _language.ParsingMatroskaFile);
    }

    private bool LoadTextSTFromMatroska(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska)
    {
        ShowStatus(Se.Language.Main.ParsingMatroskaFile);
        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, MatroskaProgress);
        ResetSubtitle();

        _subtitle.Paragraphs.Clear();
        Utilities.LoadMatroskaTextSubtitle(matroskaSubtitleInfo, matroska, sub, _subtitle);
        Utilities.ParseMatroskaTextSt(matroskaSubtitleInfo, sub, _subtitle);

        SelectedSubtitleFormat = SubtitleFormats.FirstOrDefault(p => p.Name == Configuration.Settings.General.DefaultSubtitleFormat) ?? SelectedSubtitleFormat;
        ShowStatus(Se.Language.Main.SubtitleImportedFromMatroskaFile);
        _subtitle.Renumber();
        Subtitles.Clear();
        Subtitles.AddRange(_subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p)));


        if (matroska.Path.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) || matroska.Path.EndsWith(".mks", StringComparison.OrdinalIgnoreCase))
        {
            _subtitleFileName = matroska.Path.Remove(matroska.Path.Length - 4) + SelectedSubtitleFormat.Extension;
        }

        SelectAndScrollToRow(0);

        return true;
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
        return !IsEmpty && _changeSubtitleHash != GetFastHash();
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
        _changeSubtitleHash = GetFastHash();
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
            Window!,
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && string.IsNullOrEmpty(Se.Settings.General.LibMpvPath))
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var answer = await MessageBox.Show(
                    Window!,
                    "Download mpv?",
                    $"{Environment.NewLine}\"Subtitle Edit\" requires mpv to play video/audio.{Environment.NewLine}{Environment.NewLine}Download and use mpv?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return;
                }

                var result = await _windowService.ShowDialogAsync<DownloadLibMpvWindow, DownloadLibMpvViewModel>(Window!);

            }, DispatcherPriority.Background);
        }

        if (Se.Settings.File.ShowRecentFiles)
        {
            var first = Se.Settings.File.RecentFiles.FirstOrDefault();
            if (first != null && File.Exists(first.SubtitleFileName))
            {
                SubtitleOpen(first.SubtitleFileName, first.VideoFileName, first.SelectedLine).ConfigureAwait(false);
            }
        }

        Task.Run(async () =>
        {
            await Task.Delay(1000); // delay 1 second (off UI thread)
            _undoRedoManager.StartChangeDetection();
        });
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private async Task VideoOpenFile(string videoFileName)
    {
        if (VideoPlayerControl == null)
        {
            return;
        }

        await VideoPlayerControl.Open(videoFileName);

        if (IsValidUrl(videoFileName))
        {
            //TODO: create empty waveform
            return;
        }

        var peakWaveFileName = WavePeakGenerator.GetPeakWaveFileName(videoFileName);
        if (!File.Exists(peakWaveFileName))
        {
            if (FfmpegHelper.IsFfmpegInstalled())
            {
                var tempWaveFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
                var process = WaveFileExtractor.GetCommandLineProcess(videoFileName, -1, tempWaveFileName,
                    Configuration.Settings.General.VlcWaveTranscodeSettings, out _);
                ShowStatus(Se.Language.Main.ExtractingWaveInfo);
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
            ShowStatus(Se.Language.Main.LoadingWaveInfoFromCache);
            var wavePeaks = WavePeakData.FromDisk(peakWaveFileName);
            if (AudioVisualizer != null)
            {
                AudioVisualizer.WavePeaks = wavePeaks;
            }
        }

        _videoFileName = videoFileName;
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
            ShowStatus(Se.Language.Main.FailedToExtractWaveInfo);
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
                _updateAudioVisualizer = true;
            }, DispatcherPriority.Background);
        }
    }

    private void VideoCloseFile()
    {
        VideoPlayerControl?.Close();
        _videoFileName = string.Empty;
    }

    public bool IsTyping()
    {
        if (_lastKeyPressedTicks == 0)
        {
            return false;
        }

        var ticks = Stopwatch.GetTimestamp();
        var timeSpan = TimeSpan.FromTicks(ticks - _lastKeyPressedTicks);
        if (timeSpan.Milliseconds < 500)
        {
            return true;
        }

        return false;
    }

    public int GetFastHash()
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

        _undoRedoManager.StopChangeDetection();
        foreach (var item in selectedItems)
        {
            Subtitles.Remove(item);
        }
        Renumber();
        _undoRedoManager.StartChangeDetection();
    }

    private void InsertBeforeSelectedItem()
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

    private void InsertAfterSelectedItem()
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

    private void MergeLineBefore()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            var previous = Subtitles.GetOrNull(index - 1);

            if (previous == null)
            {
                return; // no next item to merge with
            }

            var list = new List<SubtitleLineViewModel>()
            {
                previous,
                selectedItem,
            };
            _mergeManager.MergeSelectedLines(Subtitles, list);
            Renumber();
            SelectAndScrollToRow(index - 1);
        }
    }

    private void MergeLineAfter()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            var next = Subtitles.GetOrNull(index + 1);

            if (next == null)
            {
                return; // no next item to merge with
            }

            var list = new List<SubtitleLineViewModel>()
            {
                selectedItem,
                next
            };
            _mergeManager.MergeSelectedLines(Subtitles, list);
            Renumber();
            SelectAndScrollToRow(index);
        }
    }

    private void MergeLinesSelected()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);

            _mergeManager.MergeSelectedLines(Subtitles,
                SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList());

            SelectAndScrollToRow(index - 1);
            Renumber();
        }
    }

    private void MergeLinesSelectedAsDialog()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count != 2)
        {
            return; // only two items can be merged as dialog
        }

        var index = Subtitles.IndexOf(selectedItems[0]);
        _mergeManager.MergeSelectedLinesAsDialog(Subtitles, selectedItems);
        SelectAndScrollToRow(index);
        Renumber();

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
        MenuItemMergeAsDialog.IsVisible = SubtitleGrid.SelectedItems.Count == 2;
    }

    private bool IsTextInputFocused()
    {
        var focusedElement = Window?.FocusManager?.GetFocusedElement();

        return focusedElement is TextBox ||
               focusedElement is MaskedTextBox ||
               focusedElement is AutoCompleteBox ||
               (focusedElement is Control control && IsTextInputControl(control));
    }

    private static bool IsTextInputControl(Control control)
    {
        var typeName = control.GetType().Name;
        return typeName.Contains("TextBox") ||
               typeName.Contains("TextEditor") ||
               typeName.Contains("TextInput");
    }

    public void KeyDown(KeyEventArgs keyEventArgs)
    {
        var ticks = Stopwatch.GetTimestamp();
        var timeSpan = TimeSpan.FromTicks(ticks - _lastKeyPressedTicks);
        if (timeSpan.Seconds > 5)
        {
            _shortcutManager.ClearKeys(); // reset shortcuts if no key pressed for 2 seconds
        }
        _lastKeyPressedTicks = ticks;

        _shortcutManager.OnKeyPressed(this, keyEventArgs);

        if (IsTextInputFocused())
        {
            var currentKeys = _shortcutManager.GetActiveKeys();
            if (currentKeys.Count == 1)
            {
                var key = currentKeys.First();
                var allowedSingleKeyShortcuts = new HashSet<Key>
                {
                    Key.Enter,
                    Key.Escape,
                    Key.Tab,
                    Key.Back,
                    Key.Delete,
                    Key.Left,
                    Key.Right,
                    Key.Up,
                    Key.Down,
                    Key.PageUp,
                    Key.PageDown,
                    Key.Home,
                    Key.End,
                    Key.Insert,
                    Key.F1,
                    Key.F2,
                    Key.F3,
                    Key.F4,
                    Key.F5,
                    Key.F6,
                    Key.F7,
                    Key.F8,
                    Key.F9,
                    Key.F10,
                    Key.F11,
                    Key.F12,
                };

                if (!allowedSingleKeyShortcuts.Contains(key))
                {
                    return;
                }
            }
        }

        if (SubtitleGrid.IsFocused)
        {
            var relayCommand = _shortcutManager.CheckShortcuts(ShortcutCategory.SubtitleGrid.ToStringInvariant());
            if (relayCommand != null)
            {
                keyEventArgs.Handled = true;
                relayCommand.Execute(null);
                return;
            }
        }

        var rc = _shortcutManager.CheckShortcuts(ShortcutCategory.General.ToStringInvariant());
        if (rc != null)
        {
            keyEventArgs.Handled = true;
            rc.Execute(null);
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
            EditTextCharactersPerSecondBackground = Brushes.Transparent;
            EditTextTotalLength = string.Empty;
            EditTextTotalLengthBackground = Brushes.Transparent;
            EditTextLineLengths = string.Empty;
            return;
        }

        _selectedSubtitles = selectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count > 1)
        {
            StatusTextRight = $"{selectedItems.Count} lines selected of {Subtitles.Count}";
            EditTextCharactersPerSecond = string.Empty;
            EditTextCharactersPerSecondBackground = Brushes.Transparent;
            EditTextTotalLengthBackground = Brushes.Transparent;
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

        _updateAudioVisualizer = true;
        Dispatcher.UIThread.Post(() =>
        {
            MakeSubtitleTextInfo(item.Text, item);
        });
    }

    private void MakeSubtitleTextInfo(string text, SubtitleLineViewModel item)
    {
        if (text == null)
        {
            text = string.Empty;
        }

        text = HtmlUtil.RemoveHtmlTags(text, true);
        var totalLength = text.CountCharacters(false);
        var cps = new Paragraph(text, item.StartTime.TotalMilliseconds, item.EndTime.TotalMilliseconds)
            .GetCharactersPerSecond();

        var lines = text.SplitToLines();
        var lineLenghts = new List<string>(lines);
        PanelSingleLineLenghts.Children.Clear();
        PanelSingleLineLenghts.Children.Add(UiUtil.MakeTextBlock(Se.Language.Main.SingleLineLength).WithFontSize(12).WithPadding(2));
        var first = true;
        for (var i = 0; i < lines.Count; i++)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                PanelSingleLineLenghts.Children.Add(UiUtil.MakeTextBlock("/").WithFontSize(12).WithPadding(2));
            }

            var tb = UiUtil.MakeTextBlock(lineLenghts[i].Length.ToStringInvariant()).WithFontSize(12).WithPadding(2);
            if (Se.Settings.General.ColorTextTooLong && lineLenghts[i].Length > Se.Settings.General.SubtitleLineMaximumLength)
            {
                tb.Background = new SolidColorBrush(_errorColor);
            }

            PanelSingleLineLenghts.Children.Add(tb);
        }

        EditTextCharactersPerSecond = string.Format(Se.Language.Main.CharactersPerSecond, $"{cps:0.0}");
        EditTextTotalLength = string.Format(Se.Language.Main.TotalCharacters, totalLength);

        EditTextCharactersPerSecondBackground = Se.Settings.General.ColorTextTooLong && cps > Se.Settings.General.SubtitleMaximumCharactersPerSeconds
                ? new SolidColorBrush(_errorColor)
                : new SolidColorBrush(Colors.Transparent);

        EditTextTotalLengthBackground = Se.Settings.General.ColorTextTooLong && totalLength > Se.Settings.General.SubtitleLineMaximumLength * lines.Count
            ? new SolidColorBrush(_errorColor)
                : new SolidColorBrush(Colors.Transparent);
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

            text = text + " - " + Se.Language.Title;
            if (_changeSubtitleHash != GetFastHash())
            {
                text = "*" + text;
            }

            if (Window != null)
            {
                Window.Title = text;
            }

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

            var mpv = VideoPlayerControl?.VideoPlayerInstance as VideoPlayerInstanceMpv;
            if (mpv != null)
            {
                _mpvReloader.RefreshMpv(mpv.MpvContext!, GetUpdateSubtitle(), SelectedSubtitleFormat);
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

    public void AudioVisualizerOnToggleSelection(object sender, ParagraphEventArgs e)
    {
        if (!string.IsNullOrEmpty(_videoFileName) && VideoPlayerControl != null)
        {
            var p = Subtitles.FirstOrDefault(p =>
                Math.Abs(p.StartTime.TotalMilliseconds - e.Paragraph.StartTime.TotalMilliseconds) < 0.01);
            if (p != null)
            {
                if (SubtitleGrid.SelectedItems.Contains(p))
                {
                    SubtitleGrid.SelectedItems.Remove(p);
                }
                else
                {
                    SubtitleGrid.SelectedItems.Add(p);
                }

                _updateAudioVisualizer = true;
            }
        }
    }

    public void Adjust(TimeSpan adjustment, bool adjustAll, bool adjustSelectedLines, bool adjustSelectedLinesAndForward)
    {
        if (Math.Abs(adjustment.TotalMilliseconds) < 0.01)
        {
            return;
        }

        if (adjustSelectedLines)
        {
            foreach (SubtitleLineViewModel p in SubtitleGrid.SelectedItems)
            {
                p.StartTime += adjustment;
                p.UpdateDuration();
            }
        }
        else if (adjustSelectedLinesAndForward)
        {
            var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
            if (selectedItems.Count > 0)
            {
                var first = selectedItems.OrderBy(p => Subtitles.IndexOf(p)).First();
                var firstSelectedIndex = Subtitles.IndexOf(first);
                for (var i = firstSelectedIndex; i < Subtitles.Count; i++)
                {
                    var p = Subtitles[i];
                    p.StartTime += adjustment;
                    p.UpdateDuration();
                }
            }
        }
        else if (adjustAll)
        {
            foreach (var p in Subtitles)
            {
                p.StartTime += adjustment;
                p.UpdateDuration();
            }
        }

        _updateAudioVisualizer = true;
    }

    internal void DurationChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        _updateAudioVisualizer = true;

        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            MakeSubtitleTextInfo(selectedSubtitle.Text, selectedSubtitle);
        });
    }

    internal void StartTimeChanged(object? sender, TimeSpan e)
    {
        _updateAudioVisualizer = true;

        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            MakeSubtitleTextInfo(selectedSubtitle.Text, selectedSubtitle);
        });
    }

    public void GoToAndFocusLine(SubtitleLineViewModel p)
    {
        SelectAndScrollToSubtitle(p);
    }
}