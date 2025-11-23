using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Platform.Windows;
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
    [ObservableProperty] private ObservableCollection<string> _profiles;
    [ObservableProperty] private string _selectedProfile;
    [ObservableProperty] private int? _singleLineMaxLength;
    [ObservableProperty] private double? _optimalCharsPerSec;
    [ObservableProperty] private double? _maxCharsPerSec;
    [ObservableProperty] private double? _maxWordsPerMin;
    [ObservableProperty] private int? _minDurationMs;
    [ObservableProperty] private int? _maxDurationMs;
    [ObservableProperty] private int? _minGapMs;
    [ObservableProperty] private int? _maxLines;
    [ObservableProperty] private int? _unbreakLinesShorterThan;
    [ObservableProperty] private ObservableCollection<DialogStyleDisplay> _dialogStyles;
    [ObservableProperty] private DialogStyleDisplay _dialogStyle;
    [ObservableProperty] private ObservableCollection<ContinuationStyleDisplay> _continuationStyles;
    [ObservableProperty] private ContinuationStyleDisplay _continuationStyle;
    [ObservableProperty] private ObservableCollection<CpsLineLengthStrategyDisplay> _cpsLineLengthStrategies;
    [ObservableProperty] private CpsLineLengthStrategyDisplay _cpsLineLengthStrategy;
    [ObservableProperty] private ObservableCollection<string> _fonts;
    [ObservableProperty] private string _mpvPreviewFontName;
    [ObservableProperty] private int _mpvPreviewFontSize;
    [ObservableProperty] private bool _mpvPreviewFontBold;
    [ObservableProperty] private Color _mpvPreviewColorPrimary;
    [ObservableProperty] private Color _mpvPreviewColorOutline;
    [ObservableProperty] private Color _mpvPreviewColorShadow;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _mpvPreviewBorderTypes;
    [ObservableProperty] private BorderStyleItem _mpvPreviewSelectedBorderType;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _mpvPreviewBorderTypes;
    [ObservableProperty] private BorderStyleItem _mpvPreviewSelectedBorderType;


    [ObservableProperty] private int? _newEmptyDefaultMs;
    [ObservableProperty] private bool _promptDeleteLines;
    [ObservableProperty] private bool _lockTimeCodes;
    [ObservableProperty] private bool _rememberPositionAndSize;
    [ObservableProperty] private bool _useFrameMode;
    [ObservableProperty] private bool _autoBackupOn;
    [ObservableProperty] private int? _autoBackupIntervalMinutes;
    [ObservableProperty] private int? _autoBackupDeleteAfterMonths;

    [ObservableProperty] private ObservableCollection<string> _defaultSubtitleFormats;
    [ObservableProperty] private string _selectedDefaultSubtitleFormat;

    [ObservableProperty] private ObservableCollection<string> _saveSubtitleFormats;
    [ObservableProperty] private string _selectedSaveSubtitleFormat;

    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    [ObservableProperty] private TextEncoding _defaultEncoding;

    [ObservableProperty] private bool _goToLineNumberAlsoSetVideoPosition;

    [ObservableProperty] private bool _showUpDownStartTime;
    [ObservableProperty] private bool _showUpDownEndTime;
    [ObservableProperty] private bool _showUpDownDuration;
    [ObservableProperty] private bool _showUpDownLabels;

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
    [ObservableProperty] private bool _waveformFocusOnMouseOver;
    [ObservableProperty] private bool _waveformCenterVideoPosition;
    [ObservableProperty] private bool _waveformShowToolbar;
    [ObservableProperty] private bool _showWaveformVerticalZoom;
    [ObservableProperty] private bool _showWaveformHorizontalZoom;
    [ObservableProperty] private bool _showWaveformVideoPositionSlider;
    [ObservableProperty] private bool _showWaveformPlaybackSpeed;
    [ObservableProperty] private bool _waveformFocusTextboxAfterInsertNew;
    [ObservableProperty] private string _waveformSpaceInfo;
    [ObservableProperty] private string _libMpvPath;
    [ObservableProperty] private string _libMpvStatus;
    [ObservableProperty] private bool _isLibMpvDownloadVisible;
    [ObservableProperty] private string _ffmpegPath;
    [ObservableProperty] private string _ffmpegStatus;
    [ObservableProperty] private Color _waveformColor;
    [ObservableProperty] private Color _waveformSelectedColor;
    [ObservableProperty] private Color _waveformCursorColor;
    [ObservableProperty] private bool _waveformInvertMouseWheel;
    [ObservableProperty] private bool _waveformSnapToShotChanges;
    [ObservableProperty] private bool _waveformShotChangesAutoGenerate;


    [ObservableProperty] private ObservableCollection<string> _themes;
    [ObservableProperty] private string _selectedTheme;
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private double _subtitleGridFontSize;
    [ObservableProperty] private string _subtitleTextBoxAndGridFontName;
    [ObservableProperty] private double _textBoxFontSize;
    [ObservableProperty] private bool _textBoxFontBold;
    [ObservableProperty] private bool _textBoxCenterText;
    [ObservableProperty] private bool _showButtonHints;
    [ObservableProperty] private bool _gridCompactMode;
    [ObservableProperty] private bool _showAssaLayer;
    [ObservableProperty] private bool _showHorizontalLineAboveToolbar;
    [ObservableProperty] private bool _showHorizontalLineBelowToolbar;
    [ObservableProperty] private ObservableCollection<GridLinesVisibilityDisplay> _gridLinesVisibilities;
    [ObservableProperty] private GridLinesVisibilityDisplay _selectedGridLinesVisibility;
    [ObservableProperty] private Color _darkModeBackgroundColor;
    [ObservableProperty] private Color _bookmarkColor;
    [ObservableProperty] private bool _isEditCustomContinuationStyleVisible;
    [ObservableProperty] private bool _isMpvChosen;

    [ObservableProperty] private bool _existsErrorLogFile;
    [ObservableProperty] private bool _existsWhisperLogFile;
    [ObservableProperty] private bool _existsSettingsFile;

    private int _continuationPause;
    private string _customContinuationStyleSuffix;
    private bool _customContinuationStyleSuffixApplyIfComma;
    private bool _customContinuationStyleSuffixAddSpace;
    private bool _customContinuationStyleSuffixReplaceComma;
    private string _customContinuationStylePrefix;
    private bool _customContinuationStylePrefixAddSpace;
    private bool _customContinuationStyleUseDifferentStyleGap;
    private string _customContinuationStyleGapSuffix;
    private bool _customContinuationStyleGapSuffixApplyIfComma;
    private bool _customContinuationStyleGapSuffixAddSpace;
    private bool _customContinuationStyleGapSuffixReplaceComma;
    private string _customContinuationStyleGapPrefix;
    private bool _customContinuationStyleGapPrefixAddSpace;


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
    private readonly IFolderHelper _folderHelper;
    private MainViewModel? _mainViewModel;
    private List<ProfileDisplay> _profilesForEdit;

    public SettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;

        Profiles = new ObservableCollection<string>();
        SelectedProfile = "Default";
        DialogStyles = new ObservableCollection<DialogStyleDisplay>(DialogStyleDisplay.List());
        ContinuationStyles = new ObservableCollection<ContinuationStyleDisplay>(ContinuationStyleDisplay.List());
        CpsLineLengthStrategies = new ObservableCollection<CpsLineLengthStrategyDisplay>(CpsLineLengthStrategyDisplay.List());
        SubtitleTextBoxAndGridFontName = "Default";
        DialogStyle = DialogStyles.First();
        ContinuationStyle = ContinuationStyles.First();
        CpsLineLengthStrategy = CpsLineLengthStrategies.First();
        Fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        MpvPreviewBorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());

        Themes = [Se.Language.General.System, Se.Language.General.Light, Se.Language.General.Dark];
        SelectedTheme = Themes[0];

        FontNames = new ObservableCollection<string>(FontHelper.GetSystemFonts());
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
        WaveformSpaceInfo = string.Empty;
        IsMpvChosen = true;

        GridLinesVisibilities = new ObservableCollection<GridLinesVisibilityDisplay>(GridLinesVisibilityDisplay.GetAll());
        SelectedGridLinesVisibility = GridLinesVisibilities[0];

        ErrorColor = Color.FromArgb(50, 255, 0, 0);

        FfmpegStatus = Se.Language.General.NotInstalled;
        FfmpegPath = string.Empty;

        LibMpvStatus = Se.Language.General.NotInstalled;
        LibMpvPath = string.Empty;
        IsLibMpvDownloadVisible = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        _profilesForEdit = new List<ProfileDisplay>();

        _customContinuationStyleGapPrefix = string.Empty;
        _customContinuationStyleGapPrefixAddSpace = false;
        _customContinuationStyleGapSuffix = string.Empty;
        _customContinuationStyleGapSuffixAddSpace = false;
        _customContinuationStyleGapSuffixApplyIfComma = false;
        _customContinuationStyleGapSuffixReplaceComma = false;
        _customContinuationStylePrefix = string.Empty;
        _customContinuationStylePrefixAddSpace = false;
        _customContinuationStyleSuffix = string.Empty;
        _customContinuationStyleSuffixAddSpace = false;
        _customContinuationStyleSuffixApplyIfComma = false;
        _customContinuationStyleSuffixReplaceComma = false;

        LoadSettings();
    }

    private void LoadSettings()
    {
        var general = Se.Settings.General;
        var appearance = Se.Settings.Appearance;

        foreach (var profile in general.Profiles)
        {
            var pd = new ProfileDisplay
            {
                Name = profile.Name,
                SingleLineMaxLength = profile.SubtitleLineMaximumLength,
                OptimalCharsPerSec = (double)profile.SubtitleOptimalCharactersPerSeconds,
                MaxCharsPerSec = (double)profile.SubtitleMaximumCharactersPerSeconds,
                MaxWordsPerMin = (double)profile.SubtitleMaximumWordsPerMinute,
                MinDurationMs = profile.SubtitleMinimumDisplayMilliseconds,
                MaxDurationMs = profile.SubtitleMaximumDisplayMilliseconds,
                MinGapMs = profile.MinimumMillisecondsBetweenLines,
                MaxLines = profile.MaxNumberOfLines,
                UnbreakLinesShorterThan = profile.MergeLinesShorterThan,
                DialogStyle = DialogStyles.FirstOrDefault(p => p.Code == profile.DialogStyle.ToString()) ?? DialogStyles.First(),
                ContinuationStyle = ContinuationStyles.FirstOrDefault(p => p.Code == profile.ContinuationStyle.ToString()) ?? ContinuationStyles.First(),
                CpsLineLengthStrategy = CpsLineLengthStrategies.FirstOrDefault(p => p.Code == profile.CpsLineLengthStrategy) ?? CpsLineLengthStrategies.First()
            };
            _profilesForEdit.Add(pd);
        }

        Profiles.Clear();
        foreach (var profile in Se.Settings.General.Profiles)
        {
            Profiles.Add(profile.Name);
        }
        if (Profiles.Count == 0)
        {
            Profiles.Add("Default");
        }
        SelectedProfile = general.CurrentProfile;
        if (!Profiles.Contains(SelectedProfile))
        {
            SelectedProfile = Profiles.First();
        }

        SingleLineMaxLength = general.SubtitleLineMaximumLength;
        OptimalCharsPerSec = general.SubtitleOptimalCharactersPerSeconds;
        MaxCharsPerSec = general.SubtitleMaximumCharactersPerSeconds;
        MaxWordsPerMin = general.SubtitleMaximumWordsPerMinute;
        MinDurationMs = general.SubtitleMinimumDisplayMilliseconds;
        MaxDurationMs = general.SubtitleMaximumDisplayMilliseconds;
        MinGapMs = general.MinimumMillisecondsBetweenLines;
        MaxLines = general.MaxNumberOfLines;
        UnbreakLinesShorterThan = general.UnbreakLinesShorterThan;
        DialogStyle = DialogStyles.FirstOrDefault(p => p.Code == general.DialogStyle) ?? DialogStyles.First();
        ContinuationStyle = ContinuationStyles.FirstOrDefault(p => p.Code == general.ContinuationStyle) ?? ContinuationStyles.First();
        CpsLineLengthStrategy = CpsLineLengthStrategies.FirstOrDefault(p => p.Code == general.CpsLineLengthStrategy) ?? CpsLineLengthStrategies.First();

        UseFrameMode = general.UseFrameMode;
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

        GoToLineNumberAlsoSetVideoPosition = Se.Settings.Tools.GoToLineNumberAlsoSetVideoPosition;

        SelectedTheme = appearance.Theme;
        SelectedFontName = FontNames.FirstOrDefault(p => p == appearance.FontName) ?? FontNames.First();
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
        SubtitleGridFontSize = appearance.SubtitleGridFontSize;
        SubtitleTextBoxAndGridFontName = appearance.SubtitleTextBoxAndGridFontName;
        TextBoxFontSize = appearance.SubtitleTextBoxFontSize;
        TextBoxFontBold = appearance.SubtitleTextBoxFontBold;
        TextBoxCenterText = appearance.SubtitleTextBoxCenterText;
        ShowButtonHints = appearance.ShowHints;
        GridCompactMode = appearance.GridCompactMode;
        ShowAssaLayer = appearance.ShowLayer;
        ShowHorizontalLineAboveToolbar = appearance.ShowHorizontalLineAboveToolbar;
        ShowHorizontalLineBelowToolbar = appearance.ShowHorizontalLineBelowToolbar;
        SelectedGridLinesVisibility = GridLinesVisibilities.FirstOrDefault(p => p.Type.ToString() == appearance.GridLinesAppearance) ?? GridLinesVisibilities[0];
        DarkModeBackgroundColor = appearance.DarkModeBackgroundColor.FromHexToColor();
        BookmarkColor = appearance.BookmarkColor.FromHexToColor();
        ShowUpDownStartTime = appearance.ShowUpDownStartTime;
        ShowUpDownEndTime = appearance.ShowUpDownEndTime;
        ShowUpDownDuration = appearance.ShowUpDownDuration;
        ShowUpDownLabels = appearance.ShowUpDownLabels;

        WaveformDrawGridLines = Se.Settings.Waveform.DrawGridLines;
        WaveformFocusOnMouseOver = Se.Settings.Waveform.FocusOnMouseOver;
        WaveformCenterVideoPosition = Se.Settings.Waveform.CenterVideoPosition;
        WaveformShowToolbar = Se.Settings.Waveform.ShowToolbar;
        ShowWaveformVerticalZoom = Se.Settings.Waveform.ShowWaveformVerticalZoom;
        ShowWaveformHorizontalZoom = Se.Settings.Waveform.ShowWaveformHorizontalZoom;
        ShowWaveformVideoPositionSlider = Se.Settings.Waveform.ShowWaveformVideoPositionSlider;
        ShowWaveformPlaybackSpeed = Se.Settings.Waveform.ShowWaveformPlaybackSpeed;
        WaveformFocusTextboxAfterInsertNew = Se.Settings.Waveform.FocusTextBoxAfterInsertNew;
        WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor();
        WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor();
        WaveformCursorColor = Se.Settings.Waveform.WaveformCursorColor.FromHexToColor();
        WaveformInvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel;
        WaveformSnapToShotChanges = Se.Settings.Waveform.SnapToShotChanges;
        WaveformShotChangesAutoGenerate = Se.Settings.Waveform.ShotChangesAutoGenerate;

        ColorDurationTooLong = general.ColorDurationTooLong;
        ColorDurationTooShort = general.ColorDurationTooShort;
        ColorTextTooLong = general.ColorTextTooLong;
        ColorTextTooWide = general.ColorTextTooWide;
        ColorTextTooManyLines = general.ColorTextTooManyLines;
        ColorOverlap = general.ColorTimeCodeOverlap;
        ColorGapTooShort = general.ColorGapTooShort;
        ErrorColor = general.ErrorColor.FromHexToColor();

        _customContinuationStyleSuffix = general.CustomContinuationStyleSuffix;
        _customContinuationStyleSuffixApplyIfComma = general.CustomContinuationStyleSuffixApplyIfComma;
        _customContinuationStyleSuffixAddSpace = general.CustomContinuationStyleSuffixAddSpace;
        _customContinuationStyleSuffixReplaceComma = general.CustomContinuationStyleSuffixReplaceComma;
        _customContinuationStylePrefix = general.CustomContinuationStylePrefix;
        _customContinuationStylePrefixAddSpace = general.CustomContinuationStylePrefixAddSpace;
        _customContinuationStyleUseDifferentStyleGap = general.CustomContinuationStyleUseDifferentStyleGap;
        _continuationPause = general.ContinuationPause;
        _customContinuationStyleGapSuffix = general.CustomContinuationStyleGapSuffix;
        _customContinuationStyleGapSuffixApplyIfComma = general.CustomContinuationStyleGapSuffixApplyIfComma;
        _customContinuationStyleGapSuffixAddSpace = general.CustomContinuationStyleGapSuffixAddSpace;
        _customContinuationStyleGapSuffixReplaceComma = general.CustomContinuationStyleGapSuffixReplaceComma;
        _customContinuationStyleGapPrefix = general.CustomContinuationStyleGapPrefix;
        _customContinuationStyleGapPrefixAddSpace = general.CustomContinuationStyleGapPrefixAddSpace;

        var video = Se.Settings.Video;
        var videoPlayer = VideoPlayers.FirstOrDefault(p => p.Code == video.VideoPlayer);
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
        LoadFileTypeAssociations();

        ExistsErrorLogFile = File.Exists(Se.GetErrorLogFilePath());
        ExistsWhisperLogFile = File.Exists(Se.GetWhisperLogFilePath());
        ExistsSettingsFile = File.Exists(Se.GetSettingsFilePath());
    }

    private void SaveSettings()
    {
        var general = Se.Settings.General;
        var appearance = Se.Settings.Appearance;
        var video = Se.Settings.Video;

        general.SubtitleLineMaximumLength = SingleLineMaxLength ?? general.SubtitleLineMaximumLength;
        general.SubtitleOptimalCharactersPerSeconds = OptimalCharsPerSec ?? general.SubtitleOptimalCharactersPerSeconds;
        general.SubtitleMaximumCharactersPerSeconds = MaxCharsPerSec ?? general.SubtitleMaximumCharactersPerSeconds;
        general.SubtitleMaximumWordsPerMinute = MaxWordsPerMin ?? general.SubtitleMaximumWordsPerMinute;
        general.SubtitleMinimumDisplayMilliseconds = MinDurationMs ?? general.SubtitleMinimumDisplayMilliseconds;
        general.SubtitleMaximumDisplayMilliseconds = MaxDurationMs ?? general.SubtitleMaximumDisplayMilliseconds;
        general.MinimumMillisecondsBetweenLines = MinGapMs ?? general.MinimumMillisecondsBetweenLines;
        general.MaxNumberOfLines = MaxLines ?? general.MaxNumberOfLines;
        general.UnbreakLinesShorterThan = UnbreakLinesShorterThan ?? general.UnbreakLinesShorterThan;
        general.DialogStyle = DialogStyle.Code;
        general.ContinuationStyle = ContinuationStyle.Code;
        general.CpsLineLengthStrategy = CpsLineLengthStrategy.Code;

        general.UseFrameMode = UseFrameMode;
        general.NewEmptyDefaultMs = NewEmptyDefaultMs ?? general.NewEmptyDefaultMs;
        general.PromptDeleteLines = PromptDeleteLines;
        general.LockTimeCodes = LockTimeCodes;
        general.RememberPositionAndSize = RememberPositionAndSize;
        general.AutoBackupOn = AutoBackupOn;
        general.AutoBackupIntervalMinutes = AutoBackupIntervalMinutes ?? general.AutoBackupIntervalMinutes;
        general.AutoBackupDeleteAfterMonths = AutoBackupDeleteAfterMonths ?? general.AutoBackupDeleteAfterMonths;
        general.DefaultEncoding = DefaultEncoding?.DisplayName ?? Encodings.First().DisplayName;
        general.DefaultSubtitleFormat = SelectedDefaultSubtitleFormat;

        Se.Settings.Tools.GoToLineNumberAlsoSetVideoPosition = GoToLineNumberAlsoSetVideoPosition;

        appearance.Theme = SelectedTheme;
        appearance.FontName = SelectedFontName == FontNames.First()
                                ? new Label().FontFamily.Name
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
        appearance.SubtitleGridFontSize = SubtitleGridFontSize;
        appearance.SubtitleTextBoxAndGridFontName = string.IsNullOrEmpty(SubtitleTextBoxAndGridFontName) ? new Label().FontFamily.Name : SubtitleTextBoxAndGridFontName;
        appearance.SubtitleTextBoxFontSize = TextBoxFontSize;
        appearance.SubtitleTextBoxFontBold = TextBoxFontBold;
        appearance.SubtitleTextBoxCenterText = TextBoxCenterText;
        appearance.ShowHints = ShowButtonHints;
        appearance.DarkModeBackgroundColor = DarkModeBackgroundColor.FromColorToHex();
        appearance.BookmarkColor = BookmarkColor.FromColorToHex();
        appearance.ShowUpDownStartTime = ShowUpDownStartTime;
        appearance.ShowUpDownEndTime = ShowUpDownEndTime;
        appearance.ShowUpDownDuration = ShowUpDownDuration;
        appearance.ShowUpDownLabels = ShowUpDownLabels;
        appearance.GridCompactMode = GridCompactMode;
        appearance.GridLinesAppearance = SelectedGridLinesVisibility.Type.ToString();
        appearance.ShowLayer = ShowAssaLayer;
        appearance.ShowHorizontalLineAboveToolbar = ShowHorizontalLineAboveToolbar;
        appearance.ShowHorizontalLineBelowToolbar = ShowHorizontalLineBelowToolbar;

        Se.Settings.Waveform.DrawGridLines = WaveformDrawGridLines;
        Se.Settings.Waveform.FocusOnMouseOver = WaveformFocusOnMouseOver;
        Se.Settings.Waveform.CenterVideoPosition = WaveformCenterVideoPosition;
        Se.Settings.Waveform.FocusTextBoxAfterInsertNew = WaveformFocusTextboxAfterInsertNew;
        Se.Settings.Waveform.ShowToolbar = WaveformShowToolbar;
        Se.Settings.Waveform.ShowWaveformVerticalZoom = ShowWaveformVerticalZoom;
        Se.Settings.Waveform.ShowWaveformHorizontalZoom = ShowWaveformHorizontalZoom;
        Se.Settings.Waveform.ShowWaveformVideoPositionSlider = ShowWaveformVideoPositionSlider;
        Se.Settings.Waveform.ShowWaveformPlaybackSpeed = ShowWaveformPlaybackSpeed;
        Se.Settings.Waveform.WaveformColor = WaveformColor.FromColorToHex();
        Se.Settings.Waveform.WaveformSelectedColor = WaveformSelectedColor.FromColorToHex();
        Se.Settings.Waveform.WaveformCursorColor = WaveformCursorColor.FromColorToHex();
        Se.Settings.Waveform.InvertMouseWheel = WaveformInvertMouseWheel;
        Se.Settings.Waveform.SnapToShotChanges = WaveformSnapToShotChanges;
        Se.Settings.Waveform.ShotChangesAutoGenerate = WaveformShotChangesAutoGenerate;

        general.ColorDurationTooLong = ColorDurationTooLong;
        general.ColorDurationTooShort = ColorDurationTooShort;
        general.ColorTextTooLong = ColorTextTooLong;
        general.ColorTextTooWide = ColorTextTooWide;
        general.ColorTextTooManyLines = ColorTextTooManyLines;
        general.ColorTimeCodeOverlap = ColorOverlap;
        general.ColorGapTooShort = ColorGapTooShort;
        general.ErrorColor = ErrorColor.FromColorToHex();

        general.CustomContinuationStyleSuffix = _customContinuationStyleSuffix;
        general.CustomContinuationStyleSuffixApplyIfComma = _customContinuationStyleSuffixApplyIfComma;
        general.CustomContinuationStyleSuffixAddSpace = _customContinuationStyleSuffixAddSpace;
        general.CustomContinuationStyleSuffixReplaceComma = _customContinuationStyleSuffixReplaceComma;
        general.CustomContinuationStylePrefix = _customContinuationStylePrefix;
        general.CustomContinuationStylePrefixAddSpace = _customContinuationStylePrefixAddSpace;
        general.CustomContinuationStyleUseDifferentStyleGap = _customContinuationStyleUseDifferentStyleGap;
        general.ContinuationPause = _continuationPause;
        general.CustomContinuationStyleGapSuffix = _customContinuationStyleGapSuffix;
        general.CustomContinuationStyleGapSuffixApplyIfComma = _customContinuationStyleGapSuffixApplyIfComma;
        general.CustomContinuationStyleGapSuffixAddSpace = _customContinuationStyleGapSuffixAddSpace;
        general.CustomContinuationStyleGapSuffixReplaceComma = _customContinuationStyleGapSuffixReplaceComma;
        general.CustomContinuationStyleGapPrefix = _customContinuationStyleGapPrefix;
        general.CustomContinuationStyleGapPrefixAddSpace = _customContinuationStyleGapPrefixAddSpace;

        video.VideoPlayer = SelectedVideoPlayer.Code;
        video.ShowStopButton = ShowStopButton;
        video.ShowFullscreenButton = ShowFullscreenButton;
        video.AutoOpen = AutoOpenVideoFile;

        general.FfmpegPath = FfmpegPath;
        general.LibMpvPath = LibMpvPath;

        general.CurrentProfile = SelectedProfile;
        general.Profiles.Clear();
        foreach (var profile in _profilesForEdit)
        {
            var p = new RulesProfile
            {
                Name = profile.Name,
                SubtitleLineMaximumLength = profile.SingleLineMaxLength ?? general.SubtitleLineMaximumLength,
                SubtitleOptimalCharactersPerSeconds = (decimal?)profile.OptimalCharsPerSec ?? (decimal)general.SubtitleOptimalCharactersPerSeconds,
                SubtitleMaximumCharactersPerSeconds = (decimal?)profile.MaxCharsPerSec ?? (decimal)general.SubtitleMaximumCharactersPerSeconds,
                SubtitleMaximumWordsPerMinute = (decimal?)profile.MaxWordsPerMin ?? (decimal)general.SubtitleMaximumWordsPerMinute,
                SubtitleMinimumDisplayMilliseconds = profile.MinDurationMs ?? general.SubtitleMinimumDisplayMilliseconds,
                SubtitleMaximumDisplayMilliseconds = profile.MaxDurationMs ?? general.SubtitleMaximumDisplayMilliseconds,
                MinimumMillisecondsBetweenLines = profile.MinGapMs ?? general.MinimumMillisecondsBetweenLines,
                MaxNumberOfLines = profile.MaxLines ?? general.MaxNumberOfLines,
                MergeLinesShorterThan = profile.UnbreakLinesShorterThan ?? general.UnbreakLinesShorterThan,
                DialogStyle = Enum.Parse<DialogType>(profile.DialogStyle.Code),
                ContinuationStyle = Enum.TryParse<ContinuationStyle>(profile.ContinuationStyle.Code, out var cs) ? cs : Core.Enums.ContinuationStyle.None,
                CpsLineLengthStrategy = profile.CpsLineLengthStrategy.Code
            };
            general.Profiles.Add(p);
        }

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
    private async Task EmptyWaveformsAndSpectrograms()
    {
        var answer = MessageBoxResult.Yes;
        if (Se.Settings.General.PromptDeleteLines)
        {
            answer = await MessageBox.Show(
                Window!,
                Se.Language.General.Delete,
                Se.Language.Options.Settings.DeleteWaveformAndSpectrogramFoldersQuestion,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
        }
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        List<string> files = GetWaveformAndSpecgtrogramFiles();
        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // ignore
            }
        }

        _ = UpdateWaveformSpaceInfoAsync();
    }

    private static List<string> GetWaveformAndSpecgtrogramFiles()
    {
        var files = Directory.GetFiles(Se.WaveformsFolder, "*.wav").ToList();
        files.AddRange(Directory.GetFiles(Se.SpectrogramsFolder, "*.png"));
        return files;
    }

    private async Task UpdateWaveformSpaceInfoAsync()
    {
        var files = GetWaveformAndSpecgtrogramFiles();

        long totalBytes = await Task.Run(() =>
        {
            long total = 0;
            foreach (var file in files)
            {
                try
                {
                    total += new FileInfo(file).Length;
                }
                catch
                {
                    // ignore
                }
            }

            return total;
        });

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            WaveformSpaceInfo = string.Format(
                Se.Language.Options.Settings.WaveFormsAndSpectrogramFoldersContainsX,
                Utilities.FormatBytesToDisplayFileSize(totalBytes)
            );
        });
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
    private async Task ResetAllSettings()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<SettingsResetWindow, SettingsResetViewModel>(Window!);
        if (!result.OkPressed)
        {
            return;
        }

        if (result.ResetAll)
        {
            Se.Settings = new Se();
        }
        else
        {
            if (result.ResetRecentFiles)
            {
                Se.Settings.File.RecentFiles = new List<RecentFile>();
            }
            if (result.ResetWindowPositionAndSize)
            {
                Se.Settings.General.WindowPositions = new List<SeWindowPosition>();
            }
            if (result.ResetShortcuts)
            {
                Se.Settings.Shortcuts = new List<SeShortCut>();
            }
            if (result.ResetMultipleReplaceRules)
            {
                Se.Settings.Edit.MultipleReplace = new SeEditMultipleReplace();
            }
            if (result.ResetRules)
            {
                var g = new SeGeneral();
                Se.Settings.General.Profiles = g.Profiles;
                Se.Settings.General.CurrentProfile = g.CurrentProfile;
                Se.Settings.General.SubtitleLineMaximumLength = g.SubtitleLineMaximumLength;
                Se.Settings.General.SubtitleOptimalCharactersPerSeconds = g.SubtitleOptimalCharactersPerSeconds;
                Se.Settings.General.SubtitleMaximumCharactersPerSeconds = g.SubtitleMaximumCharactersPerSeconds;
                Se.Settings.General.SubtitleMaximumWordsPerMinute = g.SubtitleMaximumWordsPerMinute;
                Se.Settings.General.SubtitleMinimumDisplayMilliseconds = g.SubtitleMinimumDisplayMilliseconds;
                Se.Settings.General.SubtitleMaximumDisplayMilliseconds = g.SubtitleMaximumDisplayMilliseconds;
                Se.Settings.General.MinimumMillisecondsBetweenLines = g.MinimumMillisecondsBetweenLines;
                Se.Settings.General.MaxNumberOfLines = g.MaxNumberOfLines;
                Se.Settings.General.UnbreakLinesShorterThan = g.UnbreakLinesShorterThan;
                Se.Settings.General.DialogStyle = g.DialogStyle;
                Se.Settings.General.ContinuationStyle = g.ContinuationStyle;
                Se.Settings.General.CpsLineLengthStrategy = g.CpsLineLengthStrategy;
            }
            if (result.ResetAppearance)
            {
                Se.Settings.Appearance = new SeAppearance();
            }
            if (result.ResetAutoTranslate)
            {
                Se.Settings.AutoTranslate = new SeAutoTranslate();
            }
            if (result.ResetWaveform)
            {
                Se.Settings.Waveform = new SeWaveform();
            }
            if (result.ResetSyntaxColoring)
            {
                var g = new SeGeneral();

                Se.Settings.General.ColorDurationTooLong = Se.Settings.General.ColorDurationTooLong;
                Se.Settings.General.ColorDurationTooShort = g.ColorDurationTooShort;
                Se.Settings.General.ColorTextTooLong = g.ColorTextTooLong;
                Se.Settings.General.ColorTextTooWide = g.ColorTextTooWide;
                Se.Settings.General.ColorTextTooManyLines = g.ColorTextTooManyLines;
                Se.Settings.General.ColorTimeCodeOverlap = g.ColorTimeCodeOverlap;
                Se.Settings.General.ColorGapTooShort = g.ColorGapTooShort;
                Se.Settings.General.ErrorColor = g.ErrorColor;
            }
        }

        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void CommandOk()
    {
        SaveSettings();
        SaveFileTypeAssociations();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Apply()
    {
        SaveSettings();
        SaveFileTypeAssociations();
        _mainViewModel?.ApplySettings();
    }

    [RelayCommand]
    private async Task EditProfiles()
    {
        if (Window == null)
        {
            return;
        }

        var currentProfile = _profilesForEdit.FirstOrDefault(p => p.Name == SelectedProfile);
        if (currentProfile != null)
        {
            currentProfile.SingleLineMaxLength = SingleLineMaxLength;
            currentProfile.OptimalCharsPerSec = OptimalCharsPerSec;
            currentProfile.MaxCharsPerSec = MaxCharsPerSec;
            currentProfile.MaxWordsPerMin = MaxWordsPerMin;
            currentProfile.MinDurationMs = MinDurationMs;
            currentProfile.MaxDurationMs = MaxDurationMs;
            currentProfile.MinGapMs = MinGapMs;
            currentProfile.MaxLines = MaxLines;
            currentProfile.UnbreakLinesShorterThan = UnbreakLinesShorterThan;
            currentProfile.DialogStyle = DialogStyle;
            currentProfile.ContinuationStyle = ContinuationStyle;
            currentProfile.CpsLineLengthStrategy = CpsLineLengthStrategy;
        }

        var result = await _windowService
            .ShowDialogAsync<ProfilesWindow, ProfilesViewModel>(Window, vm =>
            {
                vm.Initialize(_profilesForEdit, SelectedProfile);
            });


        if (!result.OkPressed)
        {
            return;
        }

        var oldProfileName = SelectedProfile;
        _profilesForEdit = result.Profiles.ToList();

        if (_profilesForEdit.Count == 0)
        {
            var g = new SeGeneral();
            var pd = new ProfileDisplay
            {
                Name = "Default",
                SingleLineMaxLength = g.SubtitleLineMaximumLength,
                OptimalCharsPerSec = (double)g.SubtitleOptimalCharactersPerSeconds,
                MaxCharsPerSec = (double)g.SubtitleMaximumCharactersPerSeconds,
                MaxWordsPerMin = (double)g.SubtitleMaximumWordsPerMinute,
                MinDurationMs = g.SubtitleMinimumDisplayMilliseconds,
                MaxDurationMs = g.SubtitleMaximumDisplayMilliseconds,
                MinGapMs = g.MinimumMillisecondsBetweenLines,
                MaxLines = g.MaxNumberOfLines,
                UnbreakLinesShorterThan = g.UnbreakLinesShorterThan,
                DialogStyle = DialogStyles.FirstOrDefault(p => p.Code == g.DialogStyle) ?? DialogStyles.First(),
                ContinuationStyle = ContinuationStyles.FirstOrDefault(p => p.Code == g.ContinuationStyle) ?? ContinuationStyles.First(),
                CpsLineLengthStrategy = CpsLineLengthStrategies.FirstOrDefault(p => p.Code == g.CpsLineLengthStrategy) ?? CpsLineLengthStrategies.First()
            };
            _profilesForEdit.Add(pd);
        }

        Profiles.Clear();
        var profilesForEdit = new List<ProfileDisplay>();
        foreach (var profile in _profilesForEdit)
        {
            var pd = new ProfileDisplay(profile);
            pd.DialogStyle = DialogStyles.FirstOrDefault(p => p.Code == profile.DialogStyle.Code) ?? DialogStyles.First();
            pd.ContinuationStyle = ContinuationStyles.FirstOrDefault(p => p.Code == profile.ContinuationStyle.Code) ?? ContinuationStyles.First();
            pd.CpsLineLengthStrategy = CpsLineLengthStrategies.FirstOrDefault(p => p.Code == profile.CpsLineLengthStrategy.Code) ?? CpsLineLengthStrategies.First();
            profilesForEdit.Add(pd);
            Profiles.Add(profile.Name);
        }
        _profilesForEdit = profilesForEdit;

        SelectedProfile = Profiles.FirstOrDefault(p => p == oldProfileName) ?? Profiles.First();
    }

    [RelayCommand]
    private async Task ShowErrorLogFile()
    {
        if (Window == null || !File.Exists(Se.GetErrorLogFilePath()))
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window!, Se.GetErrorLogFilePath());
    }

    [RelayCommand]
    private async Task ShowWhisperLogFile()
    {
        if (Window == null || !File.Exists(Se.GetWhisperLogFilePath()))
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window!, Se.GetWhisperLogFilePath());
    }

    [RelayCommand]
    private async Task ShowSettingsFile()
    {
        if (Window == null || !File.Exists(Se.GetSettingsFilePath()))
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window!, Se.GetSettingsFilePath());
    }

    [RelayCommand]
    private async Task ShowEditCustomContinuationStyle()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService
             .ShowDialogAsync<CustomContinuationStyleWindow, CustomContinuationStyleViewModel>(Window, vm =>
             {
                 vm.Initialize(
                    _continuationPause,
                    _customContinuationStyleSuffix,
                    _customContinuationStyleSuffixApplyIfComma,
                    _customContinuationStyleSuffixAddSpace,
                    _customContinuationStyleSuffixReplaceComma,
                    _customContinuationStylePrefix,
                    _customContinuationStylePrefixAddSpace,
                    _customContinuationStyleUseDifferentStyleGap,
                    _customContinuationStyleGapSuffix,
                    _customContinuationStyleGapSuffixApplyIfComma,
                    _customContinuationStyleGapSuffixAddSpace,
                    _customContinuationStyleGapSuffixReplaceComma,
                    _customContinuationStyleGapPrefix,
                    _customContinuationStyleGapPrefixAddSpace);
             });

        if (!result.OkPressed)
        {
            return;
        }

        _customContinuationStyleSuffix = result.SelectedSuffix;
        _customContinuationStyleSuffixApplyIfComma = result.SelectedSuffixesProcessIfEndWithComma;
        _customContinuationStyleSuffixAddSpace = result.SelectedSuffixesAddSpace;
        _customContinuationStyleSuffixReplaceComma = result.SelectedSuffixesRemoveComma;
        _customContinuationStylePrefix = result.SelectedPrefix;
        _customContinuationStylePrefixAddSpace = result.SelectedPrefixAddSpace;
        _customContinuationStyleUseDifferentStyleGap = result.UseSpecialStyleAfterLongGaps;
        _continuationPause = result.LongGapMs;
        _customContinuationStyleGapSuffix = result.SelectedLongGapSuffix;
        _customContinuationStyleGapSuffixApplyIfComma = result.SelectedLongGapSuffixesProcessIfEndWithComma;
        _customContinuationStyleGapSuffixAddSpace = result.SelectedLongGapSuffixesAddSpace;
        _customContinuationStyleGapSuffixReplaceComma = result.SelectedLongGapSuffixesRemoveComma;
        _customContinuationStyleGapPrefix = result.SelectedLongGapPrefix;
        _customContinuationStyleGapPrefixAddSpace = result.SelectedLongGapPrefixAddSpace;

        RuleValueChanged();
    }

    private void LoadFileTypeAssociations()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var folder = Path.Combine(Se.DataFolder, "FileTypes");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var iconFileNames = Directory.GetFiles(folder, "*.ico");

        foreach (var item in FileTypeAssociations)
        {
            item.IsAssociated = FileTypeAssociationsHelper.GetChecked(item.Extension, "SubtitleEdit5");
        }
    }

    private void SaveFileTypeAssociations()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var exeFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(exeFileName) || !File.Exists(exeFileName))
        {
            return;
        }

        foreach (var item in FileTypeAssociations)
        {
            var ext = item.Extension;
            if (item.IsAssociated)
            {
                var avaResIconPath = item.IconPath;
                var folder = Path.Combine(Se.DataFolder, "FileTypes");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var iconFileName = Path.Combine(folder, ext.TrimStart('.') + ".ico");
                if (!File.Exists(iconFileName))
                {
                    try
                    {
                        var uri = new Uri(avaResIconPath);
                        using var stream = AssetLoader.Open(uri);
                        if (stream != null)
                        {
                            // If the source is already an ICO file, just copy it
                            if (avaResIconPath.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                            {
                                using var fileStream = new FileStream(iconFileName, FileMode.Create, FileAccess.Write);
                                stream.CopyTo(fileStream);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Se.LogError(exception, "SaveFileTypeAssociations");
                    }
                }
                FileTypeAssociationsHelper.SetFileAssociationViaRegistry(ext, exeFileName, iconFileName, "SubtitleEdit5");
            }
            else
            {
                FileTypeAssociationsHelper.DeleteFileAssociationViaRegistry(ext, "SubtitleEdit5");
            }
        }

        FileTypeAssociationsHelper.Refresh();
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

    internal void Initialize(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    internal void ProfileChanged()
    {
        var profile = _profilesForEdit.FirstOrDefault(p => p.Name == SelectedProfile);
        if (profile == null)
        {
            return;
        }

        SingleLineMaxLength = profile.SingleLineMaxLength;
        OptimalCharsPerSec = profile.OptimalCharsPerSec;
        MaxCharsPerSec = profile.MaxCharsPerSec;
        MaxWordsPerMin = profile.MaxWordsPerMin;
        MinDurationMs = profile.MinDurationMs;
        MaxDurationMs = profile.MaxDurationMs;
        MinGapMs = profile.MinGapMs;
        MaxLines = profile.MaxLines;
        UnbreakLinesShorterThan = profile.UnbreakLinesShorterThan;
        DialogStyle = profile.DialogStyle;
        ContinuationStyle = profile.ContinuationStyle;
        CpsLineLengthStrategy = profile.CpsLineLengthStrategy;
    }

    internal void RuleValueChanged()
    {
        var profileItem = _profilesForEdit.FirstOrDefault(p => p.Name == SelectedProfile);
        if (profileItem == null)
        {
            return;
        }

        profileItem.SingleLineMaxLength = SingleLineMaxLength;
        profileItem.OptimalCharsPerSec = OptimalCharsPerSec;
        profileItem.MaxCharsPerSec = MaxCharsPerSec;
        profileItem.MaxWordsPerMin = MaxWordsPerMin;
        profileItem.MinDurationMs = MinDurationMs;
        profileItem.MaxDurationMs = MaxDurationMs;
        profileItem.MinGapMs = MinGapMs;
        profileItem.MaxLines = MaxLines;
        profileItem.UnbreakLinesShorterThan = UnbreakLinesShorterThan;
        profileItem.DialogStyle = DialogStyle;
        profileItem.ContinuationStyle = ContinuationStyle;
        profileItem.CpsLineLengthStrategy = CpsLineLengthStrategy;
    }

    internal void ContinuationStyleChanged()
    {
        if (ContinuationStyle == null)
        {
            IsEditCustomContinuationStyleVisible = false;
            return;
        }

        IsEditCustomContinuationStyleVisible = ContinuationStyle.Code == nameof(Core.Enums.ContinuationStyle.Custom);
        RuleValueChanged();
    }

    public void VideoPlayerChanged()
    {
        IsMpvChosen = SelectedVideoPlayer.Name == "mpv";
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        UiUtil.RestoreWindowPosition(Window);
        _ = UpdateWaveformSpaceInfoAsync();
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
    }
}