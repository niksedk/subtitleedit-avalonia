using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;

public partial class VoiceSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _voiceTestText;
    
    public VoiceSettingsWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public VoiceSettingsViewModel()
    {
        VoiceTestText = Se.Settings.Video.TextToSpeech.VoiceTestText;
    }
    
    [RelayCommand]                   
    private void Ok() 
    {
        Se.Settings.Video.TextToSpeech.VoiceTestText = VoiceTestText;
        Se.SaveSettings();
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