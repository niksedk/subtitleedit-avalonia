using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel vm)
    {
        Title = "Settings";
        Width = 500;
        Height = 600;
        CanResize = true;
        
        Content = new SettingsPage(vm);
    }
}
