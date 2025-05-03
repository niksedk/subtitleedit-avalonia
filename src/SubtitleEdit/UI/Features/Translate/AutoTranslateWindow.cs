using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Translate;

public class AutoTranslateWindow : Window
{
    public AutoTranslateWindow(AutoTranslateViewModel vm)
    {
        Title = "Auto-translate";
        Width = 950;
        MinWidth = 750;
        Height = 700;
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
                UiUtil.MakeLink("Google Translate V1", vm.GoToAutranslatorUriCommand, vm, nameof(vm.AutoTranslatorLinkText))
                    .WithMarginRight(UiUtil.WindowMarginWidth),
            }
        };

        var engineCombo = UiUtil.MakeComboBox(vm.AutoTranslators, vm, nameof(vm.SelectedAutoTranslator));

        engineCombo.OnPropertyChanged(e =>
        {
            if (e.Property == SelectingItemsControl.SelectedItemProperty)
            {
                vm.AutoTranslatorChanged(e.Sender);
            }
        });

        var sourceLangCombo = UiUtil.MakeComboBox(vm.SourceLanguages!, vm, nameof(vm.SelectedSourceLanguage));
        var targetLangCombo = UiUtil.MakeComboBox(vm.TargetLanguages!, vm, nameof(vm.SelectedTargetLanguage));
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
            Margin = new Thickness(2),
            Source = vm.TranslateRowSource,
            CanUserSortColumns = false,
            ContextFlyout = contextMenu,
        };

        var scrollViewer = new ScrollViewer
        {
            Content = treeDataGrid,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        var scrollViewerBorder = UiUtil.MakeBorderForControl(scrollViewer);

        StackPanel settingsBar = UiUtil.MakeControlBarLeft(
            UiUtil.MakeTextBlock("API key", vm, null, nameof(vm.ApiKeyIsVisible)),
            UiUtil.MakeTextBox(150, vm, nameof(vm.ApiKeyText), nameof(vm.ApiKeyIsVisible)),
            UiUtil.MakeSeparatorForHorizontal(),
            UiUtil.MakeTextBlock("API url", vm, null, nameof(vm.ApiUrlIsVisible)),
            UiUtil.MakeTextBox(150, vm, nameof(vm.ApiUrlText), nameof(vm.ApiUrlIsVisible)),
            UiUtil.MakeSeparatorForHorizontal(),
            UiUtil.MakeTextBlock("Model", vm, null, nameof(vm.ModelIsVisible)),
            UiUtil.MakeTextBox(150, vm, nameof(vm.ModelText), nameof(vm.ModelIsVisible))
        );

        var settingsLink = UiUtil.MakeLink("Settings", vm.OpenSettingsCommand).WithMarginRight(10);
        var okButton = UiUtil.MakeButton("OK", vm.OkCommand);
        var cancelButton = UiUtil.MakeButton("Cancel", vm.CancelCommand);
        StackPanel bottomBar = UiUtil.MakeButtonBar(settingsLink, okButton, cancelButton);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*,Auto,Auto"),
            Margin = new Thickness(UiUtil.WindowMarginWidth),
        };

        var row = 0;
        grid.Children.Add(topBarPoweredBy);
        Grid.SetRow(topBarPoweredBy, row++);

        grid.Children.Add(topBar);
        Grid.SetRow(topBar, row++);

        grid.Children.Add(scrollViewerBorder);
        Grid.SetRow(scrollViewerBorder, row++);

        grid.Children.Add(settingsBar);
        Grid.SetRow(settingsBar, row++);

        grid.Children.Add(bottomBar);
        Grid.SetRow(bottomBar, row++);

        Content = grid;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (DataContext is AutoTranslateViewModel vm)
        {
            vm.SaveSettings();
        }
    }
}
