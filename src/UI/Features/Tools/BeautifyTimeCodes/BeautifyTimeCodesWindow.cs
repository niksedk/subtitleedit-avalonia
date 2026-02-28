using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;

public class BeautifyTimeCodesWindow : Window
{
    public BeautifyTimeCodesWindow(BeautifyTimeCodesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BeautifyTimeCodes.Title;
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 900;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        // Settings panel
        var checkBoxSnapToFrames = new CheckBox
        {
            Content = "Snap to Frames",
            [!CheckBox.IsCheckedProperty] = new Avalonia.Data.Binding("Settings.SnapToFrames") { Source = vm, Mode = BindingMode.TwoWay },
        };

        var labelFrameGap = new TextBlock
        {
            Text = "Frame Gap:",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(15, 0, 5, 0),
        };

        var numericFrameGap = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 10,
            Increment = 1,
            FormatString = "0",
            Width = 80,
            [!NumericUpDown.ValueProperty] = new Avalonia.Data.Binding("Settings.FrameGap") { Source = vm, Mode = BindingMode.TwoWay },
        };
        numericFrameGap.ValueChanged += vm.ValueChanged;

        var labelShotChangeThreshold = new TextBlock
        {
            Text = "Shot Change Threshold (ms):",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(15, 0, 5, 0),
        };

        var numericShotChangeThreshold = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Increment = 10,
            FormatString = "0",
            Width = 80,
            [!NumericUpDown.ValueProperty] = new Avalonia.Data.Binding("Settings.ShotChangeThresholdMs") { Source = vm, Mode = BindingMode.TwoWay },
        };
        numericShotChangeThreshold.ValueChanged += vm.ValueChanged;

        var labelShotChangeOffset = new TextBlock
        {
            Text = "Shot Change Offset (frames):",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(15, 0, 5, 0),
        };

        var numericShotChangeOffset = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 10,
            Increment = 1,
            FormatString = "0",
            Width = 80,
            [!NumericUpDown.ValueProperty] = new Avalonia.Data.Binding("Settings.ShotChangeOffsetFrames") { Source = vm, Mode = BindingMode.TwoWay },
        };
        numericShotChangeOffset.ValueChanged += vm.ValueChanged;

        var labelMinDuration = new TextBlock
        {
            Text = "Min Duration (ms):",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(15, 0, 5, 0),
        };

        var numericMinDuration = new NumericUpDown
        {
            Minimum = 100,
            Maximum = 10000,
            Increment = 100,
            FormatString = "0",
            Width = 80,
            [!NumericUpDown.ValueProperty] = new Avalonia.Data.Binding("Settings.MinDurationMs") { Source = vm, Mode = BindingMode.TwoWay },
        };
        numericMinDuration.ValueChanged += vm.ValueChanged;

        var settingsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto"),
            ColumnSpacing = 5,
        };
        settingsGrid.Add(checkBoxSnapToFrames, 0);
        settingsGrid.Add(labelFrameGap, 1);
        settingsGrid.Add(numericFrameGap, 2);
        settingsGrid.Add(labelShotChangeThreshold, 3);
        settingsGrid.Add(numericShotChangeThreshold, 4);
        settingsGrid.Add(labelShotChangeOffset, 5);
        settingsGrid.Add(numericShotChangeOffset, 6);
        settingsGrid.Add(labelMinDuration, 7);
        settingsGrid.Add(numericMinDuration, 8);

        var settingsTitle = new TextBlock
        {
            Text = "Beautify Settings",
            FontWeight = FontWeight.Bold,
            FontSize = 14,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var settingsPanel = new StackPanel
        {
            Spacing = 10,
        };
        settingsPanel.Children.Add(settingsTitle);
        settingsPanel.Children.Add(settingsGrid);

        var settingsBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Avalonia.Thickness(1),
            Padding = new Avalonia.Thickness(10),
            CornerRadius = new Avalonia.CornerRadius(4),
            Child = settingsPanel,
        };

        // Audio visualizers
        var labelOriginal = new TextBlock
        {
            Text = "Original",
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(5),
        };

        var audioVisualizerOriginal = new Controls.AudioVisualizerControl.AudioVisualizer
        {
            IsReadOnly = true,
            DrawGridLines = true,
        };

        var borderOriginal = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Avalonia.Thickness(1),
            Background = Brushes.Black,
            Child = audioVisualizerOriginal,
        };

        var labelBeautified = new TextBlock
        {
            Text = "Beautified",
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(5),
        };

        var audioVisualizerBeautified = new Controls.AudioVisualizerControl.AudioVisualizer
        {
            IsReadOnly = true,
            DrawGridLines = true,
        };

        var borderBeautified = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Avalonia.Thickness(1),
            Background = Brushes.Black,
            Child = audioVisualizerBeautified,
        };

        // Set up visualizers in ViewModel
        vm.AudioVisualizerOriginal = audioVisualizerOriginal;
        vm.AudioVisualizerBeautified = audioVisualizerBeautified;

        // Sync scroll and zoom changes from original to beautified
        audioVisualizerOriginal.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name == "StartPositionSeconds")
                audioVisualizerBeautified.StartPositionSeconds = audioVisualizerOriginal.StartPositionSeconds;
            else if (e.Property.Name == "ZoomFactor")
                audioVisualizerBeautified.ZoomFactor = audioVisualizerOriginal.ZoomFactor;
            else if (e.Property.Name == "VerticalZoomFactor")
                audioVisualizerBeautified.VerticalZoomFactor = audioVisualizerOriginal.VerticalZoomFactor;
        };

        var visualizerGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,10,Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        visualizerGrid.Add(labelOriginal, 0, 0);
        visualizerGrid.Add(borderOriginal, 0, 1);
        visualizerGrid.Add(labelBeautified, 0, 3);
        visualizerGrid.Add(borderBeautified, 0, 4);

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        // Main grid
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
        };
        grid.Add(settingsBorder, 0, 0);
        grid.Add(visualizerGrid, 0, 1);
        grid.Add(panelButtons, 0, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
