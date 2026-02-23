using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;

public class SettingsImportExportPage : UserControl
{
    private readonly SettingsImportExportViewModel _vm;

    public SettingsImportExportPage(SettingsImportExportViewModel vm)
    {
        _vm = vm;

        var labelSelectAreas = UiUtil.MakeLabel("Select areas to import/export:");
        labelSelectAreas.FontSize = 14;
        labelSelectAreas.FontWeight = Avalonia.Media.FontWeight.Bold;
        labelSelectAreas.Margin = new Thickness(0, 0, 0, 10);

        var scrollViewer = new ScrollViewer
        {
            Content = new ItemsControl
            {
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SettingsAreas)) { Source = _vm },
                ItemTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<SettingsAreaItem>((item, _) =>
                    new CheckBox
                    {
                        Content = item.Name,
                        [!ToggleButton.IsCheckedProperty] = new Binding(nameof(SettingsAreaItem.IsSelected))
                        {
                            Source = item,
                            Mode = BindingMode.TwoWay
                        },
                        Margin = new Thickness(0, 3, 0, 3)
                    }, true)
            },
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 350
        };

        var labelFilePath = UiUtil.MakeLabel("File path:");
        labelFilePath.Margin = new Thickness(0, 10, 0, 5);

        var textBoxFilePath = new TextBox
        {
            [!TextBox.TextProperty] = new Binding(nameof(_vm.FilePath)) { Source = _vm, Mode = BindingMode.TwoWay },
            Margin = new Thickness(0, 0, 5, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var buttonBrowse = UiUtil.MakeButtonBrowse(_vm.BrowseFileCommand);

        var filePathStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { textBoxFilePath, buttonBrowse }
        };

        var buttonOk = UiUtil.MakeButtonOk(_vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(_vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var mainStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15),
            Spacing = 5,
            Children =
            {
                labelSelectAreas,
                scrollViewer,
                labelFilePath,
                filePathStack,
                buttonBar
            }
        };

        Content = mainStack;
    }
}
