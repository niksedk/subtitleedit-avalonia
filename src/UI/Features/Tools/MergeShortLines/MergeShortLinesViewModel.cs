using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.MergeShortLines;

public partial class MergeShortLinesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MergeShortLinesItem> _fixes;
    [ObservableProperty] private MergeShortLinesItem? _selectedFix;

    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;

    [ObservableProperty] private int _singleLineMaxLength;
    [ObservableProperty] private int _maxNumberOfLines;

    [ObservableProperty] private string _fixesInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> AllSubtitlesFixed { get; set; }

    private List<SubtitleLineViewModel> _allSubtitles;

    private readonly System.Timers.Timer _previewTimer;
    private bool _isDirty;
    private List<double> _shotChanges;

    public MergeShortLinesViewModel()
    {
        Fixes = new ObservableCollection<MergeShortLinesItem>();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        _allSubtitles = new List<SubtitleLineViewModel>();
        _shotChanges = new List<double>();
        AllSubtitlesFixed = new List<SubtitleLineViewModel>();
        FixesInfo = string.Empty;

        LoadSettings();

        _previewTimer = new System.Timers.Timer(250);
        _previewTimer.Elapsed += (sender, args) =>
        {
            _previewTimer.Stop();

            if (_isDirty)
            {
                _isDirty = false;
                UpdatePreview();
            }

            _previewTimer.Start();
        };
    }

    private void UpdatePreview()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            AllSubtitlesFixed.Clear();
            Fixes.Clear();

            var mergeCount = 0;
            var maxCharactersPerSubtitle = MaxNumberOfLines * SingleLineMaxLength;

            var result = new List<SubtitleLineViewModel>(_allSubtitles.Count);
            var gapThresholdMs = Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs; // reuse existing setting for "close in time"

            for (var index = 0; index < _allSubtitles.Count; index++)
            {
                var baseVm = _allSubtitles[index];
                var current = new SubtitleLineViewModel(baseVm);

                var mergedWithCount = 0;
                var j = index + 1;
                while (j < _allSubtitles.Count)
                {
                    var next = _allSubtitles[j];

                    // stop if there is a shot change between current and next
                    var hasShotChangeBetween = _shotChanges != null && _shotChanges.Any(s =>
                        s > current.EndTime.TotalSeconds && s < next.StartTime.TotalSeconds);
                    if (hasShotChangeBetween)
                    {
                        break;
                    }

                    // Check temporal proximity
                    var gapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
                    if (gapMs > gapThresholdMs)
                    {
                        break;
                    }

                    // Check combined plain length limit
                    var combinedPlain = HtmlUtil.RemoveHtmlTags((current.Text ?? string.Empty) + " " + (next.Text ?? string.Empty), true)
                        .Replace("\r\n", " ").Replace('\n', ' ').Trim();
                    if (combinedPlain.Length > maxCharactersPerSubtitle)
                    {
                        break;
                    }

                    // Try to wrap combined text within SingleLineMaxLength and MaxNumberOfLines
                    var language = string.IsNullOrWhiteSpace(baseVm.Language) ? "en" : baseVm.Language;
                    var combinedRaw = (current.Text ?? string.Empty).TrimEnd() + " " + (next.Text ?? string.Empty).TrimStart();
                    var wrapped = Utilities.AutoBreakLine(combinedRaw, SingleLineMaxLength, Se.Settings.General.UnbreakLinesShorterThan, language);
                    var lineCount = wrapped.SplitToLines().Count;
                    if (lineCount > MaxNumberOfLines)
                    {
                        break;
                    }

                    // Merge
                    current.Text = wrapped;
                    current.EndTime = next.EndTime;
                    current.UpdateDuration();
                    mergedWithCount++;
                    mergeCount++;

                    // fix item for this merge step
                    var fix = new MergeShortLinesItem(
                        "Merge short lines",
                        index + 1,
                        $"Merged line {j + 1} into {index + 1}",
                        new SubtitleLineViewModel(current));
                    Fixes.Add(fix);

                    j++;
                }

                result.Add(current);
                // Skip the lines we merged into current
                index = j - 1;
            }

            AllSubtitlesFixed.AddRange(result);

            if (mergeCount == 0)
            {
                FixesInfo = Se.Language.Tools.ApplyDurationLimits.NoChangesNeeded;
            }
            else
            {
                FixesInfo = $"Lines merged: {mergeCount}";
            }
        });
    }

    private void LoadSettings()
    {
        SingleLineMaxLength = Se.Settings.General.SubtitleLineMaximumLength;
        MaxNumberOfLines = Se.Settings.General.MaxNumberOfLines;
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        if (Window == null)
        {
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    public void Initialize(List<SubtitleLineViewModel> toList, List<double> shotChanges)
    {
        _allSubtitles = toList;
        _shotChanges = shotChanges;
        _previewTimer.Start();
    }

    internal void SetChanged()
    {
        _isDirty = true;
    }
}