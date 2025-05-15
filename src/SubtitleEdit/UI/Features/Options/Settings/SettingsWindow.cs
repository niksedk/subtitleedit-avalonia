using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsWindow : Window
{
    private readonly SettingsViewModel _vm;

    public SettingsWindow(SettingsViewModel vm)
    {
        _vm = vm;
        Icon = UiUtil.GetSeIcon();
        Title = "Settings";
        Width = 800;
        Height = 700;
        CanResize = true;

        vm.Window = this;
        Content = new SettingsPage(vm);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _vm.OnClosing(e);
        base.OnClosing(e);
    }
}
