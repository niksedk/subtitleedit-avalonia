using System;
using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Tools.AdjustDuration;

public partial class AdjustDurationViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<AdjustDurationDisplay> _adjustTypes;
    [ObservableProperty] private AdjustDurationDisplay _selectedLanguageAdjustType;
    
    [ObservableProperty] private double _adjustSeconds;
    [ObservableProperty] private double _adjustPercent;
    [ObservableProperty] private double _adjustFixed;
    [ObservableProperty] private double _adjustRecalculateMaxCharacterPerSecond;
    [ObservableProperty] private double _adjustRecalculateOptimalCharacterPerSecond;
    
    public AdjustDurationWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public AdjustDurationViewModel()
    {
        AdjustTypes = new ObservableCollection<AdjustDurationDisplay>(AdjustDurationDisplay.ListAll());
        SelectedLanguageAdjustType = AdjustTypes[0]; 
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