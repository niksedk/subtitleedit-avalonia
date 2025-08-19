using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

public class CompareWindow : Window
{
    public CompareWindow(CompareViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = Se.Language.File.Compare;
        Width = 1200;
        Height = 600;
        MinWidth = 900;
        MinHeight = 500;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), // browse buttons + file names
                new RowDefinition(GridLength.Star), // subtitle views
                new RowDefinition(GridLength.Auto), // status text
                new RowDefinition(GridLength.Auto), // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10
        };

        var buttonLeftFileName = UiUtil.MakeButtonBrowse(vm.PickLeftSubtitleFileCommand);
        var labelLeftFileName = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.LeftFileName));
        var labelLeftFileNameHasChanges = UiUtil.MakeLabel("*").WithBindVisible(vm, nameof(vm.LeftFileNameHasChanges));
        var panelLeftBrowse = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            Children = { buttonLeftFileName, labelLeftFileName, labelLeftFileNameHasChanges },
        };
        grid.Add(panelLeftBrowse, 0);

        var buttonRightFileName = UiUtil.MakeButtonBrowse(vm.PickRightSubtitleFileCommand);
        var buttonRightReload = UiUtil.MakeButton(string.Format(Se.Language.File.LoadXFromFile, System.IO.Path.GetFileName(vm.LeftFileName)), vm.ReloadRightFromFileCommand)
            .WithBindIsVisible(nameof(vm.IsReloadFromFileVisible));
        var labelRightFileName = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.RightFileName));
        var panelRightBrowse = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            Children = { buttonRightFileName, buttonRightReload, labelRightFileName },
        };
        grid.Add(panelRightBrowse, 0, 1);

        // left subtitle view (original)
        var leftView = MakeSubtitlesView(vm.LeftSubtitles, nameof(vm.SelectedLeft));
        grid.Add(leftView, 1);

        // right subtitle view (modified)
        var rightView = MakeSubtitlesView(vm.RightSubtitles, nameof(vm.SelectedRight));
        grid.Add(rightView, 1, 1);

        // status text
        var statusText = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.StatusText));
        grid.Add(statusText, 2, 0, 1, 2);

        // display type combo box
        var labelDisplayType = UiUtil.MakeLabel(Se.Language.General.Show).WithMarginRight(5);  
        var comboBoxCompareVisual = UiUtil.MakeComboBox(vm.CompareVisuals, vm, nameof(vm.SelectedCompareVisual));
        comboBoxCompareVisual.SelectionChanged += vm.ComboBoxCompareVisualSelectionChanged;
        var panelDisplayType = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { labelDisplayType, comboBoxCompareVisual }
        };  
        grid.Add(panelDisplayType, 3, 0, 1, 2);

        // buttons
        CheckBox checkBoxIgnoreWhiteSpace = UiUtil.MakeCheckBox(Se.Language.File.IgnoreWhitespace, vm, nameof(vm.IgnoreWhiteSpace))
            .WithMarginLeft(10);
        checkBoxIgnoreWhiteSpace.IsCheckedChanged += vm.CheckBoxChanged;
        var checkBoxIngoreFormatting = UiUtil.MakeCheckBox(Se.Language.File.IgnoreFormatting, vm, nameof(vm.IgnoreFormatting))
            .WithMarginLeft(10).WithMarginRight(15);
        checkBoxIngoreFormatting.IsCheckedChanged += vm.CheckBoxChanged;
        var buttonPreviousDifference = UiUtil.MakeButton(vm.PreviousDifferenceCommand, IconNames.MdiChevronLeft).WithBindIsVisible(nameof(vm.IsExportVisible));
        if (Se.Settings.Appearance.ToolbarShowHints)
        {
            ToolTip.SetTip(buttonPreviousDifference, Se.Language.File.PreviousDifference);
        }
        var buttonNextDifference = UiUtil.MakeButton(vm.NextDifferenceCommand, IconNames.MdiChevronRight).WithBindIsVisible(nameof(vm.IsExportVisible));
        if (Se.Settings.Appearance.ToolbarShowHints)
        {
            ToolTip.SetTip(buttonNextDifference, Se.Language.File.NextDifference);
        }
        var buttonExport = UiUtil.MakeButton(Se.Language.General.Export, vm.ExportCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindIsVisible(nameof(vm.IsExportVisible));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            checkBoxIgnoreWhiteSpace, 
            checkBoxIngoreFormatting, 
            buttonPreviousDifference, 
            buttonNextDifference,
            buttonExport,
            buttonOk
            //buttonCancel
            );
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { Dispatcher.UIThread.Post(() => buttonOk.Focus()); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;

        vm.LeftDataGrid = leftView.Child as DataGrid;
        vm.RightDataGrid = rightView.Child as DataGrid;
        if (vm.LeftDataGrid != null && vm.RightDataGrid != null)
        {
            vm.LeftDataGrid.SelectionChanged += vm.LeftDataGridSelectionChanged;
            vm.RightDataGrid.SelectionChanged += vm.RightDataGridSelectionChanged;
        }
    }

    private static Border MakeSubtitlesView(ObservableCollection<CompareItem> items, string selectedBinding)
    {
        var dg = new DataGrid
        {
            ItemsSource = items,
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            Height = double.NaN,
            Margin = new Thickness(2)
        };

        // Number column
        dg.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Width = new DataGridLength(50),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.NumberBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CompareItem.Number))
                };

                border.Child = textBlock;
                return border;
            })
        });

        // StartTime column
        dg.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Show,
            Width = new DataGridLength(120),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.StartTimeBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CompareItem.StartTime))
                };

                border.Child = textBlock;
                return border;
            })
        });

        // EndTime column
        dg.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Hide,
            Width = new DataGridLength(120),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.EndTimeBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CompareItem.EndTime))
                };

                border.Child = textBlock;
                return border;
            })
        });

        // Text column
        dg.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Text,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.TextBackgroundBrush))
                };

                var stackPanel = new StackPanel();
                if (item.TextPanel.Parent is Panel parent)
                {
                    parent.Children.Remove(item.TextPanel);
                }
                stackPanel.Children.Add(item.TextPanel);

                border.Child = stackPanel;
                return border;
            })
        });

        dg.Bind(DataGrid.SelectedItemProperty, new Binding(selectedBinding)
        {
            Mode = BindingMode.TwoWay
        });

        return UiUtil.MakeBorderForControl(dg);
    }
}
