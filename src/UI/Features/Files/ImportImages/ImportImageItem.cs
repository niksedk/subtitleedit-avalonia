using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Features.Files.ImportImages;

public class ImportImageItem
{
    public string FileName { get; set; }
    public long Size { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public TimeSpan Duration { get; set; }
    public byte[] Bytes { get; set; }

    public ImportImageItem()
    {
        FileName = string.Empty;
        Bytes = Array.Empty<byte>();
    }

    public ImportImageItem(string fileName)
    {
        FileName = fileName;
        Bytes = FileUtil.ReadAllBytesShared(fileName);
    }
}

