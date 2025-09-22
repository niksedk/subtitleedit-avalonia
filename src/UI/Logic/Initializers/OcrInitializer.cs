using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Initializers;

public interface  IOcrInitializer
{
    Task UpdateOcrDictionariesIfNeeded();
}

public class OcrInitializer(IZipUnpacker zipUnpacker) : IOcrInitializer
{
    public async Task UpdateOcrDictionariesIfNeeded()
    {
        if (await NeedsUpdate())
        {
            await Unpack();
        }
    }

    private static async Task<bool> NeedsUpdate()
    {
        var outputDir = Se.OcrFolder;
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
        var themeNormalizedVersion = new SemanticVersion(version);

        if (themeNormalizedVersion.IsLessThan(currentNormalizedVersion))
        {
            try
            {
                File.Delete(versionFileName);
                File.WriteAllText(versionFileName, Se.Version);
            }
            catch
            {
                Se.LogError($"Could not write version file in \"{outputDir}\" folder.");
            }

            return true;
        }

        return false;
    }

    private async Task Unpack()
    {
        var zipUri = new Uri("avares://SubtitleEdit/Assets/Ocr.zip");
        await using var zipStream = AssetLoader.Open(zipUri);
        zipUnpacker.UnpackZipStream(zipStream, Se.OcrFolder);
    }
}
