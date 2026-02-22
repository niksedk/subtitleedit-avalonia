using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

public class TextEditorWrapper : ITextBoxWrapper
{
    private readonly TextEditor _textEditor;
    private readonly Border _border;
    private readonly SpellCheckUnderlineTransformer _spellCheckTransformer;

    public bool HasFocus { get; set; }

    public TextEditorWrapper(TextEditor textEditor, Border border)
    {
        _textEditor = textEditor;
        _border = border;

        _spellCheckTransformer = new SpellCheckUnderlineTransformer();
        _spellCheckTransformer.SetTextView(_textEditor.TextArea.TextView);
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

    /// <summary>
    /// Gets the word at the specified text offset position.
    /// </summary>
    public SpellCheckWord? GetWordAtOffset(int offset)
    {
        if (string.IsNullOrEmpty(Text) || offset < 0 || offset > Text.Length)
        {
            return null;
        }

        var words = SpellCheckWordLists2.Split(Text);
        return words.FirstOrDefault(w => offset >= w.Index && offset < w.Index + w.Length);
    }

    /// <summary>
    /// Gets the word at the current caret position.
    /// </summary>
    public SpellCheckWord? GetWordAtCaret()
    {
        return GetWordAtOffset(CaretIndex);
    }

    /// <summary>
    /// Gets the word at the specified mouse position.
    /// Returns null if the position is not over a word.
    /// </summary>
    public SpellCheckWord? GetWordAtPosition(PointerEventArgs e)
    {
        var position = e.GetPosition(_textEditor.TextArea.TextView);
        var visualPosition = _textEditor.TextArea.TextView.GetPosition(position);

        if (visualPosition == null)
        {
            return null;
        }

        var offset = _textEditor.Document.GetOffset(visualPosition.Value.Location);
        return GetWordAtOffset(offset);
    }

    /// <summary>
    /// Checks if the word at the specified offset is misspelled.
    /// </summary>
    public bool IsWordMisspelledAtOffset(int offset)
    {
        var word = GetWordAtOffset(offset);
        if (word == null || string.IsNullOrWhiteSpace(word.Text))
        {
            return false;
        }

        if (!IsSpellCheckEnabled || _spellCheckTransformer == null)
        {
            return false;
        }

        return _spellCheckTransformer.IsWordMisspelled(word.Text);
    }

    /// <summary>
    /// Gets spelling suggestions for the word at the specified offset.
    /// </summary>
    public System.Collections.Generic.List<string>? GetSuggestionsForWordAtOffset(int offset)
    {
        var word = GetWordAtOffset(offset);
        if (word == null || string.IsNullOrWhiteSpace(word.Text))
        {
            return null;
        }

        return _spellCheckTransformer.GetSuggestions(word.Text);
    }
}
