using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Ocr.FixEngine;

public interface IOcrFixEngine2
{
    void Initialize(List<OcrSubtitleItem> subtitles, string threeLetterIsoLanguageName, SpellCheckDictionaryDisplay spellCheckDictionary);
    OcrFixLineResult FixOcrErrors(int index, bool doTryToGuessUnknownWords);
    void Unload();
    bool IsLoaded();
    List<string> GetSpellCheckSuggestions(string word);
    void ChangeAll(string from, string to);
    void SkipAll(string word);
    void AddName(string name);
}

public partial class OcrFixEngine2 : IOcrFixEngine2, IDoSpell
{
    private bool _isLoaded;
    private OcrFixReplaceList2 _ocrFixReplaceList;
    private readonly HashSet<string> _wordSpellOkList = new HashSet<string>();
    private string[] _wordSplitList;
    private SpellCheckWordLists2 _spellCheckWordLists;
    //private string _spellCheckDictionaryName;
    private string _threeLetterIsoLanguageName;
    //private readonly HashSet<char> _expectedChars = new HashSet<char> { ' ', '¡', '¿', ',', '.', '!', '?', ':', ';', '(', ')', '[', ']', '{', '}', '+', '-', '£', '\\', '"', '”', '„', '“', '«', '»', '#', '&', '%', '\r', '\n', '؟' }; // removed $
    //private readonly HashSet<char> _expectedCharsNoComma = new HashSet<char> { ' ', '¡', '¿', '.', '!', '?', ':', ';', '(', ')', '[', ']', '{', '}', '+', '-', '£', '\\', '"', '”', '„', '“', '«', '»', '#', '&', '%', '\r', '\n', '؟' }; // removed $ + comma
    private Subtitle _subtitle;
    private List<OcrSubtitleItem> _subtitles;
    private HashSet<string> _wordSkipList = new HashSet<string>();
    private Dictionary<string, string> _changeAllDictionary;

    //private static readonly Regex RegexAloneIasL = new Regex(@"\bl\b", RegexOptions.Compiled);
    //private static readonly Regex RegexLowercaseL = new Regex("[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏ]l[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏ]", RegexOptions.Compiled);
    //private static readonly Regex RegexUppercaseI = new Regex("[a-zæøåöääöéèàùâêîôûëï]I.", RegexOptions.Compiled);
    //private static readonly Regex RegexNumber1 = new Regex(@"(?<=\d) 1(?!/\d)", RegexOptions.Compiled);


    private readonly ISpellCheckManager _spellCheckManager;

    public OcrFixEngine2(ISpellCheckManager spellCheckManager)
    {
        _spellCheckManager = spellCheckManager;
        _wordSkipList = new HashSet<string>();
        _changeAllDictionary = new Dictionary<string, string>();
        _subtitle = new Subtitle();
        _subtitles = new List<OcrSubtitleItem>();
        _threeLetterIsoLanguageName = string.Empty;
        _isLoaded = false;
        _ocrFixReplaceList = new OcrFixReplaceList2(string.Empty);
        _spellCheckWordLists = new SpellCheckWordLists2(string.Empty, this);
    }

    void IOcrFixEngine2.Initialize(List<OcrSubtitleItem> subtitles, string threeLetterIsoLanguageName, SpellCheckDictionaryDisplay spellCheckDictionary)
    {
        _isLoaded = true;
        var twoLetterIsoLanguageName = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(threeLetterIsoLanguageName);
        _spellCheckManager.Initialize(spellCheckDictionary.DictionaryFileName, twoLetterIsoLanguageName);
        _ocrFixReplaceList = OcrFixReplaceList2.FromLanguageId(threeLetterIsoLanguageName);

        var fiveLetterName = Path.GetFileNameWithoutExtension(spellCheckDictionary.DictionaryFileName);
        _spellCheckWordLists = new SpellCheckWordLists2(fiveLetterName, this);

        _subtitles = subtitles;
        _threeLetterIsoLanguageName = threeLetterIsoLanguageName;
        _subtitle = GetSubtitle(subtitles);

        var names = _spellCheckWordLists.GetAllNames();
        _wordSplitList = StringWithoutSpaceSplitToWords.LoadWordSplitList(Se.DictionariesFolder, threeLetterIsoLanguageName, names);
    }

    public OcrFixLineResult FixOcrErrors(int index, bool doTryToGuessUnknownWords)
    {
        var p = _subtitles[index];
        var wordsToIgnore = new List<string>();

        var replacedLine = ReplaceLineFixes(index, wordsToIgnore);
        var splitLine = SplitLine(replacedLine, p, index);
        if (replacedLine != p.Text)
        {
            splitLine.ReplacementUsed = new ReplacementUsedItem(p.Text, replacedLine, index);
        }

        for (var i = 0; i < splitLine.Words.Count; i++)
        {
            OcrFixLinePartResult? word = splitLine.Words[i];
            if (word.LinePartType != OcrFixLinePartType.Word)
            {
                word.FixedWord = word.Word;
                word.IsSpellCheckedOk = true;
                continue;
            }

            if (wordsToIgnore.Contains(word.Word))
            {
                word.IsSpellCheckedOk = true;
                continue;
            }

            CheckAndFixWord(word, splitLine, i, doTryToGuessUnknownWords);
        }

        return splitLine;
    }

    public static Subtitle GetSubtitle(List<OcrSubtitleItem> subtitles)
    {
        var subtitle = new Subtitle();
        foreach (var line in subtitles)
        {
            var p = new Paragraph(new TimeCode(line.StartTime), new TimeCode(line.EndTime), line.Text);
            subtitle.Paragraphs.Add(p);
        }

        return subtitle;
    }

    private string ReplaceLineFixes(int index, List<string> wordsToIgnore)
    {
        var line = _subtitles[index];
        var replacedLine = _ocrFixReplaceList.FixOcrErrorViaLineReplaceList(line.Text, _subtitle, index, _spellCheckManager, wordsToIgnore);
        return replacedLine;
    }

    private OcrFixLineResult SplitLine(string line, OcrSubtitleItem p, int index)
    {
        var result = new OcrFixLineResult
        {
            LineIndex = index,
        };
        if (string.IsNullOrEmpty(line))
        {
            return result;
        }
        int i = 0;
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
                        WordIndex = tagStart,
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
                        WordIndex = tagStart,
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
                    WordIndex = whitespaceStart,
                    Word = whitespace
                });
                continue;
            }
            // Check for words (letters and digits)
            if (char.IsLetterOrDigit(line[i]) && line[i] != '"')
            {
                var wordStart = i;
                while (i < line.Length &&
                       (char.IsLetterOrDigit(line[i]) || line[i] == '\'' || line[i] == '-'))
                {
                    i++;
                }
                var word = line.Substring(wordStart, i - wordStart);
                result.Words.Add(new OcrFixLinePartResult
                {
                    LinePartType = OcrFixLinePartType.Word,
                    WordIndex = wordStart,
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
                WordIndex = specialCharStart,
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

    private void CheckAndFixWord(OcrFixLinePartResult word, OcrFixLineResult splitLine, int index, bool doTryToGuessUnknownWords)
    {
        var s = word.Word;

        //TODO: check for multi-word names here too (look ahead in splitLine.Words, with e.g. "-")

        var isWordCorrect = false;
        var result = s;
        if (_changeAllDictionary.ContainsKey(s))
        {
            result = _changeAllDictionary[s];
            isWordCorrect = true;
        }
        else
        {
            result = _ocrFixReplaceList.FixCommonWordErrors(word.Word);
            if (result != word.Word)
            {
                word.ReplacementUsed = new ReplacementUsedItem(word.Word, result, splitLine.LineIndex);
            }

            isWordCorrect = _spellCheckManager.IsWordCorrect(result);

            if (!isWordCorrect && _wordSkipList.Contains(s))
            {
                isWordCorrect = true;
            }

            if (!isWordCorrect && _spellCheckWordLists.HasUserWord(result))
            {
                isWordCorrect = true;
            }

            if (!isWordCorrect && _spellCheckWordLists.HasName(result))
            {
                isWordCorrect = true;
            }

            var lineText = splitLine.GetText();
            var w = result.Trim('-');
            if (!isWordCorrect && w != result &&
                (_wordSkipList.Contains(w) ||
                 _spellCheckManager.IsWordCorrect(w) ||
                 _spellCheckWordLists.HasName(w) ||
                 _spellCheckWordLists.HasNameExtended(w, lineText)))
            {
                isWordCorrect = true;
            }

            w = result.Trim('\'');
            if (!isWordCorrect && w != result &&
                (_wordSkipList.Contains(w) ||
                 _spellCheckManager.IsWordCorrect(w) ||
                 _spellCheckWordLists.HasName(w) ||
                 _spellCheckWordLists.HasNameExtended(w, lineText)))
            {
                isWordCorrect = true;
            }

            w = result.Trim('\'', '"', '-');
            if (!isWordCorrect && w != result &&
                (_wordSkipList.Contains(w) ||
                 _spellCheckManager.IsWordCorrect(w) ||
                 _spellCheckWordLists.HasName(w) ||
                 _spellCheckWordLists.HasNameExtended(w, lineText)))
            {
                isWordCorrect = true;
            }

            if (!string.IsNullOrEmpty(result) && !isWordCorrect && doTryToGuessUnknownWords)
            {
                StringWithoutSpaceSplitToWords.SplitWord()
                var guesses = _ocrFixReplaceList.CreateGuessesFromLetters(result, _threeLetterIsoLanguageName);
                foreach (var g in guesses)
                {
                    w = g.Trim('\'', '"', '-');
                    if (IsSpelledCorrect(g) || IsSpelledCorrect(w) || _spellCheckWordLists.HasName(w))
                    {
                        result = g;
                        word.GuessUsed = true;
                        isWordCorrect = true;
                        break;
                    }
                }
            }
        }

        word.FixedWord = result;
        word.IsSpellCheckedOk = isWordCorrect;
    }

    private bool IsSpelledCorrect(string s)
    {
        if (_spellCheckManager.IsWordCorrect(s))
        {
            return true;
        }

        if (s.Contains(' '))
        {
            var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (!_spellCheckManager.IsWordCorrect(part))
                {
                    return false;
                }
            }

            return true;
        }

        if (s.Contains("-"))
        {
            var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (!_spellCheckManager.IsWordCorrect(part))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public void Unload()
    {
        _wordSkipList.Clear();
        _wordSpellOkList.Clear();
        _changeAllDictionary = new Dictionary<string, string>();
        _isLoaded = false;
        _subtitle = new Subtitle();
        _subtitles = new List<OcrSubtitleItem>();
        //_spellCheckDictionaryName = string.Empty;
        _threeLetterIsoLanguageName = string.Empty;
    }

    public bool IsLoaded()
    {
        return _isLoaded;
    }

    public bool DoSpell(string word)
    {
        if (_isLoaded)
        {
            return _spellCheckManager.IsWordCorrect(word);
        }

        return false;
    }

    public List<string> GetSpellCheckSuggestions(string word)
    {
        if (_isLoaded)
        {
            var suggestions = _spellCheckManager.GetSuggestions(word);

            if (suggestions.Count > 0 && HasMostlyUppercaseLetters(word))
            {
                var uc = suggestions[0].ToUpperInvariant();
                if (!suggestions.Contains(uc))
                {
                    suggestions.Insert(0, uc);
                }
            }

            return suggestions;
        }

        return [];
    }

    private static bool HasMostlyUppercaseLetters(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return false;
        }

        var letters = word.Where(char.IsLetter).ToArray();
        if (letters.Length == 0)
        {
            return false;
        }

        var uppercaseCount = letters.Count(char.IsUpper);

        return uppercaseCount > letters.Length / 2.0;
    }

    public void ChangeAll(string from, string to)
    {
        if (!_changeAllDictionary.ContainsKey(from))
        {
            _changeAllDictionary[from] = to;
        }
    }

    public void SkipAll(string word)
    {
        if (!_wordSkipList.Contains(word))
        {
            _wordSkipList.Add(word);
        }
    }

    public void AddName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || !_isLoaded)
        {
            return;
        }

        _spellCheckWordLists.AddName(name);
    }
}

