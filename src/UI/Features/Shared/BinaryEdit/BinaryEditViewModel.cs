using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAllTimes;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryApplyDurationLimits;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

public partial class BinaryEditViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private BinarySubtitleItem? _selectedSubtitle;

    public IOcrSubtitle? OcrSubtitle { get; set; }

    private readonly IWindowService _windowService;
    private string _loadFileName = string.Empty;

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

    private readonly IFileHelper _fileHelper;

    public BinaryEditViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _fileHelper = fileHelper;
        _fileName = string.Empty;
        Subtitles = new ObservableCollection<BinarySubtitleItem>();
    }

    public void Initialize(string fileName, IOcrSubtitle subtitle)
    {
        _loadFileName = fileName;
    }

    [RelayCommand]
    private async Task FileOpen()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".sup", "export.sup", Se.Language.General.SaveFileAsTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }
        
        await DoFileOpen(fileName);
    }

    private async Task DoFileOpen(string fileName)
    {
        if (Window == null)
        {
            return;
        }
        
        IOcrSubtitle? imageSubtitle = null;
        if (FileUtil.IsBluRaySup((fileName)))
        {
            var subtitles = BluRaySupParser.ParseBluRaySup(fileName, new StringBuilder());
            if (subtitles.Count > 0)
            { 
                imageSubtitle = new  OcrSubtitleBluRay(subtitles); 
            }
        }
        
        //TODO: other image based formats
        
        if (imageSubtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, "Image based subtitle format not found/supported.",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        
        FileName = fileName;
        OcrSubtitle = imageSubtitle;

        Subtitles.Clear();
        foreach (var s in imageSubtitle.MakeOcrSubtitleItems())
        {
            Subtitles.Add(new BinarySubtitleItem(s));
        }
    }

    [RelayCommand]
    private async Task FileSave()
    {
        if (Window == null)
        {
            return;
        }

        if (Subtitles.Count == 0)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".sup", "export.sup", Se.Language.General.SaveFileAsTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var targetFormat = BatchConverter.FormatBluRaySup;
        IExportHandler? exportHandler = null;
        string extension = string.Empty;
        if (targetFormat == BatchConverter.FormatBluRaySup)
        {
            exportHandler = new ExportHandlerBluRaySup();
            extension = ".sup";
        }

        if (targetFormat == BatchConverter.FormatBdnXml)
        {
            exportHandler = new ExportHandlerBdnXml();
            extension = string.Empty; // folder
        }

        if (targetFormat == BatchConverter.FormatVobSub)
        {
            exportHandler = new ExportHandlerVobSub();
            extension = ".sub";
        }

        if (targetFormat == BatchConverter.FormatImagesWithTimeCodesInFileName)
        {
            exportHandler = new ExportHandlerImagesWithTimeCode();
            extension = string.Empty; // folder
        }

        if (targetFormat == BatchConverter.FormatDostImage)
        {
            exportHandler = new ExportHandlerDost();
            extension = string.Empty; // folder
        }

        if (targetFormat == BatchConverter.FormatFcpImage)
        {
            exportHandler = new ExportHandlerFcp();
            extension = string.Empty; // folder
        }

        if (targetFormat == BatchConverter.FormatDCinemaInterop)
        {
            exportHandler = new ExportHandlerDCinemaInteropPng();
            extension = string.Empty; // folder
        }

        if (targetFormat == BatchConverter.FormatDCinemaSmpte2014)
        {
            exportHandler = new ExportHandlerDCinemaSmpte2014Png();
            extension = string.Empty; // folder
        }

        if (exportHandler == null)
        {
            var message = Se.Language.General.UnknownSubtitleFormat;
            return;
        }

        var imageParameter = new ImageParameter()
        {
            ScreenWidth = Subtitles.First().ScreenSize.Width,
            ScreenHeight = Subtitles.First().ScreenSize.Height,
        };

        exportHandler.WriteHeader(fileName, imageParameter);
        for (var i = 0; i < Subtitles.Count; i++)
        {
            exportHandler.CreateParagraph(imageParameter);
            exportHandler.WriteParagraph(imageParameter);
        }

        exportHandler.WriteFooter();
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
    private async Task ApplyDurationLimits()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryApplyDurationLimitsWindow, BinaryApplyDurationLimitsViewModel>(Window, vm => { });

        if (!result.OkPressed)
        {
            return;
        }

        // Get selected indices from the grid
        var selectedIndices = new List<int>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    var index = Subtitles.IndexOf(binaryItem);
                    if (index >= 0)
                    {
                        selectedIndices.Add(index);
                    }
                }
            }
        }

        result.ApplyLimits(Subtitles.ToList(), selectedIndices.Count > 0 ? selectedIndices : null);
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
    private async Task AdjustAllTimes()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryAdjustAllTimesWindow, BinaryAdjustAllTimesViewModel>(Window, vm => { });

        if (!result.OkPressed)
        {
            return;
        }

        // Get selected indices from the grid
        var selectedIndices = new List<int>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    var index = Subtitles.IndexOf(binaryItem);
                    if (index >= 0)
                    {
                        selectedIndices.Add(index);
                    }
                }
            }
        }

        result.AdjustTimes(Subtitles.ToList(), selectedIndices.Count > 0 ? selectedIndices : null);
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

    public void Loaded()
    {
        if (string.IsNullOrEmpty(_loadFileName))
        {
            return;
        }

        Dispatcher.UIThread.InvokeAsync(async() =>
        {
            await DoFileOpen(_loadFileName);
        });
    }
}