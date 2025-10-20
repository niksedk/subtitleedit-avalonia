namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

public class ReviewHistoryRow
{
    public int Number { get; set; }
    public string FileName { get; set; }
    public string Voice { get; set; }

    public ReviewHistoryRow()
    {
        FileName = string.Empty;
        Voice = string.Empty;
    }
}
