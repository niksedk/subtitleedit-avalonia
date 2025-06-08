using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;

public partial class BatchConvertSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _useSourceFolder;
    [ObservableProperty] private bool _useOutputFolder;
    [ObservableProperty] private string _outputFolder;
    [ObservableProperty] private bool _overwrite;
    
    public BatchConvertSettingsWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public BatchConvertSettingsViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        UseOutputFolder = Se.Settings.Tools.BatchConvert.UseOutputFolder;
        UseSourceFolder = !UseOutputFolder; 
        OutputFolder = Se.Settings.Tools.BatchConvert.OutputFolder;
        Overwrite = Se.Settings.Tools.BatchConvert.Overwrite;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.BatchConvert.UseOutputFolder = UseOutputFolder;
        Se.Settings.Tools.BatchConvert.OutputFolder = OutputFolder;
        Se.Settings.Tools.BatchConvert.Overwrite = Overwrite;
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