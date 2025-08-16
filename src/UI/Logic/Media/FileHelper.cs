using System.Collections.Generic;
using System.Linq;
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

        public async Task<string> PickOpenSubtitleFile(Visual sender, string title, bool includeVideoFiles = true)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = MakeOpenSubtitleFilter(includeVideoFiles),
            });

            if (files.Count >= 1)
            {
                return files[0].Path.LocalPath;
            }

            return string.Empty;
        }

        public async Task<string[]> PickOpenSubtitleFiles(Visual sender, string title, bool includeVideoFiles = true)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = true,
                FileTypeFilter = MakeOpenSubtitleFilter(includeVideoFiles),
            });

            return files.Select(p => p.Path.LocalPath).ToArray();
        }

        private static IReadOnlyList<FilePickerFileType> MakeOpenSubtitleFilter(bool includeVideoFiles)
        {
            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType("Subtitle files")
                {
                    Patterns = MakeOpenSubtitlePatterns(includeVideoFiles),
                },
                new FilePickerFileType("Video files")
                {
                    Patterns = GetVideoExtensions(),
                },
                new FilePickerFileType("All files")
                {
                    Patterns = new List<string> { "*" },
                }
            };

            return fileTypes;
        }

        private static List<string> MakeOpenSubtitlePatterns(bool includeVideoFiles)
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

            AddExt(existingTypes, patterns, ".mks");
            AddExt(existingTypes, patterns, ".pac");
            AddExt(existingTypes, patterns, ".890");
            AddExt(existingTypes, patterns, ".fpc");

            if (includeVideoFiles)
            {
                AddExt(existingTypes, patterns, ".mkv");
                AddExt(existingTypes, patterns, ".mp4");
                AddExt(existingTypes, patterns, ".ts");
                AddExt(existingTypes, patterns, ".sup");
            }

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
            string suggestedFileName,
            string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = suggestedFileName,
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

        public async Task<string> PickSaveSubtitleFile(
            Visual sender,
            string extension,
            string suggestedFileName,
            string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = suggestedFileName,
                FileTypeChoices = MakeSaveFilePickerFileTypes(extension, extension),
                DefaultExtension = extension.TrimStart('.')
            };
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);

            if (file != null)
            {
                return file.Path.LocalPath;
            }

            return string.Empty;
        }

        public async Task<string> PickSaveVideoFile(
            Visual sender,
            string extension,
            string suggestedFileName,
            string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = System.IO.Path.GetFileName(suggestedFileName),
                FileTypeChoices = MakeSaveFilePickerFileTypes(extension, extension),
                DefaultExtension = extension.TrimStart('.'),                
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

        private static List<FilePickerFileType> MakeSaveFilePickerFileTypes(string name, string extension)
        {
            var fileType = new FilePickerFileType(name)
            {
                Patterns = new List<string> { "*" + extension }
            };

            var fileTypes = new List<FilePickerFileType> { fileType };

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

        public async Task<string[]> PickOpenVideoFiles(Visual sender, string title)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = true,
                FileTypeFilter = MakeOpenVideoFilter(),
            });

            return files.Select(p => p.Path.LocalPath).ToArray();
        }


        private static IReadOnlyList<FilePickerFileType> MakeOpenVideoFilter()
        {
            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType("Video files")
                {
                    Patterns = GetVideoExtensions()
                },
                new FilePickerFileType("All files")
                {
                    Patterns = new List<string> { "*" },
                }
            };

            return fileTypes;
        }

        private static List<string> GetVideoExtensions()
        {
            return new List<string> { "*.mkv", "*.mp4", "*.ts", "*.mov", "*.mpeg", "*.m2ts" };
        }

        public async Task<string> PickOpenImageFile(Visual sender, string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = MakeOpenImageFilter(),
            });

            if (files.Count >= 1)
            {
                return files[0].Path.LocalPath;
            }

            return string.Empty;
        }

        private static IReadOnlyList<FilePickerFileType> MakeOpenImageFilter()
        {
            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType("Image files")
                {
                    Patterns = new List<string> { "*.png", "*.jpg" }
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
