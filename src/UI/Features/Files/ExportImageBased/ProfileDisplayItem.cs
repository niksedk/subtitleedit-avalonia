namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ProfileDisplayItem
{
    public string Name { get; set; }
    public bool IsSelected { get; set; }

    public ProfileDisplayItem(string displayName, bool isSelected)
    {
        Name = displayName;
        IsSelected = isSelected;
    }

    public override string ToString()
    {
        return Name;
    }
}