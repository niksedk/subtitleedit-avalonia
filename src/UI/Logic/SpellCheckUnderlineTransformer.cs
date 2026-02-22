using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public class SpellCheckUnderlineTransformer : DocumentColorizingTransformer
{
    private ISpellCheckManager? _spellCheckManager;
    private bool _isEnabled;
    private TextView? _textView;

    private static readonly Color ErrorColor = Colors.Red;
    private static readonly TextDecoration WavyUnderline = new()
    {
        Location = TextDecorationLocation.Underline,
        Stroke = new SolidColorBrush(ErrorColor),
        StrokeThickness = 1.5,
        StrokeLineCap = PenLineCap.Round,
        StrokeThicknessUnit = TextDecorationUnit.FontRecommended
    };

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnEnabledChanged();
            }
        }
    }

    public void SetTextView(TextView textView)
    {
        _textView = textView;
    }

    public void SetSpellCheckManager(ISpellCheckManager? spellCheckManager)
    {
        _spellCheckManager = spellCheckManager;
        Refresh();
    }

    public void Refresh()
    {
        _textView?.Redraw();
    }

    private void OnEnabledChanged()
    {
        _textView?.Redraw();
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (!_isEnabled || _spellCheckManager == null || line.Length == 0)
        {
            return;
        }

        try
        {
            var lineText = CurrentContext.Document.GetText(line.Offset, line.Length);
            if (string.IsNullOrWhiteSpace(lineText))
            {
                return;
            }

            var words = SpellCheckWordLists2.Split(lineText);
            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word.Text) || word.Length < 2)
                {
                    continue;
                }

                // Skip words that are numbers, URLs, or special patterns
                if (IsSpecialPattern(word.Text))
                {
                    continue;
                }

                // Check if word is correct
                if (!_spellCheckManager.IsWordCorrect(word.Text))
                {
                    var startOffset = line.Offset + word.Index;
                    var endOffset = startOffset + word.Length;

                    // Apply wavy underline to misspelled word
                    ChangeLinePart(startOffset, endOffset, element =>
                    {
                        if (element.TextRunProperties.TextDecorations == null)
                        {
                            element.TextRunProperties.SetTextDecorations(new TextDecorationCollection { WavyUnderline });
                        }
                        else
                        {
                            var decorations = new List<TextDecoration>(element.TextRunProperties.TextDecorations)
                            {
                                WavyUnderline
                            };
                            element.TextRunProperties.SetTextDecorations(new TextDecorationCollection(decorations));
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex);
        }
    }

    private static bool IsSpecialPattern(string text)
    {
        // Skip numbers
        if (text.All(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-'))
        {
            return true;
        }

        // Skip URLs
        if (text.Contains("http://", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("https://", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("www.", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Skip email-like patterns
        if (text.Contains('@'))
        {
            return true;
        }

        // Skip hashtags
        if (text.StartsWith('#'))
        {
            return true;
        }

        // Skip if all uppercase (might be acronym)
        if (text.Length > 1 && text.All(c => char.IsUpper(c) || !char.IsLetter(c)))
        {
            return true;
        }

        return false;
    }
}
