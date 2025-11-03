using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;

public partial class BatchConvertFixCommonErrorsSettingsViewModel : ObservableObject
{
    [ObservableProperty] private Tools.FixCommonErrors.ProfileDisplayItem? _fixCommonErrorsProfile;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public BatchConvertFixCommonErrorsSettingsViewModel()
    {
    }

    public void Initialize(Tools.FixCommonErrors.ProfileDisplayItem profile)
    {
        FixCommonErrorsProfile = profile;
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