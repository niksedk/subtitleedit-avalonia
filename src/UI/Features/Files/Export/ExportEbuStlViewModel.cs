using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Files.Export;

public partial class ExportEbuStlViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _codePages;
    [ObservableProperty] private string? _selectedCodePage;
    [ObservableProperty] private ObservableCollection<string> _diskFormatCodes;
    [ObservableProperty] private string? _selectedDiskFormatCode;
    [ObservableProperty] private ObservableCollection<string> _frameRates;
    [ObservableProperty] private string? _selectedFrameRate;
    [ObservableProperty] private ObservableCollection<string> _displayStandardCodes;
    [ObservableProperty] private string? _selectedDisplayStandardCode;
    [ObservableProperty] private ObservableCollection<string> _characterTables;
    [ObservableProperty] private string? _selectedCharacterTable;
    [ObservableProperty] private ObservableCollection<string> _languageCodes;
    [ObservableProperty] private string? _selectedLanguageCode;
    [ObservableProperty] private ObservableCollection<string> _timeCodeStatusList;
    [ObservableProperty] private string? _selectedTimeCodeStatus;

    [ObservableProperty] private string _originalProgramTitle;
    [ObservableProperty] private string _originalEpisodeTitle;
    [ObservableProperty] private string _translatedProgramTitle;
    [ObservableProperty] private string _translatedEpisodeTitle;
    [ObservableProperty] private string _translatorsName;
    [ObservableProperty] private string _subtitleListReferenceCode;
    [ObservableProperty] private string _countryOfOrigin;
    [ObservableProperty] private TimeSpan _startOfProgramme;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public int? PacCodePage { get; private set; }

    public ExportEbuStlViewModel()
    {
        CodePages = new ObservableCollection<string>
        {
            "Latin",
            "Greek",
            "Latin Czech",
            "Arabic",
            "Hebrew",
            "Thai",
            "Cyrillic",
            "Chinese Traditional (Big5)",
            "Chinese Simplified (gb2312)",
            "Korean",
            "Japanese",
            "Portuguese",
        };
        SelectedCodePage = CodePages[0];
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private void Import()
    {
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}