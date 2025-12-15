using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

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

    // Reuse brushes instead of creating new ones each time
    private static readonly SolidColorBrush ElementBrush = new(ElementColor);
    private static readonly SolidColorBrush AttributeBrush = new(AttributeColor);
    private static readonly SolidColorBrush CommentBrush = new(CommentColor);
    private static readonly SolidColorBrush CharsBrush = new(CharsColor);
    private static readonly SolidColorBrush ValuesBrush = new(ValuesColor);
    private static readonly SolidColorBrush StyleBrush = new(StyleColor);

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
        if (str.Length > 200)
        {
            str = str.Substring(0, 197).TrimEnd() + "...";
        }

        if (Se.Settings.Appearance.SubtitleGridFormattingType == (int)SubtitleGridFormattingTypes.ShowFormatting)
        {
            return MakeShowFormatting(inlines, str);
        }

        if (Se.Settings.Appearance.SubtitleGridFormattingType == (int)SubtitleGridFormattingTypes.ShowTags)
        {
            return MakeShowTags(str, inlines);
        }

        // No formatting (default)
        inlines.Add(new Run(str));
        return inlines;
    }

    private static InlineCollection MakeShowTags(string str, InlineCollection inlines)
    {
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
                    inlines.Add(new Run(str.Substring(i, 2)) { Foreground = CharsBrush });
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
                            Foreground = ElementBrush
                        });
                    }

                    // Add tag value/parameters
                    if (tagNameEnd < tagEnd)
                    {
                        inlines.Add(new Run(str.Substring(tagNameEnd, tagEnd - tagNameEnd))
                        {
                            Foreground = ValuesBrush
                        });
                    }

                    // Add closing brace
                    inlines.Add(new Run("}") { Foreground = CharsBrush });
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
                    Foreground = CommentBrush
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

    private static InlineCollection MakeShowFormatting(InlineCollection inlines, string str)
    {
        // Truncate long strings for performance
        if (str.Length > 500)
        {
            str = str.Substring(0, 100).TrimEnd() + "...";
        }

        // Track current formatting state
        var state = new FormattingState();
        var stateStack = new Stack<FormattingState>();

        int i = 0;
        while (i < str.Length)
        {
            var c = str[i];
            var c2 = i + 1 < str.Length ? str[i + 1] : '\0';

            // Handle ASS/SSA tags: {\tag} or {\tag1\tag2}
            if (c == '{' && c2 == '\\')
            {
                var tagEnd = str.IndexOf('}', i + 2);
                if (tagEnd != -1)
                {
                    var tagContent = str.Substring(i + 1, tagEnd - i - 1); // Content between { and }
                    ParseAssaTags(tagContent, state);
                    i = tagEnd + 1;
                    continue;
                }
            }

            // Handle HTML comments - skip them
            if (c == '<' && i + 3 < str.Length && c2 == '!' && str[i + 2] == '-' && str[i + 3] == '-')
            {
                var commentEnd = str.IndexOf("-->", i + 4, StringComparison.Ordinal);
                i = commentEnd != -1 ? commentEnd + 3 : str.Length;
                continue;
            }

            // Handle HTML tags
            if (c == '<')
            {
                var tagEnd = str.IndexOf('>', i + 1);
                if (tagEnd != -1)
                {
                    var tagContent = str.Substring(i + 1, tagEnd - i - 1).Trim();
                    var isClosingTag = tagContent.StartsWith('/');
                    if (isClosingTag)
                    {
                        tagContent = tagContent.Substring(1).Trim();
                    }

                    var tagName = tagContent.Split(' ')[0].ToLowerInvariant();

                    if (isClosingTag)
                    {
                        // Handle closing tags
                        switch (tagName)
                        {
                            case "i":
                                state.Italic = false;
                                break;
                            case "b":
                                state.Bold = false;
                                break;
                            case "u":
                                state.Underline = false;
                                break;
                            case "font":
                                state.Color = null;
                                state.FontName = null;
                                state.FontSize = null;
                                break;
                        }
                    }
                    else
                    {
                        // Handle opening tags
                        switch (tagName)
                        {
                            case "i":
                                state.Italic = true;
                                break;
                            case "b":
                                state.Bold = true;
                                break;
                            case "u":
                                state.Underline = true;
                                break;
                            case "font":
                                ParseFontTag(tagContent, state);
                                break;
                        }
                    }
                    i = tagEnd + 1;
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

            // Regular text - collect characters until we hit special markup
            var textStart = i;
            while (i < str.Length && str[i] != '<' && str[i] != '{' && str[i] != '\r' && str[i] != '\n')
            {
                // Check for ASS tag start
                if (str[i] == '{' && i + 1 < str.Length && str[i + 1] == '\\')
                {
                    break;
                }
                if (str[i] == '{')
                {
                    i++;
                    continue;
                }
                i++;
            }

            if (i > textStart)
            {
                var text = str.Substring(textStart, i - textStart);
                var run = CreateFormattedRun(text, state);
                inlines.Add(run);
            }
            else if (i < str.Length && str[i] != '<' && str[i] != '{')
            {
                // Safety: add single character if we haven't advanced
                var run = CreateFormattedRun(str[i].ToString(), state);
                inlines.Add(run);
                i++;
            }
        }

        return inlines;
    }

    private static Run CreateFormattedRun(string text, FormattingState state)
    {
        var run = new Run(text);

        // Apply italic
        if (state.Italic)
        {
            run.FontStyle = FontStyle.Italic;
        }

        // Apply bold
        if (state.Bold)
        {
            run.FontWeight = FontWeight.Bold;
        }

        // Apply underline
        if (state.Underline)
        {
            run.TextDecorations = TextDecorations.Underline;
        }

        // Apply color
        if (state.Color.HasValue)
        {
            run.Foreground = new SolidColorBrush(state.Color.Value);
        }

        // Apply font name
        if (!string.IsNullOrEmpty(state.FontName))
        {
            run.FontFamily = new FontFamily(state.FontName);
        }

        // Apply font size
        if (state.FontSize.HasValue && state.FontSize.Value > 0)
        {
            // Scale font size relative to default (assuming default is around 12-14pt)
            // We'll cap it to reasonable display sizes
            var fontSize = Math.Min(Math.Max(state.FontSize.Value * 0.8, 8), 24);
            run.FontSize = fontSize;
        }

        return run;
    }

    private static void ParseFontTag(string tagContent, FormattingState state)
    {
        // Parse color attribute
        var colorMatch = Regex.Match(tagContent, @"color\s*=\s*[""']?([^""'\s>]+)[""']?", RegexOptions.IgnoreCase);
        if (colorMatch.Success)
        {
            var colorValue = colorMatch.Groups[1].Value;
            try
            {
                var skColor = HtmlUtil.GetColorFromString(colorValue);
                state.Color = Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
            }
            catch
            {
                // Ignore invalid colors
            }
        }

        // Parse face (font name) attribute
        var faceMatch = Regex.Match(tagContent, @"face\s*=\s*[""']?([^""'>]+)[""']?", RegexOptions.IgnoreCase);
        if (faceMatch.Success)
        {
            state.FontName = faceMatch.Groups[1].Value.Trim();
        }

        // Parse size attribute
        var sizeMatch = Regex.Match(tagContent, @"size\s*=\s*[""']?(\d+)[""']?", RegexOptions.IgnoreCase);
        if (sizeMatch.Success && double.TryParse(sizeMatch.Groups[1].Value, out var size))
        {
            state.FontSize = size;
        }
    }

    private static void ParseAssaTags(string tagContent, FormattingState state)
    {
        // Split by backslash to get individual tags
        var tags = tagContent.Split('\\', StringSplitOptions.RemoveEmptyEntries);

        foreach (var tag in tags)
        {
            var trimmedTag = tag.Trim();
            if (string.IsNullOrEmpty(trimmedTag))
            {
                continue;
            }

            // Italic: \i1 or \i0
            if (trimmedTag.StartsWith("i", StringComparison.OrdinalIgnoreCase) && trimmedTag.Length >= 2)
            {
                state.Italic = trimmedTag[1] == '1';
            }
            // Bold: \b1 or \b0 (can also be \b700 for weight)
            else if (trimmedTag.StartsWith("b", StringComparison.OrdinalIgnoreCase) && trimmedTag.Length >= 2 && char.IsDigit(trimmedTag[1]))
            {
                var value = trimmedTag.Substring(1);
                if (value == "0")
                {
                    state.Bold = false;
                }
                else if (value == "1")
                {
                    state.Bold = true;
                }
                else if (int.TryParse(value, out var weight))
                {
                    state.Bold = weight >= 700;
                }
            }
            // Underline: \u1 or \u0
            else if (trimmedTag.StartsWith("u", StringComparison.OrdinalIgnoreCase) && trimmedTag.Length >= 2 && char.IsDigit(trimmedTag[1]))
            {
                state.Underline = trimmedTag[1] == '1';
            }
            // Font name: \fnFontName
            else if (trimmedTag.StartsWith("fn", StringComparison.OrdinalIgnoreCase) && trimmedTag.Length > 2)
            {
                state.FontName = trimmedTag.Substring(2).Trim();
            }
            // Font size: \fs20
            else if (trimmedTag.StartsWith("fs", StringComparison.OrdinalIgnoreCase) && trimmedTag.Length > 2)
            {
                if (double.TryParse(trimmedTag.Substring(2), NumberStyles.Any, CultureInfo.InvariantCulture, out var size))
                {
                    state.FontSize = size;
                }
            }
            // Primary color: \c&HBBGGRR& or \1c&HBBGGRR&
            else if ((trimmedTag.StartsWith("c&", StringComparison.OrdinalIgnoreCase) ||
                      trimmedTag.StartsWith("1c&", StringComparison.OrdinalIgnoreCase)))
            {
                var colorStr = trimmedTag.StartsWith("1c") ? trimmedTag.Substring(2) : trimmedTag.Substring(1);
                state.Color = ParseAssaColor(colorStr);
            }
            // Primary color alternative: \c&H or just color value
            else if (trimmedTag.Length > 1 && trimmedTag[0] == 'c' && !char.IsDigit(trimmedTag[1]))
            {
                state.Color = ParseAssaColor(trimmedTag.Substring(1));
            }
            // Reset: \r - reset all formatting
            else if (trimmedTag == "r" || trimmedTag.StartsWith("r", StringComparison.OrdinalIgnoreCase) && trimmedTag.Length == 1)
            {
                state.Reset();
            }
        }
    }

    private static Color? ParseAssaColor(string colorStr)
    {
        // ASSA colors are in format &HAABBGGRR& or &HBBGGRR& (BGR order, not RGB)
        colorStr = colorStr.Trim('&', 'H', 'h');

        if (string.IsNullOrEmpty(colorStr))
        {
            return null;
        }

        try
        {
            // Pad to 8 characters if needed (add alpha)
            if (colorStr.Length == 6)
            {
                colorStr = "00" + colorStr; // Add alpha = 0 (fully opaque in ASSA)
            }

            if (colorStr.Length >= 6)
            {
                // ASSA format: AABBGGRR
                var blue = System.Convert.ToByte(colorStr.Substring(colorStr.Length - 6, 2), 16);
                var green = System.Convert.ToByte(colorStr.Substring(colorStr.Length - 4, 2), 16);
                var red = System.Convert.ToByte(colorStr.Substring(colorStr.Length - 2, 2), 16);
                byte alpha = 255;
                if (colorStr.Length >= 8)
                {
                    var alphaValue = System.Convert.ToByte(colorStr.Substring(0, 2), 16);
                    alpha = (byte)(255 - alphaValue); // ASSA alpha is inverted (0 = opaque, 255 = transparent)
                }
                return Color.FromArgb(alpha, red, green, blue);
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return null;
    }

    private class FormattingState
    {
        public bool Italic { get; set; }
        public bool Bold { get; set; }
        public bool Underline { get; set; }
        public Color? Color { get; set; }
        public string? FontName { get; set; }
        public double? FontSize { get; set; }

        public void Reset()
        {
            Italic = false;
            Bold = false;
            Underline = false;
            Color = null;
            FontName = null;
            FontSize = null;
        }

        public FormattingState Clone()
        {
            return new FormattingState
            {
                Italic = Italic,
                Bold = Bold,
                Underline = Underline,
                Color = Color,
                FontName = FontName,
                FontSize = FontSize
            };
        }
    }

    private static void ParseHtmlTag(InlineCollection inlines, string tagContent)
    {
        // Add opening <
        inlines.Add(new Run("<") { Foreground = CharsBrush });

        var content = tagContent.Substring(1, tagContent.Length - 2); // Remove < and >
        var isClosingTag = content.StartsWith('/');
        if (isClosingTag)
        {
            inlines.Add(new Run("/") { Foreground = CharsBrush });
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
        inlines.Add(new Run(elementName) { Foreground = ElementBrush });

        // Parse attributes if any
        if (spaceIndex > 0)
        {
            var attributesPart = content.Substring(spaceIndex);
            ParseHtmlAttributes(inlines, attributesPart);
        }

        // Add self-closing /
        if (isSelfClosing)
        {
            inlines.Add(new Run(" /") { Foreground = CharsBrush });
        }

        // Add closing >
        inlines.Add(new Run(">") { Foreground = CharsBrush });
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
                    Foreground = AttributeBrush
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
                inlines.Add(new Run("=") { Foreground = CharsBrush });
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
                    inlines.Add(new Run(quote.ToString()) { Foreground = CharsBrush });
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
                            Foreground = hasColon ? StyleBrush : ValuesBrush
                        });
                    }

                    if (i < attributesPart.Length)
                    {
                        inlines.Add(new Run(quote.ToString()) { Foreground = CharsBrush });
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