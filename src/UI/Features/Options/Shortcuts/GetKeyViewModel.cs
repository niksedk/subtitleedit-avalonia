using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class GetKeyViewModel : ObservableObject
{
    [ObservableProperty] private string _infoText;
    
    public Window? Window { get; set; }
    
    public bool OkPressed { get; private set; }
    public string PressedKey { get; private set; }

    public GetKeyViewModel()
    {
        InfoText = "Press a key";
        PressedKey = string.Empty;
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
        else
        {
            PressedKey = e.Key.ToString();
            InfoText = string.Format(Se.Language.Options.Shortcuts.PressedKeyX,  PressedKey);
        }
    }
}