using Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAssa
{
    public List<SeAssaStyle> StoredStyles { get; set; }
    public string LastOverrideTag { get; internal set; }
    public int ProgressBarHeight { get; set; }
    public string ProgressBarForegroundColor { get; set; }
    public string ProgressBarBackgroundColor { get; set; }
    public int ProgressBarCornerStyleIndex { get; set; }

    public SeAssa()
    {
        StoredStyles = new List<SeAssaStyle>();
        LastOverrideTag = OverrideTagDisplay.List().First().Tag;
        
        ProgressBarHeight = 40;
        ProgressBarForegroundColor = "#FF0000";
        ProgressBarBackgroundColor = "#80000000";
        ProgressBarCornerStyleIndex = 0;
    }
}