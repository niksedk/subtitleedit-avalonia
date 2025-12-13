using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

public partial class BinaryEditViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private BinarySubtitleItem? _selectedSubtitle;

    public IOcrSubtitle? OcrSubtitle { get; set; }
    
    public BinaryEditViewModel()
    {
    }

    public Window? Window { get; set; }
    public DataGrid? SubtitleGrid { get; set; }
    public VideoPlayerControl? VideoPlayerControl { get; set; }
    public Avalonia.Controls.Image? SubtitleOverlayImage { get; set; }
    public Avalonia.Media.TranslateTransform? SubtitleOverlayTransform { get; set; }
    public Avalonia.Media.ScaleTransform? SubtitleOverlayScaleTransform { get; set; }
    public bool OkPressed { get; private set; }
    public ObservableCollection<BinarySubtitleItem> Subtitles { get; set; }

    partial void OnSelectedSubtitleChanged(BinarySubtitleItem? value)
    {
        UpdateOverlayPosition();
    }

    public void UpdateOverlayPosition()
    {
        if (SubtitleOverlayTransform == null || SubtitleOverlayScaleTransform == null || 
            SelectedSubtitle == null || VideoPlayerControl == null)
        {
            return;
        }

        // Get video player bounds (total including controls)
        var videoPlayerWidth = VideoPlayerControl.Bounds.Width;
        var videoPlayerHeight = VideoPlayerControl.Bounds.Height;

        if (videoPlayerWidth <= 0 || videoPlayerHeight <= 0)
        {
            return;
        }

        // Account for video player controls at bottom (approximately 50-60 pixels)
        // The video content is in row 0 of the VideoPlayerControl's internal grid
        const double controlsHeight = 55; // Approximate height of controls panel
        var videoDisplayHeight = videoPlayerHeight - controlsHeight;
        
        if (videoDisplayHeight <= 0)
        {
            return;
        }

        // Get screen size from subtitle
        var screenWidth = SelectedSubtitle.ScreenSize.Width;
        var screenHeight = SelectedSubtitle.ScreenSize.Height;

        if (screenWidth <= 0 || screenHeight <= 0)
        {
            return;
        }

        // Calculate video aspect ratio and screen aspect ratio
        var videoAspect = videoPlayerWidth / videoDisplayHeight;
        var screenAspect = (double)screenWidth / screenHeight;

        // Calculate actual video display area (accounting for letterboxing/pillarboxing)
        double actualVideoWidth, actualVideoHeight, offsetX, offsetY;

        if (videoAspect > screenAspect)
        {
            // Video is wider than screen aspect - will have letterboxing (black bars top/bottom)
            actualVideoWidth = videoPlayerWidth;
            actualVideoHeight = videoPlayerWidth / screenAspect;
            offsetX = 0;
            offsetY = (videoDisplayHeight - actualVideoHeight) / 2;
        }
        else
        {
            // Video is taller than screen aspect - will have pillarboxing (black bars left/right)
            actualVideoHeight = videoDisplayHeight;
            actualVideoWidth = videoDisplayHeight * screenAspect;
            offsetX = (videoPlayerWidth - actualVideoWidth) / 2;
            offsetY = 0;
        }

        // Calculate scale factor
        var scaleX = actualVideoWidth / screenWidth;
        var scaleY = actualVideoHeight / screenHeight;

        // Update scale transform
        SubtitleOverlayScaleTransform.ScaleX = scaleX;
        SubtitleOverlayScaleTransform.ScaleY = scaleY;

        // Calculate and update position
        var x = offsetX + (SelectedSubtitle.X * scaleX);
        var y = offsetY + (SelectedSubtitle.Y * scaleY);

        SubtitleOverlayTransform.X = x;
        SubtitleOverlayTransform.Y = y;
    }

    IFileHelper _fileHelper;

    public BinaryEditViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        _fileName = string.Empty;
        Subtitles = new ObservableCollection<BinarySubtitleItem>();
    }

    public void Initialize(string fileName, IOcrSubtitle subtitle)
    {
        FileName = fileName;
        OcrSubtitle = subtitle;
        
        Subtitles.Clear();
        foreach (var s in subtitle.MakeOcrSubtitleItems())
        {
            Subtitles.Add(new BinarySubtitleItem(s));
        }
    }

    [RelayCommand]
    private void FileOpen()
    {
        // TODO: Implement file open
    }

    [RelayCommand]
    private void FileSave()
    {
        // TODO: Implement file save
    }

    [RelayCommand]
    private void ImportTimeCodes()
    {
        // TODO: Implement import time codes
    }

    [RelayCommand]
    private void AdjustDurations()
    {
        // TODO: Implement adjust durations
    }

    [RelayCommand]
    private void ApplyDurationLimits()
    {
        // TODO: Implement apply duration limits
    }

    [RelayCommand]
    private void Alignment()
    {
        // TODO: Implement alignment adjustment
    }

    [RelayCommand]
    private void ResizeImages()
    {
        // TODO: Implement resize images
    }

    [RelayCommand]
    private void AdjustBrightness()
    {
        // TODO: Implement adjust brightness
    }

    [RelayCommand]
    private void AdjustAlpha()
    {
        // TODO: Implement adjust alpha
    }

    [RelayCommand]
    private void AdjustAllTimes()
    {
        // TODO: Implement adjust all times
    }

    [RelayCommand]
    private void ChangeFrameRate()
    {
        // TODO: Implement change frame rate
    }

    [RelayCommand]
    private void ChangeSpeed()
    {
        // TODO: Implement change speed
    }

    [RelayCommand]
    private async Task OpenVideo()
    {
        if (Window == null)
        {
            return;
        }

        if (VideoPlayerControl == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(VideoPlayerControl.VideoPlayerInstance.FileName))
        {
            VideoPlayerControl.VideoPlayerInstance.CloseFile();
        }

        var videoFileName = await _fileHelper.PickOpenVideoFile(Window, Se.Language.General.OpenVideoFileTitle);
        if (string.IsNullOrEmpty(videoFileName))
        {
            return;
        }

        await VideoPlayerControl.Open(videoFileName);
    }

    [RelayCommand]
    private void Settings()
    {
        // TODO: Implement settings
    }

    [RelayCommand]
    private void ExportImage()
    {
        // TODO: Implement export image
    }

    [RelayCommand]
    private void ImportImage()
    {
        // TODO: Implement import image
    }

    [RelayCommand]
    private void SetText()
    {
        // TODO: Implement set text
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

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            Ok();
        }
    }

    public void Closing()
    {
        if (VideoPlayerControl == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(VideoPlayerControl.VideoPlayerInstance.FileName))
        {
            return;
        }

        VideoPlayerControl.VideoPlayerInstance.CloseFile();
    }
}