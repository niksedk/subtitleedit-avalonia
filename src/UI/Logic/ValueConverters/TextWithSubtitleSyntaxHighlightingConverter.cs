using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TextWithSubtitleSyntaxHighlightingConverter : IValueConverter
{
    // Pastel color scheme for HTML and ASS/SSA syntax highlighting
    private static readonly Color ElementColor = Color.FromRgb(183, 89, 155);    // Soft purple - HTML element tags / ASS tag names
    private static readonly Color AttributeColor = Color.FromRgb(86, 156, 214);  // Soft blue - HTML attribute names
    private static readonly Color CommentColor = Color.FromRgb(106, 153, 85);    // Soft green - HTML comments
    private static readonly Color CharsColor = Color.FromRgb(128, 128, 128);     // Gray - delimiters and special chars
    private static readonly Color ValuesColor = Color.FromRgb(206, 145, 120);    // Soft orange/peach - attribute values / ASS tag values
    private static readonly Color StyleColor = Color.FromRgb(156, 220, 254);     // Light cyan - CSS property values

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str)
        {
            return new InlineCollection();
        }

        var inlines = new InlineCollection();
        if (string.IsNullOrEmpty(str))
        {
            return inlines;
        }

        if (Se.Settings.Appearance.SubtitleGridTextSingleLine)
        {
            var separator = Se.Settings.Appearance.SubtitleGridTextSingleLineSeparator;
            str = str
                .Replace("\r\n", separator)
                .Replace("\n", separator);
        }

        // Truncate long strings for performance
        if (str.Length > 500)
        {
            str = str.Substring(0, 100).TrimEnd() + "...";
        }

        int i = 0;
        while (i < str.Length)
        {
            var c = str[i];
            var c2 = i + 1 < str.Length ? str[i + 1] : '\0';

            // Handle ASS/SSA tags: {\tag} or {\tagValue}
            if (c == '{' && c2 == '\\')
            {
                var tagEnd = str.IndexOf('}', i + 2);
                if (tagEnd != -1)
                {
                    // Add opening brace and backslash
                    inlines.Add(new Run(str.Substring(i, 2)) { Foreground = new SolidColorBrush(CharsColor) });
                    i += 2;

                    // Find where the tag name ends
                    var tagNameStart = i;
                    var tagNameEnd = tagNameStart;
                    while (tagNameEnd < tagEnd && char.IsLetter(str[tagNameEnd]))
                    {
                        tagNameEnd++;
                    }

                    // Add tag name
                    if (tagNameEnd > tagNameStart)
                    {
                        inlines.Add(new Run(str.Substring(tagNameStart, tagNameEnd - tagNameStart))
                        {
                            Foreground = new SolidColorBrush(ElementColor)
                        });
                    }

                    // Add tag value/parameters
                    if (tagNameEnd < tagEnd)
                    {
                        inlines.Add(new Run(str.Substring(tagNameEnd, tagEnd - tagNameEnd))
                        {
                            Foreground = new SolidColorBrush(ValuesColor)
                        });
                    }

                    // Add closing brace
                    inlines.Add(new Run("}") { Foreground = new SolidColorBrush(CharsColor) });
                    i = tagEnd + 1;
                    continue;
                }
                else
                {
                    // Malformed ASS/SSA tag - treat as regular text
                    inlines.Add(new Run(c.ToString()));
                    i++;
                    continue;
                }
            }

            // Handle HTML comments
            if (c == '<' && i + 3 < str.Length && c2 == '!' && str[i + 2] == '-' && str[i + 3] == '-')
            {
                var commentEnd = str.IndexOf("-->", i + 4, StringComparison.Ordinal);
                var commentLength = commentEnd != -1 ? commentEnd + 3 - i : str.Length - i;
                inlines.Add(new Run(str.Substring(i, commentLength))
                {
                    Foreground = new SolidColorBrush(CommentColor)
                });
                i += commentLength;
                continue;
            }

            // Handle HTML tags
            if (c == '<')
            {
                var tagEnd = str.IndexOf('>', i + 1);
                if (tagEnd != -1)
                {
                    var tagContent = str.Substring(i, tagEnd - i + 1);
                    ParseHtmlTag(inlines, tagContent);
                    i = tagEnd + 1;
                    continue;
                }
                else
                {
                    // Malformed HTML tag - treat as regular text
                    inlines.Add(new Run(c.ToString()));
                    i++;
                    continue;
                }
            }

            // Handle line breaks
            if (c == '\n' || (c == '\r' && c2 == '\n'))
            {
                inlines.Add(new Run(Environment.NewLine));
                i += c == '\r' ? 2 : 1;
                continue;
            }

            // Regular text - add character by character until we hit special markup
            var textStart = i;
            while (i < str.Length && str[i] != '<' && str[i] != '{' && str[i] != '\r' && str[i] != '\n')
            {
                i++;
            }

            if (i > textStart)
            {
                inlines.Add(new Run(str.Substring(textStart, i - textStart)));
            }
            else
            {
                // Safety: if we didn't match any condition and haven't advanced, treat as regular character
                inlines.Add(new Run(str[i].ToString()));
                i++;
            }
        }

        return inlines;
    }

    private static void ParseHtmlTag(InlineCollection inlines, string tagContent)
    {
        // Add opening <
        inlines.Add(new Run("<") { Foreground = new SolidColorBrush(CharsColor) });

        var content = tagContent.Substring(1, tagContent.Length - 2); // Remove < and >
        var isClosingTag = content.StartsWith('/');
        if (isClosingTag)
        {
            inlines.Add(new Run("/") { Foreground = new SolidColorBrush(CharsColor) });
            content = content.Substring(1);
        }

        var isSelfClosing = content.EndsWith('/');
        if (isSelfClosing)
        {
            content = content.Substring(0, content.Length - 1).TrimEnd();
        }

        // Find element name
        var spaceIndex = content.IndexOf(' ');
        var elementName = spaceIndex > 0 ? content.Substring(0, spaceIndex) : content;

        // Add element name
        inlines.Add(new Run(elementName) { Foreground = new SolidColorBrush(ElementColor) });

        // Parse attributes if any
        if (spaceIndex > 0)
        {
            var attributesPart = content.Substring(spaceIndex);
            ParseHtmlAttributes(inlines, attributesPart);
        }

        // Add self-closing /
        if (isSelfClosing)
        {
            inlines.Add(new Run(" /") { Foreground = new SolidColorBrush(CharsColor) });
        }

        // Add closing >
        inlines.Add(new Run(">") { Foreground = new SolidColorBrush(CharsColor) });
    }

    private static void ParseHtmlAttributes(InlineCollection inlines, string attributesPart)
    {
        int i = 0;
        while (i < attributesPart.Length)
        {
            // Skip whitespace
            while (i < attributesPart.Length && char.IsWhiteSpace(attributesPart[i]))
            {
                inlines.Add(new Run(attributesPart[i].ToString()));
                i++;
            }

            if (i >= attributesPart.Length)
                break;

            // Read attribute name
            var attrStart = i;
            while (i < attributesPart.Length && (char.IsLetterOrDigit(attributesPart[i]) || attributesPart[i] == '-' || attributesPart[i] == '_'))
            {
                i++;
            }

            if (i > attrStart)
            {
                inlines.Add(new Run(attributesPart.Substring(attrStart, i - attrStart))
                {
                    Foreground = new SolidColorBrush(AttributeColor)
                });
            }
            else if (i < attributesPart.Length)
            {
                // Unexpected character - add it and move on to prevent infinite loop
                inlines.Add(new Run(attributesPart[i].ToString()));
                i++;
            }

            // Skip whitespace
            while (i < attributesPart.Length && char.IsWhiteSpace(attributesPart[i]))
            {
                inlines.Add(new Run(attributesPart[i].ToString()));
                i++;
            }

            // Check for =
            if (i < attributesPart.Length && attributesPart[i] == '=')
            {
                inlines.Add(new Run("=") { Foreground = new SolidColorBrush(CharsColor) });
                i++;

                // Skip whitespace
                while (i < attributesPart.Length && char.IsWhiteSpace(attributesPart[i]))
                {
                    inlines.Add(new Run(attributesPart[i].ToString()));
                    i++;
                }

                // Read attribute value (quoted)
                if (i < attributesPart.Length && (attributesPart[i] == '"' || attributesPart[i] == '\''))
                {
                    var quote = attributesPart[i];
                    inlines.Add(new Run(quote.ToString()) { Foreground = new SolidColorBrush(CharsColor) });
                    i++;

                    var valueStart = i;
                    while (i < attributesPart.Length && attributesPart[i] != quote)
                    {
                        i++;
                    }

                    if (i > valueStart)
                    {
                        var value = attributesPart.Substring(valueStart, i - valueStart);
                        var hasColon = value.Contains(':');
                        inlines.Add(new Run(value)
                        {
                            Foreground = new SolidColorBrush(hasColon ? StyleColor : ValuesColor)
                        });
                    }

                    if (i < attributesPart.Length)
                    {
                        inlines.Add(new Run(quote.ToString()) { Foreground = new SolidColorBrush(CharsColor) });
                        i++;
                    }
                }
            }
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}