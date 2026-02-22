# Spell Check in TextEditor

## Overview

The `TextEditorWrapper` now supports inline spell checking with red wavy underlines for misspelled words, similar to Microsoft Word or Visual Studio Code.

## Features

- **Real-time spell checking**: As you type, misspelled words are underlined with a red wavy line
- **Integration with existing spell check**: Uses the same `ISpellCheckManager` and dictionaries as the spell check dialog
- **Smart word detection**: Automatically skips numbers, URLs, emails, hashtags, and acronyms
- **Performance optimized**: Only checks visible lines using AvaloniaEdit's `DocumentColorizingTransformer`

## Usage

### Enabling Spell Check

To enable spell check on a `TextEditorWrapper` instance:

```csharp
// Get or inject the ISpellCheckManager
var spellCheckManager = serviceProvider.GetService<ISpellCheckManager>();

// Initialize with a dictionary
spellCheckManager.Initialize(dictionaryPath, languageCode);

// Enable spell checking on the text editor
textEditorWrapper.EnableSpellCheck(spellCheckManager);
```

### Disabling Spell Check

```csharp
textEditorWrapper.DisableSpellCheck();
```

### Refreshing Spell Check

After adding words to the dictionary or changing dictionaries, refresh the underlines:

```csharp
textEditorWrapper.RefreshSpellCheck();
```

### Checking if Spell Check is Enabled

```csharp
bool isEnabled = textEditorWrapper.IsSpellCheckEnabled;
```

## Integration in MainViewModel

To enable spell check in the main subtitle editor:

```csharp
// In MainViewModel or initialization code
if (vm.EditTextBox is TextEditorWrapper textEditorWrapper)
{
    var spellCheckManager = serviceProvider.GetService<ISpellCheckManager>();
    
    // Initialize with user's selected dictionary
    var dictionaryPath = Se.Settings.SpellCheck.LastLanguageDictionaryFile;
    var languageCode = GetLanguageCode(dictionaryPath);
    
    if (spellCheckManager.Initialize(dictionaryPath, languageCode))
    {
        textEditorWrapper.EnableSpellCheck(spellCheckManager);
    }
}
```

## Implementation Details

### SpellCheckUnderlineTransformer

The spell checking is implemented using a custom `DocumentColorizingTransformer`:

- Inherits from `DocumentColorizingTransformer` (AvaloniaEdit)
- Splits each line into words using `SpellCheckWordLists2.Split()`
- Checks each word against `ISpellCheckManager.IsWordCorrect()`
- Applies `TextDecoration` with wavy underline to misspelled words
- Only processes visible lines for performance

### Word Filtering

The following patterns are automatically excluded from spell checking:

- Pure numbers (123, 1.5, -42)
- URLs (http://, https://, www.)
- Email addresses (contains @)
- Hashtags (starts with #)
- All-caps words (likely acronyms like "NASA")

## Performance

The spell checker uses AvaloniaEdit's line-based rendering system, which means:

- Only visible lines are checked
- Checking happens on-demand during rendering
- Scrolling and editing remain smooth
- Minimal memory overhead

## Customization

To customize the underline appearance, modify `SpellCheckUnderlineTransformer`:

```csharp
private static readonly TextDecoration WavyUnderline = new()
{
    Location = TextDecorationLocation.Underline,
    Stroke = new SolidColorBrush(Colors.Red),  // Change color here
    StrokeThickness = 1.5,                      // Change thickness here
    StrokeLineCap = PenLineCap.Round,
    StrokeThicknessUnit = TextDecorationUnit.FontRecommended
};
```

## Future Enhancements

Potential improvements:

- Right-click context menu with spelling suggestions
- Quick fixes (Ctrl+. or similar)
- Configuration to enable/disable spell check per text box
- Custom dictionary words highlighting in a different color
- Language-specific word patterns (e.g., German compound words)
