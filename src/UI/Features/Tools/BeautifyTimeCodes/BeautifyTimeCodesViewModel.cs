using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;

public partial class BeautifyTimeCodesViewModel : ObservableObject
{
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private readonly List<SubtitleLineViewModel> _allSubtitles;
    private readonly List<SubtitleLineViewModel> _originalSubtitles;
    private readonly List<SubtitleLineViewModel> _beautifiedSubtitles;
    private List<double> _shotChanges;
    private double _frameRate = 25.0;

    [ObservableProperty]
    private BeautifySettings _settings;

    [ObservableProperty]
    private Controls.AudioVisualizerControl.AudioVisualizer? _audioVisualizerOriginal;

    [ObservableProperty]
    private Controls.AudioVisualizerControl.AudioVisualizer? _audioVisualizerBeautified;

    public BeautifyTimeCodesViewModel()
    {
        _settings = new BeautifySettings();
        _allSubtitles = new List<SubtitleLineViewModel>();
        _originalSubtitles = new List<SubtitleLineViewModel>();
        _beautifiedSubtitles = new List<SubtitleLineViewModel>();
        _shotChanges = new List<double>();

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            if (_dirty)
            {
                _dirty = false;
                UpdatePreview();
            }
            _timerUpdatePreview.Start();
        };

        // Listen to settings changes
        Settings.PropertyChanged += (s, e) => { _dirty = true; };
    }

    private void UpdatePreview()
    {
        if (AudioVisualizerBeautified == null)
        {
            return;
        }

        // Apply beautify and update the beautified visualizer
        var beautifiedParagraphs = ApplyBeautify(_allSubtitles.Select(p => p.Paragraph!)).ToList();

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _beautifiedSubtitles.Clear();
            var subRipFormat = new Core.SubtitleFormats.SubRip();
            foreach (var p in beautifiedParagraphs)
            {
                _beautifiedSubtitles.Add(new SubtitleLineViewModel(p, subRipFormat));
            }

            // Update the beautified visualizer's paragraphs
            AudioVisualizerBeautified.AllSelectedParagraphs = new List<SubtitleLineViewModel>(_beautifiedSubtitles);
            AudioVisualizerBeautified.InvalidateVisual();
        });
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles, Controls.AudioVisualizerControl.AudioVisualizer audioVisualizer, string videoFileName)
    {
        _allSubtitles.Clear();
        _allSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));

        _originalSubtitles.Clear();
        _originalSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));

        // Get shot changes from the existing AudioVisualizer
        _shotChanges = audioVisualizer.ShotChanges ?? new List<double>();

        // Get frame rate (try to get from video or use default 25 fps)
        _frameRate = 25.0; // Default fallback

        // Copy visualizer properties from the main window's AudioVisualizer
        if (AudioVisualizerOriginal != null)
        {
            AudioVisualizerOriginal.WavePeaks = audioVisualizer.WavePeaks;
            AudioVisualizerOriginal.ShotChanges = audioVisualizer.ShotChanges;
            AudioVisualizerOriginal.AllSelectedParagraphs = new List<SubtitleLineViewModel>(_originalSubtitles);
            AudioVisualizerOriginal.StartPositionSeconds = audioVisualizer.StartPositionSeconds;
            AudioVisualizerOriginal.ZoomFactor = audioVisualizer.ZoomFactor;
            AudioVisualizerOriginal.VerticalZoomFactor = audioVisualizer.VerticalZoomFactor;
        }

        if (AudioVisualizerBeautified != null)
        {
            AudioVisualizerBeautified.WavePeaks = audioVisualizer.WavePeaks;
            AudioVisualizerBeautified.ShotChanges = audioVisualizer.ShotChanges;
            AudioVisualizerBeautified.StartPositionSeconds = audioVisualizer.StartPositionSeconds;
            AudioVisualizerBeautified.ZoomFactor = audioVisualizer.ZoomFactor;
            AudioVisualizerBeautified.VerticalZoomFactor = audioVisualizer.VerticalZoomFactor;
        }

        _dirty = true;
        _timerUpdatePreview.Start();
    }

    private IEnumerable<Paragraph> ApplyBeautify(IEnumerable<Paragraph> input)
    {
        var paragraphs = input.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        var result = new List<Paragraph>();

        for (int i = 0; i < paragraphs.Count; i++)
        {
            var p = new Paragraph(paragraphs[i]);

            // Frame alignment
            if (Settings.SnapToFrames)
            {
                p.StartTime = AlignToFrame(p.StartTime, _frameRate);
                p.EndTime = AlignToFrame(p.EndTime, _frameRate);
            }

            // Shot change snapping
            if (_shotChanges.Count > 0)
            {
                p.StartTime = SnapStartToShotChange(p.StartTime, Settings.ShotChangeThresholdMs, Settings.ShotChangeOffsetFrames, _frameRate);
                p.EndTime = SnapEndToShotChange(p.EndTime, Settings.ShotChangeThresholdMs, Settings.ShotChangeOffsetFrames, _frameRate);
            }

            // Gap management with previous paragraph
            if (i > 0)
            {
                var previousEnd = result[i - 1].EndTime.TotalMilliseconds;
                var frameGapMs = FramesToMilliseconds(Settings.FrameGap, _frameRate);
                var minStart = previousEnd + frameGapMs;

                if (p.StartTime.TotalMilliseconds < minStart)
                {
                    p.StartTime = new TimeCode(minStart);
                }
            }

            // Validation: Ensure End > Start
            if (p.EndTime.TotalMilliseconds <= p.StartTime.TotalMilliseconds)
            {
                p.EndTime = new TimeCode(p.StartTime.TotalMilliseconds + Settings.MinDurationMs);
            }

            // Validation: Ensure minimum duration
            var duration = p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds;
            if (duration < Settings.MinDurationMs)
            {
                p.EndTime = new TimeCode(p.StartTime.TotalMilliseconds + Settings.MinDurationMs);
            }

            result.Add(p);
        }

        return result;
    }

    private TimeCode AlignToFrame(TimeCode timeCode, double fps)
    {
        var totalMilliseconds = timeCode.TotalMilliseconds;
        var frameNumber = Math.Round(totalMilliseconds * fps / 1000.0);
        var alignedMilliseconds = frameNumber * 1000.0 / fps;
        return new TimeCode(alignedMilliseconds);
    }

    private TimeCode SnapStartToShotChange(TimeCode timeCode, int thresholdMs, int offsetFrames, double fps)
    {
        var timeSeconds = timeCode.TotalMilliseconds / 1000.0;
        var thresholdSeconds = thresholdMs / 1000.0;

        foreach (var shotChange in _shotChanges)
        {
            var diff = Math.Abs(timeSeconds - shotChange);
            if (diff < thresholdSeconds)
            {
                // Snap to offsetFrames AFTER the shot change
                var offsetSeconds = offsetFrames / fps;
                var snappedSeconds = shotChange + offsetSeconds;
                return new TimeCode(snappedSeconds * 1000.0);
            }
        }

        return timeCode;
    }

    private TimeCode SnapEndToShotChange(TimeCode timeCode, int thresholdMs, int offsetFrames, double fps)
    {
        var timeSeconds = timeCode.TotalMilliseconds / 1000.0;
        var thresholdSeconds = thresholdMs / 1000.0;

        foreach (var shotChange in _shotChanges)
        {
            var diff = Math.Abs(timeSeconds - shotChange);
            if (diff < thresholdSeconds)
            {
                // Snap to offsetFrames BEFORE the shot change
                var offsetSeconds = offsetFrames / fps;
                var snappedSeconds = shotChange - offsetSeconds;
                return new TimeCode(Math.Max(0, snappedSeconds * 1000.0));
            }
        }

        return timeCode;
    }

    private double FramesToMilliseconds(int frames, double fps)
    {
        return frames * 1000.0 / fps;
    }

    public List<SubtitleLineViewModel> GetBeautifiedSubtitles()
    {
        return new List<SubtitleLineViewModel>(_beautifiedSubtitles);
    }

    private void SyncVisualizers()
    {
        if (AudioVisualizerOriginal == null || AudioVisualizerBeautified == null)
        {
            return;
        }

        // Sync scroll position
        AudioVisualizerBeautified.StartPositionSeconds = AudioVisualizerOriginal.StartPositionSeconds;

        // Sync zoom
        AudioVisualizerBeautified.ZoomFactor = AudioVisualizerOriginal.ZoomFactor;
        AudioVisualizerBeautified.VerticalZoomFactor = AudioVisualizerOriginal.VerticalZoomFactor;
    }

    [RelayCommand]
    private void Ok()
    {
        // Apply final beautification
        var beautifiedParagraphs = ApplyBeautify(_allSubtitles.Select(p => p.Paragraph!)).ToList();

        _allSubtitles.Clear();
        var subRipFormat = new Core.SubtitleFormats.SubRip();
        foreach (var p in beautifiedParagraphs)
        {
            _allSubtitles.Add(new SubtitleLineViewModel(p, subRipFormat));
        }

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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            //UiUtil.ShowHelp("features/beautify-time-codes");
        }
    }

    internal void ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        _dirty = true;
    }
}