using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
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

    public ModifySelectionViewModel()
    {
        Rules = new ObservableCollection<ModifySelectionRule>(ModifySelectionRule.List());
        SelectedRule = Rules.First();

        Subtitles = new ObservableCollection<PreviewItem>();

        LoadSettings();
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
        foreach (var item in subtitleLineViewModels)
        {
            var previewItem = new PreviewItem(item.Number, true, item.Text, item);
            Subtitles.Add(previewItem);
        }
    }
}