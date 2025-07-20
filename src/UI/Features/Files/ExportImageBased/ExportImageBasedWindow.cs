using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportImageBasedWindow : Window
{
    private readonly ExportImageBasedViewModel _vm;

    public ExportImageBasedWindow(ExportImageBasedViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 900;
        MinHeight = 700;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var subtitlesView = MakeSubtitlesView(vm);
        var controlsView = MakeControlsView(vm);
        var previewView = MakePreviewView(vm);
        
        var buttonExport = UiUtil.MakeButton(Se.Language.General.ExportDotDotDot, vm.ExportCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonExport, buttonOk, buttonCancel);

        grid.Add(subtitlesView, 0);
        grid.Add(controlsView, 1);
        grid.Add(previewView, 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Border MakeSubtitlesView(ExportImageBasedViewModel vm)
    { 
        vm.SubtitleGrid = new DataGrid
        {
            Height = double.NaN, // auto size inside scroll viewer
            Margin = new Thickness(2),
            ItemsSource = vm.Subtitles, // Use ItemsSource instead of Items
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Extended,
            DataContext = vm.Subtitles,
        };

     //   vm.SubtitleGrid.DoubleTapped += vm.OnSubtitleGridDoubleTapped;

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // Columns
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            Width = new DataGridLength(50),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
            Width = new DataGridLength(120),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Hide,
            Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter },
            Width = new DataGridLength(120),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });

        vm.SubtitleGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Duration,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.DurationBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
                };

                border.Child = textBlock;
                return border;
            })
        });

        vm.SubtitleGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Text,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.TextBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Text))
                };

                border.Child = textBlock;
                return border;
            })
        });

        vm.SubtitleGrid.DataContext = vm.Subtitles;
        vm.SubtitleGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle))
        {
            Source = vm,
            Mode = BindingMode.TwoWay
        });
        vm.SubtitleGrid.SelectionChanged += vm.SubtitleGrid_SelectionChanged;

        return UiUtil.MakeBorderForControl(vm.SubtitleGrid); 
    }

    private Border MakeControlsView(ExportImageBasedViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };


        // column 1
        var labelFontFamily = UiUtil.MakeLabel(Se.Language.General.FontName);
        var comboBoxFontFamily = UiUtil.MakeComboBox(vm.FontFamilies, vm, nameof(vm.SelectedFontFamily));
        comboBoxFontFamily.SelectionChanged += vm.ComboChanged;

        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.FontSize);
        var comboBoxFontSize = UiUtil.MakeComboBox(vm.FontSizes, vm, nameof(vm.SelectedFontSize));
        comboBoxFontSize.SelectionChanged += vm.ComboChanged;
        var checkBoxBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.IsBold));
        checkBoxBold.IsCheckedChanged += vm.CheckBoxChanged;
        var panelFontSizeAndBold = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                comboBoxFontSize,
                checkBoxBold
            }
        };

        var labelResolution = UiUtil.MakeLabel(Se.Language.General.Resolution);
        var comboBoxResolution = UiUtil.MakeComboBox(vm.Resolutions, vm, nameof(vm.SelectedResolution));
        comboBoxResolution.SelectionChanged += vm.ComboChanged;

        var labelTopBottomMargin = UiUtil.MakeLabel(Se.Language.File.Export.TopBottomMargin);
        var comboBoxTopBottomMargin = UiUtil.MakeComboBox(vm.TopBottomMargins, vm, nameof(vm.SelectedTopBottomMargin));
        comboBoxTopBottomMargin.SelectionChanged += vm.ComboChanged;

        var labelLeftRightMargin = UiUtil.MakeLabel(Se.Language.File.Export.LeftRightMargin);
        var comboBoxLeftRightMargin = UiUtil.MakeComboBox(vm.LeftRightMargins, vm, nameof(vm.SelectedLeftRightMargin));
        comboBoxLeftRightMargin.SelectionChanged += vm.ComboChanged;

        grid.Add(labelFontFamily, 0);
        grid.Add(comboBoxFontFamily, 0,1);

        grid.Add(labelFontSize, 1, 0);
        grid.Add(panelFontSizeAndBold, 1, 1);

        grid.Add(labelResolution, 2, 0);
        grid.Add(comboBoxResolution, 2, 1);

        grid.Add(labelTopBottomMargin, 3, 0);
        grid.Add(comboBoxTopBottomMargin, 3, 1);

        grid.Add(labelLeftRightMargin, 4, 0);
        grid.Add(comboBoxLeftRightMargin, 4, 1);

        // column 2
        var labelFontColor = UiUtil.MakeLabel(Se.Language.General.FontColor);
        var colorPickerFontColor = new ColorPicker
        {
            Width = 100,
            IsAlphaEnabled = true,
            IsAlphaVisible = true,
            IsColorSpectrumSliderVisible = false,
            IsColorComponentsVisible = true,
            IsColorModelVisible = false,
            IsColorPaletteVisible = false,
            IsAccentColorsVisible = false,
            IsColorSpectrumVisible = true,
            IsComponentTextInputVisible = true,
            [!ColorPicker.ColorProperty] = new Binding(nameof(_vm.FontColor))
            {
                Source = _vm,
                Mode = BindingMode.TwoWay
            },
        };
        colorPickerFontColor.ColorChanged += vm.ColorChanged;
        grid.Add(labelFontColor, 0, 2);
        grid.Add(colorPickerFontColor, 0, 3);

        var labelOutlineColor = UiUtil.MakeLabel(Se.Language.General.OutlineColor);
        var colorPickerOutlineColor = new ColorPicker
        {
            Width = 100,
            IsAlphaEnabled = true,
            IsAlphaVisible = true,
            IsColorSpectrumSliderVisible = false,
            IsColorComponentsVisible = true,
            IsColorModelVisible = false,
            IsColorPaletteVisible = false,
            IsAccentColorsVisible = false,
            IsColorSpectrumVisible = true,
            IsComponentTextInputVisible = true,
            [!ColorPicker.ColorProperty] = new Binding(nameof(_vm.OutlineColor))
            {
                Source = _vm,
                Mode = BindingMode.TwoWay
            },
        };
        colorPickerOutlineColor.ColorChanged += vm.ColorChanged;
        grid.Add(labelOutlineColor, 1, 2);
        grid.Add(colorPickerOutlineColor, 1, 3);

        var labelShadowColor = UiUtil.MakeLabel(Se.Language.General.ShadowColor);
        var colorPickerShadowColor = new ColorPicker
        {
            Width = 100,
            IsAlphaEnabled = true,
            IsAlphaVisible = true,
            IsColorSpectrumSliderVisible = false,
            IsColorComponentsVisible = true,
            IsColorModelVisible = false,
            IsColorPaletteVisible = false,
            IsAccentColorsVisible = false,
            IsColorSpectrumVisible = true,
            IsComponentTextInputVisible = true,
            [!ColorPicker.ColorProperty] = new Binding(nameof(_vm.ShadowColor))
            {
                Source = _vm,
                Mode = BindingMode.TwoWay
            },
        };
        colorPickerShadowColor.ColorChanged += vm.ColorChanged;
        grid.Add(labelShadowColor, 2, 2);
        grid.Add(colorPickerShadowColor, 2, 3);

        var labelBaclgroundColor = UiUtil.MakeLabel(Se.Language.General.BoxColor);
        var colorPickerBackgroundColor = new ColorPicker
        {
            Width = 100,
            IsAlphaEnabled = true,
            IsAlphaVisible = true,
            IsColorSpectrumSliderVisible = false,
            IsColorComponentsVisible = true,
            IsColorModelVisible = false,
            IsColorPaletteVisible = false,
            IsAccentColorsVisible = false,
            IsColorSpectrumVisible = true,
            IsComponentTextInputVisible = true,
            [!ColorPicker.ColorProperty] = new Binding(nameof(_vm.BoxColor))
            {
                Source = _vm,
                Mode = BindingMode.TwoWay
            },
        };
        colorPickerBackgroundColor.ColorChanged += vm.ColorChanged;
        grid.Add(labelBaclgroundColor, 3, 2);
        grid.Add(colorPickerBackgroundColor, 3, 3);


        // column 3

        var labelOutlineWidth = UiUtil.MakeLabel(Se.Language.General.OutlineWidth);
        var comboBoxOutlineWidth = UiUtil.MakeComboBox(vm.OutlineWidths, vm, nameof(vm.SelectedOutlineWidth));
        comboBoxOutlineWidth.SelectionChanged += vm.ComboChanged;
        grid.Add(labelOutlineWidth, 1, 4);
        grid.Add(comboBoxOutlineWidth, 1, 5);

        var labelShadowWidth = UiUtil.MakeLabel(Se.Language.General.ShadowWidth);
        var comboBoxShadowWidth = UiUtil.MakeComboBox(vm.ShadowWidths, vm, nameof(vm.SelectedShadowWidth));
        comboBoxShadowWidth.SelectionChanged += vm.ComboChanged;
        grid.Add(labelShadowWidth, 2, 4);
        grid.Add(comboBoxShadowWidth, 2, 5);

        var labelBoxCornerRadius = UiUtil.MakeLabel(Se.Language.General.BoxCornerRadius);
        var comboBoxBoxCornerRadius = UiUtil.MakeComboBox(vm.BoxCornerRadiusList, vm, nameof(vm.SelectedBoxCornerRadius));
        comboBoxBoxCornerRadius.SelectionChanged += vm.ComboChanged;
        grid.Add(labelBoxCornerRadius, 3, 4);
        grid.Add(comboBoxBoxCornerRadius, 3, 5);

        return UiUtil.MakeBorderForControl(grid); 
    }

    private Border MakePreviewView(ExportImageBasedViewModel vm)
    {
        var imagePreview = new Image
        {
            MaxHeight = 200,
            Stretch = Stretch.Uniform,
        };  
        imagePreview.Bind(Image.SourceProperty, new Binding(nameof(vm.BitmapPreview))
        {
            Source = vm,
            Mode = BindingMode.OneWay
        }); 
        
        return UiUtil.MakeBorderForControl(imagePreview).WithHeight(204); 
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded();
    }
}
