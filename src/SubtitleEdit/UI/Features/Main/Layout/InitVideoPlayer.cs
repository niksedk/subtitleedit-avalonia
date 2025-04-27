using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class InitVideoPlayer
{
    public static Grid MakeLayoutVideoPlayer(MainView mainPage, MainViewModel vm)
    {
        // Create main layout grid
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto")
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
        vm.VideoPlayer = new Grid
        {
            Margin = new Thickness(10),
            Background = Brushes.Black // Placeholder background
        };
        Grid.SetRow(vm.VideoPlayer, 1);
        mainGrid.Children.Add(vm.VideoPlayer);

        // Footer
        var footerGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            Margin = new Thickness(10)
        };

        // Toolbar 1: Navigation bar (e.g., time slider)
        var navigationBar = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Height = 20
        };
        Grid.SetRow(navigationBar, 0);
        footerGrid.Children.Add(navigationBar);

        // Toolbar 2: Control buttons (Play, Stop, Volume, Fullscreen)
        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Play.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Stop.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/VolumeBarBackground.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Fullscreen.png"));

        Grid.SetRow(controlsPanel, 1);
        footerGrid.Children.Add(controlsPanel);

        Grid.SetRow(footerGrid, 2);
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