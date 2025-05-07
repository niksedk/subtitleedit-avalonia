using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Projektanker.Icons.Avalonia;

namespace ApvPlayer.Controls
{
    public class VideoPlayerControl : UserControl
    {
        // Bindable properties
        public static readonly StyledProperty<Control?> PlayerContentProperty =
            AvaloniaProperty.Register<VideoPlayerControl, Control?>(nameof(PlayerContent));

        public static readonly StyledProperty<double> VolumeProperty =
            AvaloniaProperty.Register<VideoPlayerControl, double>(nameof(Volume), 100);

        public static readonly StyledProperty<double> PositionProperty =
            AvaloniaProperty.Register<VideoPlayerControl, double>(nameof(Position));

        public static readonly StyledProperty<double> DurationProperty =
            AvaloniaProperty.Register<VideoPlayerControl, double>(nameof(Duration));

        // Properties
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

        public VideoPlayerControl()
        {
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitions("*,Auto,Auto")
            };

            // Player area
            var contentPresenter = new ContentPresenter
            {
                [!ContentPresenter.ContentProperty] = this[!PlayerContentProperty]
            };
            mainGrid.Children.Add(contentPresenter);
            Grid.SetRow(contentPresenter, 0);

            // Progress slider
            var progressSlider = new Slider
            {
                Margin = new Thickness(10, 5),
                Minimum = 0
            };
            progressSlider.Bind(Slider.MaximumProperty, this.GetObservable(DurationProperty));
            progressSlider.Bind(Slider.ValueProperty, this.GetObservable(PositionProperty));
            mainGrid.Children.Add(progressSlider);
            Grid.SetRow(progressSlider, 1);

            // Control bar
            var controlBar = new Grid
            {
                Margin = new Thickness(10),
                ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,*,Auto,Auto,Auto"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(controlBar, 2);
            mainGrid.Children.Add(controlBar);

            // Play/Pause
            var playPauseButton = new Button();
            Attached.SetIcon(playPauseButton, "fa-solid fa-play");
            playPauseButton.Click += (_, _) => PlayPauseRequested?.Invoke();
            controlBar.Children.Add(playPauseButton);
            Grid.SetColumn(playPauseButton, 0);

            // Stop
            var stopButton = new Button();
            Attached.SetIcon(stopButton, "fa-solid fa-stop");
            stopButton.Click += (_, _) => StopRequested?.Invoke();
            controlBar.Children.Add(stopButton);
            Grid.SetColumn(stopButton, 1);

            // Volume icon
            var volumeIcon = new Icon
            {
                Value = "fa-solid fa-volume-up",
                Margin = new Thickness(10, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            controlBar.Children.Add(volumeIcon);
            Grid.SetColumn(volumeIcon, 2);

            // Volume slider
            var volumeSlider = new Slider
            {
                Minimum = 0,
                Maximum = 100,
                MinWidth = 100,
                VerticalAlignment = VerticalAlignment.Center
            };
            volumeSlider.Bind(Slider.ValueProperty, this.GetObservable(VolumeProperty));
            controlBar.Children.Add(volumeSlider);
            Grid.SetColumn(volumeSlider, 3);

            // Spacer
            var spacer = new ContentControl();
            controlBar.Children.Add(spacer);
            Grid.SetColumn(spacer, 4);

            // Screenshot
            var screenshotButton = new Button();
            Attached.SetIcon(screenshotButton, "fa-solid fa-scissors");
            screenshotButton.Click += (_, _) => ScreenshotRequested?.Invoke();
            controlBar.Children.Add(screenshotButton);
            Grid.SetColumn(screenshotButton, 5);

            // Settings
            var settingsButton = new Button();
            Attached.SetIcon(settingsButton, "fa-solid fa-gear");
            settingsButton.Click += (_, _) => SettingsRequested?.Invoke();
            controlBar.Children.Add(settingsButton);
            Grid.SetColumn(settingsButton, 6);

            Content = mainGrid;
        }

        // Events to hook up externally
        public event Action? PlayPauseRequested;
        public event Action? StopRequested;
        public event Action? ScreenshotRequested;
        public event Action? SettingsRequested;
    }
}
