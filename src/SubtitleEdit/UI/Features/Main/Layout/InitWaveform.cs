using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class InitWaveform
{
    public static Grid MakeWaveform(MainView mainPage, MainViewModel vm)
    {
        // Create main layout grid
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(10),
        };

        // waveform area
        if (vm.AudioVisualizer == null)
        {
            vm.AudioVisualizer = new AudioVisualizer() { ShowGridLines = true };
        }
        else
        {
            UiUtil.RemoveControlFromParent(vm.AudioVisualizer);
        }

        Grid.SetRow(vm.AudioVisualizer, 0);
        mainGrid.Children.Add(vm.AudioVisualizer);

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