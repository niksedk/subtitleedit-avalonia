using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public class AudioToTextWhisperWindow : Window
{
    private AudioToTextWhisperViewModel _vm;

    public AudioToTextWhisperWindow(AudioToTextWhisperViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Audio to text - Whisper";
        Width = 950;
        Height = 660;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelConsoleLog = new TextBlock
        {
            Text = "Console log",
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(10, 15, 10, 10),
        };
        var textBoxConsoleLog = new TextBox()
        {
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsReadOnly = true,
            Margin = new Thickness(10),
        };
        textBoxConsoleLog.Bind(TextBox.TextProperty, new Binding
        {
            Path = nameof(vm.ConsoleLog),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });


        var labelEngine = UiUtil.MakeTextBlock("Engine");
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithMinwidth(200);

        var labelLanguage = UiUtil.MakeTextBlock("Language");
        var comboLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)).WithMinwidth(200);

        var labelModel = UiUtil.MakeTextBlock("Model").WithMarginBottom(20);
        var comboModel = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel))
            .WithMinwidth(200)
            .WithMarginBottom(20);

        var labelTranslateToEnglish = UiUtil.MakeTextBlock("Translate to English");
        var checkTranslateToEnglish = UiUtil.MakeCheckBox(vm, nameof(vm.DoTranslateToEnglish));

        var labelAdjustTimings = UiUtil.MakeTextBlock("Adjust timings");
        var checkAdjustTimings = UiUtil.MakeCheckBox(vm, nameof(vm.DoAdjustTimings));

        var labelPostProcessing = UiUtil.MakeTextBlock("Post processing");
        var checkPostProcessing = UiUtil.MakeCheckBox(vm, nameof(vm.DoPostProcessing));

        var labelAdvancedSettings = UiUtil.MakeTextBlock("Advanced settings");
        var buttonAdvancedSettings = UiUtil.MakeButton("...", vm.ShowAdvancedSettingsCommand)
            .WithLeftAlignment()
            .WithMargin(0);

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("Transcribe", vm.TranscribeCommand),
            UiUtil.MakeButton("Cancel", vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var row = 0;

        grid.Children.Add(labelConsoleLog);
        Grid.SetRow(labelConsoleLog, row);
        Grid.SetColumn(labelConsoleLog, 2);
        Grid.SetRowSpan(labelConsoleLog, 2);
        row++;

        grid.Children.Add(textBoxConsoleLog);
        Grid.SetRow(textBoxConsoleLog, row);
        Grid.SetColumn(textBoxConsoleLog, 2);
        Grid.SetRowSpan(textBoxConsoleLog, 9);
        row++;


        grid.Children.Add(labelEngine);
        Grid.SetRow(labelEngine, row);
        Grid.SetColumn(labelEngine, 0);

        grid.Children.Add(comboEngine);
        Grid.SetRow(comboEngine, row);
        Grid.SetColumn(comboEngine, 1);
        row++;

        grid.Children.Add(labelLanguage);
        Grid.SetRow(labelLanguage, row);
        Grid.SetColumn(labelLanguage, 0);

        grid.Children.Add(comboLanguage);
        Grid.SetRow(comboLanguage, row);
        Grid.SetColumn(comboLanguage, 1);
        row++;

        grid.Children.Add(labelModel);
        Grid.SetRow(labelModel, row);
        Grid.SetColumn(labelModel, 0);

        grid.Children.Add(comboModel);
        Grid.SetRow(comboModel, row);
        Grid.SetColumn(comboModel, 1);
        row++;

        grid.Children.Add(labelTranslateToEnglish);
        Grid.SetRow(labelTranslateToEnglish, row);
        Grid.SetColumn(labelTranslateToEnglish, 0);

        grid.Children.Add(checkTranslateToEnglish);
        Grid.SetRow(checkTranslateToEnglish, row);
        Grid.SetColumn(checkTranslateToEnglish, 1);
        row++;

        grid.Children.Add(labelAdjustTimings);
        Grid.SetRow(labelAdjustTimings, row);
        Grid.SetColumn(labelAdjustTimings, 0);

        grid.Children.Add(checkAdjustTimings);
        Grid.SetRow(checkAdjustTimings, row);
        Grid.SetColumn(checkAdjustTimings, 1);
        row++;

        grid.Children.Add(labelPostProcessing);
        Grid.SetRow(labelPostProcessing, row);
        Grid.SetColumn(labelPostProcessing, 0);

        grid.Children.Add(checkPostProcessing);
        Grid.SetRow(checkPostProcessing, row);
        Grid.SetColumn(checkPostProcessing, 1);
        row++;

        grid.Children.Add(labelAdvancedSettings);
        Grid.SetRow(labelAdvancedSettings, row);
        Grid.SetColumn(labelAdvancedSettings, 0);

        grid.Children.Add(buttonAdvancedSettings);
        Grid.SetRow(buttonAdvancedSettings, row);
        Grid.SetColumn(buttonAdvancedSettings, 1);
        row++;

        row++;
        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, row);
        Grid.SetColumn(buttonPanel, 0);
        Grid.SetColumnSpan(buttonPanel, 3);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
