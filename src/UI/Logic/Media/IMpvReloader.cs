using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IMpvReloader
{
    void RefreshMpv(LibMpvDynamicPlayer mpv, Subtitle subtitle, SubtitleFormat uiFormat);
    void Reset();
    bool SmpteMode { get; set; }
}
