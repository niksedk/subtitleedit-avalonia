using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ExportImageBasedViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<string> _fontFamilies;
    [ObservableProperty] SubtitleLineViewModel? _selectedFontFamily;
    [ObservableProperty] private ObservableCollection<int> _fontSizes;
    [ObservableProperty] SubtitleLineViewModel? _selectedFontSize;
    [ObservableProperty] private ObservableCollection<ResolutionItem> _resolutions;
    [ObservableProperty] ResolutionItem? _selectedResolution;
    [ObservableProperty] private ObservableCollection<int> _topBottomMargins;
    [ObservableProperty] SubtitleLineViewModel? _selectedTopBottomMargin;
    [ObservableProperty] private ObservableCollection<int> _leftRightMargins;
    [ObservableProperty] SubtitleLineViewModel? _selectedLeftRightMargin;
    [ObservableProperty] private ObservableCollection<string> _borderStyles;
    [ObservableProperty] SubtitleLineViewModel? _selectedBorderStyle;
    [ObservableProperty] private bool _isBold;
    [ObservableProperty] private Color _fontColor;
    [ObservableProperty] private Color _borderColor;
    [ObservableProperty] private Color _shadowColor;
    [ObservableProperty] private TimeSpan _startOfProgramme;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid SubtitleGrid { get; set; }

    private string _subtitleFileName;
    private string _videoFileName;

    public ExportImageBasedViewModel()
    {
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        FontFamilies = new ObservableCollection<string>();
        FontSizes = new ObservableCollection<int>();
        Resolutions = new ObservableCollection<ResolutionItem>(ResolutionItem.GetResolutions());
        TopBottomMargins = new ObservableCollection<int> { 0, 5, 10, 15, 20, 25, 30 };
        LeftRightMargins = new ObservableCollection<int> { 0, 5, 10, 15, 20, 25, 30 };
        BorderStyles = new ObservableCollection<string> { "None", "Solid", "Dashed", "Dotted" };
        FontColor = Colors.White;
        SubtitleGrid = new DataGrid();
        Title = string.Empty;

        _subtitleFileName = string.Empty;
        _videoFileName = string.Empty;
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

    [RelayCommand]
    private void Export()
    {
    }

    [RelayCommand]
    private void ChangeFontColor()
    {
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    public void Initialize(
        ExportImageType exportImageType, 
        ObservableCollection<SubtitleLineViewModel> subtitles, 
        string? subtitleFileName, 
        string? videoFileName)
    {
        Subtitles.Clear();
        Subtitles.AddRange(subtitles);

        if (exportImageType == ExportImageType.BluRaySup)
        {
            Title = "Export Blu-ray sup";
        }
    }

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
    }
}