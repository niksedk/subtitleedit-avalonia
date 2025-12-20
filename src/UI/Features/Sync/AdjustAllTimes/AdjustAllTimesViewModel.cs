using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;

public partial class AdjustAllTimesViewModel : ObservableObject
{
    [ObservableProperty] private TimeSpan _adjustment;
    [ObservableProperty] private bool _adjustAll;
    [ObservableProperty] private bool _adjustSelectedLines;
    [ObservableProperty] private bool _adjustSelectedLinesAndForward;
    [ObservableProperty] private string _totalAdjustmentInfo;

    private double _totalAdjustment;

    private IAdjustCallback? _adjustCallback;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AdjustAllTimesViewModel()
    {
        TotalAdjustmentInfo = string.Empty;
        LoadSettings();
    }

    public void Initialize(IAdjustCallback adjustCallback, int selectedLinesCount)
    {
        _adjustCallback = adjustCallback;
        if (selectedLinesCount > 1)
        {
            AdjustSelectedLines = true;
        }
        else if (selectedLinesCount == 1 && AdjustSelectedLines)
        {
            AdjustSelectedLines = false;
            AdjustAll = true;
        }

        string choice = Se.Settings.Synchronization.AdjustAllTimesLineSelectionChoice;
        if (Se.Settings.Synchronization.AdjustAllTimesRememberLineSelectionChoice && !string.IsNullOrEmpty(choice))
        {
            AdjustAll = choice == "All";
            AdjustSelectedLines = choice == "Selected";
            AdjustSelectedLinesAndForward = choice == "SelectedAndForward";
        }
    }

    private void LoadSettings()
    {
        Adjustment = TimeSpan.FromSeconds(Se.Settings.Synchronization.AdjustAllTimes.Seconds);
        if (Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesAndForwardSelected)
        {
            AdjustSelectedLinesAndForward = true;
        }
        else if (Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesSelected)
        {
            AdjustSelectedLines = true;
        }
        else
        {
            AdjustAll = true;
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Synchronization.AdjustAllTimes.Seconds = Adjustment.TotalSeconds;
        Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesAndForwardSelected = AdjustSelectedLinesAndForward;
        Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesSelected = AdjustSelectedLines;
        Se.Settings.Synchronization.AdjustAllTimes.IsAllSelected = AdjustAll;

        Se.SaveSettings();
    }

    [RelayCommand]
    private void ShowEarlier()
    {
        _totalAdjustment -= Adjustment.TotalSeconds;
        Apply();
        ShowTotalAdjustmentInfo();
    }

    private void ShowTotalAdjustmentInfo()
    {
        TotalAdjustmentInfo = $"Total adjustment: {new TimeCode(_totalAdjustment * 1000.0).ToShortDisplayString()}";
    }

    [RelayCommand]
    private void ShowLater()
    {
        _totalAdjustment += Adjustment.TotalSeconds;
        Apply();
        ShowTotalAdjustmentInfo();
    }

    private void Apply()
    {
        SaveSettings();
        InvokeAdjustCallback();
    }

    [RelayCommand]
    private void Ok()
    {
        if (Se.Settings.Synchronization.AdjustAllTimesRememberLineSelectionChoice)
        {
            if (AdjustAll)
            {
                Se.Settings.Synchronization.AdjustAllTimesLineSelectionChoice = "All";
            }
            else if (AdjustSelectedLines)
            {
                Se.Settings.Synchronization.AdjustAllTimesLineSelectionChoice = "Selected";
            }
            else if (AdjustSelectedLinesAndForward)
            {
                Se.Settings.Synchronization.AdjustAllTimesLineSelectionChoice = "SelectedAndForward";
            }
        }

        OkPressed = true;
        Window?.Close();
    }

    private void InvokeAdjustCallback()
    {
        _adjustCallback?.Adjust(
            TimeSpan.FromSeconds(_totalAdjustment),
            AdjustAll,
            AdjustSelectedLines,
            AdjustSelectedLinesAndForward);

        TotalAdjustmentInfo = string.Empty;
        _totalAdjustment = 0; // Reset after applying
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