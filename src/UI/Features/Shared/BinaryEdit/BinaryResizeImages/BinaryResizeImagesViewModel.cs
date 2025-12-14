using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryResizeImages;

public partial class BinaryResizeImagesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ResizeModeDisplay> _resizeModes;
    [ObservableProperty] private ResizeModeDisplay _selectedResizeMode;
    [ObservableProperty] private int _percentage;
    [ObservableProperty] private int _fixedWidth;
    [ObservableProperty] private int _fixedHeight;
    [ObservableProperty] private bool _maintainAspectRatio;
    [ObservableProperty] private ObservableCollection<string> _filterQualities;
    [ObservableProperty] private string _selectedFilterQuality;
    [ObservableProperty] private Bitmap? _previewBitmap;
    [ObservableProperty] private bool _isPercentageVisible;
    [ObservableProperty] private bool _isFixedSizeVisible;

    public Window? Window { get; set; }
    public Image? PreviewImage { get; set; }
    public bool OkPressed { get; private set; }

    private List<BinarySubtitleItem> _subtitles = new();
    private int _originalWidth;
    private int _originalHeight;

    public BinaryResizeImagesViewModel()
    {
        _resizeModes = new ObservableCollection<ResizeModeDisplay>
        {
            new ResizeModeDisplay { Name = "Percentage", Mode = ResizeMode.Percentage },
            new ResizeModeDisplay { Name = "Fixed size", Mode = ResizeMode.FixedSize },
        };
        _selectedResizeMode = _resizeModes[0];
        _percentage = 100;
        _fixedWidth = 1920;
        _fixedHeight = 1080;
        _maintainAspectRatio = true;
        _filterQualities = new ObservableCollection<string> { "High", "Medium", "Low", "None" };
        _selectedFilterQuality = "High";
        UpdateVisibility();
    }

    public void Initialize(List<BinarySubtitleItem> subtitles)
    {
        _subtitles = subtitles;
        
        if (_subtitles.Count > 0 && _subtitles[0].Bitmap != null)
        {
            _originalWidth = (int)_subtitles[0].Bitmap.Size.Width;
            _originalHeight = (int)_subtitles[0].Bitmap.Size.Height;
            FixedWidth = _originalWidth;
            FixedHeight = _originalHeight;
            
            UpdatePreview();
        }
    }

    partial void OnSelectedResizeModeChanged(ResizeModeDisplay value)
    {
        UpdateVisibility();
        UpdatePreview();
    }

    partial void OnPercentageChanged(int value)
    {
        if (SelectedResizeMode.Mode == ResizeMode.Percentage)
        {
            UpdatePreview();
        }
    }

    partial void OnFixedWidthChanged(int value)
    {
        if (SelectedResizeMode.Mode == ResizeMode.FixedSize && MaintainAspectRatio && _originalWidth > 0)
        {
            var ratio = (double)value / _originalWidth;
            FixedHeight = (int)(_originalHeight * ratio);
        }
    }

    partial void OnFixedHeightChanged(int value)
    {
        if (SelectedResizeMode.Mode == ResizeMode.FixedSize && MaintainAspectRatio && _originalHeight > 0)
        {
            var ratio = (double)value / _originalHeight;
            FixedWidth = (int)(_originalWidth * ratio);
        }
    }

    partial void OnMaintainAspectRatioChanged(bool value)
    {
        if (value && _originalWidth > 0 && _originalHeight > 0)
        {
            var ratio = (double)FixedWidth / _originalWidth;
            FixedHeight = (int)(_originalHeight * ratio);
        }
    }

    private void UpdateVisibility()
    {
        IsPercentageVisible = SelectedResizeMode.Mode == ResizeMode.Percentage;
        IsFixedSizeVisible = SelectedResizeMode.Mode == ResizeMode.FixedSize;
    }

    [RelayCommand]
    private void UpdatePreview()
    {
        if (_subtitles.Count == 0 || _subtitles[0].Bitmap == null)
        {
            return;
        }

        var firstSubtitle = _subtitles[0];
        using var originalBitmap = firstSubtitle.Bitmap.ToSkBitmap();
        
        int newWidth, newHeight;
        if (SelectedResizeMode.Mode == ResizeMode.Percentage)
        {
            newWidth = (int)(originalBitmap.Width * Percentage / 100.0);
            newHeight = (int)(originalBitmap.Height * Percentage / 100.0);
        }
        else
        {
            newWidth = FixedWidth;
            newHeight = FixedHeight;
        }

        var resizedBitmap = ResizeBitmap(originalBitmap, newWidth, newHeight);
        PreviewBitmap = resizedBitmap.ToAvaloniaBitmap();
    }

    public void ApplyResize()
    {
        foreach (var subtitle in _subtitles)
        {
            if (subtitle.Bitmap == null)
            {
                continue;
            }

            using var originalBitmap = subtitle.Bitmap.ToSkBitmap();
            
            int newWidth, newHeight;
            if (SelectedResizeMode.Mode == ResizeMode.Percentage)
            {
                newWidth = (int)(originalBitmap.Width * Percentage / 100.0);
                newHeight = (int)(originalBitmap.Height * Percentage / 100.0);
            }
            else
            {
                newWidth = FixedWidth;
                newHeight = FixedHeight;
            }

            var resizedBitmap = ResizeBitmap(originalBitmap, newWidth, newHeight);
            subtitle.Bitmap = resizedBitmap.ToAvaloniaBitmap();
        }
    }

    private SKBitmap ResizeBitmap(SKBitmap originalBitmap, int width, int height)
    {
        var resizedBitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(resizedBitmap))
        {
            canvas.Clear(SKColors.Transparent);
            using (var paint = new SKPaint())
            {
                // Set filter quality based on selection
                paint.FilterQuality = SelectedFilterQuality switch
                {
                    "High" => SKFilterQuality.High,
                    "Medium" => SKFilterQuality.Medium,
                    "Low" => SKFilterQuality.Low,
                    _ => SKFilterQuality.None,
                };
                paint.IsAntialias = true;
                
                var destRect = new SKRect(0, 0, width, height);
                canvas.DrawBitmap(originalBitmap, destRect, paint);
            }
        }

        return resizedBitmap;
    }

    [RelayCommand]
    private async Task Ok()
    {
        var msg = GetValidationError();
        if (!string.IsNullOrEmpty(msg))
        {
            await MessageBox.Show(Window!, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ApplyResize();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private string GetValidationError()
    {
        if (Window == null)
        {
            return "Window is null";
        }

        if (SelectedResizeMode.Mode == ResizeMode.Percentage)
        {
            if (Percentage <= 0)
            {
                return "Percentage must be greater than 0";
            }
        }
        else if (SelectedResizeMode.Mode == ResizeMode.FixedSize)
        {
            if (FixedWidth <= 0)
            {
                return "Width must be greater than 0";
            }

            if (FixedHeight <= 0)
            {
                return "Height must be greater than 0";
            }
        }

        return string.Empty;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}

public class ResizeModeDisplay
{
    public string Name { get; set; } = string.Empty;
    public ResizeMode Mode { get; set; }

    public override string ToString() => Name;
}

public enum ResizeMode
{
    Percentage,
    FixedSize,
}
