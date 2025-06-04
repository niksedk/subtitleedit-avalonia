using Nikse.SubtitleEdit.Logic.Ocr;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public interface INOcrCaseFixer
{
    string FixUppercaseLowercaseIssues(ImageSplitterItem2 targetItem, NOcrChar result);
}