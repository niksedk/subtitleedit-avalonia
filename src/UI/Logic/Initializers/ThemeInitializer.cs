using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Initializers;

public interface  IThemeInitializer
{
    Task UpdateThemesIfNeeded();
}

public class ThemeInitializer : IThemeInitializer
{
    private readonly IZipUnpacker _zipUnpacker;

    public ThemeInitializer(IZipUnpacker zipUnpacker)
    {
        _zipUnpacker = zipUnpacker;
    }

    public async Task UpdateThemesIfNeeded()
    {
        if (await NeedsUpdate())
        {
            await Unpack();
        }
    }

    private static async Task<bool> NeedsUpdate()
    {
        string outputDir = Se.ThemesFolder;
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
        var zipUri = new Uri("avares://SubtitleEdit/Assets/Themes.zip");
        await using var zipStream = AssetLoader.Open(zipUri);
        _zipUnpacker.UnpackZipStream(zipStream, Se.ThemesFolder);
    }
}
