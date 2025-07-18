using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ResolutionItem : ObservableObject
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }


    public ResolutionItem(string name)
    {
        DisplayName = string.Empty;
        Name = name;

        SetDisplayName(name);
    }

    private void SetDisplayName(string name)
    {
        DisplayName = $"{name} - {Width}x{Height}";
    }

    public ResolutionItem(string name, int width, int height)
    {
        DisplayName = string.Empty;
        Name = name;
        Width = width;
        Height = height;
        SetDisplayName(name);
    }

    public static IEnumerable<ResolutionItem> GetResolutions()
    {
        yield return new ResolutionItem("4K DCI - Aspect Ratio 16∶9", 4096, 2160);
        yield return new ResolutionItem("4K UHD - Aspect Ratio 16∶9", 3840, 2160);
        yield return new ResolutionItem("2K WQHD - Aspect Ratio 16∶9", 2560, 1440);
        yield return new ResolutionItem("2K DCI - Aspect Ratio 16∶9", 2048, 1080);
        yield return new ResolutionItem("Full HD 1080p - Aspect Ratio 16∶9", 1920, 1080);
        yield return new ResolutionItem("HD 720p - Aspect Ratio 16∶9", 1280, 720);
        yield return new ResolutionItem("540p - Aspect Ratio 16∶9", 960, 540);
        yield return new ResolutionItem("SD PAL - Aspect Ratio 4:3", 720, 576);
        yield return new ResolutionItem("SD NTSC - Aspect Ratio 3:2", 720, 480);
        yield return new ResolutionItem("VGA - Aspect Ratio 4:3", 640, 480);
        yield return new ResolutionItem("360p - Aspect Ratio 16∶9", 640, 360);

        yield return new ResolutionItem("YouTube shorts/TikTok - Aspect Ratio 9∶16", 1080, 1920);
        yield return new ResolutionItem("YouTube shorts/TikTok - Aspect Ratio 9∶16", 720, 1280);
        yield return new ResolutionItem("1/2 A - Aspect Ratio 9∶16", 540, 960);
        yield return new ResolutionItem("1/2 B - Aspect Ratio 9∶16", 360, 540);
        yield return new ResolutionItem("1/4 A - Aspect Ratio 9∶16", 270, 480);
        yield return new ResolutionItem("1/4 B - Aspect Ratio 9∶16 - (180x270)", 180, 270);
    }
}