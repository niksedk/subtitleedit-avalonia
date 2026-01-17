using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;

public partial class AdjustAllTimesViewModel : ObservableObject
{
    [ObservableProperty] private TimeSpan _adjustment;
    [ObservableProperty] private bool _adjustAll;
    [ObservableProperty] private bool _adjustSelectedLines;
    [ObservableProperty] private bool _adjustSelectedLinesAndForward;
    [ObservableProperty] private string _totalAdjustmentInfo;
    [ObservableProperty] private string _statusText;

    private double _totalAdjustment;
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;
    private IAdjustCallback? _adjustCallback;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AdjustAllTimesViewModel()
    {
        TotalAdjustmentInfo = string.Empty;
        LoadSettings();
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        StatusText = string.Empty;  
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
        _ = ShowStatus(string.Format(Se.Language.Sync.AdjustmentX, "-" + new TimeCode(Adjustment).ToShortDisplayString()));
        Apply();
        ShowTotalAdjustmentInfo();
    }

    [RelayCommand]
    private void ShowEarlierTimeSpan(TimeSpan ts)
    {
        Adjustment = ts;
        _totalAdjustment -= ts.TotalSeconds;
        _ = ShowStatus(string.Format(Se.Language.Sync.AdjustmentX, "-" + new TimeCode(Adjustment).ToShortDisplayString()));
        Apply();
        ShowTotalAdjustmentInfo();
    }

    private void ShowTotalAdjustmentInfo()
    {
        TotalAdjustmentInfo = string.Format(Se.Language.General.TotalAdjustmentX, new TimeCode(_totalAdjustment * 1000.0).ToShortDisplayString());
    }

    [RelayCommand]
    private void ShowLater()
    {
        _totalAdjustment += Adjustment.TotalSeconds;
        _ = ShowStatus(string.Format(Se.Language.Sync.AdjustmentX, new TimeCode(Adjustment).ToShortDisplayString()));
        Apply();
        ShowTotalAdjustmentInfo();
    }

    [RelayCommand]
    private void ShowLaterTimeSpan(TimeSpan ts)
    {
        Adjustment = ts;
        _totalAdjustment += ts.TotalSeconds;
        _ = ShowStatus(string.Format(Se.Language.Sync.AdjustmentX, new TimeCode(Adjustment).ToShortDisplayString()));
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

    private Lock _statusLock = new Lock();
    private async Task ShowStatus(string statusText)
    {
        lock (_statusLock)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        StatusText = statusText;
        await Task.Delay(2000, _cancellationToken);
        StatusText = string.Empty;
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

    internal void OnClosing(WindowClosingEventArgs e)
    {
        _cancellationTokenSource.Cancel();
    }
}