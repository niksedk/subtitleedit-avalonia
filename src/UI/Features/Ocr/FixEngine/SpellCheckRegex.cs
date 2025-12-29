using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Features.Ocr.FixEngine;
// XML:
//    <RegularExpressionsIfSpelledCorrectly>
//        <RegEx find = "\b([A-Z][a-z]+)'s\b" spellCheck="$1" replaceWith="$1's" /> <!-- David's -->
//        <RegEx find = "\b(a-z]+)'s\b" spellCheck="$1" replaceWith="$1's" /> <!-- there's -->
//        <RegEx find = "\bl([A-Z]+)\b" spellCheck="I$1" replaceWith="I$1" />
//        <RegEx find = "\b([A-Z]+)l\b" spellCheck="$1I" replaceWith="$1I" />
//        <RegEx find = "\b([A-Z]+)l([A-Z]+)\b" spellCheck="$1I$2" replaceWith="$1I$2" />
//    </RegularExpressionsIfSpelledCorrectly>

public class SpellCheckRegex
{
    public string Find { get; set; }
    public Regex FindRegEx { get; set; }
    public string SpellCheckWord { get; set; }
    public string ReplaceWith { get; set; }

    /// <summary>
    /// Applies the regex rule to the input text.
    /// </summary>
    /// <param name="input">The line of text to fix.</param>
    /// <param name="isWordSpelledCorrectly">A function that returns true if the word is in the dictionary.</param>
    public string Apply(string input, List<string> wordsToIgnore, Func<string, bool> isWordSpelledCorrectly)
    {
        return FindRegEx.Replace(input, match =>
        {
            // Convert "$1" or "I$1" into the actual word to check (e.g., "ITEM")
            string wordToCheck = match.Result(SpellCheckWord);

            // If the fixed version is a real word, apply the replacement
            if (isWordSpelledCorrectly(wordToCheck))
            {
                wordsToIgnore.Add(match.Value);
                return match.Result(ReplaceWith);
            }

            // Otherwise, leave the original text alone
            return match.Value;
        });
    }

    public static List<SpellCheckRegex> LoadRegExList(XmlDocument doc, string name)
    {
        var list = new List<SpellCheckRegex>();

        var node = doc.DocumentElement?.SelectSingleNode(name);
        if (node == null)
        {
            return list;
        }

        foreach (XmlNode item in node.ChildNodes)
        {
            var find = item.Attributes?["find"]?.Value;
            var spellCheck = item.Attributes?["spellCheck"]?.Value;
            var replaceWith = item.Attributes?["replaceWith"]?.Value;
            if (string.IsNullOrEmpty(find) || string.IsNullOrEmpty(spellCheck) || string.IsNullOrEmpty(replaceWith))
            {
                continue;
            }

            try
            {
                list.Add(new SpellCheckRegex
                {
                    Find = find,
                    FindRegEx = new Regex(find, RegexOptions.Compiled),
                    SpellCheckWord = spellCheck,
                    ReplaceWith = replaceWith,
                });
            }
            catch
            {
                // ignore
            }
        }

        return list;
    }
}


