using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryResizeImages;

public class BinaryResizeImagesWindow : Window
{
    public BinaryResizeImagesWindow(BinaryResizeImagesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Resize images";
        Width = 800;
        Height = 600;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
            },
            Margin = UiUtil.MakeWindowMargin(),
        };

        // Content area with controls on left and preview on right
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(300)),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 10,
        };

        // Left side - controls
        var leftPanel = MakeControlsPanel(vm);
        contentGrid.Add(leftPanel, 0, 0);

        // Right side - preview
        var rightPanel = MakePreviewPanel(vm);
        contentGrid.Add(rightPanel, 0, 1);

        mainGrid.Add(contentGrid, 0, 0);

        // Button panel
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        mainGrid.Add(buttonPanel, 1, 0);

        Content = mainGrid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static StackPanel MakeControlsPanel(BinaryResizeImagesViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
        };

        // Resize mode
        var resizeModeLabel = new TextBlock
        {
            Text = "Resize mode:",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        panel.Children.Add(resizeModeLabel);

        var resizeModeCombo = new ComboBox
        {
            ItemsSource = vm.ResizeModes,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedResizeMode)) { Mode = BindingMode.TwoWay },
        };
        panel.Children.Add(resizeModeCombo);

        // Percentage
        var percentagePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 10, 0, 0),
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.IsPercentageVisible)),
        };

        var percentageLabel = new TextBlock
        {
            Text = "Percentage:",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        percentagePanel.Children.Add(percentageLabel);

        var percentageUpDown = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 1000,
            Increment = 10,
            FormatString = "0",
            Width = double.NaN,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.Percentage))
            {
                Mode = BindingMode.TwoWay,
                Converter = new NullableIntConverter(),
            },
        };
        percentagePanel.Children.Add(percentageUpDown);
        panel.Children.Add(percentagePanel);

        // Fixed size
        var fixedSizePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 10, 0, 0),
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.IsFixedSizeVisible)),
        };

        var widthLabel = new TextBlock
        {
            Text = "Width:",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        fixedSizePanel.Children.Add(widthLabel);

        var widthUpDown = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 10000,
            Increment = 10,
            FormatString = "0",
            Width = double.NaN,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.FixedWidth))
            {
                Mode = BindingMode.TwoWay,
                Converter = new NullableIntConverter(),
            },
        };
        fixedSizePanel.Children.Add(widthUpDown);

        var heightLabel = new TextBlock
        {
            Text = "Height:",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 5),
        };
        fixedSizePanel.Children.Add(heightLabel);

        var heightUpDown = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 10000,
            Increment = 10,
            FormatString = "0",
            Width = double.NaN,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.FixedHeight))
            {
                Mode = BindingMode.TwoWay,
                Converter = new NullableIntConverter(),
            },
        };
        fixedSizePanel.Children.Add(heightUpDown);

        var maintainAspectCheckBox = new CheckBox
        {
            Content = "Maintain aspect ratio",
            Margin = new Thickness(0, 10, 0, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.MaintainAspectRatio)) { Mode = BindingMode.TwoWay },
        };
        fixedSizePanel.Children.Add(maintainAspectCheckBox);

        panel.Children.Add(fixedSizePanel);

        // Filter quality
        var filterLabel = new TextBlock
        {
            Text = "Filter quality:",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 5),
        };
        panel.Children.Add(filterLabel);

        var filterCombo = new ComboBox
        {
            ItemsSource = vm.FilterQualities,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedFilterQuality)) { Mode = BindingMode.TwoWay },
        };
        panel.Children.Add(filterCombo);

        // Preview button
        var previewButton = UiUtil.MakeButton(vm.UpdatePreviewCommand, IconNames.Refresh);
        previewButton.Margin = new Thickness(0, 20, 0, 0);
        previewButton.HorizontalAlignment = HorizontalAlignment.Stretch;
        panel.Children.Add(previewButton);

        return panel;
    }

    private static Border MakePreviewPanel(BinaryResizeImagesViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var image = new Image
        {
            Stretch = Avalonia.Media.Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewBitmap)),
        };

        scrollViewer.Content = image;

        vm.PreviewImage = image;

        return UiUtil.MakeBorderForControl(scrollViewer);
    }
}
