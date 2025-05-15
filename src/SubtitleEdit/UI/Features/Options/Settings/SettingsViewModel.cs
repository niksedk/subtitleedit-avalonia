using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> languages;
    [ObservableProperty] private string selectedLanguage;

    [ObservableProperty] private ObservableCollection<string> _themes;
    [ObservableProperty] private string _selectedTheme;
    [ObservableProperty] private int _singleLineMaxLength;
    [ObservableProperty] private double _optimalCharsPerSec;
    [ObservableProperty] private double _maxCharsPerSec;
    [ObservableProperty] private double _maxWordsPerMin;
    [ObservableProperty] private int _minDurationMs;
    [ObservableProperty] private int _maxDurationMs;
    [ObservableProperty] private int _minGapMs;
    [ObservableProperty] private int _maxLines;
    [ObservableProperty] private int _unbreakShorterThanMs;
    
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

    [ObservableProperty] private bool _usePlayerMpv;
    [ObservableProperty] private bool _usePlayerVlc;


    public ObservableCollection<FormatViewModel> AvailableFormats { get; set; } = new()
    {
        new FormatViewModel { Name = "SRT", IsFavorite = true },
        new FormatViewModel { Name = "ASS", IsFavorite = false },
        new FormatViewModel { Name = "VTT", IsFavorite = false },
        new FormatViewModel { Name = "SUB", IsFavorite = false },
        new FormatViewModel { Name = "TXT", IsFavorite = false }
    };

    public ObservableCollection<FileTypeAssociationViewModel> FileTypeAssociations { get; set; } = new()
    {
        new() { Extension = ".ass", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/ass.ico" },
        new() { Extension = ".dfxp", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/dfxp.ico" },
        new() { Extension = ".itt", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/itt.ico" },
        new() { Extension = ".lrc", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/lrc.ico" },
        new() { Extension = ".sbv", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/sbv.ico" },
        new() { Extension = ".smi", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/smi.ico" },
        new() { Extension = ".srt", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/srt.ico" },
        new() { Extension = ".ssa", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/ssa.ico" },
        new() { Extension = ".stl", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/stl.ico" },
        new() { Extension = ".sub", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/sub.ico" },
        new() { Extension = ".vtt", IconPath = "avares://Nikse.SubtitleEdit/Assets/FileTypes/vtt.ico" },
    };

    public FormatViewModel DefaultFormat { get; set; }
    public FormatViewModel DefaultSaveAsFormat { get; set; }

    public IList<string> FavoriteFormatSelections { get; set; } = new ObservableCollection<string>();

    public bool OkPressed { get; set; }
    public SettingsWindow? Window { get; internal set; }
    public ScrollViewer ScrollView { get; internal set; }
    public List<SettingsSection> Sections { get; internal set; }

    public SettingsViewModel()
    {
        Languages = new ObservableCollection<string> { "English", "Danish", "Spanish" };
        SelectedLanguage = Languages[0];

        Themes = new ObservableCollection<string> { "Light", "Dark" };
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

        SelectedTheme = appearance.Theme;
        ShowToolbarNew = appearance.ToolbarShowFileNew;
        ShowToolbarOpen = appearance.ToolbarShowFileOpen;
        ShowToolbarSave =  appearance.ToolbarShowSave;
        ShowToolbarSaveAs = appearance.ToolbarShowSaveAs;
        ShowToolbarFind = appearance.ToolbarShowFind;
        ShowToolbarReplace = appearance.ToolbarShowReplace;
        ShowToolbarSpellCheck = appearance.ToolbarShowSpellCheck;
        ShowToolbarSettings = appearance.ToolbarShowSettings;
        ShowToolbarLayout = appearance.ToolbarShowLayout;
        ShowToolbarHelp = appearance.ToolbarShowHelp;
        ShowToolbarEncoding = appearance.ToolbarShowEncoding;
        
        ColorDurationTooLong = general.ColorDurationTooLong;
        ColorDurationTooShort = general.ColorDurationTooShort;
        ColorTextTooLong = general.ColorTextTooLong;
        ColorTextTooWide = general.ColorTextTooWide;
        ColorTextTooManyLines = general.ColorTextTooManyLines;
        ColorOverlap = general.ColorTimeCodeOverlap;
        ColorGapTooShort = general.ColorGapTooShort;
    } 

    private void SaveSettings()
    {
        var general = Se.Settings.General;
        var appearance = Se.Settings.Appearance;

        general.SubtitleLineMaximumLength = SingleLineMaxLength;
        general.SubtitleOptimalCharactersPerSeconds = OptimalCharsPerSec;
        general.SubtitleMaximumCharactersPerSeconds = MaxCharsPerSec;
        general.SubtitleMaximumWordsPerMinute = MaxWordsPerMin;
        general.SubtitleMaximumDisplayMilliseconds = MaxDurationMs; // Assuming MinDurationMs and MaxDurationMs are the same source
        general.MinimumMillisecondsBetweenLines = MinGapMs;
        general.MaxNumberOfLines = MaxLines;
        general.MergeLinesShorterThan = UnbreakShorterThanMs;

        appearance.Theme = SelectedTheme;
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
        
        general.ColorDurationTooLong = ColorDurationTooLong;
        general.ColorDurationTooShort = ColorDurationTooShort;
        general.ColorTextTooLong = ColorTextTooLong;
        general.ColorTextTooWide = ColorTextTooWide;
        general.ColorTextTooManyLines = ColorTextTooManyLines;
        general.ColorTimeCodeOverlap = ColorOverlap;
        general.ColorGapTooShort = ColorGapTooShort;
        
        Se.SaveSettings();
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
            //await FadeToAsync(ScrollView, 1, TimeSpan.FromMilliseconds(100));
            await RunFadeAnimation(ScrollView, from: 0, to: 1, TimeSpan.FromMilliseconds(200));

        }, DispatcherPriority.Background);
    }

    private static async Task FadeToAsync(Control control, double targetOpacity, TimeSpan duration)
    {
        double startOpacity = control.Opacity;
        double delta = targetOpacity - startOpacity;
        const int fps = 60;
        int steps = (int)(duration.TotalSeconds * fps);
        if (steps <= 0) steps = 1;

        for (int i = 0; i <= steps; i++)
        {
            double progress = (double)i / steps;
            control.Opacity = startOpacity + (delta * progress);
            await Task.Delay((int)(1000.0 / fps));
        }

        control.Opacity = targetOpacity;
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
                    Setters = { new Setter(Control.OpacityProperty, from) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(Control.OpacityProperty, to) }
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