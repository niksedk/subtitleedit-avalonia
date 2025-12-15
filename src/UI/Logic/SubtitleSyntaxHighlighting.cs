using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;

namespace Nikse.SubtitleEdit.Logic;

public partial class SubtitleSyntaxHighlighting : DocumentColorizingTransformer
{
    // Pastel color scheme for HTML and ASS/SSA syntax highlighting
    private static readonly Color ElementColor = Color.FromRgb(183, 89, 155);    // Soft purple - HTML element tags (e.g., <div>, <span>) / ASS tag names
    private static readonly Color AttributeColor = Color.FromRgb(86, 156, 214);  // Soft blue - HTML attribute names (e.g., class, id, style)
    private static readonly Color CommentColor = Color.FromRgb(106, 153, 85);    // Soft green - HTML comments (<!-- -->)
    private static readonly Color CharsColor = Color.FromRgb(128, 128, 128);     // Gray - delimiters and special chars (<, >, ", ', =, {, }, \)
    private static readonly Color ValuesColor = Color.FromRgb(206, 145, 120);    // Soft orange/peach - attribute values (e.g., "myclass") / ASS tag values
    private static readonly Color StyleColor = Color.FromRgb(156, 220, 254);     // Light cyan - CSS property values in style attribute

    protected override void ColorizeLine(DocumentLine line)
    {
        var lineStartOffset = line.Offset;
        var text = CurrentContext.Document.GetText(line);

        var inComment = false;
        var inHtmlTag = false;
        var inAttributeVal = false;
        var quoteChar = '\0';

        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            var c2 = i + 1 < text.Length ? text[i + 1] : '\0';

            // Handle ASS/SSA tags: {\tag} or {\tagValue}
            if (c == '{' && c2 == '\\')
            {
                var tagEnd = text.IndexOf('}', i + 2);
                if (tagEnd != -1)
                {
                    // Color opening brace and backslash
                    ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 2, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                    });

                    // Find where the tag name ends (before any numbers or special chars)
                    var tagNameStart = i + 2;
                    var tagNameEnd = tagNameStart;
                    while (tagNameEnd < tagEnd && char.IsLetter(text[tagNameEnd]))
                    {
                        tagNameEnd++;
                    }

                    // Color tag name (e.g., "i", "b", "fn", "fs", "c", "1c", etc.)
                    if (tagNameEnd > tagNameStart)
                    {
                        ChangeLinePart(lineStartOffset + tagNameStart, lineStartOffset + tagNameEnd, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(ElementColor));
                        });
                    }

                    // Color tag value/parameters (e.g., "1", "Arial", "&HFFFFFF&")
                    if (tagNameEnd < tagEnd)
                    {
                        ChangeLinePart(lineStartOffset + tagNameEnd, lineStartOffset + tagEnd, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(ValuesColor));
                        });
                    }

                    // Color closing brace
                    ChangeLinePart(lineStartOffset + tagEnd, lineStartOffset + tagEnd + 1, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                    });

                    i = tagEnd;
                    continue;
                }
            }

            if (!inComment && c == '<')
            {
                if (i + 3 < text.Length && c2 == '!' && text[i + 2] == '-' && text[i + 3] == '-')
                {
                    // Comment start: <!--
                    var commentEnd = text.IndexOf("-->", i + 4, StringComparison.Ordinal);
                    var commentLength = commentEnd != -1 ? commentEnd + 3 - i : text.Length - i;
                    ChangeLinePart(lineStartOffset + i, lineStartOffset + i + commentLength, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CommentColor));
                    });
                    i += commentLength - 1;
                    continue;
                }
                else
                {
                    // HTML tag start
                    ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 1, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                    });

                    if (c2 == '/')
                    {
                        // Closing tag
                        ChangeLinePart(lineStartOffset + i + 1, lineStartOffset + i + 2, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                        });
                        i++;
                    }

                    inHtmlTag = true;

                    // Find element name end
                    var elementStart = i + 1;
                    if (elementStart >= text.Length)
                    {
                        continue;
                    }

                    if (text[elementStart] == '/')
                    {
                        elementStart++;
                    }

                    var elementEnd = elementStart;
                    while (elementEnd < text.Length && !char.IsWhiteSpace(text[elementEnd]) &&
                           text[elementEnd] != '>' && text[elementEnd] != '/')
                    {
                        elementEnd++;
                    }

                    if (elementEnd > elementStart)
                    {
                        ChangeLinePart(lineStartOffset + elementStart, lineStartOffset + elementEnd, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(ElementColor));
                        });
                        i = elementEnd - 1;
                    }
                }
            }
            else if (inHtmlTag && c == '>')
            {
                // HTML tag end
                ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 1, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                });
                inHtmlTag = false;
                inAttributeVal = false;
                quoteChar = '\0';
            }
            else if (inHtmlTag && c == '/' && c2 == '>')
            {
                // Self-closing tag
                ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 2, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                });
                inHtmlTag = false;
                inAttributeVal = false;
                quoteChar = '\0';
                i++;
            }
            else if (inHtmlTag && !inAttributeVal && char.IsLetter(c))
            {
                // Attribute name
                var attrStart = i;
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '-' || text[i] == '_'))
                {
                    i++;
                }

                ChangeLinePart(lineStartOffset + attrStart, lineStartOffset + i, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(AttributeColor));
                });
                i--;
            }
            else if (inHtmlTag && c == '=')
            {
                // Equals sign
                ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 1, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                });
            }
            else if (inHtmlTag && (c == '"' || c == '\''))
            {
                if (!inAttributeVal)
                {
                    // Start of attribute value
                    quoteChar = c;
                    inAttributeVal = true;
                    var valueStart = i;
                    var valueEnd = text.IndexOf(quoteChar, i + 1);
                    if (valueEnd == -1)
                    {
                        valueEnd = text.Length;
                    }
                    else
                    {
                        valueEnd++;
                    }

                    // Color the quotes
                    ChangeLinePart(lineStartOffset + valueStart, lineStartOffset + valueStart + 1, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                    });

                    // Color the value content (check for style attribute)
                    var hasColon = text.IndexOf(':', i + 1, valueEnd - i - 2) != -1;
                    var valueColor = hasColon ? StyleColor : ValuesColor;

                    if (valueEnd > valueStart + 1)
                    {
                        ChangeLinePart(lineStartOffset + valueStart + 1, lineStartOffset + valueEnd - 1, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(valueColor));
                        });

                        // Color closing quote
                        ChangeLinePart(lineStartOffset + valueEnd - 1, lineStartOffset + valueEnd, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                        });
                    }

                    i = valueEnd - 1;
                    inAttributeVal = false;
                    quoteChar = '\0';
                }
            }
        }
    }
}
