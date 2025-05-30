using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.SpellCheck;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public partial class SpellCheckViewModel : ObservableObject
{
    [ObservableProperty] private string _lineText;
    [ObservableProperty] private string _wholeText;
    [ObservableProperty] private string _currentWord;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private ObservableCollection<string> _suggestions;
    [ObservableProperty] private string _selectedSuggestion;
    [ObservableProperty] private bool _areSuggestionsAvailable;
    [ObservableProperty] private bool _isPrompting;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _paragraphs;
    [ObservableProperty] private SubtitleLineViewModel? _selectedParagraph;

    public SpellCheckWindow? Window { get; set; }

    public bool OkPressed { get; private set; }
    public StackPanel PanelWholeText { get; internal set; }

    private readonly ISpellCheckManager _spellCheckManager;
    private readonly IWindowService _windowService;

    private SpellCheckWord _currentSpellCheckWord;
    private SpellCheckResult? _lastSpellCheckResult;

    public SpellCheckViewModel(ISpellCheckManager spellCheckManager, IWindowService windowService)
    {
        _spellCheckManager = spellCheckManager;
        _spellCheckManager.OnWordChanged += (sender, e) =>
        {
            UpdateChangedWordInUi(e.FromWord, e.ToWord, e.WordIndex, e.Paragraph);
        };
        _windowService = windowService;

        LineText = string.Empty;
        WholeText = string.Empty;
        CurrentWord = string.Empty;
        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
        SelectedDictionary = new SpellCheckDictionaryDisplay();
        Suggestions = new ObservableCollection<string>();
        SelectedSuggestion = string.Empty;
        PanelWholeText = new StackPanel();
        StatusText = string.Empty;
        Paragraphs = new ObservableCollection<SubtitleLineViewModel>();
        _currentSpellCheckWord = new SpellCheckWord();

        LineText = "Line 1 of 20";
        WholeText = "This is a sample text with a missspelled word for testing purposes.";
        CurrentWord = "missspelled";
        Suggestions.Add("Suggestion 1");
        Suggestions.Add("Suggestion 2");
        Suggestions.Add("Suggestion 3");

        LoadDictionaries();
    }

    private void UpdateChangedWordInUi(string fromWord, string toWord, int wordIndex, SubtitleLineViewModel paragraph)
    {
        WholeText = paragraph.Text;
        SelectedParagraph = Paragraphs.FirstOrDefault(p => p.Id == paragraph.Id);
        if (SelectedParagraph != null)
        {
            SelectedParagraph.Text = paragraph.Text;
        }
    }

    private void LoadDictionaries()
    {
        var spellCheckLanguages = _spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);
        Dictionaries.Clear();
        Dictionaries.AddRange(spellCheckLanguages);
        if (Dictionaries.Count > 0)
        {
            if (!string.IsNullOrEmpty(Se.Settings.SpellCheck.LastLanguageDictionaryFile))
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(l => l.DictionaryFileName == Se.Settings.SpellCheck.LastLanguageDictionaryFile);
            }

            SelectedDictionary = Dictionaries.FirstOrDefault(l => l.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
            if (SelectedDictionary == null)
            {
                SelectedDictionary = Dictionaries[0];
            }

            _spellCheckManager.Initialize(SelectedDictionary.DictionaryFileName, GetTwoLetterLanguageCode(SelectedDictionary));

            //DoSpellCheck();
        }

        //Page?.Initialize(subtitle, videoFileName, this);

        //_loading = false;
    }

    public void Initialize()
    {
        Dispatcher.UIThread.Post(() =>
        {
            PanelWholeText.Children.Clear();

            var textBlock = new TextBlock();
            textBlock.Inlines.Add(new Run("Your name is "));

            // Add bound name (for demo, using hardcoded)
            textBlock.Inlines.Add(new Run
            {
                Text = "Alice",
                FontSize = 24,
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.Orange
            });

            textBlock.Inlines.Add(new Run(" and not Bob.\nHow are you?\n"));
            textBlock.Inlines.Add(new Run("Next line\n"));
            textBlock.Inlines.Add(new Run("Next line\n"));
            textBlock.Inlines.Add(new Run("Next line\n"));
            textBlock.Inlines.Add(new Run("Next line\n"));

            PanelWholeText.Children.Add(textBlock);


        }, DispatcherPriority.Background);
    }

    [RelayCommand]
    private void EditWholeText()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Change()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void ChangeAll()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void Skip()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SkipAll()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void AddToNamesList()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void AddToUserDictionary()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task GoogleIt()
    {
        if (string.IsNullOrEmpty(CurrentWord))
        {
            return;
        }

        await Window!.Launcher.LaunchUriAsync(new Uri("https://www.google.com/search?q=" + HttpUtility.UrlEncode(CurrentWord)));
    }

    [RelayCommand]
    private async Task BrowseDictionary()
    {
        var result = await _windowService.ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>(Window!, vm => { });
        if (result.OkPressed)
        {
            LoadDictionaries();

            //TODO: pick the selected dictionary and re-initialize the spell checker
        }
    }

    [RelayCommand]
    private void SuggestionUseOnce()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SuggestionUseAlways()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private void DoSpellCheck()
    {
        var results = _spellCheckManager.CheckSpelling(Paragraphs, _lastSpellCheckResult);
        if (results.Count > 0)
        {
            //WordNotFoundOriginal = results[0].Word.Text;
            CurrentWord = results[0].Word.Text;
            WholeText = results[0].Paragraph.Text;
            // CurrentFormattedText = HighLightCurrentWord(results[0].Word, results[0].Paragraph);
            _currentSpellCheckWord = results[0].Word;
            _lastSpellCheckResult = results[0];
            //_currentParagraph = results[0].Paragraph;

            var suggestions = _spellCheckManager.GetSuggestions(results[0].Word.Text);
            Suggestions = new ObservableCollection<string>(suggestions);
            AreSuggestionsAvailable = true;
            if (suggestions.Count > 0)
            {
                SelectedSuggestion = suggestions[0];
            }

            var lineIndex = Paragraphs.IndexOf(results[0].Paragraph) + 1;
            LineText = $"Spell checker - line {lineIndex} of {Paragraphs.Count}";

            //if (!string.IsNullOrEmpty(_videoFileName))
            //{
            //    VideoPlayer.SeekTo(results[0].Paragraph.StartTime.TimeSpan);
            //    VideoPlayer.Pause();
            //}

            SelectedParagraph = Paragraphs.FirstOrDefault(p => p.Id == results[0].Paragraph.Id);
            if (SelectedParagraph != null)
            {
                //MainThread.BeginInvokeOnMainThread(() =>
                //{
                //    SubtitleList.ScrollTo(SelectedParagraph, null, ScrollToPosition.Center);
                //});
            }
        }
        else
        {
            //_closing = true;
            //MainThread.BeginInvokeOnMainThread(async () =>
            //{
            //    await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            //    {
            //        { "Page", nameof(SpellCheckerPage) },
            //        { "Subtitle", _subtitle },
            //        { "TotalChangedWords", _spellCheckManager.NoOfChangedWords },
            //        { "TotalSkippedWords", _spellCheckManager.NoOfSkippedWords },
            //        { "AutoClose", true },
            //    });
            //});
        }
    }

    private static string GetTwoLetterLanguageCode(SpellCheckDictionaryDisplay? language)
    {
        if (language == null)
        {
            return "en";
        }

        var fileNameOnly = Path.GetFileNameWithoutExtension(language.DictionaryFileName);

        try
        {
            var ci = CultureInfo.GetCultureInfo(fileNameOnly.Replace('_', '-'));
            return ci.TwoLetterISOLanguageName;
        }
        catch
        {
            // ignore
        }


        if (fileNameOnly.Contains("English", StringComparison.OrdinalIgnoreCase))
        {
            return "en";
        }

        if (fileNameOnly.Contains("Spanish", StringComparison.OrdinalIgnoreCase))
        {
            return "es";
        }

        if (fileNameOnly.Contains("French", StringComparison.OrdinalIgnoreCase))
        {
            return "fr";
        }

        if (fileNameOnly.Contains("German", StringComparison.OrdinalIgnoreCase))
        {
            return "de";
        }

        if (fileNameOnly.Contains("Italian", StringComparison.OrdinalIgnoreCase))
        {
            return "it";
        }

        if (fileNameOnly.Contains("Dutch", StringComparison.OrdinalIgnoreCase))
        {
            return "nl";
        }

        if (fileNameOnly.Contains("Portuguese", StringComparison.OrdinalIgnoreCase))
        {
            return "pt";
        }

        if (fileNameOnly.Contains("Russian", StringComparison.OrdinalIgnoreCase))
        {
            return "ru";
        }

        if (fileNameOnly.Contains("Swedish", StringComparison.OrdinalIgnoreCase))
        {
            return "sv";
        }

        if (fileNameOnly.Contains("Danish", StringComparison.OrdinalIgnoreCase))
        {
            return "da";
        }

        if (fileNameOnly.Contains("Norwegian", StringComparison.OrdinalIgnoreCase))
        {
            return "no";
        }

        if (fileNameOnly.Contains("Finnish", StringComparison.OrdinalIgnoreCase))
        {
            return "fi";
        }

        if (fileNameOnly.Contains("Polish", StringComparison.OrdinalIgnoreCase))
        {
            return "pl";
        }

        if (fileNameOnly.Contains("Greek", StringComparison.OrdinalIgnoreCase))
        {
            return "el";
        }

        if (fileNameOnly.Contains("Turkish", StringComparison.OrdinalIgnoreCase))
        {
            return "tr";
        }

        if (fileNameOnly.Contains("Hungarian", StringComparison.OrdinalIgnoreCase))
        {
            return "hu";
        }

        if (fileNameOnly.Contains("Czech", StringComparison.OrdinalIgnoreCase))
        {
            return "cs";
        }

        if (fileNameOnly.Contains("Slovak", StringComparison.OrdinalIgnoreCase))
        {
            return "sk";
        }

        if (fileNameOnly.Contains("Romanian", StringComparison.OrdinalIgnoreCase))
        {
            return "ro";
        }

        if (fileNameOnly.Contains("Bulgarian", StringComparison.OrdinalIgnoreCase))
        {
            return "bg";
        }

        if (fileNameOnly.Contains("Croatian", StringComparison.OrdinalIgnoreCase))
        {
            return "hr";
        }

        if (language.Name.Contains("Serbian", StringComparison.OrdinalIgnoreCase))
        {
            return "sr";
        }

        if (fileNameOnly.Contains("Ukrainian", StringComparison.OrdinalIgnoreCase))
        {
            return "uk";
        }

        if (fileNameOnly.Contains("Hebrew", StringComparison.OrdinalIgnoreCase))
        {
            return "he";
        }

        if (fileNameOnly.Contains("Arabic", StringComparison.OrdinalIgnoreCase))
        {
            return "ar";
        }

        if (fileNameOnly.Contains("Hindi", StringComparison.OrdinalIgnoreCase))
        {
            return "hi";
        }

        if (fileNameOnly.Contains("Japanese", StringComparison.OrdinalIgnoreCase))
        {
            return "ja";
        }

        if (fileNameOnly.Contains("Chinese", StringComparison.OrdinalIgnoreCase))
        {
            return "zh";
        }

        if (fileNameOnly.Contains("Korean", StringComparison.OrdinalIgnoreCase))
        {
            return "ko";
        }

        return "en";
    }
}