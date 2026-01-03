using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

public partial class EmbeddedTrack : ObservableObject
{
    public string Format { get; set; } = string.Empty;
    public string LanguageOrTitle { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Default { get; set; } = string.Empty;
    public string Forced { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public FfmpegTrackInfo? FfmpegTrackInfo { get; set; }

    public EmbeddedTrack()
    {
    }

    public override string ToString()
    {
        return Name;
    }
}
