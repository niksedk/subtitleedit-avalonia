using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic.Media
{
    public class FileHelper : IFileHelper
    {
        public async Task<string> PickOpenFile(Visual sender, string title, string extensionTitle, string extension)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType(extension)
                {
                    Patterns = new List<string> { extension }
                },
            };

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes,
            });

            if (files.Count >= 1)
            {
                return files[0].Path.LocalPath;
            }

            return string.Empty;
        }

        public async Task<string> PickOpenSubtitleFile(Visual sender, string title)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = MakeOpenSubtitleFilter(),
            });

            if (files.Count >= 1)
            {
                return files[0].Path.LocalPath;
            }

            return string.Empty;
        }

        private static IReadOnlyList<FilePickerFileType> MakeOpenSubtitleFilter()
        {
            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType("Subtitle files")
                {
                    Patterns = MakeOpenSubtitlePatterns(),
                },
                new FilePickerFileType("Video files")
                {
                    Patterns = new List<string> { "*.mkv", "*.mp4", ".ts" }
                },
                new FilePickerFileType("All files")
                {
                    Patterns = new List<string> { "*" },
                }
            };

            return fileTypes;
        }

        private static List<string> MakeOpenSubtitlePatterns()
        {
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
            AddExt(existingTypes, patterns, ".mks"); //TODO: move to settings
            AddExt(existingTypes, patterns, ".mkv");
            AddExt(existingTypes, patterns, ".mp4");
            AddExt(existingTypes, patterns, ".ts");
            AddExt(existingTypes, patterns, ".sup");
            return patterns;
        }

        private static void AddExt(HashSet<string> existingTypes, List<string> patterns, string ext)
        {
            if (!existingTypes.Contains(ext))
            {
                existingTypes.Add(ext);
                patterns.Add("*" + ext);
            }
        }

        public async Task<string> PickSaveSubtitleFile(
            Visual sender, 
            SubtitleFormat currentFormat,
            string saveSubtitleFile, 
            string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = saveSubtitleFile,
                FileTypeChoices = MakeSaveFilePickerFileTypes(currentFormat),
                DefaultExtension = currentFormat.Extension.TrimStart('.')
            };
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);

            if (file != null)
            {
                return file.Path.LocalPath;
            }

            return string.Empty;
        }

        private static List<FilePickerFileType> MakeSaveFilePickerFileTypes(SubtitleFormat currentFormat)
        {
            var fileType = new FilePickerFileType(currentFormat.Name)
            {
                Patterns = new List<string> { "*" + currentFormat.Extension }
            };
            var fileTypes = new List<FilePickerFileType> { fileType };

            //foreach (var format in SubtitleFormat.AllSubtitleFormats)
            //{
            //    if (format.IsTextBased && format.Name != currentFormat.Name)
            //    {
            //        var patterns = new List<string>
            //        {
            //            "*" + format.Extension
            //        };

            //        fileTypes.Add(new FilePickerFileType(format.Name)
            //        {
            //            Patterns = patterns
            //        });
            //    }
            //}            

            return fileTypes;
        }

        public async Task<string> PickOpenVideoFile(Visual sender, string title)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = MakeOpenVideoFilter(),
            });

            if (files.Count >= 1)
            {
                return files[0].Path.LocalPath;
            }

            return string.Empty;
        }

        private static IReadOnlyList<FilePickerFileType> MakeOpenVideoFilter()
        {
            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType("Video files")
                {
                    Patterns = new List<string> { "*.mkv", "*.mp4", ".ts", ".mov", "*.mpeg" }
                },
                new FilePickerFileType("All files")
                {
                    Patterns = new List<string> { "*" },
                }
            };

            return fileTypes;
        }
    }
}
