using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class LayoutWindow : Window
{
    private LayoutModel _vm;
    private List<Border> _borders = new List<Border>();
    private int _focusedLayout = -1;

    public LayoutWindow(LayoutModel viewModel)
    {
        _vm = viewModel;

        Title = "Choose layout";
        Width = 925;
        Height = 500;
        CanResize = false;

        var wrapPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10)
        };

        for (var i = 1; i <= 12; i++)
        {
            var image = new Image
            {
                Source = new Bitmap($"Assets/Layout/Layout{i:D2}.png"),
                Width = 200,
                Height = 139
            };

            var text = new TextBlock
            {
                Text = i.ToString(CultureInfo.InvariantCulture),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White,
                FontSize = 34,
                FontWeight = FontWeight.Bold,
                Opacity = 0.7,
            };

            // Layer the image and text
            var grid = new Grid
            {
                Width = 200,
                Height = 139
            };

            grid.Children.Add(image);
            grid.Children.Add(text);

            var border = new Border
            {
                Margin = new Thickness(10),
                Child = grid,
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                RenderTransform = new ScaleTransform(1, 1)
            };

            var layoutNumber = i;

            border.PointerEntered += (_, __) =>
            {
                border.RenderTransform = new ScaleTransform(1.1, 1.1);
                border.Background = Brushes.DarkSlateGray;
                _focusedLayout = layoutNumber;
            };

            border.PointerExited += (_, __) =>
            {
                border.RenderTransform = new ScaleTransform(1.0, 1.0);
                border.Background = Brushes.Transparent;
                _focusedLayout = -1;
            };

            border.PointerPressed += (_, __) =>
            {
                _vm.SelectedLayout = layoutNumber;
                Close();
            };

            wrapPanel.Children.Add(border);
            _borders.Add(border);
        }

        Content = new ScrollViewer { Content = wrapPanel };
    }

    protected override async void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }

        var layoutLookup = new Dictionary<Key, int>
        {
            { Key.D1, 0 },
            { Key.D2, 1 },
            { Key.D3, 2 },
            { Key.D4, 3 },
            { Key.D5, 4 },
            { Key.D6, 5 },
            { Key.D7, 6 },
            { Key.D8, 7 },
            { Key.D9, 8 },
        };
        if (layoutLookup.TryGetValue(e.Key, out var layoutNumber))
        {
            var fl = _focusedLayout - 1;
            if (fl >= 0)
            {
                _borders[fl].RenderTransform = new ScaleTransform(1.0, 1.0);
                _borders[fl].Background = Brushes.Transparent;
            }

            _borders[layoutNumber].RenderTransform = new ScaleTransform(1.1, 1.1);
            _borders[layoutNumber].Background = Brushes.DarkSeaGreen;
            await Task.Delay(500);

            _vm.SelectedLayout = layoutNumber;
            Close();
        }
    }
}