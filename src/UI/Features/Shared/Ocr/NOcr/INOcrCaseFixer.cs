using Nikse.SubtitleEdit.Logic.Ocr;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr.NOcr;

public interface INOcrCaseFixer
{
    string FixUppercaseLowercaseIssues(ImageSplitterItem2 targetItem, NOcrChar result);
}