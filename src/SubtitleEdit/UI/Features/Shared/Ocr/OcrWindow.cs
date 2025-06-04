using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class OcrWindow : Window
{
    private readonly OcrViewModel _vm;

    public OcrWindow(OcrViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        Icon = UiUtil.GetSeIcon();
        Title = vm.WindowTitle;
        Width = 1024;
        Height = 600;
        MinWidth = 800;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var topControlsView = MakeTopControlsView(vm);
        var subtitleView = MakeSubtitleView(vm);

        var buttonStart = UiUtil.MakeButton("Start OCR", vm.StartOcrCommand)
            .WithBindIsVisible(nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter());
        var buttonPause = UiUtil.MakeButton("Pause OCR", vm.PauseOcrCommand)
            .WithBindIsVisible(nameof(OcrViewModel.IsOcrRunning));
        var buttonExport = UiUtil.MakeButton("Export...", vm.ExportCommand)
            .WithBindIsEnabled(nameof(OcrViewModel.IsOcrRunning), new InverseBooleanConverter());
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonStart, buttonPause, buttonExport, buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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
        grid.Add(panelButtons, 2, 0);

        Content = grid;

        Activated += delegate
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
        };
    }

    private static StackPanel MakeTopControlsView(OcrViewModel vm)
    {
        var comboBoxEngines = UiUtil.MakeComboBox(vm.OcrEngines, vm, nameof(vm.SelectedOcrEngine))
            .WithWidth(150)
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
                UiUtil.MakeLabel("OCR Engine"),
                comboBoxEngines,
                UiUtil.MakeLabel("Database", nameof(vm.IsNOcrVisible)),
                UiUtil.MakeComboBox( vm.NOcrDatabases, vm, nameof(vm.SelectedNOcrDatabase), nameof(vm.IsNOcrVisible))
                    .WithWidth(150)
                    .WithMarginRight(10)
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
            SelectionMode = DataGridSelectionMode.Single,
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
                    Header = "#",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Show",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Duration",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.Duration)) { Converter = shortTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTemplateColumn
                {
                    Header = "Image",
                    IsReadOnly = true,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<OcrSubtitleItem>((item, _) =>
                    {
                        var stackPanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Spacing = 5
                        };

                        // Add text if available
                        if (!string.IsNullOrEmpty(item.Text))
                        {
                            var textBlock = new TextBlock
                            {
                                Text = item.Text,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                MaxWidth = 300 // Adjust as needed
                            };
                            stackPanel.Children.Add(textBlock);
                        }

                        var bitmap = item.GetBitmap();
                       // Add image if available
                        if (bitmap != null)
                        {
                            var image = new Image
                            {
                                Source = bitmap,
                                MaxHeight = 100, // Adjust as needed
                                MaxWidth = 200,  // Adjust as needed
                                Stretch = Avalonia.Media.Stretch.Uniform
                            };
                            stackPanel.Children.Add(image);
                        }

                        return stackPanel;
                    })
                },
                new DataGridTextColumn
                {
                    Header = "Text",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.Text)),
                    IsReadOnly = true,
                },
            },
        };

        var border = new Border
        {
            Child = dataGridSubtitle,
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Padding = new Thickness(10, 0, 10, 0),
            CornerRadius = new CornerRadius(5),
        };

        return border;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.SelectAndScrollToRow(0);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}