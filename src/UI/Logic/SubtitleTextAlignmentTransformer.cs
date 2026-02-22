using Avalonia.Media;
using AvaloniaEdit.Rendering;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Stores text alignment preference for subtitle text editor.
/// Note: AvaloniaEdit doesn't support text alignment natively through transformers.
/// The actual alignment is applied through the TextView's HorizontalAlignment property.
/// </summary>
public class SubtitleTextAlignmentTransformer : IVisualLineTransformer
{
    private TextAlignment _alignment = TextAlignment.Left;

    public TextAlignment Alignment
    {
        get => _alignment;
        set => _alignment = value;
    }

    public void Transform(ITextRunConstructionContext context, System.Collections.Generic.IList<VisualLineElement> elements)
    {
        // No transformation needed - alignment is handled by TextView.HorizontalAlignment
    }
}





