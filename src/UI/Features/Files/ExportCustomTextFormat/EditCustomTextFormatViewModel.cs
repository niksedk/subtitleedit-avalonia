using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public partial class EditCustomTextFormatViewModel : ObservableObject
{
    [ObservableProperty] private CustomFormatItem? _selectedCustomFormat;

    [ObservableProperty] private string _previewText;
    [ObservableProperty] private string _title;

    private List<SubtitleLineViewModel> _subtitles;
    private string? _subtitleFileNAme;
    private string? _videoFileName;
    private CustomFormatItem _customFormatItem;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public EditCustomTextFormatViewModel()
    {
        Title = string.Empty;
        PreviewText = string.Empty;
        _customFormatItem = new CustomFormatItem();
        _subtitles = new List<SubtitleLineViewModel>();
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
    private void FormatEdit()
    {
    }

    [RelayCommand]
    private void FormatDelete()
    {
    }

    [RelayCommand]
    private void FormatNew()
    {
    }

    [RelayCommand]
    private void SaveAs()
    {
        SaveSettings();
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

    internal void OnCustomFormatGridDoubleTapped(object? sender, TappedEventArgs e)
    {
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
    }

    internal void Initialize(CustomFormatItem selected, string title)
    {
        SelectedCustomFormat = selected;
        Title = title;
    }
}