using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAssa
{
    public List<SeAssaStyle> StoredStyles { get; set; }

    public SeAssa()
    {
        StoredStyles = new List<SeAssaStyle>();
    }
}