using System;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

public class TextEditorWrapper : ITextBoxWrapper
{
    private readonly TextEditor _textEditor;
    private readonly Border _border;

    public bool HasFocus { get; set; }

    public TextEditorWrapper(TextEditor textEditor, Border border)
    {
        _textEditor = textEditor;
        _border = border;
    }

    public string Text
    {
        get => _textEditor.Text;
        set => _textEditor.Text = value;
    }

    public string SelectedText
    {
        get => _textEditor.SelectedText;
        set => _textEditor.SelectedText = value;
    }

    public int SelectionStart
    {
        get => _textEditor.SelectionStart;
        set => _textEditor.SelectionStart = value;
    }

    public int SelectionLength
    {
        get => _textEditor.SelectionLength;
        set => _textEditor.SelectionLength = value;
    }

    public int SelectionEnd
    {
        get => _textEditor.SelectionStart + _textEditor.SelectionLength;
        set => _textEditor.SelectionLength = Math.Max(0, value - _textEditor.SelectionStart);
    }

    public void Select(int start, int length)
    {
        _textEditor.SelectionStart = start;
        _textEditor.SelectionLength = length;
    }

    public int CaretIndex
    {
        get => _textEditor.CaretOffset;
        set => _textEditor.CaretOffset = value;
    }

    public void Focus()
    {
        _textEditor.Focus();
    }

    public Control TextControl => _textEditor;
    public Control ContentControl => _border;

    public bool IsFocused => HasFocus;

    public void Cut()
    {
        _textEditor.Cut();
    }

    public void Copy()
    {
        _textEditor.Copy();
    }

    public void Paste()
    {
        _textEditor.Paste();
    }

    public void SelectAll()
    {
        _textEditor.SelectAll();
    }

    public void ClearSelection()
    {
        _textEditor.SelectionLength = 0;
    }

    public void SetAlignment(TextAlignment alignment)
    {
        // not supported in TextEditor
    }
}