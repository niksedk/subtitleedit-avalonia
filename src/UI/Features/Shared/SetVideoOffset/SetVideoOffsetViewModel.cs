using System;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;

public partial class SetVideoOffsetViewModel : ObservableObject
{
    [ObservableProperty] private TimeSpan? _timeOffset;
    [ObservableProperty] private bool _relativeToCurrentVideoPosition;
    [ObservableProperty] private bool _keepTimeCodes;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public bool ResetPressed { get; private set; }

    public SetVideoOffsetViewModel()
    {
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
    private void Reset()
    {
        ResetPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (e.Key == Key.Enter)
        {
            Ok();
        }
    }
}