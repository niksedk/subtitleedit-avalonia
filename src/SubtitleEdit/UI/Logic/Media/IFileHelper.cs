using Avalonia;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IFileHelper
{
    Task<string> PickOpenSubtitleFile(Visual sender, string title);
    Task<string> PickSaveSubtitleFile(Visual sender, SubtitleFormat currentFormat, string title);

    Task<string> PickOpenVideoFile(Visual sender, string title);

}