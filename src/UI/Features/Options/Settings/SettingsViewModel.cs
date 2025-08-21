using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DownloadFfmpegViewModel = Nikse.SubtitleEdit.Features.Shared.DownloadFfmpegViewModel;
using DownloadLibMpvViewModel = Nikse.SubtitleEdit.Features.Shared.DownloadLibMpvViewModel;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private int _singleLineMaxLength;
    [ObservableProperty] private double _optimalCharsPerSec;
    [ObservableProperty] private double _maxCharsPerSec;
    [ObservableProperty] private double _maxWordsPerMin;
    [ObservableProperty] private int _minDurationMs;
    [ObservableProperty] private int _maxDurationMs;
    [ObservableProperty] private int _minGapMs;
    [ObservableProperty] private int _maxLines;
    [ObservableProperty] private int _unbreakShorterThanMs;
    [ObservableProperty] private int _newEmptyDefaultMs;
    [ObservableProperty] private bool _promptDeleteLines;
    [ObservableProperty] private bool _lockTimeCodes;
    [ObservableProperty] private bool _rememberPositionAndSize;
    [ObservableProperty] private bool _autoBackupOn;
    [ObservableProperty] private int _autoBackupIntervalMinutes;
    [ObservableProperty] private int _autoBackupDeleteAfterMonths;

    [ObservableProperty] private ObservableCollection<string> _defaultSubtitleFormats;
    [ObservableProperty] private string _selectedDefaultSubtitleFormat;

    [ObservableProperty] private ObservableCollection<string> _saveSubtitleFormats;
    [ObservableProperty] private string _selectedSaveSubtitleFormat;

    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    [ObservableProperty] private TextEncoding _defaultEncoding;

    [ObservableProperty] private bool _showToolbarNew;
    [ObservableProperty] private bool _showToolbarOpen;
    [ObservableProperty] private bool _showToolbarSave;
    [ObservableProperty] private bool _showToolbarSaveAs;
    [ObservableProperty] private bool _showToolbarFind;
    [ObservableProperty] private bool _showToolbarReplace;
    [ObservableProperty] private bool _showToolbarSpellCheck;
    [ObservableProperty] private bool _showToolbarSettings;
    [ObservableProperty] private bool _showToolbarLayout;
    [ObservableProperty] private bool _showToolbarHelp;
    [ObservableProperty] private bool _showToolbarEncoding;
    [ObservableProperty] private bool _showToolbarHints;

    [ObservableProperty] private bool _colorDurationTooShort;
    [ObservableProperty] private bool _colorDurationTooLong;
    [ObservableProperty] private bool _colorTextTooLong;
    [ObservableProperty] private bool _colorTextTooWide;
    [ObservableProperty] private bool _colorTextTooManyLines;
    [ObservableProperty] private bool _colorOverlap;
    [ObservableProperty] private bool _colorGapTooShort;
    [ObservableProperty] private Color _errorColor;

    [ObservableProperty] private ObservableCollection<VideoPlayerItem> _videoPlayers;
    [ObservableProperty] private VideoPlayerItem _selectedVideoPlayer;
    [ObservableProperty] private bool _showStopButton;
    [ObservableProperty] private bool _showFullscreenButton;
    [ObservableProperty] private bool _autoOpenVideoFile;

    [ObservableProperty] private bool _waveformDrawGridLines;
    [ObservableProperty] private bool _waveformCenterVideoPosition;
    [ObservableProperty] private bool _waveformShowToolbar;
    [ObservableProperty] private bool _waveformFocusTextboxAfterInsertNew;
    [ObservableProperty] private string _libMpvPath;
    [ObservableProperty] private string _libMpvStatus;
    [ObservableProperty] private bool _isLibMpvDownloadVisible;
    [ObservableProperty] private string _ffmpegPath;
    [ObservableProperty] private string _ffmpegStatus;
    [ObservableProperty] private Color _waveformColor;
    [ObservableProperty] private Color _waveformSelectedColor;
    [ObservableProperty] private bool _waveformInvertMouseWheel;

    [ObservableProperty] private ObservableCollection<string> _themes;
    [ObservableProperty] private string _selectedTheme;
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private double _textBoxFontSize;
    [ObservableProperty] private bool _textBoxFontBold;

    public ObservableCollection<FileTypeAssociationViewModel> FileTypeAssociations { get; set; } = new()
    {
        new() { Extension = ".ass", IconPath = "avares://SubtitleEdit/Assets/FileTypes/ass.ico" },
        new() { Extension = ".dfxp", IconPath = "avares://SubtitleEdit/Assets/FileTypes/dfxp.ico" },
        new() { Extension = ".itt", IconPath = "avares://SubtitleEdit/Assets/FileTypes/itt.ico" },
        new() { Extension = ".lrc", IconPath = "avares://SubtitleEdit/Assets/FileTypes/lrc.ico" },
        new() { Extension = ".sbv", IconPath = "avares://SubtitleEdit/Assets/FileTypes/sbv.ico" },
        new() { Extension = ".smi", IconPath = "avares://SubtitleEdit/Assets/FileTypes/smi.ico" },
        new() { Extension = ".srt", IconPath = "avares://SubtitleEdit/Assets/FileTypes/srt.ico" },
        new() { Extension = ".ssa", IconPath = "avares://SubtitleEdit/Assets/FileTypes/ssa.ico" },
        new() { Extension = ".stl", IconPath = "avares://SubtitleEdit/Assets/FileTypes/stl.ico" },
        new() { Extension = ".sub", IconPath = "avares://SubtitleEdit/Assets/FileTypes/sub.ico" },
        new() { Extension = ".vtt", IconPath = "avares://SubtitleEdit/Assets/FileTypes/vtt.ico" },
    };

    public bool OkPressed { get; set; }
    public Window? Window { get; internal set; }
    public ScrollViewer ScrollView { get; internal set; }
    public List<SettingsSection> Sections { get; internal set; }

    private readonly IWindowService _windowService;

    public SettingsViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Themes = ["System", "Light", "Dark"];
        SelectedTheme = Themes[0];
        
        FontNames = new  ObservableCollection<string>(FontHelper.GetSystemFonts());
        FontNames.Insert(0, "Default");
        SelectedFontName = FontNames.First();
        
        ScrollView = new ScrollViewer();
        Sections = new List<SettingsSection>();

        VideoPlayers = new ObservableCollection<VideoPlayerItem>(VideoPlayerItem.ListVideoPlayerItem());
        SelectedVideoPlayer = VideoPlayers[0];

        var subtitleFormats = SubtitleFormat.AllSubtitleFormats;
        var defaultSubtitleFormats = new List<string>();
        var saveSubtitleFormats = new List<string> { "Auto" };
        foreach (var format in subtitleFormats)
        {
            if (format.Name.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            defaultSubtitleFormats.Add(format.FriendlyName);
            saveSubtitleFormats.Add(format.FriendlyName);
        }
        DefaultSubtitleFormats = new ObservableCollection<string>(defaultSubtitleFormats);
        SaveSubtitleFormats = new ObservableCollection<string>(saveSubtitleFormats);
        SelectedDefaultSubtitleFormat = DefaultSubtitleFormats.First();
        SelectedSaveSubtitleFormat = SaveSubtitleFormats.First();
        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        DefaultEncoding = Encodings.First();

        ErrorColor = Color.FromArgb(50, 255, 0, 0);

        FfmpegStatus = "Not installed";
        FfmpegPath = string.Empty;

        LibMpvStatus = "Not installed";
        LibMpvPath = string.Empty;
        IsLibMpvDownloadVisible = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        LoadSettings();
    }

    private void LoadSettings()
    {
        var general = Se.Settings.General;
        var appearance = Se.Settings.Appearance;

        SingleLineMaxLength = general.SubtitleLineMaximumLength;
        OptimalCharsPerSec = general.SubtitleOptimalCharactersPerSeconds;
        MaxCharsPerSec = general.SubtitleMaximumCharactersPerSeconds;
        MaxWordsPerMin = general.SubtitleMaximumWordsPerMinute;
        MinDurationMs = general.SubtitleMaximumDisplayMilliseconds;
        MaxDurationMs = general.SubtitleMaximumDisplayMilliseconds;
        MinGapMs = general.MinimumMillisecondsBetweenLines;
        MaxLines = general.MaxNumberOfLines;
        UnbreakShorterThanMs = general.MergeLinesShorterThan;
        NewEmptyDefaultMs = general.NewEmptyDefaultMs;
        PromptDeleteLines = general.PromptDeleteLines;
        LockTimeCodes = general.LockTimeCodes;
        RememberPositionAndSize = general.RememberPositionAndSize;
        AutoBackupOn = general.AutoBackupOn;
        AutoBackupIntervalMinutes = general.AutoBackupIntervalMinutes;
        AutoBackupDeleteAfterMonths = general.AutoBackupDeleteAfterMonths;
        DefaultEncoding = Encodings.FirstOrDefault(e => e.DisplayName == general.DefaultEncoding) ?? Encodings.First();

        SelectedDefaultSubtitleFormat = general.DefaultSubtitleFormat;
        if (!DefaultSubtitleFormats.Contains(SelectedDefaultSubtitleFormat))
        { 
            SelectedDefaultSubtitleFormat = DefaultSubtitleFormats.FirstOrDefault() ?? string.Empty;
        }

        SelectedTheme = appearance.Theme;
        SelectedFontName = FontNames.FirstOrDefault(p=>p == appearance.FontName) ?? FontNames.First();
        ShowToolbarNew = appearance.ToolbarShowFileNew;
        ShowToolbarOpen = appearance.ToolbarShowFileOpen;
        ShowToolbarSave = appearance.ToolbarShowSave;
        ShowToolbarSaveAs = appearance.ToolbarShowSaveAs;
        ShowToolbarFind = appearance.ToolbarShowFind;
        ShowToolbarReplace = appearance.ToolbarShowReplace;
        ShowToolbarSpellCheck = appearance.ToolbarShowSpellCheck;
        ShowToolbarSettings = appearance.ToolbarShowSettings;
        ShowToolbarLayout = appearance.ToolbarShowLayout;
        ShowToolbarHelp = appearance.ToolbarShowHelp;
        ShowToolbarEncoding = appearance.ToolbarShowEncoding;
        ShowToolbarHints = appearance.ToolbarShowHints;
        TextBoxFontSize = appearance.SubtitleTextBoxFontSize;
        TextBoxFontBold = appearance.SubtitleTextBoxFontBold;

        WaveformDrawGridLines = Se.Settings.Waveform.DrawGridLines;
        WaveformCenterVideoPosition = Se.Settings.Waveform.CenterVideoPosition;
        WaveformShowToolbar = Se.Settings.Waveform.ShowToolbar;
        WaveformFocusTextboxAfterInsertNew = Se.Settings.Waveform.FocusTextBoxAfterInsertNew;
        WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor();
        WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor();
        WaveformInvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel;

        ColorDurationTooLong = general.ColorDurationTooLong;
        ColorDurationTooShort = general.ColorDurationTooShort;
        ColorTextTooLong = general.ColorTextTooLong;
        ColorTextTooWide = general.ColorTextTooWide;
        ColorTextTooManyLines = general.ColorTextTooManyLines;
        ColorOverlap = general.ColorTimeCodeOverlap;
        ColorGapTooShort = general.ColorGapTooShort;
        ErrorColor = general.ErrorColor.FromHexToColor();

        var video = Se.Settings.Video;
        var videoPlayer = VideoPlayers.FirstOrDefault(p => p.Name == video.VideoPlayer);
        if (videoPlayer != null)
        {
            SelectedVideoPlayer = videoPlayer;
        }
        ShowStopButton = video.ShowStopButton;
        ShowFullscreenButton = video.ShowFullscreenButton;
        AutoOpenVideoFile = video.AutoOpen;

        FfmpegPath = Se.Settings.General.FfmpegPath;
        LibMpvPath = Se.Settings.General.LibMpvPath;
        SetFfmpegStatus();
        SetLibMpvStatus();
    }

    private void SaveSettings()
    {
        var general = Se.Settings.General;
        var appearance = Se.Settings.Appearance;
        var video = Se.Settings.Video;

        general.SubtitleLineMaximumLength = SingleLineMaxLength;
        general.SubtitleOptimalCharactersPerSeconds = OptimalCharsPerSec;
        general.SubtitleMaximumCharactersPerSeconds = MaxCharsPerSec;
        general.SubtitleMaximumWordsPerMinute = MaxWordsPerMin;
        general.SubtitleMaximumDisplayMilliseconds = MaxDurationMs; // Assuming MinDurationMs and MaxDurationMs are the same source
        general.MinimumMillisecondsBetweenLines = MinGapMs;
        general.MaxNumberOfLines = MaxLines;
        general.MergeLinesShorterThan = UnbreakShorterThanMs;
        general.NewEmptyDefaultMs = NewEmptyDefaultMs;
        general.PromptDeleteLines = PromptDeleteLines;
        general.LockTimeCodes = LockTimeCodes;
        general.RememberPositionAndSize = RememberPositionAndSize;
        general.AutoBackupOn = AutoBackupOn;
        general.AutoBackupIntervalMinutes = AutoBackupIntervalMinutes;
        general.AutoBackupDeleteAfterMonths = AutoBackupDeleteAfterMonths;
        general.DefaultEncoding = DefaultEncoding?.DisplayName ?? Encodings.First().DisplayName;
        general.DefaultSubtitleFormat = SelectedDefaultSubtitleFormat;

        appearance.Theme = SelectedTheme;
        appearance.FontName = SelectedFontName == FontNames.First() 
                                ? string.Empty
                                : SelectedFontName;
        appearance.ToolbarShowFileNew = ShowToolbarNew;
        appearance.ToolbarShowFileOpen = ShowToolbarOpen;
        appearance.ToolbarShowSave = ShowToolbarSave;
        appearance.ToolbarShowSaveAs = ShowToolbarSaveAs;
        appearance.ToolbarShowFind = ShowToolbarFind;
        appearance.ToolbarShowReplace = ShowToolbarReplace;
        appearance.ToolbarShowSpellCheck = ShowToolbarSpellCheck;
        appearance.ToolbarShowSettings = ShowToolbarSettings;
        appearance.ToolbarShowLayout = ShowToolbarLayout;
        appearance.ToolbarShowHelp = ShowToolbarHelp;
        appearance.ToolbarShowEncoding = ShowToolbarEncoding;
        appearance.ToolbarShowHints = ShowToolbarHints;
        appearance.SubtitleTextBoxFontSize = TextBoxFontSize;
        appearance.SubtitleTextBoxFontBold = TextBoxFontBold;

        Se.Settings.Waveform.DrawGridLines = WaveformDrawGridLines;
        Se.Settings.Waveform.CenterVideoPosition = WaveformCenterVideoPosition;
        Se.Settings.Waveform.FocusTextBoxAfterInsertNew = WaveformFocusTextboxAfterInsertNew;
        Se.Settings.Waveform.ShowToolbar = WaveformShowToolbar;
        Se.Settings.Waveform.WaveformColor = WaveformColor.FromColorToHex();
        Se.Settings.Waveform.WaveformSelectedColor = WaveformSelectedColor.FromColorToHex();
        Se.Settings.Waveform.InvertMouseWheel = WaveformInvertMouseWheel;

        general.ColorDurationTooLong = ColorDurationTooLong;
        general.ColorDurationTooShort = ColorDurationTooShort;
        general.ColorTextTooLong = ColorTextTooLong;
        general.ColorTextTooWide = ColorTextTooWide;
        general.ColorTextTooManyLines = ColorTextTooManyLines;
        general.ColorTimeCodeOverlap = ColorOverlap;
        general.ColorGapTooShort = ColorGapTooShort;
        general.ErrorColor = ErrorColor.FromColorToHex();

        video.VideoPlayer = SelectedVideoPlayer.Name;
        video.ShowStopButton = ShowStopButton;
        video.ShowFullscreenButton = ShowFullscreenButton;
        video.AutoOpen = AutoOpenVideoFile;

        general.FfmpegPath = FfmpegPath;
        general.LibMpvPath = LibMpvPath;

        Se.SaveSettings();
    }

    private void SetFfmpegStatus()
    {
        if (!string.IsNullOrEmpty(FfmpegPath) && File.Exists(FfmpegPath))
        {
            FfmpegStatus = "Installed";
        }
        else if (File.Exists(DownloadFfmpegViewModel.GetFfmpegFileName()))
        {
            FfmpegStatus = "Installed";
        }
        else
        {
            if (FfmpegHelper.IsFfmpegInstalled())
            {
                FfmpegStatus = string.Empty;
            }
            else
            {
                FfmpegStatus = "Not installed";
            }
        }
    }

    private void SetLibMpvStatus()
    {
        if (!string.IsNullOrEmpty(LibMpvPath) && File.Exists(LibMpvPath))
        {
            LibMpvStatus = "Installed";
        }
        else
        {
            LibMpvStatus = "Not installed";
        }
    }


    public async void ScrollElementIntoView(ScrollViewer scrollViewer, Control target)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Yield(); // Ensures target has been laid out

            // Fade out
            //await FadeToAsync(ScrollView, 0, TimeSpan.FromMilliseconds(100));
            await RunFadeAnimation(ScrollView, from: 1, to: 0, TimeSpan.FromMilliseconds(100));


            await Task.Yield(); // Ensures target has been laid out
            scrollViewer.ScrollToHome();
            await Task.Yield(); // Ensures target has been laid out

            var targetPosition = target.TranslatePoint(new Point(0, 0), scrollViewer);
            if (targetPosition.HasValue)
            {
                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, targetPosition.Value.Y);
            }

            await Task.Yield(); // Ensures target has been laid out
            await RunFadeAnimation(ScrollView, from: 0, to: 1, TimeSpan.FromMilliseconds(200));

        }, DispatcherPriority.Background);
    }

    private static Task RunFadeAnimation(Control control, double from, double to, TimeSpan duration)
    {
        var animation = new Animation
        {
            Duration = duration,
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters = { new Setter(Visual.OpacityProperty, from) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(Visual.OpacityProperty, to) }
                }
            }
        };

        return animation.RunAsync(control);
    }

    [RelayCommand]
    private void ScrollToSection(string title)
    {
        var section = Sections.FirstOrDefault(section => section.IsVisible && section.Title == title);
        if (section != null)
        {
            ScrollElementIntoView(ScrollView, section.Panel!);
        }
    }

    [RelayCommand]
    private async Task DownloadFfmpeg()
    {
        var vm = await _windowService.ShowDialogAsync<DownloadFfmpegWindow, DownloadFfmpegViewModel>(Window!);
        if (string.IsNullOrEmpty(vm.FfmpegFileName))
        {
            return;
        }

        FfmpegPath = vm.FfmpegFileName;
        SetFfmpegStatus();
    }

    [RelayCommand]
    private async Task DownloadLibMpv()
    {
        var vm = await _windowService.ShowDialogAsync<DownloadLibMpvWindow, DownloadLibMpvViewModel>(Window!);
        if (string.IsNullOrEmpty(vm.LibMpvFileName))
        {
            return;
        }

        LibMpvPath = vm.LibMpvFileName;
        SetLibMpvStatus();
    }

    [RelayCommand]
    private void CommandOk()
    {
        SaveSettings();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void CommandCancel()
    {
        Window?.Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}