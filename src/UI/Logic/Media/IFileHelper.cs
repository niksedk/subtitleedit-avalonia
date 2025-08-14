using Avalonia;
using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IFileHelper
{
    Task<string> PickOpenFile(Visual sender, string title, string extensionTitle, string extension);
    Task<string> PickOpenSubtitleFile(Visual sender, string title, bool includeVideoFiles = true);
    Task<string[]> PickOpenSubtitleFiles(Visual sender, string title, bool includeVideoFiles = true);
    Task<string> PickSaveSubtitleFile(
        Visual sender,
        SubtitleFormat currentFormat,
        string suggestedFileName,
        string title);
    Task<string> PickSaveSubtitleFile(
        Visual sender,
        string extension,
        string suggestedFileName,
        string title);

    Task<string> PickOpenVideoFile(Visual sender, string title);
    Task<string[]> PickOpenVideoFiles(Visual sender, string title);
    Task<string> PickOpenImageFile(Visual sender, string title);
}