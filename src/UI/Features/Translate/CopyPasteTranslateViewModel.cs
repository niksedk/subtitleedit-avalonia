using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class CopyPasteTranslateViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<AdjustDurationDisplay> _adjustTypes;
    [ObservableProperty] private double _adjustRecalculateOptimalCharacterPerSecond;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public CopyPasteTranslateViewModel()
    {
        AdjustTypes = new ObservableCollection<AdjustDurationDisplay>();
        LoadSettings();
    }

    private void LoadSettings()
    {
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    internal void Initialize(Subtitle subtitle)
    {
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