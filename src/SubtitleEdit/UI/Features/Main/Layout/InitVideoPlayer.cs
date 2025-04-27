using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class InitVideoPlayer
{
    public static Grid MakeLayoutVideoPlayer(MainView mainPage, MainViewModel vm)
    {
        // Create main layout grid
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),  // Simplified to 3 rows
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0),
        };

        // Video player area
        if (vm.LibVLC == null || vm.VideoPlayer == null)
        {
            vm.MediaPlayer?.Dispose();
            vm.LibVLC?.Dispose();
            vm.LibVLC = new LibVLC();
            vm.MediaPlayer = new MediaPlayer(vm.LibVLC);
            vm.VideoPlayer = new VideoView
            {
                Margin = new Thickness(0),
                MediaPlayer = vm.MediaPlayer,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
        }
        else
        {
            var grid = vm.VideoPlayer.Parent as Grid;
            grid?.Children.Remove(vm.VideoPlayer);
        }

        Grid.SetRow(vm.VideoPlayer, 0);
        mainGrid.Children.Add(vm.VideoPlayer);

        // Footer
        var footerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*"),
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            Margin = new Thickness(10, 0),
        };

        // Navigation bar (e.g., time slider)
        var navigationBar = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Margin = new Thickness(0),            
        };
        Grid.SetRow(navigationBar, 0);  // First row of footer grid
        footerGrid.Children.Add(navigationBar);

        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 0,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 0),
        };
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Play.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Stop.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/VolumeBarBackground.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Fullscreen.png"));
        Grid.SetRow(controlsPanel, 1);  // Second row of footer grid
        footerGrid.Children.Add(controlsPanel);

        // Add footer to main grid
        Grid.SetRow(footerGrid, 1);  // Third row of main grid
        mainGrid.Children.Add(footerGrid);

        return mainGrid;
    }

    // Helper method to create a button with an image
    private static Button CreateButtonWithIcon(string iconPath)
    {
        return new Button
        {
            Content = new Image
            {
                Source = new Bitmap(iconPath),
                Width = 32,
                Height = 32,
            },
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Margin = new Thickness(0),
        };
    }
}