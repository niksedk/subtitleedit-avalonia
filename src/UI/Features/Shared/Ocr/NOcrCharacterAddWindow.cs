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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
           // HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        Content = grid;

        Activated += delegate
        {
            Focus(); // hack to make OnKeyDown work
        };
    }
}
