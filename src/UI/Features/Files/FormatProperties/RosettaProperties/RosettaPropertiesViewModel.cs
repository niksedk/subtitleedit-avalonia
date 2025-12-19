using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.RosettaProperties;

public partial class RosettaPropertiesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _languages;
    [ObservableProperty] private string _selectedLanguage;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private string _selectedFontSize;
    [ObservableProperty] private string _selectedLineHeight;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public RosettaPropertiesViewModel()
    {
        Languages = new ObservableCollection<string>();
        Languages.Add(Se.Language.General.Autodetect);
        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            Languages.Add(ci.EnglishName);
        }
        SelectedLanguage = Languages[0];

        SelectedLanguage = string.Empty;
        SelectedFontName = string.Empty;
        SelectedFontSize = string.Empty;
        SelectedLineHeight = string.Empty;
        LoadSettings();
    }

    private void LoadSettings()
    {

    }

    private void SaveSettings()
    {
        Se.SaveSettings();
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