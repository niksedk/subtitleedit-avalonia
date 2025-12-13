using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

public class BinarySubtitleItem
{
    public BinarySubtitleItem(OcrSubtitleItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        Number = item.Number;
        IsForced = false; // OcrSubtitleItem does not expose forced flag; default to false
        Text = item.Text;

        // Format times using TimeCode to match project conventions
        StartTime = new TimeCode(item.StartTime).ToString(false);
        EndTime = new TimeCode(item.EndTime).ToString(false);
        Duration = new TimeCode(item.Duration).ToString(false);

        // Get bitmap (cropped to remove transparent borders)
        try
        {
            Bitmap = item.GetBitmapCropped();
        }
        catch
        {
            Bitmap = null;
        }

        // Position (x,y) from OcrSubtitleItem
        try
        {
            var pos = item.GetPosition();
            X = pos.X;
            Y = pos.Y;
        }
        catch
        {
            X = 0;
            Y = 0;
        }
    }
    
    public int Number { get; set; }
    public bool IsForced { get; set; }
    public Bitmap? Bitmap { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Duration { get; set; }
    public string Text { get; set; }

    // Position on screen
    public int X { get; set; }
    public int Y { get; set; }
}