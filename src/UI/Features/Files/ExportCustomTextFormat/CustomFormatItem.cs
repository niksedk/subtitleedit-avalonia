namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public class CustomFormatItem
{
    public string Name { get; set; }
    public string Extension { get; set; }
    public string FormatString { get; set; }
    public CustomFormatItem(string name, string extension, string formatString)
    {
        Name = name;
        Extension = extension;
        FormatString = formatString;
    }
}
