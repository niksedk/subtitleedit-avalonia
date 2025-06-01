using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Net.WebSockets;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceWindow : Window
{
    private MultipleReplaceViewModel _vm;
    
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
        var FixesView = MakeFixesView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);    
        var panelButtons = UiUtil.MakeButtonBar(
            buttonOk,
            UiUtil.MakeButton("Cancel", vm.CancelCommand)
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
        grid.Add(FixesView, 0, 1);
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
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = UiUtil.MakeLabel("rules view"),
        };

        return border;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
