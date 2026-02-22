using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

public class TextEditorWrapper : ITextBoxWrapper
{
    private static int Counter = 0;

    private readonly TextEditor _textEditor;
    private readonly Border _border;
    private readonly int _instanceId = Counter++;
    private readonly SpellCheckUnderlineTransformer _spellCheckTransformer;

    public bool HasFocus { get; set; }

    public TextEditorWrapper(TextEditor textEditor, Border border)
    {
        _textEditor = textEditor;
        _border = border;

        _spellCheckTransformer = new SpellCheckUnderlineTransformer();
        _textEditor.TextArea.TextView.LineTransformers.Add(_spellCheckTransformer);
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

    public void EnableSpellCheck(ISpellCheckManager spellCheckManager)
    {
        _spellCheckTransformer.SetSpellCheckManager(spellCheckManager);
        _spellCheckTransformer.IsEnabled = true;
    }

    public void DisableSpellCheck()
    {
        _spellCheckTransformer.IsEnabled = false;
    }

    public void RefreshSpellCheck()
    {
        _spellCheckTransformer.Refresh();
    }

    public bool IsSpellCheckEnabled => _spellCheckTransformer.IsEnabled;
}