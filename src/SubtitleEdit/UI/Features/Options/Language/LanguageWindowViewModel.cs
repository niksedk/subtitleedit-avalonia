using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Options.Language;

public partial class LanguageWindowViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> languages;
    [ObservableProperty] private string selectedLanguage;
    
    public LanguageWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public LanguageWindowViewModel()
    {
        Languages = new ObservableCollection<string> { "English", "Danish", "Spanish" };
        SelectedLanguage = Languages[0];
    }
    
    [RelayCommand]                   
    private void CommandOk() 
    {
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void CommandCancel() 
    {
        Window?.Close();
    }
}