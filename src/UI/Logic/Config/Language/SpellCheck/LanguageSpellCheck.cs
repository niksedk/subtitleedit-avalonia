namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageSpellCheck
{
    public string SpellCheck { get; set; }
    public string GetDictionariesTitle { get; set; }
    public string GetDictionaryInstructions { get; set; }
    public string AddNameToUserDictionary { get; set; }
    public string AddNameToNamesList { get; set; }
    public string NoDictionariesFound { get; set; }
    public string WordNotFound { get; set; }

    public LanguageSpellCheck()
    {
        SpellCheck = "Spell check";
        GetDictionariesTitle = "Spell check - get dictionaries";
        GetDictionaryInstructions = "Choose your language and click download";
        AddNameToUserDictionary = "Add name to user dictionary";
        AddNameToNamesList = "Add name to names list";
        NoDictionariesFound = "No dictionaries found";
        WordNotFound = "Word not found";
    }
}