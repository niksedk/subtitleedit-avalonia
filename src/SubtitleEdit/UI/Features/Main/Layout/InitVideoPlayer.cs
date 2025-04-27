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
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),  // Simplified to 3 rows
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Width = double.NaN,  // This tells Avalonia to size to parent
            Height = double.NaN,  // This tells Avalonia to size to parent
        };

        // Header
        var headerText = new TextBlock
        {
            Text = "File name:",
            FontSize = 14,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(10)
        };
        Grid.SetRow(headerText, 0);
        mainGrid.Children.Add(headerText);

        // Video player area
        vm.MediaPlayer?.Dispose();
        vm.LibVLC = new LibVLC();
        vm.MediaPlayer = new MediaPlayer(vm.LibVLC);
        vm.VideoPlayer = new VideoView
        {
            Margin = new Thickness(10),
            MediaPlayer = vm.MediaPlayer,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Width = double.NaN,  // This tells Avalonia to size to parent
            Height = double.NaN,  // This tells Avalonia to size to parent

        };
        Grid.SetRow(vm.VideoPlayer, 1);
        mainGrid.Children.Add(vm.VideoPlayer);

        // Footer
        var footerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*"),
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            Margin = new Thickness(10)
        };

        // Navigation bar (e.g., time slider)
        var navigationBar = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Height = 20
        };
        Grid.SetRow(navigationBar, 0);  // First row of footer grid
        footerGrid.Children.Add(navigationBar);

        // Control buttons (Play, Stop, Volume, Fullscreen)
        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Play.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Stop.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/VolumeBarBackground.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Fullscreen.png"));
        Grid.SetRow(controlsPanel, 1);  // Second row of footer grid
        footerGrid.Children.Add(controlsPanel);

        // Add footer to main grid
        Grid.SetRow(footerGrid, 2);  // Third row of main grid
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
                Height = 32
            },
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent
        };
    }
}