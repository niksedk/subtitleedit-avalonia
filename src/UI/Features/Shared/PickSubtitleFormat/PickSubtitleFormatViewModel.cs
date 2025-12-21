using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;

public partial class PickSubtitleFormatViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ObservableCollection<string> _subtitleFormatNames;
    [ObservableProperty] private string? _selectedSubtitleFormatName;
    [ObservableProperty] private string _previewText;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    
    private readonly List<string> _allSubtitleFormatNames;
    private readonly Subtitle _sampleSubtitle;

    public PickSubtitleFormatViewModel()
    {
       _allSubtitleFormatNames = SubtitleFormat.AllSubtitleFormats
           .Select(x => x.FriendlyName)
           .ToList();
        
        SubtitleFormatNames = new ObservableCollection<string>(_allSubtitleFormatNames);
        SearchText = string.Empty;
        PreviewText = string.Empty;
        
        // Create a sample subtitle for preview
        _sampleSubtitle = new Subtitle();
        _sampleSubtitle.Paragraphs.Add(new Paragraph("Hello, World!", 1000, 3000));
        _sampleSubtitle.Paragraphs.Add(new Paragraph("This is a sample subtitle.", 3500, 6000));
    }

    internal void Initialize(SubtitleFormat subtitleFormat)
    {
        SelectedSubtitleFormatName = subtitleFormat.FriendlyName;
        UpdatePreview();
    }

    public SubtitleFormat? GetSelectedFormat()
    {
        if (string.IsNullOrEmpty(SelectedSubtitleFormatName))
        {
            return null;
        }

        return SubtitleFormat.AllSubtitleFormats.FirstOrDefault(x => x.FriendlyName == SelectedSubtitleFormatName);
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
                if (encoding.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase))
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
        if (string.IsNullOrEmpty(SelectedSubtitleFormatName))
        {
            PreviewText = string.Empty;
            return;
        }

        try
        {
            var format = SubtitleFormat.AllSubtitleFormats.FirstOrDefault(x => x.FriendlyName == SelectedSubtitleFormatName);
            if (format != null)
            {
                PreviewText = format.ToText(_sampleSubtitle, "Sample");
                
                // Limit preview to first 1000 characters for better display
                if (PreviewText.Length > 1000)
                {
                    PreviewText = PreviewText.Substring(0, 1000) + "\n\n... (truncated)";
                }
            }
            else
            {
                PreviewText = string.Empty;
            }
        }
        catch (Exception ex)
        {
            PreviewText = $"Error generating preview:\n{ex.Message}";
        }
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
        UpdateSearch();
    }
    
    public void SelectedSubtitleFormatNameChanged()
    {
        UpdatePreview();
    }
}