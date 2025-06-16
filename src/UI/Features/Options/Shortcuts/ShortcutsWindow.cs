using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public class ShortcutsWindow : Window
{
    private TextBox _searchBox;
    private ShortcutsViewModel _vm;

    public ShortcutsWindow(ShortcutsViewModel vm)
    {
        var language = Se.Language.Options.Shortcuts;
        Icon = UiUtil.GetSeIcon();
        Title = language.Title;
        Width = 700;
        Height = 650;
        MinWidth = 650;
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
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        };
        _searchBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });

        var labelFilter = UiUtil.MakeTextBlock(language.Filter);
        var comboBoxFilter = UiUtil.MakeComboBox(vm.Filters, vm, nameof(vm.SelectedFilter))
            .WithMinWidth(120)
            .WithMargin(5, 0, 10, 0);
        comboBoxFilter.SelectionChanged += vm.ComboBoxFilter_SelectionChanged;

        var buttonExpand = new Button
        {
            Margin = new Thickness(0, 10, 0, 10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Command = vm.ExpandCommand,
        };
        Attached.SetIcon(buttonExpand, "fa-solid fa-plus");

        var buttonCollapse = new Button
        {
            Margin = new Thickness(5, 10, 0, 10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Command = vm.CollapseCommand,
        };
        Attached.SetIcon(buttonCollapse, "fa-solid fa-minus");

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

        topGrid.Children.Add(buttonExpand);
        Grid.SetRow(buttonExpand, 0);
        Grid.SetColumn(buttonExpand, 3);

        topGrid.Children.Add(buttonCollapse);
        Grid.SetRow(buttonCollapse, 0);
        Grid.SetColumn(buttonCollapse, 4);


        var treeView = new TreeView
        {
            Margin = new Thickness(10),
            SelectionMode = SelectionMode.Single,
            DataContext = vm,
        };

        treeView[!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Nodes));
        treeView[!TreeView.SelectedItemProperty] = new Binding(nameof(vm.SelectedNode));

        var factory = new FuncTreeDataTemplate<ShortcutTreeNode>(
            node => true,
            (node, _) =>
            {
                var textBlock = new TextBlock();
                textBlock.DataContext = node;
                textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(ShortcutTreeNode.Title))
                {
                    Mode = BindingMode.TwoWay,
                    Source = node,
                });

                return textBlock;
            },
            node => node.SubNodes ?? []
        );

        treeView.ItemTemplate = factory;
        vm.ShortcutsTreeView = treeView;
        treeView.SelectionChanged += vm.ShortcutsTreeView_SelectionChanged;

        var scrollViewer = new ScrollViewer
        {
            Content = treeView,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.CommandOkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = new Thickness(UiUtil.WindowMarginWidth),
        };
        grid.Children.Add(topGrid);
        Grid.SetRow(topGrid, 0);
        Grid.SetColumn(topGrid, 0);

        grid.Children.Add(scrollViewer);
        Grid.SetRow(scrollViewer, 1);
        Grid.SetColumn(scrollViewer, 0);

        var editPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(10),
        };

        // Control checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock("Control").WithMarginRight(3));
        var controlCheckBox = UiUtil.MakeCheckBox(vm, nameof(vm.CtrlIsSelected));
        controlCheckBox.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(controlCheckBox);

        // Alt checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock("Alt").WithMarginRight(3));
        var checkBoxAlt = UiUtil.MakeCheckBox(vm, nameof(vm.AltIsSelected));
        checkBoxAlt.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(checkBoxAlt);


        // Shift checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock("Shift").WithMarginRight(3));
        var checkBoxShift = UiUtil.MakeCheckBox(vm, nameof(vm.ShiftIsSelected));
        checkBoxShift.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(checkBoxShift);

        // Key combobox
        var comboBoxKeys = new ComboBox
        {
            Width = 200,
            Margin = new Thickness(10, 0, 10, 0),
            //ItemsSource = Enum.GetValues(typeof(Key)).Cast<Key>(),
        };
        comboBoxKeys.Bind(ComboBox.ItemsSourceProperty, new Binding(nameof(vm.Shortcuts)) { Source = vm });
        comboBoxKeys.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(vm.SelectedShortcut)) { Source = vm });
        comboBoxKeys.Bind(ComboBox.IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(comboBoxKeys);

        // Update button
        var buttonUpdate = UiUtil.MakeButton("Update", vm.UpdateShortcutCommand);
        editPanel.Children.Add(buttonUpdate);
        buttonUpdate.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });

        // Reset button
        var buttonReset = UiUtil.MakeButton("Reset", vm.ResetShortcutCommand);
        editPanel.Children.Add(buttonReset);
        buttonReset.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });


        var editGridBorder = UiUtil.MakeBorderForControl(editPanel);
        grid.Children.Add(editGridBorder);
        Grid.SetRow(editGridBorder, 2);
        Grid.SetColumn(editGridBorder, 0);

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, 3);
        Grid.SetColumn(buttonPanel, 0);

        Content = grid;

        _searchBox.TextChanged += (s, e) => vm.UpdateVisibleShortcuts(_searchBox.Text ?? string.Empty);

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
