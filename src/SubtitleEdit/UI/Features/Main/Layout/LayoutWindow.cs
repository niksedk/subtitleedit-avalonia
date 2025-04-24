using System;
using System.Globalization;
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

            var border = new Border
            {
                Margin = new Thickness(10),
                Child = image,
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                RenderTransform = new ScaleTransform(1, 1)
            };

            border.PointerEntered += (_, __) =>
            {
                border.RenderTransform = new ScaleTransform(1.1, 1.1);
                border.Background = Brushes.DarkSlateGray;
            };

            border.PointerExited += (_, __) =>
            {
                border.RenderTransform = new ScaleTransform(1.0, 1.0);
                border.Background = Brushes.Transparent;
            };

            var layoutNumber = i;
            border.PointerPressed += (_, __) =>
            {
                _vm.SelectedLayout = layoutNumber;
                Close();
            };

            wrapPanel.Children.Add(border);
        }

        Content = new ScrollViewer { Content = wrapPanel };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }


    public class HoverToBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool)value! ? Brushes.LightGray : Brushes.Transparent;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class HoverToScaleTransformConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool)value! ? new ScaleTransform(1.1, 1.1) : new ScaleTransform(1.0, 1.0);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}