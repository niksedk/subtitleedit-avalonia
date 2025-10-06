using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class CustomContinuationStyleViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _preAndSuffixes;

    [ObservableProperty] private string _selectedPrefix;
    [ObservableProperty] private bool _selectedPrefixAddSpace;
    [ObservableProperty] private string _selectedSuffix;
    [ObservableProperty] private bool _selectedSuffixesProcessIfEndWithComma;
    [ObservableProperty] private bool _selectedSuffixesAddSpace;
    [ObservableProperty] private bool _selectedSuffixesRemoveComma;

    [ObservableProperty] private bool _useSpecialStyleAfterLongGaps;
    [ObservableProperty] private int _longGapMs;
    [ObservableProperty] private string _selectedLongGapPrefix;
    [ObservableProperty] private bool _selectedLongGapPrefixAddSpace;
    [ObservableProperty] private string _selectedLongGapSuffix;
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
        SelectedSuffix = string.Empty;
        SelectedLongGapPrefix = string.Empty;
        SelectedLongGapSuffix = string.Empty;
        LongGapMs = 300;
    }

    public void Initialize(
        int continuationPause,
        string customContinuationStyleSuffix,
        bool customContinuationStyleSuffixApplyIfComma,
        bool customContinuationStyleSuffixAddSpace,
        bool customContinuationStyleSuffixReplaceComma,
        string customContinuationStylePrefix,
        bool customContinuationStylePrefixAddSpace,
        bool customContinuationStyleUseDifferentStyleGap,
        string customContinuationStyleGapSuffix,
        bool customContinuationStyleGapSuffixApplyIfComma,
        bool customContinuationStyleGapSuffixAddSpace,
        bool customContinuationStyleGapSuffixReplaceComma,
        string customContinuationStyleGapPrefix,
        bool customContinuationStyleGapPrefixAddSpace
        )
    {
        SelectedPrefix = customContinuationStylePrefix;
        SelectedPrefixAddSpace = customContinuationStylePrefixAddSpace;
        SelectedSuffix = customContinuationStyleSuffix;
        SelectedSuffixesProcessIfEndWithComma = customContinuationStyleSuffixApplyIfComma;
        SelectedSuffixesAddSpace = customContinuationStyleSuffixAddSpace;
        SelectedSuffixesRemoveComma = customContinuationStyleSuffixReplaceComma;
        UseSpecialStyleAfterLongGaps = customContinuationStyleUseDifferentStyleGap;
        LongGapMs = continuationPause;
        SelectedLongGapPrefix = customContinuationStyleGapPrefix;
        SelectedLongGapPrefixAddSpace = customContinuationStyleGapPrefixAddSpace;
        SelectedLongGapSuffix = customContinuationStyleGapSuffix;
        SelectedLongGapSuffixesProcessIfEndWithComma = customContinuationStyleGapSuffixApplyIfComma;
        SelectedLongGapSuffixesAddSpace = customContinuationStyleGapSuffixAddSpace;
        SelectedLongGapSuffixesRemoveComma = customContinuationStyleGapSuffixReplaceComma;
    }

    [RelayCommand]
    private void LoadContinuationStyleNone()
    {
        LoadSettings(ContinuationStyle.None);   
    }

    [RelayCommand]
    private void LoadContinuationStyleNoneTrailingDots()
    {
        LoadSettings(ContinuationStyle.NoneTrailingDots);
    }

    [RelayCommand]
    private void LoadContinuationStyleNoneLeadingTrailingDots()
    {
        LoadSettings(ContinuationStyle.NoneLeadingTrailingDots);
    }

    [RelayCommand]
    private void LoadContinuationStyleNoneTrailingEllipsis()
    {
        LoadSettings(ContinuationStyle.NoneTrailingEllipsis);
    }

    [RelayCommand]
    private void LoadContinuationStyleNoneLeadingTrailingEllipsis()
    {
        LoadSettings(ContinuationStyle.NoneLeadingTrailingEllipsis);
    }

    [RelayCommand]
    private void LoadContinuationStyleOnlyTrailingDots()
    {
        LoadSettings(ContinuationStyle.OnlyTrailingDots);
    }

    [RelayCommand]
    private void LoadContinuationStyleLeadingTrailingDots()
    {
        LoadSettings(ContinuationStyle.LeadingTrailingDots);
    }

    [RelayCommand]
    private void LoadContinuationStyleOnlyTrailingEllipsis()
    {
        LoadSettings(ContinuationStyle.OnlyTrailingEllipsis);
    }

    [RelayCommand]
    private void LoadContinuationStyleLeadingTrailingEllipsis()
    {
        LoadSettings(ContinuationStyle.LeadingTrailingEllipsis);
    }

    [RelayCommand]
    private void LoadContinuationStyleLeadingTrailingDash()
    {
        LoadSettings(ContinuationStyle.LeadingTrailingDash);
    }

    [RelayCommand]
    private void LoadContinuationStyleLeadingTrailingDashDots()
    {
        LoadSettings(ContinuationStyle.LeadingTrailingDashDots);
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

    private void LoadSettings(ContinuationStyle continuationStyle)
    {
        var settings = ContinuationUtilities.GetContinuationProfile(continuationStyle);
        SelectedPrefix = settings.Prefix;
        SelectedPrefixAddSpace = settings.PrefixAddSpace;
        SelectedSuffix = settings.Suffix;
        SelectedSuffixesProcessIfEndWithComma = settings.SuffixApplyIfComma;
        SelectedSuffixesAddSpace = settings.SuffixAddSpace;
        SelectedSuffixesRemoveComma = settings.SuffixReplaceComma;
        UseSpecialStyleAfterLongGaps = settings.UseDifferentStyleGap;
        SelectedLongGapPrefix = settings.GapPrefix;
        SelectedLongGapPrefixAddSpace = settings.GapPrefixAddSpace;
        SelectedLongGapSuffix = settings.GapSuffix;
        SelectedLongGapSuffixesProcessIfEndWithComma = settings.GapSuffixApplyIfComma;
        SelectedLongGapSuffixesAddSpace = settings.GapSuffixAddSpace;
        SelectedLongGapSuffixesRemoveComma = settings.GapSuffixReplaceComma;
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