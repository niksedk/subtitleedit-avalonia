using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> languages;
    [ObservableProperty] private string selectedLanguage;
    
    public BurnInWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public BurnInViewModel()
    {
        Languages = new ObservableCollection<string> { "English", "Danish", "Spanish" };
        SelectedLanguage = Languages[0];
    }
    
    [RelayCommand]                   
    private void Help() 
    {
    }
    
    [RelayCommand]                   
    private void Generate() 
    {
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
}