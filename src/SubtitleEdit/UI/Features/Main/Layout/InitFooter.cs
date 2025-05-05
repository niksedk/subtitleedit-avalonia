using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitFooter
{
    public static Grid Make(MainViewModel vm)
    {
        // grid with two colums, one left and one right, left is left aligned and right is right aligned
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            RowDefinitions = new RowDefinitions("Auto")
        };
        grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
        grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        grid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
        grid.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
        grid.Margin = new Thickness(0, 0, 0, 0);
        grid.HorizontalAlignment = HorizontalAlignment.Stretch;
        grid.VerticalAlignment = VerticalAlignment.Bottom;
        vm.StatusTextLeftLabel = new TextBlock
        {
            Text = string.Empty,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
        };
        grid.Children.Add(vm.StatusTextLeftLabel);
        vm.StatusTextLeftLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusTextLeft)));


        var right = new TextBlock
        {
            Text = "00:00:00 / 00:05:00 | EN subtitles | Saved",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 4),
            DataContext = vm,
        };
        right.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusTextRight)));


        grid.Children.Add(right);
        grid.Children[0].SetValue(Grid.ColumnProperty, 0);
        grid.Children[1].SetValue(Grid.ColumnProperty, 1);
        grid.Children[0].SetValue(Grid.RowProperty, 0);
        grid.Children[1].SetValue(Grid.RowProperty, 0);
        grid.Children[0].SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        grid.Children[1].SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        grid.Children[0].SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Center);
        grid.Children[1].SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Center);
        grid.Children[0].SetValue(Grid.MarginProperty, new Thickness(10, 4));

        return grid;
    }
    
}