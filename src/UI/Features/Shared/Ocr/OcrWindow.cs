using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class OcrWindow : Window
{
    private readonly OcrViewModel _vm;

    public OcrWindow(OcrViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        UiUtil.InitializeWindow(this);
        Title = vm.WindowTitle;
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var topControlsView = MakeTopControlsView(vm);
        var subtitleView = MakeSubtitleView(vm);
        var editView = MakeEditView(vm);
        var buttonView = MakeBottomView(vm);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(topControlsView, 0, 0);
        grid.Add(subtitleView, 1, 0);
        grid.Add(editView, 2, 0);
        grid.Add(buttonView, 3, 0);

        Content = grid;

        Activated += delegate
        {
            Focus(); // hack to make OnKeyDown work
        };
    }

    private static StackPanel MakeTopControlsView(OcrViewModel vm)
    {
        var comboBoxEngines = UiUtil.MakeComboBox(vm.OcrEngines, vm, nameof(vm.SelectedOcrEngine))
            .WithMarginRight(10)
            .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter());
        comboBoxEngines.SelectionChanged += vm.EngineSelectionChanged;

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.OcrEngine),
                comboBoxEngines,

                // NOcr settings
                UiUtil.MakeLabel(Se.Language.Ocr.Database, nameof(vm.IsNOcrVisible)),
                UiUtil.MakeComboBox(vm.NOcrDatabases, vm, nameof(vm.SelectedNOcrDatabase), nameof(vm.IsNOcrVisible))
                    .WithMarginRight(0)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),
                UiUtil.MakeButton(vm.ShowNOcrSettingsCommand, IconNames.Settings)
                    .WithMarginRight(20)
                    .WithMarginBottom(2)
                    .WithBottomAlignment()
                    .WithBindIsVisible(nameof(vm.IsNOcrVisible))
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),
                UiUtil.MakeLabel(Se.Language.Ocr.MaxWrongPixels, nameof(vm.IsNOcrVisible)),
                UiUtil.MakeComboBox(vm.NOcrMaxWrongPixelsList, vm, nameof(vm.SelectedNOcrMaxWrongPixels),
                        nameof(vm.IsNOcrVisible))
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),
                UiUtil.MakeLabel(Se.Language.Ocr.NumberOfPixelsIsSpace, nameof(vm.IsNOcrVisible)),
                UiUtil.MakeComboBox(vm.NOcrPixelsAreSpaceList, vm, nameof(vm.SelectedNOcrPixelsAreSpace),
                        nameof(vm.IsNOcrVisible))
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),

                // Tesseract settings
                UiUtil.MakeLabel(Se.Language.General.Language, nameof(vm.IsTesseractVisible)),
                UiUtil.MakeComboBox(vm.TesseractDictionaryItems, vm, nameof(vm.SelectedTesseractDictionaryItem),
                        nameof(vm.IsTesseractVisible))
                    .WithWidth(100)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),
                UiUtil.MakeBrowseButton(vm.PickTesseractModelCommand).BindIsVisible(vm, nameof(vm.IsTesseractVisible))
                    .BindIsEnabled(vm, nameof(vm.IsOcrRunning), new InverseBooleanConverter()),

                // Ollama settings
                UiUtil.MakeLabel(Se.Language.General.Language, nameof(vm.IsOllamaVisible)),
                UiUtil.MakeComboBox(vm.OllamaLanguages, vm, nameof(vm.SelectedOllamaLanguage),
                        nameof(vm.IsOllamaVisible))
                    .WithWidth(100)
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),
                UiUtil.MakeLabel(Se.Language.General.Model, nameof(vm.IsOllamaVisible)),
                UiUtil.MakeTextBox(200, vm, nameof(vm.OllamaModel))
                    .BindIsVisible(vm, nameof(vm.IsOllamaVisible))
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),
                UiUtil.MakeBrowseButton(vm.PickOllamaModelCommand)
                    .BindIsVisible(vm, nameof(vm.IsOllamaVisible))
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),

                // Google vision settings
                UiUtil.MakeLabel(Se.Language.General.Language, nameof(vm.IsGoogleVisionVisible)),
                UiUtil.MakeComboBox(vm.GoogleVisionLanguages, vm, nameof(vm.SelectedGoogleVisionLanguage),
                        nameof(vm.IsGoogleVisionVisible))
                    .WithWidth(100)
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),
                UiUtil.MakeLabel(Se.Language.General.ApiKey, nameof(vm.IsGoogleVisionVisible)),
                UiUtil.MakeTextBox(200, vm, nameof(vm.GoogleVisionApiKey))
                    .BindIsVisible(vm, nameof(vm.IsGoogleVisionVisible))
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),

                // Paddle OCR settings
                UiUtil.MakeLabel(Se.Language.General.Language, nameof(vm.IsPaddleOcrVisible)),
                UiUtil.MakeComboBox(vm.PaddleOcrLanguages, vm, nameof(vm.SelectedPaddleOcrLanguage),
                        nameof(vm.IsPaddleOcrVisible))
                    .WithWidth(100)
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),

                // Mistral OCR settings
                UiUtil.MakeLabel(Se.Language.General.ApiKey, nameof(vm.IsMistralOcrVisible)),
                UiUtil.MakeTextBox(200, vm, nameof(vm.MistralApiKey))
                    .BindIsVisible(vm, nameof(vm.IsMistralOcrVisible))
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()),
            }
        };

        return panel;
    }

    private static Border MakeSubtitleView(OcrViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var dataGridSubtitle = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Extended,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.OcrSubtitleItems,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.Duration)) { Converter = shortTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Image,
                    IsReadOnly = true,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<OcrSubtitleItem>((item, _) =>
                    {
                        var stackPanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Spacing = 5
                        };
                        var bitmap = item.GetBitmap();
                        if (bitmap != null)
                        {
                            var image = new Image
                            {
                                Source = bitmap,
                                MaxHeight = 100,
                                MaxWidth = 200,
                                Stretch = Avalonia.Media.Stretch.Uniform
                            };
                            stackPanel.Children.Add(image);
                        }

                        return stackPanel;
                    })
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGridSubtitle.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedOcrSubtitleItem)) { Source = vm });
        dataGridSubtitle.KeyDown += vm.SubtitleGridKeyDown;
        vm.SubtitleGrid = dataGridSubtitle;

        // Create a Flyout for the DataGrid
        var flyout = new MenuFlyout();

        flyout.Opening += vm.SubtitleGridContextOpening;

        var menuItemOcrSelectedLines = new MenuItem
        {
            Header = Se.Language.Ocr.OcrSelectedLines,
            DataContext = vm,
            Command = vm.StartOcrSelectedLinesCommand,
        };
        menuItemOcrSelectedLines.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu), BindingMode.TwoWay));
        flyout.Items.Add(menuItemOcrSelectedLines);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.DeleteSelectedLinesCommand,
        };
        menuItemDelete.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu), BindingMode.TwoWay));
        flyout.Items.Add(menuItemDelete);

        var menuItemItalic = new MenuItem
        {
            Header = Se.Language.General.Italic,
            DataContext = vm,
            Command = vm.ToggleItalicCommand,
        };
        menuItemItalic.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu), BindingMode.TwoWay));
        flyout.Items.Add(menuItemItalic);

        var menuItemBold = new MenuItem
        {
            Header = Se.Language.General.Bold,
            DataContext = vm,
            Command = vm.ToggleBoldCommand,
        };
        menuItemBold.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu), BindingMode.TwoWay));
        flyout.Items.Add(menuItemBold);

        vm.SubtitleGrid.ContextFlyout = flyout;

        return UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle).WithMarginBottom(5);
    }

    private static Border MakeEditView(OcrViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10),
            Children =
            {
                UiUtil.MakeLabel(Se.Language.General.Text).WithAlignmentTop(),
                new TextBox
                {
                    Width = 300,
                    Height = 60,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    AcceptsReturn = true,
                    TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
                    Margin = new Thickness(0, 0, 10, 0),
                    [!TextBox.TextProperty] =
                        new Binding($"{nameof(vm.SelectedOcrSubtitleItem)}.{nameof(OcrSubtitleItem.Text)}")
                            { Source = vm, Mode = BindingMode.TwoWay }
                }
            }
        };

        var border = UiUtil.MakeBorderForControl(panel).WithMarginBottom(5);
        border.Bind(Border.IsVisibleProperty, new Binding(nameof(vm.IsOcrRunning)) { Source = vm, Converter = new InverseBooleanConverter() });

        return border;
    }

    private static Grid MakeBottomView(OcrViewModel vm)
    {
        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 6.0)
                    }
                },
            }
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressSlider.Bind(Slider.IsVisibleProperty, new Binding(nameof(vm.IsOcrRunning)) { Source = vm });

        var statusText = new TextBlock().WithMarginTop(22);
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));
        statusText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsOcrRunning)) { Source = vm });

        var buttonStart = UiUtil.MakeButton(Se.Language.Ocr.StartOcr, vm.StartOcrCommand)
            .WithBindIsVisible(nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()).WithBottomAlignment();
        var buttonPause = UiUtil.MakeButton(Se.Language.Ocr.PauseOcr, vm.PauseOcrCommand)
            .WithBindIsVisible(nameof(OcrViewModel.IsOcrRunning)).WithBottomAlignment();
        var buttonInspect = UiUtil.MakeButton(Se.Language.Ocr.InspectLine, vm.InspectLineCommand)
            .WithBindIsVisible(nameof(OcrViewModel.IsInspectLineVisible))
            .WithBindIsEnabled(nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter())
            .WithBottomAlignment();
        var buttonInspectAdditions = UiUtil.MakeButton(Se.Language.General.InspectAdditions, vm.InspectAdditionsCommand)
            .WithBindIsVisible(nameof(vm.IsInspectAdditionsVisible))
            .WithBindIsEnabled(nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter())
            .WithBottomAlignment();
        var buttonExport = UiUtil.MakeButton(Se.Language.General.ExportDotDotDot, vm.ExportCommand)
            .WithBindIsEnabled(nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter()).WithBottomAlignment();
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBottomAlignment();
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand).WithBottomAlignment();

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(progressSlider, 0, 0);
        grid.Add(statusText, 0, 0);
        grid.Add(buttonStart, 0, 1);
        grid.Add(buttonPause, 0, 2);
        grid.Add(buttonInspect, 0, 3);
        grid.Add(buttonInspectAdditions, 0, 4);
        grid.Add(buttonExport, 0, 5);
        grid.Add(buttonOk, 0, 6);
        grid.Add(buttonCancel, 0, 7);

        return grid;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.SelectAndScrollToRow(0);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}