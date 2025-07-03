using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Projektanker.Icons.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class NOcrCharacterAddWindow : Window
{
    private readonly NOcrCharacterAddViewModel _vm;

    public NOcrCharacterAddWindow(NOcrCharacterAddViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        Icon = UiUtil.GetSeIcon();
        Title = "";
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Title + collapse/expand buttons
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Image
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // border with settings
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
        };
        
        var labelTitle = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.Title));

        var buttonCollapse = UiUtil.MakeButton("Collapse", vm.ShrinkCommand).WithBindIsVisible(nameof(vm.CanShrink));
        var buttonExpand = UiUtil.MakeButton("Expand", vm.ExpandCommand).WithBindIsVisible(nameof(vm.ExpandCommand));
        var panelCollapseAndExpand = UiUtil.MakeButtonBar(buttonCollapse, buttonExpand); 

        
        grid.Add(labelTitle, 0, 0);
        grid.Add(panelCollapseAndExpand, 0, 1);
        
        Content = grid;

        Activated += delegate
        {
            Focus(); // hack to make OnKeyDown work
        };
    }
}
