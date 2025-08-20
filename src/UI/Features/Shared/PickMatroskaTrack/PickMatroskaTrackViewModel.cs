using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;

public partial class PickMatroskaTrackViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MatroskaTrackInfoDisplay> _tracks;
    [ObservableProperty] private MatroskaTrackInfoDisplay? _selectedTrack;
    [ObservableProperty] private ObservableCollection<MatroskaSubtitleCueDisplay> _rows;

    public Window? Window { get; set; }
    public DataGrid TracksGrid { get; set; }
    public MatroskaTrackInfo? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    private List<MatroskaTrackInfo> _matroskaTracks;
    private MatroskaFile? _matroskaFile;

    public PickMatroskaTrackViewModel()
    {
        Tracks = new ObservableCollection<MatroskaTrackInfoDisplay>();
        TracksGrid = new DataGrid();
        WindowTitle = string.Empty;
        Rows = new ObservableCollection<MatroskaSubtitleCueDisplay>();
        _matroskaTracks = new List<MatroskaTrackInfo>();
    }

    public void Initialize(MatroskaFile matroskaFile, List<MatroskaTrackInfo> matroskaTracks, string fileName)
    {
        _matroskaFile = matroskaFile;
        _matroskaTracks = matroskaTracks;
        WindowTitle = $"Pick Matroska track - {fileName}";
        foreach (var track in _matroskaTracks)
        {
            var display = new MatroskaTrackInfoDisplay
            {
                TrackNumber = track.TrackNumber,
                IsDefault = track.IsDefault,
                IsForced = track.IsForced,
                Codec = track.CodecId,
                Language = track.Language,
                Name = track.Name,
                MatroskaTrackInfo = track,
            };
            Tracks.Add(display);
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    [RelayCommand]
    private void Export()
    {
    }

    [RelayCommand]
    private void Ok()
    {
        SelectedMatroskaTrack = SelectedTrack?.MatroskaTrackInfo;
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
        }
    }

    internal void DataGridTracksSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        bool flowControl = TrackChanged();
        if (!flowControl)
        {
            return;
        }
    }

    private bool TrackChanged()
    {
        var selectedTrack = SelectedTrack;
        if (selectedTrack == null || selectedTrack.MatroskaTrackInfo == null)
        {
            return false;
        }

        Rows.Clear();
        var trackinfo = selectedTrack.MatroskaTrackInfo!;
        var subtitles = _matroskaFile?.GetSubtitle(trackinfo.TrackNumber, null);
        if (trackinfo.CodecId == MatroskaTrackType.SubRip && subtitles != null)
        {
            AddTextContent(trackinfo, subtitles, new SubRip());
        }
        else if (trackinfo.CodecId is MatroskaTrackType.SubStationAlpha or MatroskaTrackType.SubStationAlpha2 && subtitles != null)
        {
            AddTextContent(trackinfo, subtitles, new SubStationAlpha());
        }
        else if (trackinfo.CodecId is MatroskaTrackType.AdvancedSubStationAlpha or MatroskaTrackType.AdvancedSubStationAlpha2 && subtitles != null)
        {
            AddTextContent(trackinfo, subtitles, new AdvancedSubStationAlpha());
        }
        else if (trackinfo.CodecId == MatroskaTrackType.BluRay && subtitles != null && _matroskaFile != null)
        {
            var pcsData = BluRaySupParser.ParseBluRaySupFromMatroska(trackinfo, _matroskaFile);
            for (var i = 0; i < 20 && i < pcsData.Count; i++)
            {
                var item = pcsData[i];
                var bitmap = item.GetBitmap();
                var cue = new MatroskaSubtitleCueDisplay()
                {
                    Number = i + 1,
                    Show = TimeSpan.FromMilliseconds(item.StartTime),
                    Hide = TimeSpan.FromMilliseconds(item.EndTime),
                    Duration = TimeSpan.FromMilliseconds(item.EndTime - item.StartTime),
                    Image = new Image { Source = bitmap.ToAvaloniaBitmap() },
                };
                Rows.Add(cue);
            }
        }
        else if (trackinfo.CodecId == MatroskaTrackType.TextSt && subtitles != null && _matroskaFile != null)
        {
            var subtitle = new Subtitle();
            var sub = _matroskaFile.GetSubtitle(trackinfo.TrackNumber, null);
            Utilities.LoadMatroskaTextSubtitle(trackinfo, _matroskaFile, sub, subtitle);
            Utilities.ParseMatroskaTextSt(trackinfo, sub, subtitle);

            for (var i = 0; i < 20 && i < subtitle.Paragraphs.Count; i++)
            {
                var item = subtitle.Paragraphs[i];
                var cue = new MatroskaSubtitleCueDisplay()
                {
                    Number = i + 1,
                    Show = item.StartTime.TimeSpan,
                    Hide = item.EndTime.TimeSpan,
                    Duration = TimeSpan.FromMilliseconds(item.EndTime.TotalMilliseconds - item.StartTime.TotalMilliseconds),
                    Text = item.Text,
                };
                Rows.Add(cue);
            }
        }

        return true;
    }

    private void AddTextContent(MatroskaTrackInfo trackInfo, List<MatroskaSubtitle> subtitles, SubtitleFormat format)
    {
        var sub = new Subtitle();
        Utilities.LoadMatroskaTextSubtitle(trackInfo, _matroskaFile, subtitles, sub);
        var raw = format.ToText(sub, string.Empty);
        for (var i = 0; i < sub.Paragraphs.Count; i++)
        {
            var p = sub.Paragraphs[i];
            var cue = new MatroskaSubtitleCueDisplay()
            {
                Number = p.Number,
                Text = p.Text,
                Show = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds),
                Hide = TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds),
                Duration = TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds),
            };
            Rows.Add(cue);
        }
    }

    internal void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Tracks.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            TracksGrid.SelectedIndex = index;
            TracksGrid.ScrollIntoView(TracksGrid.SelectedItem, null);
            TrackChanged();
        }, DispatcherPriority.Background);
    }
}