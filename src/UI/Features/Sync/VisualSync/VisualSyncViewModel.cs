using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

public partial class VisualSyncViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private SubtitleDisplayItem? _selectedParagraphLeft;
    [ObservableProperty] private SubtitleDisplayItem? _selectedParagraphRight;
    [ObservableProperty] private bool _isAudioVisualizerVisible;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _videoInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl VideoPlayerControlLeft { get; set; }
    public VideoPlayerControl VideoPlayerControlRight { get; set; }
    public AudioVisualizer AudioVisualizerLeft { get; set; }
    public AudioVisualizer AudioVisualizerRight { get; set; }

    private string? _videoFileName;
    private string? _subtitleFileName;

    public VisualSyncViewModel()
    {
        Title = string.Empty;
        VideoInfo = string.Empty;
        _videoFileName = string.Empty;
        _subtitleFileName = string.Empty;
        VideoPlayerControlLeft = new VideoPlayerControl(new VideoPlayerInstanceNone());
        VideoPlayerControlRight = new VideoPlayerControl(new VideoPlayerInstanceNone());
        AudioVisualizerLeft = new AudioVisualizer();    
        AudioVisualizerRight = new AudioVisualizer();
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>();
    }

    public void Initialize(
        List<SubtitleLineViewModel> paragraphs, 
        string? videoFileName, 
        string? subtitleFileName,
        AudioVisualizer? audioVisualizer)
    {
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>(paragraphs.Select(p => new SubtitleDisplayItem(p)));
        _videoFileName = videoFileName;
        _subtitleFileName = subtitleFileName;
        
        SelectedParagraphLeft =  Paragraphs[0];
        SelectedParagraphRight = Paragraphs[^1];

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                VideoPlayerControlLeft.VideoPlayerInstance.Open(videoFileName);
                VideoPlayerControlRight.VideoPlayerInstance.Open(videoFileName);
            }

            if (audioVisualizer != null)
            {
                AudioVisualizerLeft.WavePeaks = audioVisualizer.WavePeaks;
                AudioVisualizerRight.WavePeaks = audioVisualizer.WavePeaks;
                IsAudioVisualizerVisible = true;
            }
        });
    }

    [RelayCommand]
    private void PlayTwoSecondsAndBackLeft()
    {
    }

    [RelayCommand]
    private void PlayTwoSecondsAndBackRight()
    {
    }

    [RelayCommand]
    private void GoToSubtitlePositionLeft()
    {
    }

    [RelayCommand]
    private void GoToSubtitlePositionRight()
    {
    }

    [RelayCommand]
    private void FindTextLeft()
    {
    }

    [RelayCommand]
    private void FindTextRight()
    {
    }

    [RelayCommand]                   
    private void Sync() 
    {
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}