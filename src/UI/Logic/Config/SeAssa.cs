using Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAssa
{
    public List<SeAssaStyle> StoredStyles { get; set; }
    public string LastOverrideTag { get; internal set; }

    public SeAssa()
    {
        StoredStyles = new List<SeAssaStyle>();
        LastOverrideTag = OverrideTagDisplay.List().First().Tag;
    }
}