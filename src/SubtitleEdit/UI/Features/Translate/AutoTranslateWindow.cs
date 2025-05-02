using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Translate;

public class AutoTranslateWindow : Window
{
    public AutoTranslateWindow(AutoTranslateViewModel vm)
    {
        Title = "Auto-translate";
        Width = 900;
        Height = 700;
        MinWidth = 740;
        MinHeight = 400;

        DataContext = vm;
        vm.Window = this;

        var topBarPoweredBy = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Children =
            {
                UiUtil.MakeTextBlock("Powered by"),
                UiUtil.MakeLink("Google Translate V1", vm.GoToAutranslatorUriCommand),
            }
        };

        var engineCombo = UiUtil.MakeComboBox(vm.AutoTranslators, vm.SelectedAutoTranslator);
        var sourceLangCombo = UiUtil.MakeComboBox(vm.SourceLanguages, vm.SelectedSourceLanguage);
        var targetLangCombo = UiUtil.MakeComboBox(vm.TargetLanguages, vm.SelectedTargetLanguage);
        var buttonTranslate = UiUtil.MakeButton("Translate", vm.TranslateCommand);

        var topBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Children =
            {
                UiUtil.MakeTextBlock("Engine:"),
                engineCombo,
                UiUtil.MakeSeparatorForHorizontal(),
                UiUtil.MakeTextBlock("From:"),
                sourceLangCombo,
                UiUtil.MakeTextBlock("To:"),
                targetLangCombo,
                buttonTranslate,
            }
        };

        var contextMenu = new MenuFlyout
        {
            Items =
            {
                new MenuItem
                {
                    Header = "Translate row",
                    Command = vm.TranslateRowCommand,
                },
            }
        };

        var treeDataGrid = new TreeDataGrid
        {
            Height = double.NaN, // auto size inside scroll viewer
            Margin = new Thickness(0, 15, 0, 0),
            Source = vm.TranslateRowSource,
            CanUserSortColumns = false,
            ContextFlyout = contextMenu,
        };

        var scrollViewer = new ScrollViewer
        {
            Content = treeDataGrid,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var okButton = UiUtil.MakeButton("OK", vm.CancelCommand);
        var cancelButton = UiUtil.MakeButton("Cancel", vm.CancelCommand);
        StackPanel bottomBar = UiUtil.MakeButtonBar(okButton, cancelButton);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*,Auto"),
            Margin = new Thickness(UiUtil.WindowMarginWidth),
        };

        var row = 0;
        grid.Children.Add(topBarPoweredBy);
        Grid.SetRow(topBarPoweredBy, row++);

        grid.Children.Add(topBar);
        Grid.SetRow(topBar, row++);

        grid.Children.Add(scrollViewer);
        Grid.SetRow(scrollViewer, row++);

        grid.Children.Add(bottomBar);
        Grid.SetRow(bottomBar, row++);

        Content = grid;
    }
}
