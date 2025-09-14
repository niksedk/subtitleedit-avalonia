using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public partial class CustomFormatItem : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _extension;
    [ObservableProperty] private string _formatHeader;
    [ObservableProperty] private string _formatText;
    [ObservableProperty] private string _formatFooter;
    [ObservableProperty] private string _formatTimeCode;
    [ObservableProperty] private string _formatNewLine;

    public CustomFormatItem()
    {
        Name = string.Empty;
        Extension = string.Empty;
        FormatHeader = string.Empty;
        FormatText = string.Empty;
        FormatFooter = string.Empty;
        FormatTimeCode = string.Empty;
        FormatNewLine = string.Empty;
    }

    public CustomFormatItem(string name, string extension, string formatHeader, string formatText, string formatFooter, string formatTimeCode, string formatNewLine)
    {
        Name = name;
        Extension = extension;
        FormatHeader = formatHeader;
        FormatText = formatText;
        FormatFooter = formatFooter;
        FormatTimeCode = formatTimeCode;
        FormatNewLine = formatNewLine;
    }

    public CustomFormatItem(SeExportCustomFormatItem customFormat)
    {
        Name = customFormat.Name;
        Extension = customFormat.Extension;
        FormatHeader = customFormat.FormatHeader;
        FormatText = customFormat.FormatText;
        FormatFooter = customFormat.FormatFooter;
        FormatTimeCode = customFormat.FormatTimeCode;
        FormatNewLine = customFormat.FormatNewLine;
    }

    public override string ToString()
    {
        return Name;
    }
}
