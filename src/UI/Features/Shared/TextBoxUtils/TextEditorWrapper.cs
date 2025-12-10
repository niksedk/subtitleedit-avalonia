using System;
using Avalonia.Controls;
using AvaloniaEdit;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

public class TextEditorWrapper(TextEditor textEditor) : INikseTextBox
{
    public string Text
    {
        get => textEditor.Text;
        set => textEditor.Text = value;
    }

    public string SelectedText
    {
        get => textEditor.SelectedText;
        set => textEditor.SelectedText = value;
    }

    public int SelectionStart
    {
        get => textEditor.SelectionStart;
        set => textEditor.SelectionStart = value;
    }

    public int SelectionLength
    {
        get => textEditor.SelectionLength;
        set => textEditor.SelectionLength = value;
    }

    public int SelectionEnd
    {
        get => textEditor.SelectionStart + textEditor.SelectionLength;
        set => textEditor.SelectionLength = Math.Max(0, value - textEditor.SelectionStart);
    }

    public void Select(int start, int length)
    {
        textEditor.SelectionStart = start;
        textEditor.SelectionLength = length;
    }

    public int CaretIndex
    {
        get => textEditor.CaretOffset;
        set => textEditor.CaretOffset = value;
    }

    public void Focus()
    {
        textEditor.Focus();
    }

    public Control Control => textEditor;

    public bool IsFocused => textEditor.IsFocused;

    public void Cut()
    {
        textEditor.Cut();
    }

    public void Copy()
    {
        textEditor.Copy();
    }

    public void Paste()
    {
        textEditor.Paste();
    }

    public void SelectAll()
    {
        textEditor.SelectAll();
    }

    public void ClearSelection()
    {
        textEditor.SelectionLength = 0;
    }
}