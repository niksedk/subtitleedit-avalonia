using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;
using Avalonia.Controls.Templates;
using Avalonia.Data;

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
        MinWidth = 550;
        MinHeight = 500;
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
        
        var factory = new FuncTreeDataTemplate<ShortcutTreeNode>(
            // Match function - determines if this template applies to the item
            node => true,
            // Build function - creates the visual for each node with proper binding
            (node, _) => 
            {
                var textBlock = new TextBlock();
                textBlock.DataContext = node;
                // Set up the binding for Text property
                textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(ShortcutTreeNode.Title)) 
                { 
                    
                    // You can specify additional binding properties if needed
                    Mode = BindingMode.TwoWay,
                    Source = node,
                });
        
                return textBlock;
            },
            // ItemsSelector function - tells TreeView how to find child nodes
            node => node.SubNodes
        );

// Set the ItemTemplate
        treeView.ItemTemplate = factory;
        
        vm.ShortcutsTreeView = treeView;

        treeView.SelectionChanged += vm.ShortcutsTreeView_SelectionChanged;

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