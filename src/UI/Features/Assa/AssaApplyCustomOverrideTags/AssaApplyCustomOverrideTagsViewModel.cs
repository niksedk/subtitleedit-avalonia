using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    [ObservableProperty] private bool _adjustAll;
    [ObservableProperty] private bool _adjustSelectedLines;
    [ObservableProperty] private bool _adjustSelectedLinesAndForward;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public Subtitle UpdatedSubtitle { get; set; }
    public VideoPlayerControl VideoPlayerControl { get; set; }
    public ComboBox ComboBoxLeft { get; set; }
    public ComboBox ComboBoxRight { get; set; }

    private readonly IWindowService _windowService;
    private readonly string _tempSubtitleFileName;
    private readonly SubtitleFormat _assaFormat;
    private LibMpvDynamicPlayer? _mpvPlayer;
    private bool _isSubtitleLoaded;
    private string _oldSubtitleText;
    private string? _videoFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private List<SubtitleLineViewModel> _subtitleLines = new List<SubtitleLineViewModel>();

    public AssaApplyCustomOverrideTagsViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        OverrideTags = new ObservableCollection<OverrideTagDisplay>(OverrideTagDisplay.List());
        _videoFileName = string.Empty;
        VideoPlayerControl = new VideoPlayerControl(new VideoPlayerInstanceNone());
        ComboBoxLeft = new ComboBox();
        ComboBoxRight = new ComboBox();
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>();
        CurrentTag = string.Empty;
        _assaFormat = new AdvancedSubStationAlpha();
        _oldSubtitleText = string.Empty;
        AdjustAll = true;
        UpdatedSubtitle = new  Subtitle();

        // Toggle play/pause on surface click
        VideoPlayerControl.SurfacePointerPressed += (_, __) => VideoPlayerControl.TogglePlayPause();

        SelectedOverrideTag = OverrideTags.FirstOrDefault(p => p.Tag == Se.Settings.Assa.LastOverrideTag) ?? OverrideTags[0];

        _tempSubtitleFileName = System.IO.Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ass");
    }

    public void Initialize(
        List<SubtitleLineViewModel> paragraphs,
        List<SubtitleLineViewModel> selectedParagraphs,
        string? videoFileName)
    {
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>(paragraphs.Select(p => new SubtitleDisplayItem(p)));
        _videoFileName = videoFileName;
        _subtitleLines = paragraphs;

        if (selectedParagraphs.Count > 1)
        {
            AdjustSelectedLines = true;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                _ = VideoPlayerControl.Open(videoFileName);
            }

            StartTitleTimer();
        });
    }

    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _positionTimer.Tick += (s, e) =>
        {
            if (_mpvPlayer == null)
            {
                return;
            }

            var subtitle = new Subtitle();
            foreach (var item in Paragraphs)
            {
                var p = new SubtitleLineViewModel(item.Subtitle);
                p.Text = CurrentTag + p.Text;
                subtitle.Paragraphs.Add(p.ToParagraph());
            }

            var text = _assaFormat.ToText(subtitle, string.Empty);
            if (_oldSubtitleText == text)
            {
                return;
            }

            File.WriteAllText(_tempSubtitleFileName, text);
            if (!_isSubtitleLoaded)
            {
                _isSubtitleLoaded = true;
                _mpvPlayer.SubAdd(_tempSubtitleFileName);
            }
            else
            {
                _mpvPlayer.SubReload();
            }

            _oldSubtitleText = text;
            UpdatedSubtitle = subtitle;
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
        VideoPlayerControl.VideoPlayerInstance.CloseFile();
        try
        {
            if (File.Exists(_tempSubtitleFileName))
            {
                File.Delete(_tempSubtitleFileName);
            }
        }
        catch
        {
            // ignore
        }
        Se.Settings.Assa.LastOverrideTag = SelectedOverrideTag?.Tag ?? string.Empty;
    }

    internal async void OnLoaded()
    {
        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        // Wait a bit for video players to finish opening the file (or until they report a duration)
        await VideoPlayerControl.WaitForPlayersReadyAsync();

        Dispatcher.UIThread.Post(() =>
        {
            _mpvPlayer = VideoPlayerControl.VideoPlayerInstance as LibMpvDynamicPlayer;

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