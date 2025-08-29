using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa;

public class AssaStylesWindow : Window
{
    public AssaStylesWindow(AssaStylesViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        Title = Se.Language.File.Statitics;
        CanResize = true;
        Width = 1200;
        Height = 850;
        MinWidth = 1100;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk);

        grid.Add(MakeLeftView(vm), 0);
        grid.Add(MakeRightView(vm), 0, 1);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Grid MakeLeftView(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeFileStyles(vm), 0);
        grid.Add(MakeStorageStyles(vm), 1);

        return grid;
    }

    private static Border MakeFileStyles(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) }, // label
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }, // datagrid
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel(Se.Language.Assa.StylesInFile).WithBold();

        var dataGrid = new DataGrid
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
            ItemsSource = vm.FileStyles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.FontName)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontSize,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.FontSize)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Usages,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.UsageCount)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedFileStyle)) { Source = vm });
        dataGrid.SelectionChanged += vm.FileStylesChanged;

        var buttonNew = UiUtil.MakeButton(vm.FileNewCommand, IconNames.MdiPlus, Se.Language.General.New);
        var buttonRemove = UiUtil.MakeButton(vm.FileRemoveCommand, IconNames.MdiTrash, Se.Language.General.Delete);
        var buttonImport = UiUtil.MakeButton(vm.FileImportCommand, IconNames.MdiImport, Se.Language.General.Import);
        var buttonExport = UiUtil.MakeButton(vm.FileExportCommand, IconNames.MdiExport, Se.Language.General.Export);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonNew,
            buttonRemove,
            buttonImport,
            buttonExport
            ).WithAlignmentLeft();

        grid.Add(label, 0, 0);
        grid.Add(dataGrid, 1, 0);
        grid.Add(panelButtons, 2, 0);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5);
    }

    private static Border MakeStorageStyles(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel(Se.Language.Assa.StylesSaved).WithBold();

        var dataGrid = new DataGrid
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
            ItemsSource = vm.StorageStyles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.FontName)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontSize,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.FontSize)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedStorageStyle)) { Source = vm });

        var buttonNew = UiUtil.MakeButton(vm.StorageNewCommand, IconNames.MdiPlus, Se.Language.General.New);
        var buttonRemove = UiUtil.MakeButton(vm.StorageRemoveCommand, IconNames.MdiTrash, Se.Language.General.Delete);
        var buttonImport = UiUtil.MakeButton(vm.StorageImportCommand, IconNames.MdiImport, Se.Language.General.Import);
        var buttonExport = UiUtil.MakeButton(vm.StorageExportCommand, IconNames.MdiExport, Se.Language.General.Export);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonNew,
            buttonRemove,
            buttonImport,
            buttonExport
            ).WithAlignmentLeft();

        grid.Add(label, 0, 0);
        grid.Add(dataGrid, 1, 0);
        grid.Add(panelButtons, 2, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Grid MakeRightView(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeSelectedStyleView(vm), 0);
        grid.Add(MakePreviewView(vm), 1);

        return grid;
    }

    private static Border MakeSelectedStyleView(AssaStylesViewModel vm)
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel("Selcted style (x)").WithBold();

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name);
        var textBoxName = UiUtil.MakeTextBox(200, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Name));
        var panelName = UiUtil.MakeHorizontalPanel(labelName, textBoxName).WithMarginBottom(10);

        var labelFontName = UiUtil.MakeLabel(Se.Language.General.FontName);
        var comboBoxFontName = UiUtil.MakeComboBox(vm.Fonts, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.FontName)).WithMinWidth(150);
        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.FontSize);
        var numericUpDownFontSize = UiUtil.MakeNumericUpDownInt(1, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.FontSize));
        var panelFont = UiUtil.MakeHorizontalPanel(labelFontName, comboBoxFontName, labelFontSize, numericUpDownFontSize);

        var checkBoxBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Bold));
        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Italic));
        var checkBoxUnderline = UiUtil.MakeCheckBox(Se.Language.General.Underline, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Underline));
        var checkBoxStrikeout = UiUtil.MakeCheckBox(Se.Language.General.Strikeout, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Strikeout));
        var panelFontStyle = UiUtil.MakeHorizontalPanel(checkBoxBold, checkBoxItalic, checkBoxUnderline, checkBoxStrikeout).WithMarginBottom(10);

        var labelScaleX = UiUtil.MakeLabel("Scale X").WithMinWidth(60);
        var numericUpDownScaleX = UiUtil.MakeNumericUpDownTwoDecimals(1, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ScaleX));
        var labelScaleY = UiUtil.MakeLabel("Scale Y").WithMinWidth(60);
        var numericUpDownScaleY = UiUtil.MakeNumericUpDownTwoDecimals(1, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ScaleY));
        var panelTransform1 = UiUtil.MakeHorizontalPanel(labelScaleX, numericUpDownScaleX, labelScaleY, numericUpDownScaleY);

        var labelSpacing = UiUtil.MakeLabel("Spacing").WithMinWidth(60);
        var numericUpDownSpacing = UiUtil.MakeNumericUpDownTwoDecimals(-100, 100, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Spacing));
        var labelAngle = UiUtil.MakeLabel("Angle").WithMinWidth(60);
        var numericUpDownAngle = UiUtil.MakeNumericUpDownTwoDecimals(-360, 360, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Angle));
        var panelTransform2 = UiUtil.MakeHorizontalPanel(labelSpacing, numericUpDownSpacing, labelAngle, numericUpDownAngle).WithMarginBottom(10);

        var labelColorPrimary = UiUtil.MakeLabel("Primary");
        var colorPickerPrimary = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorPrimary));
        var labelColorSecondary = UiUtil.MakeLabel("Secondary");
        var colorPickerSecondary = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorSecondary));
        var labelColorOutline = UiUtil.MakeLabel("Outline");
        var colorPickerOutline = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorOutline));
        var labelColorShadow = UiUtil.MakeLabel("Shadow");
        var colorPickerShadow = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorShadow));
        var panelColors = UiUtil.MakeHorizontalPanel(
            labelColorPrimary,
            colorPickerPrimary,
            labelColorSecondary,
            colorPickerSecondary,
            labelColorOutline,
            colorPickerOutline,
            labelColorShadow,
            colorPickerShadow).WithMarginBottom(10);

        var alignmentView = MakeAlignmentView(vm);
        var marginView = MakeMarginView(vm);
        var borderView = MakeBorderView(vm);
        var panelMore = UiUtil.MakeHorizontalPanel(alignmentView, marginView, borderView);

        grid.Add(label, 0, 0);
        grid.Add(panelName, 1, 0);
        grid.Add(panelFont, 2, 0);
        grid.Add(panelFontStyle, 3, 0);
        grid.Add(panelTransform1, 4, 0);
        grid.Add(panelTransform2, 5, 0);
        grid.Add(panelColors, 6, 0);
        grid.Add(panelMore, 7, 0);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5);
    }

    private static Border MakeAlignmentView(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
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
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Alignment);

        grid.Add(label, 0, 0, 1, 3);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn7), "align"), 1, 0);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn8), "align"), 1, 1);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn9), "align"), 1, 2);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn4), "align"), 2, 0);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn5), "align"), 2, 1);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn6), "align"), 2, 2);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn1), "align"), 3, 0);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn2), "align"), 3, 1);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn3), "align"), 3, 2);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeMarginView(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Margin);
        grid.Add(label, 0);

        var labelMarginLeft = UiUtil.MakeLabel(Se.Language.General.Left);
        var numericUpDownMarginLeft = UiUtil.MakeNumericUpDownInt(0, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.MarginLeft));
        grid.Add(labelMarginLeft, 1, 0);
        grid.Add(numericUpDownMarginLeft, 1, 1);

        var labelMarginRight = UiUtil.MakeLabel(Se.Language.General.Right);
        var numericUpDownMarginRight = UiUtil.MakeNumericUpDownInt(0, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.MarginRight));
        grid.Add(labelMarginRight, 2, 0);
        grid.Add(numericUpDownMarginRight, 2, 1);

        var labelMarginVertical = UiUtil.MakeLabel(Se.Language.General.Vertical);
        var numericUpDownMarginVertical = UiUtil.MakeNumericUpDownInt(0, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.MarginVertical));
        grid.Add(labelMarginVertical, 3, 0);
        grid.Add(numericUpDownMarginVertical, 3, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeBorderView(AssaStylesViewModel vm)
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
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.BorderStyle);
        grid.Add(label, 1, 0);

        var comboBoxBorderType = UiUtil.MakeComboBox(vm.BorderTypes, vm, nameof(vm.SelectedBorderType));
        grid.Add(comboBoxBorderType, 2, 0, 1, 2);

        var labelOutlineWidth = UiUtil.MakeLabel(Se.Language.General.OutlineWidth);
        var numericUpDownOutlineWidth = UiUtil.MakeNumericUpDownTwoDecimals(0, 100, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.OutlineWidth));
        grid.Add(labelOutlineWidth, 3, 0);
        grid.Add(numericUpDownOutlineWidth, 3, 1);

        var labelShadowWidth = UiUtil.MakeLabel(Se.Language.General.ShadowWidth);
        var numericUpDownShadowWidth = UiUtil.MakeNumericUpDownTwoDecimals(0, 100, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ShadowWidth));
        grid.Add(labelShadowWidth, 4, 0);
        grid.Add(numericUpDownShadowWidth, 4, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakePreviewView(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel("Preview");


        grid.Add(label, 0, 0);

        return UiUtil.MakeBorderForControl(grid);
    }
}
