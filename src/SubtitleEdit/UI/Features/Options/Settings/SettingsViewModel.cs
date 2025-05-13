using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> languages;
    [ObservableProperty] private string selectedLanguage;

    [ObservableProperty] private ObservableCollection<string> themes;
    [ObservableProperty] private string selectedTheme;
    internal object SingleLineMaxLength;
    internal object OptimalCharsPerSec;
    internal object MaxCharsPerSec;
    internal object MaxWordsPerMin;
    internal object MinDurationMs;
    internal object MaxDurationMs;
    internal object MinGapMs;
    internal object MaxLines;
    internal object UnbreakShorterThanMs;
    internal object ShowToolbarNew;
    internal object ShowToolbarOpen;
    internal object ShowToolbarSave;
    internal object ShowToolbarSaveAs;

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
        SelectedTheme = Se.Settings.Appearance.Theme;
    }

    public static void ScrollElementIntoView(ScrollViewer scrollViewer, Control target)
    {
        if (scrollViewer == null || target == null)
        {
            return;
        }

        // Translate target's position to ScrollViewer's coordinate space
        var targetPosition = target.TranslatePoint(new Point(0, 0), scrollViewer);

        if (targetPosition.HasValue)
        {
            // Scroll to that Y offset
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, targetPosition.Value.Y);
        }
    }

    [RelayCommand]
    private void ScrollToSection()
    {
        ScrollElementIntoView(ScrollView, Sections[1].Panel!);
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