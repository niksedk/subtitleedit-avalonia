﻿namespace Nikse.SubtitleEdit.Logic.Config;

public class SeVideoTransparent
{
    public string OutputSuffix { get; set; }
    public double FrameRate { get; set; }
    public bool UseOutputFolder { get; internal set; }
    public string OutputFolder { get; internal set; }

    public SeVideoTransparent()
    {
        OutputSuffix = "_transparent";
        FrameRate = 23.976;
        OutputFolder = string.Empty;
    }
}