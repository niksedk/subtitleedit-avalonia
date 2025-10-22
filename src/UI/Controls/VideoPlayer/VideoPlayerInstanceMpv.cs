using Avalonia;
using Avalonia.Data;
using Avalonia.Layout;
using HanumanInstitute.LibMpv;
using HanumanInstitute.LibMpv.Avalonia;
using HanumanInstitute.LibMpv.Core;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Controls.VideoPlayer;

public class VideoPlayerInstanceMpv : IVideoPlayerInstance
{
    public string Name
    {
        get
        {
            var mpvText = MpvContext?.MpvVersion.Get();

            if (string.IsNullOrEmpty(mpvText))
            {
                return "libmpv";
            }

            var arr = mpvText.Split('-');
            if (arr.Length > 1 && arr[0].Length > 5)
            {
                return arr[0];
            }

            return mpvText;
        }
    }

    private string _fileName = string.Empty;
    public string FileName => _fileName;

    public bool IsPlaying => !MpvContext?.Pause.Get() ?? false;

    public bool IsPaused => MpvContext?.Pause.Get() ?? true;

    public double Position
    {
        get => MpvContext?.TimePos.Get() ?? 0;
        set => MpvContext?.TimePos.Set(value);
    }

    public double Duration
    {
        get => MpvContext?.Duration.Get() ?? 0;
    }

    public int VolumeMaximum => MpvApi.MaxVolume;

    public double Volume
    {
        get => MpvContext?.Volume.Get() ?? 0;
        set
        {
            MpvContext?.Volume.Set(value);
        }
    }

    public double Speed
    {
        get => MpvContext?.Speed.Get() ?? 0;
        set
        {
            MpvContext?.Speed.Set(value);
        }
    }

    public MpvContext? MpvContext { get; set; }
    public MpvView MpvView { get; set; }

    public VideoPlayerInstanceMpv(VideoRenderer renderer)
    {
        MpvContext = new MpvContext();
        MpvView = new MpvView
        {
            Margin = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Renderer = renderer,  
        };

        MpvView.DataContext = this;
        MpvView.Bind(MpvView.MpvContextProperty, new Binding(nameof(MpvContext)));
    }

    public void Close()
    {
        MpvContext?.Stop().Invoke();
        _fileName = string.Empty;
    }

    public async Task Open(string fileName)
    {
        if (MpvContext == null)
        {
            return;
        }

        await MpvContext.Stop().InvokeAsync();
        await MpvContext.LoadFile(fileName).InvokeAsync();

        if (MpvContext != null)
        {
            MpvContext.SetOptionString("keep-open", "always"); // don't auto close video
            MpvContext.SetOptionString("sid","no"); // don't load subtitles
            MpvContext.SetOptionString("hr-seek","yes"); // don't load subtitles
        }

        _fileName = fileName;
    }

    public void Pause()
    {
        MpvContext?.Pause.Set(true);
    }

    public void Play()
    {
        MpvContext?.Pause.Set(false);
    }

    public void PlayOrPause()
    {
        if (MpvContext == null)
        {
            return;
        }

        if (IsPlaying)
        {
            MpvContext.Pause.Set(true);
        }
        else
        {
            MpvContext.Pause.Set(false);
        }
    }

    public void Stop()
    {
        MpvContext?.Pause.Set(true);
        MpvContext?.TimePos.Set(0);
    }

    /// <summary>
    /// Go to next audio track and return its language code or track number. After last track, goes back to first track.
    /// </summary>
    public string ToggleAudioTrack()
    {
        if (MpvContext == null)
        {
            return string.Empty;
        }

        try
        {
            var count = MpvContext.TrackListCount.Get();
            if (count <= 0)
            {
                return string.Empty;
            }

            var audioTracks = new List<(int listIndex, int id, string? lang, bool selected)>();
            for (int i = 0; i < count; i++)
            {
                var type = MpvContext.TrackListType[i].Get();
                if (string.Equals(type, "audio", System.StringComparison.OrdinalIgnoreCase))
                {
                    var id = MpvContext.TrackListId[i].Get() ?? -1;
                    if (id < 0)
                    {
                        continue;
                    }
                    var lang = MpvContext.TrackListLanguage[i].Get();
                    var selected = MpvContext.TrackListIsSelected[i].Get() ?? false;
                    audioTracks.Add((i, id, lang, selected));
                }
            }

            if (audioTracks.Count == 0)
            {
                return string.Empty;
            }

            var currentIdx = audioTracks.FindIndex(t => t.selected);
            var nextIdx = currentIdx >= 0 ? (currentIdx + 1) % audioTracks.Count : 0;
            var next = audioTracks[nextIdx];

            // Switch to the next audio track by ID
            MpvContext.AudioId.Set(next.id);

            // Prefer language code if available, otherwise return the track ID
            if (!string.IsNullOrWhiteSpace(next.lang))
            {
                return next.lang!;
            }

            return next.id.ToString(CultureInfo.InvariantCulture);
        }
        catch
        {
            return string.Empty;
        }
    }
}
