using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceWindow : Window
{
    private readonly MultipleReplaceViewModel _vm;

    public MultipleReplaceWindow(MultipleReplaceViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Multiple replace";
        Width = 910;
        Height = 640;
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(rulesView, 0, 0);
        grid.Add(fixesView, 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    private Border MakeFixesView(MultipleReplaceViewModel vm)
    {
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = UiUtil.MakeLabel("fixes view"),
        };

        return border;
    }

    private Border MakeRulesView(MultipleReplaceViewModel vm)
    {
        var treeView = new TreeView
        {
            Margin = new Thickness(10),
            SelectionMode = SelectionMode.Single,
            DataContext = vm,
        };

        treeView[!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Nodes));
        treeView[!TreeView.SelectedItemProperty] = new Binding(nameof(vm.SelectedNode));

        var factory = new FuncTreeDataTemplate<RuleTreeNode>(
            node => true,
            (node, _) =>
            {
                var textBlock = new TextBlock();
                textBlock.DataContext = node;
                textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(RuleTreeNode.Title))
                {
                    Mode = BindingMode.TwoWay,
                    Source = node,
                });

                return textBlock;
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
            BorderBrush = UiUtil.GetBorderColor(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = scrollViewer,
        };

        return border;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
