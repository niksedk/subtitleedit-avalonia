using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

public class ReviewHistoryRow
{
    public int Number { get; set; }
    public string FileName { get; set; }
    public string VoiceName { get; set; }
    public Voice? Voice { get; set; }

    public ReviewHistoryRow()
    {
        FileName = string.Empty;
        VoiceName = string.Empty;
    }
}
