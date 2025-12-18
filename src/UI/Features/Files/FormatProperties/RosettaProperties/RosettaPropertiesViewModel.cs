using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.RosettaProperties;

public partial class RosettaPropertiesViewModel : ObservableObject
{
    [ObservableProperty] private string _language;
    [ObservableProperty] private bool _languageAutoDetect;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private string _selectedFontSize;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public RosettaPropertiesViewModel()
    {
        Language = string.Empty;
        LanguageAutoDetect = true;
        SelectedFontName = string.Empty;
        SelectedFontSize = string.Empty;
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