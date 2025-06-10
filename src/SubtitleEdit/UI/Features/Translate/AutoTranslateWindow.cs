using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Translate;

public class AutoTranslateWindow : Window
{
    public AutoTranslateWindow(AutoTranslateViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
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
        buttonTranslate.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsTranslateEnabled)));

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

        var dataGrid = new DataGrid
        {
            Height = double.NaN, // auto size inside scroll viewer
            Margin = new Thickness(2),
            CanUserSortColumns = false,
            ContextFlyout = contextMenu,
            DataContext = vm,
            IsReadOnly = true,
            AutoGenerateColumns = false,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "#",
                    Binding = new Binding(nameof(TranslateRow.Number)),
                    Width = new DataGridLength(50)
                },
                new DataGridTextColumn
                {
                    Header = "Show",
                    Binding = new Binding(nameof(TranslateRow.Show)),
                    Width = new DataGridLength(100)
                },
                new DataGridTextColumn
                {
                    Header = "Duration",
                    Binding = new Binding(nameof(TranslateRow.Duration)),
                    Width = new DataGridLength(80)
                },
                new DataGridTextColumn
                {
                    Header = "Text",
                    Binding = new Binding(nameof(TranslateRow.Text)),
                    Width = new DataGridLength(200, DataGridLengthUnitType.Star)
                },
                new DataGridTextColumn
                {
                    Header = "Translated text",
                    Binding = new Binding(nameof(TranslateRow.TranslatedText)),
                    Width = new DataGridLength(200, DataGridLengthUnitType.Star)
                }
            }
        };
        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Rows)));
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedTranslateRow)));
        vm.RowGrid = dataGrid;


        var scrollViewer = new ScrollViewer
        {
            Content = dataGrid,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        var scrollViewerBorder = UiUtil.MakeBorderForControl(scrollViewer);

        StackPanel settingsBar = UiUtil.MakeControlBarLeft(
            UiUtil.MakeTextBlock("API key", vm, null, nameof(vm.ApiKeyIsVisible)).WithMarginRight(5),
            UiUtil.MakeTextBox(150, vm, nameof(vm.ApiKeyText), nameof(vm.ApiKeyIsVisible)),
            UiUtil.MakeSeparatorForHorizontal(),
            UiUtil.MakeTextBlock("API url", vm, null, nameof(vm.ApiUrlIsVisible)).WithMarginRight(5),
            UiUtil.MakeTextBox(150, vm, nameof(vm.ApiUrlText), nameof(vm.ApiUrlIsVisible)),
            UiUtil.MakeSeparatorForHorizontal(),
            UiUtil.MakeTextBlock("Model", vm, null, nameof(vm.ModelIsVisible)).WithMarginRight(5),
            UiUtil.MakeTextBox(150, vm, nameof(vm.ModelText), nameof(vm.ModelIsVisible))
        );

        var settingsLink = UiUtil.MakeLink("Settings", vm.OpenSettingsCommand).WithMarginRight(10);
        var okButton = UiUtil.MakeButtonOk(vm.OkCommand);
        okButton.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsTranslateEnabled)));
        var cancelButton = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var bottomGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            Margin = new Thickness(10, 0, 0, 0),
            Width = double.NaN,
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    },
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 8.0)
                    },
                },
            },
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressSlider.Bind(Slider.IsVisibleProperty, new Binding(nameof(vm.IsProgressEnabled)));
        bottomGrid.Children.Add(progressSlider);
        Grid.SetRow(progressSlider, 0);
        var bottomBar = UiUtil.MakeButtonBar(settingsLink, okButton, cancelButton);
        bottomGrid.Children.Add(bottomBar);
        Grid.SetRow(bottomBar, 1);


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

        grid.Children.Add(bottomGrid);
        Grid.SetRow(bottomGrid, row++);

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
