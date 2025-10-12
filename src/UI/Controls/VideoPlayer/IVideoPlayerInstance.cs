using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Controls.VideoPlayer;

public interface IVideoPlayerInstance
{
    string Name { get; }
    string FileName { get; }

    Task Open(string fileName);
    void Close();

    void Play();
    void PlayOrPause();
    void Pause();
    void Stop();

    bool IsPlaying { get; }
    bool IsPaused { get; }

    double Position { get; set; }
    double Duration { get; }

    int VolumeMaximum { get; }
    double Volume { get; set; }

    double Speed { get; set; }
}
