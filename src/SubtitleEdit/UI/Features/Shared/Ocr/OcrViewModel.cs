using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public partial class OcrViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<OcrEngineItem> _ocrEngines;
    [ObservableProperty] private OcrEngineItem? _selectedOcrEngine;
    [ObservableProperty] private ObservableCollection<OcrSubtitleItem> _ocrSubtitleItems;
    [ObservableProperty] private OcrSubtitleItem? _selectedOcrSubtitleItem;
    [ObservableProperty] private ObservableCollection<int> _startFromNumbers;
    [ObservableProperty] private int _selectedStartFromNumber;
    [ObservableProperty] private Bitmap? _currentImageSource;
    [ObservableProperty] private string _currentBitmapInfo;
    [ObservableProperty] private string _currentText;

    public OcrWindow? Window { get; set; }
    public DataGrid SubtitleGrid { get; set; }
    public MatroskaTrackInfo? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    private List<MatroskaTrackInfo> _matroskaTracks;
    private MatroskaFile? _matroskaFile;
    private IOcrSubtitle _ocrSubtitle;

    public OcrViewModel()
    {
        OcrEngines = new ObservableCollection<OcrEngineItem>(OcrEngineItem.GetOcrEngines());
        SelectedOcrEngine = OcrEngines.FirstOrDefault();
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>();
        StartFromNumbers = new ObservableCollection<int>();
        SubtitleGrid = new DataGrid();
        WindowTitle = string.Empty;
        CurrentBitmapInfo = string.Empty;
        CurrentText = string.Empty;
        _matroskaTracks = new List<MatroskaTrackInfo>();
    }

    public void Initialize(MatroskaFile matroskaFile, List<MatroskaTrackInfo> matroskaTracks, string fileName)
    {
        _matroskaFile = matroskaFile;
        _matroskaTracks = matroskaTracks;
        WindowTitle = $"Pick Matroska track - {fileName}";
        foreach (var track in _matroskaTracks)
        {
            var display = new MatroskaTrackInfoDisplay
            {
                TrackNumber = track.TrackNumber,
                IsDefault = track.IsDefault,
                IsForced = track.IsForced,
                Codec = track.CodecId,
                Language = track.Language,
                Name = track.Name,
                MatroskaTrackInfo = track,
            };
          //  Tracks.Add(display);
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
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

    public void Initialize1(List<BluRaySupParser.PcsData> subtitles, string fileName)
    {
        _ocrSubtitle = new BluRayPcsDataList(subtitles);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
        StartFromNumbers = new ObservableCollection<int>(Enumerable.Range(1, _ocrSubtitle.Count));        
        //throw new NotImplementedException();
    }
}