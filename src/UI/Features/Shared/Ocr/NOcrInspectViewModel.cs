using Avalonia.Controls;
using Avalonia.Input;
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

public partial class NOcrInspectViewModel : ObservableObject
{
    public NOcrInspectWindow? Window { get; set; }

    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<NOcrLine> _linesForeground;
    [ObservableProperty] private NOcrLine? _selectedLineForeground;
    [ObservableProperty] private ObservableCollection<NOcrLine> _linesBackground;
    [ObservableProperty] private NOcrLine? _selectedLineBackground;
    [ObservableProperty] private ObservableCollection<NOcrDrawModeItem> _drawModes;
    [ObservableProperty] private NOcrDrawModeItem _selectedDrawMode;
    [ObservableProperty] private bool _isNewLinesForegroundActive;
    [ObservableProperty] private bool _isNewLinesBackgroundActive;
    [ObservableProperty] private string _newText;
    [ObservableProperty] private string _resolutionAndTopMargin;
    [ObservableProperty] private bool _isNewTextItalic;
    [ObservableProperty] private bool _submitOnFirstLetter;
    [ObservableProperty] private Bitmap? _sentenceImageSource;
    [ObservableProperty] private Bitmap? _itemImageSource;
    [ObservableProperty] private bool _canShrink;
    [ObservableProperty] private bool _canExpand;
    [ObservableProperty] private ObservableCollection<int> _noOfLinesToAutoDrawList;
    [ObservableProperty] private int _selectedNoOfLinesToAutoDraw;

    private List<ImageSplitterItem2> _letters;
    private ImageSplitterItem2 _splitItem;
    public Bitmap SentenceBitmap { get; set; }
    public Bitmap CurrentBitmap { get; set; }
    public NOcrChar NOcrChar { get; private set; }
    public NOcrDrawingCanvasView NOcrDrawingCanvas { get; set; }
    public TextBox TextBoxNew { get; set; }
    public bool OkPressed { get; set; }
    public bool AbortPressed { get; set; }
    public bool SkipPressed { get; set; }
    public bool UseOncePressed { get; set; }

    private List<OcrSubtitleItem> _ocrSubtitleItems;
    private int _startFromNumber;
    private int _maxWrongPixels;
    private NOcrDb _nOcrDb;

    public NOcrInspectViewModel()
    {
        Title = "Add new character";
        LinesForeground = new ObservableCollection<NOcrLine>();
        LinesBackground = new ObservableCollection<NOcrLine>();
        DrawModes = new ObservableCollection<NOcrDrawModeItem>(NOcrDrawModeItem.Items);
        SelectedDrawMode = DrawModes.First();
        IsNewLinesForegroundActive = true;
        IsNewLinesBackgroundActive = false;
        NewText = string.Empty;
        ResolutionAndTopMargin = string.Empty;
        IsNewTextItalic = false;
        SubmitOnFirstLetter = false;
        _letters = new List<ImageSplitterItem2>();

        const int maxLines = 500;
        NoOfLinesToAutoDrawList = new ObservableCollection<int>();
        for (var i = 0; i <= maxLines; i++)
        {
            NoOfLinesToAutoDrawList.Add(i);
        }

        SelectedNoOfLinesToAutoDraw = 100;
        NOcrChar = new NOcrChar();
        _ocrSubtitleItems = new List<OcrSubtitleItem>();
        _nOcrDb = new NOcrDb(string.Empty);
        SentenceBitmap = new SKBitmap(1, 1).ToAvaloniaBitmap();
        CurrentBitmap = new SKBitmap(1, 1).ToAvaloniaBitmap();
        _splitItem = new ImageSplitterItem2(string.Empty);
        NOcrDrawingCanvas = new NOcrDrawingCanvasView();
        TextBoxNew = new TextBox();
        LoadSettings();
    }

    private void LoadSettings()
    {
        IsNewTextItalic = Se.Settings.Ocr.IsNewLetterItalic;
        SubmitOnFirstLetter = Se.Settings.Ocr.SubmitOnFirstLetter;
    }

    private void SaveSettings()
    {
        Se.Settings.Ocr.IsNewLetterItalic = IsNewTextItalic;
        Se.Settings.Ocr.SubmitOnFirstLetter = SubmitOnFirstLetter;
        Se.SaveSettings();
    }

    internal void Initialize(OcrSubtitleItem? selectedOcrSubtitleItem, NOcrDb? nOcrDb, int selectedNOcrMaxWrongPixels)
    {

    }

    public void Initialize(
        OcrSubtitleItem item,
        List<ImageSplitterItem2> letters,
        int i,
        NOcrDb nOcrDb,
        int maxWrongPixels)
    {
        _nOcrDb = nOcrDb;
        _letters = letters;
        _startFromNumber = i;
        _maxWrongPixels = maxWrongPixels;

        if (i >= 0 && i < letters.Count)
        {
            _splitItem = letters[i];


            if (_splitItem.NikseBitmap != null)
            {
                NOcrChar = new NOcrChar
                {
                    Width = _splitItem.NikseBitmap.Width,
                    Height = _splitItem.NikseBitmap.Height,
                    MarginTop = _splitItem.Top,
                };

                ResolutionAndTopMargin = string.Format("{0}x{1}, margin top: {2}", NOcrChar.Width, NOcrChar.Height, NOcrChar.MarginTop);

                CurrentBitmap = _splitItem.NikseBitmap!.GetBitmap().ToAvaloniaBitmap();

                NOcrDrawingCanvas.BackgroundImage = CurrentBitmap;
                NOcrDrawingCanvas.ZoomFactor = 4;
                DrawAgain();
            }
        }

        InitSentenceBitmap(item);

        SetTitle();
    }

    private void InitSentenceBitmap(OcrSubtitleItem item)
    {
        var skBitmap = item.GetSkBitmap().Copy();

        if (_splitItem.NikseBitmap != null)
        {
            var rect = new SKRect(_splitItem.X, _splitItem.Y, _splitItem.X + _splitItem.NikseBitmap.Width, _splitItem.Y + _splitItem.NikseBitmap.Height);
            using (var canvas = new SKCanvas(skBitmap))
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Red,
                    StrokeWidth = 2, // Thickness of the rectangle border
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(rect, paint);
                }
            }
        }

        SentenceBitmap = skBitmap.ToAvaloniaBitmap();
    }

    [RelayCommand]
    private void Shrink()
    {

    }

    [RelayCommand]
    private void Expand()
    {

    }

    [RelayCommand]
    private void Ok()
    {
        NOcrChar.Text = NewText;
        OkPressed = true;
        SaveSettings();
        Close();
    }

    [RelayCommand]
    private void UseOnce()
    {
        UseOncePressed = true;
        Close();
    }

    [RelayCommand]
    private void Skip()
    {
        SkipPressed = true;
        Close();
    }

    [RelayCommand]
    private void Abort()
    {
        AbortPressed = true;
        Close();
    }

    [RelayCommand]
    private void DrawAgain()
    {
        NOcrChar.LinesForeground.Clear();
        NOcrChar.LinesBackground.Clear();
        NOcrChar.GenerateLineSegments(SelectedNoOfLinesToAutoDraw, false, NOcrChar, _splitItem.NikseBitmap!);
        ShowOcrPoints();
    }

    [RelayCommand]
    private void ClearDraw()
    {
        NOcrChar.LinesForeground.Clear();
        NOcrChar.LinesBackground.Clear();
        ShowOcrPoints();
    }

    [RelayCommand]
    private void ZoomIn()
    {
        if (NOcrDrawingCanvas.ZoomFactor < 10)
        {
            NOcrDrawingCanvas.ZoomFactor++;
        }
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (NOcrDrawingCanvas.ZoomFactor > 1)
        {
            NOcrDrawingCanvas.ZoomFactor--;
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    private void ShowOcrPoints()
    {
        NOcrDrawingCanvas.MissPaths.Clear();
        NOcrDrawingCanvas.HitPaths.Clear();

        NOcrDrawingCanvas.MissPaths.AddRange(NOcrChar.LinesBackground);
        NOcrDrawingCanvas.HitPaths.AddRange(NOcrChar.LinesForeground);
        NOcrDrawingCanvas.InvalidateVisual();
    }

    private void SetTitle()
    {
        Title = $"Add nOCR character for line  {_startFromNumber}, character {_letters.IndexOf(_splitItem) + 1} of {_letters.Count} using database \"{Path.GetFileNameWithoutExtension(_nOcrDb.FileName)}\"";
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

    internal void TextBoxNewOnKeyUp(object? sender, KeyEventArgs e)
    {
        if (SubmitOnFirstLetter && NewText.Length >= 1)
        {
            Ok();
        }
    }
}
