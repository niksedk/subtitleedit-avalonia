using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;

public partial class BinaryEditViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _selectedStartTime;
    [ObservableProperty] private string _selectedDuration;
    [ObservableProperty] private bool _selectedIsForced;
    
    public Window? Window { get; set; }
    public DataGrid? SubtitleGrid { get; set; }
    public bool OkPressed { get; private set; }
    public ObservableCollection<BinarySubtitleItem> Subtitles { get; set; }

    public BinaryEditViewModel()
    {
        _fileName = string.Empty;
        _selectedStartTime = string.Empty;
        _selectedDuration = string.Empty;
        Subtitles = new ObservableCollection<BinarySubtitleItem>();
    }

    public void Initialize(string fileName)
    {
        FileName = fileName;
        // TODO: Load binary subtitle file and populate Subtitles collection
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
    private void OpenVideo()
    {
        // TODO: Implement open video
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
}

public class BinarySubtitleItem
{
    public int Number { get; set; }
    public bool IsForced { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}