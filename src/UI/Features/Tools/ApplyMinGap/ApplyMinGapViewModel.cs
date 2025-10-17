using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyMinGap;

public partial class ApplyMinGapViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ApplyMinGapItem> _subtitles;
    [ObservableProperty] private ApplyMinGapItem? _selectedSubtitle;
    [ObservableProperty] private string _minXBetweenLines;
    [ObservableProperty] private int _minGapMs;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _allSubtitles;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private Dictionary<string, string> _dic;

    public ApplyMinGapViewModel()
    {
        Subtitles = new ObservableCollection<ApplyMinGapItem>();
        AllSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        MinGapMs = 10;
        StatusText = string.Empty;
        _dic = new Dictionary<string, string>();

        if (Se.Settings.General.UseFrameMode)
        {
            MinXBetweenLines = Se.Language.Tools.ApplyMinGaps.MinFramesBetweenLines;
        }
        else
        {
            MinXBetweenLines = Se.Language.Tools.ApplyMinGaps.MinMsBetweenLines;
        }

        LoadSettings();

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
    }

    private void UpdatePreview()
    {
        _dic = new Dictionary<string, string>();
        var fixedIndexes = new List<int>(Subtitles.Count);
        var minMsBetweenLines = MinGapMs;
        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
        {
            minMsBetweenLines = SubtitleFormat.FramesToMilliseconds(minMsBetweenLines);
        }

        var allSubtitles = new ObservableCollection<SubtitleLineViewModel>(AllSubtitles.Select(p => new SubtitleLineViewModel(p)));
        var fixedCount = 0;

        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            foreach (var v in allSubtitles)
            {
                var vm = new ApplyMinGapItem(v);
                Subtitles.Add(vm);
            }

            for (var i = 0; i < Subtitles.Count - 1; i++)
            {
                var cur = Subtitles[i];
                if (_dic.ContainsKey(cur.SubtitleLineViewModel.Id.ToString()))
                {
                    cur.InfoText = _dic[cur.SubtitleLineViewModel.Id.ToString()];
                }
                else
                {
                    var info = string.Empty;
                    var next = Subtitles[i + 1];
                    if (next != null)
                    {
                        var gap = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;
                        info = $"{gap / TimeCode.BaseUnit:0.000}";
                        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                        {
                            info = $"{SubtitleFormat.MillisecondsToFrames(gap)}";
                        }
                    }
                }

            }

            StatusText = string.Format(Se.Language.Tools.ApplyMinGaps.NumberOfGapsFixedX, fixedCount);
        });
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        AllSubtitles.Clear();
        AllSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    private void LoadSettings()
    {
        MinGapMs = Se.Settings.General.MinimumMillisecondsBetweenLines;
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
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

    internal void ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        _dirty = true;
    }
}