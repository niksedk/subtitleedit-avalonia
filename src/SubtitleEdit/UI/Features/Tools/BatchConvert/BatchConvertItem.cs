using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertItem : ObservableObject
{
    public string FileName { get; set; }
    public long Size { get; set; }
    public string DisplaySize { get; set; }
    public string Format { get; set; }
    [ObservableProperty] private string _status;
    public Subtitle? Subtitle { get; set; }

    public BatchConvertItem()
    {
        FileName = string.Empty;
        Format = string.Empty;
        Status = string.Empty;
        DisplaySize = string.Empty;
    }

    public BatchConvertItem(string fileName, long size, string format, Subtitle? subtitle)
    {
        FileName = fileName;
        Size = size;
        Format = format;
        Status = "-";
        Subtitle = subtitle;
        DisplaySize = Utilities.FormatBytesToDisplayFileSize(size);
    }
}
