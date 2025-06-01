using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertWindow : Window
{
    private BatchConvertViewModel _vm;
    
    public BatchConvertWindow(BatchConvertViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Batch convert";
        Width = 910;
        Height = 740;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var fileView = MakeFileView(vm);
        var functionsListView = MakeFunctionsListView(vm);
        var functionView = MakeFunctionView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonOk,
            UiUtil.MakeButton("Cancel", vm.CancelCommand)
        );
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

        grid.Add(fileView, 0, 0, 1, 2);
        grid.Add(functionsListView, 1, 0);
        grid.Add(functionView, 1, 1);
        grid.Add(buttonPanel, 2, 0, 1, 2);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Border MakeFileView(BatchConvertViewModel vm)
    {
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = UiUtil.MakeLabel("file view"),
        };  

        return border;  
    }

    private Border MakeFunctionsListView(BatchConvertViewModel vm)
    {
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = UiUtil.MakeLabel("functions view"),
        };

        return border;
    }

    private Border MakeFunctionView(BatchConvertViewModel vm)
    {
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = UiUtil.MakeLabel("function options"),
        };

        return border;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
