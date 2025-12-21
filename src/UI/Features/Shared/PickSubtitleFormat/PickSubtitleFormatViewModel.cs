using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;

public partial class PickSubtitleFormatViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ObservableCollection<string> _subtitleFormatNames;
    [ObservableProperty] private string? _selectedSubtitleFormatName;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    
    private readonly List<string> _allSubtitleFormatNames;

    public PickSubtitleFormatViewModel()
    {
       _allSubtitleFormatNames = SubtitleFormat.AllSubtitleFormats
           .Select(x => x.FriendlyName)
           .ToList();
        
        SubtitleFormatNames = new ObservableCollection<string>(_allSubtitleFormatNames);
        SearchText = string.Empty;
    }

    internal void Initialize(SubtitleFormat subtitleFormat)
    {
        SelectedSubtitleFormatName = subtitleFormat.FriendlyName;
    }

    private void UpdateSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText) && SubtitleFormatNames.Count == _allSubtitleFormatNames.Count)
        {
            return;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            SubtitleFormatNames.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ObservableCollectionExtensions.AddRange(SubtitleFormatNames, _allSubtitleFormatNames);
                return;
            }

            foreach (var encoding in _allSubtitleFormatNames)
            {
                if (encoding.Contains((string)SearchText, StringComparison.InvariantCultureIgnoreCase))
                {
                    SubtitleFormatNames.Add(encoding);
                }
            }

            if (SubtitleFormatNames.Count > 0)
            {
                SelectedSubtitleFormatName = SubtitleFormatNames[0];
            }
        });
    }

    private void UpdatePreview()
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

    public void SearchTextChanged()
    {
    }
}