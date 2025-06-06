using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public partial class ChangeCasingViewModel : ObservableObject
{
    [ObservableProperty] private bool _normalCasing;
    [ObservableProperty] private bool _normalCasingFixNames;
    [ObservableProperty] private bool _normalCasingOnlyUpper;
    [ObservableProperty] private bool _fixNamesOnly;
    [ObservableProperty] private bool _allUppercase;
    [ObservableProperty] private bool _allLowercase;
    
    public ChangeCasingWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public ChangeCasingViewModel()
    {
        NormalCasing = Se.Settings.Tools.ChangeCasing.NormalCasing;
        NormalCasingFixNames = Se.Settings.Tools.ChangeCasing.NormalCasingFixNames;
        NormalCasingOnlyUpper = Se.Settings.Tools.ChangeCasing.NormalCasingOnlyUpper;
        FixNamesOnly = Se.Settings.Tools.ChangeCasing.FixNamesOnly;
        AllUppercase = Se.Settings.Tools.ChangeCasing.AllUppercase;
        AllLowercase = Se.Settings.Tools.ChangeCasing.AllLowercase;
        LoadSettings();
    }

    private void LoadSettings()
    {
        NormalCasing = Se.Settings.Tools.ChangeCasing.NormalCasing;
        NormalCasingFixNames = Se.Settings.Tools.ChangeCasing.NormalCasingFixNames;
        NormalCasingOnlyUpper = Se.Settings.Tools.ChangeCasing.NormalCasingOnlyUpper;
        FixNamesOnly = Se.Settings.Tools.ChangeCasing.FixNamesOnly;
        AllUppercase = Se.Settings.Tools.ChangeCasing.AllUppercase;
        AllLowercase = Se.Settings.Tools.ChangeCasing.AllLowercase;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.ChangeCasing.NormalCasing = NormalCasing;
        Se.Settings.Tools.ChangeCasing.NormalCasingFixNames = NormalCasingFixNames;
        Se.Settings.Tools.ChangeCasing.NormalCasingOnlyUpper = NormalCasingOnlyUpper;
        Se.Settings.Tools.ChangeCasing.FixNamesOnly = FixNamesOnly;
        Se.Settings.Tools.ChangeCasing.AllUppercase = AllUppercase;
        Se.Settings.Tools.ChangeCasing.AllLowercase = AllLowercase;
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