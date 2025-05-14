using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
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
    [ObservableProperty] private double _singleLineMaxLength;
    [ObservableProperty] private double _optimalCharsPerSec;
    [ObservableProperty] private double _maxCharsPerSec;
    [ObservableProperty] private double _maxWordsPerMin;
    [ObservableProperty] private double _minDurationMs;
    [ObservableProperty] private double _maxDurationMs;
    [ObservableProperty] private double _minGapMs;
    [ObservableProperty] private double _maxLines;
    [ObservableProperty] private double _unbreakShorterThanMs;
    [ObservableProperty] private double _showToolbarNew;
    [ObservableProperty] private double _showToolbarOpen;
    [ObservableProperty] private double _showToolbarSave;
    [ObservableProperty] private double _showToolbarSaveAs;

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
    public object ShowToolbarFind { get; internal set; }

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
    }

    public static async void ScrollElementIntoView(ScrollViewer scrollViewer, Control target)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // Wait for the layout pass to complete
            await Task.Yield(); // Ensures target has been laid out

            var targetPosition = target.TranslatePoint(new Point(0, 0), scrollViewer);
            if (targetPosition.HasValue)
            {
                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, targetPosition.Value.Y);
            }
        }, DispatcherPriority.Background);
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
        Se.Settings.Appearance.Theme = SelectedTheme;
        Se.SaveSettings();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void CommandCancel()
    {
        Window?.Close();
    }
}