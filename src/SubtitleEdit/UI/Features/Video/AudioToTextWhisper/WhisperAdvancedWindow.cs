using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public class WhisperAdvancedWindow : Window
{
    private WhisperAdvancedViewModel _vm;

    public WhisperAdvancedWindow(WhisperAdvancedViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Advanced Whisper settings";
        Width = 1050;
        Height = 660;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelParameters = UiUtil.MakeTextBlock("Parameters");
        var textBoxParameters = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = true,
            DataContext = vm,
        };
        textBoxParameters.Bind(TextBox.TextProperty, new Binding(nameof(vm.Parameters))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var buttonVerticalPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 10, 0),
        };
        foreach (var engine in vm.Engines)
        {
            var button = UiUtil.MakeButton(engine.Name, vm.EngineClickedCommand)
                .WithCommandParameter(engine)
                .WithLeftAlignment()
                .WithMinwidth(200);
            buttonVerticalPanel.Children.Add(button);
        }

        var textBoxHelp = new TextBox
        {
            IsReadOnly = true,
            Margin = new Thickness(0, 0, 0, 5),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,  
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap,            
        };
        textBoxHelp.Bind(TextBox.TextProperty, new Binding(nameof(vm.HelpText))
        {
            Mode = BindingMode.OneWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });
        var scrollViewer = new ScrollViewer
        {
            Content = textBoxHelp,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };  

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.Ok, vm.OkCommand),
            UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var row = 0;
        grid.Children.Add(labelParameters);
        Grid.SetRow(labelParameters, row);
        Grid.SetColumn(labelParameters, 0);
        Grid.SetColumnSpan(labelParameters, 3);
        row++;

        grid.Children.Add(textBoxParameters);
        Grid.SetRow(textBoxParameters, row);
        Grid.SetColumn(textBoxParameters, 0);
        Grid.SetColumnSpan(textBoxParameters, 3);
        row++;

        grid.Children.Add(buttonVerticalPanel);
        Grid.SetRow(buttonVerticalPanel, row);
        Grid.SetColumn(buttonVerticalPanel, 0);

        grid.Children.Add(scrollViewer);
        Grid.SetRow(scrollViewer, row);
        Grid.SetColumn(scrollViewer, 1);
        Grid.SetColumnSpan(scrollViewer, 2);
        row++;

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, row);
        Grid.SetColumn(buttonPanel, 0);
        Grid.SetColumnSpan(buttonPanel, 3);

        Content = grid;

        Activated += delegate { textBoxParameters.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
