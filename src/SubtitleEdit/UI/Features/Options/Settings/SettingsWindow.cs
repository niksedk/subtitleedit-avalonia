
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsWindow : Window
{
    public SettingsWindow()
    {
        Title = "Settings";
        Width = 500;
        Height = 600;
        CanResize = true;

        Content = new SettingsPage();
    }
}
