using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IFileHelper
{
    Task<string> PickAndShowSubtitleFile(Avalonia.Visual sender, string title);
    //Task<string[]> PickAndShowSubtitleFiles(string title);
    //Task<string> PickAndShowSubtitleFile(string title, SubtitleFormat format);

    //Task<string> PickAndShowVideoFile(string title);
    //Task<string[]> PickAndShowVideoFiles(string title);

    //Task<string> PickAndShowFile(string title, string extension);

    //Task<string> SaveSubtitleFileAs(string title, string videoFileName, SubtitleFormat format, Subtitle subtitle,        CancellationToken cancellationToken = default);
    //Task<string> SaveStreamAs(Stream stream, string title, string fileName, SubtitleFormat format, CancellationToken cancellationToken = default);
    //Task<string> SaveStreamAs(Stream stream, string title, string fileName, string extension, CancellationToken cancellationToken = default);
}