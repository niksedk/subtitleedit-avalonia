using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;

public partial class BinaryEditViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName;
    
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Grid ContentGrid { get; set; }

    public BinaryEditViewModel()
    {
        ContentGrid = new Grid();
        FileName = string.Empty;    
    }

    public void Initialize(string fileName)
    {
    }

    [RelayCommand]                   
    private void Ok() 
    {
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void Cancel() 
    {
        Window?.Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (e.Key == Key.Enter)
        {
            Ok();
        }
    }
}