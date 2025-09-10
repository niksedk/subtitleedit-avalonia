using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceWindow : Window
{
    private readonly MultipleReplaceViewModel _vm;

    public MultipleReplaceWindow(MultipleReplaceViewModel vm)
    {
        UiUtil.InitializeWindow(this);
        Title = Se.Language.Edit.MultipleReplace.Title;
        Width = 1010;
        Height = 740;
        MinWidth = 400;
        MinHeight = 300;
        CanResize = true;
        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var rulesView = MakeRulesView(vm);
        var fixesView = MakeFixesView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // For the splitter
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 2,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        // Create the vertical splitter
        var splitter = new GridSplitter
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
            ResizeDirection = GridResizeDirection.Columns,
            Margin = new Thickness(0, 0, 0, 10),
        };

        grid.Add(rulesView, 0, 0);
        grid.Add(splitter, 0, 1);
        grid.Add(fixesView, 0, 2);
        grid.Add(panelButtons, 1, 0, 1, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private static Border MakeRulesView(MultipleReplaceViewModel vm)
    {
        var treeView = new TreeView
        {
            Margin = new Thickness(5),
            SelectionMode = SelectionMode.Single,
            DataContext = vm,
            MinWidth = 300,
        };

        treeView[!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Nodes));
        treeView[!TreeView.SelectedItemProperty] = new Binding(nameof(vm.SelectedNode));

        var factory = new FuncTreeDataTemplate<RuleTreeNode>(_ => true, (node, _) =>
            {
                var checkBox = new CheckBox
                {
                    DataContext = node
                };
                checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(RuleTreeNode.IsActive))
                {
                    Mode = BindingMode.TwoWay,
                    Source = node,
                });
                checkBox.IsCheckedChanged += vm.OnActiveChanged;

                if (node.IsCategory)
                {
                    var label = UiUtil.MakeLabel(string.Empty).WithBindText(node, nameof(RuleTreeNode.CategoryName));
                    label.FontWeight = FontWeight.Bold;
                    
                    var buttonCategoryActions = new Button
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Command = vm.NodeCategoryOpenContextMenuCommand,
                        CommandParameter = node,
                    };
                    Attached.SetIcon(buttonCategoryActions, IconNames.DotsVertical);

                    var gridCategory = new Grid
                    {
                        RowDefinitions =
                        {
                            new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                        },
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                        },
                        Width = double.NaN,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                    };
                    
                    gridCategory.Add(checkBox, 0,0);
                    gridCategory.Add(label, 0, 1);                    
                    gridCategory.Add(buttonCategoryActions, 0, 2);                    
                    
                    return gridCategory;
                }

                var grid = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    },
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    },
                    Width = double.NaN,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };

                var labelFind = UiUtil.MakeLabel(string.Empty).WithBindText(node, nameof(RuleTreeNode.Find));
                var labelSeparator = UiUtil.MakeLabel(string.Empty);
                Attached.SetIcon(labelSeparator, IconNames.ArrowRightThick);
                var labelReplaceWith = UiUtil.MakeLabel(string.Empty).WithBindText(node, nameof(RuleTreeNode.ReplaceWith));

                var labelIcon = new Label();
                Attached.SetIcon(labelIcon, node.IconName);

                var buttonActions = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Command = vm.NodeOpenContextMenuCommand,
                    CommandParameter = node,
                };
                Attached.SetIcon(buttonActions, IconNames.DotsVertical);

                var panelFindAndReplaceWith = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        labelIcon,
                        labelFind,
                        labelSeparator,
                        labelReplaceWith,
                    }
                };

                var labelDescription = UiUtil.MakeLabel(string.Empty).WithBindText(node, nameof(RuleTreeNode.Description));
                labelDescription.Opacity = 0.7;
                labelDescription.FontStyle = FontStyle.Italic;

                grid.Add(checkBox, 0, 0, 2, 1);
                grid.Add(panelFindAndReplaceWith, 0, 1);
                grid.Add(labelDescription, 1, 1);
                grid.Add(buttonActions, 0, 2, 2, 1);

                return grid;
            },
            node => node.SubNodes ?? []
        );

        treeView.ItemTemplate = factory;
        vm.RulesTreeView = treeView;
        treeView.SelectionChanged += vm.RulesTreeView_SelectionChanged;

        var scrollViewer = new ScrollViewer
        {
            Content = treeView,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderBrush(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = scrollViewer,
        };

        return border;
    }

    private static Border MakeFixesView(MultipleReplaceViewModel vm)
    {
        var editGrid = new Grid
        {
            RowDefinitions =
            {
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
            },
            Margin = new Thickness(0, 0, 0, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        }.WithBindVisible(vm, nameof(vm.IsEditPanelVisible));

        var labelFind = UiUtil.MakeLabel(Se.Language.General.Find);
        var textBoxFind = new TextBox
        {
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SelectedNode) + "." + nameof(RuleTreeNode.Find)) { Source = vm }
        };
        textBoxFind.TextChanged += vm.RuleTextChanged;
        
        var labelReplaceWith = UiUtil.MakeLabel(Se.Language.General.ReplaceWith);
        var textBoxReplaceWith = new TextBox
        {
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SelectedNode)+ "." + nameof(RuleTreeNode.ReplaceWith)) { Source = vm }
        };
        textBoxReplaceWith.TextChanged += vm.RuleTextChanged;
        
        var labelType = UiUtil.MakeLabel(Se.Language.General.Type);
        var comboBoxType = new ComboBox
        {
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedNode)+ "." + nameof(RuleTreeNode.Type)) { Source = vm }
        };
        comboBoxType.PropertyChanged += vm.RuleComboBoxChanged;
        
        editGrid.Add(labelFind, 0, 0);
        editGrid.Add(textBoxFind, 1, 0);
        
        editGrid.Add(labelReplaceWith, 0, 2);
        editGrid.Add(textBoxReplaceWith, 1, 2);
        
        editGrid.Add(labelType, 0, 3);
        editGrid.Add(comboBoxType, 1, 4);
        
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
            ItemsSource = vm.Fixes,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<MultipleReplaceFix>((item, _) =>
                        new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(MultipleReplaceFix.Apply)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MultipleReplaceFix.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MultipleReplaceFix.Before)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MultipleReplaceFix.After)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedFix)) { Source = vm });
        
        var gridFixes = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        gridFixes.Add(editGrid,0,0);
        gridFixes.Add(dataGrid,1,0);
        
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderBrush(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = gridFixes,
        };

        return border;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}