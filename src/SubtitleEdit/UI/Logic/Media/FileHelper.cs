using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Media;

namespace SubtitleAlchemist.Logic.Media
{
    public class FileHelper : IFileHelper
    {
        public async Task<string> PickAndShowSubtitleFile(Avalonia.Visual sender, string title)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = MakeSubtitleFilter(),
            });

            if (files.Count >= 1)
            {
                return files[0].Path.LocalPath;
            }

            return string.Empty;
        }

        private IReadOnlyList<FilePickerFileType> MakeSubtitleFilter()
        {
            var fileTypes = new List<FilePickerFileType>();
            var existingTypes = new HashSet<string>();
            var patterns = new List<string>();
            foreach (var format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.IsTextBased)
                {
                    AddExt(existingTypes, patterns, format.Extension);
                    if (format.AlternateExtensions != null)
                    {
                        foreach (var ext in format.AlternateExtensions)
                        {
                            AddExt(existingTypes, patterns, ext);
                        }
                    }
                }
            }
            AddExt(existingTypes, patterns, ".mks");

            fileTypes.Add(new FilePickerFileType("Subtitle files")
            {
                Patterns = patterns,
            });
            fileTypes.Add(new FilePickerFileType("Video files")
            {
                Patterns = new List<string> { "*.mkv", "*.mp4", ".ts" }
            });
            fileTypes.Add(new FilePickerFileType("All files")
            {
                Patterns = new List<string> { "*" },
            });

            return fileTypes;
        }

        private static void AddExt(HashSet<string> existingTypes, List<string> patterns, string ext)
        {
            if (!existingTypes.Contains(ext))
            {
                existingTypes.Add(ext);
                patterns.Add("*" + ext);
            }
        }
    }
}
