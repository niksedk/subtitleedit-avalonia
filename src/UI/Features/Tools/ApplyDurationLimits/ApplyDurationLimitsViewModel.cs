using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;

public partial class ApplyDurationLimitsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ApplyDurationLimitItem> _fixes;
    [ObservableProperty] private ApplyDurationLimitItem? _selectedFix;

    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;

    [ObservableProperty] private int _minDurationMs;
    [ObservableProperty] private bool _fixMinDurationMs;

    [ObservableProperty] private int _maxDurationMs;
    [ObservableProperty] private bool _fixMaxDurationMs;

    [ObservableProperty] private bool _doNotGoPastShotChange;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private bool _dirty { get; set; }

    public ApplyDurationLimitsViewModel()
    {
        Fixes = new ObservableCollection<ApplyDurationLimitItem>();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        LoadSettings();
    }

    private void LoadSettings()
    {
        FixMinDurationMs = true;
        MinDurationMs = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;

        FixMaxDurationMs = true;
        MaxDurationMs = Se.Settings.General.SubtitleMaximumDisplayMilliseconds;

        DoNotGoPastShotChange = true;
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

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}