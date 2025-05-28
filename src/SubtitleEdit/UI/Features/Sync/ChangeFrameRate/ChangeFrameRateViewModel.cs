using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;

public partial class ChangeFrameRateViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<double> _fromFrameRates;
    [ObservableProperty] private double _selectedFromFrameRate;
    
    [ObservableProperty] private ObservableCollection<double> _toFrameRates;
    [ObservableProperty] private double _selectedToFrameRate;
    
    public ChangeFrameRateWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public ChangeFrameRateViewModel()
    {
    }
    
    [RelayCommand]                   
    private void SwitchFrameRates() 
    {
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