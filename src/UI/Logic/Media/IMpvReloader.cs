using HanumanInstitute.LibMpv;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IMpvReloader
{
    void RefreshMpv(MpvContext mpv, Subtitle subtitle, SubtitleFormat uiFormat);
    void Reset();
}
