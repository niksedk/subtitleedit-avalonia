using Avalonia;
using Avalonia.Data;
using Avalonia.Layout;
using HanumanInstitute.LibMpv.Avalonia;
using HanumanInstitute.LibMpv.Core;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Controls.VideoPlayer;

public class VideoPlayerInstanceMpv : IVideoPlayerInstance
{
    public string Name => "libmpv";

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

    public HanumanInstitute.LibMpv.MpvContext? MpvContext { get; set; }
    public MpvView MpvView { get; set; }

    public VideoPlayerInstanceMpv()
    {
        MpvContext = new HanumanInstitute.LibMpv.MpvContext();
        MpvView = new MpvView
        {
            Margin = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        MpvView.DataContext = this;
        MpvView.Bind(MpvView.MpvContextProperty, new Binding(nameof(MpvContext)));
    }

    public void Close()
    {
        _fileName = string.Empty;
    }

    public async Task Open(string fileName)
    {
        if (MpvContext == null)
        {
            return;
        }

        MpvContext.Stop();
        await MpvContext.LoadFile(fileName).InvokeAsync();
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
}
