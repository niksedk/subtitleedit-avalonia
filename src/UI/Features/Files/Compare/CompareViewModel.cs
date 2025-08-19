using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public partial class CompareViewModel : ObservableObject
{

    public ObservableCollection<CompareItem> LeftSubtitles { get; } = new();
    public ObservableCollection<CompareItem> RightSubtitles { get; } = new();

    [ObservableProperty] private CompareItem? _selectedLeft;
    [ObservableProperty] private CompareItem? _selectedRight;
    [ObservableProperty] private bool _ignoreFormatting;
    [ObservableProperty] private bool _ignoreWhiteSpace;
    [ObservableProperty] private bool _onlyShowDifferences;
    [ObservableProperty] private bool _onlyCheckTextDifferences;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    private IFileHelper _fileHelper;
    private string _leftFileName = string.Empty;
    private string _rightFileName = string.Empty;
    private List<SubtitleLineViewModel> _leftLines = new();
    private List<SubtitleLineViewModel> _rightLines = new();
    private List<int> _differences = new();
    private string _language = string.Empty;

    private static readonly IBrush ListViewRed = new SolidColorBrush(Colors.Red);
    private static readonly IBrush ListViewGreen = new SolidColorBrush(Colors.LightGreen);
    private static readonly IBrush ListViewOrange = new SolidColorBrush(Color.FromArgb(255, 255, 200, 100));
    private static readonly IBrush TransparentBrush = new SolidColorBrush(Colors.Transparent);

    public CompareViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
    }

    internal void Initialize(
        ObservableCollection<SubtitleLineViewModel> left,
        string leftFileName,
        ObservableCollection<SubtitleLineViewModel> right,
        string rightFileName)
    {
        _leftLines.Clear();
        _leftLines.AddRange(left.Select(p => new SubtitleLineViewModel(p)));
        _leftFileName = leftFileName;

        _rightLines.Clear();
        _rightLines.AddRange(right.Select(p => new SubtitleLineViewModel(p)));
        _rightFileName = rightFileName;

        Compare();
    }

    private void Compare()
    {
        LeftSubtitles.Clear();
        foreach (var l in _leftLines)
        {
            LeftSubtitles.Add(new CompareItem(l));
        }

        RightSubtitles.Clear();
        foreach (var r in _rightLines)
        {
            RightSubtitles.Add(new CompareItem(r));
        }

        InsertMissingLines();

        // coloring + differences index list
        AddColoringAndCountDifferences();
    }

    private void AddColoringAndCountDifferences()
    {
        _differences = new List<int>();
        var index = 0;
        var left = GetLeftItemOrNull(index);
        var right = GetRightItemOrNull(index);
        var totalWords = 0;
        var wordsChanged = 0;
        var max = Math.Max(LeftSubtitles.Count, RightSubtitles.Count);
        var min = Math.Min(LeftSubtitles.Count, RightSubtitles.Count);
        var onlyTextDiff = OnlyCheckTextDifferences;

        // Reset all background colors first
        ResetAllBackgroundColors();

        if (onlyTextDiff)
        {
            while (index < min)
            {
                var addIndexToDifferences = false;
                Utilities.GetTotalAndChangedWords(left?.Text, right?.Text, ref totalWords, ref wordsChanged, IgnoreWhiteSpace, IgnoreFormatting, ShouldBreakToLetter());

                if (left == null || left.IsDefault)
                {
                    addIndexToDifferences = true;
                    SetItemBackgroundColor(index, true, ListViewRed, ItemColumn.All);
                }
                else if (right == null || right.IsDefault)
                {
                    addIndexToDifferences = true;
                    SetItemBackgroundColor(index, false, ListViewRed, ItemColumn.All);
                }
                else if (!AreTextsEqual(left, right))
                {
                    addIndexToDifferences = true;
                    SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.Text);
                    SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.Text);
                }

                if (addIndexToDifferences)
                {
                    _differences.Add(index);
                }

                index++;
                left = GetLeftItemOrNull(index);
                right = GetRightItemOrNull(index);
            }
        }
        else
        {
            while (index < min)
            {
                Utilities.GetTotalAndChangedWords(left?.Text, right?.Text, ref totalWords, ref wordsChanged, IgnoreWhiteSpace, IgnoreFormatting, ShouldBreakToLetter());
                var addIndexToDifferences = false;

                if (left == null || left.IsDefault)
                {
                    addIndexToDifferences = true;
                    SetItemBackgroundColor(index, true, ListViewRed, ItemColumn.All);
                }
                else if (right == null || right.IsDefault)
                {
                    addIndexToDifferences = true;
                    SetItemBackgroundColor(index, false, ListViewRed, ItemColumn.All);
                }
                else
                {
                    var columnsAlike = GetColumnsEqualExceptNumber(left, right);
                    // Not alike paragraphs
                    if (columnsAlike == 0)
                    {
                        addIndexToDifferences = true;
                        SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.All);
                        SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.All);
                        // Keep number column with default background
                        SetItemBackgroundColor(index, true, TransparentBrush, ItemColumn.Number);
                        SetItemBackgroundColor(index, false, TransparentBrush, ItemColumn.Number);
                    }
                    else if (columnsAlike < 4)
                    {
                        addIndexToDifferences = true;
                        // Start time
                        if (!IsTimeEqual(left.StartTime, right.StartTime))
                        {
                            SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.StartTime);
                            SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.StartTime);
                        }
                        // End time
                        if (!IsTimeEqual(left.EndTime, right.EndTime))
                        {
                            SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.EndTime);
                            SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.EndTime);
                        }
                        // Duration
                        if (!IsTimeEqual(left.Duration, right.Duration))
                        {
                            // Since we don't have a duration column in the grid, we can highlight the time columns
                            SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.StartTime);
                            SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.StartTime);
                            SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.EndTime);
                            SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.EndTime);
                        }
                        // Text
                        else if (!AreTextsEqual(left, right))
                        {
                            SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.Text);
                            SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.Text);
                        }
                    }
                    // Number
                    if (left.Number != right.Number)
                    {
                        addIndexToDifferences = true;
                        SetItemBackgroundColor(index, true, ListViewOrange, ItemColumn.Number);
                        SetItemBackgroundColor(index, false, ListViewOrange, ItemColumn.Number);
                    }
                }

                if (addIndexToDifferences)
                {
                    _differences.Add(index);
                }

                index++;
                left = GetLeftItemOrNull(index);
                right = GetRightItemOrNull(index);
            }
        }
    }

    private enum ItemColumn
    {
        All,
        Number,
        StartTime,
        EndTime,
        Text
    }

    private void SetItemBackgroundColor(int index, bool isLeft, IBrush brush, ItemColumn column)
    {
        var collection = isLeft ? LeftSubtitles : RightSubtitles;
        if (index >= collection.Count) return;

        var item = collection[index];

        switch (column)
        {
            case ItemColumn.All:
                item.NumberBackgroundBrush = brush;
                item.StartTimeBackgroundBrush = brush;
                item.EndTimeBackgroundBrush = brush;
                item.TextBackgroundBrush = brush;
                break;
            case ItemColumn.Number:
                item.NumberBackgroundBrush = brush;
                break;
            case ItemColumn.StartTime:
                item.StartTimeBackgroundBrush = brush;
                break;
            case ItemColumn.EndTime:
                item.EndTimeBackgroundBrush = brush;
                break;
            case ItemColumn.Text:
                item.TextBackgroundBrush = brush;
                break;
        }
    }

    private void ResetAllBackgroundColors()
    {
        foreach (var item in LeftSubtitles)
        {
            item.NumberBackgroundBrush = TransparentBrush;
            item.StartTimeBackgroundBrush = TransparentBrush;
            item.EndTimeBackgroundBrush = TransparentBrush;
            item.TextBackgroundBrush = TransparentBrush;
        }

        foreach (var item in RightSubtitles)
        {
            item.NumberBackgroundBrush = TransparentBrush;
            item.StartTimeBackgroundBrush = TransparentBrush;
            item.EndTimeBackgroundBrush = TransparentBrush;
            item.TextBackgroundBrush = TransparentBrush;
        }
    }

    private bool ShouldBreakToLetter() => _language != null && (_language == "ja" || _language == "zh");

    private void InsertMissingLines()
    {
        var index = 0;
        var left = GetLeftItemOrNull(index);
        var right = GetRightItemOrNull(index);
        var max = Math.Max(_leftLines.Count, _rightLines.Count);
        while (index < max)
        {
            if (left != null && right != null && GetColumnsEqualExceptNumberAndDuration(left, right) == 0)
            {
                for (var i = index + 1; i < max; i++)
                {
                    // Try to find at least two matching properties
                    if (GetColumnsEqualExceptNumber(GetLeftItemOrNull(i), right) > 1)
                    {
                        for (var j = index; j < i; j++)
                        {
                            RightSubtitles.Insert(index++, new CompareItem());
                        }
                        break;
                    }

                    if (GetColumnsEqualExceptNumber(left, GetRightItemOrNull(i)) > 1)
                    {
                        for (var j = index; j < i; j++)
                        {
                            LeftSubtitles.Insert(index++, new CompareItem());
                        }
                        break;
                    }
                }
            }

            index++;
            left = GetLeftItemOrNull(index);
            right = GetRightItemOrNull(index);
        }

        // insert rest
        var minSub = LeftSubtitles.Count < RightSubtitles.Count ? LeftSubtitles : RightSubtitles;
        for (var idx = minSub.Count; idx < max; idx++)
        {
            minSub.Insert(idx, new CompareItem());
        }
    }

    private CompareItem? GetLeftItemOrNull(int index)
    {
        if (index >= 0 && index < LeftSubtitles.Count)
        {
            return LeftSubtitles[index];
        }

        return null;
    }

    private CompareItem? GetRightItemOrNull(int index)
    {
        if (index >= 0 && index < RightSubtitles.Count)
        {
            return RightSubtitles[index];
        }

        return null;
    }


    private int GetColumnsEqualExceptNumberAndDuration(CompareItem p1, CompareItem p2)
    {
        if (p1 == null || p2 == null)
        {
            return 0;
        }

        var columnsEqual = 0;
        if (IsTimeEqual(p1.StartTime, p2.StartTime))
        {
            columnsEqual++;
        }

        if (IsTimeEqual(p1.EndTime, p2.EndTime))
        {
            columnsEqual++;
        }

        if (AreTextsEqual(p1, p2))
        {
            columnsEqual++;
        }

        return columnsEqual;
    }

    private bool AreTextsEqual(CompareItem p1, CompareItem p2)
    {
        return p1.Text.Trim() == p2.Text.Trim() ||
                    IgnoreFormatting && HtmlUtil.RemoveHtmlTags(p1.Text.Trim()) == HtmlUtil.RemoveHtmlTags(p2.Text.Trim()) ||
                    IgnoreWhiteSpace && RemoveWhiteSpace(p1.Text) == RemoveWhiteSpace(p2.Text) ||
                    IgnoreFormatting && IgnoreWhiteSpace && RemoveWhiteSpace(HtmlUtil.RemoveHtmlTags(p1.Text)) == RemoveWhiteSpace(HtmlUtil.RemoveHtmlTags(p2.Text));
    }

    public static string RemoveWhiteSpace(string text)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in text)
        {
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private int GetColumnsEqualExceptNumber(CompareItem? left, CompareItem? right)
    {
        if (left == null || right == null)
        {
            return 0;
        }

        var columnsEqual = 0;
        if (IsTimeEqual(left.StartTime, right.StartTime))
        {
            columnsEqual++;
        }

        if (IsTimeEqual(left.EndTime, right.EndTime))
        {
            columnsEqual++;
        }

        if (IsTimeEqual(left.Duration, right.Duration))
        {
            columnsEqual++;
        }

        if (AreTextsEqual(left, right))
        {
            columnsEqual++;
        }

        return columnsEqual;
    }

    private static bool IsTimeEqual(TimeSpan t1, TimeSpan t2)
    {
        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
        {
            return new TimeCode(t1).ToDisplayString() == new TimeCode(t2).ToDisplayString();
        }

        const double tolerance = 0.1;
        return Math.Abs(t1.TotalMilliseconds - t2.TotalMilliseconds) < tolerance;
    }

    [RelayCommand]
    private async Task PickLeftSubtitleFile()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        _leftFileName = fileName;

        Compare();
    }

    [RelayCommand]
    private async Task PickRightSubtitleFile()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        _rightFileName = fileName;

        Compare();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
