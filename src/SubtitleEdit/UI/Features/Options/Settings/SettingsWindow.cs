using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Settings";
        Width = 700;
        Height = 600;
        CanResize = true;

        vm.Window = this;
        Content = new SettingsPage(vm);
    }
}
