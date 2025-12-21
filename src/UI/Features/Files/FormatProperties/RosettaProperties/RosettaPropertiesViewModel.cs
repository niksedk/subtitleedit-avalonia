using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

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
        Languages =
        [
            Se.Language.General.Autodetect,
            .. Iso639Dash2LanguageCode.List.Select(p=>p.TwoLetterCode)
                .Distinct()
                .OrderBy(p => p),
        ];
        SelectedLanguage = Languages[0];

        SelectedLanguage = string.Empty;
        SelectedFontName = string.Empty;
        SelectedFontSize = string.Empty;
        SelectedLineHeight = string.Empty;
        LoadSettings();
    }

    private void LoadSettings()
    {
        SelectedLineHeight = Se.Settings.Formats.RosettaLineHeight;
        SelectedFontSize = Se.Settings.Formats.RosettaFontSize;
        if (Se.Settings.Formats.RosettaLanguageAutoDetect)
        {
            SelectedLanguage = Languages[0];
        }
        else
        {
            var ci = new CultureInfo(Se.Settings.Formats.RosettaLanguage);
            SelectedLanguage = ci.TwoLetterISOLanguageName.ToLowerInvariant();
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Formats.RosettaLineHeight = SelectedLineHeight;
        Se.Settings.Formats.RosettaFontSize = SelectedFontSize;

        if (SelectedLanguage == Languages[0])
        {
            Se.Settings.Formats.RosettaLanguageAutoDetect = true;
            Se.Settings.Formats.RosettaLanguage = "en";
        }
        else
        {
            Se.Settings.Formats.RosettaLanguageAutoDetect = false;
            Se.Settings.Formats.RosettaLanguage = SelectedLanguage;
        }   

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
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