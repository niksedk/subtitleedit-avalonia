using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public class AudioToTextWhisperWindow : Window
{
    private StackPanel _contentPanel;
    private AudioToTextWhisperViewModel _vm;
    
    public AudioToTextWhisperWindow(AudioToTextWhisperViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Audio to text - Whisper";
        Width = 300;
        Height = 160;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = "Language",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var combo = new ComboBox
        {
            ItemsSource = vm.Languages,
            SelectedValue = vm.SelectedLanguage,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 180,
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

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, 1);
        Grid.SetColumn(buttonPanel, 0);
        Grid.SetColumnSpan(buttonPanel, 2);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
