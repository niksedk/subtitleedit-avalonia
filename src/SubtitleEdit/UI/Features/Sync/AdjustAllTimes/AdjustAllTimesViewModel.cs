using System;
using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;

public partial class AdjustAllTimesViewModel : ObservableObject
{
    [ObservableProperty] private TimeSpan _adjustment;
    [ObservableProperty] private bool _adjustAll;
    [ObservableProperty] private bool _adjustSelectedLines;
    [ObservableProperty] private bool _adjustSelectedLinesAndForward;
    [ObservableProperty] private string _totalAdjustmentInfo;
    
    private double _totalAdjustment;
    
    public AdjustAllTimesWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public AdjustAllTimesViewModel()
    {
        TotalAdjustmentInfo = string.Empty;

        LoadSettings();
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
        ShowTotalAdjustmentInfo();
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