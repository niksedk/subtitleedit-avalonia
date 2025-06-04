using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public partial class OcrViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<OcrEngineItem> _ocrEngines;
    [ObservableProperty] private OcrEngineItem? _selectedOcrEngine;
    [ObservableProperty] private ObservableCollection<OcrSubtitleItem> _ocrSubtitleItems;
    [ObservableProperty] private OcrSubtitleItem? _selectedOcrSubtitleItem;
    //[ObservableProperty] private ObservableCollection<int> _startFromNumbers;
    //[ObservableProperty] private int _selectedStartFromNumber;
    [ObservableProperty] private ObservableCollection<string> _nOcrDatabases;
    [ObservableProperty] private string? _selectedNOcrDatabase;
    [ObservableProperty] private ObservableCollection<int> _nOcrMaxWrongPixelsList;
    [ObservableProperty] private int _selectedNOcrMaxWrongPixels;
    [ObservableProperty] private ObservableCollection<int> _nOcrPixelsAreSpaceList;
    [ObservableProperty] private int _selectedNOcrPixelsAreSpace;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private Bitmap? _currentImageSource;
    [ObservableProperty] private string _currentBitmapInfo;
    [ObservableProperty] private string _currentText;
    [ObservableProperty] private bool _isOcrRunning;
    [ObservableProperty] private bool _isNOcrVisible;

    public OcrWindow? Window { get; set; }
    public DataGrid SubtitleGrid { get; set; }
    public MatroskaTrackInfo? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    private List<MatroskaTrackInfo> _matroskaTracks;
    private MatroskaFile? _matroskaFile;
    private IOcrSubtitle? _ocrSubtitle;

    public OcrViewModel()
    {
        OcrEngines = new ObservableCollection<OcrEngineItem>(OcrEngineItem.GetOcrEngines());
        SelectedOcrEngine = OcrEngines.FirstOrDefault();
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>();
        NOcrDatabases = new ObservableCollection<string>();
        NOcrMaxWrongPixelsList = new ObservableCollection<int>(Enumerable.Range(1, 500));
        NOcrPixelsAreSpaceList = new ObservableCollection<int>(Enumerable.Range(1, 50));
        SubtitleGrid = new DataGrid();
        WindowTitle = string.Empty;
        CurrentBitmapInfo = string.Empty;
        CurrentText = string.Empty;
        ProgressText = string.Empty;
        _matroskaTracks = new List<MatroskaTrackInfo>();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    [RelayCommand]
    private void StartOcr()
    {
        IsOcrRunning = true;
    }

    [RelayCommand]
    private void PauseOcr()
    {
        IsOcrRunning = false;
    }


    [RelayCommand]
    private void Export()
    {
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
        }
    }

    internal void DataGridTracksSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        bool flowControl = TrackChanged();
        if (!flowControl)
        {
            return;
        }
    }

    private bool TrackChanged()
    {
        return true;
    }

    public static Bitmap ConvertSKBitmapToAvaloniaBitmap(SKBitmap skBitmap)
    {
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream(data.ToArray());

        return new Bitmap(stream);
    }


    internal void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= OcrSubtitleItems.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleGrid.SelectedIndex = index;
            SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);
            TrackChanged();
        }, DispatcherPriority.Background);
    }

    public void Initialize(List<BluRaySupParser.PcsData> subtitles, string fileName)
    {
        _ocrSubtitle = new BluRayPcsDataList(subtitles);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    internal void EngineSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IsNOcrVisible = SelectedOcrEngine?.EngineType == OcrEngineType.nOcr;
    }
}