using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class VideoEncodingItem
{
    public string Codec { get; set; }
    public string Name { get; set; }

    public VideoEncodingItem(string codec, string name)
    {
        Codec = codec;
        Name = name;
    }

    public override string ToString()
    {
        return $"{Codec}: {Name}";
    }

    public static List<VideoEncodingItem> VideoEncodings = new()
    {
        new VideoEncodingItem("libx264", "H.264/AVC (CPU)"),
        new VideoEncodingItem("libx265", "H.265/HEVC (CPU)"),
        new VideoEncodingItem("libvpx-vp9", "VP9 (CPU)"),
        new VideoEncodingItem("h264_nvenc", "H.264/AVC (NVIDIA GPU)"),
        new VideoEncodingItem("hevc_nvenc", "H.265/HEVC (NVIDIA GPU)"),
        new VideoEncodingItem("h264_amf", "H.264/AVC (AMD GPU)"),
        new VideoEncodingItem("hevc_amf", "H.265/HEVC (AMD GPU)"),
        new VideoEncodingItem("prores_ks", "ProRes (CPU)"),
        new VideoEncodingItem("h264_qsv", "H.264/AVC (Intel QSV)"),
        new VideoEncodingItem("hevc_qsv", "H.265/HEVC (Intel QSV)")
    };
}
