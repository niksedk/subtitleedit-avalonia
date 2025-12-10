using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

public interface INikseTextBox
{
    string Text { get; set; }
    string SelectedText { get; set; }
    int SelectionStart { get; set; }
    int SelectionLength { get; set; }
    int SelectionEnd { get; set; }
    void Select(int start, int length);
    int CaretIndex { get; set; }
    void Focus();
    Control Control { get; } 
    bool IsFocused { get; }
    void Cut();
    void Copy();
    void Paste();
    void SelectAll();
    void ClearSelection();
}