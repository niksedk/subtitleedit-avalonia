namespace Nikse.SubtitleEdit.Features.Translate;

public class OllamaModelDisplay
{
    public string Name { get; set; }
    public string Model { get; set; }
    public long Size { get; set; }

    public OllamaModelDisplay()
    {
        Name = string.Empty;
        Model = string.Empty;
    }

    public override string ToString()
    {
        return Name;
    }
}