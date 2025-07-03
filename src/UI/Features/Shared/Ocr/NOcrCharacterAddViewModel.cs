using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public partial class NOcrCharacterAddViewModel : ObservableObject
{
    public NOcrCharacterAddWindow? Window { get; set; }

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
    private SKBitmap _sentenceBitmap;
    public NOcrChar NOcrChar { get; private set; }
    public NOcrDrawingCanvasView NOcrDrawingCanvas { get; set; }
    public TextBox EntryNewText { get; set; }
    public bool OkPressed { get; set; }

    private List<OcrSubtitleItem> _ocrSubtitleItems;
    private int _startFromNumber;
    private int _maxWrongPixels;
    private NOcrDb _nOcrDb;

    public NOcrCharacterAddViewModel()
    {
        Title = "Add new character";
        LinesForeground = new ObservableCollection<NOcrLine>();
        LinesBackground = new ObservableCollection<NOcrLine>();
        DrawModes = new  ObservableCollection<NOcrDrawModeItem>(NOcrDrawModeItem.Items);
        SelectedDrawMode = DrawModes.First();
        IsNewLinesForegroundActive = true;
        IsNewLinesBackgroundActive = false;
        NewText = string.Empty;
        ResolutionAndTopMargin = string.Empty;
        IsNewTextItalic = false;
        SubmitOnFirstLetter = true;
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
        _sentenceBitmap = new SKBitmap(1, 1);
        _splitItem = new ImageSplitterItem2(string.Empty);
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

    }

    [RelayCommand]
    private void UseOnce()
    {

    }

    [RelayCommand]
    private void Skip()
    {

    }

    [RelayCommand]
    private void Abort()
    {

    }

    [RelayCommand]
    private void DrawAgain()
    {

    }

    [RelayCommand]
    private void ClearDraw()
    {

    }
}
