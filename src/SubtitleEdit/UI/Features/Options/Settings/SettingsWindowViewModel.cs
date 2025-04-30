using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class SettingsWindowViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> languages;
    [ObservableProperty] private string selectedLanguage;

    [ObservableProperty] private ObservableCollection<string> themes;
    [ObservableProperty] private string selectedTheme;
    
    public SettingsWindowViewModel()
    {
        Languages = new ObservableCollection<string> { "English", "Danish", "Spanish" };
        SelectedLanguage = Languages[0];
        
        Themes = new ObservableCollection<string> { "Light", "Dark" };
        SelectedTheme = Themes[0];
    }
}