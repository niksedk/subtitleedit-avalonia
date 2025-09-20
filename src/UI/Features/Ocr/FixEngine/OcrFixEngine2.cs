using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.SpellCheck;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Ocr.FixEngine;

public interface IOcrFixEngine2
{
    void Initialize(string threeLetterIsoLanguageName, SpellCheckDictionaryDisplay spellCheckDictionary);
    OcrFixLineResult FixOcrErrors(List<SubtitleLineViewModel> subtitles, int index);
}

public partial class OcrFixEngine2 : IOcrFixEngine2
{
    private string _fiveLetterWordListLanguageName;
    private OcrFixReplaceList _ocrFixReplaceList;
    private NameList _nameListObj;
    private HashSet<string> _nameList = new HashSet<string>();
    private HashSet<string> _nameListUppercase = new HashSet<string>();
    private HashSet<string> _nameListWithApostrophe = new HashSet<string>();
    private HashSet<string> _nameMultiWordList = new HashSet<string>(); // case sensitive phrases
    private List<string> _nameMultiWordListAndWordsWithPeriods;
    private HashSet<string> _abbreviationList;
    private HashSet<string> _wordSkipList = new HashSet<string>();
    private readonly HashSet<string> _wordSpellOkList = new HashSet<string>();
    private string[] _wordSplitList;
    private Dictionary<string, string> _changeAllDictionary;
    private SpellCheckWordLists _spellCheckWordLists;
    private string _spellCheckDictionaryName;
    private readonly string _threeLetterIsoLanguageName;

    private static readonly Regex RegexAloneIasL = new Regex(@"\bl\b", RegexOptions.Compiled);
    private static readonly Regex RegexLowercaseL = new Regex("[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏ]l[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏ]", RegexOptions.Compiled);
    private static readonly Regex RegexUppercaseI = new Regex("[a-zæøåöääöéèàùâêîôûëï]I.", RegexOptions.Compiled);
    private static readonly Regex RegexNumber1 = new Regex(@"(?<=\d) 1(?!/\d)", RegexOptions.Compiled);

    private readonly HashSet<char> _expectedChars = new HashSet<char> { ' ', '¡', '¿', ',', '.', '!', '?', ':', ';', '(', ')', '[', ']', '{', '}', '+', '-', '£', '\\', '"', '”', '„', '“', '«', '»', '#', '&', '%', '\r', '\n', '؟' }; // removed $
    private readonly HashSet<char> _expectedCharsNoComma = new HashSet<char> { ' ', '¡', '¿', '.', '!', '?', ':', ';', '(', ')', '[', ']', '{', '}', '+', '-', '£', '\\', '"', '”', '„', '“', '«', '»', '#', '&', '%', '\r', '\n', '؟' }; // removed $ + comma

    private readonly ISpellCheckManager _spellCheckManager;

    public OcrFixEngine2(ISpellCheckManager spellCheckManager)
    {
        _spellCheckManager = spellCheckManager;
    }

    void IOcrFixEngine2.Initialize(string threeLetterIsoLanguageName, SpellCheckDictionaryDisplay spellCheckDictionary)
    {
        var twoLetterIsoLanguageName = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(threeLetterIsoLanguageName);
        _spellCheckManager.Initialize(spellCheckDictionary.DictionaryFileName, threeLetterIsoLanguageName);
        _ocrFixReplaceList = OcrFixReplaceList.FromLanguageId(threeLetterIsoLanguageName);
    }

    public OcrFixLineResult FixOcrErrors(List<SubtitleLineViewModel> subtitles, int index)
    {
        var p = subtitles[index];

        var replacedLine = ReplaceLineFixes(p, subtitles, index);

        var splitLine = SplitLine(replacedLine, p, index);
        for (int i = 0; i < splitLine.Words.Count; i++)
        {
            OcrFixLinePartResult? word = splitLine.Words[i];
            if (word.LinePartType != OcrFixLinePartType.Word)
            {
                continue;
            }

            CheckAndFixWord(word, splitLine, i);
        }

        return splitLine;
    }

    public static Subtitle GetSubtitle(List<SubtitleLineViewModel> subtitles)
    {
        var subtitle = new Subtitle();
        foreach (var line in subtitles)
        {
            subtitle.Paragraphs.Add(line.ToParagraph());
        }

        return subtitle;
    }

    private string ReplaceLineFixes(SubtitleLineViewModel line, List<SubtitleLineViewModel> subtitles, int index)
    {
        var subtitle = GetSubtitle(subtitles);
        var replacedLine = _ocrFixReplaceList.FixOcrErrorViaLineReplaceList(line.Text, subtitle, index);
        return replacedLine;
    }

    private OcrFixLineResult SplitLine(string line, SubtitleLineViewModel p, int index)
    {
        var result = new OcrFixLineResult
        {
            LineIndex = index,
            Paragraph = p,
        };

        if (string.IsNullOrEmpty(line))
        {
            return result;
        }

        int i = 0;
        int wordIndex = 0;

        while (i < line.Length)
        {
            // Check for HTML tags starting with "<" and ending with "/>" or ">"
            if (line[i] == '<')
            {
                var tagStart = i;
                var tagEnd = FindTagEnd(line, i);

                if (tagEnd > tagStart)
                {
                    var tag = line.Substring(tagStart, tagEnd - tagStart + 1);
                    result.Words.Add(new OcrFixLinePartResult
                    {
                        LinePartType = OcrFixLinePartType.Tag,
                        WordIndex = wordIndex++,
                        Word = tag
                    });
                    i = tagEnd + 1;
                    continue;
                }
            }

            // Check for tags starting with "{\" and ending with "}"
            if (i < line.Length - 1 && line[i] == '{' && line[i + 1] == '\\')
            {
                var tagStart = i;
                var tagEnd = line.IndexOf('}', i);

                if (tagEnd > tagStart)
                {
                    var tag = line.Substring(tagStart, tagEnd - tagStart + 1);
                    result.Words.Add(new OcrFixLinePartResult
                    {
                        LinePartType = OcrFixLinePartType.Tag,
                        WordIndex = wordIndex++,
                        Word = tag
                    });
                    i = tagEnd + 1;
                    continue;
                }
            }

            // Check for whitespace
            if (char.IsWhiteSpace(line[i]))
            {
                var whitespaceStart = i;
                while (i < line.Length && char.IsWhiteSpace(line[i]))
                {
                    i++;
                }

                var whitespace = line.Substring(whitespaceStart, i - whitespaceStart);
                result.Words.Add(new OcrFixLinePartResult
                {
                    LinePartType = OcrFixLinePartType.Whitespace,
                    WordIndex = wordIndex++,
                    Word = whitespace
                });
                continue;
            }

            // Check for words (letters and digits)
            if (char.IsLetterOrDigit(line[i]))
            {
                var wordStart = i;
                while (i < line.Length && (char.IsLetterOrDigit(line[i]) || line[i] == '\'' || line[i] == '-'))
                {
                    i++;
                }

                var word = line.Substring(wordStart, i - wordStart);
                result.Words.Add(new OcrFixLinePartResult
                {
                    LinePartType = OcrFixLinePartType.Word,
                    WordIndex = wordIndex++,
                    Word = word
                });
                continue;
            }

            // Everything else is special characters
            var specialCharStart = i;
            while (i < line.Length &&
                   !char.IsLetterOrDigit(line[i]) &&
                   !char.IsWhiteSpace(line[i]) &&
                   line[i] != '<' &&
                   !(i < line.Length - 1 && line[i] == '{' && line[i + 1] == '\\'))
            {
                i++;
            }

            var specialChars = line.Substring(specialCharStart, i - specialCharStart);
            result.Words.Add(new OcrFixLinePartResult
            {
                LinePartType = OcrFixLinePartType.SpecialCharacters,
                WordIndex = wordIndex++,
                Word = specialChars
            });
        }

        return result;
    }

    private static int FindTagEnd(string text, int startIndex)
    {
        int i = startIndex + 1; // Start after '<'

        while (i < text.Length)
        {
            if (text[i] == '>')
            {
                // Check if it's a self-closing tag ending with "/>"
                if (i > startIndex + 1 && text[i - 1] == '/')
                {
                    return i;
                }
                // Regular closing tag ">"
                return i;
            }
            i++;
        }

        return startIndex; // No valid tag end found
    }

    private void CheckAndFixWord(OcrFixLinePartResult word, OcrFixLineResult splitLine, int index)
    {
        var s = word.Word;

        //TODO: check for multi-word names here too (look ahead in splitLine.Words, with e.g. "-")

        var result = s;
        if (_changeAllDictionary != null && _changeAllDictionary.ContainsKey(s))
        {
            result = _changeAllDictionary[s];
        }
        else
        {
            result = _ocrFixReplaceList.FixCommonWordErrors(word.Word);
        }

        word.FixedWord = result;  
    }
}

