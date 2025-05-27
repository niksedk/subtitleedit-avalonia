using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.AdjustDuration;

public class AdjustDurationWindow : Window
{
    private AdjustDurationViewModel _vm;
    
    private const int LabelMinWidth = 100;
    private const int NumericUpDownWidth = 150;
    
    public AdjustDurationWindow(AdjustDurationViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Adjust duration";
        Width = 540;
        Height = 240;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = "Adjust duration by",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var combo = new ComboBox
        {
            ItemsSource = vm.AdjustTypes,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 180,
        };
        combo.Bind(ComboBox.SelectedValueProperty, new Binding
        {
            Path = nameof(vm.SelectedAdjustType),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });
        
        var panelSeconds = MakeAdjustSeconds(vm);
        var panelPercent = MakeAdjustPercent(vm);
        var panelFixed = MakeAdjustFixed(vm);
        var panelRecalculate = MakeAdjustRecalculate(vm);

        var labelInfo = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Left,
            Text = "Note: Adjustments will not cause overlap",
            Margin = new Thickness(10,10,10,15),
        };

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("OK", vm.OkCommand),
            UiUtil.MakeButton("Cancel", vm.CancelCommand)
        );
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Children.Add(label);
        Grid.SetRow(label, 0);
        Grid.SetColumn(label, 0);

        grid.Children.Add(combo);
        Grid.SetRow(combo, 0);
        Grid.SetColumn(combo, 1);
        
        grid.Children.Add(panelSeconds);
        Grid.SetRow(panelSeconds, 1);
        Grid.SetColumn(panelSeconds, 0);
        Grid.SetColumnSpan(panelSeconds, 2);
        
        grid.Children.Add(panelPercent);
        Grid.SetRow(panelPercent, 1);
        Grid.SetColumn(panelPercent, 0);
        Grid.SetColumnSpan(panelPercent, 2);
        
        grid.Children.Add(panelFixed);
        Grid.SetRow(panelFixed, 1);
        Grid.SetColumn(panelFixed, 0);
        Grid.SetColumnSpan(panelFixed, 2);
        
        grid.Children.Add(panelRecalculate);
        Grid.SetRow(panelRecalculate, 1);
        Grid.SetColumn(panelRecalculate, 0);
        Grid.SetColumnSpan(panelRecalculate, 2);

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, 2);
        Grid.SetColumn(buttonPanel, 0);
        Grid.SetColumnSpan(buttonPanel, 2);

        grid.Children.Add(labelInfo);
        Grid.SetRow(labelInfo, 2);
        Grid.SetColumn(labelInfo, 0);
        Grid.SetColumnSpan(labelInfo, 2);

        Content = grid;
        
        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }
    
    private static StackPanel MakeAdjustSeconds(AdjustDurationViewModel vm)
    {
        var textBlockSeconds = new TextBlock
        {
            Text = "Seconds",
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = LabelMinWidth,
        };
        var numericUpDownSeconds = new NumericUpDown
        {
            Minimum = -1000000,
            Maximum = 1000000,
            Width = NumericUpDownWidth,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDownSeconds.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustSeconds),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });
        
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
            Children =
            {
                textBlockSeconds,
                numericUpDownSeconds
            }
        };
        panel.Bind(StackPanel.IsVisibleProperty, new Binding()
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(AdjustDurationDisplay.IsSecondsVisible)}",
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        
        return panel;
    }
    
    private static StackPanel MakeAdjustPercent(AdjustDurationViewModel vm)
    {
        var textBlockSeconds = new TextBlock
        {
            Text = "Percent",
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = LabelMinWidth,
        };
        var numericUpDownSeconds = new NumericUpDown
        {
            Minimum = -1000000,
            Maximum = 1000000,
            Width = NumericUpDownWidth,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDownSeconds.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustPercent),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });
        
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
            Children =
            {
                textBlockSeconds,
                numericUpDownSeconds
            }
        };
        panel.Bind(StackPanel.IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(AdjustDurationDisplay.IsPercentVisible)}",
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        
        return panel;
    }

    
    private static StackPanel MakeAdjustFixed(AdjustDurationViewModel vm)
    {
        var textBlockSeconds = new TextBlock
        {
            Text = "Fixed seconds",
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = LabelMinWidth,
        };
        var numericUpDownSeconds = new NumericUpDown
        {
            Minimum = -1000000,
            Maximum = 1000000,
            Width = NumericUpDownWidth,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDownSeconds.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Mode = BindingMode.TwoWay,
            Source = vm,
            Path = nameof(vm.AdjustFixed),
        });
        
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
            Children =
            {
                textBlockSeconds,
                numericUpDownSeconds
            }
        };
        panel.Bind(StackPanel.IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(AdjustDurationDisplay.IsFixedVisible)}",
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        return panel;
    }
    
    private static Grid MakeAdjustRecalculate(AdjustDurationViewModel vm)
    {
        var textBlockMax = new TextBlock
        {
            Text = "Max characters per second",
            VerticalAlignment = VerticalAlignment.Center,
        };
        var numericUpDownMax = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Width = NumericUpDownWidth,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10,0,0,0),
        };
        numericUpDownMax.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustRecalculateMaxCharacterPerSecond),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });
        
        var textBlockOptimal = new TextBlock
        {
            Text = "Optimal characters per second",
            VerticalAlignment = VerticalAlignment.Center,
        };
        var numericUpDownOptimal = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Width = NumericUpDownWidth,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10,0,0,0),
        };
        numericUpDownOptimal.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustRecalculateOptimalCharacterPerSecond),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });
        
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            RowSpacing = 10,
            Margin = new Thickness(10, 0, 0, 0),
        };
        
        grid.Children.Add(textBlockMax);
        Grid.SetColumn(textBlockMax, 0);
        
        grid.Children.Add(numericUpDownMax);
        Grid.SetColumn(numericUpDownMax, 1);
        
        grid.Children.Add(textBlockOptimal);
        Grid.SetColumn(textBlockOptimal, 0);
        Grid.SetRow(textBlockOptimal, 1);
        
        grid.Children.Add(numericUpDownOptimal);
        Grid.SetColumn(numericUpDownOptimal, 1);
        Grid.SetRow(numericUpDownOptimal, 1);

        grid.Bind(Grid.IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(AdjustDurationDisplay.IsRecalculateVisible)}",
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        
        return grid;
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
