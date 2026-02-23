using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;

public class SettingsImportExportWindow : Window
{
    public SettingsImportExportWindow(SettingsImportExportViewModel vm)
    {
        Width = 500;
        Height = 700;
        MinWidth = 400;
        MinHeight = 500;
        CanResize = true;

        vm.Window = this;
        DataContext = vm;
        Content = new SettingsImportExportPage(vm);
        Title = vm.TitleText;
        Loaded += vm.OnLoaded;
    }
}
