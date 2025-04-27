using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class InitWaveform
{
    public static Grid MakeWaveform(MainView mainPage, MainViewModel vm)
    {
        // Create main layout grid
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto")
        };

        // waveform area
        vm.Waveform = new Grid
        {
            Margin = new Thickness(10),
            Background = Brushes.Black,
            Children =
            {
                new TextBox
                {
                    Text = "Waveform",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Foreground = Brushes.White,
                    FontSize = 24,
                    IsHitTestVisible = false
                }
            }
        };
        Grid.SetRow(vm.Waveform, 0);
        mainGrid.Children.Add(vm.Waveform);

        // Footer
        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Play.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Stop.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/VolumeBarBackground.png"));
        controlsPanel.Children.Add(CreateButtonWithIcon("Assets/Themes/Dark/VideoPlayer/Fullscreen.png"));

        mainGrid.Children.Add(controlsPanel);
        Grid.SetRow(controlsPanel, 1);

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