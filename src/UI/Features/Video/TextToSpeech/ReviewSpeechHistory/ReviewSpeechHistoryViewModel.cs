using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.LibMpv;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;

public partial class ReviewSpeechHistoryViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ReviewHistoryRow> _historyItems;
    [ObservableProperty] private ReviewHistoryRow? _selectedHistoryItem;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private MpvContext? _mpvContext;
    private Lock _playLock;
    private readonly System.Timers.Timer _timer;

    public ReviewSpeechHistoryViewModel()
    {
        HistoryItems = new ObservableCollection<ReviewHistoryRow>();
        LoadSettings();

        _playLock = new Lock();
        _timer = new System.Timers.Timer(200);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
    }

    private async Task PlayAudio(string fileName)
    {
        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();
            _mpvContext = new MpvContext();
        }
        await _mpvContext.LoadFile(fileName).InvokeAsync();
        _timer.Start();
    }

    private void LoadSettings()
    {
    }

    public void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        SaveSettings();
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task Play(ReviewHistoryRow? item)
    {
        if (item == null)
        {
            return;
        }

        item.IsPlaying = true;
        await PlayAudio(item.FileName);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(ReviewRow line)
    {
        foreach (var item in line.HistoryItems)
        {
            HistoryItems.Add(item);
        }

        if (HistoryItems.Count > 0)
        {
            SelectedHistoryItem = HistoryItems[0];
        }
    }
}