using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public class TextToSpeechWindow : Window
{
    private TextToSpeechViewModel _vm;

    public TextToSpeechWindow(TextToSpeechViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Text to speech";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelEngine = new Label
        {
            Content = "Text To Speech engine",
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        var labelSettings = new Label
        {
            Content = "Settings",
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        var engineLayout = MakeEngineControls(vm);

        var settingsLayout = MakeSettingsControls(vm);

        var progressBarLayout = MakeProgressBarControls(vm);

        var buttonDone = UiUtil.MakeButton("Done", vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("Generate speech from text", vm.GenerateTtsCommand),
            UiUtil.MakeButton("Import...", vm.ImportCommand),
            UiUtil.MakeButton("Export...", vm.ExportCommand),
            buttonDone
        ).WithMarginTop(0);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelEngine, 0, 0);
        grid.Add(labelSettings, 0, 1);

        grid.Add(engineLayout, 1, 0);
        grid.Add(settingsLayout, 1, 1);

        grid.Add(progressBarLayout, 2, 0, 1, 2);

        grid.Add(buttonPanel, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonDone.Focus(); }; // hack to make OnKeyDown work
    }

    private static Border MakeEngineControls(TextToSpeechViewModel vm)
    {
        var labelMinWidth = 100;
        var controlMinWidth = 200;

        var comboBoxEngines = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithMinwidth(controlMinWidth);
        comboBoxEngines.SelectionChanged += vm.SelectedEngineChanged;

        var panelEngine = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = "Engine",
                    MinWidth = labelMinWidth,
                },
                comboBoxEngines,
                UiUtil.MakeButton(vm.ShowEngineSettingsCommand, IconNames.MdiSettings)
                    .WithMarginLeft(5)
                    .WithBindIsVisible(nameof(vm.IsEngineSettingsVisible)),
            }
        };

        var panelVoice = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Label
                {
                    Content = "Voice",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Voices, vm, nameof(vm.SelectedVoice)).WithMinwidth(controlMinWidth),
                new Label
                {
                    [!Label.ContentProperty] = new Binding(nameof(vm.VoiceCountInfo)) { Mode = BindingMode.TwoWay }
                },
                UiUtil.MakeButton("Test voice", vm.TestVoiceCommand),
                UiUtil.MakeButton(vm.ShowTestVoiceSettingsCommand, IconNames.MdiSettings),
            }
        };

        var panelModel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Label
                {
                    Content = "Model",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel)).WithMinwidth(controlMinWidth),
            }
        };

        var panelRegion = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Label
                {
                    Content = "Region",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Regions, vm, nameof(vm.SelectedRegion)).WithMinwidth(controlMinWidth),
            }
        };

        var panelLanguage = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Label
                {
                    Content = "Language",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)).WithMinwidth(controlMinWidth),
            }
        };

        var panelApiKey = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Label
                {
                    Content = "API key",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeTextBox(325, vm, nameof(vm.ApiKey)),
            }
        };

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
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 15),
        };

        grid.Add(panelEngine, 0, 0);
        grid.Add(panelVoice, 1, 0);
        grid.Add(panelModel, 2, 0);
        grid.Add(panelRegion, 3, 0);
        grid.Add(panelLanguage, 4, 0);
        grid.Add(panelApiKey, 5, 0);

        var boder = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Child = grid,
            Padding = new Thickness(10, 0, 10, 0),
            CornerRadius = new CornerRadius(5),
        };

        return boder;
    }

    private static Border MakeSettingsControls(TextToSpeechViewModel vm)
    {
        var checkBoxReviewAudioClips = new CheckBox
        {
            Content = "Review audio segments",
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 10, 0, 10),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.DoReviewAudioClips)) { Mode = BindingMode.TwoWay }
        };

        var checkBoxAddAudioToVideoFile = new CheckBox
        {
            Content = "Add audio to video file",
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 0, 10),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.DoGenerateVideoFile)) { Mode = BindingMode.TwoWay }
        };

        var panelAddAudioToVideoFile = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                checkBoxAddAudioToVideoFile,
                UiUtil.MakeButton(vm.ShowEncodingSettingsCommand, IconNames.MdiSettings)
                      .WithMarginLeft(5).WithMarginTop(0).WithTopAlignment(),
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxReviewAudioClips, 0, 0);
        grid.Add(panelAddAudioToVideoFile, 1, 0);

        var boder = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Child = grid,
            Padding = new Thickness(10, 0, 10, 0),
            CornerRadius = new CornerRadius(5),
        };

        return boder;
    }

    private static StackPanel MakeProgressBarControls(TextToSpeechViewModel vm)
    {
        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 6.0)
                    }
                },
            }
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var panelProgress = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                progressSlider,
                new Label
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    [!Label.ContentProperty] = new Binding(nameof(vm.ProgressText)) { Mode = BindingMode.TwoWay },
                },
            }
        };

        return panelProgress;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
