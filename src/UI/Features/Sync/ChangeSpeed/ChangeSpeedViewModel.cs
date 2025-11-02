using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;

public partial class ChangeSpeedViewModel : ObservableObject
{
    [ObservableProperty] private double _speedPercent;
    [ObservableProperty] private bool _adjustAll;
    [ObservableProperty] private bool _adjustSelectedLines;
    [ObservableProperty] private bool _adjustSelectedLinesAndForward;

    public Window? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public ChangeSpeedViewModel()
    {
        AdjustAll = true;
    }

    [RelayCommand]
    private void SetFromDropFrameValue()
    {
        SpeedPercent = 100.1001; 
    }

    [RelayCommand]
    private void SetToDropFrameValue()
    {
        SpeedPercent = 99.9889;
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

    internal static void ChangeSpeed(ObservableCollection<SubtitleLineViewModel> subtitles, double speedPercent)
    {
        foreach (var subtitle in subtitles)
        {
            subtitle.StartTime = TimeSpan.FromMilliseconds(subtitle.StartTime.TotalMilliseconds * (100.0 / speedPercent));
            subtitle.EndTime = TimeSpan.FromMilliseconds(subtitle.EndTime.TotalMilliseconds * (100.0 / speedPercent));
        }
    }
}