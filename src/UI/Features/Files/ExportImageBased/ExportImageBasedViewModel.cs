using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ExportImageBasedViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<string> _fontFamilies;
    [ObservableProperty] string? _selectedFontFamily;
    [ObservableProperty] private ObservableCollection<int> _fontSizes;
    [ObservableProperty] int? _selectedFontSize;
    [ObservableProperty] private ObservableCollection<ResolutionItem> _resolutions;
    [ObservableProperty] ResolutionItem? _selectedResolution;
    [ObservableProperty] private ObservableCollection<int> _topBottomMargins;
    [ObservableProperty] int? _selectedTopBottomMargin;
    [ObservableProperty] private ObservableCollection<int> _leftRightMargins;
    [ObservableProperty] int? _selectedLeftRightMargin;
    [ObservableProperty] private ObservableCollection<string> _borderStyles;
    [ObservableProperty] string? _selectedBorderStyle;
    [ObservableProperty] private ObservableCollection<int> _shadowWidths;
    [ObservableProperty] int? _selectedShadowWidth;
    [ObservableProperty] private bool _isBold;
    [ObservableProperty] private Color _fontColor;
    [ObservableProperty] private Color _borderColor;
    [ObservableProperty] private Color _shadowColor;
    [ObservableProperty] private TimeSpan _startOfProgramme;
    [ObservableProperty] private Bitmap _bitmapPreview;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid SubtitleGrid { get; set; }

    private string _subtitleFileName;
    private string _videoFileName;
    private bool _dirty;
    private readonly Timer _timerUpdatePreview;

    public ExportImageBasedViewModel()
    {
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        FontFamilies = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        SelectedFontFamily = FontFamilies.FirstOrDefault();
        FontSizes = new ObservableCollection<int>(Enumerable.Range(15, 486));
        SelectedFontSize = 20;
        Resolutions = new ObservableCollection<ResolutionItem>(ResolutionItem.GetResolutions());
        SelectedResolution = Resolutions.FirstOrDefault(r => r.Width == 1920 && r.Height == 1080);
        TopBottomMargins = new ObservableCollection<int> { 0, 5, 10, 15, 20, 25, 30 };
        SelectedTopBottomMargin = 10;
        LeftRightMargins = new ObservableCollection<int> { 0, 5, 10, 15, 20, 25, 30 };
        SelectedLeftRightMargin = 10;
        BorderStyles = new ObservableCollection<string> { "None", "Solid", "Dashed", "Dotted" };
        SelectedBorderStyle = BorderStyles.FirstOrDefault();
        ShadowWidths = new ObservableCollection<int> { 0, 1, 2, 3, 4, 5 };
        SelectedShadowWidth = 3;
        FontColor = Colors.White;
        BorderColor = Colors.Black;
        ShadowColor = Colors.Black;
        SubtitleGrid = new DataGrid();
        Title = string.Empty;
        BitmapPreview = new SKBitmap(1, 1).ToAvaloniaBitmap();

        _subtitleFileName = string.Empty;
        _videoFileName = string.Empty;

        _timerUpdatePreview = new Timer();
        _timerUpdatePreview.Interval = 250;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
        _timerUpdatePreview.Start();
    }

    private void TimerUpdatePreviewElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_dirty)
        {
            _dirty = false;
            Dispatcher.UIThread.Post(() =>
            {
                SubtitleLineChanged();
            });
        }
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
            Title = Se.Language.File.Export.TitleExportBluRaySup;
        }

        SelectedSubtitle = Subtitles.FirstOrDefault();
        Dispatcher.UIThread.Invoke(() =>
        {
        });
    }

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SubtitleLineChanged();
    }

    private void SubtitleLineChanged()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var text = selected.Text;
        if (string.IsNullOrEmpty(text))
        {
            BitmapPreview = new SKBitmap(1, 1).ToAvaloniaBitmap();
            return;
        }

        var fontName = SelectedFontFamily ?? "Arial";
        var fontSize = SelectedFontSize ?? 20;

        // Create font and paint objects
        using var typeface = SKTypeface.FromFamilyName(fontName, IsBold ? SKFontStyle.Bold : SKFontStyle.Normal);
        using var font = new SKFont(typeface, fontSize);
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        // Measure the text to determine bitmap size
        var textWidth = font.MeasureText(text);
        var fontMetrics = font.Metrics;
        var textHeight = fontMetrics.Descent - fontMetrics.Ascent;

        // Calculate bitmap dimensions with some padding
        var padding = 10;
        var width = (int)Math.Ceiling(textWidth) + padding * 2;
        var height = (int)Math.Ceiling(textHeight) + padding * 2;

        // Ensure minimum size
        width = Math.Max(width, 1);
        height = Math.Max(height, 1);

        // Create bitmap and canvas
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Clear background (optional - white background)
        canvas.Clear(SKColors.White);

        // Calculate text position (centered with padding)
        var x = padding;
        var y = padding - fontMetrics.Ascent; // Ascent is negative, so we subtract it

        // Draw the text
        canvas.DrawText(text, x, y, font, paint);

        // Convert to Avalonia bitmap
        BitmapPreview = bitmap.ToAvaloniaBitmap();
    }

    internal void ComboChanged(object? sender, SelectionChangedEventArgs e)
    {
        _dirty = true;
    }

    internal void CheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        _dirty = true;
    }
}