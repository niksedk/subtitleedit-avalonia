using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class GetKeyViewModel : ObservableObject
{
    [ObservableProperty] private string _infoText;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public string PressedKeyOnly { get; private set; }
    public string PressedKey { get; private set; }
    public bool IsControlPressed { get; private set; }
    public bool IsAltPressed { get; private set; }
    public bool IsShiftPressed { get; private set; }

    public GetKeyViewModel()
    {
        InfoText = Se.Language.Options.Shortcuts.PressAKey;
        PressedKey = string.Empty;
        PressedKeyOnly = string.Empty;
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
        else
        {
            PressedKey = e.Key.ToString();
            PressedKeyOnly = PressedKey;
            IsControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            IsAltPressed = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
            IsShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
                PressedKey != Key.LeftCtrl.ToString() &&
                PressedKey != Key.RightCtrl.ToString())
            {
                PressedKey = "Ctrl + " + PressedKey;
            }

            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt) &&
                 PressedKey != Key.LeftAlt.ToString() &&
                 PressedKey != Key.RightAlt.ToString())
            {
                PressedKey = "Alt + " + PressedKey;
            }

            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift) &&
                 PressedKey != Key.LeftShift.ToString() &&
                 PressedKey != Key.RightShift.ToString())
            {
                PressedKey = "Shift + " + PressedKey;
            }

            InfoText = string.Format(Se.Language.Options.Shortcuts.PressedKeyX, PressedKey);
        }
    }
}