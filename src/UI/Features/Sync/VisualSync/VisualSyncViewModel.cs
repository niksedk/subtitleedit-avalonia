using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

public partial class VisualSyncViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private SubtitleDisplayItem? _selectedParagraphLeft;
    [ObservableProperty] private SubtitleDisplayItem? _selectedParagraphRight;
    [ObservableProperty] private bool _isAudioVisualizerVisible;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _videoInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl VideoPlayerControlLeft { get; set; }
    public VideoPlayerControl VideoPlayerControlRight { get; set; }
    public AudioVisualizer AudioVisualizerLeft { get; set; }
    public AudioVisualizer AudioVisualizerRight { get; set; }

    private string? _videoFileName;
    private string? _subtitleFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private List<SubtitleLineViewModel> _subtitleLines = new List<SubtitleLineViewModel>();
    private bool _updateAudioVisualizer;

    public VisualSyncViewModel()
    {
        Title = string.Empty;
        VideoInfo = string.Empty;
        _videoFileName = string.Empty;
        _subtitleFileName = string.Empty;
        VideoPlayerControlLeft = new VideoPlayerControl(new VideoPlayerInstanceNone());
        VideoPlayerControlRight = new VideoPlayerControl(new VideoPlayerInstanceNone());
        AudioVisualizerLeft = new AudioVisualizer();
        AudioVisualizerRight = new AudioVisualizer();
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>();
    }

    public void Initialize(
        List<SubtitleLineViewModel> paragraphs,
        string? videoFileName,
        string? subtitleFileName,
        AudioVisualizer? audioVisualizer)
    {
        SetVideoInFo(videoFileName);
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>(paragraphs.Select(p => new SubtitleDisplayItem(p)));
        _videoFileName = videoFileName;
        _subtitleFileName = subtitleFileName;

        _subtitleLines = paragraphs;
        SelectedParagraphLeft = Paragraphs[0];
        SelectedParagraphRight = Paragraphs[^1];

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                _ = VideoPlayerControlLeft.Open(videoFileName);
                _ = VideoPlayerControlRight.Open(videoFileName);
            }

            if (audioVisualizer != null)
            {
                AudioVisualizerLeft.WavePeaks = audioVisualizer.WavePeaks;
                AudioVisualizerRight.WavePeaks = audioVisualizer.WavePeaks;
                IsAudioVisualizerVisible = true;
            }

            StartTitleTimer();
            _updateAudioVisualizer = true;
        });
    }

    private void SetVideoInFo(string? videoFileName)
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            VideoInfo = Se.Language.General.NoVideoLoaded;
            return;
        }

        _ = Task.Run(() =>
        {
            for (var i = 0; i < 10; i++)
            {
                var mediaInfo = FfmpegMediaInfo.Parse(videoFileName);
                if (mediaInfo?.Dimension is { Width: > 0, Height: > 0 })
                {
                    VideoInfo = $"File name: {videoFileName}" + Environment.NewLine +
                                $"Resolution: {mediaInfo.Dimension.Width}x{mediaInfo.Dimension.Height}, " +
                                $"duration: {mediaInfo.Duration.ToShortDisplayString()}, " +
                                $"frame rate: {mediaInfo.FramesRate}";
                    return;
                }

                Task.Delay(250).Wait();
            }

            VideoInfo = Se.Language.General.NoVideoLoaded;
        });

    }

    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _positionTimer.Tick += (s, e) =>
        {
            UpdateAudioVisualizer(VideoPlayerControlLeft.VideoPlayerInstance, AudioVisualizerLeft, SelectedParagraphLeft);
            UpdateAudioVisualizer(VideoPlayerControlRight.VideoPlayerInstance, AudioVisualizerRight, SelectedParagraphRight);

            if (_updateAudioVisualizer)
            {
                AudioVisualizerLeft.InvalidateVisual();
                AudioVisualizerRight.InvalidateVisual();
                _updateAudioVisualizer = false;
            }
        };
        _positionTimer.Start();
    }

    private void UpdateAudioVisualizer(
        IVideoPlayerInstance vp,
        AudioVisualizer av,
        SubtitleDisplayItem? selectedParagraph)
    {
        var subtitle = new ObservableCollection<SubtitleLineViewModel>();
        var orderedList = _subtitleLines.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        var firstSelectedIndex = -1;
        for (var i = 0; i < orderedList.Count; i++)
        {
            var dp = orderedList[i];
            subtitle.Add(dp);
        }

        var mediaPlayerSeconds = vp.Position;
        var startPos = mediaPlayerSeconds - 0.01;
        if (startPos < 0)
        {
            startPos = 0;
        }

        av.CurrentVideoPositionSeconds = vp.Position;
        var isPlaying = vp.IsPlaying;

        var selectedSubtitles = new List<SubtitleLineViewModel>
        {
            selectedParagraph?.Subtitle ??  new  SubtitleLineViewModel(),
        };

        if (Se.Settings.Waveform.CenterVideoPosition)
        {
            // calculate the center position based on the waveform width
            var waveformHalfSeconds = (av.EndPositionSeconds - av.StartPositionSeconds) / 2.0;
            av.SetPosition(Math.Max(0, mediaPlayerSeconds - waveformHalfSeconds), subtitle, mediaPlayerSeconds,
                firstSelectedIndex, selectedSubtitles);
        }
        else if ((isPlaying || !av.IsScrolling) && (mediaPlayerSeconds > av.EndPositionSeconds ||
                                                    mediaPlayerSeconds < av.StartPositionSeconds))
        {
            av.SetPosition(startPos, subtitle, mediaPlayerSeconds, 0,
                selectedSubtitles);
        }
        else
        {
            av.SetPosition(av.StartPositionSeconds, subtitle, mediaPlayerSeconds, firstSelectedIndex,
                selectedSubtitles);
        }
    }

    [RelayCommand]
    private void PlayTwoSecondsAndBackLeft()
    {
    }

    [RelayCommand]
    private void PlayTwoSecondsAndBackRight()
    {
    }

    [RelayCommand]
    private void GoToSubtitlePositionLeft()
    {
    }

    [RelayCommand]
    private void GoToSubtitlePositionRight()
    {
    }

    [RelayCommand]
    private void FindTextLeft()
    {
    }

    [RelayCommand]
    private void FindTextRight()
    {
    }

    [RelayCommand]
    private void Sync()
    {
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    public void AudioVisualizerLeftPositionChanged(object sender, AudioVisualizer.PositionEventArgs e)
    {
        VideoPlayerControlLeft.Position = e.PositionInSeconds;
        _updateAudioVisualizer = true;
    }

    public void AudioVisualizerRightPositionChanged(object sender, AudioVisualizer.PositionEventArgs e)
    {
        VideoPlayerControlRight.Position = e.PositionInSeconds;
        _updateAudioVisualizer = true;
    }

    internal void OnClosing()
    {
        _positionTimer.Stop();
        VideoPlayerControlLeft.VideoPlayerInstance.Close();
        VideoPlayerControlRight.VideoPlayerInstance.Close();
    }

    internal void ComboBoxLeftChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = SelectedParagraphLeft;
        if (selected == null)
        {
            return;
        }

        VideoPlayerControlLeft.Position = selected.Subtitle.StartTime.TotalSeconds;
        AudioVisualizerLeft.CurrentVideoPositionSeconds = selected.Subtitle.StartTime.TotalSeconds;
        _updateAudioVisualizer = true;
    }

    internal void ComboBoxRightChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = SelectedParagraphRight;
        if (selected == null)
        {
            return;
        }

        VideoPlayerControlRight.Position = selected.Subtitle.StartTime.TotalSeconds;
        AudioVisualizerRight.CurrentVideoPositionSeconds = selected.Subtitle.StartTime.TotalSeconds;
        _updateAudioVisualizer = true;
    }
}