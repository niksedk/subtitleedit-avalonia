namespace SeConv.Core;

/// <summary>
/// Helper class to integrate with LibSE subtitle library
/// TODO: Implement actual integration with LibSE
/// </summary>
internal static class LibSEIntegration
{
    /// <summary>
    /// Gets all available subtitle formats from LibSE
    /// </summary>
    public static List<SubtitleFormat> GetAvailableFormats()
    {
        // TODO: Use LibSE's SubtitleFormat class to get all available formats
        // Example: return SubtitleFormat.AllSubtitleFormats.Select(f => new SubtitleFormat
        // {
        //     Name = f.Name,
        //     Extension = f.Extension,
        //     Description = f.Description
        // }).ToList();

        // For now, return a sample list
        return new List<SubtitleFormat>
        {
            new() { Name = "SubRip", Extension = ".srt", Description = "SubRip text format" },
            new() { Name = "SAMI", Extension = ".smi", Description = "Synchronized Accessible Media Interchange" },
            new() { Name = "AdvancedSubStationAlpha", Extension = ".ass", Description = "Advanced Sub Station Alpha" },
            new() { Name = "WebVTT", Extension = ".vtt", Description = "Web Video Text Tracks" },
        };
    }

    /// <summary>
    /// Loads a subtitle file using LibSE
    /// </summary>
    public static object? LoadSubtitle(string filePath, string? encoding = null)
    {
        // TODO: Implement using LibSE
        // Example:
        // var subtitle = new Subtitle();
        // var format = subtitle.LoadSubtitle(filePath, out _, encoding);
        // return subtitle;
        
        return null;
    }

    /// <summary>
    /// Saves a subtitle to a file using LibSE
    /// </summary>
    public static void SaveSubtitle(object subtitle, string filePath, string formatName, string? encoding = null)
    {
        // TODO: Implement using LibSE
        // Example:
        // var format = SubtitleFormat.AllSubtitleFormats.FirstOrDefault(f => 
        //     f.Name.Replace(" ", "").Equals(formatName, StringComparison.OrdinalIgnoreCase));
        // if (format != null)
        // {
        //     var content = format.ToText((Subtitle)subtitle, formatName);
        //     File.WriteAllText(filePath, content, GetEncoding(encoding));
        // }
    }

    /// <summary>
    /// Applies subtitle operations/transformations
    /// </summary>
    public static void ApplyOperations(object subtitle, List<string> operations)
    {
        // TODO: Implement operations using LibSE
        // Example operations:
        // - FixCommonErrors: Use LibSE's FixCommonErrors class
        // - RemoveTextForHI: Use LibSE's RemoveTextForHI class
        // - MergeSameTexts: Use LibSE's MergeSameTexts functionality
        // - SplitLongLines: Use LibSE's SplitLongLines functionality
        
        foreach (var operation in operations)
        {
            // Apply each operation in order
            // switch (operation)
            // {
            //     case "FixCommonErrors":
            //         // Apply FixCommonErrors
            //         break;
            //     case "RemoveTextForHI":
            //         // Apply RemoveTextForHI
            //         break;
            //     // ... other operations
            // }
        }
    }
}

/// <summary>
/// Represents a subtitle format
/// </summary>
internal record SubtitleFormat
{
    public required string Name { get; init; }
    public required string Extension { get; init; }
    public string Description { get; init; } = string.Empty;
}
