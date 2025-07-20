using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IFolderHelper
{
    Task<string> PickFolderAsync(Window window, string title);
    Task OpenFolder(Window window, string folder);
}

public class FolderHelper : IFolderHelper
{
    public async Task<string> PickFolderAsync(Window window, string title)
    {
        var storageProvider = window.StorageProvider;

        if (storageProvider.CanPickFolder)
        {
            var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

            var selected = folders.Count > 0 ? folders[0] : null;
            return selected?.Path.LocalPath; 
        }

        return string.Empty;
    }

    public async Task OpenFolder(Window window, string folder)
    {
        var path = Path.GetDirectoryName(folder)!;
        var dirInfo = new DirectoryInfo(path);
        await window!.Launcher.LaunchDirectoryInfoAsync(dirInfo);
    }
}