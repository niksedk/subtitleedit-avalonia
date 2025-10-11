using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using SharpCompress.Compressors.RLE90;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public partial class ModifySelectionViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ModifySelectionRule> _rules;
    [ObservableProperty] private ModifySelectionRule _selectedRule;

    [ObservableProperty] private ObservableCollection<PreviewItem> _subtitles;
    [ObservableProperty] private PreviewItem? _selectedSubtitle;

    [ObservableProperty] private bool _selectionNew;
    [ObservableProperty] private bool _selectionAdd;
    [ObservableProperty] private bool _selectionSubtract;
    [ObservableProperty] private bool _selectionIntersect;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private List<SubtitleLineViewModel> _allSubtitles;  
    private readonly System.Timers.Timer _previewTimer;
    private bool _isDirty;

    public ModifySelectionViewModel()
    {
        Rules = new ObservableCollection<ModifySelectionRule>(ModifySelectionRule.List());
        SelectedRule = Rules.First();

        Subtitles = new ObservableCollection<PreviewItem>();

        _allSubtitles = new List<SubtitleLineViewModel>();

        LoadSettings();

        _previewTimer = new System.Timers.Timer(500);
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
        var rule = SelectedRule;
        if (rule == null || _allSubtitles.Count == 0)
        {
            return;
        }

        Subtitles.Clear();
        foreach (var item in _allSubtitles)
        {
            if (rule.IsMatch(item))
            {
                var previewItem = new PreviewItem(item.Number, true, item.Text, item);
                Subtitles.Add(previewItem);
            }
        }
    }

    private void LoadSettings()
    {
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

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(List<SubtitleLineViewModel> subtitleLineViewModels, List<SubtitleLineViewModel> selectedItems)
    {
        _allSubtitles = subtitleLineViewModels;
    }
}