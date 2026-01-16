using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.BridgeGaps;

public class SortCriterion : ObservableObject
{
    private string _property;
    private bool _ascending;

    public string Property
    {
        get => _property;
        set => SetProperty(ref _property, value);
    }

    public bool Ascending
    {
        get => _ascending;
        set => SetProperty(ref _ascending, value);
    }

    public string DisplayName => Ascending ? $"{Property} ↑" : $"{Property} ↓";

    public SortCriterion(string property, bool ascending = true)
    {
        _property = property;
        _ascending = ascending;
    }
}

public partial class SortByViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _allSubtitles;
    [ObservableProperty] private ObservableCollection<string> _availableProperties;
    [ObservableProperty] private ObservableCollection<SortCriterion> _sortCriteria;
    [ObservableProperty] private SortCriterion? _selectedSortCriterion;
    [ObservableProperty] private string? _selectedAvailableProperty;
    [ObservableProperty] private bool _newCriterionAscending = true;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private readonly List<SubtitleLineViewModel> _originalSubtitles;


    public SortByViewModel()
    {
        _subtitles = new ObservableCollection<SubtitleLineViewModel>();
        _allSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        _originalSubtitles = new List<SubtitleLineViewModel>();
        _availableProperties = new ObservableCollection<string>();
        _sortCriteria = new ObservableCollection<SortCriterion>();

        InitializeAvailableProperties();
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

    private void InitializeAvailableProperties()
    {
        AvailableProperties.Add("Number");
        AvailableProperties.Add("StartTime");
        AvailableProperties.Add("EndTime");
        AvailableProperties.Add("Duration");
        AvailableProperties.Add("Text");
        AvailableProperties.Add("OriginalText");
        AvailableProperties.Add("Style");
        AvailableProperties.Add("Actor");
        AvailableProperties.Add("Layer");
        AvailableProperties.Add("Gap");
        AvailableProperties.Add("CharactersPerSecond");
        AvailableProperties.Add("WordsPerMinute");
    }

    private void UpdatePreview()
    {
        if (SortCriteria.Count == 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var sorted = ApplySorting(_originalSubtitles);
            Subtitles.Clear();
            foreach (var item in sorted)
            {
                Subtitles.Add(item);
            }
        });
    }

    private List<SubtitleLineViewModel> ApplySorting(List<SubtitleLineViewModel> items)
    {
        if (SortCriteria.Count == 0)
        {
            return items.ToList();
        }

        IOrderedEnumerable<SubtitleLineViewModel>? orderedItems = null;

        for (var i = 0; i < SortCriteria.Count; i++)
        {
            var criterion = SortCriteria[i];
            var isFirst = i == 0;

            if (isFirst)
            {
                orderedItems = criterion.Ascending
                    ? items.OrderBy(item => GetPropertyValue(item, criterion.Property))
                    : items.OrderByDescending(item => GetPropertyValue(item, criterion.Property));
            }
            else
            {
                orderedItems = criterion.Ascending
                    ? orderedItems!.ThenBy(item => GetPropertyValue(item, criterion.Property))
                    : orderedItems!.ThenByDescending(item => GetPropertyValue(item, criterion.Property));
            }
        }

        return orderedItems?.ToList() ?? items.ToList();
    }

    private object GetPropertyValue(SubtitleLineViewModel item, string propertyName)
    {
        return propertyName switch
        {
            "Number" => item.Number,
            "StartTime" => item.StartTime.TotalMilliseconds,
            "EndTime" => item.EndTime.TotalMilliseconds,
            "Duration" => item.Duration.TotalMilliseconds,
            "Text" => item.Text ?? string.Empty,
            "OriginalText" => item.OriginalText ?? string.Empty,
            "Style" => item.Style ?? string.Empty,
            "Actor" => item.Actor ?? string.Empty,
            "Layer" => item.Layer,
            "Gap" => item.Gap,
            "CharactersPerSecond" => item.CharactersPerSecond,
            "WordsPerMinute" => item.WordsPerMinute,
            _ => string.Empty
        };
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        _originalSubtitles.Clear();
        _originalSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));
        
        AllSubtitles.Clear();
        AllSubtitles.AddRange(_originalSubtitles.Select(p => new SubtitleLineViewModel(p)));
        
        Subtitles.Clear();
        foreach (var sub in AllSubtitles)
        {
            Subtitles.Add(new SubtitleLineViewModel(sub));
        }
        
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    private void LoadSettings()
    {
       
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private void AddSortCriterion()
    {
        if (string.IsNullOrEmpty(SelectedAvailableProperty))
        {
            return;
        }

        var criterion = new SortCriterion(SelectedAvailableProperty, NewCriterionAscending);
        SortCriteria.Add(criterion);
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    [RelayCommand]
    private void RemoveSortCriterion()
    {
        if (SelectedSortCriterion == null)
        {
            return;
        }

        SortCriteria.Remove(SelectedSortCriterion);
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    [RelayCommand]
    private void MoveSortCriterionUp()
    {
        if (SelectedSortCriterion == null)
        {
            return;
        }

        var index = SortCriteria.IndexOf(SelectedSortCriterion);
        if (index > 0)
        {
            SortCriteria.Move(index, index - 1);
            _dirty = true;
            _timerUpdatePreview.Start();
        }
    }

    [RelayCommand]
    private void MoveSortCriterionDown()
    {
        if (SelectedSortCriterion == null)
        {
            return;
        }

        var index = SortCriteria.IndexOf(SelectedSortCriterion);
        if (index < SortCriteria.Count - 1)
        {
            SortCriteria.Move(index, index + 1);
            _dirty = true;
            _timerUpdatePreview.Start();
        }
    }

    [RelayCommand]
    private void ToggleSortDirection()
    {
        if (SelectedSortCriterion == null)
        {
            return;
        }

        SelectedSortCriterion.Ascending = !SelectedSortCriterion.Ascending;
        OnPropertyChanged(nameof(SelectedSortCriterion));
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    [RelayCommand]
    private void ClearSortCriteria()
    {
        SortCriteria.Clear();
        Subtitles.Clear();
        foreach (var sub in AllSubtitles)
        {
            Subtitles.Add(new SubtitleLineViewModel(sub));
        }
    }

    [RelayCommand]
    private void Ok()
    {
        if (SortCriteria.Count > 0)
        {
            AllSubtitles.Clear();
            AllSubtitles.AddRange(Subtitles.Select(p => new SubtitleLineViewModel(p)));
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