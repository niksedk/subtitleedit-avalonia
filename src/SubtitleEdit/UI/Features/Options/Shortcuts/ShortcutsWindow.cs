using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
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
            node => node.SubNodes
        );

        treeView.ItemTemplate = factory;       
        vm.ShortcutsTreeView = treeView;
        treeView.SelectionChanged += vm.ShortcutsTreeView_SelectionChanged;

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
