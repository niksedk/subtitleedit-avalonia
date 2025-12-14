using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAllTimes;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryApplyDurationLimits;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

public partial class BinaryEditViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private BinarySubtitleItem? _selectedSubtitle;
    [ObservableProperty] private int _screenWidth;
    [ObservableProperty] private int _screenHeight;
    [ObservableProperty] private string _statusText;

    public IOcrSubtitle? OcrSubtitle { get; set; }

    public Window? Window { get; set; }
    public DataGrid? SubtitleGrid { get; set; }
    public VideoPlayerControl? VideoPlayerControl { get; set; }
    public Image? SubtitleOverlayImage { get; set; }
    public Avalonia.Media.TranslateTransform? SubtitleOverlayTransform { get; set; }
    public Avalonia.Media.ScaleTransform? SubtitleOverlayScaleTransform { get; set; }
    public bool OkPressed { get; private set; }
    public ObservableCollection<BinarySubtitleItem> Subtitles { get; set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    private string _loadFileName = string.Empty;

    public BinaryEditViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _fileHelper = fileHelper;
        _fileName = string.Empty;
        Subtitles = new ObservableCollection<BinarySubtitleItem>();
        StatusText = string.Empty;
    }

    public void Initialize(string fileName, IOcrSubtitle subtitle)
    {
        _loadFileName = fileName;
    }

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

    [RelayCommand]
    private async Task FileOpen()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.General.OpenSubtitleFileTitle, ".sup", "Blu-ray sup",  "All files", ".*");
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
        if (FileUtil.IsBluRaySup(fileName))
        {
            var subtitles = BluRaySupParser.ParseBluRaySup(fileName, new StringBuilder());
            if (subtitles.Count > 0)
            {
                imageSubtitle = new OcrSubtitleBluRay(subtitles);
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

        if (Subtitles.Count > 0)
        {
            SelectAndScrollToRow(0);
            ScreenWidth = Subtitles[0].ScreenSize.Width;
            ScreenHeight = Subtitles[0].ScreenSize.Height;
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
            ScreenWidth = ScreenWidth,
            ScreenHeight = ScreenHeight,
        };

        exportHandler.WriteHeader(fileName, imageParameter);
        for (var i = 0; i < Subtitles.Count; i++)
        {
            imageParameter.Bitmap = Subtitles[i].Bitmap!.ToSkBitmap();
            imageParameter.Text = Subtitles[i].Text;
            imageParameter.StartTime = Subtitles[i].StartTime;
            imageParameter.EndTime = Subtitles[i].EndTime;
            imageParameter.Index = i;

            exportHandler.CreateParagraph(imageParameter);
            exportHandler.WriteParagraph(imageParameter);
        }

        exportHandler.WriteFooter();
    }

    [RelayCommand]
    private async Task ImportTimeCodes()
    {
        if (Window == null)
        {
            return;
        }

        if (Subtitles.Count == 0)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.UnknownSubtitleFormat,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (subtitle.Paragraphs.Count != Subtitles.Count)
        {
            var message = "The time code import subtitle does not have the same number of lines as the current subtitle." + Environment.NewLine
                + "Imported lines: " + subtitle.Paragraphs.Count + Environment.NewLine
                + "Current lines: " + Subtitles.Count + Environment.NewLine
                + Environment.NewLine +
                "Do you want to continue anyway?";

            var answer = await MessageBox.Show(Window, Se.Language.General.Import, message, MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Error);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        for (var i = 0; i < Subtitles.Count && i < subtitle.Paragraphs.Count; i++)
        {
            Subtitles[i].StartTime = subtitle.Paragraphs[i].StartTime.TimeSpan;
            Subtitles[i].EndTime = subtitle.Paragraphs[i].EndTime.TimeSpan;
            Subtitles[i].Duration = Subtitles[i].EndTime - Subtitles[i].StartTime;
        }
    }

    [RelayCommand]
    private async Task AdjustDurations()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryAdjustDuration.BinaryAdjustDurationWindow, BinaryAdjustDuration.BinaryAdjustDurationViewModel>(Window, vm => { });

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

        result.AdjustDuration(Subtitles.ToList(), selectedIndices.Count > 0 ? selectedIndices : null);
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
    private async Task Alignment()
    {
        if (Window == null)
        {
            return;
        }

        // Get selected subtitles
        var selectedItems = new List<BinarySubtitleItem>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    selectedItems.Add(binaryItem);
                }
            }
        }

        // If no selection, nothing to do
        if (selectedItems.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Information, 
                "Please select one or more subtitles to adjust alignment.", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Show alignment picker
        var result = await _windowService.ShowDialogAsync<PickAlignment.PickAlignmentWindow, PickAlignment.PickAlignmentViewModel>(
            Window, vm => vm.Initialize(null, selectedItems.Count));

        if (!result.OkPressed || string.IsNullOrEmpty(result.Alignment))
        {
            return;
        }

        // Apply alignment to selected subtitles
        ApplyAlignmentToSubtitles(selectedItems, result.Alignment);
    }

    private void ApplyAlignmentToSubtitles(List<BinarySubtitleItem> subtitles, string alignment)
    {
        foreach (var subtitle in subtitles)
        {
            if (subtitle.Bitmap == null)
            {
                continue;
            }

            var screenWidth = subtitle.ScreenSize.Width;
            var screenHeight = subtitle.ScreenSize.Height;
            var imageWidth = (int)subtitle.Bitmap.Size.Width;
            var imageHeight = (int)subtitle.Bitmap.Size.Height;

            // Calculate new position based on alignment
            switch (alignment)
            {
                case "an1": // Bottom Left
                    subtitle.X = 0;
                    subtitle.Y = screenHeight - imageHeight;
                    break;
                case "an2": // Bottom Center
                    subtitle.X = (screenWidth - imageWidth) / 2;
                    subtitle.Y = screenHeight - imageHeight;
                    break;
                case "an3": // Bottom Right
                    subtitle.X = screenWidth - imageWidth;
                    subtitle.Y = screenHeight - imageHeight;
                    break;
                case "an4": // Middle Left
                    subtitle.X = 0;
                    subtitle.Y = (screenHeight - imageHeight) / 2;
                    break;
                case "an5": // Middle Center
                    subtitle.X = (screenWidth - imageWidth) / 2;
                    subtitle.Y = (screenHeight - imageHeight) / 2;
                    break;
                case "an6": // Middle Right
                    subtitle.X = screenWidth - imageWidth;
                    subtitle.Y = (screenHeight - imageHeight) / 2;
                    break;
                case "an7": // Top Left
                    subtitle.X = 0;
                    subtitle.Y = 0;
                    break;
                case "an8": // Top Center
                    subtitle.X = (screenWidth - imageWidth) / 2;
                    subtitle.Y = 0;
                    break;
                case "an9": // Top Right
                    subtitle.X = screenWidth - imageWidth;
                    subtitle.Y = 0;
                    break;
            }
        }

        // Update overlay position if the selected subtitle was adjusted
        UpdateOverlayPosition();
    }

    [RelayCommand]
    private async Task ResizeImages()
    {
        if (Window == null)
        {
            return;
        }

        // Get selected subtitles
        var selectedItems = new List<BinarySubtitleItem>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    selectedItems.Add(binaryItem);
                }
            }
        }

        // If no selection, work on all subtitles
        var itemsToResize = selectedItems.Count > 0 ? selectedItems : Subtitles.ToList();

        if (itemsToResize.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Information, 
                "No subtitles to resize.", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryResizeImages.BinaryResizeImagesWindow, BinaryResizeImages.BinaryResizeImagesViewModel>(
            Window, vm => vm.Initialize(itemsToResize));

        if (!result.OkPressed)
        {
            return;
        }

        // Refresh grid to show updated bitmaps
        if (SubtitleGrid != null)
        {
            var currentIndex = SubtitleGrid.SelectedIndex;
            SubtitleGrid.ItemsSource = null;
            SubtitleGrid.ItemsSource = Subtitles;
            SubtitleGrid.SelectedIndex = currentIndex;
        }

        UpdateOverlayPosition();
    }

    [RelayCommand]
    private async Task AdjustBrightness()
    {
        if (Window == null)
        {
            return;
        }

        // Get selected subtitles
        var selectedItems = new List<BinarySubtitleItem>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    selectedItems.Add(binaryItem);
                }
            }
        }

        // If no selection, work on all subtitles
        var itemsToAdjust = selectedItems.Count > 0 ? selectedItems : Subtitles.ToList();

        if (itemsToAdjust.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Information, 
                "No subtitles to adjust.", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryAdjustBrightness.BinaryAdjustBrightnessWindow, BinaryAdjustBrightness.BinaryAdjustBrightnessViewModel>(
            Window, vm => vm.Initialize(itemsToAdjust));

        if (!result.OkPressed)
        {
            return;
        }

        // Refresh grid to show updated bitmaps
        if (SubtitleGrid != null)
        {
            var currentIndex = SubtitleGrid.SelectedIndex;
            SubtitleGrid.ItemsSource = null;
            SubtitleGrid.ItemsSource = Subtitles;
            SubtitleGrid.SelectedIndex = currentIndex;
        }

        UpdateOverlayPosition();
    }

    [RelayCommand]
    private async Task AdjustAlpha()
    {
        if (Window == null)
        {
            return;
        }

        // Get selected subtitles
        var selectedItems = new List<BinarySubtitleItem>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    selectedItems.Add(binaryItem);
                }
            }
        }

        // If no selection, work on all subtitles
        var itemsToAdjust = selectedItems.Count > 0 ? selectedItems : Subtitles.ToList();

        if (itemsToAdjust.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Information, 
                "No subtitles to adjust.", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryAdjustAlpha.BinaryAdjustAlphaWindow, BinaryAdjustAlpha.BinaryAdjustAlphaViewModel>(
            Window, vm => vm.Initialize(itemsToAdjust));

        if (!result.OkPressed)
        {
            return;
        }

        // Refresh grid to show updated bitmaps
        if (SubtitleGrid != null)
        {
            var currentIndex = SubtitleGrid.SelectedIndex;
            SubtitleGrid.ItemsSource = null;
            SubtitleGrid.ItemsSource = Subtitles;
            SubtitleGrid.SelectedIndex = currentIndex;
        }

        UpdateOverlayPosition();
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
    private async Task ChangeFrameRate()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ChangeFrameRateWindow, ChangeFrameRateViewModel>(Window, vm => { });

        if (!result.OkPressed)
        {
            return;
        }

        var ratio = result.SelectedToFrameRate / result.SelectedFromFrameRate;

        // If there are selected items in the grid, apply only to them
        var appliedToSelected = false;
        if (SubtitleGrid?.SelectedItems != null && SubtitleGrid.SelectedItems.Count > 0)
        {
            var selectedIndices = new List<int>();
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

            if (selectedIndices.Count > 0)
            {
                foreach (var idx in selectedIndices)
                {
                    var s = Subtitles[idx];
                    s.StartTime = TimeSpan.FromMilliseconds(s.StartTime.TotalMilliseconds * ratio);
                    s.EndTime = TimeSpan.FromMilliseconds(s.EndTime.TotalMilliseconds * ratio);
                }

                appliedToSelected = true;
            }
        }

        if (!appliedToSelected)
        {
            // Apply to all subtitles
            foreach (var s in Subtitles)
            {
                s.StartTime = TimeSpan.FromMilliseconds(s.StartTime.TotalMilliseconds * ratio);
                s.EndTime = TimeSpan.FromMilliseconds(s.EndTime.TotalMilliseconds * ratio);
            }
        }
    }

    [RelayCommand]
    private async Task ChangeSpeed()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ChangeSpeedWindow, ChangeSpeedViewModel>(Window, vm => { });

        if (!result.OkPressed)
        {
            return;
        }

        // result.SpeedPercent is percentage; factor is 100 / percent as used elsewhere
        var factor = 100.0 / result.SpeedPercent;

        var appliedToSelected = false;
        if (SubtitleGrid?.SelectedItems != null && SubtitleGrid.SelectedItems.Count > 0)
        {
            var selectedIndices = new List<int>();
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

            if (selectedIndices.Count > 0)
            {
                foreach (var idx in selectedIndices)
                {
                    var s = Subtitles[idx];
                    s.StartTime = TimeSpan.FromMilliseconds(s.StartTime.TotalMilliseconds * factor);
                    s.EndTime = TimeSpan.FromMilliseconds(s.EndTime.TotalMilliseconds * factor);
                }

                appliedToSelected = true;
            }
        }

        if (!appliedToSelected)
        {
            foreach (var s in Subtitles)
            {
                s.StartTime = TimeSpan.FromMilliseconds(s.StartTime.TotalMilliseconds * factor);
                s.EndTime = TimeSpan.FromMilliseconds(s.EndTime.TotalMilliseconds * factor);
            }
        }
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

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await DoFileOpen(_loadFileName);
        });
    }

    private void SelectAndScrollToRow(int index)
    {
        if (index < 0 || SubtitleGrid == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (SubtitleGrid.SelectedIndex != index)
            {
                SubtitleGrid.SelectedIndex = index;
            }

            SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);
        });
    }
}