using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class CustomContinuationStyleViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _preAndSuffixes;

    [ObservableProperty] private string _selectedPrefix;
    [ObservableProperty] private bool _selectedPrefixAddSpace;
    [ObservableProperty] private string _selectedSuffixes;
    [ObservableProperty] private bool _selectedSuffixesProcessIfEndWithComma;
    [ObservableProperty] private bool _selectedSuffixesAddSpace;
    [ObservableProperty] private bool _selectedSuffixesRemoveComma;

    [ObservableProperty] private bool _useSpecialStyleAfterLongGaps;
    [ObservableProperty] private int _longGapMs;
    [ObservableProperty] private string _selectedLongGapPrefix;
    [ObservableProperty] private bool _selectedLongGapPrefixAddSpace;
    [ObservableProperty] private string _selectedLongGapSuffixes;
    [ObservableProperty] private bool _selectedLongGapSuffixesProcessIfEndWithComma;
    [ObservableProperty] private bool _selectedLongGapSuffixesAddSpace;
    [ObservableProperty] private bool _selectedLongGapSuffixesRemoveComma;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public CustomContinuationStyleViewModel()
    {
        PreAndSuffixes = new ObservableCollection<string>(new[]
        {
            string.Empty,
            "...",
            "…",
            "..",
            "–",
        });

        SelectedPrefix = string.Empty;
        SelectedSuffixes = string.Empty;
        SelectedLongGapPrefix = string.Empty;
        SelectedLongGapSuffixes = string.Empty;
        LongGapMs = 300;
    }

    public void Initialize()
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

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}