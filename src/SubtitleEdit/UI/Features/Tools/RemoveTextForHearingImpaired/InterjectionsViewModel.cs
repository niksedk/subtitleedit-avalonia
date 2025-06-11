using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

public partial class InterjectionsViewModel : ObservableObject
{
    [ObservableProperty] private string _interjectionsText;
    [ObservableProperty] private string _interjectionsSkipStartText;
    
    public InterjectionsWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public InterjectionsViewModel()
    {
        InterjectionsText = string.Empty;
        InterjectionsSkipStartText = string.Empty;
    }
    
    [RelayCommand]                   
    private void Ok() 
    {
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void Cancel() 
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    public void Initialize(RemoveTextForHearingImpairedViewModel.LanguageItem? selectedLanguage)
    {
        //throw new System.NotImplementedException();
    }
}