﻿using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using HanumanInstitute.LibMpv.Core;
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

        double _positionIgnore = -1;

        private readonly Button _playButton = new Button();
        private readonly Button _volumeButton = new Button();

        private void NotifyPositionChanged(double newPosition)
        {
            if (_positionIgnore == newPosition)
            {
                return;
            }

            // First update our property
            Position = newPosition;

            // Then notify listeners like the ViewModel
            PositionChanged?.Invoke(newPosition);
        }

        public void SetPosition(double seconds)
        {
            _positionIgnore = seconds;
            Position = seconds;
        }

        public VideoPlayerControl()
        {
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitions("*,Auto") // video + controls
            };

            // PlayerContent
            var contentPresenter = new ContentPresenter
            {
                [!ContentPresenter.ContentProperty] = this[!PlayerContentProperty]
            };
            mainGrid.Children.Add(contentPresenter);
            Grid.SetRow(contentPresenter, 0);

            // Row with buttons + position slider + volume slider
            var progressGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,*,Auto,Auto"),
                Margin = new Thickness(10, 4)
            };
            Grid.SetRow(progressGrid, 1);
            mainGrid.Children.Add(progressGrid);


            // Play
            _playButton = new Button
            {
                Margin = new Thickness(0, 0, 3, 0),
            };
            Attached.SetIcon(_playButton, "fa-solid fa-play");
            _playButton.Click += (_, _) => PlayPauseRequested?.Invoke();
            _playButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(PlayCommand),
                Source = this
            });

            progressGrid.Children.Add(_playButton);
            Grid.SetColumn(_playButton, 0);

            // Stop
            var stopButton = new Button
            {
                Margin = new Thickness(0, 0, 3, 0),
            };
            Attached.SetIcon(stopButton, "fa-solid fa-stop");
            stopButton.Click += (_, _) => StopRequested?.Invoke();
            progressGrid.Children.Add(stopButton);
            Grid.SetColumn(stopButton, 1);
            stopButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(StopCommand),
                Source = this
            });

            // Fullscreen
            var fullscreenButton = new Button()
            {
                Margin = new Thickness(0, 0, 3, 0),
            };
            Attached.SetIcon(fullscreenButton, "fa-solid fa-expand");
            fullscreenButton.Click += (_, _) => FullscreenRequested?.Invoke();
            progressGrid.Children.Add(fullscreenButton);
            Grid.SetColumn(fullscreenButton, 2);
            fullscreenButton.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(FullScreenCommand),
                Source = this
            });


            var positionSlider = new Slider
            {
                Minimum = 0,
                Margin = new Thickness(2, 0, 0, 0),
            };
            positionSlider.TemplateApplied += (s, e) =>
            {
                if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
                {
                    thumb.Width = 15;
                    thumb.Height = 15;
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
            Grid.SetColumn(positionSlider, 3);

            _volumeButton = new Button
            {
                Margin = new Thickness(10, 0, 4, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Attached.SetIcon(_volumeButton, "fa-solid fa-volume-up");
            _volumeButton.Click += (_, _) => MuteRequested?.Invoke();
            progressGrid.Children.Add(_volumeButton);
            Grid.SetColumn(_volumeButton, 4);

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
            volumeSlider.Bind(RangeBase.ValueProperty, this.GetObservable(VolumeProperty));

            volumeSlider.ValueChanged += (s, e) =>
            {
                Volume = e.NewValue;
                VolumeChanged?.Invoke(e.NewValue);
                SetVolumeIcon(e.NewValue < 0.0001);
            };

            progressGrid.Children.Add(volumeSlider);
            Grid.SetColumn(volumeSlider, 5);

            Content = mainGrid;

            positionSlider.Maximum = 1;
            positionSlider.Value = 0;

            volumeSlider.Maximum = MpvApi.MaxVolume;
            volumeSlider.Value = 50;
        }

        public event Action? PlayPauseRequested;
        public event Action? StopRequested;
        public event Action? MuteRequested;
        public event Action? FullscreenRequested;
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
                Attached.SetIcon(_volumeButton, isMuted ? "fa-solid fa-volume-xmark" : "fa-solid fa-volume-up");
            });
        }
    }
}
