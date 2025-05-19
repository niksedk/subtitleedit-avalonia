using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public class ShortcutsWindow : Window
{
    private TextBox _searchBox;
    private ShortcutsViewModel _vm;

    public ShortcutsWindow(ShortcutsViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Shortcuts";
        Width = 650;
        Height = 650;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        _searchBox = new TextBox
        {
            Watermark = "Search shortcuts...",
            Margin = new Thickness(10),
        };

        var treeView = new TreeView
        {
            Margin = new Thickness(10),
            SelectionMode = SelectionMode.Single,
            DataContext = vm,
        };

        treeView[!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Nodes));
        treeView[!TreeView.SelectedItemProperty] = new Binding(nameof(vm.SelectedNode));
        
          
        // Create the tree data template with the correct constructor parameters
        var factory = new FuncTreeDataTemplate<ShortcutTreeNode>(
            // Match function - determines if this template applies to the item
            node => true,
            // Build function - creates the visual for each node
            (node, _) => new TextBlock { Text = node.Title }, // Assuming nodes have a Name property
            // ItemsSelector function - tells TreeView how to find child nodes
            node => node.SubNodes
        );

        treeView.ItemTemplate = factory;
        
        vm.ShortcutsGrid = treeView;

        // if (vm.ShortcutsSource is HierarchicalTreeDataGridSource<ShortcutItem> source)
        // {
        //     source.RowSelection!.SelectionChanged += (sender, e) =>
        //     {
        //         vm.ShortcutGrid_SelectionChanged(source.RowSelection.SelectedItems);
        //     };
        // }

        var scrollViewer = new ScrollViewer
        {
            Content = treeView,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var buttonOk = UiUtil.MakeButton("OK", vm.CommandOkCommand);
        var buttonCancel = UiUtil.MakeButton("Cancel", vm.CommandCancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = new Thickness(UiUtil.WindowMarginWidth),
        };
        grid.Children.Add(_searchBox);
        Grid.SetRow(_searchBox, 0);
        Grid.SetColumn(_searchBox, 0);

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
        editPanel.Children.Add(UiUtil.MakeCheckBox());

        // Alt checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock("Alt").WithMarginRight(3));
        editPanel.Children.Add(UiUtil.MakeCheckBox());

        // Shift checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock("Shift").WithMarginRight(3));
        editPanel.Children.Add(new CheckBox
        {
            Name = "ShiftCheckBox",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });

        // Key combobox
        var comboBoxKeys = new ComboBox
        {
            Name = "KeyComboBox",
            Width = 100,
            Margin = new Thickness(10, 0, 10, 0),
            ItemsSource = Enum.GetValues(typeof(Key)).Cast<Key>(),
        };
        editPanel.Children.Add(comboBoxKeys);

        comboBoxKeys.Bind(
            IsEnabledProperty,
            new Avalonia.Data.Binding(nameof(vm.ControlsEnabled)) { Source = vm }
        );


        // Update button
        editPanel.Children.Add(UiUtil.MakeButton("Update"));


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
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}

public class ShortcutItem
{
    public ShortcutCategory Category { get; set; }
    public string CategoryText { get; set; }
    public string Name { get; set; }
    public string Keys { get; set; }
    public bool IsExpanded { get; set; }
    public ShortCut Shortcut { get; set; }
    public ObservableCollection<ShortcutItem> Children { get; } = new();

    public ShortcutItem()
    {
        CategoryText = string.Empty;
        Name = string.Empty;
        Keys = string.Empty;
        IsExpanded = false;
    }
}

public enum ShortcutCategory
{
    General,
    SubtitleGridAndTextBox,
    Waveform,
    SubtitleGrid,
}