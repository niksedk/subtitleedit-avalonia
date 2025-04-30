using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

public static class FontHelper
{
    public static List<string> GetSystemFonts()
    {
        return new List<string> { "Arial", "Sans Sherif", "Tahoma", "Verdena" };
    }
}
