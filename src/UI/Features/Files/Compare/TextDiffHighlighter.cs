using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using System;

public static class TextDiffHighlighter
{
    private static readonly IBrush ForegroundDifferenceColor = Brushes.Red;
    private static readonly IBrush BackDifferenceColor = Brushes.Yellow;
    private static readonly IBrush NormalColor = Brushes.Black;
    private static readonly IBrush DiffBackgroundColor = Brushes.LightGreen;

    public static (TextBlock left, TextBlock right) Compare(string text1, string text2)
    {
        var left = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var right = new TextBlock { TextWrapping = TextWrapping.Wrap };

        if (left.Inlines == null || right.Inlines == null)
        {
            return (left, right);
        }

        bool hasDifferences = false;

        // One string is a pure prefix of the other
        if (text1.StartsWith(text2, StringComparison.Ordinal))
        {
            left.Inlines.Add(new Run(text2)
            {
                Foreground = NormalColor
            });
            left.Inlines.Add(new Run(text1.Substring(text2.Length))
            {
                Foreground = ForegroundDifferenceColor,
                Background = BackDifferenceColor
            });

            right.Inlines.Add(new Run(text2)
            {
                Foreground = NormalColor
            });

            hasDifferences = true;
        }
        else if (text2.StartsWith(text1, StringComparison.Ordinal))
        {
            right.Inlines.Add(new Run(text1)
            {
                Foreground = NormalColor
            });
            right.Inlines.Add(new Run(text2.Substring(text1.Length))
            {
                Foreground = ForegroundDifferenceColor,
                Background = BackDifferenceColor
            });

            left.Inlines.Add(new Run(text1)
            {
                Foreground = NormalColor
            });

            hasDifferences = true;
        }
        else
        {
            // Diff from start
            int startOk = 0;
            int minLength = Math.Min(text1.Length, text2.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (text1[i] == text2[i])
                {
                    left.Inlines.Add(new Run(text1[i].ToString())
                    {
                        Foreground = NormalColor
                    });
                    right.Inlines.Add(new Run(text2[i].ToString())
                    {
                        Foreground = NormalColor
                    });
                    startOk++;
                }
                else
                {
                    break;
                }
            }

            // Diff from end
            int endOk = 0;
            while (endOk < minLength - startOk &&
                   text1[text1.Length - 1 - endOk] == text2[text2.Length - 1 - endOk])
            {
                endOk++;
            }

            // Middle difference (per character)
            string mid1 = text1.Substring(startOk, text1.Length - startOk - endOk);
            string mid2 = text2.Substring(startOk, text2.Length - startOk - endOk);

            int maxMid = Math.Max(mid1.Length, mid2.Length);
            for (int i = 0; i < maxMid; i++)
            {
                char? c1 = i < mid1.Length ? mid1[i] : (char?)null;
                char? c2 = i < mid2.Length ? mid2[i] : (char?)null;

                if (c1 == c2 && c1 != null)
                {
                    left.Inlines.Add(new Run(c1.ToString())
                    {
                        Foreground = NormalColor
                    });
                    right.Inlines.Add(new Run(c2!.ToString())
                    {
                        Foreground = NormalColor
                    });
                }
                else
                {
                    if (c1 != null)
                    {
                        left.Inlines.Add(new Run(c1.ToString())
                        {
                            Foreground = ForegroundDifferenceColor,
                            Background = BackDifferenceColor
                        });
                        hasDifferences = true;
                    }
                    if (c2 != null)
                    {
                        right.Inlines.Add(new Run(c2.ToString())
                        {
                            Foreground = ForegroundDifferenceColor,
                            Background = BackDifferenceColor
                        });
                        hasDifferences = true;
                    }
                }
            }

            // Add suffix (if any)
            if (endOk > 0)
            {
                string suffix = text1.Substring(text1.Length - endOk);
                left.Inlines.Add(new Run(suffix)
                {
                    Foreground = NormalColor
                });

                suffix = text2.Substring(text2.Length - endOk);
                right.Inlines.Add(new Run(suffix)
                {
                    Foreground = NormalColor
                });
            }
        }

        // Apply green background only to normal text if differences exist
        if (hasDifferences)
        {
            foreach (var run in left.Inlines)
            {
                if (run is Run r && r.Background == null)
                {
                    r.Background = DiffBackgroundColor;
                }
            }
            foreach (var run in right.Inlines)
            {
                if (run is Run r && r.Background == null)
                {
                    r.Background = DiffBackgroundColor;
                }
            }
        }

        return (left, right);
    }
}
