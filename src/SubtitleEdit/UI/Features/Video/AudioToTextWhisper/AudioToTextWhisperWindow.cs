using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
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
            Margin = new Thickness(10, 12, 10, 10),
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
        vm.TextBoxConsoleLog = textBoxConsoleLog;

        var labelEngine = UiUtil.MakeTextBlock("Engine").WithMarginTop(10);
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine))
            .WithMinwidth(220)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithMarginTop(10);

        comboEngine.SelectionChanged += vm.OnEngineChanged;

        var labelLanguage = UiUtil.MakeTextBlock("Language").WithMarginTop(10);
        var comboLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage))
            .WithMinwidth(220)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithMarginTop(10);

        var labelModel = UiUtil.MakeTextBlock("Model").WithMarginBottom(20).WithMarginTop(10);
        var comboModel = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel))
            .WithMinwidth(220)
            .WithMarginBottom(20)
            .WithMarginTop(10)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));

        var buttonModelDownload = new Button
        {
            Content = "...",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = vm.DownloadModelCommand,
            Margin = new Thickness(5, 0, 0, 0),
        }.WithMarginBottom(20).WithMarginTop(10).BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));

        var panelModelControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                comboModel,
                buttonModelDownload
            }
        };

        var labelTranslateToEnglish = UiUtil.MakeTextBlock("Translate to English");
        var checkTranslateToEnglish = UiUtil.MakeCheckBox(vm, nameof(vm.DoTranslateToEnglish)).BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));

        var labelPostProcessing = UiUtil.MakeTextBlock("Post processing").WithMarginTop(15);
        var checkPostProcessing = UiUtil.MakeCheckBox(vm, nameof(vm.DoPostProcessing)).BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        var buttonPostProcessing = new Button()
        {
            Content = "Settings",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = vm.ShowPostProcessingSettingsCommand,
        }.BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        var panelPostProcessingControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 15, 0, 0),
            Children =
            {
                checkPostProcessing,
                buttonPostProcessing
            }
        };

        var labelAdvancedSettings = UiUtil.MakeTextBlock("Advanced settings").WithMarginTop(15);
        var buttonAdvancedSettings = new Button()
        {
            Content = "Settings",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = vm.ShowAdvancedSettingsCommand,
        }.BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled)).WithMarginTop(15);

        var textBoxAdvancedSettings = new TextBox()
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            IsReadOnly = true,
            FontSize = 12,
            Margin = new Thickness(0),
            Opacity = 0.6,
            BorderThickness = new Thickness(0),
            MaxWidth = 320,
        };
        textBoxAdvancedSettings.Bind(TextBox.TextProperty, new Binding
        {
            Path = nameof(vm.Parameters),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        var progressSlider = new Slider()
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            Margin = new Thickness(10, 0, 0, 0),
            Width = double.NaN,
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    },
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 8.0)
                    },
                },
            },
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding
        {
            Path = nameof(vm.ProgressValue),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });
        progressSlider.Bind(Slider.OpacityProperty, new Binding
        {
            Path = nameof(vm.ProgressOpacity),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var progressText = new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0),
        };
        progressText.Bind(TextBlock.TextProperty, new Binding
        {
            Path = nameof(vm.ProgressText),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        progressText.Bind(TextBlock.OpacityProperty, new Binding
        {
            Path = nameof(vm.ProgressOpacity),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        var estimatedTimeText = new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0),
        };
        estimatedTimeText.Bind(TextBlock.TextProperty, new Binding
        {
            Path = nameof(vm.EstimatedText),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        estimatedTimeText.Bind(TextBlock.OpacityProperty, new Binding
        {
            Path = nameof(vm.ProgressOpacity),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        var elapsedTimeText = new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0),
        };
        elapsedTimeText.Bind(TextBlock.TextProperty, new Binding
        {
            Path = nameof(vm.ElapsedText),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        elapsedTimeText.Bind(TextBlock.OpacityProperty, new Binding
        {
            Path = nameof(vm.ProgressOpacity),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });


        var panelProgress = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                progressSlider,
                progressText,
                elapsedTimeText,
                estimatedTimeText,
            },
        };

        var transcribeButton = UiUtil.MakeButton("Transcribe", vm.TranscribeCommand).BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        var buttonPanel = UiUtil.MakeButtonBar(
            transcribeButton,
            UiUtil.MakeButton("Cancel", vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Engine
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Language
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Model
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Translate to English
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Post processing
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Advanced settings
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Console log
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // OK/Cancel
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 0,
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
        Grid.SetRowSpan(textBoxConsoleLog, 8);
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

        grid.Children.Add(panelModelControls);
        Grid.SetRow(panelModelControls, row);
        Grid.SetColumn(panelModelControls, 1);
        row++;

        grid.Children.Add(labelTranslateToEnglish);
        Grid.SetRow(labelTranslateToEnglish, row);
        Grid.SetColumn(labelTranslateToEnglish, 0);

        grid.Children.Add(checkTranslateToEnglish);
        Grid.SetRow(checkTranslateToEnglish, row);
        Grid.SetColumn(checkTranslateToEnglish, 1);
        row++;

        grid.Children.Add(labelPostProcessing);
        Grid.SetRow(labelPostProcessing, row);
        Grid.SetColumn(labelPostProcessing, 0);

        grid.Children.Add(panelPostProcessingControls);
        Grid.SetRow(panelPostProcessingControls, row);
        Grid.SetColumn(panelPostProcessingControls, 1);
        row++;

        grid.Children.Add(labelAdvancedSettings);
        Grid.SetRow(labelAdvancedSettings, row);
        Grid.SetColumn(labelAdvancedSettings, 0);

        grid.Children.Add(buttonAdvancedSettings);
        Grid.SetRow(buttonAdvancedSettings, row);
        Grid.SetColumn(buttonAdvancedSettings, 1);
        row++;

        grid.Children.Add(textBoxAdvancedSettings);
        Grid.SetRow(textBoxAdvancedSettings, row);
        Grid.SetColumn(textBoxAdvancedSettings, 0);
        Grid.SetColumnSpan(textBoxAdvancedSettings, 2);
        row++;

        grid.Children.Add(panelProgress);
        Grid.SetRow(panelProgress, row);
        Grid.SetColumn(panelProgress, 0);
        Grid.SetColumnSpan(panelProgress, 3);
        Grid.SetRowSpan(panelProgress, 2);

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
