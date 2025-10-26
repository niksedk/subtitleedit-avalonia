using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public class ShortcutsWindow : Window
{
    private TextBox _searchBox;
    private readonly ShortcutsViewModel _vm;

    public ShortcutsWindow(ShortcutsViewModel vm)
    {
        var language = Se.Language.Options.Shortcuts;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = language.Title;
        Width = 760;
        Height = 650;
        MinWidth = 740;
        MinHeight = 500;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        _searchBox = new TextBox
        {
            Watermark = language.SearchShortcuts,
            Margin = new Thickness(10),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        _searchBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });

        var labelFilter = UiUtil.MakeTextBlock(language.Filter);
        var comboBoxFilter = UiUtil.MakeComboBox(vm.Filters, vm, nameof(vm.SelectedFilter))
            .WithMinWidth(120)
            .WithMargin(5, 0, 10, 0);
        comboBoxFilter.SelectionChanged += vm.ComboBoxFilter_SelectionChanged;

        var topGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto"),
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto,Auto"),
            Margin = new Thickness(10),
        };
        topGrid.Children.Add(_searchBox);
        Grid.SetRow(_searchBox, 0);
        Grid.SetColumn(_searchBox, 0);

        topGrid.Children.Add(labelFilter);
        Grid.SetRow(labelFilter, 0);
        Grid.SetColumn(labelFilter, 1);

        topGrid.Children.Add(comboBoxFilter);
        Grid.SetRow(comboBoxFilter, 0);
        Grid.SetColumn(comboBoxFilter, 2);

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
            ItemsSource = vm.FlatNodes,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Category,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ShortcutTreeNode.Category)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ShortcutTreeNode.Title)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Shortcut,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ShortcutTreeNode.DisplayShortcut)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedNode)) { Source = vm });
        dataGrid.SelectionChanged += vm.ShortcutsDataGrid_SelectionChanged;
        dataGrid.DoubleTapped += vm.ShortcutsDataGridDoubleTapped;
        var borderDataGrid = UiUtil.MakeBorderForControlNoPadding(dataGrid).WithMarginBottom(5);

        var buttonOk = UiUtil.MakeButtonOk(vm.CommandOkCommand);
        var buttonResetAllShortcuts = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetAllShortcutsCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonResetAllShortcuts, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = new Thickness(UiUtil.WindowMarginWidth),
        };
        grid.Add(topGrid, 0);
        grid.Add(borderDataGrid, 1);


        var labelBadgeCount = new Border
        {
            Background = UiUtil.GetBorderBrush(),     // badge background
            CornerRadius = new CornerRadius(10),      // makes it pill-like
            Padding = new Thickness(6, 0, 6, 0),      // spacing around text
            Margin = new Thickness(8),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            Child = new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding(nameof(vm.FlatNodes) + ".Count", BindingMode.OneWay) { Converter = new NumberToStringWithThousandSeparator() },
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.WhiteSmoke,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
        grid.Add(labelBadgeCount, 1);

        var editPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
        };

        // Get platform-specific labels
        var isMac = OperatingSystem.IsMacOS();
        var ctrlLabel = isMac ? Se.Language.Options.Shortcuts.ControlMac : Se.Language.Options.Shortcuts.Control;
        var altLabel = isMac ? Se.Language.Options.Shortcuts.AltMac : Se.Language.Options.Shortcuts.Alt;
        var shiftLabel = isMac ? Se.Language.Options.Shortcuts.ShiftMac : Se.Language.Options.Shortcuts.Shift;
        var winLabel = isMac ? Se.Language.Options.Shortcuts.WinMac : Se.Language.Options.Shortcuts.Win;

        // Shift checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(shiftLabel).WithMarginRight(3));
        var checkBoxShift = UiUtil.MakeCheckBox(vm, nameof(vm.ShiftIsSelected));
        checkBoxShift.PropertyChanged += (s, e) => vm.UpdateShortcutCommand.Execute(null);
        checkBoxShift.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(checkBoxShift);

        // Control checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(ctrlLabel).WithMarginRight(3));
        var controlCheckBox = UiUtil.MakeCheckBox(vm, nameof(vm.CtrlIsSelected));
        controlCheckBox.PropertyChanged += (s, e) => vm.UpdateShortcutCommand.Execute(null);
        controlCheckBox.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(controlCheckBox);

        // Alt checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(altLabel).WithMarginRight(3));
        var checkBoxAlt = UiUtil.MakeCheckBox(vm, nameof(vm.AltIsSelected));
        checkBoxAlt.PropertyChanged += (s, e) => vm.UpdateShortcutCommand.Execute(null);
        checkBoxAlt.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(checkBoxAlt);

        // Win key checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(winLabel).WithMarginRight(3));
        var checkBoxWin = UiUtil.MakeCheckBox(vm, nameof(vm.WinIsSelected));
        checkBoxWin.PropertyChanged += (s, e) => vm.UpdateShortcutCommand.Execute(null);
        checkBoxWin.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(checkBoxWin);

        // Key combobox
        var comboBoxKeys = new ComboBox
        {
            Width = 200,
            Margin = new Thickness(10, 0, 5, 0),
        };
        comboBoxKeys.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Shortcuts)) { Source = vm });
        comboBoxKeys.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedShortcut)) { Source = vm });
        comboBoxKeys.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(comboBoxKeys);
        comboBoxKeys.PropertyChanged += (s, e) => vm.UpdateShortcutCommand.Execute(null);

        // browse button
        var buttonBrowse = UiUtil.MakeButtonBrowse(vm.ShowGetKeyCommand);
        editPanel.Children.Add(buttonBrowse);
        buttonBrowse.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        buttonBrowse.Margin = new Thickness(0, 0, 9, 0);

        // configure button
        var buttonConfig = UiUtil.MakeButton(vm.ConfigureCommand, IconNames.Settings);
        editPanel.Children.Add(buttonConfig);
        buttonConfig.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        buttonConfig.Bind(IsVisibleProperty, new Binding(nameof(vm.IsConfigureVisible)) { Source = vm });
        buttonConfig.Margin = new Thickness(0, 0, 10, 0);

        // Reset button
        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetShortcutCommand);
        editPanel.Children.Add(buttonReset);
        buttonReset.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });

        var editGridBorder = UiUtil.MakeBorderForControl(editPanel);
        grid.Add(editGridBorder, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        _searchBox.TextChanged += (s, e) => vm.UpdateVisibleShortcuts(_searchBox.Text ?? string.Empty);
        Activated += delegate { _searchBox.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}