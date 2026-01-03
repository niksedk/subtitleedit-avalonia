using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

public partial class EmbeddedTrack : ObservableObject
{
    [ObservableProperty] private string _format;
    [ObservableProperty] private string _languageOrTitle;
    [ObservableProperty] private string _name;
    public bool Default { get; set; }
    [ObservableProperty] private bool _forced;
    [ObservableProperty] private bool _deleted;
    public bool New { get; set; }
    public string FileName { get; set; } = string.Empty;
    public FfmpegTrackInfo? FfmpegTrackInfo { get; set; }

    public EmbeddedTrack()
    {
        Format = string.Empty;
        LanguageOrTitle = string.Empty;
        Name = string.Empty;
    }

    public override string ToString()
    {
        return Name;
    }
}
