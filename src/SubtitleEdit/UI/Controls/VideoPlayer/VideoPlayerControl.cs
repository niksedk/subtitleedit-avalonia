using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using HanumanInstitute.LibMpv.Core;
using Nikse.SubtitleEdit.Core.Common;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Controls.VideoPlayer
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

        public Control? PlayerContent
        {
            get => GetValue(PlayerContentProperty);
            set => SetValue(PlayerContentProperty, value);
        }

        public double Volume
        {
            get => GetValue(VolumeProperty);
            set
            {
                SetValue(VolumeProperty, value);
                _videoPlayerInstance.Volume = value;
            }
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

        private readonly TextBlock _textBlockPlayerName;

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

        private bool _isFullScreen = false;
        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                _fullScreenCollapseButton.IsVisible = value;
                _fullScreenButton.IsVisible = !value;
                _isFullScreen = value;
            }
        }

        double _positionIgnore = -1;
        double _volumeIgnore = -1;
        private readonly Button _playButton = new Button();
        private readonly Button _fullScreenButton = new Button();
        private readonly Button _fullScreenCollapseButton = new Button();
        private readonly Icon _volumeIcon = new Icon();
        private DispatcherTimer? _positionTimer;
        IVideoPlayerInstance _videoPlayerInstance;

        private void NotifyPositionChanged(double newPosition)
        {
            if (_positionIgnore == newPosition)
            {
                return;
            }

            // First update our property
            Position = newPosition;

            _videoPlayerInstance.Position = newPosition;

            // Then notify listeners like the ViewModel
            PositionChanged?.Invoke(newPosition);
        }

        public void SetPosition(double seconds)
        {
            _positionIgnore = seconds;
            Position = seconds;
        }

        public VideoPlayerControl(IVideoPlayerInstance videoPlayerInstance)
        {
            _videoPlayerInstance = videoPlayerInstance;

            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitions("*,Auto") // video + controls
            };

            // PlayerContent
            var contentPresenter = new ContentPresenter
            {
                [!ContentPresenter.ContentProperty] = this[!PlayerContentProperty],
                Background = new SolidColorBrush(Colors.Black),
            };
            mainGrid.Children.Add(contentPresenter);
            Grid.SetRow(contentPresenter, 0);

            // Row with buttons + position slider + volume slider
            var progressGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
                Margin = new Thickness(10, 4)
            };
            Grid.SetRow(progressGrid, 1);
            mainGrid.Children.Add(progressGrid);


            // Buttons
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Play
            _playButton = new Button()
            {
                Margin = new Thickness(0, 0, 3, 0),
            };
            Attached.SetIcon(_playButton, "fa-solid fa-play");
            _playButton.Click += (_, _) =>
            {
                _videoPlayerInstance.PlayOrPause();
                PlayPauseRequested?.Invoke();
            };
            _playButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(PlayCommand),
                Source = this
            });

            stackPanel.Children.Add(_playButton);

            // Stop
            var stopButton = new Button()
            {
                Margin = new Thickness(0, 0, 3, 0),
            }
            ;
            Attached.SetIcon(stopButton, "fa-solid fa-stop");
            stopButton.Click += (_, _) =>
            {
                _videoPlayerInstance.Stop();
                StopRequested?.Invoke();
            };
            stackPanel.Children.Add(stopButton);
            stopButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(StopCommand),
                Source = this
            });

            // Fullscreen
            _fullScreenButton = new Button()
            {
                Margin = new Thickness(0, 0, 3, 0),
            };
            Attached.SetIcon(_fullScreenButton, "fa-solid fa-expand");
            _fullScreenButton.Click += (_, _) => FullscreenRequested?.Invoke();
            stackPanel.Children.Add(_fullScreenButton);
            _fullScreenButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(FullScreenCommand),
                Source = this
            });


            _fullScreenCollapseButton = new Button()
            {
                Margin = new Thickness(0, 0, 3, 0),
                IsVisible = false,
            };
            Attached.SetIcon(_fullScreenCollapseButton, "fa-solid fa-compress");
            _fullScreenCollapseButton.Click += (_, _) => FullscreenCollapseRequested?.Invoke();
            stackPanel.Children.Add(_fullScreenCollapseButton);

            progressGrid.Children.Add(stackPanel);
            Grid.SetColumn(stackPanel, 0);

            var positionSlider = new Slider
            {
                Minimum = 0,
                Margin = new Thickness(2, 0, 0, 0),
            };
            positionSlider.TemplateApplied += (s, e) =>
            {
                if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
                {
                    thumb.Width = 14;
                    thumb.Height = 14;
                }
            };

            positionSlider.Bind(RangeBase.MaximumProperty, this.GetObservable(DurationProperty));
            positionSlider.Bind(RangeBase.ValueProperty, this.GetObservable(PositionProperty));

            // Also ensure the control can receive keyboard focus
            positionSlider.Focusable = true;

            // For any direct value changes
            positionSlider.ValueChanged += (s, e) =>
            {
                NotifyPositionChanged(e.NewValue);
            };

            progressGrid.Children.Add(positionSlider);
            Grid.SetColumn(positionSlider, 1);

            _volumeIcon = new Icon
            {
                Value = "fa-solid fa-volume-up",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 4, 0)
            };
            progressGrid.Children.Add(_volumeIcon);
            Grid.SetColumn(_volumeIcon, 2);

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
                    thumb.Width = 14;
                    thumb.Height = 14;
                }
            };
            volumeSlider.Bind(RangeBase.ValueProperty, this.GetObservable(VolumeProperty));

            volumeSlider.ValueChanged += (s, e) =>
            {
                if (_volumeIgnore == e.NewValue)
                {
                    return;
                }

                Volume = e.NewValue;
                _videoPlayerInstance.Volume = e.NewValue;
                VolumeChanged?.Invoke(e.NewValue);
                SetVolumeIcon(e.NewValue < 0.0001);
            };


            progressGrid.Children.Add(volumeSlider);
            Grid.SetColumn(volumeSlider, 3);


            // ProgressText
            var progressText = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12,
                FontWeight = FontWeight.Bold,
            };
            progressText.Bind(TextBlock.TextProperty, this.GetObservable(ProgressTextProperty));
            progressGrid.Children.Add(progressText);
            Grid.SetColumn(progressText, 1);
            ProgressText = string.Empty;

            _textBlockPlayerName = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 8,
                FontWeight = FontWeight.Bold,
                Opacity = 0.4,
            };
            progressGrid.Children.Add(_textBlockPlayerName);
            Grid.SetColumn(_textBlockPlayerName, 3);

            Content = mainGrid;

            positionSlider.Maximum = 1;
            positionSlider.Value = 0;

            volumeSlider.Maximum = MpvApi.MaxVolume;
            volumeSlider.Value = 50;
        }

        public event Action? PlayPauseRequested;
        public event Action? StopRequested;
        public event Action? FullscreenRequested;
        public event Action? FullscreenCollapseRequested;
        public event Action? ScreenshotRequested;
        public event Action? SettingsRequested;
        public event Action<double>? PositionChanged;
        public event Action<double>? VolumeChanged;

        public void SetPlayPauseIcon(bool isPlaying)
        {
            if (isPlaying)
            {
                Attached.SetIcon(_playButton, "fa-solid fa-pause");
            }
            else
            {
                Attached.SetIcon(_playButton, "fa-solid fa-play");
            }
        }

        public void SetVolumeIcon(bool isMuted)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                _volumeIcon.Value = isMuted ? "fa-solid fa-volume-xmark" : "fa-solid fa-volume-up";
            });
        }

        internal async Task Open(string videoFileName)
        {
            await _videoPlayerInstance.Open(videoFileName);
            _videoPlayerInstance.Volume = Volume;
            if (_positionTimer == null)
            {
                StartPositionTimer();
            }
            _videoPlayerInstance.Pause();
            _textBlockPlayerName.Text = _videoPlayerInstance.Name;
        }

        internal void Close()
        {
            _positionTimer?.Stop();
            _videoPlayerInstance.Close();
            ProgressText = string.Empty;
        }

        private void StartPositionTimer()
        {
            _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _positionTimer.Tick += (s, e) =>
            {
                var pos = _videoPlayerInstance.Position;
                SetPosition(pos);

                //TODO: move to a slower timer or events
                Duration = _videoPlayerInstance.Duration;

                ProgressText = $"{TimeCode.FromSeconds(pos).ToShortDisplayString() } / {TimeCode.FromSeconds(Duration).ToShortDisplayString()}";

                var isPlaying = _videoPlayerInstance.IsPlaying;
                SetPlayPauseIcon(isPlaying);
            };
            _positionTimer.Start();
        }
    }
}
