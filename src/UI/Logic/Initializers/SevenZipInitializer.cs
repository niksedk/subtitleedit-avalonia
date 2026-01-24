using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Initializers;

public interface ISevenZipInitializer
{
    Task UpdateSevenZipIfNeeded();
}

public class SevenZipInitializer : ISevenZipInitializer
{
    public async Task UpdateSevenZipIfNeeded()
    {
        // On Windows, we bundle 7zxa.dll; on Linux/macOS, use system libraries
        if (!OperatingSystem.IsWindows())
        {
            return; // Linux/macOS will use system-installed p7zip
        }

        string outputDir = Se.SevenZipFolder;
        if (await NeedsUpdate(outputDir))
        {
            await Unpack(outputDir);
            WriteNewVersionFile(outputDir);
        }
    }

    private static void WriteNewVersionFile(string outputDir)
    {
        try
        {
            if (!Directory.Exists(outputDir))
            {
                return;
            }

            var versionFileName = Path.Combine(outputDir, "version.txt");
            File.Delete(versionFileName);
            File.WriteAllText(versionFileName, Se.Version);
        }
        catch
        {
            Se.LogError($"Could not write version file in \"{outputDir}\" folder.");
        }
    }

    private static async Task<bool> NeedsUpdate(string outputDir)
    {
        if (!Directory.Exists(outputDir))
        {
            return true;
        }

        var versionFileName = Path.Combine(outputDir, "version.txt");
        if (!File.Exists(versionFileName))
        {
            return true;
        }

        var currentNormalizedVersion = new SemanticVersion(Se.Version);

        var version = await File.ReadAllTextAsync(versionFileName);
        var normalizedVersion = new SemanticVersion(version);

        if (normalizedVersion.IsLessThan(currentNormalizedVersion))
        {
            return true;
        }

        return false;
    }

    private async Task Unpack(string outputDir)
    {
        try
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var uri = new Uri("avares://SubtitleEdit/Assets/SevenZip/7zxa.dll");
            await using var stream = AssetLoader.Open(uri);
            var outputPath = Path.Combine(outputDir, "7zxa.dll");
            await using var fileStream = File.Create(outputPath);
            await stream.CopyToAsync(fileStream);
        }
        catch
        {
            // Ignore
        }
    }
}
