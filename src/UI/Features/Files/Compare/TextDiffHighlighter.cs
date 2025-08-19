using Avalonia.Controls;
using Avalonia.Media;
using System;
using Avalonia.Controls.Documents;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public class TextDiffHighlighter
{
    private static readonly IBrush ForegroundDifferenceColor = Brushes.Red;
    private static readonly IBrush BackDifferenceColor = Brushes.Yellow;
    private static readonly IBrush NormalColor = Brushes.Black;

    public static TextBlock BuildDiffBlock(string text1, string text2)
    {
        var tb = new TextBlock { TextWrapping = Avalonia.Media.TextWrapping.Wrap };
        int startCharactersOk = 0;
        int minLength = Math.Min(text1.Length, text2.Length);

        // from start
        for (int i = 0; i < minLength; i++)
        {
            if (text1[i] == text2[i])
            {
                tb.Inlines.Add(new Run(text1[i].ToString()) { Foreground = NormalColor });
                startCharactersOk++;
            }
            else
            {
                tb.Inlines.Add(new Run(text1[i].ToString())
                {
                    Foreground = ForegroundDifferenceColor,
                    Background = " .,".Contains(text1[i]) ? BackDifferenceColor : null
                });
                startCharactersOk++;
                break; // stop after first mismatch (similar to your code)
            }
        }

        // middle differences
        for (int i = startCharactersOk; i < text1.Length; i++)
        {
            tb.Inlines.Add(new Run(text1[i].ToString())
            {
                Foreground = ForegroundDifferenceColor,
                Background = " .,".Contains(text1[i]) ? BackDifferenceColor : null
            });
        }

        return tb;
    }

    public static (TextBlock left, TextBlock right) Compare(string text1, string text2)
    {
        var left = new TextBlock { TextWrapping = Avalonia.Media.TextWrapping.Wrap };
        var right = new TextBlock { TextWrapping = Avalonia.Media.TextWrapping.Wrap };

        int startCharactersOk = 0;
        int minLength = Math.Min(text1.Length, text2.Length);

        // from start
        for (int i = 0; i < minLength; i++)
        {
            if (text1[i] == text2[i])
            {
                var run1 = new Run(text1[i].ToString()) { Foreground = NormalColor };
                var run2 = new Run(text2[i].ToString()) { Foreground = NormalColor };
                left.Inlines.Add(run1);
                right.Inlines.Add(run2);
                startCharactersOk++;
            }
            else
            {
                var run1 = new Run(text1[i].ToString())
                {
                    Foreground = ForegroundDifferenceColor,
                    Background = " .,".Contains(text1[i]) ? BackDifferenceColor : null
                };
                var run2 = new Run(text2[i].ToString())
                {
                    Foreground = ForegroundDifferenceColor,
                    Background = " .,".Contains(text2[i]) ? BackDifferenceColor : null
                };
                left.Inlines.Add(run1);
                right.Inlines.Add(run2);
                startCharactersOk++;
                break;
            }
        }

        // remaining characters in text1
        for (int i = startCharactersOk; i < text1.Length; i++)
        {
            left.Inlines.Add(new Run(text1[i].ToString())
            {
                Foreground = ForegroundDifferenceColor,
                Background = " .,".Contains(text1[i]) ? BackDifferenceColor : null
            });
        }

        // remaining characters in text2
        for (int i = startCharactersOk; i < text2.Length; i++)
        {
            right.Inlines.Add(new Run(text2[i].ToString())
            {
                Foreground = ForegroundDifferenceColor,
                Background = " .,".Contains(text2[i]) ? BackDifferenceColor : null
            });
        }

        return (left, right);
    }
}
