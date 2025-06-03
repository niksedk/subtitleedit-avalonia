using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;

namespace Nikse.SubtitleEdit.Logic;

public interface IBluRayHelper
{
    Subtitle LoadBluRaySubFromMatroska(MatroskaTrackInfo track, MatroskaFile matroska, out string errorMessage);
}