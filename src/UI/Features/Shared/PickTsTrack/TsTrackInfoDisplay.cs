using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;

namespace Nikse.SubtitleEdit.Features.Shared.PickTsTrack;

public class TsTrackInfoDisplay
{
    public int TrackNumber { get; set; }
    public bool IsDefault { get; set; }
    public bool IsForced { get; set; }
    public string Codec { get; set; }
    public string Language { get; set; }
    public string Name { get; set; }
    public MatroskaTrackInfo MatroskaTrackInfo { get; set; }

    public TsTrackInfoDisplay()
    {
        Codec = string.Empty;
        Language = string.Empty;
        Name = string.Empty;
        MatroskaTrackInfo = new MatroskaTrackInfo();
    }
}
