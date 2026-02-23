using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;

public class SettingsImportExportWindow : Window
{
    public SettingsImportExportWindow(SettingsImportExportViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        CanResize = true;
        Width = 500;
        Height = 700;
        MinWidth = 400;
        MinHeight = 500;

        vm.Window = this;
        DataContext = vm;
        Content = new SettingsImportExportPage(vm);
        Title = vm.TitleText;
        Loaded += vm.OnLoaded;
        KeyDown += vm.KeyDown;
    }
}
