using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Media;

public static class FfmpegHelper
{
    public static bool IsFfmpegInstalled()
    {
        Configuration.Settings.General.UseFFmpegForWaveExtraction = true;        
        return File.Exists(Se.Settings.General.FfmpegPath);
    }
}
