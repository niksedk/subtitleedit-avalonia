using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class SettingsPageViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> languages;
    [ObservableProperty] private string selectedLanguage;

    [ObservableProperty] private ObservableCollection<string> themes;
    [ObservableProperty] private string selectedTheme;
    
    public SettingsPageViewModel()
    {
        Languages = new ObservableCollection<string> { "English", "Danish", "Spanish" };
        SelectedLanguage = Languages[0];
        
        Themes = new ObservableCollection<string> { "Light", "Dark" };
        SelectedTheme = Themes[0];
    }
}