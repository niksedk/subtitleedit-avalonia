using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class InitWaveform
{
    public static Grid MakeWaveform(MainView mainPage, MainViewModel vm)
    {
        // Create main layout grid
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(10, 0, 10, 0),
        };

        // waveform area
        if (vm.AudioVisualizer == null)
        {
            vm.AudioVisualizer = new AudioVisualizer
            {
                DrawGridLines = Se.Settings.Waveform.DrawGridLines,
            };
            vm.AudioVisualizer.OnNewSelectionInsert += vm.AudioVisualizerOnNewSelectionInsert;
            vm.AudioVisualizer.OnVideoPositionChanged += vm.AudioVisualizerOnVideoPositionChanged;
            vm.AudioVisualizer.OnStatus += vm.AudioVisualizerOnStatus;
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
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        controlsPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsWaveformToolbarVisible)));

        var buttonPlay = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
        };
        Attached.SetIcon(buttonPlay, "fa-solid fa-play");

        var iconHorizontal = new Icon
        {
            Value = "fa-solid fa-arrows-left-right",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 4, 0)
        };

        var sliderHorizontalZoom = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center
        };
        sliderHorizontalZoom.TemplateApplied += (s, e) =>
        {
            if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
            {
                thumb.Width = 14;
                thumb.Height = 14;
            }
        };

        var iconVertical = new Icon
        {
            Value = "fa-solid fa-arrows-up-down",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 4, 0)
        };

        var sliderVerticalZoom = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center
        };
        sliderVerticalZoom.TemplateApplied += (s, e) =>
        {
            if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
            {
                thumb.Width = 14;
                thumb.Height = 14;
            }
        };


        var buttonMore = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
        };
        Attached.SetIcon(buttonMore, "fa-ellipsis-v");

        var labelAutoSelectOnPlay = UiUtil.MakeTextBlock("Select current line when playing");
        var checkBoxAutoSelectOnPlay = UiUtil.MakeCheckBox();


        controlsPanel.Children.Add(buttonPlay);
        controlsPanel.Children.Add(UiUtil.MakeSeparatorForHorizontal());
        controlsPanel.Children.Add(iconHorizontal);
        controlsPanel.Children.Add(sliderHorizontalZoom);
        controlsPanel.Children.Add(iconVertical);
        controlsPanel.Children.Add(sliderVerticalZoom);

        controlsPanel.Children.Add(labelAutoSelectOnPlay);
        controlsPanel.Children.Add(checkBoxAutoSelectOnPlay);

        controlsPanel.Children.Add(buttonMore);

        mainGrid.Children.Add(controlsPanel);
        Grid.SetRow(controlsPanel, 1);

        return mainGrid;
    }
}