using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public partial class NOcrCharacterHistoryViewModel : ObservableObject
{
    public NOcrCharacterHistoryWindow? Window { get; set; }

    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<NOcrAddHistoryItem> _historyItems;
    [ObservableProperty] private string _newText;
    [ObservableProperty] private string _resolutionAndTopMargin;
    [ObservableProperty] private string _zoomFactorInfo;
    [ObservableProperty] private bool _isNewTextItalic;
    [ObservableProperty] private Bitmap _currentBitmap;

    public NOcrChar NOcrChar { get; private set; }
    public NOcrDrawingCanvasView NOcrDrawingCanvas { get; set; }
    public TextBox TextBoxNew { get; set; }
    public bool OkPressed { get; set; }
    private NOcrDb _nOcrDb;

    public NOcrCharacterHistoryViewModel()
    {
        Title = Se.Language.Ocr.AddNewCharcter;
        NewText = string.Empty;
        ResolutionAndTopMargin = string.Empty;
        IsNewTextItalic = false;
        ZoomFactorInfo = string.Empty;

        HistoryItems = new ObservableCollection<NOcrAddHistoryItem>();
        NOcrChar = new NOcrChar();
        _nOcrDb = new NOcrDb(string.Empty);
        CurrentBitmap = new SKBitmap(1, 1).ToAvaloniaBitmap();
        NOcrDrawingCanvas = new NOcrDrawingCanvasView();
        TextBoxNew = new TextBox();
    }


    public void Initialize(
        NOcrDb nOcrDb,
        NOcrAddHistoryManager nOcrAddHistoryManager)
    {
        _nOcrDb = nOcrDb;
        NOcrDrawingCanvas.ZoomFactor = 4;
        nOcrAddHistoryManager.ClearNotInOcrDb(nOcrDb);
        foreach (var historyItem in nOcrAddHistoryManager.Items)
        {
            HistoryItems.Add(historyItem);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }
    
    [RelayCommand]
    private void Abort()
    {
        Close();
    }
    
    [RelayCommand]
    private void ZoomIn()
    {
        if (NOcrDrawingCanvas.ZoomFactor < 10)
        {
            NOcrDrawingCanvas.ZoomFactor++;
        }

        ZoomFactorInfo = string.Format(Se.Language.Ocr.ZoomFactorX, NOcrDrawingCanvas.ZoomFactor);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (NOcrDrawingCanvas.ZoomFactor > 1)
        {
            NOcrDrawingCanvas.ZoomFactor--;
        }

        ZoomFactorInfo = string.Format(Se.Language.Ocr.ZoomFactorX, NOcrDrawingCanvas.ZoomFactor);
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }
    
    private void ShowOcrPoints()
    {
        NOcrDrawingCanvas.MissPaths.Clear();
        NOcrDrawingCanvas.HitPaths.Clear();

        NOcrDrawingCanvas.MissPaths.AddRange(NOcrChar.LinesBackground);
        NOcrDrawingCanvas.HitPaths.AddRange(NOcrChar.LinesForeground);
        NOcrDrawingCanvas.InvalidateVisual();
    }

    internal void TextBoxNewOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(TextBoxNew.Text))
        {
            Ok();
        }
        else if (e.Key == Key.Escape)
        {
            Abort();
        }
    }
    
    public void KeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Abort();
        }
    }
    
    public void ItalicCheckChanged(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBoxNew.FontStyle = IsNewTextItalic ? FontStyle.Italic : FontStyle.Normal;
        });
    }
}