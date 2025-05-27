using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.AdjustDuration;

public partial class AdjustDurationViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<AdjustDurationDisplay> _adjustTypes;
    [ObservableProperty] private AdjustDurationDisplay _selectedAdjustType;
    
    [ObservableProperty] private double _adjustSeconds;
    [ObservableProperty] private int _adjustPercent;
    [ObservableProperty] private double _adjustFixed;
    [ObservableProperty] private double _adjustRecalculateMaxCharacterPerSecond;
    [ObservableProperty] private double _adjustRecalculateOptimalCharacterPerSecond;
    
    public AdjustDurationWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public AdjustDurationViewModel()
    {
        AdjustTypes = new ObservableCollection<AdjustDurationDisplay>(AdjustDurationDisplay.ListAll());
        SelectedAdjustType = AdjustTypes[0];
        LoadSettings();
    }

    private void LoadSettings()
    {
        AdjustSeconds = Se.Settings.Tools.AdjustDurations.AdjustDurationSeconds;
        AdjustPercent = Se.Settings.Tools.AdjustDurations.AdjustDurationPercent;
        AdjustFixed = Se.Settings.Tools.AdjustDurations.AdjustDurationFixed;
        AdjustRecalculateMaxCharacterPerSecond = Se.Settings.Tools.AdjustDurations.AdjustDurationMaximumCps;
        AdjustRecalculateOptimalCharacterPerSecond = Se.Settings.Tools.AdjustDurations.AdjustDurationOptimalCps;

        SelectedAdjustType = AdjustTypes.FirstOrDefault(p =>
            p.Type.ToString() == Se.Settings.Tools.AdjustDurations.AdjustDurationLast)
            ?? AdjustTypes[0];;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.AdjustDurations.AdjustDurationSeconds = AdjustSeconds;
        Se.Settings.Tools.AdjustDurations.AdjustDurationPercent = AdjustPercent;
        Se.Settings.Tools.AdjustDurations.AdjustDurationFixed = AdjustFixed;
        Se.Settings.Tools.AdjustDurations.AdjustDurationMaximumCps = AdjustRecalculateMaxCharacterPerSecond;
        Se.Settings.Tools.AdjustDurations.AdjustDurationOptimalCps = AdjustRecalculateOptimalCharacterPerSecond;

        Se.Settings.Tools.AdjustDurations.AdjustDurationLast = SelectedAdjustType.Type.ToString();
        
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