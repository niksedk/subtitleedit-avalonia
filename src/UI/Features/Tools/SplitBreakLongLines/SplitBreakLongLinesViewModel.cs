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

namespace Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

public partial class SplitBreakLongLinesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SplitBreakLongLinesItem> _fixes;
    [ObservableProperty] private SplitBreakLongLinesItem? _selectedFix;

    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;

    [ObservableProperty] private bool _splitLongLines;
    [ObservableProperty] private int _singleLineMaxLength;
    [ObservableProperty] private int _maxNumberOfLines;

    [ObservableProperty] private bool _rebalanceLongLines;

    [ObservableProperty] private string _fixesInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> AllSubtitlesFixed { get; set; }

    private List<SubtitleLineViewModel> _allSubtitles;

    private readonly System.Timers.Timer _previewTimer;
    private bool _isDirty;
    private List<double> _shotChanges;

    public SplitBreakLongLinesViewModel()
    {
        Fixes = new ObservableCollection<SplitBreakLongLinesItem>();
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

            var splitCount = 0;
            var rebalanceCount = 0;
            var maxCharactersPerSubtitle = MaxNumberOfLines * SingleLineMaxLength;

            if (SplitLongLines)
            {
                for (var index = 0; index < _allSubtitles.Count; index++)
                {
                    var item = new SubtitleLineViewModel(_allSubtitles[index]);

                    var splitLines = Split(item, maxCharactersPerSubtitle);
                    if (splitLines.Count > 1)
                    {
                        splitCount++;
                        var fixItem = new SplitBreakLongLinesItem("Split long line", index + 1, string.Format("Split into {0} lines", splitLines.Count), item);
                        Fixes.Add(fixItem);
                    }
                    foreach (var s in splitLines)
                    {
                        AllSubtitlesFixed.Add(s);
                    }
                }
            }

            if (RebalanceLongLines)
            {
                for (var index = 0; index < AllSubtitlesFixed.Count; index++)
                {
                    var item = AllSubtitlesFixed[index];
                    var rebalancedText = item.Text = Utilities.AutoBreakLine(item.Text, SingleLineMaxLength, Se.Settings.General.UnbreakLinesShorterThan, "en");
                    if (rebalancedText != item.Text)
                    {
                        rebalanceCount++;
                        var fixItem = new SplitBreakLongLinesItem("Rebalance long line", index + 1, "Rebalanced line", item);
                        Fixes.Add(fixItem);
                    }
                }
            }

            if (splitCount == 0 && rebalanceCount == 0)
            {
                FixesInfo = Se.Language.Tools.ApplyDurationLimits.NoChangesNeeded;
                return;
            }

            if (rebalanceCount == 0)
            {
                FixesInfo = string.Format("Lines split: {0}", splitCount);
            }
            else
            {
                FixesInfo = string.Format("Lines split: {0}, lines rebalanced: {1}", splitCount, rebalanceCount);
            }
        });
    }

    private List<SubtitleLineViewModel> Split(SubtitleLineViewModel item, int maxCharactersPerSubtitle)
    {
        var lines = new List<SubtitleLineViewModel>();

        //TODO: implement splitting logic here
        // Try to split early when having periods, questions marks, exclamation marks, commas etc.
        // Time should be divided equally between the new lines by number of characters

        return lines;
    }

    private void LoadSettings()
    {
        SingleLineMaxLength = Se.Settings.General.SubtitleLineMaximumLength;
        MaxNumberOfLines = Se.Settings.General.MaxNumberOfLines;
        SplitLongLines = Se.Settings.Tools.SplitRebalanceLongLinesSplit;
        RebalanceLongLines = Se.Settings.Tools.SplitRebalanceLongLinesRebalance;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.SplitRebalanceLongLinesSplit = SplitLongLines;
        Se.Settings.Tools.SplitRebalanceLongLinesRebalance = RebalanceLongLines;
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