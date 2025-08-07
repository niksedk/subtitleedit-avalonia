using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.FindText;
using Nikse.SubtitleEdit.Logic;
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
    [ObservableProperty] private int _selectedParagraphLeftIndex = -1;
    [ObservableProperty] private int _selectedParagraphRightIndex = -1;
    [ObservableProperty] private bool _isAudioVisualizerVisible;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _videoInfo;
    [ObservableProperty] private string _adjustInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl VideoPlayerControlLeft { get; set; }
    public VideoPlayerControl VideoPlayerControlRight { get; set; }
    public AudioVisualizer AudioVisualizerLeft { get; set; }
    public AudioVisualizer AudioVisualizerRight { get; set; }

    private readonly IWindowService _windowService;

    private string? _videoFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private List<SubtitleLineViewModel> _subtitleLines = new List<SubtitleLineViewModel>();
    private bool _updateAudioVisualizer;

    public VisualSyncViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Title = string.Empty;
        VideoInfo = string.Empty;
        AdjustInfo = string.Empty;
        _videoFileName = string.Empty;
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
        _subtitleLines = paragraphs;

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
                    VideoInfo = string.Format(Se.Language.General.FileNameX, videoFileName) + Environment.NewLine +
                                string.Format(Se.Language.Sync.ResolutionXDurationYFrameRateZ,
                                    $"{mediaInfo.Dimension.Width}x{mediaInfo.Dimension.Height}",
                                    mediaInfo.Duration.ToShortDisplayString(),
                                    mediaInfo.FramesRate);
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
            UpdateAudioVisualizer(VideoPlayerControlLeft.VideoPlayerInstance, AudioVisualizerLeft, SelectedParagraphLeftIndex);
            UpdateAudioVisualizer(VideoPlayerControlRight.VideoPlayerInstance, AudioVisualizerRight, SelectedParagraphRightIndex);

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
        int selectedParagraphIndex)
    {
        SubtitleDisplayItem? selectedParagraph = selectedParagraphIndex < 0
            ? null
            : Paragraphs[selectedParagraphIndex];

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

        if ((isPlaying || !av.IsScrolling) && (mediaPlayerSeconds > av.EndPositionSeconds ||
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
    private async Task PlayTwoSecondsAndBackLeft()
    {
        await PlayAndBack(VideoPlayerControlLeft, 2000);
    }

    [RelayCommand]
    private async Task PlayTwoSecondsAndBackRight()
    {
        await PlayAndBack(VideoPlayerControlRight, 2000);
    }

    [RelayCommand]
    private async Task FindTextLeft()
    {
        var result = await _windowService.ShowDialogAsync<FindTextWindow, FindTextViewModel>(Window!, vm =>
        {
            vm.Initialize(_subtitleLines, string.Format(Se.Language.General.FindTextX, Se.Language.Sync.StartScene));
        });

        if (!result.OkPressed || result.SelectedSubtitle == null)
        {
            return;
        }

        var s = Paragraphs.FirstOrDefault(p => p.Subtitle == result.SelectedSubtitle);
        if (s == null)
        {
            return;
        }

        SelectedParagraphLeftIndex = Paragraphs.IndexOf(s);
        VideoPlayerControlLeft.Position = s.Subtitle.StartTime.TotalSeconds;
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task FindTextRight()
    {
        var result = await _windowService.ShowDialogAsync<FindTextWindow, FindTextViewModel>(Window!, vm =>
        {
            vm.Initialize(_subtitleLines, string.Format(Se.Language.General.FindTextX, Se.Language.Sync.EndScene));
        });

        if (!result.OkPressed || result.SelectedSubtitle == null)
        {
            return;
        }

        var s = Paragraphs.FirstOrDefault(p => p.Subtitle == result.SelectedSubtitle);
        if (s == null)
        {
            return;
        }

        SelectedParagraphRightIndex = Paragraphs.IndexOf(s);
        VideoPlayerControlRight.Position = s.Subtitle.StartTime.TotalSeconds;
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task Sync()
    {
        if (SelectedParagraphLeftIndex < 0 || SelectedParagraphRightIndex < 0)
        {
            return;
        }

        // Video player current start and end position.
        double videoPlayerCurrentStartPos = VideoPlayerControlLeft.Position;
        double videoPlayerCurrentEndPos = VideoPlayerControlRight.Position;

        // Subtitle start and end time in seconds.
        double subStart = Paragraphs[SelectedParagraphLeftIndex].Subtitle.StartTime.TotalSeconds;
        double subEnd = Paragraphs[SelectedParagraphRightIndex].Subtitle.StartTime.TotalSeconds;

        // Validate: End time must be greater than start time.
        if (!(videoPlayerCurrentEndPos > videoPlayerCurrentStartPos && subEnd > subStart))
        {
            await MessageBox.Show(Window!, Title, Se.Language.Sync.StartSceneMustComeBeforeEndScene);
            return;
        }

        SetSyncFactorLabel(videoPlayerCurrentStartPos, videoPlayerCurrentEndPos);

        double subDiff = subEnd - subStart;
        double realDiff = videoPlayerCurrentEndPos - videoPlayerCurrentStartPos;

        // speed factor
        double factor = realDiff / subDiff;

        // adjust to starting position
        double adjust = videoPlayerCurrentStartPos - subStart * factor;

        foreach (var p in Paragraphs)
        {
            p.Subtitle.Adjust(factor, adjust);
            p.UpdateText();
        }

        // fix overlapping time codes
        for (var i = 0; i < Paragraphs.Count - 1; i++)
        {
            var current = Paragraphs[i].Subtitle;
            var next = Paragraphs[i + 1].Subtitle;
            if (current.EndTime.TotalMilliseconds > next.StartTime.TotalMilliseconds)
            {
                var newEndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - 1);
                if (newEndTime < current.StartTime)
                {
                    continue;
                }

                current.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - 1);
            }
        }

        _updateAudioVisualizer = true;
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

    private void SetSyncFactorLabel(double videoPlayerCurrentStartPos, double videoPlayerCurrentEndPos)
    {
        if (string.IsNullOrWhiteSpace(_videoFileName) || SelectedParagraphLeftIndex < 0 || SelectedParagraphRightIndex < 0)
        {
            return;
        }

        AdjustInfo = string.Empty;
        if (videoPlayerCurrentEndPos > videoPlayerCurrentStartPos)
        {
            double subStart = Paragraphs[SelectedParagraphLeftIndex].Subtitle.StartTime.TotalSeconds;
            double subEnd = Paragraphs[SelectedParagraphRightIndex].Subtitle.StartTime.TotalSeconds;

            double subDiff = subEnd - subStart;
            double realDiff = videoPlayerCurrentEndPos - videoPlayerCurrentStartPos;

            // speed factor
            double factor = realDiff / subDiff;

            // adjust to starting position
            double adjust = videoPlayerCurrentStartPos - subStart * factor;

            if (Math.Abs(adjust) > 0.001 || (Math.Abs(1 - factor)) > 0.001)
            {
                AdjustInfo = string.Format("*{0:0.000}, {1:+0.000;-0.000}", factor, adjust);
            }
        }
    }

    private async Task PlayAndBack(VideoPlayerControl videoPlayer, int milliseconds)
    {
        var originalPosition = videoPlayer.Position;
        videoPlayer.VideoPlayerInstance.Play();
        await Task.Delay(milliseconds);
        videoPlayer.VideoPlayerInstance.Pause();
        videoPlayer.Position = originalPosition;
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
        var selectedIndex = SelectedParagraphLeftIndex;
        if (selectedIndex < 0)
        {
            return;
        }

        var selected = Paragraphs[selectedIndex];
        VideoPlayerControlLeft.Position = selected.Subtitle.StartTime.TotalSeconds;
        AudioVisualizerLeft.CurrentVideoPositionSeconds = selected.Subtitle.StartTime.TotalSeconds;
        _updateAudioVisualizer = true;
    }

    internal void ComboBoxRightChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedIndex = SelectedParagraphRightIndex;
        if (selectedIndex < 0)
        {
            return;
        }

        var selected = Paragraphs[selectedIndex];
        VideoPlayerControlRight.Position = selected.Subtitle.StartTime.TotalSeconds;
        AudioVisualizerRight.CurrentVideoPositionSeconds = selected.Subtitle.StartTime.TotalSeconds;
        _updateAudioVisualizer = true;
    }

    internal void OnLoaded()
    {
        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        SelectedParagraphLeftIndex = 0;
        SelectedParagraphRightIndex = Paragraphs.Count - 1;

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        timer.Tick += (s, e) =>
        {
            var left = Paragraphs[SelectedParagraphLeftIndex];
            if (VideoPlayerControlLeft.Position < left.Subtitle.StartTime.TotalSeconds - 0.1)
            {
                VideoPlayerControlLeft.Position = left.Subtitle.StartTime.TotalSeconds;
                AudioVisualizerLeft.CurrentVideoPositionSeconds = left.Subtitle.StartTime.TotalSeconds;
                _updateAudioVisualizer = true;
                return;
            }

            var right = Paragraphs[SelectedParagraphRightIndex];
            if (VideoPlayerControlRight.Position < right.Subtitle.StartTime.TotalSeconds - 0.1)
            {
                VideoPlayerControlRight.Position = right.Subtitle.StartTime.TotalSeconds;
                AudioVisualizerRight.CurrentVideoPositionSeconds = right.Subtitle.StartTime.TotalSeconds;
                _updateAudioVisualizer = true;
                return;
            }

            timer.Stop();
        };
        timer.Start();
    }
}