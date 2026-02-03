using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;

public partial class AssaApplyCustomOverrideTagsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<OverrideTagDisplay> _overrideTags;
    [ObservableProperty] private OverrideTagDisplay? _selectedOverrideTag;
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private int _selectedParagraphLeftIndex = -1;
    [ObservableProperty] private int _selectedParagraphRightIndex = -1;
    [ObservableProperty] private bool _isAudioVisualizerVisible;
    [ObservableProperty] private string _currentTag;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl VideoPlayerControlLeft { get; set; }
    public ComboBox ComboBoxLeft { get; set; }
    public ComboBox ComboBoxRight { get; set; }

    private readonly IWindowService _windowService;

    private string? _videoFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private List<SubtitleLineViewModel> _subtitleLines = new List<SubtitleLineViewModel>();

    public AssaApplyCustomOverrideTagsViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        OverrideTags = new ObservableCollection<OverrideTagDisplay>(OverrideTagDisplay.List());
        _videoFileName = string.Empty;
        VideoPlayerControlLeft = new VideoPlayerControl(new VideoPlayerInstanceNone());
        ComboBoxLeft = new ComboBox();
        ComboBoxRight = new ComboBox();
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>();
        CurrentTag = string.Empty;

        // Toggle play/pause on surface click
        VideoPlayerControlLeft.SurfacePointerPressed += (_, __) => VideoPlayerControlLeft.TogglePlayPause();

        SelectedOverrideTag = OverrideTags.FirstOrDefault(p=> p.Tag == Se.Settings.Assa.LastOverrideTag) ?? OverrideTags[0];
    }

    public void Initialize(
        List<SubtitleLineViewModel> paragraphs,
        string? videoFileName,
        string? subtitleFileName,
        AudioVisualizer? audioVisualizer)
    {
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>(paragraphs.Select(p => new SubtitleDisplayItem(p)));
        _videoFileName = videoFileName;
        _subtitleLines = paragraphs;

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                _ = VideoPlayerControlLeft.Open(videoFileName);
            }

            StartTitleTimer();
        });
    }

    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _positionTimer.Tick += (s, e) =>
        {
            
        };
        _positionTimer.Start();
    }


    [RelayCommand]
    private void Add()
    {
        var tag = SelectedOverrideTag;
        if (tag == null)
        {
            return;
        }

        var newTag = CurrentTag + tag.Tag;
        newTag = newTag.Replace("}{", string.Empty);
        CurrentTag = newTag;
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
    
    private async Task PlayAndBack(VideoPlayerControl videoPlayer, int milliseconds)
    {
        var originalPosition = videoPlayer.Position;
        videoPlayer.VideoPlayerInstance.Play();
        await Task.Delay(milliseconds);
        videoPlayer.VideoPlayerInstance.Pause();
        videoPlayer.Position = originalPosition;
    }

    internal void OnClosing()
    {
        _positionTimer.Stop();
        VideoPlayerControlLeft.VideoPlayerInstance.CloseFile();
        Se.Settings.Assa.LastOverrideTag = SelectedOverrideTag?.Tag ?? string.Empty;
    }

    internal async void OnLoaded()
    {
        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        // Wait a bit for video players to finish opening the file (or until they report a duration)
        await VideoPlayerControlLeft.WaitForPlayersReadyAsync();

        Dispatcher.UIThread.Post(() =>
        {
            if (Paragraphs.Count == 0)
            {
                return;
            }

            SelectedParagraphLeftIndex = 0;
            SelectedParagraphRightIndex = Paragraphs.Count - 1;
        });
    }

    internal void OnKeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}