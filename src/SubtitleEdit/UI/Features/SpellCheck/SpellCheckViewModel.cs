using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public partial class SpellCheckViewModel : ObservableObject
{

    [ObservableProperty] private string _lineText;
    [ObservableProperty] private string _wholeText;
    [ObservableProperty] private string _word;
    [ObservableProperty] private ObservableCollection<string> _dictionaries;
    [ObservableProperty] private string _selectedDictionary;
    [ObservableProperty] private ObservableCollection<string> _suggestions;
    [ObservableProperty] private string _selectedSuggestion;

    public SpellCheckWindow? Window { get; set; }

    public bool OkPressed { get; private set; }
    public StackPanel PanelWholeText { get; internal set; }

    public SpellCheckViewModel()
    {
        LineText = string.Empty;
        WholeText = string.Empty;
        Word = string.Empty;
        Dictionaries = new ObservableCollection<string>();
        SelectedDictionary = string.Empty;
        Suggestions = new ObservableCollection<string>();
        SelectedSuggestion = string.Empty;
        PanelWholeText = new StackPanel();

        LineText = "Line 1 of 20";
        WholeText = "This is a sample text with a missspelled word for testing purposes.";
        Word = "missspelled";
        Suggestions.Add("Suggestion 1");
        Suggestions.Add("Suggestion 2");
        Suggestions.Add("Suggestion 3");


        LoadDictionaries();
    }

    private void LoadDictionaries()
    {

    }

    public void Initialize()
    {
        Dispatcher.UIThread.Post(() =>
        {
            PanelWholeText.Children.Clear();

            // Add normal text
            PanelWholeText.Children.Add(new TextBlock
            {
                Text = "This is a ",
            });

            // Add highlighted word
            PanelWholeText.Children.Add(new TextBlock
            {
                Text = "highlighted",
                Foreground = Brushes.Red,
                FontWeight = FontWeight.Bold
            });

            // Add rest of the sentence
            PanelWholeText.Children.Add(new TextBlock
            {
                Text = "word.\n" ,
            });

            PanelWholeText.Children.Add(new TextBlock
            {
                Text = "Seconds line.\n",
            });


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
    private void GoogleIt()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void BrowseDictionary()
    {
        Window?.Close();
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
}