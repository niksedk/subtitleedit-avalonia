using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Nikse.SubtitleEdit.Features.Main;

public static class ViewToolbar
{
    public static Border Make(MainViewModel vm)
    {
        return new Border
        {
            Background = Brushes.Black,
            Height = 40,
            Child = CreateToolbar(vm),
        };
    }
    
    private static StackPanel CreateToolbar(MainViewModel vm)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Background = Brushes.Black,
            Margin = new Avalonia.Thickness(5),
            Children =
            {
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/New.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/Open.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/Save.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/SaveAs.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Border
                {
                    Width = 1,
                    Background = Brushes.Gray,
                    Margin = new Avalonia.Thickness(5, 5, 5, 5),
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/Find.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/Replace.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Border
                {
                    Width = 1,
                    Background = Brushes.Gray,
                    Margin = new Avalonia.Thickness(5, 5, 5, 5),
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/SpellCheck.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/Settings.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/Layout.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Border
                {
                    Width = 1,
                    Background = Brushes.Gray,
                    Margin = new Avalonia.Thickness(5, 5, 5, 5),
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/Help.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
            }
        };
    }
}