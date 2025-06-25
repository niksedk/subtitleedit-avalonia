using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Initializers;

public interface IDictionaryInitializer
{
    Task UpdateDictionariesIfNeeded();
}

public class DictionaryInitializer : IDictionaryInitializer
{
    private readonly IZipUnpacker _zipUnpacker;

    public DictionaryInitializer(IZipUnpacker zipUnpacker)
    {
        _zipUnpacker = zipUnpacker;
    }

    public async Task UpdateDictionariesIfNeeded()
    {
        if (await NeedsUpdate())
        {
            await Unpack();
        }
    }

    private static async Task<bool> NeedsUpdate()
    {
        string outputDir = Se.DictionariesFolder;
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

        return themeNormalizedVersion.IsLessThan(currentNormalizedVersion);
    }

    private async Task Unpack()
    {    
        var zipUri = new Uri("avares://SubtitleEdit/Assets/Dictionaries.zip");
        await using var zipStream = AssetLoader.Open(zipUri);
        _zipUnpacker.UnpackZipStream(zipStream, Se.DictionariesFolder);
    }
}
