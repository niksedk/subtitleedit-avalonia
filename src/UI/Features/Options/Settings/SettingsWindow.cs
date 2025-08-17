using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsWindow : Window
{
    private readonly SettingsViewModel _vm;

    public SettingsWindow(SettingsViewModel vm)
    {
        _vm = vm;
        Icon = UiUtil.GetSeIcon();
        Title = Se.Language.General.Settings;
        Width = 900;
        Height = 700;
        MinWidth = 650;
        MinHeight = 500;
        CanResize = true;

        vm.Window = this;
        Content = new SettingsPage(vm);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
