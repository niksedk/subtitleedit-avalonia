using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Nikse.SubtitleEdit.Features.Main;

public static class ViewContent
{
    public static Grid Make(MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

// Top content (will hold the nested grid)
        var topContent = new Border
        {
            Background = Brushes.OrangeRed
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

// Create a nested grid with columns (for vertical split)
        var nestedGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

// Left part of nested grid
        var nestedLeft = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "List and edit", 
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetColumn(nestedLeft, 0);
        nestedGrid.Children.Add(nestedLeft);

// Vertical GridSplitter
        var nestedSplitter = new GridSplitter
        {
            Background = Brushes.Black,
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(nestedSplitter, 1);
        nestedGrid.Children.Add(nestedSplitter);

// Right part of nested grid
        var nestedRight = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Video player", 
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetColumn(nestedRight, 2);
        nestedGrid.Children.Add(nestedRight);

// Add nested grid to top content
        topContent.Child = nestedGrid;

// Main GridSplitter (horizontal)
        var splitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

// Bottom content
        var bottomContent = new Border
        {
            Background = Brushes.MediumPurple,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Waveform", 
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);


        return new Grid
        {
            Background = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children = { contentGrid }
        };
    }
}