using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.Validators;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Features.Edit.ShowHistory;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Files.Export.ExportEbuStl;
using Nikse.SubtitleEdit.Features.Files.ExportCavena890;
using Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;
using Nikse.SubtitleEdit.Features.Files.ExportEbuStl;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Files.ExportPac;
using Nikse.SubtitleEdit.Features.Files.ManualChosenEncoding;
using Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;
using Nikse.SubtitleEdit.Features.Files.Statistics;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Options.Language;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.Options.WordLists;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.Bookmarks;
using Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;
using Nikse.SubtitleEdit.Features.Shared.PickAlignment;
using Nikse.SubtitleEdit.Features.Shared.PickColor;
using Nikse.SubtitleEdit.Features.Shared.PickFontName;
using Nikse.SubtitleEdit.Features.Shared.PickLayers;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Features.Shared.PickMp4Track;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Shared.SourceView;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.BridgeGaps;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Tools.JoinSubtitles;
using Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameText;
using Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameTimeCodes;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
using Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;
using Nikse.SubtitleEdit.Features.Video.BlankVideo;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Features.Video.CutVideo;
using Nikse.SubtitleEdit.Features.Video.GoToVideoPosition;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Features.Video.ReEncodeVideo;
using Nikse.SubtitleEdit.Features.Video.ShotChanges;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel :
    ObservableObject,
    IAdjustCallback,
    IFocusSubtitleLine,
    IUndoRedoClient,
    IFindResult
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    private List<SubtitleLineViewModel>? _selectedSubtitles;
    [ObservableProperty] private int? _selectedSubtitleIndex;

    [ObservableProperty] private string _editText;
    [ObservableProperty] private string _editTextCharactersPerSecond;
    [ObservableProperty] private IBrush _editTextCharactersPerSecondBackground;
    [ObservableProperty] private string _editTextTotalLength;
    [ObservableProperty] private IBrush _editTextTotalLengthBackground;
    [ObservableProperty] private string _editTextLineLengths;

    [ObservableProperty] private string _editTextOriginal;
    [ObservableProperty] private string _editTextCharactersPerSecondOriginal;
    [ObservableProperty] private IBrush _editTextCharactersPerSecondBackgroundOriginal;
    [ObservableProperty] private string _editTextTotalLengthOriginal;
    [ObservableProperty] private IBrush _editTextTotalLengthBackgroundOriginal;
    [ObservableProperty] private string _editTextLineLengthsOriginal;

    [ObservableProperty] private ObservableCollection<SubtitleFormat> _subtitleFormats;
    [ObservableProperty] private SubtitleFormat _selectedSubtitleFormat;

    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    [ObservableProperty] private TextEncoding _selectedEncoding;

    [ObservableProperty] private string _statusTextLeft;
    [ObservableProperty] private string _statusTextRight;

    [ObservableProperty] private bool _isWaveformToolbarVisible;
    [ObservableProperty] private bool _isSubtitleGridFlyoutHeaderVisible;
    [ObservableProperty] private bool _showColumnOriginalText;
    [ObservableProperty] private bool _showColumnEndTime;
    [ObservableProperty] private bool _showColumnGap;
    [ObservableProperty] private bool _showColumnDuration;
    [ObservableProperty] private bool _showColumnActor;
    [ObservableProperty] private bool _showColumnCps;
    [ObservableProperty] private bool _showColumnWpm;
    [ObservableProperty] private bool _showColumnLayer;
    [ObservableProperty] private bool _showUpDownStartTime;
    [ObservableProperty] private bool _showUpDownEndTime;
    [ObservableProperty] private bool _showUpDownDuration;
    [ObservableProperty] private bool _showUpDownLabels;
    [ObservableProperty] private bool _isColumnLayerVisible;
    [ObservableProperty] private bool _lockTimeCodes;
    [ObservableProperty] private bool _areVideoControlsUndocked;
    [ObservableProperty] private bool _isFormatAssa;
    [ObservableProperty] private bool _hasFormatStyle;
    [ObservableProperty] private bool _areAssaContentMenuItemsVisible;
    [ObservableProperty] private bool _selectCurrentSubtitleWhilePlaying;
    [ObservableProperty] private bool _waveformCenter;
    [ObservableProperty] private bool _isRightToLeftEnabled;
    [ObservableProperty] private bool _showAutoTranslateSelectedLines;
    [ObservableProperty] private bool _showShotChangesListMenuItem;
    [ObservableProperty] private bool _showLayer;
    [ObservableProperty] private bool _showLayerFilterIcon;
    [ObservableProperty] private bool _showColumnLayerFlyoutMenuItem;
    [ObservableProperty] private bool _isVideoLoaded;


    public DataGrid SubtitleGrid { get; set; }
    public TextBox EditTextBox { get; set; }
    public Window? Window { get; set; }
    public Grid ContentGrid { get; set; }
    public MainView? MainView { get; set; }
    public TextBlock StatusTextLeftLabel { get; set; }
    public MenuItem MenuReopen { get; set; }
    public AudioVisualizer? AudioVisualizer { get; set; }

    VideoPlayerUndockedViewModel? _videoPlayerUndockedViewModel;
    AudioVisualizerUndockedViewModel? _audioVisualizerUndockedViewModel;
    FindViewModel? _findViewModel;
    ReplaceViewModel? _replaceViewModel;

    private static Color _errorColor = Se.Settings.General.ErrorColor.FromHexToColor();

    private bool _updateAudioVisualizer;
    private string? _subtitleFileName;
    private string? _subtitleFileNameOriginal;
    private bool _converted;
    private Subtitle _subtitle;
    private Subtitle _subtitleOriginal;
    private SubtitleFormat? _lastOpenSaveFormat;
    private string? _videoFileName;
    private CancellationTokenSource? _statusFadeCts;
    private int _changeSubtitleHash = -1;
    private int _changeSubtitleHashOriginal = -1;
    private bool _subtitleGridSelectionChangedSkip;
    private long _lastKeyPressedTicks;
    private bool _loading;
    private bool _opening;
    private List<int>? _visibleLayers;
    private DispatcherTimer _positionTimer = new();
    private DispatcherTimer _slowTimer = new();
    private CancellationTokenSource _videoOpenTokenSource;

    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IShortcutManager _shortcutManager;
    private readonly IWindowService _windowService;
    private readonly IInsertService _insertService;
    private readonly IMergeManager _mergeManager;
    private readonly ISplitManager _splitManager;
    private readonly IAutoBackupService _autoBackupService;
    private readonly IUndoRedoManager _undoRedoManager;
    private readonly IBluRayHelper _bluRayHelper;
    private readonly IMpvReloader _mpvReloader;
    private readonly IFindService _findService;
    private readonly IColorService _colorService;
    private readonly IFontNameService _fontNameService;

    private bool IsEmpty => Subtitles.Count == 0 || (Subtitles.Count == 1 && string.IsNullOrEmpty(Subtitles[0].Text));

    private bool IsEmptyOriginal => Subtitles.Count == 0 ||
                                    (Subtitles.Count == 1 && string.IsNullOrEmpty(Subtitles[0].OriginalText));

    public VideoPlayerControl? VideoPlayerControl { get; internal set; }
    public Menu Menu { get; internal set; }
    public Border Toolbar { get; internal set; }
    public StackPanel PanelSingleLineLenghts { get; internal set; }
    public MenuItem MenuItemMergeAsDialog { get; internal set; }
    public MenuItem MenuItemMerge { get; internal set; }
    public MenuItem MenuItemAudioVisualizerInsertNewSelection { get; set; }
    public MenuItem MenuItemAudioVisualizerDelete { get; set; }
    public MenuItem MenuItemAudioVisualizerInsertBefore { get; set; }
    public MenuItem MenuItemAudioVisualizerInsertAfter { get; set; }
    public MenuItem MenuItemAudioVisualizerSplit { get; set; }
    public Separator MenuItemAudioVisualizerSeparator1 { get; set; }
    public MenuItem MenuItemAudioVisualizerInsertAtPosition { get; set; }
    public MenuItem MenuItemAudioVisualizerDeleteAtPosition { get; set; }
    public MenuItem MenuItemAudioVisualizerFilterByLayer { get; set; }
    public TextBox EditTextBoxOriginal { get; set; }
    public StackPanel PanelSingleLineLenghtsOriginal { get; set; }
    public MenuItem MenuItemStyles { get; set; }
    public MenuItem MenuItemActors { get; set; }
    public Button ButtonWaveformPlay { get; set; }

    public MainViewModel(
        IFileHelper fileHelper,
        IFolderHelper folderHelper,
        IShortcutManager shortcutManager,
        IWindowService windowService,
        IInsertService insertService,
        IMergeManager mergeManager,
        ISplitManager splitManager,
        IAutoBackupService autoBackupService,
        IUndoRedoManager undoRedoManager,
        IBluRayHelper bluRayHelper,
        IMpvReloader mpvReloader,
        IFindService findService,
        IDictionaryInitializer dictionaryInitializer,
        ILanguageInitializer languageInitializer,
        IOcrInitializer ocrInitializer,
        IThemeInitializer themeInitializer,
        IColorService colorService,
        IFontNameService fontNameService)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _shortcutManager = shortcutManager;
        _windowService = windowService;
        _insertService = insertService;
        _mergeManager = mergeManager;
        _splitManager = splitManager;
        _autoBackupService = autoBackupService;
        _undoRedoManager = undoRedoManager;
        _bluRayHelper = bluRayHelper;
        _mpvReloader = mpvReloader;
        _findService = findService;
        _colorService = colorService;
        _fontNameService = fontNameService;

        _loading = true;
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
        MenuItemMerge = new MenuItem();
        MenuItemAudioVisualizerInsertNewSelection = new MenuItem();
        MenuItemAudioVisualizerDelete = new MenuItem();
        MenuItemAudioVisualizerInsertBefore = new MenuItem();
        MenuItemAudioVisualizerInsertAfter = new MenuItem();
        MenuItemAudioVisualizerSplit = new MenuItem();
        MenuItemAudioVisualizerSeparator1 = new Separator();
        MenuItemAudioVisualizerInsertAtPosition = new MenuItem();
        MenuItemAudioVisualizerDeleteAtPosition = new MenuItem();
        MenuItemAudioVisualizerFilterByLayer = new MenuItem();
        MenuItemStyles = new MenuItem();
        MenuItemActors = new MenuItem();
        Toolbar = new Border();
        ButtonWaveformPlay = new Button();
        _subtitle = new Subtitle();
        _subtitleOriginal = new Subtitle();
        _videoFileName = string.Empty;
        _subtitleFileName = string.Empty;
        Subtitles = [];

        SubtitleFormats = [.. SubtitleFormat.AllSubtitleFormats];
        var defaultFormat =
            SubtitleFormats.Where(f => f.FriendlyName == Se.Settings.General.DefaultSubtitleFormat).FirstOrDefault() ??
            SubtitleFormats[0];
        SubtitleFormats.Remove(defaultFormat);
        SubtitleFormats.Insert(0, defaultFormat);

        SelectedSubtitleFormat = SubtitleFormats[0];
        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        SelectedEncoding = Encodings.FirstOrDefault(p => p.DisplayName == Se.Settings.General.DefaultEncoding) ??
                           Encodings[0];
        StatusTextLeft = string.Empty;
        StatusTextRight = string.Empty;
        ShowColumnEndTime = Se.Settings.General.ShowColumnEndTime;
        ShowColumnDuration = Se.Settings.General.ShowColumnDuration;
        ShowColumnGap = Se.Settings.General.ShowColumnGap;
        ShowColumnActor = Se.Settings.General.ShowColumnActor;
        ShowColumnCps = Se.Settings.General.ShowColumnCps;
        ShowColumnWpm = Se.Settings.General.ShowColumnWpm;
        ShowColumnLayer = Se.Settings.General.ShowColumnLayer;
        ShowUpDownStartTime = Se.Settings.Appearance.ShowUpDownStartTime;
        ShowUpDownEndTime = Se.Settings.Appearance.ShowUpDownEndTime;
        ShowUpDownDuration = Se.Settings.Appearance.ShowUpDownDuration;
        ShowUpDownLabels = Se.Settings.Appearance.ShowUpDownLabels;
        SelectCurrentSubtitleWhilePlaying = Se.Settings.General.SelectCurrentSubtitleWhilePlaying;
        WaveformCenter = Se.Settings.Waveform.CenterVideoPosition;
        EditTextBoxOriginal = new TextBox();
        EditTextCharactersPerSecondOriginal = string.Empty;
        EditTextCharactersPerSecondBackgroundOriginal = Brushes.Transparent;
        EditTextTotalLengthOriginal = string.Empty;
        EditTextTotalLengthBackgroundOriginal = Brushes.Transparent;
        EditTextLineLengthsOriginal = string.Empty;
        EditTextOriginal = string.Empty;
        PanelSingleLineLenghtsOriginal = new StackPanel();
        IsWaveformToolbarVisible = Se.Settings.Waveform.ShowToolbar;
        _videoOpenTokenSource = new CancellationTokenSource();

        Configuration.DataDirectoryOverride = Se.DataFolder;

        themeInitializer.UpdateThemesIfNeeded().ConfigureAwait(true);
        Dispatcher.UIThread.Post(async void () =>
        {
            try
            {
                await languageInitializer.UpdateLanguagesIfNeeded();
                await dictionaryInitializer.UpdateDictionariesIfNeeded();
                await ocrInitializer.UpdateOcrDictionariesIfNeeded();
            }
            catch (Exception e)
            {
                Se.LogError(e);
            }
        }, DispatcherPriority.Loaded);
        InitializeLibMpv();
        InitializeFfmpeg();
        LoadShortcuts();

        StartTimers();
        _autoBackupService.StartAutoBackup(this);
        _undoRedoManager.SetupChangeDetection(this, TimeSpan.FromSeconds(1));
        LockTimeCodes = Se.Settings.General.LockTimeCodes;
    }

    private static void InitializeFfmpeg()
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
    private void CommandExit()
    {
        if (Window == null)
        {
            return;
        }

        Window.Close();
    }

    [RelayCommand]
    private async Task CommandShowLayout()
    {
        var vm = await _windowService.ShowDialogAsync<LayoutWindow, LayoutViewModel>(Window!,
            viewModel => { viewModel.SelectedLayout = Se.Settings.General.LayoutNumber; });

        if (vm.OkPressed && vm.SelectedLayout != null && vm.SelectedLayout != Se.Settings.General.LayoutNumber)
        {
            if (AreVideoControlsUndocked)
            {
                VideoRedockControls();
            }

            Se.Settings.General.LayoutNumber = InitLayout.MakeLayout(MainView!, this, vm.SelectedLayout.Value);

            if (OperatingSystem.IsMacOS() && !string.IsNullOrEmpty(_videoFileName) && VideoPlayerControl != null)
            {
                VideoPlayerControl.Reload();
            }
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
    }

    [RelayCommand]
    private void TogglePlayPause2()
    {
        TogglePlayPause();
    }

    [RelayCommand]
    private void ToggleLockTimeCodes()
    {
        LockTimeCodes = !LockTimeCodes;
        Se.Settings.General.LockTimeCodes = LockTimeCodes;
        if (AudioVisualizer != null)
        {
            AudioVisualizer.IsReadOnly = LockTimeCodes;
        }
    }

    [RelayCommand]
    private async Task ShowHelp()
    {
        await Window!.Launcher.LaunchUriAsync(new Uri("https://www.nikse.dk/subtitleedit/help"));
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSourceView()
    {
        var result = await _windowService.ShowDialogAsync<SourceViewWindow, SourceViewViewModel>(Window!, vm =>
        {
            var text = GetUpdateSubtitle().ToText(SelectedSubtitleFormat);
            vm.Initialize(
                "Source view - " + (string.IsNullOrEmpty(_subtitleFileName)
                    ? "untitled"
                    : Path.GetFileName(_subtitleFileName)), text);
        });

        if (result.OkPressed)
        {
            //_subtitle.Header = result.Header;
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowAssaStyles()
    {
        var result = await _windowService.ShowDialogAsync<AssaStylesWindow, AssaStylesViewModel>(Window!,
            vm =>
            {
                vm.Initialize(_subtitle, SelectedSubtitleFormat, _subtitleFileName ?? string.Empty,
                    SelectedSubtitle?.Style ?? string.Empty);
            });

        if (result.OkPressed)
        {
            _subtitle.Header = result.Header;
            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
            var first = styles.FirstOrDefault() ?? "Default";

            foreach (var s in Subtitles)
            {
                if (string.IsNullOrEmpty(s.Style) || !styles.Contains(s.Style))
                {
                    s.Style = first;
                }
            }
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowAssaProperties()
    {
        var result = await _windowService.ShowDialogAsync<AssaPropertiesWindow, AssaPropertiesViewModel>(Window!,
            vm =>
            {
                vm.Initialize(_subtitle, SelectedSubtitleFormat, _subtitleFileName ?? string.Empty,
                    _videoFileName ?? string.Empty);
            });

        if (result.OkPressed)
        {
            _subtitle.Header = result.Header;
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowAssaAttachments()
    {
        var result = await _windowService.ShowDialogAsync<AssaAttachmentsWindow, AssaAttachmentsViewModel>(Window!,
            vm => { vm.Initialize(_subtitle, SelectedSubtitleFormat, _subtitleFileName ?? string.Empty); });

        if (result.OkPressed)
        {
            _subtitle.Header = result.Header;
            _subtitle.Footer = result.Footer;
        }

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
        var newWindow = new AboutWindow(new AboutViewModel());
        await newWindow.ShowDialog(Window!);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileNew()
    {
        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        ResetSubtitle();
        VideoCloseFile();
        AddToRecentFiles(false);
    }

    [RelayCommand]
    private async Task CommandFileNewKeepVideo()
    {
        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        ResetSubtitle();
        AddToRecentFiles(false);
    }

    private void ResetSubtitle(SubtitleFormat? format = null)
    {
        _videoOpenTokenSource?.Cancel();
        ShowColumnOriginalText = false;
        _subtitle.Paragraphs.Clear();
        Subtitles.Clear();

        if (format != null)
        {
            SelectedSubtitleFormat =
                SubtitleFormats.FirstOrDefault(f => f.FriendlyName == format.FriendlyName) ??
                SubtitleFormats[0];
        }
        else
        {
            SelectedSubtitleFormat =
                SubtitleFormats.FirstOrDefault(f => f.FriendlyName == Se.Settings.General.DefaultSubtitleFormat) ??
                SubtitleFormats[0];
        }

        SelectedEncoding = Encodings.FirstOrDefault(p => p.DisplayName == Se.Settings.General.DefaultEncoding) ??
                           Encodings[0];
        _subtitleFileName = string.Empty;
        _subtitle = new Subtitle();
        _changeSubtitleHash = GetFastHash();

        _subtitleFileNameOriginal = string.Empty;
        _subtitleOriginal = new Subtitle();
        _changeSubtitleHashOriginal = GetFastHashOriginal();

        _visibleLayers = null;
        ShowLayerFilterIcon = false;

        AutoFitColumns();

        _mpvReloader.Reset();

        if (_findViewModel != null)
        {
            _findViewModel.Window?.Close();
            _findViewModel = null;
        }

        if (_replaceViewModel != null)
        {
            _replaceViewModel.Window?.Close();
            _replaceViewModel = null;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task FileOpenOriginal()
    {
        var selectedIndex = SelectedSubtitleIndex ?? 0;

        var fileName =
            await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenOriginalSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            _shortcutManager.ClearKeys();
            return;
        }

        bool flowControl = await SubtitleOpenOriginal(selectedIndex, fileName);
        if (!flowControl)
        {
            return;
        }

        _shortcutManager.ClearKeys();
    }

    private async Task<bool> SubtitleOpenOriginal(int selectedIndex, string fileName)
    {
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
                var message = Se.Language.General.UnknownSubtitleFormat;
                await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _shortcutManager.ClearKeys();
                return false;
            }
        }

        if (subtitle.Paragraphs.Count == Subtitles.Count)
        {
            _subtitleOriginal = subtitle;
            _subtitleFileNameOriginal = fileName;
            for (var i = 0; i < Subtitles.Count; i++)
            {
                Subtitles[i].OriginalText = subtitle.Paragraphs[i].Text;
            }

            _changeSubtitleHashOriginal = GetFastHashOriginal();
            ShowColumnOriginalText = true;

            AutoFitColumns();
            SelectAndScrollToRow(selectedIndex);
            AddToRecentFiles(true);
        }
        else
        {
            var message = "The original subtitle does not have the same number of lines as the current subtitle." +
                          Environment.NewLine
                          + "Original lines: " + subtitle.Paragraphs.Count + Environment.NewLine
                          + "Current lines: " + Subtitles.Count;
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        return true;
    }

    [RelayCommand]
    private void FileCloseOriginal()
    {
        ShowColumnOriginalText = false;
        AutoFitColumns();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileOpen()
    {
        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle);
        if (!string.IsNullOrEmpty(fileName))
        {
            VideoCloseFile();
            await SubtitleOpen(fileName);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileOpenKeepVideo()
    {
        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle);
        if (!string.IsNullOrEmpty(fileName))
        {
            await SubtitleOpen(fileName);
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private void CommandFileReopen(RecentFile recentFile)
    {
        Dispatcher.UIThread.Post(async void () =>
        {
            var doContinue = await HasChangesContinue();
            if (!doContinue)
            {
                return;
            }

            VideoCloseFile();
            await SubtitleOpen(recentFile.SubtitleFileName, recentFile.VideoFileName, recentFile.SelectedLine);

            if (!string.IsNullOrEmpty(recentFile.SubtitleFileNameOriginal) &&
                File.Exists(recentFile.SubtitleFileNameOriginal))
            {
                var selectedIndex = recentFile.SelectedLine;
                await SubtitleOpenOriginal(selectedIndex, recentFile.SubtitleFileNameOriginal);
            }

            _shortcutManager.ClearKeys();
        });
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

        if (ShowColumnOriginalText)
        {
            await SaveSubtitleOriginal();
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileSaveAs()
    {
        await SaveSubtitleAs();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task OpenContainingFolder()
    {
        if (string.IsNullOrEmpty(_subtitleFileName))
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window!, _subtitleFileName);

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowCompare()
    {
        var result = await _windowService.ShowDialogAsync<CompareWindow, CompareViewModel>(Window!, vm =>
        {
            var right = new ObservableCollection<SubtitleLineViewModel>();
            vm.Initialize(Subtitles, _subtitleFileName ?? string.Empty, right, string.Empty, HasChanges());
        });

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowStatistics()
    {
        var result = await _windowService.ShowDialogAsync<StatisticsWindow, StatisticsViewModel>(Window!,
            vm => { vm.Initialize(GetUpdateSubtitle(), SelectedSubtitleFormat, _subtitleFileName ?? string.Empty); });

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportBluRaySup()
    {
        IExportHandler exportHandler = new ExportHandlerBluRaySup();
        var result = await _windowService.ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(Window!,
            vm => { vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName); });

        if (!result.OkPressed)
        {
            return;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportVobSub()
    {
        IExportHandler exportHandler = new ExportHandlerVobSub();
        var result = await _windowService.ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(Window!,
            vm => { vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName); });

        if (!result.OkPressed)
        {
            return;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowExportCustomTextFormat()
    {
        var result = await _windowService.ShowDialogAsync<ExportCustomTextFormatWindow, ExportCustomTextFormatViewModel>(Window!,
            vm => { vm.Initialize(Subtitles.ToList(), _subtitleFileName, _videoFileName); });

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
            GetNewFileName(),
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
        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, cavena, GetNewFileName(),
            string.Format(Se.Language.Main.SaveXFileAs, cavena.Name));
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        using (var ms = new MemoryStream())
        {
            cavena.Save(fileName, ms, GetUpdateSubtitle(), false);
            ms.Position = 0;
            File.WriteAllBytes(fileName, ms.ToArray());
        }

        ShowStatus($"File exported in format {cavena.Name} to {fileName}");

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportPac()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ExportPacWindow, ExportPacViewModel>(Window);
        if (!result.OkPressed || result.PacCodePage == null)
        {
            return;
        }

        var pac = new Pac { CodePage = result.PacCodePage!.Value };

        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, pac, GetNewFileName(),
            string.Format(Se.Language.Main.SaveXFileAs, pac.Name));
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        using var ms = new MemoryStream();
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
            GetNewFileName(),
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
        var result =
            await _windowService.ShowDialogAsync<ExportEbuStlWindow, ExportEbuStlViewModel>(Window!,
                vm => { vm.Initialize(GetUpdateSubtitle()); });

        if (!result.OkPressed)
        {
            return;
        }

        Ebu.EbuUiHelper ??= new UiEbuSaveHelper();

        var format = new Ebu();

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            format,
            GetNewFileName(),
            $"Save {format.Name} file as");

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        format.Save(fileName, result.Subtitle);
        ShowStatus($"File exported in format \"{format.Name}\" to file \"{fileName}\"");

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ImportTimeCodes()
    {
        if (Window == null)
        {
            return;
        }

        if (Subtitles.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.NoVideoLoaded,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _shortcutManager.ClearKeys();
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            _shortcutManager.ClearKeys();
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.UnknownSubtitleFormat,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _shortcutManager.ClearKeys();
            return;
        }

        if (subtitle.Paragraphs.Count != Subtitles.Count)
        {
            var message = "The time code import subtitle does not have the same number of lines as the current subtitle." + Environment.NewLine
                          + "Imported lines: " + subtitle.Paragraphs.Count + Environment.NewLine
                          + "Current lines: " + Subtitles.Count + Environment.NewLine
                          + Environment.NewLine +
                          "Do you want to continue anyway?";

            var answer = await MessageBox.Show(Window, Se.Language.General.Import, message, MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Error);

            if (answer != MessageBoxResult.Yes)
            {
                _shortcutManager.ClearKeys();
                return;
            }
        }

        for (var i = 0; i < Subtitles.Count && i < subtitle.Paragraphs.Count; i++)
        {
            Subtitles[i].SetStartTimeOnly(subtitle.Paragraphs[i].StartTime.TimeSpan);
            Subtitles[i].EndTime = subtitle.Paragraphs[i].EndTime.TimeSpan;
        }
    }

    [RelayCommand]
    private async Task ShowImportSubtitleWithManuallyChosenEncoding()
    {
        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        var fileName =
            await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var result =
            await _windowService.ShowDialogAsync<ManualChosenEncodingWindow, ManualChosenEncodingViewModel>(Window!,
                vm => { vm.Initialize(fileName); });

        if (result.OkPressed && result.SelectedEncoding != null)
        {
            await SubtitleOpen(fileName, null, null, result.SelectedEncoding);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowPickLayerFilter()
    {
        if (Window == null || AudioVisualizer?.WavePeaks == null)
        {
            return;
        }

        var result =
            await _windowService.ShowDialogAsync<PickLayersWindow, PickLayersViewModel>(Window!, vm => { vm.Initialize(Subtitles.ToList(), _visibleLayers); });

        if (!result.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        _visibleLayers = result.SelectedLayers;
        ShowLayerFilterIcon = IsFormatAssa && Se.Settings.Appearance.ShowLayer && _visibleLayers != null;
        AudioVisualizer.LayersFilter = _visibleLayers;
        _updateAudioVisualizer = true;

        _shortcutManager.ClearKeys();
    }

    private string GetNewFileName()
    {
        if (!string.IsNullOrEmpty(_subtitleFileName))
        {
            return Path.GetFileNameWithoutExtension(_subtitleFileName);
        }

        if (!string.IsNullOrEmpty(_videoFileName))
        {
            return Path.GetFileNameWithoutExtension(_videoFileName);
        }

        return string.Empty;
    }

    [RelayCommand]
    private async Task VideoGenerateBlank()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BlankVideoWindow, BlankVideoViewModel>(Window!,
            vm => { vm.Initialize(_subtitleFileName ?? string.Empty, SelectedSubtitleFormat); });

        if (!result.OkPressed)
        {
            return;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task VideoCut()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            await CommandVideoOpen();
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<CutVideoWindow, CutVideoViewModel>(Window!,
            vm => { vm.Initialize(_videoFileName, AudioVisualizer?.WavePeaks, SelectedSubtitleFormat); });

        if (!result.OkPressed)
        {
            return;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task VideoReEncode()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            await CommandVideoOpen();
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ReEncodeVideoWindow, ReEncodeVideoViewModel>(Window!,
            vm => { vm.Initialize(_videoFileName, SelectedSubtitleFormat); });

        if (!result.OkPressed)
        {
            return;
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
        var result =
            await _windowService.ShowDialogAsync<AdjustDurationWindow, AdjustDurationViewModel>(Window!, vm => { });

        if (result.OkPressed)
        {
            result.AdjustDuration(Subtitles);
            _updateAudioVisualizer = true;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowApplyDurationLimits()
    {
        await _windowService.ShowDialogAsync<ApplyDurationLimitsWindow, ApplyDurationLimitsViewModel>(Window!,
            vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsBatchConvert()
    {
        await _windowService.ShowDialogAsync<BatchConvertWindow, BatchConvertViewModel>(Window!, vm => { });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsJoin()
    {
        var result =
            await _windowService.ShowDialogAsync<JoinSubtitlesWindow, JoinSubtitlesViewModel>(Window!, vm => { });
        _shortcutManager.ClearKeys();
        if (!result.OkPressed)
        {
            return;
        }

        ResetSubtitle();
        SetSubtitles(result.JoinedSubtitle);
        SelectedSubtitleFormat = SubtitleFormats.FirstOrDefault(p => p.Name == result.JoinedFormat.Name) ??
                                 SubtitleFormats[0];
        SelectAndScrollToRow(0);
        ShowStatus(Se.Language.Main.JoinedSubtitleLoaded);
    }

    [RelayCommand]
    private async Task ShowToolsMergeLinesWithSameText()
    {
        var result = await _windowService.ShowDialogAsync<MergeSameTextWindow, MergeSameTextViewModel>(Window!,
            vm => { vm.Initialize(Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList()); });

        if (!result.OkPressed)
        {
            return;
        }

        Subtitles.Clear();
        Subtitles.AddRange(result.ResultSubtitles.Select(p => new SubtitleLineViewModel(p)));
        Renumber();
        SelectAndScrollToRow(0);

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsMergeLinesWithSameTimeCodes()
    {
        var result = await _windowService.ShowDialogAsync<MergeSameTimeCodesWindow, MergeSameTimeCodesViewModel>(
            Window!,
            vm => { vm.Initialize(Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList(), GetUpdateSubtitle()); });

        if (!result.OkPressed)
        {
            return;
        }

        Subtitles.Clear();
        Subtitles.AddRange(result.ResultSubtitles.Select(p => new SubtitleLineViewModel(p)));
        Renumber();
        SelectAndScrollToRow(0);

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void ToolsMakeEmptyTranslationFromCurrentSubtitle()
    {
        foreach (var subtitle in Subtitles)
        {
            subtitle.OriginalText = subtitle.Text;
            subtitle.Text = string.Empty;
        }

        _shortcutManager.ClearKeys();
        ShowColumnOriginalText = true;
        AutoFitColumns();
        ShowStatus(Se.Language.Main.CreatedEmptyTranslation);
    }

    [RelayCommand]
    private async Task ShowToolsSplit()
    {
        var s = GetUpdateSubtitle();
        var fileName = _subtitleFileName;
        if (s.Paragraphs.Count == 0)
        {
            fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle,
                false);
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            s = Subtitle.Parse(fileName);
        }

        if (s == null || s.Paragraphs.Count == 0)
        {
            return;
        }

        await _windowService.ShowDialogAsync<SplitSubtitleWindow, SplitSubtitleViewModel>(Window!,
            vm => { vm.Initialize(fileName ?? string.Empty, s); });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowBridgeGaps()
    {
        var result = await _windowService.ShowDialogAsync<BridgeGapsWindow, BridgeGapsViewModel>(Window!,
            vm => { vm.Initialize(Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList()); });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            Subtitles.AddRange(result.AllSubtitles.Select(p => new SubtitleLineViewModel(p)));
            SelectAndScrollToRow(0);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsChangeCasing()
    {
        var result =
            await _windowService.ShowDialogAsync<ChangeCasingWindow, ChangeCasingViewModel>(Window!,
                vm => { vm.Initialize(GetUpdateSubtitle()); });

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
        var viewModel = await _windowService.ShowDialogAsync<FixCommonErrorsWindow, FixCommonErrorsViewModel>(Window!,
            vm => { vm.Initialize(GetUpdateSubtitle(), SelectedSubtitleFormat); });

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
                Window!, vm => { vm.Initialize(GetUpdateSubtitle()); });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            Subtitles.AddRange(
                result.FixedSubtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
            SelectAndScrollToRow(0);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandVideoOpen()
    {
        var fileName = await _fileHelper.PickOpenVideoFile(Window!, Se.Language.General.OpenVideoFileTitle);
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
    private async Task ShowGoToVideoPosition()
    {
        if (Subtitles.Count == 0 || string.IsNullOrEmpty(_videoFileName) || VideoPlayerControl == null || Window == null)
        {
            return;
        }

        var viewModel =
            await _windowService.ShowDialogAsync<GoToVideoPositionWindow, GoToVideoPositionViewModel>(Window,
                vm => { vm.Time = TimeSpan.FromSeconds(VideoPlayerControl.Position); });

        if (viewModel is { OkPressed: true, Time.TotalMicroseconds: >= 0 })
        {
            VideoPlayerControl.Position = viewModel.Time.TotalSeconds;
            _updateAudioVisualizer = true;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void ToggleIsWaveformToolbarVisible()
    {
        IsWaveformToolbarVisible = !IsWaveformToolbarVisible;
    }

    [RelayCommand]
    private void VideoUndockControls()
    {
        if (Window == null)
        {
            return;
        }

        AreVideoControlsUndocked = true;

        _windowService.ShowWindow<VideoPlayerUndockedWindow, VideoPlayerUndockedViewModel>(Window, (window, vm) =>
        {
            _videoPlayerUndockedViewModel = vm;
            vm.Initialize(VideoPlayerControl!, this);
        });

        _windowService.ShowWindow<AudioVisualizerUndockedWindow, AudioVisualizerUndockedViewModel>(Window, (window, vm) =>
        {
            _audioVisualizerUndockedViewModel = vm;
            vm.Initialize(AudioVisualizer!, this);
        });

        InitLayout.MakeLayout12KeepVideo(MainView!, this);
    }

    [RelayCommand]
    private void VideoRedockControls()
    {
        AreVideoControlsUndocked = false;
        var videoFileName = _videoFileName ?? string.Empty;
        VideoCloseFile();
        VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();

        if (_videoPlayerUndockedViewModel != null)
        {
            _videoPlayerUndockedViewModel.AllowClose = true;
            _videoPlayerUndockedViewModel.Window?.Close();
        }

        if (_audioVisualizerUndockedViewModel != null)
        {
            _audioVisualizerUndockedViewModel.AllowClose = true;
            _audioVisualizerUndockedViewModel.Window?.Close();
        }

        VideoPlayerControl = null;
        InitLayout.MakeLayout(MainView!, this, Se.Settings.General.LayoutNumber);

        if (!string.IsNullOrEmpty(videoFileName))
        {
            Dispatcher.UIThread.Post(async void () => { await VideoOpenFile(videoFileName); });
        }
    }

    [RelayCommand]
    private async Task ShowSpellCheck()
    {
        var result = await _windowService.ShowDialogAsync<SpellCheckWindow, SpellCheckViewModel>(Window!,
            vm => { vm.Initialize(Subtitles, SelectedSubtitleIndex, this); });

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

        var result =
            await _windowService.ShowDialogAsync<AudioToTextWhisperWindow, AudioToTextWhisperViewModel>(Window!,
                vm => { vm.Initialize(_videoFileName); });

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
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        await _windowService.ShowDialogAsync<BurnInWindow, BurnInViewModel>(Window!,
            vm => { vm.Initialize(_videoFileName ?? string.Empty, GetUpdateSubtitle(), SelectedSubtitleFormat); });
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
            }
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoTextToSpeech()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        await _windowService.ShowDialogAsync<TextToSpeechWindow, TextToSpeechViewModel>(Window!, vm =>
        {
            vm.Initialize(GetUpdateSubtitle(), _videoFileName ?? string.Empty, AudioVisualizer?.WavePeaks,
                Path.GetTempPath());
        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoTransparentSubtitles()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        await _windowService.ShowDialogAsync<TransparentSubtitlesWindow, TransparentSubtitlesViewModel>(Window!,
            vm => { vm.Initialize(_videoFileName ?? string.Empty, GetUpdateSubtitle(), SelectedSubtitleFormat); });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowShotChangesSubtitles()
    {
        if (string.IsNullOrEmpty(_videoFileName) || VideoPlayerControl == null || AudioVisualizer == null)
        {
            return;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ShotChangesWindow, ShotChangesViewModel>(Window!, vm => { vm.Initialize(_videoFileName); });

        if (result.OkPressed && result.FfmpegLines.Count > 0)
        {
            AudioVisualizer.ShotChanges = result.FfmpegLines.Select(p => p.Seconds).ToList();
            ShowShotChangesListMenuItem = AudioVisualizer.ShotChanges.Count > 0;
            _updateAudioVisualizer = true;
            ShotChangesHelper.SaveShotChanges(_videoFileName, AudioVisualizer.ShotChanges);
            ShowStatus(string.Format(Se.Language.Main.XShotChangedLoaded, AudioVisualizer.ShotChanges.Count));
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowShotChangesList()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ShotChangeListWindow, ShotChangeListViewModel>(Window!,
            vm => { vm.Initialize(AudioVisualizer?.ShotChanges ?? new List<double>()); });

        if (result.OKProssed && AudioVisualizer != null)
        {
            AudioVisualizer.ShotChanges = result.ShotChanges.Select(p => p.Seconds).ToList();
        }

        if (result.GoToPressed && result.SelectedShotChange != null)
        {
            VideoPlayerControl!.Position = result.SelectedShotChange.Seconds;
        }

        ShowShotChangesListMenuItem = AudioVisualizer?.ShotChanges.Count > 0;
        _updateAudioVisualizer = true;

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void ToggleShotChangesAtVideoPosition()
    {
        if (string.IsNullOrEmpty(_videoFileName) || VideoPlayerControl == null || AudioVisualizer == null)
        {
            return;
        }

        var cp = AudioVisualizer.CurrentVideoPositionSeconds;
        var idx = AudioVisualizer.GetShotChangeIndex(cp);
        if (idx >= 0)
        {
            RemoveShotChange(idx);
            if (AudioVisualizer.ShotChanges.Count == 0)
            {
                ShotChangesHelper.DeleteShotChanges(_videoFileName);
            }
        }
        else
        {
            // add shot change
            var list = AudioVisualizer.ShotChanges.Where(p => p > 0).ToList();
            list.Add(cp);
            list.Sort();
            AudioVisualizer.ShotChanges = list;
            ShotChangesHelper.SaveShotChanges(_videoFileName, list);
        }

        ShowShotChangesListMenuItem = AudioVisualizer?.ShotChanges.Count > 0;
        _updateAudioVisualizer = true;
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
    private async Task ShowVisualSync()
    {
        var result = await _windowService.ShowDialogAsync<VisualSyncWindow, VisualSyncViewModel>(Window!, vm =>
        {
            var paragraphs = Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList();
            vm.Initialize(paragraphs, _videoFileName, _subtitleFileName, AudioVisualizer);
        });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            foreach (var p in result.Paragraphs)
            {
                Subtitles.Add(p.Subtitle);
            }

            SelectAndScrollToRow(0);
        }

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
    private async Task ShowAutoTranslate()
    {
        var result = await _windowService.ShowDialogAsync<AutoTranslateWindow, AutoTranslateViewModel>(Window!, vm => { vm.Initialize(GetUpdateSubtitle()); });

        if (!result.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        for (var i = 0; i < Subtitles.Count; i++)
        {
            if (result.Rows.Count <= i)
            {
                break;
            }

            Subtitles[i].OriginalText = Subtitles[i].Text;
            Subtitles[i].Text = result.Rows[i].TranslatedText;
        }

        _subtitleFileNameOriginal = _subtitleFileName;
        _subtitleFileName = string.Empty;
        ShowColumnOriginalText = true;
        AutoFitColumns();
        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task AutoTranslateSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0 || !ShowColumnOriginalText)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AutoTranslateWindow, AutoTranslateViewModel>(Window!, vm =>
        {
            var sub = new Subtitle();
            foreach (var line in selectedItems)
            {
                var p = new Paragraph()
                {
                    Number = line.Number,
                    StartTime = new TimeCode(line.StartTime),
                    EndTime = new TimeCode(line.EndTime),
                    Text = line.OriginalText,
                    Actor = line.Actor,
                    Style = line.Style,
                    Language = line.Language,
                    Region = line.Region,
                    Layer = line.Layer,
                    Bookmark = line.Bookmark,
                };
                sub.Paragraphs.Add(p);
            }

            vm.Initialize(sub);
        });

        if (!result.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        for (int i = 0; i < result.Rows.Count; i++)
        {
            var translatedText = result.Rows[i].TranslatedText;
            var id = selectedItems[i].Id;
            var p = Subtitles.FirstOrDefault(x => x.Id == id);
            if (p != null && !string.IsNullOrEmpty(translatedText))
            {
                p.Text = translatedText;
            }
        }

        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ChangeCasingSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ChangeCasingWindow, ChangeCasingViewModel>(Window!, vm =>
        {
            var sub = new Subtitle();
            foreach (var line in selectedItems)
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
                    Region = line.Region,
                    Layer = line.Layer,
                    Bookmark = line.Bookmark,
                };
                sub.Paragraphs.Add(p);
            }

            vm.Initialize(sub);
        });

        if (!result.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        for (var i = 0; i < result.Subtitle.Paragraphs.Count; i++)
        {
            var text = result.Subtitle.Paragraphs[i].Text;
            var id = selectedItems[i].Id;
            var p = Subtitles.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                p.Text = text;
            }
        }

        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task StatisticsSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<StatisticsWindow, StatisticsViewModel>(Window!, vm =>
        {
            var sub = new Subtitle();
            foreach (var line in selectedItems)
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
                    Region = line.Region,
                    Layer = line.Layer,
                    Bookmark = line.Bookmark,
                };
                sub.Paragraphs.Add(p);
            }

            vm.Initialize(sub, SelectedSubtitleFormat, _subtitleFileName ?? string.Empty);
        });

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task MultipleReplaceSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<MultipleReplaceWindow, MultipleReplaceViewModel>(Window!, vm =>
        {
            var sub = new Subtitle();
            foreach (var line in selectedItems)
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
                    Region = line.Region,
                    Layer = line.Layer,
                    Bookmark = line.Bookmark,
                };
                sub.Paragraphs.Add(p);
            }

            vm.Initialize(sub);
        });

        if (!result.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        for (var i = 0; i < result.FixedSubtitle.Paragraphs.Count; i++)
        {
            var text = result.FixedSubtitle.Paragraphs[i].Text;
            var id = selectedItems[i].Id;
            var p = Subtitles.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                p.Text = text;
            }
        }

        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task FixCommonErrorsSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<FixCommonErrorsWindow, FixCommonErrorsViewModel>(Window!, vm =>
        {
            var sub = new Subtitle();
            foreach (var line in selectedItems)
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
                    Region = line.Region,
                    Layer = line.Layer,
                    Bookmark = line.Bookmark,
                };
                sub.Paragraphs.Add(p);
            }

            vm.Initialize(sub, SelectedSubtitleFormat);
        });

        if (!result.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        for (var i = 0; i < result.FixedSubtitle.Paragraphs.Count; i++)
        {
            var text = result.FixedSubtitle.Paragraphs[i].Text;
            var id = selectedItems[i].Id;
            var p = Subtitles.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                p.Text = text;
            }
        }

        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }


    private DataGrid _oldSubtitleGrid = new DataGrid();
    private TextBox _oldEditTextBox = new TextBox();

    [RelayCommand]
    private async Task CommandShowSettings()
    {
        _oldSubtitleGrid = SubtitleGrid;
        _oldEditTextBox = EditTextBox;

        var viewModel = await _windowService
            .ShowDialogAsync<SettingsWindow, SettingsViewModel>(Window!, vm => { vm.Initialize(this); });

        if (!viewModel.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        ApplySettings();

        _shortcutManager.ClearKeys();
    }

    public void ApplySettings()
    {
        UiUtil.SetFontName(Se.Settings.Appearance.FontName);
        UiUtil.SetCurrentTheme();

        InitListViewAndEditBox.MakeLayoutListViewAndEditBox(MainView!, this);
        UiUtil.ReplaceControl(_oldSubtitleGrid, SubtitleGrid);
        UiUtil.ReplaceControl(_oldEditTextBox, EditTextBox);

        if (Toolbar is Border toolbarBorder)
        {
            var tb = InitToolbar.Make(this);
            if (tb is Border newToolbarBorder)
            {
                var grid = newToolbarBorder.Child;
                newToolbarBorder.Child = null;
                toolbarBorder.Child = grid;
            }
        }

        LockTimeCodes = Se.Settings.General.LockTimeCodes;
        IsWaveformToolbarVisible = Se.Settings.Waveform.ShowToolbar;

        if (AudioVisualizer != null)
        {
            AudioVisualizer.DrawGridLines = Se.Settings.Waveform.DrawGridLines;
            AudioVisualizer.WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor();
            AudioVisualizer.WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor();
            AudioVisualizer.WaveformCursorColor = Se.Settings.Waveform.WaveformCursorColor.FromHexToColor();
            AudioVisualizer.InvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel;
            AudioVisualizer.UpdateTheme();
            AudioVisualizer.IsReadOnly = LockTimeCodes;
        }

        ShowUpDownStartTime = Se.Settings.Appearance.ShowUpDownStartTime;
        ShowUpDownEndTime = Se.Settings.Appearance.ShowUpDownEndTime;
        ShowUpDownDuration = Se.Settings.Appearance.ShowUpDownDuration;
        ShowUpDownLabels = Se.Settings.Appearance.ShowUpDownLabels;

        _errorColor = Se.Settings.General.ErrorColor.FromHexToColor();

        InitLayout.MakeLayout(MainView!, this, Se.Settings.General.LayoutNumber);

        _autoBackupService.StopAutobackup();
        _autoBackupService.StartAutoBackup(this);

        _updateAudioVisualizer = true;

        _oldSubtitleGrid = SubtitleGrid;
        _oldEditTextBox = EditTextBox;
    }

    [RelayCommand]
    private async Task CommandShowSettingsShortcuts()
    {
        await _windowService.ShowDialogAsync<ShortcutsWindow, ShortcutsViewModel>(Window!,
            vm => { vm.LoadShortCuts(this); });
        ReloadShortcuts();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowWordLists()
    {
        var result = await _windowService.ShowDialogAsync<WordListsWindow, WordListsViewModel>(Window!);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task AddOrEditBookmark()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BookmarkEditWindow, BookmarkEditViewModel>(Window!, vm =>
        {
            var allBookmarks = Subtitles.Where(p => p.Bookmark != null).ToList();
            vm.Initialize(selectedItems, allBookmarks);
        });

        if (result.OkPressed)
        {
            foreach (var item in selectedItems)
            {
                item.Bookmark = result.BookmarkText;
            }

            new BookmarkPersistence(GetUpdateSubtitle(), _subtitleFileName).Save();
        }
        else if (result.ListPressed)
        {
            await ListBookmarks();
        }

        _shortcutManager.ClearKeys();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ToggleBookmarkSelectedLinesNoText()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        foreach (var item in selectedItems)
        {
            if (item.Bookmark == null)
            {
                item.Bookmark = string.Empty;
            }
            else
            {
                item.Bookmark = null;
            }
        }

        new BookmarkPersistence(GetUpdateSubtitle(), _subtitleFileName).Save();

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ListBookmarks()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BookmarksListWindow, BookmarksListViewModel>(Window!,
            vm => { vm.Initialize(Subtitles.Where(p => p.Bookmark != null).ToList()); });

        if (result.GoToPressed && result.SelectedSubtitle != null)
        {
            SelectAndScrollToSubtitle(result.SelectedSubtitle);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveBookmarkSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        foreach (var item in selectedItems)
        {
            item.Bookmark = null;
        }

        new BookmarkPersistence(GetUpdateSubtitle(), _subtitleFileName).Save();

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void GoToNextBookmark()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(selected);
        if (idx < 0)
        {
            return;
        }

        for (var i = idx + 1; i < Subtitles.Count; i++)
        {
            if (Subtitles[i].Bookmark != null)
            {
                SelectAndScrollToSubtitle(Subtitles[i]);
                return;
            }
        }

        ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
    }

    [RelayCommand]
    private async Task CommandShowSettingsLanguage()
    {
        var viewModel = await _windowService.ShowDialogAsync<LanguageWindow, LanguageViewModel>(Window!);
        if (viewModel.OkPressed && viewModel.SelectedLanguage != null)
        {
            var jsonFileName = viewModel.SelectedLanguage.FileName;
            var json = await File.ReadAllTextAsync(jsonFileName, Encoding.UTF8);
            var language = System.Text.Json.JsonSerializer.Deserialize<SeLanguage>(json,
                new System.Text.Json.JsonSerializerOptions
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
    private void ToggleShowColumnEndTime()
    {
        Se.Settings.General.ShowColumnEndTime = !Se.Settings.General.ShowColumnEndTime;
        ShowColumnEndTime = Se.Settings.General.ShowColumnEndTime;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnGap()
    {
        Se.Settings.General.ShowColumnGap = !Se.Settings.General.ShowColumnGap;
        ShowColumnGap = Se.Settings.General.ShowColumnGap;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnDuration()
    {
        Se.Settings.General.ShowColumnDuration = !Se.Settings.General.ShowColumnDuration;
        ShowColumnDuration = Se.Settings.General.ShowColumnDuration;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnActor()
    {
        Se.Settings.General.ShowColumnActor = !Se.Settings.General.ShowColumnActor;
        ShowColumnActor = Se.Settings.General.ShowColumnActor;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnCps()
    {
        Se.Settings.General.ShowColumnCps = !Se.Settings.General.ShowColumnCps;
        ShowColumnCps = Se.Settings.General.ShowColumnCps;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnWpm()
    {
        Se.Settings.General.ShowColumnWpm = !Se.Settings.General.ShowColumnWpm;
        ShowColumnWpm = Se.Settings.General.ShowColumnWpm;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnLayer()
    {
        ShowColumnLayer = !ShowColumnLayer;
        Se.Settings.General.ShowColumnLayer = ShowColumnLayer;
        ShowColumnLayer = Se.Settings.General.ShowColumnLayer;
        AutoFitColumns();
    }

    [RelayCommand]
    private void DuplicateSelectedLines()
    {
        var newSubtitles = new List<SubtitleLineViewModel>();
        foreach (var selected in _selectedSubtitles ?? [])
        {
            newSubtitles.Add(new SubtitleLineViewModel(selected));
        }

        foreach (var newSubtitle in newSubtitles)
        {
            _insertService.InsertInCorrectPosition(Subtitles, newSubtitle);
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
    private async Task ShowAlignmentPicker()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<PickAlignmentWindow, PickAlignmentViewModel>(Window!,
            vm => { vm.Initialize(selected, SubtitleGrid.SelectedItems.Count); });

        if (result.OkPressed)
        {
            SetAlignmentToSelected(result.Alignment);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowFontNamePicker()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result =
            await _windowService.ShowDialogAsync<PickFontNameWindow, PickFontNameViewModel>(Window!,
                vm => { vm.Initialize(); });

        if (result.OkPressed && result.SelectedFontName != null)
        {
            _fontNameService.SetFontName(selectedItems, result.SelectedFontName, SelectedSubtitleFormat);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowColorPicker()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<PickColorWindow, PickColorViewModel>(Window!, vm =>
        {
            // vm.Initialize();
        });

        if (result.OkPressed)
        {
            _colorService.SetColor(selectedItems, result.SelectedColor, GetUpdateSubtitle(), SelectedSubtitleFormat);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingAll()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = HtmlUtil.RemoveHtmlTags(item.Text, true);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingItalic()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = item.Text.Replace("{\\i1}", string.Empty);
            item.Text = item.Text.Replace("{\\i0}", string.Empty);
            item.Text = item.Text.Replace("\\i1", string.Empty);
            item.Text = item.Text.Replace("\\i0", string.Empty);

            item.Text = HtmlUtil.RemoveOpenCloseTags(item.Text, "i");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingBold()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = item.Text.Replace("{\\b1}", string.Empty);
            item.Text = item.Text.Replace("{\\b0}", string.Empty);
            item.Text = item.Text.Replace("\\b1", string.Empty);
            item.Text = item.Text.Replace("\\b0", string.Empty);

            item.Text = HtmlUtil.RemoveOpenCloseTags(item.Text, "b");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingUnderline()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = item.Text.Replace("{\\u1}", string.Empty);
            item.Text = item.Text.Replace("{\\u0}", string.Empty);
            item.Text = item.Text.Replace("\\u1", string.Empty);
            item.Text = item.Text.Replace("\\u0", string.Empty);

            item.Text = HtmlUtil.RemoveOpenCloseTags(item.Text, "u");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingColor()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        _colorService.RemoveColorTags(selectedItems);

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingFontName()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        _fontNameService.RemoveFontNames(selectedItems, SelectedSubtitleFormat);

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingAligment()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = HtmlUtil.RemoveAssAlignmentTags(item.Text);
        }

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

        var result =
            await _windowService.ShowDialogAsync<ShowHistoryWindow, ShowHistoryViewModel>(Window!,
                vm => { vm.Initialize(_undoRedoManager); });

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
    private void ShowFind()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null || Window == null)
        {
            return;
        }

        if (_replaceViewModel != null)
        {
            _replaceViewModel.Window?.Close();
            _replaceViewModel = null;
        }

        if (_findViewModel != null && _findViewModel.Window != null && _findViewModel.Window.IsVisible)
        {
            _findViewModel.Window.Activate();
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();
        var result = _windowService.ShowWindow<FindWindow, FindViewModel>(Window!, (window, vm) =>
        {
            window.Topmost = true;
            _findViewModel = vm;

            var selectedText = string.Empty;
            if (EditTextBox != null && !string.IsNullOrEmpty(EditTextBox.SelectedText))
            {
                selectedText = EditTextBox.SelectedText;
            }

            if (string.IsNullOrEmpty(selectedText) && !string.IsNullOrEmpty(_findService.SearchText))
            {
                selectedText = _findService.SearchText;
            }

            vm.InitializeFindData(_findService, subs, selectedText, this);
        });

        _shortcutManager.ClearKeys();
    }

    public void RequestFindData()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null || _findViewModel == null)
        {
            return;
        }

        var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
        var currentCharIndex = EditTextBox.CaretIndex;
        var subs = Subtitles.Select(p => p.Text).ToList();
        _findViewModel.InitializeFindData(_findService, subs, _findService.SearchText, this);
    }

    public void HandleFindResult(FindViewModel result)
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null)
        {
            return;
        }

        if ((result.FindNextPressed || result.FindPreviousPressed) && !string.IsNullOrEmpty(result.SearchText))
        {
            var findMode = FindMode.CaseSensitive;
            if (result.FindTypeCanseInsensitive)
            {
                findMode = FindMode.CaseInsensitive;
            }
            else if (result.FindTypeRegularExpression)
            {
                findMode = FindMode.RegularExpression;
            }

            var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
            var currentCharIndex = EditTextBox.CaretIndex;
            var subs = Subtitles.Select(p => p.Text).ToList();
            _findService.Initialize(subs, SelectedSubtitleIndex ?? 0, result.WholeWord, findMode);

            var idx = -1;
            if (result.FindNextPressed)
            {
                idx = _findService.FindNext(result.SearchText, subs, currentLineIndex, currentCharIndex + 1);
            }
            else
            {
                idx = _findService.FindPrevious(result.SearchText, subs, currentLineIndex, currentCharIndex - 1);
            }

            if (idx < 0)
            {
                ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                SubtitleGrid.SelectedIndex = idx;
                SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);

                ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, _findService.CurrentTextFound, _findService.CurrentLineNumber + 1, _findService.CurrentTextIndex + 1));

                // wait for text box to update
                Task.Delay(50);

                EditTextBox.CaretIndex = _findService.CurrentTextIndex;
                EditTextBox.SelectionStart = _findService.CurrentTextIndex;
                EditTextBox.SelectionEnd = _findService.CurrentTextIndex + _findService.CurrentTextFound.Length;
            });
        }
    }

    [RelayCommand]
    private void FindNext()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null)
        {
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();
        var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
        var currentCharIndex = EditTextBox.CaretIndex;
        var idx = _findService.FindNext(_findService.SearchText, subs, currentLineIndex, currentCharIndex + 1);

        if (idx < 0)
        {
            ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleGrid.SelectedIndex = idx;
            SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);

            ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, _findService.CurrentTextFound, _findService.CurrentLineNumber + 1, _findService.CurrentTextIndex + 1));

            // wait for text box to update
            Task.Delay(50);

            EditTextBox.CaretIndex = _findService.CurrentTextIndex;
            EditTextBox.SelectionStart = _findService.CurrentTextIndex;
            EditTextBox.SelectionEnd = _findService.CurrentTextIndex + _findService.CurrentTextFound.Length;
        });


        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void FindPrevious()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null)
        {
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();
        var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
        var currentCharIndex = EditTextBox.CaretIndex;
        var idx = _findService.FindPrevious(_findService.SearchText, subs, currentLineIndex, currentCharIndex - 1);

        if (idx < 0)
        {
            ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleGrid.SelectedIndex = idx;
            SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);

            ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, _findService.CurrentTextFound, _findService.CurrentLineNumber + 1, _findService.CurrentTextIndex + 1));

            // wait for text box to update
            Task.Delay(50);

            EditTextBox.CaretIndex = _findService.CurrentTextIndex;
            EditTextBox.SelectionStart = _findService.CurrentTextIndex;
            EditTextBox.SelectionEnd = _findService.CurrentTextIndex + _findService.CurrentTextFound.Length;
        });
    }

    [RelayCommand]
    private void ShowReplace()
    {
        if (Subtitles.Count == 0 || Window == null)
        {
            return;
        }

        if (_findViewModel != null)
        {
            _findViewModel.Window?.Close();
            _findViewModel = null;
        }

        if (_replaceViewModel != null && _replaceViewModel.Window != null && _replaceViewModel.Window.IsVisible)
        {
            _replaceViewModel.Window.Activate();
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();
        var result = _windowService.ShowWindow<ReplaceWindow, ReplaceViewModel>(Window, (window, vm) =>
        {
            window.Topmost = true;
            _replaceViewModel = vm;

            var selectedText = string.Empty;
            if (EditTextBox != null && !string.IsNullOrEmpty(EditTextBox.SelectedText))
            {
                selectedText = EditTextBox.SelectedText;
            }

            if (string.IsNullOrEmpty(selectedText) && !string.IsNullOrEmpty(_findService.SearchText))
            {
                selectedText = _findService.SearchText;
            }

            vm.InitializeFindData(_findService, subs, selectedText, this);
        });

        _shortcutManager.ClearKeys();
    }

    public void HandleReplaceResult(ReplaceViewModel result)
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null)
        {
            return;
        }

        if ((result.FindNextPressed || result.ReplaceNextPressed || result.ReplaceAllPressed) && !string.IsNullOrEmpty(result.SearchText))
        {
            var findMode = FindMode.CaseSensitive;
            if (result.FindTypeCanseInsensitive)
            {
                findMode = FindMode.CaseInsensitive;
            }
            else if (result.FindTypeRegularExpression)
            {
                findMode = FindMode.RegularExpression;
            }

            var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
            var currentCharIndex = EditTextBox.CaretIndex;
            var subs = Subtitles.Select(p => p.Text).ToList();
            _findService.Initialize(subs, SelectedSubtitleIndex ?? 0, result.WholeWord, findMode);

            var idx = -1;
            if (result.FindNextPressed)
            {
                idx = _findService.FindNext(result.SearchText, subs, currentLineIndex, currentCharIndex + 1);
            }
            else if (result.ReplaceAllPressed)
            {
                var replaceCount = _findService.ReplaceAll(result.SearchText, result.ReplaceText);

                for (var i = 0; i < Subtitles.Count && i < subs.Count; i++)
                {
                    var s = Subtitles[i];
                    var newText = subs[i];
                    if (newText != s.Text)
                    {
                        s.Text = newText;
                    }
                }

                ShowStatus(string.Format(Se.Language.Main.ReplacedXWithYCountZ, result.SearchText, result.ReplaceText, replaceCount));
                return;
            }
            else
            {
                idx = _findService.ReplaceNext(result.SearchText, result.ReplaceText, subs, currentLineIndex, currentCharIndex);
                if (idx >= 0)
                {
                    var s = Subtitles[idx];
                    var newText = subs[idx];
                    if (newText != s.Text)
                    {
                        s.Text = newText;
                        ShowStatus(string.Format(Se.Language.Main.ReplacedXWithYInLineZ, result.SearchText, result.ReplaceText, idx));
                        Dispatcher.UIThread.Post(() =>
                        {
                            SubtitleGrid.SelectedIndex = idx;
                            SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);

                            ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, _findService.CurrentTextFound, _findService.CurrentLineNumber + 1,
                                _findService.CurrentTextIndex + 1));

                            // wait for text box to update
                            Task.Delay(50);

                            EditTextBox.CaretIndex = _findService.CurrentTextIndex;
                            EditTextBox.SelectionStart = _findService.CurrentTextIndex;
                            EditTextBox.SelectionEnd = _findService.CurrentTextIndex + _findService.CurrentTextFound.Length;
                        });
                        return;
                    }
                }

                ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                return;
            }

            if (idx < 0)
            {
                ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                SubtitleGrid.SelectedIndex = idx;
                SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);

                ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, _findService.CurrentTextFound, _findService.CurrentLineNumber + 1, _findService.CurrentTextIndex + 1));

                // wait for text box to update
                Task.Delay(50);

                EditTextBox.CaretIndex = _findService.CurrentTextIndex;
                EditTextBox.SelectionStart = _findService.CurrentTextIndex;
                EditTextBox.SelectionEnd = _findService.CurrentTextIndex + _findService.CurrentTextFound.Length;
            });
        }
    }

    [RelayCommand]
    private async Task ShowMultipleReplace()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var result =
            await _windowService.ShowDialogAsync<MultipleReplaceWindow, MultipleReplaceViewModel>(Window!,
                vm => { vm.Initialize(GetUpdateSubtitle()); });

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

        var viewModel = await _windowService.ShowDialogAsync<GoToLineNumberWindow, GoToLineNumberViewModel>(Window!, vm =>
        {
            var idx = 1;
            if (SelectedSubtitle != null)
            {
                idx = Subtitles.IndexOf(SelectedSubtitle) + 1;
            }

            vm.Initialize(idx, Subtitles.Count);
        });

        if (viewModel is { OkPressed: true, LineNumber: >= 0 } && viewModel.LineNumber <= Subtitles.Count)
        {
            var no = (int)viewModel.LineNumber;
            SelectAndScrollToRow(no - 1);
            if (Se.Settings.Tools.GoToLineNumberAlsoSetVideoPosition && !string.IsNullOrEmpty(_videoFileName) && VideoPlayerControl != null)
            {
                var s = Subtitles.GetOrNull(no - 1);
                if (s != null)
                {
                    VideoPlayerControl.Position = s.StartTime.TotalSeconds;
                    _updateAudioVisualizer = true;
                }
            }
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RightToLeftToggle()
    {
        if (Window == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (Window.FlowDirection == FlowDirection.RightToLeft)
            {
                IsRightToLeftEnabled = false;
                Se.Settings.Appearance.RightToLeft = false;
                Window.FlowDirection = FlowDirection.LeftToRight;
            }
            else
            {
                IsRightToLeftEnabled = true;
                Se.Settings.Appearance.RightToLeft = true;
                Window.FlowDirection = FlowDirection.RightToLeft;
            }

            // Force UI to update layout
            var content = Window.Content;
            Window.Content = null;
            Window.Content = content;
            Task.Delay(50);
            Window.InvalidateMeasure();
            Window.InvalidateArrange();
            Window.InvalidateVisual();
            Task.Delay(50);
            Window.Width += 0.1;
            Task.Delay(50);
            Window.Width -= 0.1;
        });

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

    [RelayCommand]
    private void GoToNextLineAndSetVideoPosition()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (Subtitles.Count == 0 ||
            idx < 0 || idx - 2 >= Subtitles.Count ||
            string.IsNullOrEmpty(_videoFileName) ||
            VideoPlayerControl == null)
        {
            return;
        }

        idx++;
        if (idx >= Subtitles.Count)
        {
            return;
        }

        SelectAndScrollToRow(idx);
        VideoPlayerControl.Position = Subtitles[idx].StartTime.TotalSeconds;
    }

    [RelayCommand]
    private void GoToPreviousLineAndSetVideoPosition()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (Subtitles.Count == 0 ||
            idx <= 0 || idx >= Subtitles.Count ||
            string.IsNullOrEmpty(_videoFileName) ||
            VideoPlayerControl == null)
        {
            return;
        }

        idx--;
        if (idx < 0)
        {
            return;
        }

        VideoPlayerControl.Position = Subtitles[idx].StartTime.TotalSeconds;
        SelectAndScrollToRow(idx);
    }

    private Control? _fullScreenBeforeParent;

    [RelayCommand]
    private void VideoFullScreen()
    {
        var control = VideoPlayerControl;
        if (control == null || control.IsFullScreen || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var parent = (Control)control.Parent!;
        _fullScreenBeforeParent = parent;
        control.RemoveControlFromParent();
        control.IsFullScreen = true;
        var fullScreenWindow = new FullScreenVideoWindow(control, _videoFileName, () =>
        {
            if (_fullScreenBeforeParent != null)
            {
                control.RemoveControlFromParent().AddControlToParent(_fullScreenBeforeParent);
            }

            control.IsFullScreen = false;

            if (OperatingSystem.IsMacOS() && !string.IsNullOrEmpty(_videoFileName) && VideoPlayerControl != null)
            {
                VideoPlayerControl.Reload();
            }
        });
        fullScreenWindow.Show(Window!);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void ToggleVideoPlayerDisplayTimeLeft()
    {
        Se.Settings.Video.VideoPlayerDisplayTimeLeft = !Se.Settings.Video.VideoPlayerDisplayTimeLeft;

        if (VideoPlayerControl != null)
        {
            VideoPlayerControl.VideoPlayerDisplayTimeLeft = Se.Settings.Video.VideoPlayerDisplayTimeLeft;
        }
    }

    [RelayCommand]
    private void Unbreak()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var s in selectedItems)
        {
            s.Text = Utilities.UnbreakLine(s.Text);
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void AutoBreak()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var s in selectedItems)
        {
            s.Text = Utilities.AutoBreakLine(s.Text);
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void Split()
    {
        SplitSelectedLine(false, false);
    }

    [RelayCommand]
    private void SplitAtVideoPosition()
    {
        SplitSelectedLine(true, false);
    }

    [RelayCommand]
    private void SplitAtTextBoxCursorPosition()
    {
        SplitSelectedLine(false, true);
    }

    [RelayCommand]
    private void SplitAtVideoPositionAndTextBoxCursorPosition()
    {
        SplitSelectedLine(true, true);
    }

    [RelayCommand]
    private void TextBoxRemoveAllFormatting()
    {
        var tb = EditTextBox;
        if (tb == null || tb.Text == null)
        {
            return;
        }

        var selectionStart = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var selectionEnd = Math.Max(tb.SelectionStart, tb.SelectionEnd);
        var selectionLength = selectionEnd - selectionStart;

        if (selectionLength == 0)
        {
            tb.Text = HtmlUtil.RemoveHtmlTags(tb.Text, true);
        }
        else
        {
            var selectedText = tb.Text.Substring(selectionStart, selectionLength);
            var newText = HtmlUtil.RemoveHtmlTags(selectedText, true);
            tb.Text = tb.Text
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, newText);
            tb.SelectionStart = selectionStart;
            tb.SelectionEnd = selectionStart + newText.Length;
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void TextBoxBold()
    {
        var tb = EditTextBox;
        ToggleTextBoxTag(tb, "b", "b1", "b0");
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void TextBoxItalic()
    {
        var tb = EditTextBox;
        ToggleTextBoxTag(tb, "i", "i1", "i0");
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void TextBoxUnderline()
    {
        var tb = EditTextBox;
        ToggleTextBoxTag(tb, "u", "u1", "u0");
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task TextBoxColor()
    {
        var tb = EditTextBox;
        if (tb == null || tb.Text == null)
        {
            return;
        }

        var selectionStart = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var selectionEnd = Math.Max(tb.SelectionStart, tb.SelectionEnd);
        var selectionLength = selectionEnd - selectionStart;

        var result = await _windowService.ShowDialogAsync<PickColorWindow, PickColorViewModel>(Window!);
        if (!result.OkPressed)
        {
            return;
        }

        var isAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        var isWebVtt = SelectedSubtitleFormat is WebVTT;
        if (selectionLength == 0 || selectionLength == tb.Text.Length)
        {
            tb.Text = _colorService.SetColorTag(tb.Text, result.SelectedColor, isAssa, isWebVtt, GetUpdateSubtitle());
        }
        else
        {
            var selectedText = tb.Text.Substring(selectionStart, selectionLength);
            selectedText = _colorService.SetColorTag(selectedText, result.SelectedColor, isAssa, isWebVtt,
                GetUpdateSubtitle());

            if (isAssa) // close color tag (display normal style color)
            {
                var closeTag = "{\\c&HFFFFFF&}"; // white color
                var styleName = SelectedSubtitle?.Style;
                if (_subtitle != null && _subtitle.Header != null && styleName != null)
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, _subtitle.Header);
                    var endColor =
                        _colorService.SetColorTag("x", style.Primary.ToAvaloniaColor(), true, false, _subtitle);
                    closeTag = endColor.TrimEnd('x');
                }

                selectedText += closeTag;
            }

            tb.Text = tb.Text
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, selectedText);

            Dispatcher.UIThread.Post(() =>
            {
                tb.Focus();
                tb.SelectionStart = selectionStart;
                tb.SelectionEnd = selectionStart + selectedText.Length;
            });
        }
    }

    [RelayCommand]
    private async Task TextBoxFontName()
    {
        var tb = EditTextBox;
        if (tb == null || tb.Text == null)
        {
            return;
        }

        var selectionStart = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var selectionEnd = Math.Max(tb.SelectionStart, tb.SelectionEnd);
        var selectionLength = selectionEnd - selectionStart;

        var result =
            await _windowService.ShowDialogAsync<PickFontNameWindow, PickFontNameViewModel>(Window!,
                vm => { vm.Initialize(); });

        if (!result.OkPressed || result.SelectedFontName == null)
        {
            return;
        }

        var isAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        var isWebVtt = SelectedSubtitleFormat is WebVTT;
        if (selectionLength == 0)
        {
            tb.Text = _fontNameService.SetFontName(tb.Text, result.SelectedFontName, isAssa);
        }
        else
        {
            var selectedText = tb.Text.Substring(selectionStart, selectionLength);
            selectedText = _fontNameService.SetFontName(selectedText, result.SelectedFontName, isAssa);
            tb.Text = tb.Text
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, selectedText);

            Dispatcher.UIThread.Post(() =>
            {
                tb.Focus();
                tb.SelectionStart = selectionStart;
                tb.SelectionEnd = selectionStart + selectedText.Length;
            });
        }
    }

    [RelayCommand]
    private void WaveformInsertNewSelection()
    {
        if (VideoPlayerControl == null ||
            AudioVisualizer == null ||
            AudioVisualizer.NewSelectionParagraph == null)
        {
            return;
        }

        var newParagraph = AudioVisualizer.NewSelectionParagraph;
        _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
        AudioVisualizer.NewSelectionParagraph = null;
        SelectAndScrollToSubtitle(newParagraph);
        EditTextBox.Focus();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformInsertAtPosition()
    {
        if (VideoPlayerControl == null ||
            AudioVisualizer == null ||
            AudioVisualizer.NewSelectionParagraph == null)
        {
            return;
        }

        var startMs = VideoPlayerControl.Position * 1000.0;
        var endMs = startMs + Se.Settings.General.NewEmptyDefaultMs;
        var newParagraph =
            new SubtitleLineViewModel(new Paragraph(string.Empty, startMs, endMs), SelectedSubtitleFormat);
        var idx = _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next != null)
        {
            if (next.StartTime.TotalMilliseconds < endMs)
            {
                newParagraph.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds -
                                                                 Se.Settings.General.MinimumMillisecondsBetweenLines);
            }
        }

        AudioVisualizer.NewSelectionParagraph = null;
        SelectAndScrollToSubtitle(newParagraph);
        EditTextBox.Focus();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformDeleteAtPosition()
    {
        if (VideoPlayerControl == null ||
            AudioVisualizer == null)
        {
            return;
        }

        var pos = VideoPlayerControl.Position;
        var subtitlesAtPosition = Subtitles
            .Where(p =>
                p.StartTime.TotalSeconds < pos &&
                p.EndTime.TotalSeconds > pos).ToList();

        foreach (var p in subtitlesAtPosition)
        {
            Subtitles.Remove(p);
        }

        Renumber();

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformSetStartAndOffsetTheRest()
    {
        var s = SelectedSubtitle;
        if (s == null || VideoPlayerControl == null || LockTimeCodes)
        {
            return;
        }

        var videoPositionSeconds = VideoPlayerControl.Position;
        var index = Subtitles.IndexOf(s);
        if (index < 0 || index >= Subtitles.Count)
        {
            return;
        }

        _undoRedoManager.StopChangeDetection();
        var videoStartTime = TimeSpan.FromSeconds(videoPositionSeconds);
        var subtitleStartTime = s.StartTime;
        var difference = videoStartTime - subtitleStartTime;

        for (var i = index; i < Subtitles.Count; i++)
        {
            var subtitle = Subtitles[i];
            subtitle.StartTime += difference;
        }

        _updateAudioVisualizer = true;
        _undoRedoManager.StartChangeDetection();
    }

    [RelayCommand]
    private void WaveformSetStart()
    {
        var s = SelectedSubtitle;
        if (s == null || VideoPlayerControl == null || LockTimeCodes)
        {
            return;
        }

        var videoPositionSeconds = VideoPlayerControl.Position;
        var gap = Se.Settings.General.MinimumMillisecondsBetweenLines / 1000.0;
        if (videoPositionSeconds >= s.EndTime.TotalSeconds - gap)
        {
            return;
        }

        s.SetStartTimeOnly(TimeSpan.FromSeconds(videoPositionSeconds));
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformSetEnd()
    {
        var s = SelectedSubtitle;
        if (s == null || VideoPlayerControl == null || LockTimeCodes)
        {
            return;
        }

        var videoPositionSeconds = VideoPlayerControl.Position;
        var gap = Se.Settings.General.MinimumMillisecondsBetweenLines / 1000.0;
        if (videoPositionSeconds < s.StartTime.TotalSeconds + gap)
        {
            return;
        }

        s.EndTime = TimeSpan.FromSeconds(videoPositionSeconds);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformSetEndAndStartOfNextAfterGap()
    {
        var s = SelectedSubtitle;
        if (s == null || AudioVisualizer?.WavePeaks == null || VideoPlayerControl == null || LockTimeCodes)
        {
            return;
        }

        var idx = Subtitles.IndexOf(s);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next == null)
        {
            return;
        }

        var videoPositionSeconds = VideoPlayerControl.Position;
        var gapMs = Se.Settings.General.MinimumMillisecondsBetweenLines;
        if (videoPositionSeconds < s.StartTime.TotalSeconds + 0.001)
        {
            return;
        }

        s.EndTime = TimeSpan.FromSeconds(videoPositionSeconds);
        var nextStart = s.EndTime.TotalMilliseconds + gapMs;
        next.StartTime = TimeSpan.FromMilliseconds(nextStart);

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformSetStartAndSetEndOfPreviousMinusGap()
    {
        var s = SelectedSubtitle;
        if (s == null || AudioVisualizer?.WavePeaks == null || VideoPlayerControl == null || LockTimeCodes)
        {
            return;
        }

        var idx = Subtitles.IndexOf(s);
        var previous = Subtitles.GetOrNull(idx - 1);
        if (previous == null)
        {
            return;
        }

        var videoPositionSeconds = VideoPlayerControl.Position;
        var gapMs = Se.Settings.General.MinimumMillisecondsBetweenLines;
        if (videoPositionSeconds > s.EndTime.TotalSeconds - 0.001)
        {
            return;
        }

        s.SetStartTimeOnly(TimeSpan.FromSeconds(videoPositionSeconds));
        var previousEnd = s.StartTime.TotalMilliseconds - gapMs;
        previous.EndTime = TimeSpan.FromMilliseconds(previousEnd);

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void FetchFirstWordForNextSubtitle()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(s);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next == null)
        {
            return;
        }

        var currentText = s.Text.Trim();
        var nextText = next.Text.Trim();

        var upDown = new MoveWordUpDown(currentText, nextText);
        upDown.MoveWordUp();

        s.Text = upDown.S1;
        next.Text = upDown.S2;

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void MoveLastWordToNextSubtitle()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(s);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next == null)
        {
            return;
        }

        var currentText = s.Text.Trim();
        var nextText = next.Text.Trim();

        var upDown = new MoveWordUpDown(currentText, nextText);
        upDown.MoveWordDown();

        s.Text = upDown.S1;
        next.Text = upDown.S2;

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void MoveLastWordFromFirstLineDownCurrentSubtitle()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        var lines = s.Text.SplitToLines();
        if (!string.IsNullOrWhiteSpace(s.Text) && lines.Count == 1)
        {
            lines.Add(string.Empty);
        }

        if (lines.Count != 2)
        {
            return;
        }

        var currentText = lines[0].Trim();
        var nextText = lines[1].Trim();

        var upDown = new MoveWordUpDown(currentText, nextText);
        upDown.MoveWordDown();

        s.Text = upDown.S1 + Environment.NewLine + upDown.S2;

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void MoveFirstWordFromNextLineUpCurrentSubtitle()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        var lines = s.Text.SplitToLines();
        if (!string.IsNullOrWhiteSpace(s.Text) && lines.Count == 1)
        {
            lines.Add(string.Empty);
        }

        if (lines.Count != 2)
        {
            return;
        }

        var currentText = lines[0].Trim();
        var nextText = lines[1].Trim();

        var upDown = new MoveWordUpDown(currentText, nextText);
        upDown.MoveWordUp();

        s.Text = upDown.S1 + Environment.NewLine + upDown.S2;

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ToggleFocusGridAndWaveform()
    {
        if (AudioVisualizer == null)
        {
            return;
        }

        if (SubtitleGrid.IsFocused)
        {
            AudioVisualizer.Focus();
        }
        else
        {
            SubtitleGrid.Focus();
        }
    }

    [RelayCommand]
    private void ToggleFocusTextBoxAndWaveform()
    {
        if (AudioVisualizer == null)
        {
            return;
        }

        if (EditTextBox.IsFocused)
        {
            AudioVisualizer.Focus();
        }
        else
        {
            EditTextBox.Focus();
        }
    }

    [RelayCommand]
    private void WaveformOneSecondBack()
    {
        MoveVideoPositionMs(-1000);
    }

    [RelayCommand]
    private void WaveformOneSecondForward()
    {
        MoveVideoPositionMs(1000);
    }

    [RelayCommand]
    private void ExtendSelectedToPrevious()
    {
        var s = SelectedSubtitle;
        var idx = SelectedSubtitleIndex;
        if (s == null || idx == null || idx == 0 || LockTimeCodes)
        {
            return;
        }

        var prev = Subtitles[idx.Value - 1];
        s.StartTime =
            TimeSpan.FromMilliseconds(prev.EndTime.TotalMilliseconds +
                                      Se.Settings.General.MinimumMillisecondsBetweenLines);
        _updateAudioVisualizer = true;
    }


    [RelayCommand]
    private void ExtendSelectedToNext()
    {
        var s = SelectedSubtitle;
        var idx = SelectedSubtitleIndex;
        if (s == null || idx == null || idx < Subtitles.Count - 1 || LockTimeCodes)
        {
            return;
        }

        var next = Subtitles.GetOrNull(idx.Value + 1);
        if (next == null)
        {
            return;
        }

        ;
        s.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds -
                                              Se.Settings.General.MinimumMillisecondsBetweenLines);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void TextBoxCut()
    {
        EditTextBox.Cut();
    }

    [RelayCommand]
    private void TextBoxCut2()
    {
        EditTextBox.Cut();
    }

    [RelayCommand]
    private void TextBoxCopy()
    {
        EditTextBox.Copy();
    }

    [RelayCommand]
    private void TextBoxPaste()
    {
        EditTextBox.Paste();
    }

    [RelayCommand]
    private void TextBoxSelectAll()
    {
        EditTextBox.SelectAll();
    }

    [RelayCommand]
    private void TextBoxDeleteSelection()
    {
        EditTextBox.SelectedText = string.Empty;
    }

    [RelayCommand]
    private async Task SubtitleGridCut()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0 || Window == null)
        {
            return;
        }

        await SubtitleGridCopyPasteHelper.Cut(Window, Subtitles, selectedItems, SelectedSubtitleFormat);
        Renumber();
        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task SubtitleGridCopy()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0 || Window == null)
        {
            return;
        }

        await SubtitleGridCopyPasteHelper.Copy(Window, selectedItems, SelectedSubtitleFormat);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task SubtitleGridPaste()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (idx < 0 || Window == null)
        {
            return;
        }

        await SubtitleGridCopyPasteHelper.Paste(Window, Subtitles, idx, SelectedSubtitleFormat);
        Renumber();
        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task SetNewStyleForSelectedLines(string styleName)
    {
        var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.General.Style + " - " + Se.Language.General.New, string.Empty, 250, 20, true); });

        if (result.OkPressed && !string.IsNullOrWhiteSpace(result.Text))
        {
            _subtitle ??= new Subtitle();

            var header = _subtitle?.Header ?? string.Empty;
            if (header != null && header.Contains("http://www.w3.org/ns/ttml"))
            {
                var s = new Subtitle { Header = header };
                AdvancedSubStationAlpha.LoadStylesFromTimedText10(s, string.Empty, header,
                    AdvancedSubStationAlpha.HeaderNoStyles, new StringBuilder());
                header = s.Header;
            }
            else if (header != null && header.StartsWith("WEBVTT", StringComparison.Ordinal))
            {
                _subtitle = WebVttToAssa.Convert(_subtitle, new SsaStyle(), 0, 0);
                header = _subtitle.Header;
            }

            var defaultHeader = GetDefaultAssaHeader();
            if (header == null || !header.Contains("style:", StringComparison.OrdinalIgnoreCase))
            {
                header = defaultHeader;
            }

            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(header);
            var newStyle = AdvancedSubStationAlpha.GetSsaStylesFromHeader(defaultHeader).First();

            newStyle.Name = result.Text.Trim();

            // ensure unique style name
            var idx = 1;
            while (styles.Any(s => s.Name.Equals(newStyle.Name, StringComparison.OrdinalIgnoreCase)))
            {
                idx++;
                newStyle.Name = $"{result.Text.Trim()}_{idx}";
            }

            styles.Add(newStyle);
            header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(header, styles);
            _subtitle!.Header = header;

            SetStyleForSelectedLines(newStyle.Name);
        }
    }

    private static string GetDefaultAssaHeader()
    {
        var format = new AdvancedSubStationAlpha();
        var sub = new Subtitle();
        var text = format.ToText(sub, string.Empty);
        var lines = text.SplitToLines();
        format.LoadSubtitle(sub, lines, string.Empty);
        return sub.Header;
    }

    [RelayCommand]
    private void SetStyleForSelectedLines(string styleName)
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var p in selectedItems)
            {
                p.Style = styleName;
            }
        });
    }

    [RelayCommand]
    private async Task SetNewActorForSelectedLines(string styleName)
    {
        var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.General.Actor + " - " + Se.Language.General.New, string.Empty, 250, 20, true); });

        if (result.OkPressed && !string.IsNullOrWhiteSpace(result.Text))
        {
            SetStyleForSelectedLines(result.Text);
        }
    }

    [RelayCommand]
    private void SetActorForSelectedLines(string actorName)
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var p in selectedItems)
            {
                p.Actor = actorName;
            }
        });
    }

    private void SplitSelectedLine(bool atVideoPosition, bool atTextBoxPosition)
    {
        var s = SelectedSubtitle;
        if (s == null || VideoPlayerControl == null || EditTextBox == null)
        {
            return;
        }

        if (atTextBoxPosition && atTextBoxPosition)
        {
            _splitManager.Split(Subtitles, s, VideoPlayerControl.Position, EditTextBox.SelectionStart);
        }
        else if (atVideoPosition)
        {
            _splitManager.Split(Subtitles, s, VideoPlayerControl.Position);
        }
        else if (atTextBoxPosition)
        {
            _splitManager.Split(Subtitles, s, EditTextBox.SelectionStart);
        }
        else
        {
            _splitManager.Split(Subtitles, s);
        }
    }

    private void MoveVideoPositionMs(int ms)
    {
        if (VideoPlayerControl == null || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var newPosition = VideoPlayerControl.Position + (ms / 1000.0);
        if (newPosition < 0)
        {
            newPosition = 0;
        }

        if (newPosition > VideoPlayerControl.Duration)
        {
            newPosition = VideoPlayerControl.Duration;
        }

        VideoPlayerControl.Position = newPosition;
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

    public void AutoFitColumns()
    {
        var columns = SubtitleGrid.Columns.Where(p => p.IsVisible).ToList();

        var numberOfStarColumns = 0;
        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];

            var originalWidth = column.Width;
            column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            SubtitleGrid.UpdateLayout();

            if (column.Header.ToString() == Se.Language.General.OriginalText ||
                column.Header.ToString() == Se.Language.General.Text)
            {
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                numberOfStarColumns++;
            }
            else
            {
                column.Width = originalWidth;
            }

            if (i == columns.Count - 1)
            {
                if (numberOfStarColumns == 0)
                {
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                }
                else if (numberOfStarColumns == 1 && column.Width.IsStar)
                {
                }
                else
                {
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }
            }
        }

        SubtitleGrid.UpdateLayout();
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
        });
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

    private bool ToggleTextBoxTag(TextBox tb, string htmlTag, string assaOn, string assaOff)
    {
        if (tb == null || tb.Text == null)
        {
            return false;
        }

        var selectionStart = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var selectionEnd = Math.Max(tb.SelectionStart, tb.SelectionEnd);
        var selectionLength = selectionEnd - selectionStart;

        var isAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        if (selectionLength == 0)
        {
            if (isAssa)
            {
                if (tb.Text.Contains("{\\" + assaOn + " }"))
                {
                    tb.Text = tb.Text.Replace("{\\" + assaOn + "}", string.Empty)
                        .Replace("{\\" + assaOff + "0}", string.Empty);
                }
                else
                {
                    tb.Text = "{\\" + assaOn + "}" + tb.Text + "{\\" + assaOff + "}";
                }
            }
            else
            {
                if (tb.Text.Contains("<" + htmlTag + ">"))
                {
                    tb.Text = HtmlUtil.RemoveOpenCloseTags(tb.Text, htmlTag);
                }
                else
                {
                    tb.Text = "<" + htmlTag + ">" + tb.Text + "</" + htmlTag + ">";
                }
            }
        }
        else
        {
            var selectedText = tb.Text.Substring(selectionStart, selectionLength);

            if (isAssa)
            {
                if (selectedText.Contains("{\\" + assaOn + "}"))
                {
                    selectedText = selectedText.Replace("{\\" + assaOn + "}", string.Empty)
                        .Replace("{\\" + assaOff + "}", string.Empty);
                }
                else
                {
                    selectedText = "{\\" + assaOn + "}" + selectedText + "{\\" + assaOff + "}";
                }
            }
            else
            {
                if (selectedText.Contains("<" + htmlTag + ">"))
                {
                    selectedText = HtmlUtil.RemoveOpenCloseTags(selectedText, htmlTag);
                }
                else
                {
                    selectedText = "<" + htmlTag + ">" + selectedText + "</" + htmlTag + ">";
                }
            }

            tb.Text = tb.Text
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, selectedText);

            Dispatcher.UIThread.Post(() =>
            {
                tb.Focus();
                tb.SelectionStart = selectionStart;
                tb.SelectionEnd = selectionStart + selectedText.Length;
            });
        }

        return true;
    }

    public async Task SubtitleOpen(
        string fileName,
        string? videoFileName = null,
        int? selectedSubtitleIndex = null,
        TextEncoding? textEncoding = null)
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
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        try
        {
            _opening = true;

            if (FileUtil.IsMatroskaFileFast(fileName) && FileUtil.IsMatroskaFile(fileName))
            {
                await ImportSubtitleFromMatroskaFile(fileName, videoFileName);
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
                        var result = await _windowService.ShowDialogAsync<OcrWindow, OcrViewModel>(Window!,
                            vm => { vm.Initialize(subtitles, fileName); });

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

            if ((ext == ".mp4" || ext == ".m4v" || ext == ".3gp" || ext == ".mov" || ext == ".cmaf") &&
                fileSize > 2000 || ext == ".m4s")
            {
                if (!new IsmtDfxp().IsMine(null, fileName))
                {
                    await ImportSubtitleFromMp4(fileName);
                    return;
                }
            }

            if ((ext == ".ts" || ext == ".tsv" || ext == ".tts" || ext == ".rec" || ext == ".mpeg" || ext == ".mpg") && fileSize > 10000 && FileUtil.IsTransportStream(fileName))
            {
                await ImportSubtitleFromTransportStream(fileName);
                return;
            }

            if (((ext == ".m2ts" || ext == ".ts" || ext == ".tsv" || ext == ".tts" || ext == ".mts") &&
                 fileSize > 10000 && FileUtil.IsM2TransportStream(fileName)) ||
                (ext == ".textst" && FileUtil.IsMpeg2PrivateStream2(fileName)))
            {
                bool isTextSt = false;
                if (fileSize < 2000000)
                {
                    var textSt = new TextST();
                    isTextSt = textSt.IsMine(null, fileName);
                }

                if (!isTextSt)
                {
                    await ImportSubtitleFromTransportStream(fileName);
                    return;
                }
            }

            if (FileUtil.IsVobSub(fileName) && ext == ".sub")
            {
                ImportSubtitleFromVobSubFile(fileName, videoFileName);
                return;
            }

            if (ext == ".ismt" || ext == ".mp4" || ext == ".m4v" || ext == ".mov" || ext == ".3gp" || ext == ".cmaf" ||
                ext == ".m4s")
            {
                var f = new IsmtDfxp();
                if (f.IsMine(null, fileName))
                {
                    f.LoadSubtitle(_subtitle, null, fileName);

                    if (_subtitle.OriginalFormat?.Name == new TimedTextBase64Image().Name)
                    {
                        ImportAndInlineBase64(_subtitle, fileName);
                        return;
                    }

                    if (_subtitle.OriginalFormat?.Name == new TimedTextImage().Name)
                    {
                        ImportAndOcrDost(fileName, _subtitle);
                        return;
                    }

                    ResetSubtitle();
                    _subtitleFileName = Utilities.GetPathAndFileNameWithoutExtension(fileName) +
                                        SelectedSubtitleFormat.Extension;
                    _subtitle.Renumber();
                    Subtitles.AddRange(_subtitle.Paragraphs.Select(p =>
                        new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
                    ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                    SelectAndScrollToRow(0);
                    LoadBookmarks();
                    _converted = true;
                    return;
                }
            }

            var subtitle = Subtitle.Parse(fileName, textEncoding?.Encoding);
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
                    var message = Se.Language.General.UnknownSubtitleFormat;
                    await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            SelectedSubtitleFormat = SubtitleFormats.FirstOrDefault(p => p.Name == subtitle.OriginalFormat.Name) ??
                                     SelectedSubtitleFormat;

            _subtitleFileName = fileName;
            _subtitle = subtitle;
            _lastOpenSaveFormat = subtitle.OriginalFormat;
            SetSubtitles(_subtitle);
            _changeSubtitleHash = GetFastHash();
            ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
            LoadBookmarks();

            if (selectedSubtitleIndex != null && selectedSubtitleIndex >= 0 && selectedSubtitleIndex < Subtitles.Count)
            {
                SelectAndScrollToRow(selectedSubtitleIndex.Value);
            }
            else if (Subtitles.Count > 0)
            {
                SelectAndScrollToRow(0);
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
        }
        finally
        {
            _undoRedoManager.Do(MakeUndoRedoObject(string.Format(Se.Language.General.SubtitleLoadedX, fileName)));
            _undoRedoManager.StartChangeDetection();
            _opening = false;
        }
    }

    private void LoadBookmarks()
    {
        var sub = GetUpdateSubtitle();
        new BookmarkPersistence(sub, _subtitleFileName).Load();
        for (var i = 0; i < Subtitles.Count && i < sub.Paragraphs.Count; i++)
        {
            Subtitles[i].Bookmark = sub.Paragraphs[i].Bookmark;
        }
    }

    private void ImportAndOcrDost(string fileName, Subtitle subtitle)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            var result = await _windowService.ShowDialogAsync<OcrWindow, OcrViewModel>(Window!,
                vm => { vm.InitializeBdn(subtitle, fileName, false); });

            if (result.OkPressed)
            {
                ResetSubtitle();
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                _converted = true;
                _subtitle.Paragraphs.Clear();
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                Renumber();
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);
            }
        });
    }

    private void ImportAndInlineBase64(Subtitle subtitle, string fileName)
    {
        IList<IBinaryParagraphWithPosition> list = [];
        foreach (var p in subtitle.Paragraphs)
        {
            var x = new TimedTextBase64Image.Base64PngImage()
            {
                Text = p.Text,
                StartTimeCode = p.StartTime,
                EndTimeCode = p.EndTime,
            };

            using var bitmap = x.GetBitmap();
            var nikseBmp = new NikseBitmap(bitmap);
            var nonTransparentHeight = nikseBmp.GetNonTransparentHeight();
            if (nonTransparentHeight > 1)
            {
                list.Add(x);
            }
        }

        if (list.Count == 0)
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                await MessageBox.Show(
                    Window!,
                    Se.Language.General.Error,
                    Se.Language.General.NoSubtitlesFound,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            });
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            var result =
                await _windowService.ShowDialogAsync<OcrWindow, OcrViewModel>(Window!,
                    vm => { vm.Initialize(list, fileName); });

            if (result.OkPressed)
            {
                ResetSubtitle();
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                _converted = true;
                _subtitle.Paragraphs.Clear();
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                Renumber();
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);
            }
        });
    }

    private void RemoveShotChange(int idx)
    {
        if (AudioVisualizer == null || AudioVisualizer.ShotChanges == null)
        {
            return;
        }

        if (idx >= 0 && idx < AudioVisualizer.ShotChanges.Count)
        {
            var temp = new List<double>(AudioVisualizer.ShotChanges);
            temp.RemoveAt(idx);
            AudioVisualizer.ShotChanges = temp;
            ShotChangesHelper.SaveShotChanges(_videoFileName, temp);
        }
    }

    private async Task ImportSubtitleFromTransportStream(string fileName)
    {
        //ShowStatus(_language.ParsingTransportStream);
        var tsParser = new TransportStreamParser();
        tsParser.Parse(fileName,
            (pos, total) =>
                UpdateProgress(pos, total,
                    string.Format("Parsing {0}", fileName))); // _language.ParsingTransportStreamFile););
        //ShowStatus(string.Empty);
        //TaskbarList.SetProgressState(Handle, TaskbarButtonProgressFlags.NoProgress);

        if (tsParser.SubtitlePacketIds.Count == 0 && tsParser.TeletextSubtitlesLookup.Count == 0)
        {
            await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.General.NoSubtitlesFound,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (tsParser.SubtitlePacketIds.Count == 0 && tsParser.TeletextSubtitlesLookup.Count == 1 &&
            tsParser.TeletextSubtitlesLookup.First().Value.Count == 1)
        {
            ResetSubtitle();
            _subtitle = new Subtitle(tsParser.TeletextSubtitlesLookup.First().Value.First().Value);
            _subtitle.Renumber();
            Subtitles.AddRange(_subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
            SelectAndScrollToRow(0);
            //if (!Configuration.Settings.General.DisableVideoAutoLoading)
            //{
            //    OpenVideo(fileName);
            //}

            _subtitleFileName = Path.GetFileNameWithoutExtension(fileName) + SelectedSubtitleFormat.Extension;
            _converted = true;
            return;
        }

        int packetId = 0;
        if (tsParser.SubtitlePacketIds.Count + tsParser.TeletextSubtitlesLookup.Sum(p => p.Value.Count) > 1)
        {
            //using (var subChooser = new TransportStreamSubtitleChooser())
            //{
            //    subChooser.Initialize(tsParser, fileName);
            //    if (subChooser.ShowDialog(this) == DialogResult.Cancel)
            //    {
            //        return false;
            //    }

            //    if (subChooser.IsTeletext)
            //    {
            //        new SubRip().LoadSubtitle(_subtitle, subChooser.Srt.SplitToLines(), null);
            //        _subtitle.Renumber();
            //        SubtitleListview1.Fill(_subtitle);
            //        SubtitleListview1.SelectIndexAndEnsureVisible(0);
            //        if (!Configuration.Settings.General.DisableVideoAutoLoading)
            //        {
            //            OpenVideo(fileName);
            //        }

            //        _fileName = Path.GetFileNameWithoutExtension(fileName) + GetCurrentSubtitleFormat().Extension;
            //        _converted = true;
            //        SetTitle();
            //        return true;
            //    }

            //    packetId = tsParser.SubtitlePacketIds[subChooser.SelectedIndex];
            //}
        }
        else
        {
            packetId = tsParser.SubtitlePacketIds[0];
        }


        var subtitles = tsParser.GetDvbSubtitles(packetId);
        Dispatcher.UIThread.Post(async () =>
        {
            var result = await _windowService.ShowDialogAsync<OcrWindow, OcrViewModel>(Window!, vm => { vm.Initialize(tsParser, subtitles, fileName); });

            if (result.OkPressed)
            {
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
            }
        });

        //using (var formSubOcr = new VobSubOcr())
        //{
        //    string language = null;
        //    var programMapTableParser = new ProgramMapTableParser();
        //    programMapTableParser.Parse(fileName); // get languages
        //    if (programMapTableParser.GetSubtitlePacketIds().Count > 0)
        //    {
        //        language = programMapTableParser.GetSubtitleLanguage(packetId);
        //    }

        //    formSubOcr.Initialize(subtitles, Configuration.Settings.VobSubOcr, fileName, language);
        //    if (formSubOcr.ShowDialog(this) == DialogResult.OK)
        //    {
        //        MakeHistoryForUndo(_language.BeforeImportingDvdSubtitle);

        //        _subtitle.Paragraphs.Clear();
        //        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
        //        foreach (var p in formSubOcr.SubtitleFromOcr.Paragraphs)
        //        {
        //            _subtitle.Paragraphs.Add(p);
        //        }

        //        UpdateSourceView();
        //        SubtitleListview1.Fill(_subtitle, _subtitleOriginal);
        //        _subtitleListViewIndex = -1;
        //        SubtitleListview1.FirstVisibleIndex = -1;
        //        SubtitleListview1.SelectIndexAndEnsureVisible(0, true);

        //        _fileName = string.Empty;
        //        if (!string.IsNullOrEmpty(formSubOcr.FileName))
        //        {
        //            var currentFormat = GetCurrentSubtitleFormat();
        //            _fileName = Utilities.GetPathAndFileNameWithoutExtension(formSubOcr.FileName) + currentFormat.Extension;
        //            if (!Configuration.Settings.General.DisableVideoAutoLoading)
        //            {
        //                OpenVideo(fileName);
        //            }

        //            _converted = true;
        //        }

        //        SetTitle();
        //        Configuration.Settings.Save();
        //        return true;
        //    }
        //}

        return;
    }

    private int _lastProgressPercent = -1;

    private void UpdateProgress(long position, long total, string statusMessage)
    {
        var percent = (int)Math.Round(position * 100.0 / total);
        if (percent == _lastProgressPercent)
        {
            return;
        }

        ShowStatus(string.Format("{0}, {1:0}%", statusMessage, _lastProgressPercent));
        _lastProgressPercent = percent;
    }

    private async Task ImportSubtitleFromMp4(string fileName)
    {
        var mp4Parser = new MP4Parser(fileName);
        var mp4SubtitleTracks = mp4Parser.GetSubtitleTracks();
        if (mp4SubtitleTracks.Count == 0)
        {
            if (mp4Parser.VttcSubtitle?.Paragraphs.Count > 0)
            {
                ResetSubtitle();
                SelectedSubtitleFormat = SubtitleFormats.FirstOrDefault(p => p.Name == new WebVTT().Name) ??
                                         SelectedSubtitleFormat;
                _subtitle = mp4Parser.VttcSubtitle;
                _subtitle.Renumber();
                _subtitleFileName = Utilities.GetPathAndFileNameWithoutExtension(fileName) +
                                    SelectedSubtitleFormat.Extension;
                Subtitles.AddRange(
                    _subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);
                return;
            }

            if (mp4Parser.TrunCea608Subtitle?.Paragraphs.Count > 0)
            {
                ResetSubtitle();
                _subtitle = mp4Parser.TrunCea608Subtitle;
                _subtitle.Renumber();
                _subtitleFileName = Utilities.GetPathAndFileNameWithoutExtension(fileName) +
                                    SelectedSubtitleFormat.Extension;
                Subtitles.AddRange(
                    _subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);
                return;
            }

            await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.General.NoSubtitlesFound,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else if (mp4SubtitleTracks.Count == 1)
        {
            LoadMp4Subtitle(fileName, mp4SubtitleTracks[0]);
        }
        else
        {
            var result =
                await _windowService.ShowDialogAsync<PickMp4TrackWindow, PickMp4TrackViewModel>(Window!,
                    vm => { vm.Initialize(mp4SubtitleTracks, fileName); });

            if (result.OkPressed && result.SelectedTrack != null && result.SelectedTrack.Track != null)
            {
                LoadMp4Subtitle(fileName, result.SelectedTrack.Track);
            }
        }
    }

    private void LoadMp4Subtitle(string fileName, Trak mp4SubtitleTrack)
    {
        if (mp4SubtitleTrack.Mdia.IsVobSubSubtitle)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var paragraphs = mp4SubtitleTrack.Mdia.Minf.Stbl.GetParagraphs();
                var result = await _windowService.ShowDialogAsync<OcrWindow, OcrViewModel>(Window!,
                    vm => { vm.Initialize(mp4SubtitleTrack, paragraphs, fileName); });

                if (result.OkPressed)
                {
                    ResetSubtitle();
                    _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                    Subtitles.Clear();
                    Subtitles.AddRange(result.OcredSubtitle);
                    Renumber();
                    ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                    SelectAndScrollToRow(0);
                }
            });
        }
        else
        {
            ResetSubtitle();
            _subtitle.Paragraphs.Clear();
            _subtitle.Paragraphs.AddRange(mp4SubtitleTrack.Mdia.Minf.Stbl.GetParagraphs());
            Subtitles.Clear();
            Subtitles.AddRange(_subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
            Renumber();
            ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
            SelectAndScrollToRow(0);
        }
    }

    private async Task ImportSubtitleFromMatroskaFile(string fileName, string? videoFileName)
    {
        var matroska = new MatroskaFile(fileName);
        var subtitleList = matroska.GetTracks(true);
        if (subtitleList.Count == 0)
        {
            matroska.Dispose();
            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    var answer = await MessageBox.Show(
                        Window!,
                        "No subtitle found",
                        "The Matroska file does not seem to contain any subtitles.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (Exception e)
                {
                    Se.LogError(e);
                }
            });

            matroska.Dispose();
            return;
        }

        if (subtitleList.Count > 1)
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                var result =
                    await _windowService.ShowDialogAsync<PickMatroskaTrackWindow, PickMatroskaTrackViewModel>(Window!,
                        vm => { vm.Initialize(matroska, subtitleList, fileName); });
                if (result.OkPressed && result.SelectedMatroskaTrack != null)
                {
                    if (await LoadMatroskaSubtitle(result.SelectedMatroskaTrack, matroska, fileName))
                    {
                        _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);

                        if (Se.Settings.General.AutoOpenVideo)
                        {
                            if (fileName.EndsWith("mkv", StringComparison.OrdinalIgnoreCase))
                            {
                                await VideoOpenFile(fileName);
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
            if (await LoadMatroskaSubtitle(subtitleList[0], matroska, fileName))
            {
                if (!Configuration.Settings.General.DisableVideoAutoLoading)
                {
                    if (ext == ".mkv")
                    {
                        Dispatcher.UIThread.Post(async void () =>
                        {
                            try
                            {
                                await VideoOpenFile(matroska.Path);
                                matroska.Dispose();
                            }
                            catch (Exception e)
                            {
                                Se.LogError(e);
                            }
                        });
                    }
                    else
                    {
                        if (FindVideoFileName.TryFindVideoFileName(matroska.Path, out videoFileName))
                        {
                            Dispatcher.UIThread.Post(async void () =>
                            {
                                try
                                {
                                    await VideoOpenFile(videoFileName);
                                    matroska.Dispose();
                                }
                                catch (Exception e)
                                {
                                    Se.LogError(e);
                                }
                            });
                        }
                        else
                        {
                            matroska.Dispose();
                        }
                    }
                }
            }
            else
            {
                matroska.Dispose();
            }
        }
    }

    private async Task<bool> LoadMatroskaSubtitle(
        MatroskaTrackInfo matroskaSubtitleInfo,
        MatroskaFile matroska,
        string fileName)
    {
        if (matroskaSubtitleInfo.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
        {
            var pgsSubtitle =
                _bluRayHelper.LoadBluRaySubFromMatroska(matroskaSubtitleInfo, matroska, out var errorMessage);

            Dispatcher.UIThread.Post(async () =>
            {
                var result = await _windowService.ShowDialogAsync<OcrWindow, OcrViewModel>(Window!,
                    vm => { vm.Initialize(matroskaSubtitleInfo, pgsSubtitle, fileName); });

                if (result.OkPressed)
                {
                    _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                    Subtitles.Clear();
                    Subtitles.AddRange(result.OcredSubtitle);
                    Renumber();
                }
            });

            return true;
        }

        if (matroskaSubtitleInfo.CodecId.Equals("S_HDMV/TEXTST", StringComparison.OrdinalIgnoreCase))
        {
            return LoadTextSTFromMatroska(matroskaSubtitleInfo, matroska);
        }

        if (matroskaSubtitleInfo.CodecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase))
        {
            return LoadDvbFromMatroska(matroskaSubtitleInfo, matroska, fileName);
        }

        if (matroskaSubtitleInfo.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
        {
            return await LoadVobSubFromMatroska(matroskaSubtitleInfo, matroska, fileName);
        }

        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, null);
        var subtitle = new Subtitle();
        var format = Utilities.LoadMatroskaTextSubtitle(matroskaSubtitleInfo, matroska, sub, subtitle);
        ResetSubtitle(format);
        _subtitle = subtitle;
        _subtitle.Renumber();
        Subtitles.Clear();
        Subtitles.AddRange(_subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
        _converted = true;

        return true;
    }

    private bool LoadDvbFromMatroska(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska, string fileName)
    {
        ShowStatus(Se.Language.Main.ParsingMatroskaFile);
        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, MatroskaProgress);

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
                if (data != null && data.Length > 9 && data[0] == 15 &&
                    data[1] >= SubtitleSegment.PageCompositionSegment &&
                    data[1] <= SubtitleSegment.DisplayDefinitionSegment) // sync byte + segment id
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
                        if (last.DurationTotalMilliseconds >
                            Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
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
                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds -
                                              Se.Settings.General.MinimumMillisecondsBetweenLines;
            }
        }

        Dispatcher.UIThread.Post(async () =>
        {
            var result = await _windowService.ShowDialogAsync<OcrWindow, OcrViewModel>(Window!,
                vm => { vm.Initialize(matroskaSubtitleInfo, subtitle, subtitleImages, fileName); });

            if (result.OkPressed)
            {
                ResetSubtitle();
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                Renumber();
                SelectAndScrollToRow(0);
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
            }
        });

        return true;
    }

    private async Task<bool> LoadVobSubFromMatroska(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska,
        string fileName)
    {
        if (matroskaSubtitleInfo.ContentEncodingType == 1)
        {
            var message = "Encrypted VobSub subtitles are not supported.";
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, MatroskaProgress);
        _subtitle.Paragraphs.Clear();

        List<VobSubMergedPack> mergedVobSubPacks = [];
        var idx = new Core.VobSub.Idx(matroskaSubtitleInfo.GetCodecPrivate().SplitToLines());
        foreach (var p in sub)
        {
            mergedVobSubPacks.Add(new VobSubMergedPack(p.GetData(matroskaSubtitleInfo),
                TimeSpan.FromMilliseconds(p.Start), 32, null));
            if (mergedVobSubPacks.Count > 0)
            {
                mergedVobSubPacks[mergedVobSubPacks.Count - 1].EndTime = TimeSpan.FromMilliseconds(p.End);
            }

            // fix overlapping (some versions of Handbrake makes overlapping time codes - thx Hawke)
            if (mergedVobSubPacks.Count > 1 && mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime >
                mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime)
            {
                mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime =
                    TimeSpan.FromMilliseconds(
                        mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime.TotalMilliseconds - 1);
            }
        }

        // Remove bad packs
        for (int i = mergedVobSubPacks.Count - 1; i >= 0; i--)
        {
            if (mergedVobSubPacks[i].SubPicture.SubPictureDateSize <= 2)
            {
                mergedVobSubPacks.RemoveAt(i);
            }
            else if (mergedVobSubPacks[i].SubPicture.SubPictureDateSize <= 67 &&
                     mergedVobSubPacks[i].SubPicture.Delay.TotalMilliseconds < 35)
            {
                mergedVobSubPacks.RemoveAt(i);
            }
        }

        Dispatcher.UIThread.Post(async () =>
        {
            var result = await _windowService.ShowDialogAsync<OcrWindow, OcrViewModel>(Window!,
                vm => { vm.Initialize(mergedVobSubPacks, matroskaSubtitleInfo, fileName); });

            if (result.OkPressed)
            {
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
            }
        });

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

        _subtitle.Paragraphs.Clear();
        Utilities.LoadMatroskaTextSubtitle(matroskaSubtitleInfo, matroska, sub, _subtitle);
        Utilities.ParseMatroskaTextSt(matroskaSubtitleInfo, sub, _subtitle);

        SelectedSubtitleFormat =
            SubtitleFormats.FirstOrDefault(p => p.Name == Configuration.Settings.General.DefaultSubtitleFormat) ??
            SelectedSubtitleFormat;
        ShowStatus(Se.Language.Main.SubtitleImportedFromMatroskaFile);
        _subtitle.Renumber();
        Subtitles.Clear();
        Subtitles.AddRange(_subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));


        if (matroska.Path.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
            matroska.Path.EndsWith(".mks", StringComparison.OrdinalIgnoreCase))
        {
            _subtitleFileName = matroska.Path.Remove(matroska.Path.Length - 4) + SelectedSubtitleFormat.Extension;
        }

        SelectAndScrollToRow(0);

        return true;
    }

    private bool ImportSubtitleFromVobSubFile(string vobSubFileName, string? videoFileName)
    {
        var vobSubParser = new VobSubParser(true);
        string idxFileName = Path.ChangeExtension(vobSubFileName, ".idx");
        vobSubParser.OpenSubIdx(vobSubFileName, idxFileName);
        var vobSubMergedPackList = vobSubParser.MergeVobSubPacks();
        var palette = vobSubParser.IdxPalette;
        vobSubParser.VobSubPacks.Clear();

        var languageStreamIds = new List<int>();
        var streamIdDictionary = new Dictionary<int, List<VobSubMergedPack>>();
        foreach (var pack in vobSubMergedPackList)
        {
            if (pack.SubPicture.Delay.TotalMilliseconds > 500 && !languageStreamIds.Contains(pack.StreamId))
            {
                languageStreamIds.Add(pack.StreamId);
            }

            if (!streamIdDictionary.TryGetValue(pack.StreamId, out List<VobSubMergedPack>? value))
            {
                streamIdDictionary.Add(pack.StreamId, new List<VobSubMergedPack>([pack]));
            }
            else
            {
                value.Add(pack);
            }
        }

        if (languageStreamIds.Count == 0)
        {
            return false;
        }

        if (languageStreamIds.Count > 1)
        {
            //using (var chooseLanguage = new DvdSubRipChooseLanguage())
            //{
            //    if (ShowInTaskbar)
            //    {
            //        chooseLanguage.Icon = (Icon)Icon.Clone();
            //        chooseLanguage.ShowInTaskbar = true;
            //        chooseLanguage.ShowIcon = true;
            //    }

            //    chooseLanguage.Initialize(_vobSubMergedPackList, _palette, vobSubParser.IdxLanguages, string.Empty);
            //    var form = _main ?? (Form)this;
            //    if (batchMode)
            //    {
            //        chooseLanguage.SelectActive();
            //        vobSubMergedPackList = chooseLanguage.SelectedVobSubMergedPacks;
            //        SetTesseractLanguageFromLanguageString(chooseLanguage.SelectedLanguageString);
            //        _importLanguageString = chooseLanguage.SelectedLanguageString;
            //        return true;
            //    }

            //    chooseLanguage.Activate();
            //    if (chooseLanguage.ShowDialog(form) == DialogResult.OK)
            //    {
            //        _vobSubMergedPackList = chooseLanguage.SelectedVobSubMergedPacks;
            //        SetTesseractLanguageFromLanguageString(chooseLanguage.SelectedLanguageString);
            //        _importLanguageString = chooseLanguage.SelectedLanguageString;
            //        return true;
            //    }

            //    return false;
            //}
        }

        var streamId = languageStreamIds.First();
        Dispatcher.UIThread.Post(async () =>
        {
            var result = await _windowService.ShowDialogAsync<OcrWindow, OcrViewModel>(Window!,
                vm => { vm.Initialize(streamIdDictionary[streamId], palette, vobSubFileName); });
        });

        return false;
    }

    private void SetSubtitles(Subtitle subtitle)
    {
        Subtitles.Clear();
        foreach (var p in subtitle.Paragraphs)
        {
            Subtitles.Add(new SubtitleLineViewModel(p, SelectedSubtitleFormat));
        }

        Renumber();
        UpdateGaps();
    }

    public bool HasChanges()
    {
        var hasChanges = !IsEmpty && _changeSubtitleHash != GetFastHash();
        if (!hasChanges && ShowColumnOriginalText)
        {
            hasChanges = _changeSubtitleHashOriginal != GetFastHashOriginal();
        }

        return hasChanges;
    }


    /// <returns>True, if continue. False, if the use aborts the current action (keep current unchanged work)</returns>
    private async Task<bool> HasChangesContinue()
    {
        var currentSubtitleHash = GetFastHash();
        if (_changeSubtitleHash != currentSubtitleHash && !IsEmpty)
        {
            string promptText = string.Format(Se.Language.General.SaveChangesToX, Se.Language.General.Untitled);
            if (!string.IsNullOrEmpty(_subtitleFileName))
            {
                promptText = string.Format(Se.Language.General.SaveChangesToX, _subtitleFileName);
            }

            var dr = await MessageBox.Show(Window!, Se.Language.General.SaveChangesTitle, promptText,
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dr == MessageBoxResult.Cancel)
            {
                return false;
            }

            if (dr == MessageBoxResult.No)
            {
                return true;
            }

            if (string.IsNullOrEmpty(_subtitleFileName))
            {
                var saved = await SaveSubtitleAs();
                if (!saved)
                {
                    return false;
                }
            }
            else
            {
                await SaveSubtitle();
            }
        }

        return await ContinueNewOrExitOriginal();
    }

    private async Task<bool> ContinueNewOrExitOriginal()
    {
        if (!ShowColumnOriginalText)
        {
            return true;
        }

        var currentSubtitleHash = GetFastHash();
        if (_changeSubtitleHashOriginal != currentSubtitleHash && !IsEmptyOriginal)
        {
            string promptText = string.Format(Se.Language.General.SaveChangesToXOriginal, Se.Language.General.Untitled);
            if (!string.IsNullOrEmpty(_subtitleFileNameOriginal))
            {
                promptText = string.Format(Se.Language.General.SaveChangesToXOriginal, _subtitleFileNameOriginal);
            }

            var dr = await MessageBox.Show(Window!, Se.Language.General.SaveChangesTitle, promptText,
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dr == MessageBoxResult.Cancel)
            {
                return false;
            }

            if (dr == MessageBoxResult.No)
            {
                return true;
            }

            if (string.IsNullOrEmpty(_subtitleFileNameOriginal))
            {
                var saved = await SaveSubtitleOriginalAs();
                if (!saved)
                {
                    return false;
                }
            }
            else
            {
                await SaveSubtitleOriginal();
            }
        }

        return true;
    }

    private async Task<bool> SaveSubtitle()
    {
        if (Subtitles == null || !Subtitles.Any())
        {
            ShowStatus("Nothing to save");
            return false;
        }

        if (string.IsNullOrEmpty(_subtitleFileName) || _converted)
        {
            var result = await SaveSubtitleAs();
            return result;
        }

        if (_lastOpenSaveFormat == null || _lastOpenSaveFormat.Name != SelectedSubtitleFormat.Name)
        {
            var result = await SaveSubtitleAs();
            return result;
        }

        var text = GetUpdateSubtitle().ToText(SelectedSubtitleFormat);

        try
        {
            await File.WriteAllTextAsync(_subtitleFileName, text);
        }
        catch (Exception ex)
        {
            var message = string.Format(Se.Language.General.CouldNotSaveFileXErrorY, _subtitleFileName, ex.Message);
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        _changeSubtitleHash = GetFastHash();
        _lastOpenSaveFormat = SelectedSubtitleFormat;

        return true;
    }

    private async Task SaveSubtitleOriginal()
    {
        if (Subtitles == null || !Subtitles.Any())
        {
            ShowStatus("Nothing to save (original)");
            return;
        }

        if (string.IsNullOrEmpty(_subtitleFileNameOriginal) || _converted)
        {
            await SaveSubtitleOriginalAs();
            return;
        }

        if (_lastOpenSaveFormat == null || _lastOpenSaveFormat.Name != SelectedSubtitleFormat.Name)
        {
            await SaveSubtitleOriginalAs();
            return;
        }

        var text = GetUpdateSubtitleOriginal().ToText(SelectedSubtitleFormat);
        await File.WriteAllTextAsync(_subtitleFileNameOriginal, text);
        _changeSubtitleHashOriginal = GetFastHashOriginal();
        _lastOpenSaveFormat = SelectedSubtitleFormat;
    }

    public Subtitle GetUpdateSubtitle()
    {
        _subtitle.Paragraphs.Clear();
        foreach (var line in Subtitles)
        {
            _subtitle.Paragraphs.Add(line.ToParagraph(SelectedSubtitleFormat));
        }

        return _subtitle;
    }

    public Subtitle GetUpdateSubtitleOriginal()
    {
        _subtitleOriginal ??= new Subtitle();

        _subtitleOriginal.OriginalFormat ??= SelectedSubtitleFormat;

        _subtitleOriginal.Paragraphs.Clear();
        foreach (var line in Subtitles)
        {
            _subtitleOriginal.Paragraphs.Add(line.ToParagraphOriginal(SelectedSubtitleFormat));
        }

        return _subtitleOriginal;
    }

    private async Task<bool> SaveSubtitleAs()
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

        var title = Se.Language.General.SaveFileAsTitle;
        if (ShowColumnOriginalText)
        {
            title = Se.Language.General.SaveTranslationAsTitle;
        }

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            SelectedSubtitleFormat,
            newFileName,
            title);

        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        _subtitleFileName = fileName;
        _subtitle.FileName = fileName;
        _lastOpenSaveFormat = SelectedSubtitleFormat;
        var result = await SaveSubtitle();
        AddToRecentFiles(true);
        return result;
    }

    private async Task<bool> SaveSubtitleOriginalAs()
    {
        var newFileName = "New" + SelectedSubtitleFormat.Extension;
        if (!string.IsNullOrEmpty(_subtitleFileNameOriginal))
        {
            newFileName = Path.GetFileNameWithoutExtension(_subtitleFileNameOriginal) +
                          SelectedSubtitleFormat.Extension;
        }
        else if (!string.IsNullOrEmpty(_videoFileName))
        {
            newFileName = Path.GetFileNameWithoutExtension(_videoFileName) + SelectedSubtitleFormat.Extension;
        }

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            SelectedSubtitleFormat,
            newFileName,
            Se.Language.General.SaveOriginalAsTitle);

        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        _subtitleFileNameOriginal = fileName;

        _subtitleOriginal ??= new Subtitle();
        _subtitleOriginal.FileName = fileName;

        _lastOpenSaveFormat = SelectedSubtitleFormat;
        await SaveSubtitleOriginal();
        AddToRecentFiles(true);
        return true;
    }

    private void AddToRecentFiles(bool updateMenu)
    {
        if (_loading)
        {
            return;
        }

        var idx = SelectedSubtitleIndex ?? 0;
        Se.Settings.File.AddToRecentFiles(_subtitleFileName ?? string.Empty, _subtitleFileNameOriginal ?? string.Empty,
            _videoFileName ?? string.Empty, idx, SelectedEncoding.DisplayName);
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
        catch
        {
            // Ignore
        }
    }

    internal async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _videoOpenTokenSource?.Cancel();
        AddToRecentFiles(false);

        if (Window != null)
        {
            Se.Settings.General.ShowColumnEndTime = ShowColumnEndTime;
            Se.Settings.General.ShowColumnDuration = ShowColumnDuration;
            Se.Settings.General.ShowColumnGap = ShowColumnGap;
            Se.Settings.General.ShowColumnActor = ShowColumnActor;
            Se.Settings.General.ShowColumnCps = ShowColumnCps;
            Se.Settings.General.ShowColumnWpm = ShowColumnWpm;
            Se.Settings.General.ShowColumnLayer = ShowColumnLayer;
            Se.Settings.General.SelectCurrentSubtitleWhilePlaying = SelectCurrentSubtitleWhilePlaying;
            Se.Settings.Waveform.ShowToolbar = IsWaveformToolbarVisible;
            Se.Settings.Waveform.CenterVideoPosition = WaveformCenter;

            UiUtil.SaveWindowPosition(Window);
            Se.Settings.General.UndockVideoControls = Se.Settings.General.RememberPositionAndSize && AreVideoControlsUndocked;

            if (_findViewModel != null)
            {
                UiUtil.SaveWindowPosition(_findViewModel.Window);
            }

            if (_videoPlayerUndockedViewModel != null)
            {
                UiUtil.SaveWindowPosition(_videoPlayerUndockedViewModel.Window);
            }

            if (_audioVisualizerUndockedViewModel != null)
            {
                UiUtil.SaveWindowPosition(_audioVisualizerUndockedViewModel.Window);
            }
        }

        Se.SaveSettings();

        if (HasChanges() && Window != null)
        {
            // Cancel the closing to show the dialog
            e.Cancel = true;

            var result = await MessageBox.Show(
                Window,
                Se.Language.General.SaveChangesTitle,
                Se.Language.General.SaveChangesMessage,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == MessageBoxResult.Cancel)
            {
                // Stay cancelled - window won't close
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                await SaveSubtitle();
            }

            CleanUp();

            Window.Closing -= OnClosing;
            Window.Close();
        }
        else
        {
            CleanUp();
        }
    }

    private void CleanUp()
    {
        if (_findViewModel != null)
        {
            _findViewModel.Window?.Close();
        }

        if (_videoPlayerUndockedViewModel != null)
        {
            _videoPlayerUndockedViewModel.AllowClose = true;
            _videoPlayerUndockedViewModel.Window?.Close();
        }

        if (_audioVisualizerUndockedViewModel != null)
        {
            _audioVisualizerUndockedViewModel.AllowClose = true;
            _audioVisualizerUndockedViewModel.Window?.Close();
        }

        VideoPlayerControl?.VideoPlayerInstance.Close();
    }

    internal void OnLoaded()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && string.IsNullOrEmpty(Se.Settings.General.LibMpvPath))
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                try
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

                    var result =
                        await _windowService.ShowDialogAsync<DownloadLibMpvWindow, DownloadLibMpvViewModel>(Window!);
                }
                catch (Exception e)
                {
                    Se.LogError(e);
                }
            }, DispatcherPriority.Background);
        }

        var subtitleFileLoaded = false;
        var arguments = Environment.GetCommandLineArgs();
        if (arguments.Length > 1)
        {
            var sb = new StringBuilder();
            foreach (var arg in arguments)
            {
                sb.AppendLine(arg);
            }

            Se.LogError("OnLoaded Environment.GetCommandLineArgs: " + sb);

            var fileName = arguments[1];
            if (File.Exists(fileName))
            {
                subtitleFileLoaded = true;
                Dispatcher.UIThread.Post(async void () =>
                {
                    try
                    {
                        await SubtitleOpen(fileName);
                    }
                    catch (Exception e)
                    {
                        Se.LogError(e);
                    }
                });
            }
        }

        if (AudioVisualizer != null)
        {
            AudioVisualizer.IsReadOnly = LockTimeCodes;
        }

        if (!subtitleFileLoaded && Se.Settings.File.ShowRecentFiles)
        {
            var first = Se.Settings.File.RecentFiles.FirstOrDefault();
            if (first != null && File.Exists(first.SubtitleFileName))
            {
                Dispatcher.UIThread.Post(async void () =>
                {
                    try
                    {
                        await SubtitleOpen(first.SubtitleFileName, first.VideoFileName, first.SelectedLine);
                    }
                    catch (Exception e)
                    {
                        Se.LogError(e);
                    }
                });
            }
            else
            {
                Se.Settings.File.RecentFiles =
                    Se.Settings.File.RecentFiles.Where(p => File.Exists(p.SubtitleFileName)).ToList();
            }
        }

        if (Se.Settings.Appearance.RightToLeft)
        {
            RightToLeftToggle();
        }

        UiUtil.RestoreWindowPosition(Window);
        if (Se.Settings.General.UndockVideoControls)
        {
            VideoUndockControls();
        }

        Task.Run(async () =>
        {
            await Task.Delay(1000); // delay 1 second (off UI thread)
            _undoRedoManager.StartChangeDetection();
            _loading = false;
        });
    }

    private bool IsPositionOnAnyScreen(PixelRect windowBounds)
    {
        if (Window?.Screens?.All == null)
        {
            return false;
        }

        foreach (var screen in Window.Screens.All)
        {
            var screenBounds = screen.WorkingArea;
            if (screenBounds.Intersects(windowBounds))
            {
                // Ensure at least 100px of the title bar is visible for dragging
                var titleBarArea = new PixelRect(
                    windowBounds.X,
                    windowBounds.Y,
                    windowBounds.Width,
                    30); // Approximate title bar height

                if (screenBounds.Intersects(titleBarArea))
                    return true;
            }
        }

        return false;
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

    private async Task VideoOpenFile(string videoFileName) // OpenVideoFile
    {
        if (VideoPlayerControl == null)
        {
            return;
        }

        _videoOpenTokenSource?.Cancel();
        await VideoPlayerControl.Open(videoFileName);
        _mpvReloader.Reset();

        if (IsValidUrl(videoFileName))
        {
            //TODO: create empty waveform
            return;
        }

        var peakWaveFileName = WavePeakGenerator2.GetPeakWaveFileName(videoFileName);
        var spectrogramFolder = WavePeakGenerator2.SpectrogramDrawer.GetSpectrogramFolder(videoFileName, 0);
        if (!File.Exists(peakWaveFileName))
        {
            if (FfmpegHelper.IsFfmpegInstalled())
            {
                var tempWaveFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
                var process = WaveFileExtractor.GetCommandLineProcess(videoFileName, -1, tempWaveFileName,
                    Configuration.Settings.General.VlcWaveTranscodeSettings, out _);
                ShowStatus(Se.Language.Main.ExtractingWaveInfo);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await ExtractWaveformAndSpectrogramAndShotChanges(process, tempWaveFileName, peakWaveFileName, videoFileName); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
        else
        {
            ShowStatus(Se.Language.Main.LoadingWaveInfoFromCache);
            var wavePeaks = WavePeakData2.FromDisk(peakWaveFileName);
            if (AudioVisualizer != null)
            {
                AudioVisualizer.WavePeaks = wavePeaks;
                AudioVisualizer.SetSpectrogram(SpectrogramData2.FromDisk(spectrogramFolder));
                AudioVisualizer.ShotChanges = ShotChangesHelper.FromDisk(videoFileName);
                if (AudioVisualizer.ShotChanges.Count == 0)
                {
                    ExtractShotChanges(videoFileName);
                }
            }
        }

        _videoFileName = videoFileName;
        IsVideoLoaded = true;
    }

    private async Task ExtractWaveformAndSpectrogramAndShotChanges(
        Process process,
        string tempWaveFileName,
        string peakWaveFileName,
        string videoFileName)
    {
        process.Start();

        _videoOpenTokenSource = new CancellationTokenSource();
        while (!process.HasExited)
        {
            await Task.Delay(100, _videoOpenTokenSource.Token);
        }

        if (process.ExitCode != 0)
        {
            ShowStatus(Se.Language.Main.FailedToExtractWaveInfo);
            return;
        }

        if (_videoOpenTokenSource.IsCancellationRequested)
        {
            DeleteTempFile(tempWaveFileName);
            return;
        }

        if (File.Exists(tempWaveFileName))
        {
            using var waveFile = new WavePeakGenerator2(tempWaveFileName);
            waveFile.GeneratePeaks(0, peakWaveFileName);

            var wavePeaks = WavePeakData2.FromDisk(peakWaveFileName);

            if (_videoOpenTokenSource.IsCancellationRequested)
            {
                DeleteTempFile(tempWaveFileName);
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                if (AudioVisualizer != null)
                {
                    AudioVisualizer.WavePeaks = wavePeaks;
                }

                _updateAudioVisualizer = true;
            }, DispatcherPriority.Background);
        }

        ExtractShotChanges(videoFileName);
    }

    private void ExtractShotChanges(string videoFileName)
    {
        if (Se.Settings.Waveform.ShotChangesAutoGenerate)
        {
            var threshold = Se.Settings.Waveform.ShotChangesSensitivity.ToString(CultureInfo.InvariantCulture);
            var argumentsFormat = Se.Settings.Video.ShowChangesFFmpegArguments;
            var arguments = string.Format(argumentsFormat, videoFileName, threshold);

            var ffmpegProcess = FfmpegGenerator.GetProcess(arguments, OutputHandler);
#pragma warning disable CA1416 // Validate platform compatibility
            ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.BeginErrorReadLine();

            _ = Task.Run(() =>
            {
                while (!ffmpegProcess.HasExited)
                {
                    if (_videoOpenTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            ffmpegProcess.Kill(true);
                        }
                        catch
                        {
                            // ignore
                        }

                        break;
                    }

                    Task.Delay(100).Wait();
                }

                if (!_videoOpenTokenSource.IsCancellationRequested && AudioVisualizer != null && AudioVisualizer.ShotChanges != null)
                {
                    ShotChangesHelper.SaveShotChanges(videoFileName, AudioVisualizer.ShotChanges);
                }
            });
        }
    }

    private Lock _addShotChangeLock = new Lock();

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        var match = ShotChangesViewModel.TimeRegex.Match(outLine.Data);
        if (match.Success)
        {
            var timeCode = match.Value.Replace("pts_time:", string.Empty).Replace(",", ".").Replace("٫", ".").Replace("⠨", ".");
            if (double.TryParse(timeCode, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds) && seconds > 0.2)
            {
                lock (_addShotChangeLock)
                {
                    AudioVisualizer?.ShotChanges.Add(seconds);
                }
            }
        }
    }

    private static void DeleteTempFile(string tempWaveFileName)
    {
        try
        {
            if (File.Exists(tempWaveFileName))
            {
                File.Delete(tempWaveFileName);
            }
        }
        catch
        {
            // ignore
        }
    }

    private void VideoCloseFile()
    {
        _videoOpenTokenSource?.Cancel();
        VideoPlayerControl?.Close();
        _videoFileName = string.Empty;
        IsVideoLoaded = false;

        if (AudioVisualizer != null)
        {
            AudioVisualizer.WavePeaks = null;
            AudioVisualizer.ShotChanges = new List<double>();
        }
    }

    public bool IsTyping()
    {
        if (_lastKeyPressedTicks == 0)
        {
            return false;
        }

        var ticks = DateTime.UtcNow.Ticks;
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

    public int GetFastHashOriginal()
    {
        _subtitleOriginal ??= new Subtitle();

        var pre = _subtitleFileNameOriginal + SelectedEncoding.DisplayName;
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;
            hash = hash * 23 + pre.GetHashCode();

            if (_subtitleOriginal.Header != null)
            {
                hash = hash * 23 + _subtitleOriginal.Header.Trim().GetHashCode();
            }

            if (_subtitleOriginal.Footer != null)
            {
                hash = hash * 23 + _subtitleOriginal.Footer.Trim().GetHashCode();
            }

            var max = Subtitles.Count;
            for (var i = 0; i < max; i++)
            {
                var p = Subtitles[i];
                hash = hash * 23 + p.Number.GetHashCode();
                hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();

                foreach (var line in p.OriginalText.SplitToLines())
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

    private async Task DeleteSelectedItems()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        if (Se.Settings.General.PromptDeleteLines)
        {
            var title = Se.Language.General.Delete;

            var message = string.Format(Se.Language.General.DeleteXLinesPrompt, selectedItems.Count);
            if (selectedItems.Count == 1)
            {
                message = string.Format(Se.Language.General.DeleteLineXPrompt, SelectedSubtitleIndex + 1);
            }

            var answer = await MessageBox.Show(
                Window!,
                title,
                message,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            _shortcutManager.ClearKeys();
        }

        var idx = Subtitles.IndexOf(selectedItems.First());
        _undoRedoManager.StopChangeDetection();
        foreach (var item in selectedItems)
        {
            Subtitles.Remove(item);
        }

        if (idx >= Subtitles.Count)
        {
            idx = Subtitles.Count - 1;
        }

        SelectAndScrollToRow(idx);

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
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        var first = true;
        var makeItalic = true;
        foreach (var item in selectedItems)
        {
            if (first)
            {
                first = false;
                makeItalic = !item.Text.Contains("<i>");
            }

            item.Text = item.Text.Replace("<i>", string.Empty).Replace("</i>", string.Empty);
            item.Text = item.Text.Replace("<I>", string.Empty).Replace("</I>", string.Empty);
            if (makeItalic)
            {
                if (!string.IsNullOrEmpty(item.Text))
                {
                    item.Text = $"<i>{item.Text}</i>";
                }
            }
        }
    }

    private void ToggleBold()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        var first = true;
        var makeBold = true;
        foreach (var item in selectedItems)
        {
            if (first)
            {
                first = false;
                makeBold = !item.Text.Contains("<b>");
            }

            item.Text = item.Text.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
            item.Text = item.Text.Replace("<B>", string.Empty).Replace("</B>", string.Empty);
            if (makeBold)
            {
                if (!string.IsNullOrEmpty(item.Text))
                {
                    item.Text = $"<b>{item.Text}</b>";
                }
            }
        }
    }

    private void SetAlignmentToSelected(string alignment)
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = item.Text
                .Replace("{\\an1}", string.Empty)
                .Replace("{\\an2}", string.Empty)
                .Replace("{\\an3}", string.Empty)
                .Replace("{\\an4}", string.Empty)
                .Replace("{\\an5}", string.Empty)
                .Replace("{\\an6}", string.Empty)
                .Replace("{\\an7}", string.Empty)
                .Replace("{\\an8}", string.Empty)
                .Replace("{\\an9}", string.Empty)
                .Replace("\\an1", string.Empty)
                .Replace("\\an2", string.Empty)
                .Replace("\\an3", string.Empty)
                .Replace("\\an4", string.Empty)
                .Replace("\\an5", string.Empty)
                .Replace("\\an6", string.Empty)
                .Replace("\\an7", string.Empty)
                .Replace("\\an8", string.Empty)
                .Replace("\\an9", string.Empty);

            if (alignment == "an2" && Se.Settings.General.WriteAn2Tag == false)
            {
                return;
            }

            if (!string.IsNullOrEmpty(item.Text))
            {
                if (item.Text.StartsWith("{\\"))
                {
                    item.Text = item.Text.Insert(2, alignment + "\\");
                }
                else
                {
                    item.Text = $"{{\\{alignment}}}{item.Text}";
                }
            }
        }
    }

    public void SubtitleContextOpening(object? sender, EventArgs e)
    {
        MenuItemMergeAsDialog.IsVisible = SubtitleGrid.SelectedItems.Count == 2;
        MenuItemMerge.IsVisible = SubtitleGrid.SelectedItems.Count > 1;
        AreAssaContentMenuItemsVisible = false;
        ShowAutoTranslateSelectedLines = SubtitleGrid.SelectedItems.Count > 0 && ShowColumnOriginalText;
        ShowColumnLayerFlyoutMenuItem = IsFormatAssa;

        if (IsSubtitleGridFlyoutHeaderVisible)
        {
        }
        else
        {
            if (IsFormatAssa)
            {
                AreAssaContentMenuItemsVisible = true;

                MenuItemStyles.Items.Clear();
                var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(_subtitle.Header);
                var stylesToAdd = styles.Select(p => p.Name).Where(p => !string.IsNullOrEmpty(p)).DistinctBy(p => p)
                    .OrderBy(p => p);
                foreach (var style in stylesToAdd)
                {
                    MenuItemStyles.Items.Add(new MenuItem
                    {
                        Header = style,
                        Command = SetStyleForSelectedLinesCommand,
                        CommandParameter = style,
                    });
                }

                if (stylesToAdd.Any())
                {
                    MenuItemStyles.Items.Add(new Separator());
                }

                MenuItemStyles.Items.Add(new MenuItem
                {
                    Header = Se.Language.General.NewDotDotDot,
                    Command = SetNewStyleForSelectedLinesCommand,
                });

                MenuItemActors.Items.Clear();
                foreach (var actor in Subtitles.Select(p => p.Actor).Where(p => !string.IsNullOrEmpty(p))
                             .DistinctBy(p => p).OrderBy(p => p))
                {
                    MenuItemActors.Items.Add(new MenuItem
                    {
                        Header = actor,
                        Command = SetActorForSelectedLinesCommand,
                        CommandParameter = actor,
                    });
                }

                if (MenuItemActors.Items.Count > 0)
                {
                    MenuItemActors.Items.Add(new Separator());
                }

                MenuItemActors.Items.Add(new MenuItem
                {
                    Header = Se.Language.General.NewDotDotDot,
                    Command = SetNewActorForSelectedLinesCommand,
                });
            }
        }
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

    private readonly Lock _onKeyDownHandlerLock = new();

    internal void OnKeyDownHandler(object? sender, KeyEventArgs keyEventArgs)
    {
        lock (_onKeyDownHandlerLock)
        {
            var ticks = DateTime.UtcNow.Ticks; // Stopwatch.GetTimestamp(); GetTimestamp does not work on mac!?
            var timeSpan = TimeSpan.FromTicks(ticks - _lastKeyPressedTicks);
            var k = keyEventArgs.Key;
            if (timeSpan.Seconds > 5)
            {
                _shortcutManager.ClearKeys(); // reset shortcuts if no key pressed for 5 seconds
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
                        Key.Escape,
                        Key.Tab,
                        Key.PageUp,
                        Key.PageDown,
                        Key.BrowserBack,
                        Key.BrowserForward,
                        Key.BrowserFavorites,
                        Key.BrowserHome,
                        Key.Execute,
                        Key.ExSel,
                        Key.LaunchApplication1,
                        Key.LaunchApplication2,
                        Key.LaunchMail,
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
                        Key.F13,
                        Key.F14,
                        Key.F15,
                        Key.F16,
                        Key.F17,
                        Key.F18,
                        Key.F19,
                        Key.F20,
                        Key.F21,
                        Key.F22,
                        Key.F23,
                        Key.F24,
                    };

                    if (!allowedSingleKeyShortcuts.Contains(key))
                    {
                        return;
                    }
                }
            }

            if (SubtitleGrid.IsFocused)
            {
                if (keyEventArgs.Key == Key.Home && keyEventArgs.KeyModifiers == KeyModifiers.None && Subtitles.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    SelectAndScrollToRow(0);
                    return;
                }
                else if (keyEventArgs.Key == Key.End && keyEventArgs.KeyModifiers == KeyModifiers.None && Subtitles.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    SelectAndScrollToRow(Subtitles.Count - 1);
                    return;
                }

                var relayCommand = _shortcutManager.CheckShortcuts(ShortcutCategory.SubtitleGrid.ToStringInvariant());
                if (relayCommand != null)
                {
                    keyEventArgs.Handled = true;
                    relayCommand.Execute(null);
                    return;
                }
            }

            if (AudioVisualizer != null && AudioVisualizer.IsFocused)
            {
                var relayCommand = _shortcutManager.CheckShortcuts(ShortcutCategory.Waveform.ToStringInvariant());
                if (relayCommand != null)
                {
                    keyEventArgs.Handled = true;
                    relayCommand.Execute(null);
                    return;
                }
            }

            var keys = _shortcutManager.GetActiveKeys().Select(p => p.ToString()).ToList();
            var hashCode = ShortCut.CalculateHash(keys, ShortcutCategory.General.ToStringInvariant());

            var rc = _shortcutManager.CheckShortcuts(ShortcutCategory.General.ToStringInvariant().ToLowerInvariant());
            if (rc != null)
            {
                keyEventArgs.Handled = true;
                rc.Execute(null);
            }
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
        IsSubtitleGridFlyoutHeaderVisible = false;
        if (sender is Control { ContextFlyout: not null } control)
        {
            var props = e.GetCurrentPoint(control).Properties;
            _subtitleGridIsLeftClick = props.IsLeftButtonPressed;
            _subtitleGridIsRightClick = props.IsRightButtonPressed;
            _subtitleGridIsControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);

            var hitTest = SubtitleGrid.InputHitTest(e.GetPosition(SubtitleGrid));
            var current = hitTest as Control;
            while (current != null)
            {
                if (current is DataGridColumnHeader)
                {
                    IsSubtitleGridFlyoutHeaderVisible = true;
                    return;
                }

                current = current.Parent as Control;
            }
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

            EditTextCharactersPerSecondOriginal = string.Empty;
            EditTextCharactersPerSecondBackgroundOriginal = Brushes.Transparent;
            EditTextTotalLengthOriginal = string.Empty;
            EditTextTotalLengthBackgroundOriginal = Brushes.Transparent;
            EditTextLineLengthsOriginal = string.Empty;

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

            EditTextCharactersPerSecondOriginal = string.Empty;
            EditTextCharactersPerSecondBackgroundOriginal = Brushes.Transparent;
            EditTextTotalLengthOriginal = string.Empty;
            EditTextTotalLengthBackgroundOriginal = Brushes.Transparent;
            EditTextTotalLengthOriginal = string.Empty;
            EditTextLineLengthsOriginal = string.Empty;

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
            MakeSubtitleTextInfoOriginal(item.OriginalText, item);
        });
    }

    private void MakeSubtitleTextInfo(string text, SubtitleLineViewModel item)
    {
        text ??= string.Empty;

        text = HtmlUtil.RemoveHtmlTags(text, true);
        var totalLength = text.CountCharacters(false);
        var cps = new Paragraph(text, item.StartTime.TotalMilliseconds, item.EndTime.TotalMilliseconds)
            .GetCharactersPerSecond();

        var lines = text.SplitToLines();
        var lineLenghts = new List<string>(lines);
        PanelSingleLineLenghts.Children.Clear();
        PanelSingleLineLenghts.Children.Add(UiUtil.MakeTextBlock(Se.Language.Main.SingleLineLength).WithFontSize(12)
            .WithPadding(2));
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
            if (Se.Settings.General.ColorTextTooLong &&
                lineLenghts[i].Length > Se.Settings.General.SubtitleLineMaximumLength)
            {
                tb.Background = new SolidColorBrush(_errorColor);
            }

            PanelSingleLineLenghts.Children.Add(tb);
        }

        EditTextCharactersPerSecond = string.Format(Se.Language.Main.CharactersPerSecond, $"{cps:0.0}");
        EditTextTotalLength = string.Format(Se.Language.Main.TotalCharacters, totalLength);

        EditTextCharactersPerSecondBackground = Se.Settings.General.ColorTextTooLong &&
                                                cps > Se.Settings.General.SubtitleMaximumCharactersPerSeconds
            ? new SolidColorBrush(_errorColor)
            : new SolidColorBrush(Colors.Transparent);

        EditTextTotalLengthBackground = Se.Settings.General.ColorTextTooLong &&
                                        totalLength > Se.Settings.General.SubtitleLineMaximumLength * lines.Count
            ? new SolidColorBrush(_errorColor)
            : new SolidColorBrush(Colors.Transparent);
    }

    private void MakeSubtitleTextInfoOriginal(string text, SubtitleLineViewModel item)
    {
        text ??= string.Empty;

        text = HtmlUtil.RemoveHtmlTags(text, true);
        var totalLength = text.CountCharacters(false);
        var cps = new Paragraph(text, item.StartTime.TotalMilliseconds, item.EndTime.TotalMilliseconds)
            .GetCharactersPerSecond();

        var lines = text.SplitToLines();
        var lineLenghts = new List<string>(lines);
        PanelSingleLineLenghtsOriginal.Children.Clear();
        PanelSingleLineLenghtsOriginal.Children.Add(UiUtil.MakeTextBlock(Se.Language.Main.SingleLineLength)
            .WithFontSize(12)
            .WithPadding(2));
        var first = true;
        for (var i = 0; i < lines.Count; i++)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                PanelSingleLineLenghtsOriginal.Children.Add(UiUtil.MakeTextBlock("/").WithFontSize(12).WithPadding(2));
            }

            var tb = UiUtil.MakeTextBlock(lineLenghts[i].Length.ToStringInvariant()).WithFontSize(12).WithPadding(2);
            if (Se.Settings.General.ColorTextTooLong &&
                lineLenghts[i].Length > Se.Settings.General.SubtitleLineMaximumLength)
            {
                tb.Background = new SolidColorBrush(_errorColor);
            }

            PanelSingleLineLenghtsOriginal.Children.Add(tb);
        }

        EditTextCharactersPerSecondOriginal = string.Format(Se.Language.Main.CharactersPerSecond, $"{cps:0.0}");
        EditTextTotalLengthOriginal = string.Format(Se.Language.Main.TotalCharacters, totalLength);

        EditTextCharactersPerSecondBackgroundOriginal = Se.Settings.General.ColorTextTooLong &&
                                                        cps > Se.Settings.General.SubtitleMaximumCharactersPerSeconds
            ? new SolidColorBrush(_errorColor)
            : new SolidColorBrush(Colors.Transparent);

        EditTextTotalLengthBackgroundOriginal = Se.Settings.General.ColorTextTooLong &&
                                                totalLength > Se.Settings.General.SubtitleLineMaximumLength *
                                                lines.Count
            ? new SolidColorBrush(_errorColor)
            : new SolidColorBrush(Colors.Transparent);
    }

    private void StartTimers()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _positionTimer.Tick += (s, e) =>
        {
            var text = Se.Language.General.Untitled;
            if (!string.IsNullOrEmpty(_subtitleFileName))
            {
                text = Configuration.Settings.General.TitleBarFullFileName
                    ? _subtitleFileName
                    : Path.GetFileName(_subtitleFileName);
            }

            if (ShowColumnOriginalText)
            {
                text += " + ";

                if (_changeSubtitleHashOriginal != GetFastHashOriginal())
                {
                    text += "*";
                }

                if (string.IsNullOrEmpty(_subtitleFileNameOriginal))
                {
                    text += Se.Language.General.Untitled;
                }
                else
                {
                    text += Configuration.Settings.General.TitleBarFullFileName
                        ? _subtitleFileNameOriginal
                        : Path.GetFileName(_subtitleFileNameOriginal);
                }
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
            if (av != null && vp != null && !string.IsNullOrEmpty(_videoFileName))
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

                if (WaveformCenter && isPlaying)
                {
                    // calculate the center position based on the waveform width
                    var waveformHalfSeconds = (av.EndPositionSeconds - av.StartPositionSeconds) / 2.0;
                    av.SetPosition(Math.Max(0, mediaPlayerSeconds - waveformHalfSeconds), subtitle, mediaPlayerSeconds,
                        firstSelectedIndex, _selectedSubtitles ?? []);
                }
                else if ((isPlaying || !av.IsScrolling) && (mediaPlayerSeconds > av.EndPositionSeconds ||
                                                            mediaPlayerSeconds < av.StartPositionSeconds))
                {
                    av.SetPosition(startPos, subtitle, mediaPlayerSeconds, 0,
                        _selectedSubtitles ?? []);
                }
                else
                {
                    av.SetPosition(av.StartPositionSeconds, subtitle, mediaPlayerSeconds, firstSelectedIndex,
                        _selectedSubtitles ?? []);
                }

                if (_updateAudioVisualizer)
                {
                    av.InvalidateVisual();
                    _updateAudioVisualizer = false;
                }

                if (isPlaying)
                {
                    Projektanker.Icons.Avalonia.Attached.SetIcon(ButtonWaveformPlay, IconNames.Pause);

                    if (SelectCurrentSubtitleWhilePlaying)
                    {
                        var ss = SelectedSubtitle;
                        if (ss == null || mediaPlayerSeconds < ss.StartTime.TotalSeconds ||
                            mediaPlayerSeconds > ss.EndTime.TotalSeconds)
                        {
                            for (var i = 0; i < subtitle.Count; i++)
                            {
                                var p = subtitle[i];
                                if (mediaPlayerSeconds >= p.StartTime.TotalSeconds &&
                                    mediaPlayerSeconds <= p.EndTime.TotalSeconds)
                                {
                                    SelectAndScrollToSubtitle(p);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Projektanker.Icons.Avalonia.Attached.SetIcon(ButtonWaveformPlay, IconNames.Play);
                }
            }

            if (VideoPlayerControl?.VideoPlayerInstance is VideoPlayerInstanceMpv mpv)
            {
                _mpvReloader.RefreshMpv(mpv.MpvContext!, GetUpdateSubtitle(), SelectedSubtitleFormat);
            }
        };
        _positionTimer.Start();

        _slowTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _slowTimer.Tick += (s, e) => { UpdateGaps(); };
        _slowTimer.Start();
    }

    private void UpdateGaps()
    {
        try
        {
            for (var i = 0; i < Subtitles.Count - 1; i++)
            {
                var p = Subtitles[i];
                var next = Subtitles[i + 1];
                p.Gap = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
            }

            Subtitles[Subtitles.Count - 1].Gap = double.MaxValue;
        }
        catch
        {
            // ignore
        }
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

    internal void SubtitleTextOriginalChanged(object? sender, TextChangedEventArgs e)
    {
        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        MakeSubtitleTextInfoOriginal(selectedSubtitle.OriginalText, selectedSubtitle);
        _updateAudioVisualizer = true;
    }

    public void AudioVisualizerOnNewSelectionInsert(object sender, ParagraphEventArgs e)
    {
        var index = _insertService.InsertInCorrectPosition(Subtitles, e.Paragraph);
        SelectAndScrollToRow(index);
        Renumber();
        _updateAudioVisualizer = true;

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

    public void Adjust(TimeSpan adjustment, bool adjustAll, bool adjustSelectedLines,
        bool adjustSelectedLinesAndForward)
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
            MakeSubtitleTextInfoOriginal(selectedSubtitle.OriginalText, selectedSubtitle);
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
            MakeSubtitleTextInfoOriginal(selectedSubtitle.OriginalText, selectedSubtitle);
        });
    }

    internal void EndTimeChanged(object? sender, TimeSpan e)
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
            MakeSubtitleTextInfoOriginal(selectedSubtitle.OriginalText, selectedSubtitle);
        });
    }

    public void GoToAndFocusLine(SubtitleLineViewModel p)
    {
        SelectAndScrollToSubtitle(p);
    }

    internal void TextBoxContextOpening(object? sender, EventArgs e)
    {
    }

    private async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (Window == null || !HasChanges())
        {
            return;
        }

        e.Cancel = true;

        var result = await MessageBox.Show(
            Window,
            Se.Language.General.SaveChangesTitle,
            Se.Language.General.SaveChangesMessage,
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

        Window.Closing -= OnWindowClosing;
        Window.Close();
    }

    public void AudioVisualizerFlyoutMenuOpening(object sender, AudioVisualizer.ContextEventArgs e)
    {
        MenuItemAudioVisualizerInsertNewSelection.IsVisible = false;
        MenuItemAudioVisualizerInsertAtPosition.IsVisible = false;
        MenuItemAudioVisualizerInsertBefore.IsVisible = false;
        MenuItemAudioVisualizerInsertAfter.IsVisible = false;
        MenuItemAudioVisualizerSeparator1.IsVisible = false;
        MenuItemAudioVisualizerDelete.IsVisible = false;
        MenuItemAudioVisualizerDeleteAtPosition.IsVisible = false;
        MenuItemAudioVisualizerSplit.IsVisible = false;
        MenuItemAudioVisualizerFilterByLayer.IsVisible = false;

        if (e.NewParagraph != null)
        {
            MenuItemAudioVisualizerInsertNewSelection.IsVisible = true;
            return;
        }

        var selectedSubtitles = _selectedSubtitles;
        var subtitlesAtPosition = Subtitles
            .Where(p => p.StartTime.TotalSeconds < e.PositionInSeconds &&
                        p.EndTime.TotalSeconds > e.PositionInSeconds).ToList();

        MenuItemAudioVisualizerFilterByLayer.IsVisible = true;

        if (selectedSubtitles?.Count == 1 &&
            subtitlesAtPosition.Count == 1 &&
            selectedSubtitles[0] == subtitlesAtPosition[0])
        {
            MenuItemAudioVisualizerInsertBefore.IsVisible = true;
            MenuItemAudioVisualizerInsertAfter.IsVisible = true;
            MenuItemAudioVisualizerSeparator1.IsVisible = true;
            MenuItemAudioVisualizerDelete.IsVisible = true;
            MenuItemAudioVisualizerSplit.IsVisible = true;
            return;
        }

        if (subtitlesAtPosition.Count == 0)
        {
            MenuItemAudioVisualizerInsertAtPosition.IsVisible = true;
            return;
        }

        if (subtitlesAtPosition.Count > 0)
        {
            MenuItemAudioVisualizerDeleteAtPosition.IsVisible = true;
            return;
        }
    }

    internal void SubtitleTextBoxGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (Subtitles.Count == 0)
        {
            var newSubtitle = new SubtitleLineViewModel
            {
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromMilliseconds(Se.Settings.General.NewEmptyDefaultMs),
                Text = string.Empty,
                OriginalText = string.Empty,
                Number = 1
            };

            Subtitles.Add(newSubtitle);
            SelectedSubtitle = newSubtitle;
            SelectedSubtitleIndex = 0;
        }
    }

    internal void ComboBoxSubtitleFormatChanged(object? sender, SelectionChangedEventArgs e)
    {
        IsFormatAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        HasFormatStyle = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        ShowLayer = IsFormatAssa && Se.Settings.Appearance.ShowLayer;
        ShowLayerFilterIcon = IsFormatAssa && Se.Settings.Appearance.ShowLayer && _visibleLayers != null;

        if (!IsFormatAssa)
        {
            ShowColumnLayer = false;
            ShowColumnLayerFlyoutMenuItem = false;
        }

        AutoFitColumns();

        if (!_opening && e.RemovedItems.Count == 1 && e.AddedItems.Count == 1)
        {
            var oldFormat = e.RemovedItems[0] as SubtitleFormat;
            var format = e.AddedItems[0] as SubtitleFormat;

            if (oldFormat != null && format != null)
            {
                _subtitle = GetUpdateSubtitle();

                oldFormat.RemoveNativeFormatting(_subtitle, format);

                if (format is AdvancedSubStationAlpha)
                {
                    if (oldFormat is WebVTT || oldFormat is WebVTTFileWithLineNumber)
                    {
                        //                        _subtitle = WebVttToAssa.Convert(_subtitle, new SsaStyle(), VideoPlayerControl?.VideoPlayerInstance?.Width ?? 0, VideoPlayerControl?.VideoPlayerInstance?.Height ?? 0);
                    }

                    foreach (var p in _subtitle.Paragraphs)
                    {
                        p.Text = AdvancedSubStationAlpha.FormatText(p.Text);
                    }

                    if (oldFormat is SubStationAlpha)
                    {
                        if (_subtitle.Header != null && !_subtitle.Header.Contains("[V4+ Styles]"))
                        {
                            _subtitle.Header =
                                AdvancedSubStationAlpha.GetHeaderAndStylesFromSubStationAlpha(_subtitle.Header);
                            foreach (var p in _subtitle.Paragraphs)
                            {
                                if (p.Extra != null)
                                {
                                    p.Extra = p.Extra.TrimStart('*');
                                }
                            }
                        }
                    }
                    else if (oldFormat is AdvancedSubStationAlpha && string.IsNullOrEmpty(_subtitle.Header))
                    {
                        _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
                    }

                    //SetAssaResolutionWithChecks();
                }

                SetSubtitles(_subtitle);
            }
        }
    }

    internal void AutoSelectOnPlayCheckedChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Task.Delay(50);
            Se.Settings.General.SelectCurrentSubtitleWhilePlaying = SelectCurrentSubtitleWhilePlaying;
        });
    }

    internal void WaveformCenterCheckedChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Task.Delay(50);
            Se.Settings.General.SelectCurrentSubtitleWhilePlaying = SelectCurrentSubtitleWhilePlaying;
        });
    }

    internal void SubtitleGridOnDragOver(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Copy; // show copy cursor
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    internal void SubtitleGridOnDrop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files))
        {
            return;
        }

        var files = e.Data.GetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var doContinue = await HasChangesContinue();
                if (!doContinue)
                {
                    return;
                }

                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && File.Exists(path))
                    {
                        await SubtitleOpen(path);
                    }
                }
            });
        }
    }

    internal void VideoOnDragOver(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Copy; // show copy cursor
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    internal void VideoOnDrop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files))
        {
            return;
        }

        var files = e.Data.GetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && File.Exists(path))
                    {
                        var ext = Path.GetExtension(path).ToLowerInvariant();
                        var subtitelExtensions = new List<string>
                        {
                            ".ass",
                            ".cap",
                            ".dfxp",
                            ".pac",
                            ".sami",
                            ".smi",
                            ".srt",
                            ".ssa",
                            ".stl",
                            ".sub",
                            ".sup",
                            ".ttml",
                            ".txt",
                            ".vtt",
                            ".xml",
                        };

                        if (subtitelExtensions.Contains(ext))
                        {
                            var doContinue = await HasChangesContinue();
                            if (!doContinue)
                            {
                                return;
                            }

                            await SubtitleOpen(path);
                            return;
                        }

                        await VideoOpenFile(path);
                    }
                }
            });
        }
    }
}