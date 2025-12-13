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
    [ObservableProperty] private string _selectedStartTime;
    [ObservableProperty] private string _selectedDuration;
    [ObservableProperty] private bool _selectedIsForced;
    [ObservableProperty] private BinarySubtitleItem? _selectedSubtitle;

    public BinaryEditViewModel()
    {
    }

    public Window? Window { get; set; }
    public DataGrid? SubtitleGrid { get; set; }
    public VideoPlayerControl? VideoPlayerControl { get; set; }
    public bool OkPressed { get; private set; }
    public ObservableCollection<BinarySubtitleItem> Subtitles { get; set; }

    IFileHelper _fileHelper;

    public BinaryEditViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        _fileName = string.Empty;
        _selectedStartTime = string.Empty;
        _selectedDuration = string.Empty;
        Subtitles = new ObservableCollection<BinarySubtitleItem>();
    }

    public void Initialize(string fileName, IOcrSubtitle subtitle)
    {
        FileName = fileName;
        
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
    private void QuickOcr()
    {
        // TODO: Implement quick OCR
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