using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Shared.GetAudioClips;

public class AudioClip
{
    public string AudioFileName { get; set; } 
    public SubtitleLineViewModel Line { get; set; }

    public AudioClip(string audioFileName, SubtitleLineViewModel line)
    {
        AudioFileName = audioFileName;
        Line = line;
    }
}
