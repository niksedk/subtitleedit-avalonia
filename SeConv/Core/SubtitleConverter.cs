using System.Text;

namespace SeConv.Core;

internal class SubtitleConverter
{
    public async Task<ConversionResult> ConvertAsync(ConversionOptions options)
    {
        var result = new ConversionResult();

        try
        {
            // Get input files
            var inputFiles = GetInputFiles(options);
            result.TotalFiles = inputFiles.Count;

            foreach (var inputFile in inputFiles)
            {
                try
                {
                    await ConvertFileAsync(inputFile, options);
                    result.SuccessfulFiles++;
                }
                catch (Exception ex)
                {
                    result.FailedFiles++;
                    result.Errors.Add($"{inputFile}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Conversion failed: {ex.Message}");
        }

        return result;
    }

    private List<string> GetInputFiles(ConversionOptions options)
    {
        var files = new List<string>();
        var patterns = options.Pattern.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var baseFolder = string.IsNullOrEmpty(options.InputFolder)
            ? Directory.GetCurrentDirectory()
            : options.InputFolder;

        foreach (var pattern in patterns)
        {
            var trimmedPattern = pattern.Trim();
            var searchPath = Path.IsPathRooted(trimmedPattern)
                ? trimmedPattern
                : Path.Combine(baseFolder, trimmedPattern);

            var directory = Path.GetDirectoryName(searchPath) ?? baseFolder;
            var filePattern = Path.GetFileName(searchPath);

            if (Directory.Exists(directory))
            {
                files.AddRange(Directory.GetFiles(directory, filePattern));
            }
        }

        return files;
    }

    private async Task ConvertFileAsync(string inputFile, ConversionOptions options)
    {
        // TODO: Implement actual conversion using LibSE
        
        // Determine output file
        var outputFile = GetOutputFileName(inputFile, options);

        // Check if file exists and overwrite is not set
        if (File.Exists(outputFile) && !options.Overwrite)
        {
            throw new InvalidOperationException($"Output file already exists: {outputFile}. Use --overwrite to replace.");
        }

        // Simulate async work
        await Task.Delay(100);

        // TODO: Load subtitle file using LibSE
        // TODO: Apply operations in order
        // TODO: Convert to target format
        // TODO: Save to output file

        // For now, just create a placeholder file
        var outputDir = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
    }

    private string GetOutputFileName(string inputFile, ConversionOptions options)
    {
        if (!string.IsNullOrEmpty(options.OutputFilename))
        {
            return options.OutputFilename;
        }

        var fileName = Path.GetFileNameWithoutExtension(inputFile);
        var extension = GetExtensionForFormat(options.Format);

        var outputFolder = string.IsNullOrEmpty(options.OutputFolder)
            ? Path.GetDirectoryName(inputFile) ?? Directory.GetCurrentDirectory()
            : options.OutputFolder;

        return Path.Combine(outputFolder, fileName + extension);
    }

    private string GetExtensionForFormat(string format)
    {
        // TODO: Get extension from LibSE format
        return format.ToLowerInvariant() switch
        {
            "subrip" => ".srt",
            "sami" => ".smi",
            "adobeencore" => ".txt",
            "substationalpha" => ".ssa",
            "advancedsubstationalpha" => ".ass",
            "webvtt" => ".vtt",
            "microdvd" => ".sub",
            "timedtext" => ".xml",
            _ => ".txt"
        };
    }
}

internal class ConversionOptions
{
    public required string Pattern { get; init; }
    public required string Format { get; init; }
    public string? InputFolder { get; init; }
    public string? OutputFolder { get; init; }
    public string? OutputFilename { get; init; }
    public string? Encoding { get; init; }
    public double? Fps { get; init; }
    public double? TargetFps { get; init; }
    public bool Overwrite { get; init; }
    public List<string> Operations { get; init; } = new();
}

internal class ConversionResult
{
    public int TotalFiles { get; set; }
    public int SuccessfulFiles { get; set; }
    public int FailedFiles { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success => FailedFiles == 0 && Errors.Count == 0;
}
