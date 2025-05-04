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
    
    public bool OkPressed { get; set; }
    public SettingsWindow? Window { get; internal set; }
    public ScrollViewer ScrollView { get; internal set; }
    public List<SettingsSection> Sections { get; internal set; }

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