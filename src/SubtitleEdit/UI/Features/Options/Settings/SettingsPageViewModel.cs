using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class SettingsPageViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> languages;
    [ObservableProperty] private string selectedLanguage;
    
    public SettingsPageViewModel()
    {
        Languages = new ObservableCollection<string> { "English", "Danish", "Spanish" };
        SelectedLanguage = Languages[0];
    }
}