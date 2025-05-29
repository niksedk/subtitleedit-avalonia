using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;

public partial class ChangeSpeedViewModel : ObservableObject
{
    [ObservableProperty] private double _speedPercent;
    [ObservableProperty] private bool _adjustAll;
    [ObservableProperty] private bool _adjustSelectedLines;
    [ObservableProperty] private bool _adjustSelectedLinesAndForward;

    public ChangeSpeedWindow? Window { get; set; }
    
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
}