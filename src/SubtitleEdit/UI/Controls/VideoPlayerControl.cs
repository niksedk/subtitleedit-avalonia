using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Controls
{
    public class VideoPlayerControl : UserControl
    {
        public static readonly StyledProperty<Control?> PlayerContentProperty =
            AvaloniaProperty.Register<VideoPlayerControl, Control?>(nameof(PlayerContent));

        public static readonly StyledProperty<double> VolumeProperty =
            AvaloniaProperty.Register<VideoPlayerControl, double>(nameof(Volume), 100);

        public static readonly StyledProperty<double> PositionProperty =
            AvaloniaProperty.Register<VideoPlayerControl, double>(nameof(Position));

        public static readonly StyledProperty<double> DurationProperty =
            AvaloniaProperty.Register<VideoPlayerControl, double>(nameof(Duration));

        public static readonly StyledProperty<string> ProgressTextProperty =
            AvaloniaProperty.Register<VideoPlayerControl, string>(nameof(ProgressText), default!);

        public static readonly StyledProperty<ICommand> PlayCommandProperty =
                AvaloniaProperty.Register<VideoPlayerControl, ICommand>(nameof(PlayCommand));

        public static readonly StyledProperty<ICommand> StopCommandProperty =
            AvaloniaProperty.Register<VideoPlayerControl, ICommand>(nameof(StopCommand));

        public static readonly StyledProperty<ICommand> FullScreenCommandProperty =
            AvaloniaProperty.Register<VideoPlayerControl, ICommand>(nameof(FullScreenCommand));

        public static readonly StyledProperty<ICommand> ScreenshotCommandProperty =
            AvaloniaProperty.Register<VideoPlayerControl, ICommand>(nameof(ScreenshotCommand));

        public static readonly StyledProperty<ICommand> SettingsCommandProperty =
            AvaloniaProperty.Register<VideoPlayerControl, ICommand>(nameof(SettingsCommand));

        public Control? PlayerContent
        {
            get => GetValue(PlayerContentProperty);
            set => SetValue(PlayerContentProperty, value);
        }

        public double Volume
        {
            get => GetValue(VolumeProperty);
            set => SetValue(VolumeProperty, value);
        }

        public double Position
        {
            get => GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public double Duration
        {
            get => GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        public string ProgressText
        {
            get => GetValue(ProgressTextProperty);
            set => SetValue(ProgressTextProperty, value);
        }

        public ICommand PlayCommand
        {
            get => GetValue(PlayCommandProperty);
            set => SetValue(PlayCommandProperty, value);
        }

        public ICommand StopCommand
        {
            get => GetValue(StopCommandProperty);
            set => SetValue(StopCommandProperty, value);
        }

        public ICommand FullScreenCommand
        {
            get => GetValue(FullScreenCommandProperty);
            set => SetValue(FullScreenCommandProperty, value);
        }

        public ICommand ScreenshotCommand
        {
            get => GetValue(ScreenshotCommandProperty);
            set => SetValue(ScreenshotCommandProperty, value);
        }

        public ICommand SettingsCommand
        {
            get => GetValue(SettingsCommandProperty);
            set => SetValue(SettingsCommandProperty, value);
        }

        public VideoPlayerControl()
        {
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitions("*,Auto,Auto")
            };

            // PlayerContent
            var contentPresenter = new ContentPresenter
            {
                [!ContentPresenter.ContentProperty] = this[!PlayerContentProperty]
            };
            mainGrid.Children.Add(contentPresenter);
            Grid.SetRow(contentPresenter, 0);

            // Row 1: Progress + volume
            var progressGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("* ,Auto ,Auto"),
                Margin = new Thickness(10, 4)
            };
            Grid.SetRow(progressGrid, 1);
            mainGrid.Children.Add(progressGrid);

            var progressSlider = new Slider
            {
                Minimum = 0
            };
            progressSlider.TemplateApplied += (s, e) =>
            {
                if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
                {
                    thumb.Width = 15;
                    thumb.Height = 15;
                }
            };

            progressSlider.Bind(Slider.MaximumProperty, this.GetObservable(DurationProperty));
            progressSlider.Bind(Slider.ValueProperty, this.GetObservable(PositionProperty));
            progressGrid.Children.Add(progressSlider);
            Grid.SetColumn(progressSlider, 0);

            var volumeIcon = new Icon
            {
                Value = "fa-solid fa-volume-up",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 4, 0)
            };
            progressGrid.Children.Add(volumeIcon);
            Grid.SetColumn(volumeIcon, 1);

            var volumeSlider = new Slider
            {
                Minimum = 0,
                Maximum = 100,
                Width = 80,
                VerticalAlignment = VerticalAlignment.Center
            };
            volumeSlider.TemplateApplied += (s, e) =>
            {
                if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
                {
                    thumb.Width = 15;
                    thumb.Height = 15;
                }
            };
            volumeSlider.Bind(Slider.ValueProperty, this.GetObservable(VolumeProperty));
            progressGrid.Children.Add(volumeSlider);
            Grid.SetColumn(volumeSlider, 2);

            // Row 2: Controls
            var controlGrid = new Grid
            {
                Margin = new Thickness(10, 4),
                ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,*,Auto,Auto"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(controlGrid, 2);
            mainGrid.Children.Add(controlGrid);

            // Play
            var playButton = new Button();
            Attached.SetIcon(playButton, "fa-solid fa-play");
            playButton.Click += (_, _) => PlayPauseRequested?.Invoke();
            playButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(PlayCommand),
                Source = this
            });

            controlGrid.Children.Add(playButton);
            Grid.SetColumn(playButton, 0);

            // Stop
            var stopButton = new Button();
            Attached.SetIcon(stopButton, "fa-solid fa-stop");
            stopButton.Click += (_, _) => StopRequested?.Invoke();
            controlGrid.Children.Add(stopButton);
            Grid.SetColumn(stopButton, 1);
            stopButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(StopCommand),
                Source = this
            });

            // Fullscreen
            var fullscreenButton = new Button();
            Attached.SetIcon(fullscreenButton, "fa-solid fa-expand");
            fullscreenButton.Click += (_, _) => FullscreenRequested?.Invoke();
            controlGrid.Children.Add(fullscreenButton);
            Grid.SetColumn(fullscreenButton, 2);
            fullscreenButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(FullScreenCommand),
                Source = this
            });

            // ProgressText
            var progressText = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 16
            };
            progressText.Bind(TextBlock.TextProperty, this.GetObservable(ProgressTextProperty));
            controlGrid.Children.Add(progressText);
            Grid.SetColumn(progressText, 3);

            // Clip
            var clipButton = new Button();
            Attached.SetIcon(clipButton, "fa-solid fa-scissors");
            clipButton.Click += (_, _) => ScreenshotRequested?.Invoke();
            controlGrid.Children.Add(clipButton);
            Grid.SetColumn(clipButton, 4);
            clipButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(ScreenshotCommand),
                Source = this
            });

            // Settings
            var settingsButton = new Button();
            Attached.SetIcon(settingsButton, "fa-solid fa-gear");
            settingsButton.Click += (_, _) => SettingsRequested?.Invoke();
            controlGrid.Children.Add(settingsButton);
            Grid.SetColumn(settingsButton, 5);
            settingsButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(SettingsCommand),
                Source = this
            });

            Content = mainGrid;
        }

        public event Action? PlayPauseRequested;
        public event Action? StopRequested;
        public event Action? FullscreenRequested;
        public event Action? ScreenshotRequested;
        public event Action? SettingsRequested;
    }
}
