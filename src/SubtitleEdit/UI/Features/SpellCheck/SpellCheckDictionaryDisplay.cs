namespace Nikse.SubtitleEdit.Features.SpellCheck;

public class SpellCheckDictionaryDisplay
{
    public string Name { get; set; } = string.Empty;
    public string DictionaryFileName { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }
}
