using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;

public partial class ApplyDurationLimitsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<AdjustDurationDisplay> _adjustTypes;
    [ObservableProperty] private double _adjustRecalculateOptimalCharacterPerSecond;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public ApplyDurationLimitsViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
    }

    private void SaveSettings()
    {
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