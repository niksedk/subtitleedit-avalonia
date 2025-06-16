namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class NOcrDrawModeItem
{
    public string Name { get; set; }

    public NOcrDrawModeItem(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public static NOcrDrawModeItem ForegroundItem = new("Foreground");
    public static NOcrDrawModeItem BackgroundItem = new("Background");

    public static NOcrDrawModeItem[] Items = new NOcrDrawModeItem[]
    {
        ForegroundItem,
        BackgroundItem
    };
}
